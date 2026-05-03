// Datei: src/ITW.Web/Security/PasswordReset/PasswortResetVerantwortungHelper.cs
using ITW.Application.Organisation.Contracts;
using ITW.Web.Security.CurrentUser;

namespace ITW.Web.Security.PasswordReset;

public static class PasswortResetVerantwortungHelper
{
    public static bool DarfAnfragenBearbeiten(
        CurrentUserContext? currentUser,
        OrganisationsbereichCode bereich)
    {
        if (currentUser is null)
        {
            return false;
        }

        if (currentUser.Bereich != bereich)
        {
            return false;
        }

        if (currentUser.Rolle is BereichsrolleCode.Wachleiter or
            BereichsrolleCode.Vorstandsverwaltung or
            BereichsrolleCode.Vorstand)
        {
            return true;
        }

        return currentUser.Fuehrungsverantwortung is
            FuehrungsverantwortungCode.OperativeLeitung or
            FuehrungsverantwortungCode.Bereichsleitung or
            FuehrungsverantwortungCode.UebergeordneteLeitung;
    }
}