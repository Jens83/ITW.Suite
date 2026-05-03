namespace ITW.Web.ViewModels.Benutzer;

public sealed class BereichsbenutzerlisteViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public string BereichName { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public IReadOnlyList<BereichsbenutzerEintragViewModel> Benutzer { get; set; }
        = Array.Empty<BereichsbenutzerEintragViewModel>();
}