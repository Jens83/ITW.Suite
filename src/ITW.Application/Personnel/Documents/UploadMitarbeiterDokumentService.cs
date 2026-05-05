// Datei: src/ITW.Application/Personnel/Documents/UploadMitarbeiterDokumentService.cs
using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Entities;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.Documents;

public sealed class UploadMitarbeiterDokumentService
{
    private const int MaxDateigroesseBytes = 10 * 1024 * 1024;

    private static readonly IReadOnlyDictionary<string, string> ErlaubteDateitypen =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".doc"] = "application/msword",
            [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IMitarbeiterDokumentRepository _mitarbeiterDokumentRepository;
    private readonly IMitarbeiterDokumentDateiSpeicher _mitarbeiterDokumentDateiSpeicher;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<UploadMitarbeiterDokumentService> _logger;

    public UploadMitarbeiterDokumentService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IMitarbeiterDokumentRepository mitarbeiterDokumentRepository,
        IMitarbeiterDokumentDateiSpeicher mitarbeiterDokumentDateiSpeicher,
        IDateTimeProvider dateTimeProvider,
        ILogger<UploadMitarbeiterDokumentService> logger)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _mitarbeiterDokumentRepository = mitarbeiterDokumentRepository
            ?? throw new ArgumentNullException(nameof(mitarbeiterDokumentRepository));
        _mitarbeiterDokumentDateiSpeicher = mitarbeiterDokumentDateiSpeicher
            ?? throw new ArgumentNullException(nameof(mitarbeiterDokumentDateiSpeicher));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UploadMitarbeiterDokumentResult> ExecuteAsync(
        UploadMitarbeiterDokumentCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(UploadMitarbeiterDokumentService));

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(UploadMitarbeiterDokumentService));
            return UploadMitarbeiterDokumentResult.Fehler("Die UserId ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.HochgeladenVonUserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: HochgeladenVonUserId leer", nameof(UploadMitarbeiterDokumentService));
            return UploadMitarbeiterDokumentResult.Fehler("Die Bearbeiter-UserId ist erforderlich.");
        }

        if (!MitarbeiterDokumentKategorien.IstErlaubt(command.Kategorie))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Kategorie ungültig", nameof(UploadMitarbeiterDokumentService));
            return UploadMitarbeiterDokumentResult.Fehler("Bitte eine gültige Dokumentkategorie auswählen.");
        }

        if (string.IsNullOrWhiteSpace(command.DateinameOriginal))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Dateiname leer", nameof(UploadMitarbeiterDokumentService));
            return UploadMitarbeiterDokumentResult.Fehler("Der Dateiname ist erforderlich.");
        }

        if (command.Dateiinhalt is null || command.Dateiinhalt.Length == 0)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Dateiinhalt leer", nameof(UploadMitarbeiterDokumentService));
            return UploadMitarbeiterDokumentResult.Fehler("Es wurde keine Datei übergeben.");
        }

        if (command.Dateiinhalt.Length > MaxDateigroesseBytes)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(UploadMitarbeiterDokumentService), "Datei zu groß");
            return UploadMitarbeiterDokumentResult.Fehler("Die Datei ist zu groß. Erlaubt sind maximal 10 MB.");
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            command.UserId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(UploadMitarbeiterDokumentService), "Keine aktive ITW-Zuordnung");
            return UploadMitarbeiterDokumentResult.Fehler(
                "Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        var dateinameOriginal = Path.GetFileName(command.DateinameOriginal.Trim());
        var dateiendung = Path.GetExtension(dateinameOriginal)?.Trim() ?? string.Empty;

        if (!ErlaubteDateitypen.TryGetValue(dateiendung, out var inhaltstyp))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(UploadMitarbeiterDokumentService), "Ungültiger Dateityp");
            return UploadMitarbeiterDokumentResult.Fehler(
                "Es sind nur PDF-, JPG-, JPEG-, PNG-, DOC- und DOCX-Dateien erlaubt.");
        }

        string? speicherpfad = null;

        try
        {
            speicherpfad = await _mitarbeiterDokumentDateiSpeicher.SpeichereAsync(
                command.UserId,
                dateinameOriginal,
                command.Dateiinhalt,
                cancellationToken);

            var dokument = new MitarbeiterDokument(
                Guid.NewGuid(),
                command.UserId,
                command.Kategorie.Trim(),
                dateinameOriginal,
                speicherpfad,
                inhaltstyp,
                command.Dateiinhalt.Length,
                _dateTimeProvider.UtcNow,
                command.HochgeladenVonUserId);

            await _mitarbeiterDokumentRepository.AddAsync(dokument, cancellationToken);
            await _mitarbeiterDokumentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(UploadMitarbeiterDokumentService));
            return UploadMitarbeiterDokumentResult.Erfolg();
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(speicherpfad))
            {
                await _mitarbeiterDokumentDateiSpeicher.LoescheAsync(speicherpfad, cancellationToken);
            }

            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(UploadMitarbeiterDokumentService), "Dokument konnte nicht gespeichert werden");
            return UploadMitarbeiterDokumentResult.Fehler(
                "Das Dokument konnte nicht gespeichert werden.");
        }
    }
}