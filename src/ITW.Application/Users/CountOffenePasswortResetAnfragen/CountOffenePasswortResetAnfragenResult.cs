// Datei: src/ITW.Application/Users/CountOffenePasswortResetAnfragen/CountOffenePasswortResetAnfragenResult.cs
namespace ITW.Application.Users.CountOffenePasswortResetAnfragen;

public sealed record CountOffenePasswortResetAnfragenResult(
    bool IsSuccess,
    string? ErrorMessage,
    int Anzahl)
{
    public static CountOffenePasswortResetAnfragenResult Erfolg(int anzahl) =>
        new(true, null, anzahl);

    public static CountOffenePasswortResetAnfragenResult Fehler(string errorMessage) =>
        new(false, errorMessage, 0);
}