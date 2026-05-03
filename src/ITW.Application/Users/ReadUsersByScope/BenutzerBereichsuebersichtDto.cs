using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadUsersByScope;

public sealed record BenutzerBereichsuebersichtDto(
    Guid BereichszuordnungId,
    string UserId,
    string Benutzername,
    string Email,
    OrganisationsbereichCode Bereich,
    BereichsrolleCode Rolle,
    FuehrungsverantwortungCode Fuehrungsverantwortung,
    bool IsPrimary,
    bool IsActive,
    bool IstGesperrt,
    DateTimeOffset ZugewiesenAm);