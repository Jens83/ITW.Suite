using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadUserOrganisationskontext;

public sealed record BenutzerOrganisationskontextDto(
    string UserId,
    OrganisationsbereichCode Bereich,
    BereichsrolleCode Rolle,
    FuehrungsverantwortungCode Fuehrungsverantwortung);