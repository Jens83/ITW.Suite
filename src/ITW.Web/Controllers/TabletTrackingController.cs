using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Controllers;

[AllowAnonymous]
[Route("tablet/tracking")]
public sealed class TabletTrackingController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}