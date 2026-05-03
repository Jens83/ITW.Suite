namespace ITW.Web.ViewModels.UI;

public sealed class AppPageHeaderViewModel
{
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? Eyebrow { get; init; }

    public string? EyebrowIconCssClass { get; init; }

    public string? BadgeText { get; init; }

    public string? BadgeIconCssClass { get; init; }
}