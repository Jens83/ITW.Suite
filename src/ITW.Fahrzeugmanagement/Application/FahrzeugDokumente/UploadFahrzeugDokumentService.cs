using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;

public sealed record UploadFahrzeugDokumentCommand(
    Guid FahrzeugId,
    FahrzeugDokumentKategorie Kategorie,
    string Bezeichnung,
    string DateinameOriginal,
    byte[] Dateiinhalt,
    DateOnly? GueltigBis,
    string HochgeladenVonUserId);

public sealed class UploadFahrzeugDokumentResult
{
    private UploadFahrzeugDokumentResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static UploadFahrzeugDokumentResult Erfolg()
        => new(true, null);

    public static UploadFahrzeugDokumentResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class UploadFahrzeugDokumentService
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

    private readonly IFahrzeugRepository _fahrzeugRepository;
    private readonly IFahrzeugDokumentRepository _dokumentRepository;
    private readonly IFahrzeugDokumentDateiSpeicher _dateiSpeicher;

    public UploadFahrzeugDokumentService(
        IFahrzeugRepository fahrzeugRepository,
        IFahrzeugDokumentRepository dokumentRepository,
        IFahrzeugDokumentDateiSpeicher dateiSpeicher)
    {
        _fahrzeugRepository = fahrzeugRepository
            ?? throw new ArgumentNullException(nameof(fahrzeugRepository));

        _dokumentRepository = dokumentRepository
            ?? throw new ArgumentNullException(nameof(dokumentRepository));

        _dateiSpeicher = dateiSpeicher
            ?? throw new ArgumentNullException(nameof(dateiSpeicher));
    }

    public async Task<UploadFahrzeugDokumentResult> ExecuteAsync(
        UploadFahrzeugDokumentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.FahrzeugId == Guid.Empty)
        {
            return UploadFahrzeugDokumentResult.Fehler("Das Fahrzeug konnte nicht ermittelt werden.");
        }

        if (string.IsNullOrWhiteSpace(command.Bezeichnung))
        {
            return UploadFahrzeugDokumentResult.Fehler("Bitte eine Bezeichnung für das Dokument eingeben.");
        }

        if (command.Kategorie == FahrzeugDokumentKategorie.Unbekannt)
        {
            return UploadFahrzeugDokumentResult.Fehler("Bitte eine Dokumentkategorie auswählen.");
        }

        if (string.IsNullOrWhiteSpace(command.DateinameOriginal))
        {
            return UploadFahrzeugDokumentResult.Fehler("Der Dateiname ist erforderlich.");
        }

        if (command.Dateiinhalt.Length == 0)
        {
            return UploadFahrzeugDokumentResult.Fehler("Es wurde keine Datei übergeben.");
        }

        if (command.Dateiinhalt.Length > MaxDateigroesseBytes)
        {
            return UploadFahrzeugDokumentResult.Fehler("Die Datei ist zu groß. Erlaubt sind maximal 10 MB.");
        }

        if (string.IsNullOrWhiteSpace(command.HochgeladenVonUserId))
        {
            return UploadFahrzeugDokumentResult.Fehler("Der aktuelle Benutzer konnte nicht ermittelt werden.");
        }

        var fahrzeug = await _fahrzeugRepository.GetFahrzeugByIdAsync(
            command.FahrzeugId,
            cancellationToken);

        if (fahrzeug is null)
        {
            return UploadFahrzeugDokumentResult.Fehler("Das Fahrzeug wurde nicht gefunden.");
        }

        var dateinameOriginal = Path.GetFileName(command.DateinameOriginal.Trim());
        var dateiendung = Path.GetExtension(dateinameOriginal)?.Trim() ?? string.Empty;

        if (!ErlaubteDateitypen.TryGetValue(dateiendung, out var contentType))
        {
            return UploadFahrzeugDokumentResult.Fehler(
                "Es sind nur PDF-, JPG-, JPEG-, PNG-, DOC- und DOCX-Dateien erlaubt.");
        }

        string? speicherpfad = null;

        try
        {
            speicherpfad = await _dateiSpeicher.SpeichereAsync(
                command.FahrzeugId,
                dateinameOriginal,
                command.Dateiinhalt,
                cancellationToken);

            var dokument = new FahrzeugDokument(
                Guid.NewGuid(),
                command.FahrzeugId,
                command.Kategorie,
                dateinameOriginal,
                command.Bezeichnung,
                contentType,
                speicherpfad,
                command.GueltigBis,
                command.HochgeladenVonUserId,
                DateTimeOffset.UtcNow);

            await _dokumentRepository.AddAsync(dokument, cancellationToken);
            await _dokumentRepository.SaveChangesAsync(cancellationToken);

            return UploadFahrzeugDokumentResult.Erfolg();
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(speicherpfad))
            {
                await _dateiSpeicher.LoescheAsync(speicherpfad, cancellationToken);
            }

            return UploadFahrzeugDokumentResult.Fehler(
                "Das Dokument konnte nicht gespeichert werden.");
        }
    }
}