namespace ITW.Web.ViewModels.UI;

public sealed class AppEmptyStateViewModel
{
    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string IconCssClass { get; init; } = "bi bi-inbox";

    public string? PrimaryActionText { get; init; }

    public string? PrimaryActionUrl { get; init; }

    public string? PrimaryActionIconCssClass { get; init; }

    public string? SecondaryActionText { get; init; }

    public string? SecondaryActionUrl { get; init; }

    public string? SecondaryActionIconCssClass { get; init; }
}