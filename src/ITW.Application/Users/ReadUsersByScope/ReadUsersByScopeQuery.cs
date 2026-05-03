using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadUsersByScope;

public sealed record ReadUsersByScopeQuery(
    OrganisationsbereichCode AufrufenderBereich,
    BereichsrolleCode AufrufendeRolle);