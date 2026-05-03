// Datei: src/ITW.Application/Users/SetzeTemporaeresPasswort/SetzeTemporaeresPasswortService.cs
using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;

namespace ITW.Application.Users.SetzeTemporaeresPasswort;

public sealed class SetzeTemporaeresPasswortService
{
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public SetzeTemporaeresPasswortService(
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository
            ?? throw new ArgumentNullException(nameof(passwortResetAnfrageRepository));
        _benutzerkontoRepository = benutzerkontoRepository
            ?? throw new ArgumentNullException(nameof(benutzerkontoRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<SetzeTemporaeresPasswortResult> ExecuteAsync(
        SetzeTemporaeresPasswortCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.AnfrageId == Guid.Empty)
        {
            return SetzeTemporaeresPasswortResult.Fehler("Die Passwort-Reset-Anfrage ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BearbeitetVonUserId))
        {
            return SetzeTemporaeresPasswortResult.Fehler("Die Bearbeiter-UserId ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.TemporaeresPasswort))
        {
            return SetzeTemporaeresPasswortResult.Fehler("Das temporäre Passwort ist erforderlich.");
        }

        var anfrage = await _passwortResetAnfrageRepository.GetOffeneAnfrageByIdAsync(
            command.AnfrageId,
            cancellationToken);

        if (anfrage is null)
        {
            return SetzeTemporaeresPasswortResult.Fehler(
                "Die offene Passwort-Reset-Anfrage wurde nicht gefunden oder wurde bereits bearbeitet.");
        }

        var setPasswortResult = await _benutzerkontoRepository.SetzeTemporaeresPasswortAsync(
            anfrage.UserId,
            command.TemporaeresPasswort,
            cancellationToken);

        if (!setPasswortResult.IsSuccess)
        {
            return SetzeTemporaeresPasswortResult.Fehler(
                setPasswortResult.ErrorMessage ?? "Das temporäre Passwort konnte nicht gesetzt werden.");
        }

        anfrage.AlsErledigtMarkieren(
            command.BearbeitetVonUserId.Trim(),
            _dateTimeProvider.UtcNow);

        await _passwortResetAnfrageRepository.SaveChangesAsync(cancellationToken);

        return SetzeTemporaeresPasswortResult.Erfolg();
    }
}