namespace ITW.Application.Personnel.ProfileQueries;

public sealed record ReadAllgemeinesMitarbeiterprofilDetailResult(
    bool IsSuccess,
    string? ErrorMessage,
    AllgemeinesMitarbeiterprofilDetailDto? Profil)
{
    public static ReadAllgemeinesMitarbeiterprofilDetailResult Erfolg(AllgemeinesMitarbeiterprofilDetailDto profil)
        => new(true, null, profil);

    public static ReadAllgemeinesMitarbeiterprofilDetailResult Fehler(string errorMessage)
        => new(false, errorMessage, null);
}