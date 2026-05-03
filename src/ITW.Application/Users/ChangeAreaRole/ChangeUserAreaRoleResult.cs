namespace ITW.Application.Users.ChangeAreaRole;

public sealed record ChangeUserAreaRoleResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static ChangeUserAreaRoleResult Erfolg() =>
        new(true, null);

    public static ChangeUserAreaRoleResult Fehler(string errorMessage) =>
        new(false, errorMessage);
}