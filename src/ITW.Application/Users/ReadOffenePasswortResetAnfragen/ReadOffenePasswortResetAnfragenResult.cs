// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfragen/ReadOffenePasswortResetAnfragenResult.cs
namespace ITW.Application.Users.ReadOffenePasswortResetAnfragen;

public sealed record ReadOffenePasswortResetAnfragenResult(
    bool IsSuccess,
    string? ErrorMessage,
    IReadOnlyList<OffenePasswortResetAnfrageDto> Anfragen)
{
    public static ReadOffenePasswortResetAnfragenResult Erfolg(
        IReadOnlyList<OffenePasswortResetAnfrageDto> anfragen) =>
        new(true, null, anfragen);

    public static ReadOffenePasswortResetAnfragenResult Fehler(string errorMessage) =>
        new(false, errorMessage, Array.Empty<OffenePasswortResetAnfrageDto>());
}