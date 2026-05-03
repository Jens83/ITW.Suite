// Datei: src/ITW.Application/Personnel/Documents/DownloadMitarbeiterDokumentService.cs
using ITW.Application.Abstractions.Persistence;

namespace ITW.Application.Personnel.Documents;

public sealed class DownloadMitarbeiterDokumentService
{
    private readonly IMitarbeiterDokumentRepository _mitarbeiterDokumentRepository;
    private readonly IMitarbeiterDokumentDateiSpeicher _mitarbeiterDokumentDateiSpeicher;

    public DownloadMitarbeiterDokumentService(
        IMitarbeiterDokumentRepository mitarbeiterDokumentRepository,
        IMitarbeiterDokumentDateiSpeicher mitarbeiterDokumentDateiSpeicher)
    {
        _mitarbeiterDokumentRepository = mitarbeiterDokumentRepository
            ?? throw new ArgumentNullException(nameof(mitarbeiterDokumentRepository));
        _mitarbeiterDokumentDateiSpeicher = mitarbeiterDokumentDateiSpeicher
            ?? throw new ArgumentNullException(nameof(mitarbeiterDokumentDateiSpeicher));
    }

    public async Task<DownloadMitarbeiterDokumentResult> ExecuteAsync(
        DownloadMitarbeiterDokumentQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.UserId))
        {
            return DownloadMitarbeiterDokumentResult.Fehler("Die UserId ist erforderlich.");
        }

        if (query.DokumentId == Guid.Empty)
        {
            return DownloadMitarbeiterDokumentResult.Fehler("Die Dokument-ID ist ungültig.");
        }

        var dokument = await _mitarbeiterDokumentRepository.GetByIdAsync(
            query.DokumentId,
            cancellationToken);

        if (dokument is null || !string.Equals(dokument.UserId, query.UserId, StringComparison.Ordinal))
        {
            return DownloadMitarbeiterDokumentResult.Fehler("Das Dokument wurde nicht gefunden.");
        }

        var dateiinhalt = await _mitarbeiterDokumentDateiSpeicher.LadeAsync(
            dokument.Speicherpfad,
            cancellationToken);

        if (dateiinhalt is null || dateiinhalt.Length == 0)
        {
            return DownloadMitarbeiterDokumentResult.Fehler(
                "Die Dokumentdatei konnte nicht geladen werden.");
        }

        return DownloadMitarbeiterDokumentResult.Erfolg(
            dokument.DateinameOriginal,
            dokument.Inhaltstyp,
            dateiinhalt);
    }
}