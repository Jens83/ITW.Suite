namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class TrackingGeraeteIndexViewModel
{
    public string Titel { get; init; } = "Tablets";

    public string Beschreibung { get; init; } =
        "Hier siehst du die Einsatz-Tablets und kannst ein neues Tablet per QR-Code hinzufügen.";

    public string? Erfolgsmeldung { get; init; }

    public string? Fehlermeldung { get; init; }

    public IReadOnlyList<TrackingGeraetIndexItemViewModel> Geraete { get; init; } = [];
}

public sealed class TrackingGeraetIndexItemViewModel
{
    public Guid TrackingGeraetId { get; init; }

    public string DeviceIdentifier { get; init; } = string.Empty;

    public bool IstAktiv { get; init; }

    public bool IstOnline { get; init; }

    public bool HatStandort { get; init; }

    public bool IstInBewegung { get; init; }

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

    public string LetzterKontaktText { get; init; } = "-";

    public string GeschwindigkeitText { get; init; } = "-";

    public string StreckeText { get; init; } = "-";
}