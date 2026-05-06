using ITW.Application.Organisation.Contracts;
using ITW.Web.Areas.Intensivtransport.Services.Dashboard;
using ITW.Web.Areas.Intensivtransport.ViewModels;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers;

[Area("Intensivtransport")]
public sealed class DashboardController : BereichsDashboardControllerBase
{
    private readonly GetItwDashboardDataService _dashboardDataService;

    public DashboardController(
        ICurrentUserContextAccessor currentUserContextAccessor,
        GetItwDashboardDataService dashboardDataService)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(dashboardDataService);
        _dashboardDataService = dashboardDataService;
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var userContext = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        var currentUser = userContext.CurrentUser!;

        var istWachleiter  = currentUser.Rolle == BereichsrolleCode.Wachleiter;
        var istMitarbeiter = currentUser.Rolle == BereichsrolleCode.Mitarbeiter;

        var hatDienstplan           = currentUser.HatModul(ModulCode.Dienstplan);
        var hatEinsatzverwaltung    = currentUser.HatModul(ModulCode.Einsatzverwaltung);
        var hatPersonal             = currentUser.HatModul(ModulCode.Personal);
        var hatFahrzeugmanagement   = currentUser.HatModul(ModulCode.Fahrzeugmanagement);
        var darfPersonaldatenSehen  = istWachleiter && hatPersonal;
        var darfFahrzeugmgmtSehen   = istWachleiter && hatFahrzeugmanagement;

        ItwDashboardDataResult? dashboardData = null;
        if (istWachleiter)
            dashboardData = await _dashboardDataService.ExecuteAsync(cancellationToken);

        var viewModel = new ItwDashboardViewModel
        {
            IstWachleiter               = istWachleiter,
            IstMitarbeiter              = istMitarbeiter,
            HatDienstplan               = hatDienstplan,
            HatEinsatzverwaltung        = hatEinsatzverwaltung,
            DarfPersonaldatenSehen      = darfPersonaldatenSehen,
            DarfFahrzeugmanagementSehen = darfFahrzeugmgmtSehen,
            HatMindestensEinModul       = hatDienstplan || hatEinsatzverwaltung
                                          || darfPersonaldatenSehen || darfFahrzeugmgmtSehen,
            LetzteAktivitaeten          = dashboardData?.LetzteAktivitaeten
                                          ?? Array.Empty<ItwAktivitaetViewModel>(),
            AktuelleWunschphase         = dashboardData?.AktuelleWunschphase
        };

        return View(viewModel);
    }
}
