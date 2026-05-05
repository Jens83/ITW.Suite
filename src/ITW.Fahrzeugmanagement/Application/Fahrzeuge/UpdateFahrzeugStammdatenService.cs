using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Fahrzeuge;

public sealed record UpdateFahrzeugStammdatenCommand(
    Guid FahrzeugId,
    string InterneNummer,
    string Kennzeichen,
    string Vin,
    string Hersteller,
    string Modell,
    string Fahrzeugtyp,
    int? Baujahr,
    DateOnly? Erstzulassung,
    Kraftstoffart Kraftstoffart,
    int? LeistungKw,
    int KilometerstandAktuell,
    FahrzeugStatus Status,
    string? StandardStandort,
    string AktualisiertVonUserId);

public sealed class UpdateFahrzeugStammdatenResult
{
    private UpdateFahrzeugStammdatenResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static UpdateFahrzeugStammdatenResult Erfolg()
        => new(true, null);

    public static UpdateFahrzeugStammdatenResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class UpdateFahrzeugStammdatenService
{
    private readonly IFahrzeugRepository _repository;

    public UpdateFahrzeugStammdatenService(IFahrzeugRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<UpdateFahrzeugStammdatenResult> ExecuteAsync(
        UpdateFahrzeugStammdatenCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.FahrzeugId == Guid.Empty)
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Das Fahrzeug konnte nicht ermittelt werden.");
        }

        if (string.IsNullOrWhiteSpace(command.InterneNummer))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Bitte eine interne Nummer oder einen Funkrufnamen eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Kennzeichen))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Bitte ein Kennzeichen eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Vin))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Bitte die Fahrzeug-Identifikationsnummer eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Hersteller))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Bitte den Hersteller eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Modell))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Bitte das Modell eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Fahrzeugtyp))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Bitte den Fahrzeugtyp eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.AktualisiertVonUserId))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }

        var fahrzeug = await _repository.GetFahrzeugByIdAsync(
            command.FahrzeugId,
            cancellationToken);

        if (fahrzeug is null)
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Das Fahrzeug wurde nicht gefunden.");
        }

        var kennzeichen = command.Kennzeichen.Trim().ToUpperInvariant();
        var vin = command.Vin.Trim().ToUpperInvariant();

        if (await _repository.ExistsByKennzeichenAsync(
                kennzeichen,
                command.FahrzeugId,
                cancellationToken))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Es gibt bereits ein anderes Fahrzeug mit diesem Kennzeichen.");
        }

        if (await _repository.ExistsByVinAsync(
                vin,
                command.FahrzeugId,
                cancellationToken))
        {
            return UpdateFahrzeugStammdatenResult.Fehler("Es gibt bereits ein anderes Fahrzeug mit dieser Fahrzeug-Identifikationsnummer.");
        }

        try
        {
            var jetzt = DateTimeOffset.UtcNow;

            fahrzeug.AktualisiereStammdaten(
                command.InterneNummer,
                kennzeichen,
                vin,
                command.Hersteller,
                command.Modell,
                command.Fahrzeugtyp,
                command.Baujahr,
                command.Erstzulassung,
                command.Kraftstoffart,
                command.LeistungKw,
                command.Status,
                command.StandardStandort,
                command.AktualisiertVonUserId,
                jetzt);

            if (command.KilometerstandAktuell != fahrzeug.KilometerstandAktuell)
            {
                fahrzeug.AktualisiereKilometerstand(
                    command.KilometerstandAktuell,
                    command.AktualisiertVonUserId,
                    jetzt);
            }

            await _repository.SaveChangesAsync(cancellationToken);

            return UpdateFahrzeugStammdatenResult.Erfolg();
        }
        catch (ArgumentException ex)
        {
            return UpdateFahrzeugStammdatenResult.Fehler(ex.Message);
        }
    }
}