using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Fahrtenbuch;

public sealed record AbschliessenFahrtenbuchEintragCommand(
    Guid FahrzeugId,
    Guid EintragId,
    DateTimeOffset EndzeitUtc,
    string? Zielort,
    int EndKilometerstand,
    string AktualisiertVonUserId);

public sealed class AbschliessenFahrtenbuchEintragResult
{
    private AbschliessenFahrtenbuchEintragResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static AbschliessenFahrtenbuchEintragResult Erfolg()
        => new(true, null);

    public static AbschliessenFahrtenbuchEintragResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class AbschliessenFahrtenbuchEintragService
{
    private readonly IFahrtenbuchRepository _repository;

    public AbschliessenFahrtenbuchEintragService(IFahrtenbuchRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<AbschliessenFahrtenbuchEintragResult> ExecuteAsync(
        AbschliessenFahrtenbuchEintragCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.FahrzeugId == Guid.Empty)
        {
            return AbschliessenFahrtenbuchEintragResult.Fehler("Die Fahrzeug-ID ist erforderlich.");
        }

        if (command.EintragId == Guid.Empty)
        {
            return AbschliessenFahrtenbuchEintragResult.Fehler("Der Fahrtenbucheintrag konnte nicht ermittelt werden.");
        }

        if (command.EndKilometerstand < 0)
        {
            return AbschliessenFahrtenbuchEintragResult.Fehler("Der Endkilometerstand darf nicht negativ sein.");
        }

        if (string.IsNullOrWhiteSpace(command.AktualisiertVonUserId))
        {
            return AbschliessenFahrtenbuchEintragResult.Fehler("Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }

        var eintrag = await _repository.GetByIdAsync(
            command.EintragId,
            cancellationToken);

        if (eintrag is null || eintrag.FahrzeugId != command.FahrzeugId)
        {
            return AbschliessenFahrtenbuchEintragResult.Fehler("Der Fahrtenbucheintrag wurde nicht gefunden.");
        }

        if (eintrag.Status != FahrtenbuchStatus.Offen)
        {
            return AbschliessenFahrtenbuchEintragResult.Fehler("Nur offene Fahrten können abgeschlossen werden.");
        }

        try
        {
            eintrag.Abschliessen(
                command.EndzeitUtc,
                command.Zielort,
                command.EndKilometerstand,
                command.AktualisiertVonUserId,
                DateTimeOffset.UtcNow);

            await _repository.SaveChangesAsync(cancellationToken);

            return AbschliessenFahrtenbuchEintragResult.Erfolg();
        }
        catch (ArgumentException ex)
        {
            return AbschliessenFahrtenbuchEintragResult.Fehler(ex.Message);
        }
    }
}