// Datei: src/ITW.Web/Areas/Verwaltung/Controllers/PasswortResetController.cs
using ITW.Application.Organisation.Contracts;
using ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;
using ITW.Application.Users.ReadOffenePasswortResetAnfragen;
using ITW.Application.Users.SetzeTemporaeresPasswort;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Verwaltung.Controllers;

[Area("Verwaltung")]
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

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Verwaltung;

    protected override string BereichName => "Verwaltung";

    protected override string ListenTitel => "Passwort-Reset-Anfragen";

    protected override string ListenBeschreibung =>
        "Hier sehen zuständige Leitungen alle offenen Passwort-Reset-Anfragen aus dem Bereich Verwaltung.";

    protected override string AreaLayoutPath =>
        "~/Areas/Verwaltung/Views/Shared/_LayoutVerwaltung.cshtml";
}