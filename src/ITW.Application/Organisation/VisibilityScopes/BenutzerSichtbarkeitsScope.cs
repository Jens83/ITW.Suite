using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Organisation.VisibilityScopes;

public sealed record BenutzerSichtbarkeitsScope(
    bool DarfBenutzerlistenLesen,
    OrganisationsbereichCode? Zielbereich,
    string? ErrorMessage)
{
    public static BenutzerSichtbarkeitsScope Erlaubt(OrganisationsbereichCode zielbereich) =>
        new(true, zielbereich, null);

    public static BenutzerSichtbarkeitsScope Verweigert(string errorMessage) =>
        new(false, null, errorMessage);
}