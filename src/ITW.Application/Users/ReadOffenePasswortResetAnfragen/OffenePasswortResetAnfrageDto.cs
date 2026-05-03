// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfragen/OffenePasswortResetAnfrageDto.cs
namespace ITW.Application.Users.ReadOffenePasswortResetAnfragen;

public sealed record OffenePasswortResetAnfrageDto(
    Guid AnfrageId,
    string UserId,
    string Benutzername,
    string Vorname,
    string Nachname,
    DateTimeOffset AngefordertAm);