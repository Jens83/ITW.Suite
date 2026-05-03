// Datei: src/ITW.Application/Personnel/Documents/UploadMitarbeiterDokumentService.cs
using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Entities;

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

    public UploadMitarbeiterDokumentService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IMitarbeiterDokumentRepository mitarbeiterDokumentRepository,
        IMitarbeiterDokumentDateiSpeicher mitarbeiterDokumentDateiSpeicher,
        IDateTimeProvider dateTimeProvider)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _mitarbeiterDokumentRepository = mitarbeiterDokumentRepository
            ?? throw new ArgumentNullException(nameof(mitarbeiterDokumentRepository));
        _mitarbeiterDokumentDateiSpeicher = mitarbeiterDokumentDateiSpeicher
            ?? throw new ArgumentNullException(nameof(mitarbeiterDokumentDateiSpeicher));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<UploadMitarbeiterDokumentResult> ExecuteAsync(
        UploadMitarbeiterDokumentCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return UploadMitarbeiterDokumentResult.Fehler("Die UserId ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.HochgeladenVonUserId))
        {
            return UploadMitarbeiterDokumentResult.Fehler("Die Bearbeiter-UserId ist erforderlich.");
        }

        if (!MitarbeiterDokumentKategorien.IstErlaubt(command.Kategorie))
        {
            return UploadMitarbeiterDokumentResult.Fehler("Bitte eine gültige Dokumentkategorie auswählen.");
        }

        if (string.IsNullOrWhiteSpace(command.DateinameOriginal))
        {
            return UploadMitarbeiterDokumentResult.Fehler("Der Dateiname ist erforderlich.");
        }

        if (command.Dateiinhalt is null || command.Dateiinhalt.Length == 0)
        {
            return UploadMitarbeiterDokumentResult.Fehler("Es wurde keine Datei übergeben.");
        }

        if (command.Dateiinhalt.Length > MaxDateigroesseBytes)
        {
            return UploadMitarbeiterDokumentResult.Fehler("Die Datei ist zu groß. Erlaubt sind maximal 10 MB.");
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            command.UserId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            return UploadMitarbeiterDokumentResult.Fehler(
                "Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        var dateinameOriginal = Path.GetFileName(command.DateinameOriginal.Trim());
        var dateiendung = Path.GetExtension(dateinameOriginal)?.Trim() ?? string.Empty;

        if (!ErlaubteDateitypen.TryGetValue(dateiendung, out var inhaltstyp))
        {
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

            return UploadMitarbeiterDokumentResult.Erfolg();
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(speicherpfad))
            {
                await _mitarbeiterDokumentDateiSpeicher.LoescheAsync(speicherpfad, cancellationToken);
            }

            return UploadMitarbeiterDokumentResult.Fehler(
                "Das Dokument konnte nicht gespeichert werden.");
        }
    }
}