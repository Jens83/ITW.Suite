using ITW.Application.Organisation.Contracts;
using ITW.Web.Controllers.Base;
using ITW.Web.Logging;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Verwaltung.Controllers;

[Area("Verwaltung")]
public sealed class SystemLogController : BereichsDashboardControllerBase
{
    private readonly ILogEintragService _logEintragService;

    public SystemLogController(
        ICurrentUserContextAccessor currentUserContextAccessor,
        ILogEintragService logEintragService)
        : base(currentUserContextAccessor)
    {
        _logEintragService = logEintragService
            ?? throw new ArgumentNullException(nameof(logEintragService));
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Verwaltung;

    public async Task<IActionResult> Index(
        string? level = null,
        CancellationToken cancellationToken = default)
    {
        var zugriffsPruefung = await PruefeBereichszugriffAsync(cancellationToken);
        if (zugriffsPruefung is not null)
            return zugriffsPruefung;

        var kontextResult = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        if (!kontextResult.IsSuccess
            || kontextResult.CurrentUser?.Fuehrungsverantwortung == FuehrungsverantwortungCode.Keine)
        {
            return RedirectToKeinZugriff();
        }

        var eintraege = await _logEintragService.GetRecentAsync(
            maxEintraege: 300,
            levelFilter: level,
            cancellationToken: cancellationToken);

        ViewData["Title"] = "Systemprotokoll";
        ViewData["AktiverLevelFilter"] = level;

        return View(eintraege);
    }
}
