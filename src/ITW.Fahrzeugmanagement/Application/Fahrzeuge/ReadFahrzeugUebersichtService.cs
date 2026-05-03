using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Fahrzeuge;

public sealed class ReadFahrzeugUebersicht
{
    public IReadOnlyList<FahrzeugUebersichtListItem> Fahrzeuge { get; init; } = [];
}

public sealed class FahrzeugUebersichtListItem
{
    public Guid FahrzeugId { get; init; }

    public string InterneNummer { get; init; } = string.Empty;

    public string Kennzeichen { get; init; } = string.Empty;

    public string Hersteller { get; init; } = string.Empty;

    public string Modell { get; init; } = string.Empty;

    public string Fahrzeugtyp { get; init; } = string.Empty;

    public int? Baujahr { get; init; }

    public DateOnly? Erstzulassung { get; init; }

    public Kraftstoffart Kraftstoffart { get; init; }

    public int? LeistungKw { get; init; }

    public int KilometerstandAktuell { get; init; }

    public FahrzeugStatus Status { get; init; }

    public string? StandardStandort { get; init; }
}

public sealed class ReadFahrzeugUebersichtService
{
    private readonly IFahrzeugRepository _repository;

    public ReadFahrzeugUebersichtService(IFahrzeugRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ReadFahrzeugUebersicht> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var fahrzeuge = await _repository.GetFahrzeugeAsync(cancellationToken);

        return new ReadFahrzeugUebersicht
        {
            Fahrzeuge = fahrzeuge
                .OrderBy(x => x.InterneNummer)
                .ThenBy(x => x.Kennzeichen)
                .Select(x => new FahrzeugUebersichtListItem
                {
                    FahrzeugId = x.Id,
                    InterneNummer = x.InterneNummer,
                    Kennzeichen = x.Kennzeichen,
                    Hersteller = x.Hersteller,
                    Modell = x.Modell,
                    Fahrzeugtyp = x.Fahrzeugtyp,
                    Baujahr = x.Baujahr,
                    Erstzulassung = x.Erstzulassung,
                    Kraftstoffart = x.Kraftstoffart,
                    LeistungKw = x.LeistungKw,
                    KilometerstandAktuell = x.KilometerstandAktuell,
                    Status = x.Status,
                    StandardStandort = x.StandardStandort
                })
                .ToList()
        };
    }
}