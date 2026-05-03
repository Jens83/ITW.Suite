// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfrageDetail/OffenePasswortResetAnfrageDetailDto.cs
using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;

public sealed record OffenePasswortResetAnfrageDetailDto(
    Guid AnfrageId,
    string UserId,
    string Benutzername,
    string Vorname,
    string Nachname,
    OrganisationsbereichCode Bereich,
    DateTimeOffset AngefordertAm);