namespace ITW.Application.Users.LockUser;

public sealed record LockUserResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static LockUserResult Erfolg()
        => new(true, null);

    public static LockUserResult Fehler(string errorMessage)
        => new(false, errorMessage);
}