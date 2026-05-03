using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.Profiles;

public sealed record SaveAllgemeinesMitarbeiterprofilCommand(
    string UserId,
    string Vorname,
    string Nachname,
    MitarbeiterBeschaeftigungsart Beschaeftigungsart,
    string? Telefonnummer,
    string? Strasse,
    string? Hausnummer,
    string? Postleitzahl,
    string? Ort);