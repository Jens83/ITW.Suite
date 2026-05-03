namespace ITW.Application.Personnel.ProfileQueries;

public sealed record ItwMitarbeiterprofilDetailDto(
    string UserId,
    string Benutzername,
    string Email,
    bool IstGesperrt,
    Guid? ProfilId,
    Guid? HauptqualifikationId,
    IReadOnlyList<Guid> ZusatzqualifikationIds,
    DateTimeOffset? AktualisiertAm);