// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfragen/ReadOffenePasswortResetAnfragenQuery.cs
using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadOffenePasswortResetAnfragen;

public sealed record ReadOffenePasswortResetAnfragenQuery(
    OrganisationsbereichCode Bereich);