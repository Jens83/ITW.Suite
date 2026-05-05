using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ITW.Web.UI.TagHelpers;

[HtmlTargetElement("button", Attributes = "app-btn")]
[HtmlTargetElement("a", Attributes = "app-btn")]
public sealed class AppButtonTagHelper : TagHelper
{
    [HtmlAttributeName("app-btn")]
    public string? Modifiers { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var classes = new List<string> { "app-btn" };

        if (!string.IsNullOrWhiteSpace(Modifiers))
        {
            foreach (var modifier in Modifiers.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                classes.Add($"app-btn-{modifier}");
            }
        }

        output.Attributes.RemoveAll("app-btn");

        var existingClass = output.Attributes["class"]?.Value?.ToString() ?? string.Empty;

        var allClasses = string.IsNullOrWhiteSpace(existingClass)
            ? string.Join(" ", classes)
            : $"{string.Join(" ", classes)} {existingClass}";

        output.Attributes.SetAttribute("class", allClasses);
    }
}
