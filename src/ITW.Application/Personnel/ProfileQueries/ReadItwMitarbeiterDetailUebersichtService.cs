using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Enums;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Personnel.ProfileQueries;

public sealed class ReadItwMitarbeiterDetailUebersichtService
{
    private readonly IBenutzerBereichszuordnungRepository _benutzerBereichszuordnungRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly IItwMitarbeiterprofilRepository _itwMitarbeiterprofilRepository;
    private readonly IAllgemeinesMitarbeiterprofilRepository _allgemeinesMitarbeiterprofilRepository;
    private readonly ILogger<ReadItwMitarbeiterDetailUebersichtService> _logger;

    public ReadItwMitarbeiterDetailUebersichtService(
        IBenutzerBereichszuordnungRepository benutzerBereichszuordnungRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        IItwMitarbeiterprofilRepository itwMitarbeiterprofilRepository,
        IAllgemeinesMitarbeiterprofilRepository allgemeinesMitarbeiterprofilRepository,
        ILogger<ReadItwMitarbeiterDetailUebersichtService> logger)
    {
        ArgumentNullException.ThrowIfNull(benutzerBereichszuordnungRepository);
        _benutzerBereichszuordnungRepository = benutzerBereichszuordnungRepository;
        ArgumentNullException.ThrowIfNull(benutzerkontoRepository);
        _benutzerkontoRepository = benutzerkontoRepository;
        ArgumentNullException.ThrowIfNull(itwMitarbeiterprofilRepository);
        _itwMitarbeiterprofilRepository = itwMitarbeiterprofilRepository;
        ArgumentNullException.ThrowIfNull(allgemeinesMitarbeiterprofilRepository);
        _allgemeinesMitarbeiterprofilRepository = allgemeinesMitarbeiterprofilRepository;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<ReadItwMitarbeiterDetailUebersichtResult> ExecuteAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(ReadItwMitarbeiterDetailUebersichtService));
            return ReadItwMitarbeiterDetailUebersichtResult.Fehler("Die UserId ist erforderlich.");
        }

        var zuordnung = await _benutzerBereichszuordnungRepository.GetAktivePrimaereZuordnungAsync(
            userId,
            cancellationToken);

        if (zuordnung is null || zuordnung.Bereich != Organisationsbereich.Intensivtransport)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadItwMitarbeiterDetailUebersichtService), "Keine aktive ITW-Zuordnung");
            return ReadItwMitarbeiterDetailUebersichtResult.Fehler(
                "Für dieses Benutzerkonto existiert keine aktive ITW-Zuordnung.");
        }

        var konto = (await _benutzerkontoRepository.GetByIdsAsync(new[] { userId }, cancellationToken))
            .FirstOrDefault();

        if (konto is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadItwMitarbeiterDetailUebersichtService), "Benutzerkonto nicht gefunden");
            return ReadItwMitarbeiterDetailUebersichtResult.Fehler("Das Benutzerkonto wurde nicht gefunden.");
        }

        await _itwMitarbeiterprofilRepository.EnsureStandardqualifikationenAsync(cancellationToken);

        var itwProfil = await _itwMitarbeiterprofilRepository.GetByUserIdAsync(userId, cancellationToken);
        var allgemeinesProfil = await _allgemeinesMitarbeiterprofilRepository.GetByUserIdAsync(userId, cancellationToken);
        var qualifikationen = await _itwMitarbeiterprofilRepository.GetAktiveQualifikationenAsync(cancellationToken);
        var qualifikationLookup = qualifikationen.ToDictionary(x => x.Id, x => x.Bezeichnung);

        var hauptqualifikation = "Noch kein ITW-Profil";
        var zusatzqualifikationen = Array.Empty<string>();
        DateTimeOffset? profilAktualisiertAm = null;

        if (itwProfil is not null)
        {
            hauptqualifikation = itwProfil.Qualifikationen
                .Where(x => x.IstHauptqualifikation)
                .Select(x => qualifikationLookup.TryGetValue(x.QualifikationId, out var name)
                    ? name
                    : "Unbekannte Qualifikation")
                .FirstOrDefault()
                ?? "Keine Hauptqualifikation";

            zusatzqualifikationen = itwProfil.Qualifikationen
                .Where(x => !x.IstHauptqualifikation)
                .Select(x => qualifikationLookup.TryGetValue(x.QualifikationId, out var name)
                    ? name
                    : "Unbekannte Qualifikation")
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            profilAktualisiertAm = itwProfil.AktualisiertAm;
        }

        var anzeigeName = konto.Benutzername;
        var vorname = string.Empty;
        var nachname = string.Empty;
        var beschaeftigungsart = MitarbeiterBeschaeftigungsart.Unbekannt;
        var telefonnummer = string.Empty;
        var anschriftKurz = "Noch keine Anschrift hinterlegt";
        DateTimeOffset? stammdatenAktualisiertAm = null;

        if (allgemeinesProfil is not null)
        {
            anzeigeName = !string.IsNullOrWhiteSpace(allgemeinesProfil.DisplayName)
                ? allgemeinesProfil.DisplayName!
                : $"{allgemeinesProfil.Vorname} {allgemeinesProfil.Nachname}".Trim();

            vorname = allgemeinesProfil.Vorname;
            nachname = allgemeinesProfil.Nachname;
            beschaeftigungsart = allgemeinesProfil.Beschaeftigungsart;
            telefonnummer = allgemeinesProfil.Telefonnummer ?? string.Empty;
            anschriftKurz = BaueAnschriftKurz(allgemeinesProfil.Strasse, allgemeinesProfil.Hausnummer, allgemeinesProfil.Postleitzahl, allgemeinesProfil.Ort);
            stammdatenAktualisiertAm = allgemeinesProfil.AktualisiertAm;
        }

        var dto = new ItwMitarbeiterDetailUebersichtDto(
            konto.UserId,
            konto.Benutzername,
            konto.Email,
            konto.IstGesperrt,
            zuordnung.Rolle.ToString(),
            zuordnung.Fuehrungsverantwortung.ToString(),
            itwProfil is not null,
            hauptqualifikation,
            zusatzqualifikationen,
            profilAktualisiertAm,
            allgemeinesProfil is not null,
            anzeigeName,
            vorname,
            nachname,
            beschaeftigungsart,
            telefonnummer,
            anschriftKurz,
            stammdatenAktualisiertAm,
            zuordnung.ZugewiesenAm);

        return ReadItwMitarbeiterDetailUebersichtResult.Erfolg(dto);
    }

    private static string BaueAnschriftKurz(
        string? strasse,
        string? hausnummer,
        string? postleitzahl,
        string? ort)
    {
        var zeile1 = string.Join(" ", new[] { strasse, hausnummer }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
        var zeile2 = string.Join(" ", new[] { postleitzahl, ort }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

        if (string.IsNullOrWhiteSpace(zeile1) && string.IsNullOrWhiteSpace(zeile2))
        {
            return "Noch keine Anschrift hinterlegt";
        }

        if (string.IsNullOrWhiteSpace(zeile1))
        {
            return zeile2;
        }

        if (string.IsNullOrWhiteSpace(zeile2))
        {
            return zeile1;
        }

        return $"{zeile1}, {zeile2}";
    }
}