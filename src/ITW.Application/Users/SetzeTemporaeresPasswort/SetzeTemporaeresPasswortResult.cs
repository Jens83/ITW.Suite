// Datei: src/ITW.Application/Users/SetzeTemporaeresPasswort/SetzeTemporaeresPasswortResult.cs
namespace ITW.Application.Users.SetzeTemporaeresPasswort;

public sealed record SetzeTemporaeresPasswortResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static SetzeTemporaeresPasswortResult Erfolg() =>
        new(true, null);

    public static SetzeTemporaeresPasswortResult Fehler(string errorMessage) =>
        new(false, errorMessage);
}