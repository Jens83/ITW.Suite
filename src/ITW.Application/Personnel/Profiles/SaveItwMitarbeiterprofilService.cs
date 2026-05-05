using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Qualifications;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.Profiles;

public sealed class SaveItwMitarbeiterprofilService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IItwMitarbeiterprofilRepository _itwMitarbeiterprofilRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SaveItwMitarbeiterprofilService> _logger;

    public SaveItwMitarbeiterprofilService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IItwMitarbeiterprofilRepository itwMitarbeiterprofilRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<SaveItwMitarbeiterprofilService> logger)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _itwMitarbeiterprofilRepository = itwMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(itwMitarbeiterprofilRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SaveItwMitarbeiterprofilResult> ExecuteAsync(
        SaveItwMitarbeiterprofilCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(SaveItwMitarbeiterprofilService));

        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(SaveItwMitarbeiterprofilService));
            return SaveItwMitarbeiterprofilResult.Fehler("Die UserId ist erforderlich.");
        }

        if (command.HauptqualifikationId == Guid.Empty)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: HauptqualifikationId leer", nameof(SaveItwMitarbeiterprofilService));
            return SaveItwMitarbeiterprofilResult.Fehler("Die Hauptqualifikation ist erforderlich.");
        }

        var zusatzqualifikationIds = command.ZusatzqualifikationIds
            .Where(x => x != Guid.Empty && x != command.HauptqualifikationId)
            .Distinct()
            .ToArray();

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            command.UserId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SaveItwMitarbeiterprofilService), "Keine aktive ITW-Zuordnung");
            return SaveItwMitarbeiterprofilResult.Fehler("Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        await _itwMitarbeiterprofilRepository.EnsureStandardqualifikationenAsync(cancellationToken);

        var verfuegbareQualifikationen = await _itwMitarbeiterprofilRepository.GetAktiveQualifikationenAsync(cancellationToken);
        var qualifikationLookup = verfuegbareQualifikationen.ToDictionary(x => x.Id);

        if (!qualifikationLookup.TryGetValue(command.HauptqualifikationId, out var hauptqualifikation))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SaveItwMitarbeiterprofilService), "Hauptqualifikation nicht verfügbar");
            return SaveItwMitarbeiterprofilResult.Fehler("Die ausgewählte Hauptqualifikation ist nicht verfügbar.");
        }

        if (!ItwQualifikationsCodes.IstHauptqualifikationCode(hauptqualifikation.Code))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SaveItwMitarbeiterprofilService), "Ungültiger Hauptqualifikationscode");
            return SaveItwMitarbeiterprofilResult.Fehler("Als Hauptqualifikation sind aktuell nur Arzt oder Notfallsanitäter zulässig.");
        }

        foreach (var zusatzqualifikationId in zusatzqualifikationIds)
        {
            if (!qualifikationLookup.TryGetValue(zusatzqualifikationId, out var zusatzqualifikation))
            {
                _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SaveItwMitarbeiterprofilService), "Zusatzqualifikation nicht verfügbar");
                return SaveItwMitarbeiterprofilResult.Fehler("Mindestens eine ausgewählte Zusatzqualifikation ist nicht verfügbar.");
            }

            if (!ItwQualifikationsCodes.IstZusatzqualifikationCode(zusatzqualifikation.Code))
            {
                _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SaveItwMitarbeiterprofilService), "Ungültiger Zusatzqualifikationscode");
                return SaveItwMitarbeiterprofilResult.Fehler("Arzt und Notfallsanitäter dürfen nicht als Zusatzqualifikation gespeichert werden.");
            }
        }

        var jetzt = _dateTimeProvider.UtcNow;

        await _itwMitarbeiterprofilRepository.UpsertQualifikationenAsync(
            command.UserId,
            command.HauptqualifikationId,
            zusatzqualifikationIds,
            jetzt,
            cancellationToken);

        await _itwMitarbeiterprofilRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SaveItwMitarbeiterprofilService));
        return SaveItwMitarbeiterprofilResult.Erfolg();
    }
}