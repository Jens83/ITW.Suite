namespace ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter;

public sealed class MitarbeiterDetailViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string Benutzername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IstGesperrt { get; set; }

    public string Rolle { get; set; } = string.Empty;

    public string Fuehrungsverantwortung { get; set; } = string.Empty;

    public bool HatItwProfil { get; set; }

    public string Hauptqualifikation { get; set; } = string.Empty;

    public IReadOnlyList<string> Zusatzqualifikationen { get; set; } = Array.Empty<string>();

    public DateTimeOffset? ProfilAktualisiertAm { get; set; }

    public bool HatStammdatenprofil { get; set; }

    public string AnzeigeName { get; set; } = string.Empty;

    public string Vorname { get; set; } = string.Empty;

    public string Nachname { get; set; } = string.Empty;

    public string BeschaeftigungsartText { get; set; } = string.Empty;

    public string Telefonnummer { get; set; } = string.Empty;

    public string AnschriftKurz { get; set; } = string.Empty;

    public DateTimeOffset? StammdatenAktualisiertAm { get; set; }

    public DateTimeOffset ZugewiesenAm { get; set; }

    public IReadOnlyList<MitarbeiterDokumentEintragViewModel> Dokumente { get; set; }
        = Array.Empty<MitarbeiterDokumentEintragViewModel>();

    public MitarbeiterDetailNavigationViewModel Navigation { get; set; } = new();
}