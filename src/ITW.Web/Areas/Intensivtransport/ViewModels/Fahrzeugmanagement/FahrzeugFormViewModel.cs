using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;

public sealed class FahrzeugFormViewModel
{
    public Guid FahrzeugId { get; set; }

    public string Titel { get; set; } = "Fahrzeug anlegen";

    public string Beschreibung { get; set; } =
        "Neues Fahrzeug für den Intensivtransport erfassen.";

    public string? Fehlermeldung { get; set; }

    public string InterneNummer { get; set; } = string.Empty;

    public string Kennzeichen { get; set; } = string.Empty;

    public string Vin { get; set; } = string.Empty;

    public string Hersteller { get; set; } = string.Empty;

    public string Modell { get; set; } = string.Empty;

    public string Fahrzeugtyp { get; set; } = string.Empty;

    public int? Baujahr { get; set; }

    public DateOnly? Erstzulassung { get; set; }

    public Kraftstoffart Kraftstoffart { get; set; }

    public int? LeistungKw { get; set; }

    public int KilometerstandAktuell { get; set; }

    public FahrzeugStatus Status { get; set; } = FahrzeugStatus.Aktiv;

    public string? StandardStandort { get; set; }

    public FahrzeugDetailNavigationViewModel Navigation { get; set; } = new();
}