using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Planung;

public sealed record ReadAutoplanVertreterPraeferenzenQuery(
    int Mindestanzahl = 2,
    DienstausfallGrundCode? AusfallGrundCode = null,
    DateOnly? BewertungsDatum = null);

public sealed class AutoplanVertreterPraeferenzEintrag
{
    public string UrspruenglichGeplanterUserId { get; init; } = string.Empty;

    public DienstbesetzungsSlotCode BesetzungsSlotCode { get; init; }

    public string VertretungsUserId { get; init; } = string.Empty;

    public DienstausfallGrundCode? AusfallGrundCode { get; init; }

    public int Anzahl { get; init; }

    public int GesamtanzahlFuerUrspruenglichGeplanten { get; init; }

    public decimal GewichteteAnzahl { get; init; }

    public decimal GewichteteGesamtanzahlFuerUrspruenglichGeplanten { get; init; }

    public decimal Anteil { get; init; }

    public DateTimeOffset LetzteVerwendungAm { get; init; }
}

public sealed class ReadAutoplanVertreterPraeferenzenResult
{
    private ReadAutoplanVertreterPraeferenzenResult(
        bool isSuccess,
        string? errorMessage,
        IReadOnlyList<AutoplanVertreterPraeferenzEintrag> eintraege)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Eintraege = eintraege;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<AutoplanVertreterPraeferenzEintrag> Eintraege { get; }

    public static ReadAutoplanVertreterPraeferenzenResult Erfolg(
        IReadOnlyList<AutoplanVertreterPraeferenzEintrag> eintraege)
        => new(true, null, eintraege);

    public static ReadAutoplanVertreterPraeferenzenResult Fehler(string message)
        => new(false, message, Array.Empty<AutoplanVertreterPraeferenzEintrag>());
}

public sealed class ReadAutoplanVertreterPraeferenzenService
{
    private readonly IAutoplanLernereignisRepository _autoplanLernereignisRepository;

    public ReadAutoplanVertreterPraeferenzenService(
        IAutoplanLernereignisRepository autoplanLernereignisRepository)
    {
        ArgumentNullException.ThrowIfNull(autoplanLernereignisRepository);
        _autoplanLernereignisRepository = autoplanLernereignisRepository;
    }

    public async Task<ReadAutoplanVertreterPraeferenzenResult> ExecuteAsync(
        ReadAutoplanVertreterPraeferenzenQuery? query = null,
        CancellationToken cancellationToken = default)
    {
        query ??= new ReadAutoplanVertreterPraeferenzenQuery();

        if (query.Mindestanzahl < 1)
        {
            return ReadAutoplanVertreterPraeferenzenResult.Fehler("Die Mindestanzahl muss mindestens 1 sein.");
        }

        var lernereignisse = await _autoplanLernereignisRepository.GetVertretungsLernereignisseAsync(cancellationToken);

        var relevanteEreignisse = lernereignisse
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.UrspruenglichGeplanterUserId) &&
                !string.IsNullOrWhiteSpace(x.NeueUserId) &&
                !string.Equals(x.UrspruenglichGeplanterUserId, x.NeueUserId, StringComparison.OrdinalIgnoreCase))
            .Where(x => !query.AusfallGrundCode.HasValue || x.AusfallGrundCode == query.AusfallGrundCode)
            .Select(x => new RelevantesVertretungsLernereignis(
                x.UrspruenglichGeplanterUserId!.Trim(),
                x.BesetzungsSlotCode,
                x.NeueUserId!.Trim(),
                x.AusfallGrundCode,
                x.ErfasstAm))
            .ToArray();

        if (relevanteEreignisse.Length == 0)
        {
            return ReadAutoplanVertreterPraeferenzenResult.Erfolg(Array.Empty<AutoplanVertreterPraeferenzEintrag>());
        }

        var gesamtLookup = relevanteEreignisse
            .GroupBy(x => new { x.UrspruenglichGeplanterUserId, x.BesetzungsSlotCode })
            .ToDictionary(
                x => BaueGesamtschluessel(x.Key.UrspruenglichGeplanterUserId, x.Key.BesetzungsSlotCode),
                x => new
                {
                    Anzahl = x.Count(),
                    GewichteteAnzahl = x.Sum(y => BerechneZeitgewicht(y.ErfasstAm, query.BewertungsDatum))
                },
                StringComparer.OrdinalIgnoreCase);

        var eintraege = relevanteEreignisse
            .GroupBy(x => new { x.UrspruenglichGeplanterUserId, x.BesetzungsSlotCode, x.VertretungsUserId })
            .Select(x =>
            {
                var gesamtKey = BaueGesamtschluessel(x.Key.UrspruenglichGeplanterUserId, x.Key.BesetzungsSlotCode);
                var gesamt = gesamtLookup[gesamtKey];

                var anzahl = x.Count();
                var gewichteteAnzahl = x.Sum(y => BerechneZeitgewicht(y.ErfasstAm, query.BewertungsDatum));
                var anteil = gesamt.GewichteteAnzahl <= 0m
                    ? 0m
                    : Math.Round(gewichteteAnzahl / gesamt.GewichteteAnzahl, 4, MidpointRounding.AwayFromZero);

                return new AutoplanVertreterPraeferenzEintrag
                {
                    UrspruenglichGeplanterUserId = x.Key.UrspruenglichGeplanterUserId,
                    BesetzungsSlotCode = x.Key.BesetzungsSlotCode,
                    VertretungsUserId = x.Key.VertretungsUserId,
                    AusfallGrundCode = query.AusfallGrundCode,
                    Anzahl = anzahl,
                    GesamtanzahlFuerUrspruenglichGeplanten = gesamt.Anzahl,
                    GewichteteAnzahl = Math.Round(gewichteteAnzahl, 4, MidpointRounding.AwayFromZero),
                    GewichteteGesamtanzahlFuerUrspruenglichGeplanten = Math.Round(gesamt.GewichteteAnzahl, 4, MidpointRounding.AwayFromZero),
                    Anteil = anteil,
                    LetzteVerwendungAm = x.Max(y => y.ErfasstAm)
                };
            })
            .Where(x => x.Anzahl >= query.Mindestanzahl)
            .OrderBy(x => x.UrspruenglichGeplanterUserId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.BesetzungsSlotCode)
            .ThenByDescending(x => x.GewichteteAnzahl)
            .ThenByDescending(x => x.Anteil)
            .ThenByDescending(x => x.LetzteVerwendungAm)
            .ThenBy(x => x.VertretungsUserId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ReadAutoplanVertreterPraeferenzenResult.Erfolg(eintraege);
    }

    private static decimal BerechneZeitgewicht(
        DateTimeOffset erfasstAm,
        DateOnly? bewertungsDatum)
    {
        if (!bewertungsDatum.HasValue)
        {
            return 1m;
        }

        var alterInTagen = (bewertungsDatum.Value.ToDateTime(TimeOnly.MinValue) - erfasstAm.Date).TotalDays;

        if (alterInTagen <= 31)
        {
            return 1m;
        }

        if (alterInTagen <= 92)
        {
            return 0.75m;
        }

        if (alterInTagen <= 183)
        {
            return 0.5m;
        }

        return 0.25m;
    }

    private static string BaueGesamtschluessel(
        string urspruenglichGeplanterUserId,
        DienstbesetzungsSlotCode besetzungsSlotCode)
    {
        return $"{urspruenglichGeplanterUserId}|{(int)besetzungsSlotCode}";
    }

    private sealed record RelevantesVertretungsLernereignis(
        string UrspruenglichGeplanterUserId,
        DienstbesetzungsSlotCode BesetzungsSlotCode,
        string VertretungsUserId,
        DienstausfallGrundCode? AusfallGrundCode,
        DateTimeOffset ErfasstAm);
}