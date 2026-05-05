using ITW.Fahrzeugmanagement.Application.Contracts;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;

public sealed class ReadFahrzeugDokumenteResult
{
    public IReadOnlyList<FahrzeugDokumentListenEintrag> Dokumente { get; init; } = [];
}

public sealed class ReadFahrzeugDokumenteService
{
    private readonly IFahrzeugDokumentRepository _repository;

    public ReadFahrzeugDokumenteService(IFahrzeugDokumentRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<ReadFahrzeugDokumenteResult> ExecuteAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return new ReadFahrzeugDokumenteResult();
        }

        var dokumente = await _repository.GetByFahrzeugIdAsync(
            fahrzeugId,
            cancellationToken);

        return new ReadFahrzeugDokumenteResult
        {
            Dokumente = dokumente
                .OrderByDescending(x => x.HochgeladenAm)
                .ThenBy(x => x.Kategorie)
                .ThenBy(x => x.Bezeichnung)
                .Select(x => new FahrzeugDokumentListenEintrag
                    {
                        DokumentId = x.Id,
                        FahrzeugId = x.FahrzeugId,
                        Kategorie = x.Kategorie,
                        Dateiname = x.Dateiname,
                        Bezeichnung = x.Bezeichnung,
                        ContentType = x.ContentType,
                        GueltigBis = x.GueltigBis,
                        HochgeladenAm = x.HochgeladenAm
                    })
                    .ToList()
        };
    }
}