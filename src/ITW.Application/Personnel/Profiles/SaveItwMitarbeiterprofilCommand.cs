namespace ITW.Application.Personnel.Profiles;

public sealed record SaveItwMitarbeiterprofilCommand(
    string UserId,
    Guid HauptqualifikationId,
    IReadOnlyList<Guid> ZusatzqualifikationIds);