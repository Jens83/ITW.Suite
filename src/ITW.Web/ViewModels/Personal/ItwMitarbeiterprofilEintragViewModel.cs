namespace ITW.Web.ViewModels.Personal;

public sealed class ItwMitarbeiterprofilEintragViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string AnzeigeName { get; set; } = string.Empty;

    public string Benutzername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IstGesperrt { get; set; }

    public bool HatStammdatenprofil { get; set; }

    public DateTimeOffset? StammdatenAktualisiertAm { get; set; }

    public string BeschaeftigungsartText { get; set; } = string.Empty;

    public bool HatProfil { get; set; }

    public string Hauptqualifikation { get; set; } = string.Empty;

    public IReadOnlyList<string> Zusatzqualifikationen { get; set; }
        = Array.Empty<string>();

    public DateTimeOffset? ProfilAktualisiertAm { get; set; }
}