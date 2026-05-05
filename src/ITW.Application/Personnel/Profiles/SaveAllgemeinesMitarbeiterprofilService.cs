using ITW.Application.Abstractions.DateTime;
using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Enums;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.Profiles;

public sealed class SaveAllgemeinesMitarbeiterprofilService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IAllgemeinesMitarbeiterprofilRepository _allgemeinesMitarbeiterprofilRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<SaveAllgemeinesMitarbeiterprofilService> _logger;

    public SaveAllgemeinesMitarbeiterprofilService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IAllgemeinesMitarbeiterprofilRepository allgemeinesMitarbeiterprofilRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        IDateTimeProvider dateTimeProvider,
        ILogger<SaveAllgemeinesMitarbeiterprofilService> logger)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _allgemeinesMitarbeiterprofilRepository = allgemeinesMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(allgemeinesMitarbeiterprofilRepository));
        _benutzerkontoRepository = benutzerkontoRepository
            ?? throw new ArgumentNullException(nameof(benutzerkontoRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SaveAllgemeinesMitarbeiterprofilResult> ExecuteAsync(
        SaveAllgemeinesMitarbeiterprofilCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(SaveAllgemeinesMitarbeiterprofilService));

        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(SaveAllgemeinesMitarbeiterprofilService));
            return SaveAllgemeinesMitarbeiterprofilResult.Fehler("Die UserId ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.Vorname))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Vorname leer", nameof(SaveAllgemeinesMitarbeiterprofilService));
            return SaveAllgemeinesMitarbeiterprofilResult.Fehler("Der Vorname ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.Nachname))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Nachname leer", nameof(SaveAllgemeinesMitarbeiterprofilService));
            return SaveAllgemeinesMitarbeiterprofilResult.Fehler("Der Nachname ist erforderlich.");
        }

        if (!Enum.IsDefined(typeof(MitarbeiterBeschaeftigungsart), command.Beschaeftigungsart))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Beschaeftigungsart ungültig", nameof(SaveAllgemeinesMitarbeiterprofilService));
            return SaveAllgemeinesMitarbeiterprofilResult.Fehler("Die Beschäftigungsart ist ungültig.");
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            command.UserId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(SaveAllgemeinesMitarbeiterprofilService), "Keine aktive ITW-Zuordnung");
            return SaveAllgemeinesMitarbeiterprofilResult.Fehler(
                "Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        var jetzt = _dateTimeProvider.UtcNow;

        await _allgemeinesMitarbeiterprofilRepository.UpsertAsync(
            command.UserId,
            command.Vorname,
            command.Nachname,
            command.Beschaeftigungsart,
            command.Telefonnummer,
            command.Strasse,
            command.Hausnummer,
            command.Postleitzahl,
            command.Ort,
            jetzt,
            cancellationToken);

        await _allgemeinesMitarbeiterprofilRepository.SaveChangesAsync(cancellationToken);

        await _benutzerkontoRepository.SynchronisiereNamensClaimsAsync(
            command.UserId,
            command.Vorname,
            command.Nachname,
            cancellationToken);

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(SaveAllgemeinesMitarbeiterprofilService));
        return SaveAllgemeinesMitarbeiterprofilResult.Erfolg();
    }
}