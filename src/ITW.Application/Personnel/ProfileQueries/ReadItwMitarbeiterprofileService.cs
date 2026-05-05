using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Enums;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.ProfileQueries;

public sealed class ReadItwMitarbeiterprofileService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly IItwMitarbeiterprofilRepository _itwMitarbeiterprofilRepository;
    private readonly IAllgemeinesMitarbeiterprofilRepository _allgemeinesMitarbeiterprofilRepository;
    private readonly ILogger<ReadItwMitarbeiterprofileService> _logger;

    public ReadItwMitarbeiterprofileService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        IItwMitarbeiterprofilRepository itwMitarbeiterprofilRepository,
        IAllgemeinesMitarbeiterprofilRepository allgemeinesMitarbeiterprofilRepository,
        ILogger<ReadItwMitarbeiterprofileService> logger)
    {
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository
            ?? throw new ArgumentNullException(nameof(benutzerBereichszuordnungRepository));
        _benutzerkontoRepository = benutzerkontoRepository
            ?? throw new ArgumentNullException(nameof(benutzerkontoRepository));
        _itwMitarbeiterprofilRepository = itwMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(itwMitarbeiterprofilRepository));
        _allgemeinesMitarbeiterprofilRepository = allgemeinesMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(allgemeinesMitarbeiterprofilRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReadItwMitarbeiterprofileResult> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        await _itwMitarbeiterprofilRepository.EnsureStandardqualifikationenAsync(cancellationToken);

        var zuordnungen = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungenByBereichAsync(
        Organisationsbereich.Intensivtransport,
        cancellationToken);

        if (zuordnungen.Count == 0)
        {
            return ReadItwMitarbeiterprofileResult.Erfolg(Array.Empty<ItwMitarbeiterprofilUebersichtDto>());
        }

        var userIds = zuordnungen
            .Select(x => x.UserId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var konten = await _benutzerkontoRepository.GetByIdsAsync(userIds, cancellationToken);
        var profile = await _itwMitarbeiterprofilRepository.GetByUserIdsAsync(userIds, cancellationToken);
        var allgemeineProfile = await _allgemeinesMitarbeiterprofilRepository.GetByUserIdsAsync(userIds, cancellationToken);
        var qualifikationen = await _itwMitarbeiterprofilRepository.GetAktiveQualifikationenAsync(cancellationToken);

        var qualifikationLookup = qualifikationen.ToDictionary(x => x.Id, x => x.Bezeichnung);
        var profilLookup = profile.ToDictionary(x => x.UserId, StringComparer.OrdinalIgnoreCase);
        var allgemeinesProfilLookup = allgemeineProfile.ToDictionary(x => x.UserId, StringComparer.OrdinalIgnoreCase);

        var ergebnis = konten
            .Select(konto =>
            {
                profilLookup.TryGetValue(konto.UserId, out var profil);
                allgemeinesProfilLookup.TryGetValue(konto.UserId, out var allgemeinesProfil);

                var anzeigeName = konto.Benutzername;
                var hatStammdatenprofil = false;
                DateTimeOffset? stammdatenAktualisiertAm = null;
                var beschaeftigungsart = MitarbeiterBeschaeftigungsart.Unbekannt;

                if (allgemeinesProfil is not null)
                {
                    hatStammdatenprofil = true;
                    stammdatenAktualisiertAm = allgemeinesProfil.AktualisiertAm;
                    beschaeftigungsart = allgemeinesProfil.Beschaeftigungsart;

                    anzeigeName = !string.IsNullOrWhiteSpace(allgemeinesProfil.DisplayName)
                        ? allgemeinesProfil.DisplayName!
                        : $"{allgemeinesProfil.Vorname} {allgemeinesProfil.Nachname}".Trim();

                    if (string.IsNullOrWhiteSpace(anzeigeName))
                    {
                        anzeigeName = konto.Benutzername;
                    }
                }

                var hauptqualifikation = "Noch kein Profil";
                var zusatzqualifikationen = Array.Empty<string>();
                DateTimeOffset? aktualisiertAm = null;

                if (profil is not null)
                {
                    var qualifikationsnamen = profil.Qualifikationen
                        .Select(x => new
                        {
                            x.IstHauptqualifikation,
                            Name = qualifikationLookup.TryGetValue(x.QualifikationId, out var name)
                                ? name
                                : "Unbekannte Qualifikation"
                        })
                        .ToList();

                    hauptqualifikation = qualifikationsnamen
                        .FirstOrDefault(x => x.IstHauptqualifikation)?.Name
                        ?? "Keine Hauptqualifikation";

                    zusatzqualifikationen = qualifikationsnamen
                        .Where(x => !x.IstHauptqualifikation)
                        .Select(x => x.Name)
                        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    aktualisiertAm = profil.AktualisiertAm;
                }

                return new ItwMitarbeiterprofilUebersichtDto(
                    konto.UserId,
                    anzeigeName,
                    konto.Benutzername,
                    konto.Email,
                    konto.IstGesperrt,
                    hatStammdatenprofil,
                    stammdatenAktualisiertAm,
                    beschaeftigungsart,
                    profil is not null,
                    hauptqualifikation,
                    zusatzqualifikationen,
                    aktualisiertAm);
            })
            .OrderBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Benutzername, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ReadItwMitarbeiterprofileResult.Erfolg(ergebnis);
    }
}