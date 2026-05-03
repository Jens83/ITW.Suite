// Datei: src/ITW.Application/Users/RequestPasswordReset/SubmitPasswortResetAnfrageResult.cs
namespace ITW.Application.Users.RequestPasswordReset;

public sealed record SubmitPasswortResetAnfrageResult(
    bool IsSuccess,
    string? ErrorMessage,
    string Bestaetigungsnachricht)
{
    public static SubmitPasswortResetAnfrageResult Erfolg(string bestaetigungsnachricht) =>
        new(true, null, bestaetigungsnachricht);

    public static SubmitPasswortResetAnfrageResult Fehler(string errorMessage) =>
        new(false, errorMessage, string.Empty);
}