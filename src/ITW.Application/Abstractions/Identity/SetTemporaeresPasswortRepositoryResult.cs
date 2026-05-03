// Datei: src/ITW.Application/Abstractions/Identity/SetTemporaeresPasswortRepositoryResult.cs
namespace ITW.Application.Abstractions.Identity;

public sealed record SetTemporaeresPasswortRepositoryResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static SetTemporaeresPasswortRepositoryResult Erfolg() =>
        new(true, null);

    public static SetTemporaeresPasswortRepositoryResult Fehler(string errorMessage) =>
        new(false, errorMessage);
}