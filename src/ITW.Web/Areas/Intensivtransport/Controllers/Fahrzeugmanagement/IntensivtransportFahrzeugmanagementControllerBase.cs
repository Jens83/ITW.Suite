using ITW.Application.Organisation.Contracts;
using ITW.Web.Controllers.Base;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Fahrzeugmanagement;

public abstract class IntensivtransportFahrzeugmanagementControllerBase : BereichsControllerBase
{
    protected const string AreaLayoutPath =
        "~/Views/Shared/_AppLayout.cshtml";

    protected IntensivtransportFahrzeugmanagementControllerBase(
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    protected void SetzeBereichslayout()
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
    }

    protected async Task<FahrzeugmanagementZugriffResult> PruefeFahrzeugmanagementzugriffAsync(
        CancellationToken cancellationToken)
    {
        var result = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        if (!result.IsSuccess || result.CurrentUser is null)
        {
            return FahrzeugmanagementZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        var aktuellerBenutzer = result.CurrentUser;

        if (aktuellerBenutzer.Bereich != Bereich)
        {
            return FahrzeugmanagementZugriffResult.MitErgebnis(
                ErzeugeBereichsWeiterleitung(
                    aktuellerBenutzer.Bereich,
                    "Dashboard",
                    "Index",
                    "Fahrzeugmanagement",
                    BereichsRoutingHelper.GetBereichsname(Bereich),
                    "Das Fahrzeugmanagement steht aktuell nur im Bereich Intensivtransport zur Verfügung. Ihr zuständiger Bereich wird jetzt geöffnet."));
        }

        if (aktuellerBenutzer.Rolle != BereichsrolleCode.Wachleiter)
        {
            return FahrzeugmanagementZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        return FahrzeugmanagementZugriffResult.MitBenutzer(aktuellerBenutzer);
    }

    protected sealed record FahrzeugmanagementZugriffResult(
        CurrentUserContext? CurrentUser,
        IActionResult? EarlyResult)
    {
        public static FahrzeugmanagementZugriffResult MitBenutzer(CurrentUserContext currentUser)
            => new(currentUser, null);

        public static FahrzeugmanagementZugriffResult MitErgebnis(IActionResult earlyResult)
            => new(null, earlyResult);
    }
}