using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.ProfileQueries;

public sealed record ItwMitarbeiterprofilUebersichtDto(
    string UserId,
    string AnzeigeName,
    string Benutzername,
    string Email,
    bool IstGesperrt,
    bool HatStammdatenprofil,
    DateTimeOffset? StammdatenAktualisiertAm,
    MitarbeiterBeschaeftigungsart Beschaeftigungsart,
    bool HatProfil,
    string Hauptqualifikation,
    IReadOnlyList<string> Zusatzqualifikationen,
    DateTimeOffset? ProfilAktualisiertAm);