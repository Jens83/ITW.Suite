using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class FahrzeugeIndexViewModel
{
    public string Titel { get; init; } = "Fahrzeuge";

    public string Beschreibung { get; init; } =
        "Übersicht der Fahrzeuge im Intensivtransport.";

    public IReadOnlyList<FahrzeugIndexItemViewModel> Fahrzeuge { get; init; } = [];
}

public sealed class FahrzeugIndexItemViewModel
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

    public string StandardStandort { get; init; } = "-";

    public string FahrzeugnameText => $"{Hersteller} {Modell}".Trim();

    public string ErstzulassungText => Erstzulassung.HasValue
        ? Erstzulassung.Value.ToString("dd.MM.yyyy")
        : "-";

    public string BaujahrText => Baujahr.HasValue
        ? Baujahr.Value.ToString()
        : "-";

    public string KilometerstandText => $"{KilometerstandAktuell:N0} km";

    public string LeistungText => LeistungKw.HasValue
        ? $"{LeistungKw.Value} kW"
        : "-";

    public string KraftstoffText => Kraftstoffart switch
    {
        Kraftstoffart.Diesel => "Diesel",
        Kraftstoffart.Benzin => "Benzin",
        Kraftstoffart.Elektro => "Elektro",
        Kraftstoffart.Hybrid => "Hybrid",
        Kraftstoffart.Gas => "Gas",
        Kraftstoffart.Wasserstoff => "Wasserstoff",
        _ => "-"
    };

    public string StatusText => Status switch
    {
        FahrzeugStatus.Aktiv => "Aktiv",
        FahrzeugStatus.InWartung => "In Wartung",
        FahrzeugStatus.AusserBetrieb => "Außer Betrieb",
        FahrzeugStatus.Reserviert => "Reserviert",
        FahrzeugStatus.Archiviert => "Archiviert",
        _ => "Unbekannt"
    };

    public string StatusBadgeCssClass => Status switch
    {
        FahrzeugStatus.Aktiv => "bg-success",
        FahrzeugStatus.InWartung => "bg-warning text-dark",
        FahrzeugStatus.AusserBetrieb => "bg-danger",
        FahrzeugStatus.Reserviert => "bg-primary",
        FahrzeugStatus.Archiviert => "bg-secondary",
        _ => "bg-secondary"
    };
}