using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Qualifications;

namespace ITW.Application.Personnel.ProfileQueries;

public sealed class ReadItwMitarbeiterprofilDetailService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly IItwMitarbeiterprofilRepository _itwMitarbeiterprofilRepository;

    public ReadItwMitarbeiterprofilDetailService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        IItwMitarbeiterprofilRepository itwMitarbeiterprofilRepository)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _benutzerkontoRepository = benutzerkontoRepository
            ?? throw new ArgumentNullException(nameof(benutzerkontoRepository));
        _itwMitarbeiterprofilRepository = itwMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(itwMitarbeiterprofilRepository));
    }

    public async Task<ReadItwMitarbeiterprofilDetailResult> ExecuteAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return ReadItwMitarbeiterprofilDetailResult.Fehler("Die UserId ist erforderlich.");
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            userId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            return ReadItwMitarbeiterprofilDetailResult.Fehler("Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        await _itwMitarbeiterprofilRepository.EnsureStandardqualifikationenAsync(cancellationToken);

        var konto = (await _benutzerkontoRepository.GetByIdsAsync(new[] { userId }, cancellationToken))
            .FirstOrDefault();

        if (konto is null)
        {
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