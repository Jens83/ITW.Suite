namespace ITW.Application.Users.ReadUserOrganisationskontext;

public sealed record ReadUserOrganisationskontextResult(
    bool IsSuccess,
    string? ErrorMessage,
    BenutzerOrganisationskontextDto? Benutzer)
{
    public static ReadUserOrganisationskontextResult Erfolg(BenutzerOrganisationskontextDto benutzer) =>
        new(true, null, benutzer);

    public static ReadUserOrganisationskontextResult Fehler(string errorMessage) =>
        new(false, errorMessage, null);
}