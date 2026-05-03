namespace ITW.Application.Personnel.Profiles;

public sealed record SaveItwMitarbeiterprofilResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static SaveItwMitarbeiterprofilResult Erfolg()
        => new(true, null);

    public static SaveItwMitarbeiterprofilResult Fehler(string errorMessage)
        => new(false, errorMessage);
}