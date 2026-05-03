// Datei: src/ITW.Application/Users/SetzeTemporaeresPasswort/SetzeTemporaeresPasswortCommand.cs
namespace ITW.Application.Users.SetzeTemporaeresPasswort;

public sealed record SetzeTemporaeresPasswortCommand(
    Guid AnfrageId,
    string BearbeitetVonUserId,
    string TemporaeresPasswort);