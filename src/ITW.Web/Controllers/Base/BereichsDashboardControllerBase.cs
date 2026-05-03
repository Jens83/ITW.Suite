using ITW.Application.Organisation.Contracts;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Controllers.Base;

[Authorize]
public abstract class BereichsDashboardControllerBase : BereichsControllerBase
{
    protected BereichsDashboardControllerBase(
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
    }

    protected override abstract OrganisationsbereichCode Bereich { get; }

    protected async Task<IActionResult?> PruefeBereichszugriffAsync(
        CancellationToken cancellationToken)
    {
        var result = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        if (!result.IsSuccess || result.CurrentUser is null)
        {
            return RedirectToKeinZugriff();
        }

        if (result.CurrentUser.Bereich == Bereich)
        {
            return null;
        }

        return ErzeugeBereichsWeiterleitung(
            result.CurrentUser.Bereich,
            "Dashboard",
            "Index",
            "Dashboard",
            BereichsRoutingHelper.GetBereichsname(Bereich),
            $"Sie haben das Dashboard für den Bereich {BereichsRoutingHelper.GetBereichsname(Bereich)} geöffnet. Aufgrund Ihrer Zuordnung wird jetzt Ihr zuständiger Bereich geladen.");
    }
}