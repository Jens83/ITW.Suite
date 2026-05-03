namespace ITW.Web.ViewModels.Navigation;

public sealed class BereichsNavigationViewModel
{
    public IReadOnlyList<BereichsNavigationSectionViewModel> Sektionen { get; init; }
        = Array.Empty<BereichsNavigationSectionViewModel>();
}