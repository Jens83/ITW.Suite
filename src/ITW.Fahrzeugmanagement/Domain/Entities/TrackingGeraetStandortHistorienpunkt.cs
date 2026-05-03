namespace ITW.Fahrzeugmanagement.Domain.Entities;

public sealed class TrackingGeraetStandortHistorienpunkt
{
    private TrackingGeraetStandortHistorienpunkt()
    {
        DeviceIdentifier = string.Empty;
    }

    public TrackingGeraetStandortHistorienpunkt(
        Guid trackingGeraetId,
        Guid routeSessionId,
        decimal latitude,
        decimal longitude,
        decimal speedKmh,
        DateTimeOffset erfasstAmUtc,
        string deviceIdentifier)
    {
        if (trackingGeraetId == Guid.Empty)
        {
            throw new ArgumentException("Die TrackingGeraetId ist erforderlich.", nameof(trackingGeraetId));
        }

        if (routeSessionId == Guid.Empty)
        {
            throw new ArgumentException("Die RouteSessionId ist erforderlich.", nameof(routeSessionId));
        }

        if (string.IsNullOrWhiteSpace(deviceIdentifier))
        {
            throw new ArgumentException("Der Device-Identifier ist erforderlich.", nameof(deviceIdentifier));
        }

        if (speedKmh < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(speedKmh), "Die Geschwindigkeit darf nicht negativ sein.");
        }

        TrackingGeraetId = trackingGeraetId;
        RouteSessionId = routeSessionId;
        Latitude = latitude;
        Longitude = longitude;
        SpeedKmh = speedKmh;
        ErfasstAmUtc = erfasstAmUtc;
        DeviceIdentifier = deviceIdentifier.Trim();
    }

    public long Id { get; private set; }

    public Guid TrackingGeraetId { get; private set; }

    public Guid RouteSessionId { get; private set; }

    public decimal Latitude { get; private set; }

    public decimal Longitude { get; private set; }

    public decimal SpeedKmh { get; private set; }

    public DateTimeOffset ErfasstAmUtc { get; private set; }

    public string DeviceIdentifier { get; private set; }
}