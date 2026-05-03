using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Fahrtenbuch;

public sealed record CreateFahrtenbuchEintragCommand(
    Guid FahrzeugId,
    string FahrerUserId,
    string FahrerName,
    string? BeifahrerName,
    FahrtKategorie FahrtKategorie,
    string Fahrtzweck,
    DateTimeOffset StartzeitUtc,
    DateTimeOffset EndzeitUtc,
    string? Startort,
    string? Zielort,
    int StartKilometerstand,
    int EndKilometerstand,
    string? Bemerkung,
    string ErstelltVonUserId);

public sealed class CreateFahrtenbuchEintragResult
{
    private CreateFahrtenbuchEintragResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static CreateFahrtenbuchEintragResult Erfolg()
        => new(true, null);

    public static CreateFahrtenbuchEintragResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class CreateFahrtenbuchEintragService
{
    private readonly IFahrzeugRepository _fahrzeugRepository;
    private readonly IFahrtenbuchRepository _fahrtenbuchRepository;

    public CreateFahrtenbuchEintragService(
        IFahrzeugRepository fahrzeugRepository,
        IFahrtenbuchRepository fahrtenbuchRepository)
    {
        _fahrzeugRepository = fahrzeugRepository
            ?? throw new ArgumentNullException(nameof(fahrzeugRepository));

        _fahrtenbuchRepository = fahrtenbuchRepository
            ?? throw new ArgumentNullException(nameof(fahrtenbuchRepository));
    }

    public async Task<CreateFahrtenbuchEintragResult> ExecuteAsync(
        CreateFahrtenbuchEintragCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.FahrzeugId == Guid.Empty)
        {
            return CreateFahrtenbuchEintragResult.Fehler("Das Fahrzeug konnte nicht ermittelt werden.");
        }

        if (string.IsNullOrWhiteSpace(command.FahrerUserId))
        {
            return CreateFahrtenbuchEintragResult.Fehler("Der Fahrer konnte nicht ermittelt werden.");
        }

        if (string.IsNullOrWhiteSpace(command.FahrerName))
        {
            return CreateFahrtenbuchEintragResult.Fehler("Bitte einen Fahrer eingeben.");
        }

        if (command.FahrtKategorie == FahrtKategorie.Unbekannt)
        {
            return CreateFahrtenbuchEintragResult.Fehler("Bitte eine Fahrtkategorie auswählen.");
        }

        if (string.IsNullOrWhiteSpace(command.Fahrtzweck))
        {
            return CreateFahrtenbuchEintragResult.Fehler("Bitte einen Fahrtzweck eingeben.");
        }

        if (command.EndzeitUtc < command.StartzeitUtc)
        {
            return CreateFahrtenbuchEintragResult.Fehler("Die Endzeit darf nicht vor der Startzeit liegen.");
        }

        if (command.StartKilometerstand < 0)
        {
            return CreateFahrtenbuchEintragResult.Fehler("Der Startkilometerstand darf nicht negativ sein.");
        }

        if (command.EndKilometerstand < 0)
        {
            return CreateFahrtenbuchEintragResult.Fehler("Der Endkilometerstand darf nicht negativ sein.");
        }

        if (command.EndKilometerstand < command.StartKilometerstand)
        {
            return CreateFahrtenbuchEintragResult.Fehler("Der Endkilometerstand darf nicht kleiner als der Startkilometerstand sein.");
        }

        if (string.IsNullOrWhiteSpace(command.ErstelltVonUserId))
        {
            return CreateFahrtenbuchEintragResult.Fehler("Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }

        var fahrzeug = await _fahrzeugRepository.GetFahrzeugByIdAsync(
            command.FahrzeugId,
            cancellationToken);

        if (fahrzeug is null)
        {
            return CreateFahrtenbuchEintragResult.Fehler("Das Fahrzeug wurde nicht gefunden.");
        }

        try
        {
            var eintrag = new FahrtenbuchEintrag(
                Guid.NewGuid(),
                command.FahrzeugId,
                command.FahrerUserId,
                command.FahrerName,
                command.BeifahrerName,
                routeSessionId: null,
                einsatzId: null,
                command.FahrtKategorie,
                command.Fahrtzweck,
                command.StartzeitUtc,
                command.EndzeitUtc,
                command.Startort,
                command.Zielort,
                command.StartKilometerstand,
                command.EndKilometerstand,
                tankmengeLiter: null,
                kilometerstandBeimTanken: null,
                istAutomatischVorbelegt: false,
                command.Bemerkung,
                command.ErstelltVonUserId,
                DateTimeOffset.UtcNow);

            await _fahrtenbuchRepository.AddAsync(eintrag, cancellationToken);
            await _fahrtenbuchRepository.SaveChangesAsync(cancellationToken);

            return CreateFahrtenbuchEintragResult.Erfolg();
        }
        catch (ArgumentException ex)
        {
            return CreateFahrtenbuchEintragResult.Fehler(ex.Message);
        }
    }
}