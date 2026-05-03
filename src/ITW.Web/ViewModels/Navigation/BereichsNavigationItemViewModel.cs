namespace ITW.Web.ViewModels.Navigation;

public sealed class BereichsNavigationItemViewModel
{
    public string Text { get; init; } = string.Empty;

    public string IconCssClass { get; init; } = string.Empty;

    public string Area { get; init; } = string.Empty;

    public string Controller { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public bool IstAktiv { get; init; }
}