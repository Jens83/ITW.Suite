// Datei: src/ITW.Application/Personnel/Documents/UploadMitarbeiterDokumentResult.cs
namespace ITW.Application.Personnel.Documents;

public sealed record UploadMitarbeiterDokumentResult(
    bool IsSuccess,
    string? ErrorMessage)
{
    public static UploadMitarbeiterDokumentResult Erfolg() =>
        new(true, null);

    public static UploadMitarbeiterDokumentResult Fehler(string errorMessage) =>
        new(false, errorMessage);
}