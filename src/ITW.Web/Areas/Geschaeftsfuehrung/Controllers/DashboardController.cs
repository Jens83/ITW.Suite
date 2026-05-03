using ITW.Application.Organisation.Contracts;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Geschaeftsfuehrung.Controllers;

[Area("Geschaeftsfuehrung")]
public sealed class DashboardController : BereichsDashboardControllerBase
{
    public DashboardController(ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Vorstand;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
        {
            return redirectResult;
        }

        return View();
    }
}