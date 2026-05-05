namespace ITW.Web.Logging;

public interface ILogEintragService
{
    Task<IReadOnlyList<LogEintrag>> GetRecentAsync(
        int maxEintraege = 200,
        string? levelFilter = null,
        CancellationToken cancellationToken = default);
}
