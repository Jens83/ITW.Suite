using ITW.Web.ViewModels.Navigation;

namespace ITW.Web.ViewModels.Layout;

public sealed class AppShellViewModel
{
    public string ThemeCssClass { get; init; } = "theme-itw";

    public string PageTitle { get; init; } = string.Empty;

    public bool ShowMenu { get; init; } = true;

    public string AreaDisplayName { get; init; } = string.Empty;

    public string MenuBrandPrimaryText { get; init; } = "Intensivtransport";

    public string MenuBrandAccentText { get; init; } = "Neubrandenburg";

    public string MenuBrandSubtitle { get; init; } = "Arbeitsbereich";

    public string MenuBrandIconCssClass { get; init; } = "bi bi-grid-1x2-fill";

    public BereichsNavigationViewModel? Navigation { get; init; }

    public BereichsBenutzerMenuViewModel? UserMenu { get; init; }

    public int OffeneDienstplanPerioden { get; init; }

    public int UrlaubsBadgeAnzahl { get; init; }
}