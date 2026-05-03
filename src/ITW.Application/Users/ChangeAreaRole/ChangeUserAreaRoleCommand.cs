using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ChangeAreaRole;

public sealed record ChangeUserAreaRoleCommand(
    string UserId,
    BereichsrolleCode NeueRolle,
    FuehrungsverantwortungCode NeueFuehrungsverantwortung);