namespace ITW.Web.Security.CurrentUser;

public sealed record CurrentUserContextLookupResult(
    bool IsSuccess,
    string? ErrorMessage,
    CurrentUserContext? CurrentUser)
{
    public static CurrentUserContextLookupResult Erfolg(CurrentUserContext currentUser) =>
        new(true, null, currentUser);

    public static CurrentUserContextLookupResult Fehler(string errorMessage) =>
        new(false, errorMessage, null);
}