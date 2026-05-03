namespace ITW.Web.ViewModels.Navigation;

public sealed class BereichsNavigationSectionViewModel
{
    public string Titel { get; init; } = string.Empty;

    public IReadOnlyList<BereichsNavigationItemViewModel> Eintraege { get; init; }
        = Array.Empty<BereichsNavigationItemViewModel>();
}