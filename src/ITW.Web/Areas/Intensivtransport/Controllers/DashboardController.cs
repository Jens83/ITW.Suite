using ITW.Application.Aufgaben;
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
    private readonly SystemAufgabeGeneratorService _aufgabeGenerator;
    private readonly AufgabeErledigenService _aufgabeErledigen;
    private readonly AufgabeErstellenService _aufgabeErstellen;
    private readonly AufgabeAufschiebenService _aufgabeAufschieben;
    private readonly AufgabeLoeschenService _aufgabeLoeschen;

    public DashboardController(
        ICurrentUserContextAccessor currentUserContextAccessor,
        GetItwDashboardDataService dashboardDataService,
        SystemAufgabeGeneratorService aufgabeGenerator,
        AufgabeErledigenService aufgabeErledigen,
        AufgabeErstellenService aufgabeErstellen,
        AufgabeAufschiebenService aufgabeAufschieben,
        AufgabeLoeschenService aufgabeLoeschen)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(dashboardDataService);
        ArgumentNullException.ThrowIfNull(aufgabeGenerator);
        ArgumentNullException.ThrowIfNull(aufgabeErledigen);
        ArgumentNullException.ThrowIfNull(aufgabeErstellen);
        ArgumentNullException.ThrowIfNull(aufgabeAufschieben);
        ArgumentNullException.ThrowIfNull(aufgabeLoeschen);

        _dashboardDataService = dashboardDataService;
        _aufgabeGenerator     = aufgabeGenerator;
        _aufgabeErledigen     = aufgabeErledigen;
        _aufgabeErstellen     = aufgabeErstellen;
        _aufgabeAufschieben   = aufgabeAufschieben;
        _aufgabeLoeschen      = aufgabeLoeschen;
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
        {
            await _aufgabeGenerator.AktualisierenAsync(cancellationToken);
            dashboardData = await _dashboardDataService.ExecuteAsync(cancellationToken);
        }

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
            AktuelleWunschphase         = dashboardData?.AktuelleWunschphase,
            OffeneAufgaben              = dashboardData?.OffeneAufgaben
                                          ?? Array.Empty<ItwAufgabeViewModel>(),
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AufgabeErledigen(
        Guid aufgabeId,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        await _aufgabeErledigen.ExecuteAsync(
            aufgabeId,
            OrganisationsbereichCode.Intensivtransport,
            cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AufgabeErstellen(
        string titel,
        AufgabePrioritaet prioritaet,
        string? faelligkeitsdatum,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        DateOnly? faelligkeit = null;
        if (!string.IsNullOrWhiteSpace(faelligkeitsdatum) &&
            DateOnly.TryParseExact(faelligkeitsdatum, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var parsed))
        {
            faelligkeit = parsed;
        }

        await _aufgabeErstellen.ExecuteAsync(
            new AufgabeErstellenCommand(titel, prioritaet, faelligkeit),
            OrganisationsbereichCode.Intensivtransport,
            cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AufgabeAufschieben(
        Guid aufgabeId,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        await _aufgabeAufschieben.ExecuteAsync(
            aufgabeId,
            OrganisationsbereichCode.Intensivtransport,
            cancellationToken);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AufgabeLoeschen(
        Guid aufgabeId,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        await _aufgabeLoeschen.ExecuteAsync(
            aufgabeId,
            OrganisationsbereichCode.Intensivtransport,
            cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}
