using System.Globalization;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class TabletLiveStandortIndexViewModel
{
    public string Titel { get; init; } = "Fahrzeugstandort";

    public string Beschreibung { get; init; } =
        "Aktueller Standort des Einsatzfahrzeugs.";

    public DateTimeOffset AktualisiertAm { get; init; }

    public TabletLiveStandortViewModel? FokusTablet { get; init; }

    public IReadOnlyList<TabletLiveStandortViewModel> Tablets { get; init; } = [];
}

public sealed class TabletLiveStandortViewModel
{
    public Guid TrackingGeraetId { get; init; }

    public string DeviceIdentifier { get; init; } = string.Empty;

    public bool IstAktiv { get; init; }

    public bool IstOnline { get; init; }

    public bool HatStandort { get; init; }

    public bool IstInBewegung { get; init; }

    public Guid? RouteSessionId { get; init; }

    public decimal? Latitude { get; init; }

    public decimal? Longitude { get; init; }

    public decimal? SpeedKmh { get; init; }

    public decimal GefahreneStreckeKm { get; init; }

    public DateTimeOffset? ErfasstAmUtc { get; init; }

    public DateTimeOffset? LetzterKontaktAm { get; init; }

    public IReadOnlyList<TabletRoutePointViewModel> RouteHistorie { get; init; } = [];

    public string OnlineStatusText
    {
        get
        {
            if (!IstAktiv)
            {
                return "Inaktiv";
            }

            return IstOnline ? "Online" : "Offline";
        }
    }

    public string OnlineStatusCssClass
    {
        get
        {
            if (!IstAktiv)
            {
                return "status-pill status-pill-neutral";
            }

            return IstOnline
                ? "status-pill status-pill-success"
                : "status-pill status-pill-warning";
        }
    }

    public string BewegungsstatusText
    {
        get
        {
            if (!HatStandort)
            {
                return "Kein Standort";
            }

            return IstInBewegung ? "Fährt" : "Steht";
        }
    }

    public string BewegungsstatusCssClass
        => IstInBewegung
            ? "status-pill status-pill-danger"
            : "status-pill status-pill-neutral";

    public string PositionText
    {
        get
        {
            if (!HatStandort || !Latitude.HasValue || !Longitude.HasValue)
            {
                return "Noch keine Position empfangen";
            }

            return $"{Latitude.Value.ToString("0.000000", CultureInfo.InvariantCulture)}, {Longitude.Value.ToString("0.000000", CultureInfo.InvariantCulture)}";
        }
    }

    public string SpeedText
        => SpeedKmh.HasValue
            ? $"{SpeedKmh.Value:0.0} km/h"
            : "-";

    public string GefahreneStreckeText
        => $"{GefahreneStreckeKm:0.00} km";

    public string LetzterKontaktText
        => LetzterKontaktAm.HasValue
            ? LetzterKontaktAm.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")
            : "-";

    public string ErfasstAmText
        => ErfasstAmUtc.HasValue
            ? ErfasstAmUtc.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")
            : "-";
}

public sealed class TabletRoutePointViewModel
{
    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }

    public decimal SpeedKmh { get; init; }

    public DateTimeOffset ErfasstAmUtc { get; init; }

    public string ErfasstAmText
        => ErfasstAmUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss");
}