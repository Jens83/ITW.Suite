namespace ITW.Application.Personnel.ProfileQueries;

public sealed record ItwQualifikationOptionDto(
    Guid Id,
    string Code,
    string Bezeichnung,
    int Sortierung);