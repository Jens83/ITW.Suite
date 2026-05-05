using ITW.Application.Organisation.Contracts;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using ITW.Web.ViewModels.Navigation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Controllers.Base;

[Authorize]
public abstract class BereichsControllerBase : Controller
{
    private readonly ICurrentUserContextAccessor _currentUserContextAccessor;

    protected BereichsControllerBase(
        ICurrentUserContextAccessor currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(currentUserContextAccessor);
        _currentUserContextAccessor = currentUserContextAccessor;
    }

    protected abstract OrganisationsbereichCode Bereich { get; }

    protected Task<CurrentUserContextLookupResult> HoleAktuellenBenutzerkontextAsync(
        CancellationToken cancellationToken)
    {
        return _currentUserContextAccessor.GetCurrentAsync(cancellationToken);
    }

    protected IActionResult RedirectToKeinZugriff()
    {
        return RedirectToAction("KeinZugriff", "Account", new { area = "" });
    }

    protected IActionResult ErzeugeBereichsWeiterleitung(
        OrganisationsbereichCode zielbereich,
        string controllerName,
        string actionName,
        string? funktionsName = null,
        string? aufgerufenerBereichName = null,
        string? hinweistext = null)
    {
        var zielbereichName = BereichsRoutingHelper.GetBereichsname(zielbereich);
        var zielAreaName = BereichsRoutingHelper.GetAreaName(zielbereich);

        if (string.IsNullOrWhiteSpace(zielAreaName))
        {
            return RedirectToKeinZugriff();
        }

        var zielUrl = Url.Action(
            actionName,
            controllerName,
            new { area = zielAreaName });

        if (string.IsNullOrWhiteSpace(zielUrl))
        {
            return RedirectToKeinZugriff();
        }

        var effektiverAufgerufenerBereich = string.IsNullOrWhiteSpace(aufgerufenerBereichName)
            ? BereichsRoutingHelper.GetBereichsname(Bereich)
            : aufgerufenerBereichName;

        var effektiverHinweistext = string.IsNullOrWhiteSpace(hinweistext)
            ? "Ihre aktuelle Zuordnung passt nicht zum aufgerufenen Modul. Die passende Umgebung wird jetzt automatisch geöffnet."
            : hinweistext;

        var viewModel = new BereichsWeiterleitungViewModel
        {
            Titel = string.IsNullOrWhiteSpace(funktionsName)
                ? "Weiterleitung in den zuständigen Bereich"
                : $"{funktionsName} wird weitergeleitet",
            FunktionsName = funktionsName ?? string.Empty,
            AufgerufenerBereichName = effektiverAufgerufenerBereich,
            ZielbereichName = zielbereichName,
            Hinweistext = effektiverHinweistext,
            ZielUrl = zielUrl,
            ButtonText = $"Zu {zielbereichName}",
            VerzogerungInMillisekunden = 1400
        };

        return View(
            "~/Views/Shared/Navigation/BereichsWeiterleitung.cshtml",
            viewModel);
    }
}