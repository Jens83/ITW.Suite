using ITW.Fahrzeugmanagement.Application.Contracts;

namespace ITW.Fahrzeugmanagement.Application.Fahrtenbuch;

public sealed class ReadFahrtenbuchResult
{
    public IReadOnlyList<FahrtenbuchEintragListenEintrag> Eintraege { get; init; } = [];
}

public sealed class ReadFahrtenbuchService
{
    private readonly IFahrtenbuchRepository _repository;

    public ReadFahrtenbuchService(IFahrtenbuchRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<ReadFahrtenbuchResult> ExecuteAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return new ReadFahrtenbuchResult();
        }

        var eintraege = await _repository.GetByFahrzeugIdAsync(
            fahrzeugId,
            cancellationToken);

        return new ReadFahrtenbuchResult
        {
            Eintraege = eintraege
                .Select(x => new FahrtenbuchEintragListenEintrag
                {
                    EintragId = x.Id,
                    FahrzeugId = x.FahrzeugId,
                    FahrerUserId = x.FahrerUserId,
                    FahrerName = x.FahrerName,
                    BeifahrerName = x.BeifahrerName,
                    FahrtKategorie = x.FahrtKategorie,
                    Fahrtzweck = x.Fahrtzweck,
                    StartzeitUtc = x.StartzeitUtc,
                    EndzeitUtc = x.EndzeitUtc,
                    Startort = x.Startort,
                    Zielort = x.Zielort,
                    StartKilometerstand = x.StartKilometerstand,
                    EndKilometerstand = x.EndKilometerstand,
                    GefahreneKilometer = x.GefahreneKilometer,
                    TankmengeLiter = x.TankmengeLiter,
                    KilometerstandBeimTanken = x.KilometerstandBeimTanken,
                    Status = x.Status,
                    IstAutomatischVorbelegt = x.IstAutomatischVorbelegt,
                    Bemerkung = x.Bemerkung
                })
                .ToList()
        };
    }
}