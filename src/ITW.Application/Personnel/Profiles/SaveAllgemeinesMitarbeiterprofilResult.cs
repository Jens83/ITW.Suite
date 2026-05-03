namespace ITW.Application.Personnel.Profiles;

public sealed record SaveAllgemeinesMitarbeiterprofilResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static SaveAllgemeinesMitarbeiterprofilResult Erfolg()
        => new(true, null);

    public static SaveAllgemeinesMitarbeiterprofilResult Fehler(string errorMessage)
        => new(false, errorMessage);
}