// Datei: src/ITW.Application/Personnel/Documents/ReadMitarbeiterDokumenteResult.cs
namespace ITW.Application.Personnel.Documents;

public sealed record ReadMitarbeiterDokumenteResult(
    bool IsSuccess,
    string? ErrorMessage,
    IReadOnlyList<MitarbeiterDokumentListenEintragDto> Dokumente)
{
    public static ReadMitarbeiterDokumenteResult Erfolg(
        IReadOnlyList<MitarbeiterDokumentListenEintragDto> dokumente) =>
        new(true, null, dokumente);

    public static ReadMitarbeiterDokumenteResult Fehler(string errorMessage) =>
        new(false, errorMessage, Array.Empty<MitarbeiterDokumentListenEintragDto>());
}