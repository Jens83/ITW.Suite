using ITW.Application.Organisation.Contracts;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ITW.Web.Authorization.Modules;

public sealed class ModuleAccessFilter : IAsyncAuthorizationFilter
{
    private readonly ICurrentUserContextAccessor _currentUserContextAccessor;
    private readonly ModulCode _modul;

    public ModuleAccessFilter(
        ICurrentUserContextAccessor currentUserContextAccessor,
        ModulCode modul)
    {
        _currentUserContextAccessor = currentUserContextAccessor
            ?? throw new ArgumentNullException(nameof(currentUserContextAccessor));
        _modul = modul;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (_modul == ModulCode.Unbekannt)
        {
            context.Result = new RedirectToActionResult("KeinZugriff", "Account", new { area = "" });
            return;
        }

        var currentUserLookup = await _currentUserContextAccessor.GetCurrentAsync(context.HttpContext.RequestAborted);

        if (!currentUserLookup.IsSuccess || currentUserLookup.CurrentUser is null)
        {
            context.Result = new RedirectToActionResult("KeinZugriff", "Account", new { area = "" });
            return;
        }

        if (currentUserLookup.CurrentUser.HatModul(_modul))
        {
            return;
        }

        var areaName = BereichsRoutingHelper.GetAreaName(currentUserLookup.CurrentUser.Bereich);

        if (string.IsNullOrWhiteSpace(areaName))
        {
            context.Result = new RedirectToActionResult("KeinZugriff", "Account", new { area = "" });
            return;
        }

        context.Result = new RedirectToActionResult(
            "Index",
            "Dashboard",
            new { area = areaName });
    }
}