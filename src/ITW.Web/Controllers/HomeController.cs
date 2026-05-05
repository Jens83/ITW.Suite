using System.Diagnostics;
using ITW.Application.Organisation.Contracts;
using ITW.Web.Models;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using ITW.Web.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Controllers;

[AllowAnonymous]
public sealed class HomeController : Controller
{
    private readonly ICurrentUserContextAccessor _currentUserContextAccessor;

    public HomeController(ICurrentUserContextAccessor currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(currentUserContextAccessor);
        _currentUserContextAccessor = currentUserContextAccessor;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        OrganisationsbereichCode? aktuellerBereich = null;
        string? aktuellerBereichName = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var result = await _currentUserContextAccessor.GetCurrentAsync(cancellationToken);

            if (result.IsSuccess && result.CurrentUser is not null)
            {
                aktuellerBereich = result.CurrentUser.Bereich;
                aktuellerBereichName = BereichsRoutingHelper.GetBereichsname(result.CurrentUser.Bereich);
            }
        }

        var viewModel = new HomeIndexViewModel
        {
            IstAngemeldet = User.Identity?.IsAuthenticated == true,
            AktuellerBereichName = aktuellerBereichName,
            Bereiche = ErzeugeBereiche(aktuellerBereich)
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        ViewData["Fehlerpfad"] = exceptionFeature?.Path;

        var viewModel = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        return View(viewModel);
    }

    private static IReadOnlyList<HomeBereichKachelViewModel> ErzeugeBereiche(
        OrganisationsbereichCode? aktuellerBereich)
    {
        return new[]
        {
            new HomeBereichKachelViewModel
            {
                Titel = "Intensivtransport",
                Beschreibung = "Einsatzsteuerung, Patientendokumentation und medizinische Protokolle.",
                Area = "Intensivtransport",
                Controller = "Dashboard",
                Action = "Index",
                IconCssClass = "bi bi-car-front-fill",
                BorderCssClass = "border-primary",
                ButtonCssClass = "btn-outline-primary",
                IstMeinBereich = aktuellerBereich == OrganisationsbereichCode.Intensivtransport,
                ButtonText = aktuellerBereich == OrganisationsbereichCode.Intensivtransport
                    ? "Zu meinem Bereich"
                    : "Intensivtransport"
            },
            new HomeBereichKachelViewModel
            {
                Titel = "Verwaltung",
                Beschreibung = "Personalwesen, Abrechnung und organisatorische Stammdaten.",
                Area = "Verwaltung",
                Controller = "Dashboard",
                Action = "Index",
                IconCssClass = "bi bi-person-lines-fill",
                BorderCssClass = "border-dark",
                ButtonCssClass = "btn-outline-dark",
                IstMeinBereich = aktuellerBereich == OrganisationsbereichCode.Verwaltung,
                ButtonText = aktuellerBereich == OrganisationsbereichCode.Verwaltung
                    ? "Zu meinem Bereich"
                    : "Verwaltung"
            },
            new HomeBereichKachelViewModel
            {
                Titel = "Gesch�ftsf�hrung",
                Beschreibung = "Unternehmensf�hrung, Kennzahlen und strategische Auswertungen.",
                Area = "Geschaeftsfuehrung",
                Controller = "Dashboard",
                Action = "Index",
                IconCssClass = "bi bi-pie-chart",
                BorderCssClass = "border-danger",
                ButtonCssClass = "btn-outline-danger",
                IstMeinBereich = aktuellerBereich == OrganisationsbereichCode.Vorstand,
                ButtonText = aktuellerBereich == OrganisationsbereichCode.Vorstand
                    ? "Zu meinem Bereich"
                    : "Gesch�ftsf�hrung"
            }
        };
    }
}