using ITW.Application.Organisation.Contracts;
using ITW.Web.Authorization.Modules;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Lagermanagement;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Lagerlogistik)]
public abstract class LagermanagementControllerBase : BereichsDashboardControllerBase
{
    private const string AreaLayoutPath = "~/Views/Shared/_AppLayout.cshtml";

    protected LagermanagementControllerBase(ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    protected IActionResult LagerView(string viewName, object model)
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
        return View($"~/Areas/Intensivtransport/Views/Lagermanagement/{viewName}.cshtml", model);
    }
}
