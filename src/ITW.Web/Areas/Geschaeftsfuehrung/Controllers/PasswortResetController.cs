// Datei: src/ITW.Web/Areas/Geschaeftsfuehrung/Controllers/PasswortResetController.cs
using ITW.Application.Organisation.Contracts;
using ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;
using ITW.Application.Users.ReadOffenePasswortResetAnfragen;
using ITW.Application.Users.SetzeTemporaeresPasswort;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Geschaeftsfuehrung.Controllers;

[Area("Geschaeftsfuehrung")]
public sealed class PasswortResetController : BereichsPasswortResetControllerBase
{
    public PasswortResetController(
        ReadOffenePasswortResetAnfragenService readOffenePasswortResetAnfragenService,
        ReadOffenePasswortResetAnfrageDetailService readOffenePasswortResetAnfrageDetailService,
        SetzeTemporaeresPasswortService setzeTemporaeresPasswortService,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(
            readOffenePasswortResetAnfragenService,
            readOffenePasswortResetAnfrageDetailService,
            setzeTemporaeresPasswortService,
            currentUserContextAccessor)
    {
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Vorstand;

    protected override string BereichName => "Geschäftsführung";

    protected override string ListenTitel => "Passwort-Reset-Anfragen";

    protected override string ListenBeschreibung =>
        "Hier sieht die zuständige Leitung alle offenen Passwort-Reset-Anfragen aus dem Bereich Geschäftsführung.";

    protected override string AreaLayoutPath =>
        "~/Areas/Geschaeftsfuehrung/Views/Shared/_LayoutGeschaeftsfuehrung.cshtml";
}