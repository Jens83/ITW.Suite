using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Controllers;

[AllowAnonymous]
[Route("tablet/setup")]
public sealed class TabletSetupController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}