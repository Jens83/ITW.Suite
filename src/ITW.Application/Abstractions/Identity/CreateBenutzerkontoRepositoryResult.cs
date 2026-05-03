namespace ITW.Application.Abstractions.Identity;

public sealed record CreateBenutzerkontoRepositoryResult(
    bool IsSuccess,
    BenutzerkontoDto? Benutzerkonto,
    string? ErrorMessage)
{
    public static CreateBenutzerkontoRepositoryResult Erfolg(BenutzerkontoDto benutzerkonto)
        => new(true, benutzerkonto, null);

    public static CreateBenutzerkontoRepositoryResult Fehler(string errorMessage)
        => new(false, null, errorMessage);
}