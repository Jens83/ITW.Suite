namespace ITW.Application.Abstractions.Identity;

public sealed record UpdateBenutzerkontoStatusRepositoryResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static UpdateBenutzerkontoStatusRepositoryResult Erfolg()
        => new(true, null);

    public static UpdateBenutzerkontoStatusRepositoryResult Fehler(string errorMessage)
        => new(false, errorMessage);
}