using ITW.Application.Abstractions.Identity;

namespace ITW.Application.Users.CreateUser;

public sealed record CreateBenutzerkontoResult(
    bool IsSuccess,
    BenutzerkontoDto? Benutzerkonto,
    string? ErrorMessage)
{
    public static CreateBenutzerkontoResult Erfolg(BenutzerkontoDto benutzerkonto)
        => new(true, benutzerkonto, null);

    public static CreateBenutzerkontoResult Fehler(string errorMessage)
        => new(false, null, errorMessage);
}