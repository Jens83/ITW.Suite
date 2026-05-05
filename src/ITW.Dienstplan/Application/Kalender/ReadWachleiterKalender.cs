using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Perioden;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Kalender;

public sealed record ReadWachleiterKalenderQuery(Guid? PeriodeId, DateOnly Heute);

public sealed class WachleiterKalenderTagEintrag
{
    public DateOnly Datum { get; init; }

    public bool IstImAktivenMonat { get; init; }

    public bool IstHeute { get; init; }

    public bool IstAlsWunschMarkiert { get; init; }

    public bool IstAlsNichtVerfuegbarMarkiert { get; init; }

    public bool IstFeiertag { get; init; }

    public bool IstWochenende { get; init; }

    public bool IstAuswaehlbar { get; init; }

    public string FeiertagsName { get; init; } = string.Empty;

    public int AnzahlWuenscheGesamt { get; init; }

    public int AnzahlAerzte { get; init; }

    public int AnzahlNotfallsanitaeter { get; init; }

    public int AnzahlGeplanteAerzte { get; init; }

    public int AnzahlGeplanteNotfallsanitaeter { get; init; }

    public bool HatAutomatikKonflikt { get; init; }

    public string AutomatikKonfliktText { get; init; } = string.Empty;
}

public sealed class WachleiterKalenderWocheEintrag
{
    public IReadOnlyList<WachleiterKalenderTagEintrag> Tage { get; init; }
        = Array.Empty<WachleiterKalenderTagEintrag>();
}

public sealed class ReadWachleiterKalenderResult
{
    public bool IsSuccess { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyList<DienstplanPeriodeListenEintrag> Perioden { get; init; }
        = Array.Empty<DienstplanPeriodeListenEintrag>();

    public DienstplanPeriodeListenEintrag? AusgewaehltePeriode { get; init; }

    public IReadOnlyList<WachleiterKalenderWocheEintrag> KalenderWochen { get; init; }
        = Array.Empty<WachleiterKalenderWocheEintrag>();
}

public sealed class ReadWachleiterKalenderService
{
    private readonly ReadDienstplanperiodenService _readDienstplanperiodenService;
    private readonly IDienstwunschAuswertungRepository _dienstwunschAuswertungRepository;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly ReadAutomatischePlanungsvorschauService _readAutomatischePlanungsvorschauService;

    public ReadWachleiterKalenderService(
        ReadDienstplanperiodenService readDienstplanperiodenService,
        IDienstwunschAuswertungRepository dienstwunschAuswertungRepository,
        IGeplanterDienstTagRepository geplanterDienstTagRepository,
        ReadAutomatischePlanungsvorschauService readAutomatischePlanungsvorschauService)
    {
        ArgumentNullException.ThrowIfNull(readDienstplanperiodenService);
        _readDienstplanperiodenService = readDienstplanperiodenService;

        ArgumentNullException.ThrowIfNull(dienstwunschAuswertungRepository);
        _dienstwunschAuswertungRepository = dienstwunschAuswertungRepository;

        ArgumentNullException.ThrowIfNull(geplanterDienstTagRepository);
        _geplanterDienstTagRepository = geplanterDienstTagRepository;

        ArgumentNullException.ThrowIfNull(readAutomatischePlanungsvorschauService);
        _readAutomatischePlanungsvorschauService = readAutomatischePlanungsvorschauService;
    }

    public async Task<ReadWachleiterKalenderResult> ExecuteAsync(
        ReadWachleiterKalenderQuery query,
        CancellationToken cancellationToken = default)
    {
        var readPeriodenResult = await _readDienstplanperiodenService.ExecuteAsync(cancellationToken);
        var perioden = readPeriodenResult.Perioden;

        var ausgewaehltePeriode = ErmittleAusgewaehltePeriode(
            perioden,
            query.PeriodeId);

        if (ausgewaehltePeriode is null)
        {
            return new ReadWachleiterKalenderResult
            {
                IsSuccess = readPeriodenResult.IsSuccess,
                ErrorMessage = readPeriodenResult.ErrorMessage,
                Perioden = perioden,
                AusgewaehltePeriode = null,
                KalenderWochen = Array.Empty<WachleiterKalenderWocheEintrag>()
            };
        }

        var tagesstatistik = await _dienstwunschAuswertungRepository.GetTagesstatistikAsync(
            ausgewaehltePeriode.Id,
            cancellationToken);

        var statistikLookup = tagesstatistik.ToDictionary(x => x.Datum);

        var geplanteDiensttage = await _geplanterDienstTagRepository.GetAlleFuerPeriodeAsync(
            ausgewaehltePeriode.Id,
            cancellationToken);

        var planLookup = geplanteDiensttage.ToDictionary(x => x.DienstDatum);

        var vorschauResult = await _readAutomatischePlanungsvorschauService.ExecuteAsync(
            ausgewaehltePeriode.Id,
            AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen,
            cancellationToken);

        var automatikKonfliktLookup = vorschauResult.IsSuccess
            ? vorschauResult.Tage
                .Where(x =>
                    x.HatKonflikt &&
                    (
                        string.IsNullOrWhiteSpace(x.ArztUserId) ||
                        string.IsNullOrWhiteSpace(x.Notfallsanitaeter1UserId) ||
                        string.IsNullOrWhiteSpace(x.Notfallsanitaeter2UserId) ||
                        x.Konflikte.Any(k => k.Art == AutomatischePlanungKonfliktArt.BestehendePlanungKollidiertMitUrlaub)
                    ))
                .ToDictionary(x => x.Datum, x => x.KonfliktText)
            : null;

        var kalenderWochen = BaueKalenderWochen(
            ausgewaehltePeriode.Jahr,
            ausgewaehltePeriode.Monat,
            Array.Empty<int>(),
            Array.Empty<int>(),
            query.Heute,
            statistikLookup,
            planLookup,
            automatikKonfliktLookup);

        return new ReadWachleiterKalenderResult
        {
            IsSuccess = readPeriodenResult.IsSuccess,
            ErrorMessage = readPeriodenResult.ErrorMessage,
            Perioden = perioden,
            AusgewaehltePeriode = ausgewaehltePeriode,
            KalenderWochen = kalenderWochen
        };
    }

    private static DienstplanPeriodeListenEintrag? ErmittleAusgewaehltePeriode(
        IReadOnlyList<DienstplanPeriodeListenEintrag> perioden,
        Guid? periodeId)
    {
        DienstplanPeriodeListenEintrag? ausgewaehltePeriode = null;

        if (periodeId.HasValue && periodeId.Value != Guid.Empty)
        {
            ausgewaehltePeriode = perioden.FirstOrDefault(x => x.Id == periodeId.Value);
        }

        ausgewaehltePeriode ??= perioden.FirstOrDefault(x => x.WunschphaseIstOffen);
        ausgewaehltePeriode ??= perioden.FirstOrDefault();

        return ausgewaehltePeriode;
    }

    private static IReadOnlyList<WachleiterKalenderWocheEintrag> BaueKalenderWochen(
        int jahr,
        int monat,
        IReadOnlyList<int> wunschTage,
        IReadOnlyList<int> nichtVerfuegbareTage,
        DateOnly heute,
        IReadOnlyDictionary<DateOnly, DienstwunschTagesstatistik>? statistikLookup = null,
        IReadOnlyDictionary<DateOnly, GeplanterDienstTag>? planLookup = null,
        IReadOnlyDictionary<DateOnly, string>? automatikKonfliktLookup = null)
    {
        var ersteDatum = new DateOnly(jahr, monat, 1);
        var letzteDatum = ersteDatum.AddMonths(1).AddDays(-1);
        var startOffset = ((int)ersteDatum.DayOfWeek + 6) % 7;
        var kalenderStart = ersteDatum.AddDays(-startOffset);
        var endOffset = 6 - (((int)letzteDatum.DayOfWeek + 6) % 7);
        var kalenderEnde = letzteDatum.AddDays(endOffset);
        var feiertage = MecklenburgVorpommernFeiertage.GetFeiertage(jahr);

        var wunschTageSet = wunschTage.ToHashSet();
        var nichtVerfuegbareTageSet = nichtVerfuegbareTage.ToHashSet();

        var wochen = new List<WachleiterKalenderWocheEintrag>();
        var aktuelleWoche = new List<WachleiterKalenderTagEintrag>();

        for (var datum = kalenderStart; datum <= kalenderEnde; datum = datum.AddDays(1))
        {
            var istImAktivenMonat = datum.Month == monat && datum.Year == jahr;
            var istFeiertag = feiertage.TryGetValue(datum, out var feiertagsname);
            var istWochenende = datum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

            DienstwunschTagesstatistik? statistik = null;
            GeplanterDienstTag? planung = null;
            string? automatikKonfliktText = null;

            statistikLookup?.TryGetValue(datum, out statistik);
            planLookup?.TryGetValue(datum, out planung);
            automatikKonfliktLookup?.TryGetValue(datum, out automatikKonfliktText);

            var zeigeTageswerte = istImAktivenMonat && !istFeiertag && !istWochenende;

            aktuelleWoche.Add(new WachleiterKalenderTagEintrag
            {
                Datum = datum,
                IstImAktivenMonat = istImAktivenMonat,
                IstHeute = datum == heute,
                IstAlsWunschMarkiert = istImAktivenMonat && !istFeiertag && !istWochenende && wunschTageSet.Contains(datum.Day),
                IstAlsNichtVerfuegbarMarkiert = istImAktivenMonat && !istFeiertag && !istWochenende && nichtVerfuegbareTageSet.Contains(datum.Day),
                IstFeiertag = istImAktivenMonat && istFeiertag,
                IstWochenende = istImAktivenMonat && istWochenende,
                FeiertagsName = istImAktivenMonat && istFeiertag ? feiertagsname! : string.Empty,
                IstAuswaehlbar = istImAktivenMonat && !istFeiertag && !istWochenende,
                AnzahlWuenscheGesamt = zeigeTageswerte ? statistik?.AnzahlWuenscheGesamt ?? 0 : 0,
                AnzahlAerzte = zeigeTageswerte ? statistik?.AnzahlAerzte ?? 0 : 0,
                AnzahlNotfallsanitaeter = zeigeTageswerte ? statistik?.AnzahlNotfallsanitaeter ?? 0 : 0,
                AnzahlGeplanteAerzte = zeigeTageswerte && !string.IsNullOrWhiteSpace(planung?.ArztUserId) ? 1 : 0,
                AnzahlGeplanteNotfallsanitaeter = zeigeTageswerte
                    ? new[] { planung?.Notfallsanitaeter1UserId, planung?.Notfallsanitaeter2UserId }
                        .Count(x => !string.IsNullOrWhiteSpace(x))
                    : 0,
                HatAutomatikKonflikt = zeigeTageswerte && !string.IsNullOrWhiteSpace(automatikKonfliktText),
                AutomatikKonfliktText = zeigeTageswerte ? automatikKonfliktText ?? string.Empty : string.Empty
            });

            if (aktuelleWoche.Count == 7)
            {
                wochen.Add(new WachleiterKalenderWocheEintrag
                {
                    Tage = aktuelleWoche.ToArray()
                });

                aktuelleWoche = new List<WachleiterKalenderTagEintrag>();
            }
        }

        return wochen;
    }
}