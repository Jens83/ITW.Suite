using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadUsersByScope;

public sealed record ReadUsersByScopeResult(
    bool IsSuccess,
    string? ErrorMessage,
    OrganisationsbereichCode? SichtbarerBereich,
    IReadOnlyList<BenutzerBereichsuebersichtDto> Benutzer)
{
    public static ReadUsersByScopeResult Erfolg(
        OrganisationsbereichCode sichtbarerBereich,
        IReadOnlyList<BenutzerBereichsuebersichtDto> benutzer) =>
        new(true, null, sichtbarerBereich, benutzer);

    public static ReadUsersByScopeResult Fehler(string errorMessage) =>
        new(false, errorMessage, null, Array.Empty<BenutzerBereichsuebersichtDto>());
}