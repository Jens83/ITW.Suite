using ITW.Application.Organisation.Contracts;
using ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Dienstplan)]
public sealed class DienstplanController : IntensivtransportDienstplanControllerBase
{
    public DienstplanController(
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: false,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        return zugriff.CurrentUser!.Rolle == BereichsrolleCode.Wachleiter
            ? RedirectToAction("Index", "DienstplanWachleiter")
            : RedirectToAction("Index", "DienstplanMitarbeiter");
    }
}