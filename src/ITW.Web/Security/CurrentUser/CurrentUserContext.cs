using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Security.CurrentUser;

public sealed record CurrentUserContext(
    string UserId,
    OrganisationsbereichCode Bereich,
    BereichsrolleCode Rolle,
    FuehrungsverantwortungCode Fuehrungsverantwortung,
    IReadOnlyCollection<ModulCode> AktiveModule);