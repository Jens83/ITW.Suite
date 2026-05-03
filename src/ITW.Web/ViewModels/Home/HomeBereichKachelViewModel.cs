namespace ITW.Web.ViewModels.Home;

public sealed class HomeBereichKachelViewModel
{
    public string Titel { get; init; } = string.Empty;

    public string Beschreibung { get; init; } = string.Empty;

    public string Area { get; init; } = string.Empty;

    public string Controller { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public string IconCssClass { get; init; } = string.Empty;

    public string BorderCssClass { get; init; } = string.Empty;

    public string ButtonCssClass { get; init; } = string.Empty;

    public bool IstMeinBereich { get; init; }

    public string ButtonText { get; init; } = string.Empty;
}