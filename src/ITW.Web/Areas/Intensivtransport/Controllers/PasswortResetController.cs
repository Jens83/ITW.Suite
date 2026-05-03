// Datei: src/ITW.Web/Areas/Intensivtransport/Controllers/PasswortResetController.cs
using ITW.Application.Organisation.Contracts;
using ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;
using ITW.Application.Users.ReadOffenePasswortResetAnfragen;
using ITW.Application.Users.SetzeTemporaeresPasswort;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers;

[Area("Intensivtransport")]
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

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    protected override string BereichName => "Intensivtransport";

    protected override string ListenTitel => "Passwort-Reset-Anfragen";

    protected override string ListenBeschreibung =>
        "Hier sehen zuständige Leitungen alle offenen Passwort-Reset-Anfragen aus dem Bereich Intensivtransport.";

    protected override string AreaLayoutPath =>
        "~/Areas/Intensivtransport/Views/Shared/_LayoutIntensivtransport.cshtml";
}