using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Layout;

public sealed class AppLayoutMetadataProvider : IAppLayoutMetadataProvider
{
    private static readonly IReadOnlyDictionary<string, AppLayoutMetadata> Metadaten =
        new Dictionary<string, AppLayoutMetadata>(StringComparer.OrdinalIgnoreCase)
        {
            ["Intensivtransport"] = new(
                OrganisationsbereichCode.Intensivtransport,
                Bereichsanzeige: "Intensivtransport",
                AvatarBackgroundColor: "f59e0b",
                AvatarTextColor: "1c1917",
                ThemeCssClass: "theme-itw",
                MenuBrandAccentText: "Neubrandenburg",
                MenuBrandIconCssClass: "bi bi-car-front-fill",
                TitelSuffix: "ITW NB"),

            ["Verwaltung"] = new(
                OrganisationsbereichCode.Verwaltung,
                Bereichsanzeige: "Verwaltung",
                AvatarBackgroundColor: "6b7280",
                AvatarTextColor: "ffffff",
                ThemeCssClass: "theme-admin",
                MenuBrandAccentText: "Verwaltung",
                MenuBrandIconCssClass: "bi bi-building-gear",
                TitelSuffix: "Verwaltung – ITW NB"),

            ["Geschaeftsfuehrung"] = new(
                OrganisationsbereichCode.Vorstand,
                Bereichsanzeige: "Geschäftsführung",
                AvatarBackgroundColor: "ef4444",
                AvatarTextColor: "ffffff",
                ThemeCssClass: "theme-gf",
                MenuBrandAccentText: "Geschäftsführung",
                MenuBrandIconCssClass: "bi bi-graph-up-arrow",
                TitelSuffix: "Geschäftsführung – ITW NB"),
        };

    private static readonly AppLayoutMetadata Fallback = Metadaten["Intensivtransport"];

    public AppLayoutMetadata GetForArea(string areaName) =>
        Metadaten.TryGetValue(areaName, out var meta) ? meta : Fallback;
}
