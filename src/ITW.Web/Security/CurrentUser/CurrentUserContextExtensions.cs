using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Security.CurrentUser;

public static class CurrentUserContextExtensions
{
    public static bool HatModul(
        this CurrentUserContext? currentUser,
        ModulCode modul)
    {
        if (currentUser is null || modul == ModulCode.Unbekannt)
        {
            return false;
        }

        return currentUser.AktiveModule.Any(x => x == modul);
    }
}