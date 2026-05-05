using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Enums;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.ProfileQueries;

public sealed class ReadAllgemeinesMitarbeiterprofilDetailService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly IAllgemeinesMitarbeiterprofilRepository _allgemeinesMitarbeiterprofilRepository;
    private readonly ILogger<ReadAllgemeinesMitarbeiterprofilDetailService> _logger;

    public ReadAllgemeinesMitarbeiterprofilDetailService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        IAllgemeinesMitarbeiterprofilRepository allgemeinesMitarbeiterprofilRepository,
        ILogger<ReadAllgemeinesMitarbeiterprofilDetailService> logger)
    {
        ArgumentNullException.ThrowIfNull(benutzerBereichszuordnungRepository);
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository;
        ArgumentNullException.ThrowIfNull(benutzerkontoRepository);
        _benutzerkontoRepository = benutzerkontoRepository;
        ArgumentNullException.ThrowIfNull(allgemeinesMitarbeiterprofilRepository);
        _allgemeinesMitarbeiterprofilRepository = allgemeinesMitarbeiterprofilRepository;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<ReadAllgemeinesMitarbeiterprofilDetailResult> ExecuteAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(ReadAllgemeinesMitarbeiterprofilDetailService));
            return ReadAllgemeinesMitarbeiterprofilDetailResult.Fehler("Die UserId ist erforderlich.");
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            userId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadAllgemeinesMitarbeiterprofilDetailService), "Keine aktive ITW-Zuordnung");
            return ReadAllgemeinesMitarbeiterprofilDetailResult.Fehler(
                "Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        var konto = (await _benutzerkontoRepository.GetByIdsAsync(new[] { userId }, cancellationToken))
            .FirstOrDefault();

        if (konto is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadAllgemeinesMitarbeiterprofilDetailService), "Benutzerkonto nicht gefunden");
            return ReadAllgemeinesMitarbeiterprofilDetailResult.Fehler("Das Benutzerkonto wurde nicht gefunden.");
        }

        var profil = await _allgemeinesMitarbeiterprofilRepository.GetByUserIdAsync(userId, cancellationToken);

        var dto = new AllgemeinesMitarbeiterprofilDetailDto(
            konto.UserId,
            konto.Benutzername,
            konto.Email,
            konto.IstGesperrt,
            profil?.Id,
            profil?.Vorname ?? string.Empty,
            profil?.Nachname ?? string.Empty,
            profil?.DisplayName ?? string.Empty,
            profil?.Beschaeftigungsart ?? MitarbeiterBeschaeftigungsart.Unbekannt,
            profil?.Telefonnummer ?? string.Empty,
            profil?.Strasse ?? string.Empty,
            profil?.Hausnummer ?? string.Empty,
            profil?.Postleitzahl ?? string.Empty,
            profil?.Ort ?? string.Empty,
            profil?.AktualisiertAm);

        return ReadAllgemeinesMitarbeiterprofilDetailResult.Erfolg(dto);
    }
}