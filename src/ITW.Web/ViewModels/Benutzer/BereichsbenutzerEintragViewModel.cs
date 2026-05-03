namespace ITW.Web.ViewModels.Benutzer;

public sealed class BereichsbenutzerEintragViewModel
{
    public Guid BereichszuordnungId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Benutzername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Bereich { get; set; } = string.Empty;

    public string Rolle { get; set; } = string.Empty;

    public string Fuehrungsverantwortung { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public bool IstGesperrt { get; set; }

    public DateTimeOffset ZugewiesenAm { get; set; }
}