// Datei: src/ITW.Application/Personnel/Documents/ReadMitarbeiterDokumenteService.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.Documents;

public sealed class ReadMitarbeiterDokumenteService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IMitarbeiterDokumentRepository _mitarbeiterDokumentRepository;
    private readonly ILogger<ReadMitarbeiterDokumenteService> _logger;

    public ReadMitarbeiterDokumenteService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IMitarbeiterDokumentRepository mitarbeiterDokumentRepository,
        ILogger<ReadMitarbeiterDokumenteService> logger)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _mitarbeiterDokumentRepository = mitarbeiterDokumentRepository
            ?? throw new ArgumentNullException(nameof(mitarbeiterDokumentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReadMitarbeiterDokumenteResult> ExecuteAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(ReadMitarbeiterDokumenteService));
            return ReadMitarbeiterDokumenteResult.Fehler("Die UserId ist erforderlich.");
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            userId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadMitarbeiterDokumenteService), "Keine aktive ITW-Zuordnung");
            return ReadMitarbeiterDokumenteResult.Fehler(
                "Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        var dokumente = await _mitarbeiterDokumentRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        var result = dokumente
            .Select(x => new MitarbeiterDokumentListenEintragDto(
                x.Id,
                x.Kategorie,
                x.DateinameOriginal,
                x.Inhaltstyp,
                x.DateigroesseBytes,
                x.HochgeladenAm))
            .ToList();

        return ReadMitarbeiterDokumenteResult.Erfolg(result);
    }
}