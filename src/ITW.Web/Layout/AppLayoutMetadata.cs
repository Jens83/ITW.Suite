using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Layout;

public sealed record AppLayoutMetadata(
    OrganisationsbereichCode Bereich,
    string Bereichsanzeige,
    string AvatarBackgroundColor,
    string AvatarTextColor,
    string ThemeCssClass,
    string MenuBrandAccentText,
    string MenuBrandIconCssClass,
    string TitelSuffix);
