using ITW.Application.Organisation.Contracts;
using ITW.Web.Controllers.Base;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;

public abstract class IntensivtransportDienstplanControllerBase : BereichsControllerBase
{
    private const string AreaLayoutPath = "~/Views/Shared/_AppLayout.cshtml";

    protected IntensivtransportDienstplanControllerBase(
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    protected IActionResult BereichsView(string viewName, object model)
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
        return View($"~/Areas/Intensivtransport/Views/Dienstplan/{viewName}.cshtml", model);
    }

    protected IActionResult RedirectToDienstplanEinstieg()
        => RedirectToAction("Index", "Dienstplan");

    protected async Task<DienstplanzugriffResult> PruefeDienstplanzugriffAsync(
        bool erfordertWachleiter,
        CancellationToken cancellationToken)
    {
        var result = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        if (!result.IsSuccess || result.CurrentUser is null)
        {
            return DienstplanzugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        var aktuellerBenutzer = result.CurrentUser;

        if (aktuellerBenutzer.Bereich != Bereich)
        {
            return DienstplanzugriffResult.MitErgebnis(
                ErzeugeBereichsWeiterleitung(
                    aktuellerBenutzer.Bereich,
                    "Dashboard",
                    "Index",
                    "Dienstplan",
                    BereichsRoutingHelper.GetBereichsname(Bereich),
                    "Der Dienstplanbereich steht aktuell nur im Bereich Intensivtransport zur Verfügung. Ihr zuständiger Bereich wird jetzt geöffnet."));
        }

        var istErlaubteRolle =
            aktuellerBenutzer.Rolle == BereichsrolleCode.Mitarbeiter ||
            aktuellerBenutzer.Rolle == BereichsrolleCode.Wachleiter;

        if (!istErlaubteRolle)
        {
            return DienstplanzugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        if (erfordertWachleiter && aktuellerBenutzer.Rolle != BereichsrolleCode.Wachleiter)
        {
            return DienstplanzugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        return DienstplanzugriffResult.MitBenutzer(aktuellerBenutzer);
    }

    protected static bool TryBaueDatum(
        int? jahr,
        int? monat,
        int? tag,
        out DateOnly datum)
    {
        datum = default;

        if (!jahr.HasValue || !monat.HasValue || !tag.HasValue)
        {
            return false;
        }

        try
        {
            datum = new DateOnly(jahr.Value, monat.Value, tag.Value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected sealed record DienstplanzugriffResult(CurrentUserContext? CurrentUser, IActionResult? EarlyResult)
    {
        public static DienstplanzugriffResult MitBenutzer(CurrentUserContext currentUser)
            => new(currentUser, null);

        public static DienstplanzugriffResult MitErgebnis(IActionResult earlyResult)
            => new(null, earlyResult);
    }
}