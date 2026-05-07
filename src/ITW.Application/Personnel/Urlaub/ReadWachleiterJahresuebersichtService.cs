using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.Urlaub;

public sealed class WachleiterJahresMitarbeiterDto
{
    public string  UserId       { get; init; } = string.Empty;
    public string  AnzeigeName  { get; init; } = string.Empty;
    public int     GesamtTage   { get; init; }
    public IReadOnlyList<WachleiterJahresZeitraumDto> Zeitraeume { get; init; } = [];
}

public sealed class WachleiterJahresZeitraumDto
{
    public Guid                  ZeitraumId         { get; init; }
    public DateOnly              Von                 { get; init; }
    public DateOnly              Bis                 { get; init; }
    public int                   Tage                { get; init; }
    public UrlaubszeitraumStatus Status              { get; init; }
    public bool                  HatUeberschneidung  { get; init; }
}

public sealed class WachleiterJahresUeberschneidungDto
{
    public DateOnly          Von            { get; init; }
    public DateOnly          Bis            { get; init; }
    public List<string>      MitarbeiterNamen { get; init; } = [];
}

public sealed class ReadWachleiterJahresuebersichtResult
{
    public int     Jahr         { get; init; }
    public IReadOnlyList<WachleiterJahresMitarbeiterDto>      Mitarbeiter     { get; init; } = [];
    public IReadOnlyList<WachleiterJahresUeberschneidungDto>  Ueberschneidungen { get; init; } = [];
}

public sealed class ReadWachleiterJahresuebersichtService
{
    private readonly IMitarbeiterUrlaubszeitraumRepository _urlaubszeitraumRepository;
    private readonly ReadItwMitarbeiterprofileService _profileService;

    public ReadWachleiterJahresuebersichtService(
        IMitarbeiterUrlaubszeitraumRepository urlaubszeitraumRepository,
        ReadItwMitarbeiterprofileService profileService)
    {
        ArgumentNullException.ThrowIfNull(urlaubszeitraumRepository);
        _urlaubszeitraumRepository = urlaubszeitraumRepository;

        ArgumentNullException.ThrowIfNull(profileService);
        _profileService = profileService;
    }

    public async Task<ReadWachleiterJahresuebersichtResult> ExecuteAsync(
        int jahr,
        CancellationToken cancellationToken = default)
    {
        var profileResult = await _profileService.ExecuteAsync(cancellationToken);

        var relevanteProfiles = profileResult.IsSuccess
            ? profileResult.Profile
                .Where(p => !p.IstGesperrt
                         && p.HatProfil
                         && (p.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Festangestellt
                          || p.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Freelancer))
                .OrderBy(p => p.AnzeigeName, StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : [];

        var alleZeitraeume = await _urlaubszeitraumRepository.GetAlleAktiveFuerJahrAsync(
            jahr, cancellationToken);

        // Zeiträume nach userId gruppieren
        var zeitraeumeByUser = alleZeitraeume
            .GroupBy(z => z.UserId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Überschneidungen erkennen: Tage, an denen ≥2 Mitarbeiter gleichzeitig Urlaub haben
        var jahresStart = new DateOnly(jahr, 1, 1);
        var jahresEnde  = new DateOnly(jahr, 12, 31);
        var tageImJahr  = DateTime.IsLeapYear(jahr) ? 366 : 365;

        // tagesbesetzung[i] = Liste der UserIds mit Urlaub an Tag i (0-basiert)
        var tagesbesetzung = new List<string>[tageImJahr];
        for (var i = 0; i < tageImJahr; i++)
            tagesbesetzung[i] = [];

        foreach (var zeitraum in alleZeitraeume)
        {
            var von = zeitraum.Von < jahresStart ? jahresStart : zeitraum.Von;
            var bis = zeitraum.Bis > jahresEnde  ? jahresEnde  : zeitraum.Bis;

            for (var tag = von; tag <= bis; tag = tag.AddDays(1))
            {
                var idx = tag.DayOfYear - 1;
                tagesbesetzung[idx].Add(zeitraum.UserId);
            }
        }

        // Zusammenhängende Überschneidungsperioden ermitteln
        var ueberschneidungen = new List<WachleiterJahresUeberschneidungDto>();
        var inUeberschneidung = false;
        DateOnly ueberschneidungStart = default;
        HashSet<string> beteiligteUserIds = [];

        for (var i = 0; i < tageImJahr; i++)
        {
            var besetzt = tagesbesetzung[i];
            var tag = jahresStart.AddDays(i);

            if (besetzt.Count >= 2)
            {
                if (!inUeberschneidung)
                {
                    inUeberschneidung  = true;
                    ueberschneidungStart = tag;
                    beteiligteUserIds   = [];
                }

                foreach (var uid in besetzt)
                    beteiligteUserIds.Add(uid);
            }
            else if (inUeberschneidung)
            {
                inUeberschneidung = false;
                ueberschneidungen.Add(ErzeugeUeberschneidung(
                    ueberschneidungStart,
                    tag.AddDays(-1),
                    beteiligteUserIds,
                    profileResult));
            }
        }

        if (inUeberschneidung)
        {
            ueberschneidungen.Add(ErzeugeUeberschneidung(
                ueberschneidungStart,
                jahresEnde,
                beteiligteUserIds,
                profileResult));
        }

        // Set of date ranges that overlap (for HatUeberschneidung flag)
        // A Zeitraum has an overlap if any of its days is in a multi-person day
        var ueberschneidungsTage = new HashSet<DateOnly>();
        for (var i = 0; i < tageImJahr; i++)
        {
            if (tagesbesetzung[i].Count >= 2)
                ueberschneidungsTage.Add(jahresStart.AddDays(i));
        }

        // Pro Mitarbeiter DTO bauen
        var namenByUserId = profileResult.IsSuccess
            ? profileResult.Profile.ToDictionary(
                p => p.UserId,
                p => p.AnzeigeName,
                StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var mitarbeiterDtos = relevanteProfiles
            .Select(profil =>
            {
                var zeitraeume = zeitraeumeByUser.TryGetValue(profil.UserId, out var list)
                    ? list
                    : [];

                var zeitraumDtos = zeitraeume
                    .Select(z =>
                    {
                        var tage = ZaehleArbeitstage(z.Von, z.Bis, jahr);
                        var vonClamped = z.Von < jahresStart ? jahresStart : z.Von;
                        var bisClamped = z.Bis > jahresEnde  ? jahresEnde  : z.Bis;
                        var hatUeberschneidung = false;

                        for (var tag = vonClamped; tag <= bisClamped && !hatUeberschneidung; tag = tag.AddDays(1))
                            hatUeberschneidung = ueberschneidungsTage.Contains(tag);

                        return new WachleiterJahresZeitraumDto
                        {
                            ZeitraumId        = z.Id,
                            Von               = z.Von,
                            Bis               = z.Bis,
                            Tage              = tage,
                            Status            = z.Status,
                            HatUeberschneidung = hatUeberschneidung
                        };
                    })
                    .ToArray();

                return new WachleiterJahresMitarbeiterDto
                {
                    UserId      = profil.UserId,
                    AnzeigeName = profil.AnzeigeName,
                    GesamtTage  = zeitraumDtos.Sum(z => z.Tage),
                    Zeitraeume  = zeitraumDtos
                };
            })
            .ToList();

        return new ReadWachleiterJahresuebersichtResult
        {
            Jahr              = jahr,
            Mitarbeiter       = mitarbeiterDtos,
            Ueberschneidungen = ueberschneidungen
        };
    }

    private static WachleiterJahresUeberschneidungDto ErzeugeUeberschneidung(
        DateOnly von,
        DateOnly bis,
        HashSet<string> userIds,
        ReadItwMitarbeiterprofileResult profileResult)
    {
        var namenByUserId = profileResult.IsSuccess
            ? profileResult.Profile.ToDictionary(
                p => p.UserId,
                p => p.AnzeigeName,
                StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var namen = userIds
            .Select(uid => namenByUserId.TryGetValue(uid, out var n) ? n : uid)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new WachleiterJahresUeberschneidungDto
        {
            Von              = von,
            Bis              = bis,
            MitarbeiterNamen = namen
        };
    }

    private static int ZaehleArbeitstage(DateOnly von, DateOnly bis, int jahr)
    {
        var start = von.Year < jahr ? new DateOnly(jahr, 1, 1) : von;
        var ende  = bis.Year > jahr ? new DateOnly(jahr, 12, 31) : bis;

        if (ende < start) return 0;

        var feiertage = ITW.Domain.Kalender.MecklenburgVorpommernFeiertage.GetFeiertage(jahr);
        var zaehler   = 0;

        for (var tag = start; tag <= ende; tag = tag.AddDays(1))
        {
            if (tag.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) continue;
            if (feiertage.ContainsKey(tag)) continue;
            zaehler++;
        }

        return zaehler;
    }
}
