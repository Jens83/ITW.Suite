// Datei: src/ITW.Application/Users/RequestPasswordReset/SubmitPasswortResetAnfrageCommand.cs
namespace ITW.Application.Users.RequestPasswordReset;

public sealed record SubmitPasswortResetAnfrageCommand(
    string Benutzername,
    string Vorname,
    string Nachname);