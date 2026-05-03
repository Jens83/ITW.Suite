namespace ITW.Web.ViewModels.Personal;

public sealed class ItwMitarbeiterprofilListeViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public IReadOnlyList<ItwMitarbeiterprofilEintragViewModel> Profile { get; set; }
        = Array.Empty<ItwMitarbeiterprofilEintragViewModel>();
}