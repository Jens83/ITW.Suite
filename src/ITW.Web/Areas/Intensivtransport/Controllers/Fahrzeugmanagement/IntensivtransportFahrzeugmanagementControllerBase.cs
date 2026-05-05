using ITW.Application.Organisation.Contracts;
using ITW.Fahrzeugmanagement.Application.Fahrzeuge;
using ITW.Fahrzeugmanagement.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;
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

    protected static FahrzeugDetailNavigationViewModel BaueNavigation(
        FahrzeugDetail detail,
        string aktiveSeite)
    {
        return new FahrzeugDetailNavigationViewModel
        {
            FahrzeugId = detail.FahrzeugId,
            Kennzeichen = detail.Kennzeichen,
            InterneNummer = detail.InterneNummer,
            Fahrzeugname = $"{detail.Hersteller} {detail.Modell}".Trim(),
            StatusText = ErmittleStatusText(detail.Status),
            AktiveSeite = aktiveSeite
        };
    }

    protected static string ErmittleStatusText(FahrzeugStatus status)
    {
        return status switch
        {
            FahrzeugStatus.Aktiv => "Aktiv",
            FahrzeugStatus.InWartung => "In Wartung",
            FahrzeugStatus.AusserBetrieb => "Außer Betrieb",
            FahrzeugStatus.Reserviert => "Reserviert",
            FahrzeugStatus.Archiviert => "Archiviert",
            _ => "Unbekannt"
        };
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