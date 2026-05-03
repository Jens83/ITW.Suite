// Datei: src/ITW.Application/Personnel/Documents/DownloadMitarbeiterDokumentResult.cs
namespace ITW.Application.Personnel.Documents;

public sealed record DownloadMitarbeiterDokumentResult(
    bool IsSuccess,
    string? ErrorMessage,
    string DateinameOriginal,
    string Inhaltstyp,
    byte[]? Dateiinhalt)
{
    public static DownloadMitarbeiterDokumentResult Erfolg(
        string dateinameOriginal,
        string inhaltstyp,
        byte[] dateiinhalt) =>
        new(true, null, dateinameOriginal, inhaltstyp, dateiinhalt);

    public static DownloadMitarbeiterDokumentResult Fehler(string errorMessage) =>
        new(false, errorMessage, string.Empty, string.Empty, null);
}