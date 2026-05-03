using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.ProfileQueries;

public sealed record AllgemeinesMitarbeiterprofilDetailDto(
    string UserId,
    string Benutzername,
    string Email,
    bool IstGesperrt,
    Guid? ProfilId,
    string Vorname,
    string Nachname,
    string DisplayName,
    MitarbeiterBeschaeftigungsart Beschaeftigungsart,
    string Telefonnummer,
    string Strasse,
    string Hausnummer,
    string Postleitzahl,
    string Ort,
    DateTimeOffset? AktualisiertAm);