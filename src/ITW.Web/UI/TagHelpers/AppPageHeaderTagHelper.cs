using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ITW.Web.UI.TagHelpers;

[HtmlTargetElement("app-page-header")]
public sealed class AppPageHeaderTagHelper : TagHelper
{
    [HtmlAttributeName("title")]
    public string Title { get; set; } = string.Empty;

    [HtmlAttributeName("description")]
    public string? Description { get; set; }

    [HtmlAttributeName("eyebrow")]
    public string? Eyebrow { get; set; }

    [HtmlAttributeName("eyebrow-icon")]
    public string? EyebrowIcon { get; set; }

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = null;

        var childContent = await output.GetChildContentAsync();
        var hasActions = !childContent.IsEmptyOrWhiteSpace;

        var sb = new StringBuilder();
        sb.Append("<header class=\"app-page-header\">");
        sb.Append("<div class=\"app-page-heading\">");

        if (!string.IsNullOrWhiteSpace(Eyebrow) || !string.IsNullOrWhiteSpace(EyebrowIcon))
        {
            sb.Append("<div class=\"app-page-heading__eyebrow\">");

            if (!string.IsNullOrWhiteSpace(EyebrowIcon))
            {
                sb.Append($"<i class=\"{HtmlEncoder.Default.Encode(EyebrowIcon)}\"></i> ");
            }

            if (!string.IsNullOrWhiteSpace(Eyebrow))
            {
                sb.Append(HtmlEncoder.Default.Encode(Eyebrow));
            }

            sb.Append("</div>");
        }

        sb.Append($"<h1 class=\"app-page-heading__title\">{HtmlEncoder.Default.Encode(Title)}</h1>");

        if (!string.IsNullOrWhiteSpace(Description))
        {
            sb.Append($"<p class=\"app-page-heading__text\">{HtmlEncoder.Default.Encode(Description)}</p>");
        }

        sb.Append("</div>");

        if (hasActions)
        {
            sb.Append("<div class=\"app-page-actions\">");
            sb.Append(childContent.GetContent());
            sb.Append("</div>");
        }

        sb.Append("</header>");

        output.Content.SetHtmlContent(sb.ToString());
    }
}
