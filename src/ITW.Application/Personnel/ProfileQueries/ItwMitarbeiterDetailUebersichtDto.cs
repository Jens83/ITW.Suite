using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.ProfileQueries;

public sealed record ItwMitarbeiterDetailUebersichtDto(
    string UserId,
    string Benutzername,
    string Email,
    bool IstGesperrt,
    string Rolle,
    string Fuehrungsverantwortung,
    bool HatItwProfil,
    string Hauptqualifikation,
    IReadOnlyList<string> Zusatzqualifikationen,
    DateTimeOffset? ProfilAktualisiertAm,
    bool HatStammdatenprofil,
    string AnzeigeName,
    string Vorname,
    string Nachname,
    MitarbeiterBeschaeftigungsart Beschaeftigungsart,
    string Telefonnummer,
    string AnschriftKurz,
    DateTimeOffset? StammdatenAktualisiertAm,
    DateTimeOffset ZugewiesenAm);