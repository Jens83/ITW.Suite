namespace ITW.Application.Users.ActivateUser;

public sealed record ActivateUserResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static ActivateUserResult Erfolg()
        => new(true, null);

    public static ActivateUserResult Fehler(string errorMessage)
        => new(false, errorMessage);
}