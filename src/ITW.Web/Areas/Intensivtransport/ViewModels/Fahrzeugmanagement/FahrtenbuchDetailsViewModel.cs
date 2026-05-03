using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class FahrtenbuchDetailsViewModel
{
    public Guid FahrzeugId { get; init; }

    public Guid EintragId { get; init; }

    public string FahrzeugText { get; init; } = string.Empty;

    public string FahrerName { get; init; } = string.Empty;

    public string BeifahrerName { get; init; } = "-";

    public FahrtKategorie FahrtKategorie { get; init; }

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

    public bool IstAutomatischVorbelegt { get; init; }

    public string? Bemerkung { get; init; }

    public FahrzeugDetailNavigationViewModel Navigation { get; init; } = new();

    public string DatumText => StartzeitUtc.ToLocalTime().ToString("dd.MM.yyyy");

    public string StartzeitText => StartzeitUtc.ToLocalTime().ToString("HH:mm");

    public string EndzeitText => EndzeitUtc.HasValue
        ? EndzeitUtc.Value.ToLocalTime().ToString("HH:mm")
        : "-";

    public string StartDatumZeitText => StartzeitUtc.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

    public string EndDatumZeitText => EndzeitUtc.HasValue
        ? EndzeitUtc.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
        : "-";

    public string StartortText => string.IsNullOrWhiteSpace(Startort)
        ? "-"
        : Startort;

    public string ZielortText => string.IsNullOrWhiteSpace(Zielort)
        ? "-"
        : Zielort;

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
        FahrtenbuchStatus.Offen => "bg-warning text-dark",
        FahrtenbuchStatus.Abgeschlossen => "bg-success",
        FahrtenbuchStatus.Korrigiert => "bg-primary",
        FahrtenbuchStatus.Storniert => "bg-secondary",
        _ => "bg-secondary"
    };

    public string AutomatischVorbelegtText => IstAutomatischVorbelegt
        ? "Ja"
        : "Nein";

    public string BemerkungText => string.IsNullOrWhiteSpace(Bemerkung)
        ? "-"
        : Bemerkung;
}