// Datei: src/ITW.Application/Users/CountOffenePasswortResetAnfragen/CountOffenePasswortResetAnfragenQuery.cs
using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.CountOffenePasswortResetAnfragen;

public sealed record CountOffenePasswortResetAnfragenQuery(
    OrganisationsbereichCode Bereich);