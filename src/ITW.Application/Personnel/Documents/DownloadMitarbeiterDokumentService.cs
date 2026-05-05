// Datei: src/ITW.Application/Personnel/Documents/DownloadMitarbeiterDokumentService.cs
using ITW.Application.Abstractions.Persistence;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.Documents;

public sealed class DownloadMitarbeiterDokumentService
{
    private readonly IMitarbeiterDokumentRepository _mitarbeiterDokumentRepository;
    private readonly IMitarbeiterDokumentDateiSpeicher _mitarbeiterDokumentDateiSpeicher;
    private readonly ILogger<DownloadMitarbeiterDokumentService> _logger;

    public DownloadMitarbeiterDokumentService(
        IMitarbeiterDokumentRepository mitarbeiterDokumentRepository,
        IMitarbeiterDokumentDateiSpeicher mitarbeiterDokumentDateiSpeicher,
        ILogger<DownloadMitarbeiterDokumentService> logger)
    {
        ArgumentNullException.ThrowIfNull(mitarbeiterDokumentRepository);
        _mitarbeiterDokumentRepository = mitarbeiterDokumentRepository;
        ArgumentNullException.ThrowIfNull(mitarbeiterDokumentDateiSpeicher);
        _mitarbeiterDokumentDateiSpeicher = mitarbeiterDokumentDateiSpeicher;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<DownloadMitarbeiterDokumentResult> ExecuteAsync(
        DownloadMitarbeiterDokumentQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(DownloadMitarbeiterDokumentService));
            return DownloadMitarbeiterDokumentResult.Fehler("Die UserId ist erforderlich.");
        }

        if (query.DokumentId == Guid.Empty)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: DokumentId leer", nameof(DownloadMitarbeiterDokumentService));
            return DownloadMitarbeiterDokumentResult.Fehler("Die Dokument-ID ist ungültig.");
        }

        var dokument = await _mitarbeiterDokumentRepository.GetByIdAsync(
            query.DokumentId,
            cancellationToken);

        if (dokument is null || !string.Equals(dokument.UserId, query.UserId, StringComparison.Ordinal))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(DownloadMitarbeiterDokumentService), "Dokument nicht gefunden");
            return DownloadMitarbeiterDokumentResult.Fehler("Das Dokument wurde nicht gefunden.");
        }

        var dateiinhalt = await _mitarbeiterDokumentDateiSpeicher.LadeAsync(
            dokument.Speicherpfad,
            cancellationToken);

        if (dateiinhalt is null || dateiinhalt.Length == 0)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(DownloadMitarbeiterDokumentService), "Dokumentdatei konnte nicht geladen werden");
            return DownloadMitarbeiterDokumentResult.Fehler(
                "Die Dokumentdatei konnte nicht geladen werden.");
        }

        return DownloadMitarbeiterDokumentResult.Erfolg(
            dokument.DateinameOriginal,
            dokument.Inhaltstyp,
            dateiinhalt);
    }
}