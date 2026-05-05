using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugPruefungen;

public sealed record SaveFahrzeugPruefungCommand(
    Guid FahrzeugId,
    FahrzeugPruefungTyp Typ,
    DateOnly? FaelligAm,
    DateOnly? LetzteErledigungAm,
    string? Bemerkung,
    string UserId);

public sealed class SaveFahrzeugPruefungResult
{
    private SaveFahrzeugPruefungResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveFahrzeugPruefungResult Erfolg()
        => new(true, null);

    public static SaveFahrzeugPruefungResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class SaveFahrzeugPruefungService
{
    private readonly IFahrzeugRepository _fahrzeugRepository;
    private readonly IFahrzeugPruefungRepository _pruefungRepository;

    public SaveFahrzeugPruefungService(
        IFahrzeugRepository fahrzeugRepository,
        IFahrzeugPruefungRepository pruefungRepository)
    {
        ArgumentNullException.ThrowIfNull(fahrzeugRepository);
        _fahrzeugRepository = fahrzeugRepository;

        ArgumentNullException.ThrowIfNull(pruefungRepository);
        _pruefungRepository = pruefungRepository;
    }

    public async Task<SaveFahrzeugPruefungResult> ExecuteAsync(
        SaveFahrzeugPruefungCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.FahrzeugId == Guid.Empty)
        {
            return SaveFahrzeugPruefungResult.Fehler("Das Fahrzeug konnte nicht ermittelt werden.");
        }

        if (command.Typ == FahrzeugPruefungTyp.Unbekannt)
        {
            return SaveFahrzeugPruefungResult.Fehler("Bitte einen Prüfpunkt auswählen.");
        }

        if (!command.FaelligAm.HasValue)
        {
            return SaveFahrzeugPruefungResult.Fehler("Bitte ein Fälligkeitsdatum eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return SaveFahrzeugPruefungResult.Fehler("Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }

        var fahrzeug = await _fahrzeugRepository.GetFahrzeugByIdAsync(
            command.FahrzeugId,
            cancellationToken);

        if (fahrzeug is null)
        {
            return SaveFahrzeugPruefungResult.Fehler("Das Fahrzeug wurde nicht gefunden.");
        }

        try
        {
            var jetzt = DateTimeOffset.UtcNow;

            var vorhandenePruefung = await _pruefungRepository.GetByFahrzeugIdUndTypAsync(
                command.FahrzeugId,
                command.Typ,
                cancellationToken);

            if (vorhandenePruefung is null)
            {
                var neuePruefung = new FahrzeugPruefung(
                    Guid.NewGuid(),
                    command.FahrzeugId,
                    command.Typ,
                    command.FaelligAm.Value,
                    command.LetzteErledigungAm,
                    command.Bemerkung,
                    command.UserId,
                    jetzt);

                await _pruefungRepository.AddAsync(neuePruefung, cancellationToken);
            }
            else
            {
                vorhandenePruefung.Aktualisiere(
                    command.FaelligAm.Value,
                    command.LetzteErledigungAm,
                    command.Bemerkung,
                    command.UserId,
                    jetzt);
            }

            await _pruefungRepository.SaveChangesAsync(cancellationToken);

            return SaveFahrzeugPruefungResult.Erfolg();
        }
        catch (ArgumentException ex)
        {
            return SaveFahrzeugPruefungResult.Fehler(ex.Message);
        }
    }
}