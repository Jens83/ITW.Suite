using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.AssignArea;

public sealed record AssignUserToPrimaryAreaCommand(
    string UserId,
    OrganisationsbereichCode Bereich,
    BereichsrolleCode Rolle,
    FuehrungsverantwortungCode Fuehrungsverantwortung,
    bool BestehendePrimaereZuordnungErsetzen = false);