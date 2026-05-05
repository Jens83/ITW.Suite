using ITW.Fahrzeugmanagement.Application.Contracts;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugVertraege;

public sealed class ReadFahrzeugVertraegeResult
{
    public IReadOnlyList<FahrzeugVertragListenEintrag> Vertraege { get; init; } = [];
}

public sealed class ReadFahrzeugVertraegeService
{
    private readonly IFahrzeugVertragRepository _repository;

    public ReadFahrzeugVertraegeService(IFahrzeugVertragRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<ReadFahrzeugVertraegeResult> ExecuteAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return new ReadFahrzeugVertraegeResult();
        }

        var vertraege = await _repository.GetByFahrzeugIdAsync(
            fahrzeugId,
            cancellationToken);

        return new ReadFahrzeugVertraegeResult
        {
            Vertraege = vertraege
                .Select(x => new FahrzeugVertragListenEintrag
                {
                    VertragId = x.Id,
                    FahrzeugId = x.FahrzeugId,
                    VertragTyp = x.VertragTyp,
                    Anbieter = x.Anbieter,
                    Vertragsnummer = x.Vertragsnummer,
                    GueltigVon = x.GueltigVon,
                    GueltigBis = x.GueltigBis,
                    BetragProPeriode = x.BetragProPeriode,
                    Periodizitaet = x.Periodizitaet,
                    KuendigungsfristTage = x.KuendigungsfristTage,
                    DokumentId = x.DokumentId,
                    Notiz = x.Notiz,
                    ErstelltAm = x.ErstelltAm
                })
                .ToList()
        };
    }
}