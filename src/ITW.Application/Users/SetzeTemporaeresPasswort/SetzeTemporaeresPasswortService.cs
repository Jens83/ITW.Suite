// Datei: src/ITW.Application/Users/SetzeTemporaeresPasswort/SetzeTemporaeresPasswortService.cs
using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.SetzeTemporaeresPasswort;

public sealed class SetzeTemporaeresPasswortService
{
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SetzeTemporaeresPasswortService> _logger;

    public SetzeTemporaeresPasswortService(
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<SetzeTemporaeresPasswortService> logger)
    {
        ArgumentNullException.ThrowIfNull(passwortResetAnfrageRepository);
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository;
        ArgumentNullException.ThrowIfNull(benutzerkontoRepository);
        _benutzerkontoRepository = benutzerkontoRepository;
        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<SetzeTemporaeresPasswortResult> ExecuteAsync(
        SetzeTemporaeresPasswortCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(SetzeTemporaeresPasswortService));

        if (command.AnfrageId == Guid.Empty)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: AnfrageId leer", nameof(SetzeTemporaeresPasswortService));
            return SetzeTemporaeresPasswortResult.Fehler("Die Passwort-Reset-Anfrage ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BearbeitetVonUserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: BearbeitetVonUserId leer", nameof(SetzeTemporaeresPasswortService));
            return SetzeTemporaeresPasswortResult.Fehler("Die Bearbeiter-UserId ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.TemporaeresPasswort))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: TemporaeresPasswort leer", nameof(SetzeTemporaeresPasswortService));
            return SetzeTemporaeresPasswortResult.Fehler("Das temporäre Passwort ist erforderlich.");
        }

        var anfrage = await _passwortResetAnfrageRepository.GetOffeneAnfrageByIdAsync(
            command.AnfrageId,
            cancellationToken);

        if (anfrage is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SetzeTemporaeresPasswortService), "Anfrage nicht gefunden oder bereits bearbeitet");
            return SetzeTemporaeresPasswortResult.Fehler(
                "Die offene Passwort-Reset-Anfrage wurde nicht gefunden oder wurde bereits bearbeitet.");
        }

        var setPasswortResult = await _benutzerkontoRepository.SetzeTemporaeresPasswortAsync(
            anfrage.UserId,
            command.TemporaeresPasswort,
            cancellationToken);

        if (!setPasswortResult.IsSuccess)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SetzeTemporaeresPasswortService), setPasswortResult.ErrorMessage);
            return SetzeTemporaeresPasswortResult.Fehler(
                setPasswortResult.ErrorMessage ?? "Das temporäre Passwort konnte nicht gesetzt werden.");
        }

        anfrage.AlsErledigtMarkieren(
            command.BearbeitetVonUserId.Trim(),
            _dateTimeProvider.UtcNow);

        await _passwortResetAnfrageRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SetzeTemporaeresPasswortService));
        return SetzeTemporaeresPasswortResult.Erfolg();
    }
}