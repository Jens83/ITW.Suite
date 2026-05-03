namespace ITW.Web.Security.CurrentUser;

public interface ICurrentUserContextAccessor
{
    Task<CurrentUserContextLookupResult> GetCurrentAsync(
        CancellationToken cancellationToken = default);
}