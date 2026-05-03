namespace ITW.Application.Personnel.ProfileQueries;

public sealed record ReadItwMitarbeiterprofileResult(
    bool IsSuccess,
    string? ErrorMessage,
    IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> Profile)
{
    public static ReadItwMitarbeiterprofileResult Erfolg(IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> profile)
        => new(true, null, profile);

    public static ReadItwMitarbeiterprofileResult Fehler(string errorMessage)
        => new(false, errorMessage, Array.Empty<ItwMitarbeiterprofilUebersichtDto>());
}