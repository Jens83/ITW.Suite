using ITW.Fahrzeugmanagement.Application.Contracts;

namespace ITW.Fahrzeugmanagement.Application.Tracking;

public sealed class ReadTabletLiveStandortOverview
{
    public IReadOnlyList<TabletLiveStandortListItem> Tablets { get; init; } = [];
}

public sealed class TabletLiveStandortListItem
{
    public Guid TrackingGeraetId { get; init; }

    public string DeviceIdentifier { get; init; } = string.Empty;

    public bool IstAktiv { get; init; }

    public DateTimeOffset? LetzterKontaktAm { get; init; }

    public bool HatStandort { get; init; }

    public Guid? RouteSessionId { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public decimal? SpeedKmh { get; init; }

    public DateTimeOffset? ErfasstAmUtc { get; init; }

    public DateTimeOffset? AktualisiertAmUtc { get; init; }

    public bool IstOnline { get; init; }

    public bool IstInBewegung { get; init; }

    public decimal GefahreneStreckeKm { get; init; }

    public IReadOnlyList<TabletRoutePointListItem> RouteHistorie { get; init; } = [];
}

public sealed class TabletRoutePointListItem
{
    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }

    public decimal SpeedKmh { get; init; }

    public DateTimeOffset ErfasstAmUtc { get; init; }
}

public sealed class ReadTabletLiveStandortOverviewService
{
    private static readonly TimeSpan OnlineGrenze = TimeSpan.FromMinutes(2);
    private const int MaxHistorienpunkte = 500;
    private const decimal MindestgeschwindigkeitFaehrtKmh = 5m;

    private readonly IFahrzeugTrackingRepository _repository;

    public ReadTabletLiveStandortOverviewService(IFahrzeugTrackingRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<ReadTabletLiveStandortOverview> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var geraete = await _repository.GetTrackingGeraeteAsync(cancellationToken);
        var jetztUtc = DateTimeOffset.UtcNow;

        var items = new List<TabletLiveStandortListItem>();

        foreach (var geraet in geraete)
        {
            var standort = await _repository.GetAktuellenTrackingGeraetStandortAsync(
                geraet.Id,
                cancellationToken);

            var istOnline =
                geraet.LetzterKontaktAm.HasValue &&
                jetztUtc - geraet.LetzterKontaktAm.Value.ToUniversalTime() <= OnlineGrenze;

            var routeHistorie = new List<TabletRoutePointListItem>();
            var gefahreneStreckeKm = 0m;
            var istInBewegung = false;

            if (standort is not null)
            {
                var historienpunkte = await _repository.GetTrackingGeraetHistorienpunkteAsync(
                    geraet.Id,
                    standort.RouteSessionId,
                    MaxHistorienpunkte,
                    cancellationToken);

                routeHistorie = historienpunkte
                    .Select(x => new TabletRoutePointListItem
                    {
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,
                        SpeedKmh = x.SpeedKmh,
                        ErfasstAmUtc = x.ErfasstAmUtc
                    })
                    .ToList();

                gefahreneStreckeKm = BerechneStreckeKm(routeHistorie);
                istInBewegung = standort.SpeedKmh >= MindestgeschwindigkeitFaehrtKmh;
            }

            items.Add(new TabletLiveStandortListItem
            {
                TrackingGeraetId = geraet.Id,
                DeviceIdentifier = geraet.DeviceIdentifier,
                IstAktiv = geraet.IstAktiv,
                LetzterKontaktAm = geraet.LetzterKontaktAm,
                HatStandort = standort is not null,
                RouteSessionId = standort?.RouteSessionId,
                Latitude = standort?.Latitude,
                Longitude = standort?.Longitude,
                SpeedKmh = standort?.SpeedKmh,
                ErfasstAmUtc = standort?.ErfasstAmUtc,
                AktualisiertAmUtc = standort?.AktualisiertAmUtc,
                IstOnline = istOnline,
                IstInBewegung = istInBewegung,
                GefahreneStreckeKm = gefahreneStreckeKm,
                RouteHistorie = routeHistorie
            });
        }

        return new ReadTabletLiveStandortOverview
        {
            Tablets = items
                .OrderByDescending(x => x.IstOnline)
                .ThenByDescending(x => x.HatStandort)
                .ThenBy(x => x.DeviceIdentifier)
                .ToList()
        };
    }

    private static decimal BerechneStreckeKm(IReadOnlyList<TabletRoutePointListItem> punkte)
    {
        if (punkte.Count < 2)
        {
            return 0m;
        }

        double meter = 0;

        for (var i = 1; i < punkte.Count; i++)
        {
            meter += BerechneDistanzInMetern(
                punkte[i - 1].Latitude,
                punkte[i - 1].Longitude,
                punkte[i].Latitude,
                punkte[i].Longitude);
        }

        return Math.Round((decimal)(meter / 1000d), 2, MidpointRounding.AwayFromZero);
    }

    private static double BerechneDistanzInMetern(
        decimal latitude1,
        decimal longitude1,
        decimal latitude2,
        decimal longitude2)
    {
        const double erdradiusMeter = 6371000d;

        var lat1 = GradZuRadiant((double)latitude1);
        var lon1 = GradZuRadiant((double)longitude1);
        var lat2 = GradZuRadiant((double)latitude2);
        var lon2 = GradZuRadiant((double)longitude2);

        var deltaLat = lat2 - lat1;
        var deltaLon = lon2 - lon1;

        var a =
            Math.Pow(Math.Sin(deltaLat / 2d), 2d) +
            Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2d), 2d);

        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));

        return erdradiusMeter * c;
    }

    private static double GradZuRadiant(double grad)
        => grad * (Math.PI / 180d);
}