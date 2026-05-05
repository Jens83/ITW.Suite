using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugVertraege;

public sealed record CreateFahrzeugVertragCommand(
    Guid FahrzeugId,
    FahrzeugVertragTyp VertragTyp,
    string Anbieter,
    string Vertragsnummer,
    DateOnly? GueltigVon,
    DateOnly? GueltigBis,
    decimal? BetragProPeriode,
    int? Periodizitaet,
    int? KuendigungsfristTage,
    string? Notiz,
    string ErstelltVonUserId);

public sealed class CreateFahrzeugVertragResult
{
    private CreateFahrzeugVertragResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static CreateFahrzeugVertragResult Erfolg()
        => new(true, null);

    public static CreateFahrzeugVertragResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class CreateFahrzeugVertragService
{
    private readonly IFahrzeugRepository _fahrzeugRepository;
    private readonly IFahrzeugVertragRepository _vertragRepository;

    public CreateFahrzeugVertragService(
        IFahrzeugRepository fahrzeugRepository,
        IFahrzeugVertragRepository vertragRepository)
    {
        ArgumentNullException.ThrowIfNull(fahrzeugRepository);
        _fahrzeugRepository = fahrzeugRepository;

        ArgumentNullException.ThrowIfNull(vertragRepository);
        _vertragRepository = vertragRepository;
    }

    public async Task<CreateFahrzeugVertragResult> ExecuteAsync(
        CreateFahrzeugVertragCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.FahrzeugId == Guid.Empty)
        {
            return CreateFahrzeugVertragResult.Fehler("Das Fahrzeug konnte nicht ermittelt werden.");
        }

        if (command.VertragTyp == FahrzeugVertragTyp.Unbekannt)
        {
            return CreateFahrzeugVertragResult.Fehler("Bitte einen Vertragstyp auswählen.");
        }

        if (string.IsNullOrWhiteSpace(command.Anbieter))
        {
            return CreateFahrzeugVertragResult.Fehler("Bitte einen Anbieter eingeben.");
        }

        if (string.IsNullOrWhiteSpace(command.Vertragsnummer))
        {
            return CreateFahrzeugVertragResult.Fehler("Bitte eine Vertragsnummer eingeben.");
        }

        if (!command.GueltigVon.HasValue)
        {
            return CreateFahrzeugVertragResult.Fehler("Bitte ein Beginn-Datum eingeben.");
        }

        if (command.GueltigBis.HasValue &&
            command.GueltigBis.Value < command.GueltigVon.Value)
        {
            return CreateFahrzeugVertragResult.Fehler("Das Ende darf nicht vor dem Beginn liegen.");
        }

        if (command.BetragProPeriode.HasValue &&
            command.BetragProPeriode.Value < 0)
        {
            return CreateFahrzeugVertragResult.Fehler("Der Betrag darf nicht negativ sein.");
        }

        if (command.Periodizitaet.HasValue &&
            command.Periodizitaet.Value < 0)
        {
            return CreateFahrzeugVertragResult.Fehler("Die Periodizität darf nicht negativ sein.");
        }

        if (command.KuendigungsfristTage.HasValue &&
            command.KuendigungsfristTage.Value < 0)
        {
            return CreateFahrzeugVertragResult.Fehler("Die Kündigungsfrist darf nicht negativ sein.");
        }

        if (string.IsNullOrWhiteSpace(command.ErstelltVonUserId))
        {
            return CreateFahrzeugVertragResult.Fehler("Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }

        var fahrzeug = await _fahrzeugRepository.GetFahrzeugByIdAsync(
            command.FahrzeugId,
            cancellationToken);

        if (fahrzeug is null)
        {
            return CreateFahrzeugVertragResult.Fehler("Das Fahrzeug wurde nicht gefunden.");
        }

        try
        {
            var vertrag = new FahrzeugVertrag(
                Guid.NewGuid(),
                command.FahrzeugId,
                command.VertragTyp,
                command.Anbieter,
                command.Vertragsnummer,
                command.GueltigVon.Value,
                command.GueltigBis,
                command.BetragProPeriode,
                command.Periodizitaet,
                command.KuendigungsfristTage,
                dokumentId: null,
                command.Notiz,
                command.ErstelltVonUserId,
                DateTimeOffset.UtcNow);

            await _vertragRepository.AddAsync(vertrag, cancellationToken);
            await _vertragRepository.SaveChangesAsync(cancellationToken);

            return CreateFahrzeugVertragResult.Erfolg();
        }
        catch (ArgumentException ex)
        {
            return CreateFahrzeugVertragResult.Fehler(ex.Message);
        }
     
    }
}