using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Navigation.AreaNavigation;

public static class BereichsRoutingHelper
{
    public static string GetBereichsname(OrganisationsbereichCode bereich)
    {
        return bereich switch
        {
            OrganisationsbereichCode.Intensivtransport => "Intensivtransport",
            OrganisationsbereichCode.Verwaltung => "Verwaltung",
            OrganisationsbereichCode.Vorstand => "Geschäftsführung",
            _ => "zuständigen Bereich"
        };
    }

    public static string? GetAreaName(OrganisationsbereichCode bereich)
    {
        return bereich switch
        {
            OrganisationsbereichCode.Intensivtransport => "Intensivtransport",
            OrganisationsbereichCode.Verwaltung => "Verwaltung",
            OrganisationsbereichCode.Vorstand => "Geschaeftsfuehrung",
            _ => null
        };
    }

    public static bool TryGetAreaName(
        OrganisationsbereichCode bereich,
        out string areaName)
    {
        areaName = GetAreaName(bereich) ?? string.Empty;
        return !string.IsNullOrWhiteSpace(areaName);
    }
}