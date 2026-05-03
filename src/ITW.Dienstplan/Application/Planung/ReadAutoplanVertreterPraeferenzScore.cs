using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Planung;

public sealed record ReadAutoplanVertreterPraeferenzScoreQuery(
    string UrspruenglichGeplanterUserId,
    DienstbesetzungsSlotCode BesetzungsSlotCode,
    IReadOnlyCollection<string> KandidatenUserIds,
    int Mindestanzahl = 2,
    DienstausfallGrundCode? AusfallGrundCode = null,
    DateOnly? BewertungsDatum = null);

public sealed class AutoplanVertreterPraeferenzScoreEintrag
{
    public string KandidatUserId { get; init; } = string.Empty;

    public bool HatPraeferenz { get; init; }

    public bool HatAusfallgrundPraeferenz { get; init; }

    public DienstausfallGrundCode? AusfallGrundCode { get; init; }

    public int Anzahl { get; init; }

    public int GesamtanzahlFuerUrspruenglichGeplanten { get; init; }

    public decimal Anteil { get; init; }

    public decimal GewichteteAnzahl { get; init; }

    public decimal LernBonus { get; init; }
}

public sealed class ReadAutoplanVertreterPraeferenzScoreResult
{
    private ReadAutoplanVertreterPraeferenzScoreResult(
        bool isSuccess,
        string? errorMessage,
        IReadOnlyList<AutoplanVertreterPraeferenzScoreEintrag> eintraege)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Eintraege = eintraege;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<AutoplanVertreterPraeferenzScoreEintrag> Eintraege { get; }

    public static ReadAutoplanVertreterPraeferenzScoreResult Erfolg(
        IReadOnlyList<AutoplanVertreterPraeferenzScoreEintrag> eintraege)
        => new(true, null, eintraege);

    public static ReadAutoplanVertreterPraeferenzScoreResult Fehler(string message)
        => new(false, message, Array.Empty<AutoplanVertreterPraeferenzScoreEintrag>());
}

public sealed class ReadAutoplanVertreterPraeferenzScoreService
{
    private readonly ReadAutoplanVertreterPraeferenzenService _readAutoplanVertreterPraeferenzenService;

    public ReadAutoplanVertreterPraeferenzScoreService(
        ReadAutoplanVertreterPraeferenzenService readAutoplanVertreterPraeferenzenService)
    {
        _readAutoplanVertreterPraeferenzenService = readAutoplanVertreterPraeferenzenService
            ?? throw new ArgumentNullException(nameof(readAutoplanVertreterPraeferenzenService));
    }

    public async Task<ReadAutoplanVertreterPraeferenzScoreResult> ExecuteAsync(
        ReadAutoplanVertreterPraeferenzScoreQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var urspruenglichGeplanterUserId = NormalisiereUserId(query.UrspruenglichGeplanterUserId);

        if (string.IsNullOrWhiteSpace(urspruenglichGeplanterUserId))
        {
            return ReadAutoplanVertreterPraeferenzScoreResult.Fehler(
                "Die ursprünglich geplante UserId ist erforderlich.");
        }

        if (query.KandidatenUserIds is null || query.KandidatenUserIds.Count == 0)
        {
            return ReadAutoplanVertreterPraeferenzScoreResult.Fehler(
                "Es muss mindestens ein Kandidat angegeben werden.");
        }

        if (query.Mindestanzahl < 1)
        {
            return ReadAutoplanVertreterPraeferenzScoreResult.Fehler(
                "Die Mindestanzahl muss mindestens 1 sein.");
        }

        var kandidatenUserIds = query.KandidatenUserIds
            .Select(NormalisiereUserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToArray();

        if (kandidatenUserIds.Length == 0)
        {
            return ReadAutoplanVertreterPraeferenzScoreResult.Fehler(
                "Es muss mindestens ein gültiger Kandidat angegeben werden.");
        }

        var exakterAusfallgrundLookup = query.AusfallGrundCode.HasValue
            ? await LadePraeferenzLookupAsync(
                urspruenglichGeplanterUserId,
                query.BesetzungsSlotCode,
                kandidatenUserIds,
                query.Mindestanzahl,
                query.AusfallGrundCode,
                query.BewertungsDatum,
                cancellationToken)
            : new Dictionary<string, AutoplanVertreterPraeferenzEintrag>(StringComparer.OrdinalIgnoreCase);

        var verwendeAusfallgrundPraeferenzen = exakterAusfallgrundLookup.Count > 0;

        var praeferenzLookup = verwendeAusfallgrundPraeferenzen
            ? exakterAusfallgrundLookup
            : await LadePraeferenzLookupAsync(
                urspruenglichGeplanterUserId,
                query.BesetzungsSlotCode,
                kandidatenUserIds,
                query.Mindestanzahl,
                null,
                query.BewertungsDatum,
                cancellationToken);

        var eintraege = kandidatenUserIds
            .Select(kandidatUserId =>
            {
                if (!praeferenzLookup.TryGetValue(kandidatUserId, out var praeferenz))
                {
                    return new AutoplanVertreterPraeferenzScoreEintrag
                    {
                        KandidatUserId = kandidatUserId,
                        HatPraeferenz = false,
                        HatAusfallgrundPraeferenz = false,
                        AusfallGrundCode = null,
                        Anzahl = 0,
                        GesamtanzahlFuerUrspruenglichGeplanten = 0,
                        Anteil = 0m,
                        GewichteteAnzahl = 0m,
                        LernBonus = 0m
                    };
                }

                var hatAusfallgrundPraeferenz =
                    verwendeAusfallgrundPraeferenzen &&
                    query.AusfallGrundCode.HasValue &&
                    praeferenz.AusfallGrundCode == query.AusfallGrundCode;

                return new AutoplanVertreterPraeferenzScoreEintrag
                {
                    KandidatUserId = kandidatUserId,
                    HatPraeferenz = true,
                    HatAusfallgrundPraeferenz = hatAusfallgrundPraeferenz,
                    AusfallGrundCode = hatAusfallgrundPraeferenz ? query.AusfallGrundCode : null,
                    Anzahl = praeferenz.Anzahl,
                    GesamtanzahlFuerUrspruenglichGeplanten = praeferenz.GesamtanzahlFuerUrspruenglichGeplanten,
                    Anteil = praeferenz.Anteil,
                    GewichteteAnzahl = praeferenz.GewichteteAnzahl,
                    LernBonus = BerechneLernBonus(
                        praeferenz.GewichteteAnzahl,
                        praeferenz.Anteil,
                        hatAusfallgrundPraeferenz)
                };
            })
            .OrderByDescending(x => x.LernBonus)
            .ThenByDescending(x => x.Anzahl)
            .ThenByDescending(x => x.Anteil)
            .ThenBy(x => x.KandidatUserId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ReadAutoplanVertreterPraeferenzScoreResult.Erfolg(eintraege);
    }

    private async Task<Dictionary<string, AutoplanVertreterPraeferenzEintrag>> LadePraeferenzLookupAsync(
        string urspruenglichGeplanterUserId,
        DienstbesetzungsSlotCode besetzungsSlotCode,
        IReadOnlyCollection<string> kandidatenUserIds,
        int mindestanzahl,
        DienstausfallGrundCode? ausfallGrundCode,
        DateOnly? bewertungsDatum,
        CancellationToken cancellationToken)
    {
        var praeferenzenResult = await _readAutoplanVertreterPraeferenzenService.ExecuteAsync(
            new ReadAutoplanVertreterPraeferenzenQuery(
                Mindestanzahl: mindestanzahl,
                AusfallGrundCode: ausfallGrundCode,
                BewertungsDatum: bewertungsDatum),
            cancellationToken);

        if (!praeferenzenResult.IsSuccess)
        {
            return new Dictionary<string, AutoplanVertreterPraeferenzEintrag>(StringComparer.OrdinalIgnoreCase);
        }

        return praeferenzenResult.Eintraege
            .Where(x =>
                string.Equals(x.UrspruenglichGeplanterUserId, urspruenglichGeplanterUserId, StringComparison.OrdinalIgnoreCase) &&
                x.BesetzungsSlotCode == besetzungsSlotCode &&
                kandidatenUserIds.Contains(x.VertretungsUserId, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(
                x => x.VertretungsUserId,
                x => x,
                StringComparer.OrdinalIgnoreCase);
    }

    private static decimal BerechneLernBonus(
        decimal gewichteteAnzahl,
        decimal anteil,
        bool hatAusfallgrundPraeferenz)
    {
        if (gewichteteAnzahl <= 0m || anteil <= 0m)
        {
            return 0m;
        }

        var bonus = (anteil * 10m) + Math.Min(gewichteteAnzahl, 5m) - 1m;

        if (hatAusfallgrundPraeferenz)
        {
            bonus += 2m;
        }

        return Math.Min(15m, Math.Round(bonus, 2, MidpointRounding.AwayFromZero));
    }

    private static string? NormalisiereUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return userId.Trim();
    }
}