using ITW.Fahrzeugmanagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class FahrtenbuchViewModel
{
    public Guid FahrzeugId { get; set; }

    public string FahrzeugText { get; set; } = string.Empty;

    public string Titel { get; set; } = "Fahrtenbuch";

    public string Beschreibung { get; set; } =
        "Fahrten und Kilometerstände zum Fahrzeug dokumentieren.";

    public FahrtKategorie FahrtKategorie { get; set; }

    public string Fahrtzweck { get; set; } = string.Empty;

    public string FahrerName { get; set; } = string.Empty;

    public string? BeifahrerName { get; set; }

    public DateTime? Startzeit { get; set; }

    public DateTime? Endzeit { get; set; }

    public string? Startort { get; set; }

    public string? Zielort { get; set; }

    public int StartKilometerstand { get; set; }

    public int? EndKilometerstand { get; set; }

    public string? Bemerkung { get; set; }

    public FahrzeugDetailNavigationViewModel Navigation { get; set; } = new();

    public IReadOnlyList<SelectListItem> FahrtKategorieOptionen { get; set; } = [];

    public IReadOnlyList<FahrtenbuchEintragItemViewModel> Eintraege { get; set; } = [];
}

public sealed class FahrtenbuchEintragItemViewModel
{
    public Guid EintragId { get; init; }

    public string FahrzeugText { get; init; } = string.Empty;

    public string FahrerName { get; init; } = string.Empty;

    public string BeifahrerName { get; init; } = "-";

    public FahrtKategorie FahrtKategorie { get; init; }

    public string FahrtKategorieText => FahrtKategorie switch
    {
        FahrtKategorie.Einsatzfahrt => "Einsatzfahrt",
        FahrtKategorie.Dienstfahrt => "Dienstfahrt",
        FahrtKategorie.Werkstattfahrt => "Werkstattfahrt",
        FahrtKategorie.Tankfahrt => "Tankfahrt",
        FahrtKategorie.Ueberfuehrungsfahrt => "Überführungsfahrt",
        FahrtKategorie.Sonstige => "Sonstige Fahrt",
        _ => "Unbekannt"
    };

    public string Fahrtzweck { get; init; } = string.Empty;

    public DateTimeOffset StartzeitUtc { get; init; }

    public DateTimeOffset? EndzeitUtc { get; init; }

    public string? Startort { get; init; }

    public string? Zielort { get; init; }

    public int StartKilometerstand { get; init; }

    public int? EndKilometerstand { get; init; }

    public int? GefahreneKilometer { get; init; }

    public decimal? TankmengeLiter { get; init; }

    public int? KilometerstandBeimTanken { get; init; }

    public FahrtenbuchStatus Status { get; init; }

    public string? Bemerkung { get; init; }

    public string DatumText => StartzeitUtc.ToLocalTime().ToString("dd.MM.yyyy");

    public string StartzeitText => StartzeitUtc.ToLocalTime().ToString("HH:mm");

    public string EndzeitText => EndzeitUtc.HasValue
        ? EndzeitUtc.Value.ToLocalTime().ToString("HH:mm")
        : "-";

    public string ZeitraumText => $"{StartzeitText} - {EndzeitText}";

    public string StartortText => string.IsNullOrWhiteSpace(Startort)
        ? "-"
        : Startort;

    public string ZielortText => string.IsNullOrWhiteSpace(Zielort)
        ? "-"
        : Zielort;

    public string StreckeText => $"{StartortText} → {ZielortText}";

    public string StartKilometerstandText => $"{StartKilometerstand:N0} km";

    public string EndKilometerstandText => EndKilometerstand.HasValue
        ? $"{EndKilometerstand.Value:N0} km"
        : "-";

    public string GefahreneKilometerText => GefahreneKilometer.HasValue
        ? $"{GefahreneKilometer.Value:N0} km"
        : "-";

    public string TankmengeText => TankmengeLiter.HasValue
        ? $"{TankmengeLiter.Value:N2} l"
        : "-";

    public string KilometerstandBeimTankenText => KilometerstandBeimTanken.HasValue
        ? $"{KilometerstandBeimTanken.Value:N0} km"
        : "-";

    public bool IstOffen => Status == FahrtenbuchStatus.Offen;

    public string StatusText => Status switch
    {
        FahrtenbuchStatus.Offen => "Offen",
        FahrtenbuchStatus.Abgeschlossen => "Abgeschlossen",
        FahrtenbuchStatus.Korrigiert => "Korrigiert",
        FahrtenbuchStatus.Storniert => "Storniert",
        _ => "Unbekannt"
    };

    public string StatusBadgeCssClass => Status switch
    {
        FahrtenbuchStatus.Offen => "app-pill--warning",
        FahrtenbuchStatus.Abgeschlossen => "app-pill--success",
        FahrtenbuchStatus.Korrigiert => "app-pill--accent",
        FahrtenbuchStatus.Storniert => "app-pill--neutral",
        _ => "app-pill--neutral"
    };
}