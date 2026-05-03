namespace ITW.Web.ViewModels.Home;

public sealed class HomeIndexViewModel
{
    public bool IstAngemeldet { get; init; }

    public string? AktuellerBereichName { get; init; }

    public IReadOnlyList<HomeBereichKachelViewModel> Bereiche { get; init; }
        = Array.Empty<HomeBereichKachelViewModel>();
}