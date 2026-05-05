using ITW.Application.Organisation.Contracts;
using ITW.Application.Organisation.VisibilityScopes;
using ITW.Application.Users.ActivateUser;
using ITW.Application.Users.AssignArea;
using ITW.Application.Users.ChangeAreaRole;
using ITW.Application.Users.CreateUser;
using ITW.Application.Users.LockUser;
using ITW.Application.Users.ReadNichtZugeordneteBenutzerkonten;
using ITW.Application.Users.ReadUsersByScope;
using ITW.Web.Authorization.Modules;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Geschaeftsfuehrung.Controllers;

[Area("Geschaeftsfuehrung")]
[RequireModule(ModulCode.Personal)]
public sealed class BenutzerController : BereichsBenutzerControllerBase
{
    public BenutzerController(
        ReadUsersByScopeService readUsersByScopeService,
        AssignUserToPrimaryAreaService assignUserToPrimaryAreaService,
        ChangeUserAreaRoleService changeUserAreaRoleService,
        ReadNichtZugeordneteBenutzerkontenService readNichtZugeordneteBenutzerkontenService,
        CreateBenutzerkontoService createBenutzerkontoService,
        LockUserService lockUserService,
        ActivateUserService activateUserService,
        ICurrentUserContextAccessor currentUserContextAccessor,
        BenutzerSichtbarkeitsScopeErmittler benutzerSichtbarkeitsScopeErmittler)
        : base(
            readUsersByScopeService,
            assignUserToPrimaryAreaService,
            changeUserAreaRoleService,
            readNichtZugeordneteBenutzerkontenService,
            createBenutzerkontoService,
            lockUserService,
            activateUserService,
            currentUserContextAccessor,
            benutzerSichtbarkeitsScopeErmittler)
    {
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Vorstand;

    protected override string BereichName => "Geschäftsführung";

    protected override string ListenTitel => "Mitarbeiterverwaltung Geschäftsführung";

    protected override string ListenBeschreibung =>
        "Bereichsbezogene Mitarbeiterliste für den Bereich Geschäftsführung.";

    protected override string AreaLayoutPath =>
        "~/Views/Shared/_AppLayout.cshtml";

    protected override IReadOnlyList<BereichsrolleCode> ErlaubteRollen =>
        new[]
        {
            BereichsrolleCode.Vorstand
        };
}