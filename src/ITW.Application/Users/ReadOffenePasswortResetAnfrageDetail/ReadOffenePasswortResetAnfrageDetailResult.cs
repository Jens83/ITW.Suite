// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfrageDetail/ReadOffenePasswortResetAnfrageDetailResult.cs
namespace ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;

public sealed record ReadOffenePasswortResetAnfrageDetailResult(
    bool IsSuccess,
    string? ErrorMessage,
    OffenePasswortResetAnfrageDetailDto? Anfrage)
{
    public static ReadOffenePasswortResetAnfrageDetailResult Erfolg(
        OffenePasswortResetAnfrageDetailDto anfrage) =>
        new(true, null, anfrage);

    public static ReadOffenePasswortResetAnfrageDetailResult Fehler(
        string errorMessage) =>
        new(false, errorMessage, null);
}