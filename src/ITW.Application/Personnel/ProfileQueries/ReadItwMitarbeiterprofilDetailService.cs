using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Qualifications;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.ProfileQueries;

public sealed class ReadItwMitarbeiterprofilDetailService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly IItwMitarbeiterprofilRepository _itwMitarbeiterprofilRepository;
    private readonly ILogger<ReadItwMitarbeiterprofilDetailService> _logger;

    public ReadItwMitarbeiterprofilDetailService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        IItwMitarbeiterprofilRepository itwMitarbeiterprofilRepository,
        ILogger<ReadItwMitarbeiterprofilDetailService> logger)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _benutzerkontoRepository = benutzerkontoRepository
            ?? throw new ArgumentNullException(nameof(benutzerkontoRepository));
        _itwMitarbeiterprofilRepository = itwMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(itwMitarbeiterprofilRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReadItwMitarbeiterprofilDetailResult> ExecuteAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(ReadItwMitarbeiterprofilDetailService));
            return ReadItwMitarbeiterprofilDetailResult.Fehler("Die UserId ist erforderlich.");
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            userId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadItwMitarbeiterprofilDetailService), "Keine aktive ITW-Zuordnung");
            return ReadItwMitarbeiterprofilDetailResult.Fehler("Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        await _itwMitarbeiterprofilRepository.EnsureStandardqualifikationenAsync(cancellationToken);

        var konto = (await _benutzerkontoRepository.GetByIdsAsync(new[] { userId }, cancellationToken))
            .FirstOrDefault();

        if (konto is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadItwMitarbeiterprofilDetailService), "Benutzerkonto nicht gefunden");
            return ReadItwMitarbeiterprofilDetailResult.Fehler("Das Benutzerkonto wurde nicht gefunden.");
        }

        var profil = await _itwMitarbeiterprofilRepository.GetByUserIdAsync(userId, cancellationToken);
        var qualifikationen = await _itwMitarbeiterprofilRepository.GetAktiveQualifikationenAsync(cancellationToken);

        var detail = new ItwMitarbeiterprofilDetailDto(
            konto.UserId,
            konto.Benutzername,
            konto.Email,
            konto.IstGesperrt,
            profil?.Id,
            profil?.Qualifikationen.FirstOrDefault(x => x.IstHauptqualifikation)?.QualifikationId,
            profil?.Qualifikationen
                .Where(x => !x.IstHauptqualifikation)
                .Select(x => x.QualifikationId)
                .ToArray() ?? Array.Empty<Guid>(),
            profil?.AktualisiertAm);

        var hauptqualifikationen = qualifikationen
            .Where(x => ItwQualifikationsCodes.IstHauptqualifikationCode(x.Code))
            .OrderBy(x => x.Sortierung)
            .ThenBy(x => x.Bezeichnung, StringComparer.OrdinalIgnoreCase)
            .Select(x => new ItwQualifikationOptionDto(
                x.Id,
                x.Code,
                x.Bezeichnung,
                x.Sortierung))
            .ToArray();

        var zusatzqualifikationen = qualifikationen
            .Where(x => ItwQualifikationsCodes.IstZusatzqualifikationCode(x.Code))
            .OrderBy(x => x.Sortierung)
            .ThenBy(x => x.Bezeichnung, StringComparer.OrdinalIgnoreCase)
            .Select(x => new ItwQualifikationOptionDto(
                x.Id,
                x.Code,
                x.Bezeichnung,
                x.Sortierung))
            .ToArray();

        return ReadItwMitarbeiterprofilDetailResult.Erfolg(
            detail,
            hauptqualifikationen,
            zusatzqualifikationen);
    }
}