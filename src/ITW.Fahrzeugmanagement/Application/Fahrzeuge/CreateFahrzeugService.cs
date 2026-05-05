using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Fahrzeuge;

public sealed record CreateFahrzeugCommand(
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
    string ErstelltVonUserId);

public sealed class CreateFahrzeugResult
{
    private CreateFahrzeugResult(
        bool isSuccess,
        Guid? fahrzeugId,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        FahrzeugId = fahrzeugId;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public Guid? FahrzeugId { get; }

    public string? ErrorMessage { get; }

    public static CreateFahrzeugResult Erfolg(Guid fahrzeugId)
        => new(true, fahrzeugId, null);

    public static CreateFahrzeugResult Fehler(string errorMessage)
        => new(false, null, errorMessage);
}

public sealed class CreateFahrzeugService
{
    private readonly IFahrzeugRepository _repository;

    public CreateFahrzeugService(IFahrzeugRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<CreateFahrzeugResult> ExecuteAsync(
        CreateFahrzeugCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.InterneNummer))
        {
            return CreateFahrzeugResult.Fehler("Bitte eine interne Nummer oder einen Funkrufnamen eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Kennzeichen))
        {
            return CreateFahrzeugResult.Fehler("Bitte ein Kennzeichen eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Vin))
        {
            return CreateFahrzeugResult.Fehler("Bitte die Fahrzeug-Identifikationsnummer eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Hersteller))
        {
            return CreateFahrzeugResult.Fehler("Bitte den Hersteller eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Modell))
        {
            return CreateFahrzeugResult.Fehler("Bitte das Modell eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Fahrzeugtyp))
        {
            return CreateFahrzeugResult.Fehler("Bitte den Fahrzeugtyp eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.ErstelltVonUserId))
        {
            return CreateFahrzeugResult.Fehler("Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }

        var kennzeichen = command.Kennzeichen.Trim().ToUpperInvariant();
        var vin = command.Vin.Trim().ToUpperInvariant();

        if (await _repository.ExistsByKennzeichenAsync(kennzeichen, cancellationToken))
        {
            return CreateFahrzeugResult.Fehler("Es gibt bereits ein Fahrzeug mit diesem Kennzeichen.");
        }

        if (await _repository.ExistsByVinAsync(vin, cancellationToken))
        {
            return CreateFahrzeugResult.Fehler("Es gibt bereits ein Fahrzeug mit dieser Fahrzeug-Identifikationsnummer.");
        }

        try
        {
            var jetzt = DateTimeOffset.UtcNow;

            var fahrzeug = new Fahrzeug(
                Guid.NewGuid(),
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
                command.KilometerstandAktuell,
                command.Status,
                command.StandardStandort,
                command.ErstelltVonUserId,
                jetzt);

            await _repository.AddFahrzeugAsync(fahrzeug, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return CreateFahrzeugResult.Erfolg(fahrzeug.Id);
        }
        catch (ArgumentException ex)
        {
            return CreateFahrzeugResult.Fehler(ex.Message);
        }        
    }
}