using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Planung;

public sealed record ReadAutoplanTeamkombinationsScoreQuery(
    DienstbesetzungsSlotCode BesetzungsSlotCode,
    IReadOnlyCollection<string> KandidatenUserIds,
    string? KontextArztUserId = null,
    string? KontextNotfallsanitaeter1UserId = null,
    string? KontextNotfallsanitaeter2UserId = null,
    DateOnly? BewertungsDatum = null,
    int Mindestanzahl = 2,
    int MindestanzahlNegativeKorrekturen = 2);

public sealed class AutoplanTeamkombinationsScoreEintrag
{
    public string KandidatUserId { get; init; } = string.Empty;

    public bool HatTeamPraeferenz { get; init; }

    public bool HatNegativesKorrekturmuster { get; init; }

    public int AnzahlPassenderKombinationen { get; init; }

    public int AnzahlPassenderKorrekturen { get; init; }

    public int GesamtanzahlKandidatImSlot { get; init; }

    public decimal Anteil { get; init; }

    public decimal NegativAnteil { get; init; }

    public decimal GewichtetePassendeKombinationen { get; init; }

    public decimal GewichtetePassendeKorrekturen { get; init; }

    public decimal LernBonus { get; init; }
}

public sealed class ReadAutoplanTeamkombinationsScoreResult
{
    private ReadAutoplanTeamkombinationsScoreResult(
        bool isSuccess,
        string? errorMessage,
        IReadOnlyList<AutoplanTeamkombinationsScoreEintrag> eintraege)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Eintraege = eintraege;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<AutoplanTeamkombinationsScoreEintrag> Eintraege { get; }

    public static ReadAutoplanTeamkombinationsScoreResult Erfolg(
        IReadOnlyList<AutoplanTeamkombinationsScoreEintrag> eintraege)
        => new(true, null, eintraege);

    public static ReadAutoplanTeamkombinationsScoreResult Fehler(string message)
        => new(false, message, Array.Empty<AutoplanTeamkombinationsScoreEintrag>());
}

public sealed class ReadAutoplanTeamkombinationsScoreService
{
    private readonly IAutoplanLernereignisRepository _autoplanLernereignisRepository;
    private IReadOnlyList<AutoplanLernereignis>? _grundbesetzungsLernereignisseCache;

    public ReadAutoplanTeamkombinationsScoreService(
        IAutoplanLernereignisRepository autoplanLernereignisRepository)
    {
        ArgumentNullException.ThrowIfNull(autoplanLernereignisRepository);
        _autoplanLernereignisRepository = autoplanLernereignisRepository;
    }

    public async Task<ReadAutoplanTeamkombinationsScoreResult> ExecuteAsync(
        ReadAutoplanTeamkombinationsScoreQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.KandidatenUserIds is null || query.KandidatenUserIds.Count == 0)
        {
            return ReadAutoplanTeamkombinationsScoreResult.Fehler(
                "Es muss mindestens ein Kandidat angegeben werden.");
        }

        if (query.Mindestanzahl < 1)
        {
            return ReadAutoplanTeamkombinationsScoreResult.Fehler(
                "Die Mindestanzahl muss mindestens 1 sein.");
        }

        if (query.MindestanzahlNegativeKorrekturen < 1)
        {
            return ReadAutoplanTeamkombinationsScoreResult.Fehler(
                "Die Mindestanzahl negativer Korrekturen muss mindestens 1 sein.");
        }

        var kandidatenUserIds = query.KandidatenUserIds
            .Select(NormalisiereUserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToArray();

        if (kandidatenUserIds.Length == 0)
        {
            return ReadAutoplanTeamkombinationsScoreResult.Fehler(
                "Es muss mindestens ein gültiger Kandidat angegeben werden.");
        }

        var kontext = ErzeugeRelevantenKontext(query);

        if (kontext.Count == 0)
        {
            var leereEintraege = kandidatenUserIds
                .OrderBy(x => x)
                .Select(x => new AutoplanTeamkombinationsScoreEintrag
                {
                    KandidatUserId = x,
                    HatTeamPraeferenz = false,
                    HatNegativesKorrekturmuster = false,
                    AnzahlPassenderKombinationen = 0,
                    AnzahlPassenderKorrekturen = 0,
                    GesamtanzahlKandidatImSlot = 0,
                    Anteil = 0m,
                    NegativAnteil = 0m,
                    GewichtetePassendeKombinationen = 0m,
                    GewichtetePassendeKorrekturen = 0m,
                    LernBonus = 0m
                })
                .ToArray();

            return ReadAutoplanTeamkombinationsScoreResult.Erfolg(leereEintraege);
        }

        var lernereignisse = await LadeGrundbesetzungsLernereignisseAsync(cancellationToken);

        var relevanteEreignisse = lernereignisse
            .Where(x =>
                x.BesetzungsSlotCode == query.BesetzungsSlotCode &&
                (
                    (!string.IsNullOrWhiteSpace(x.NeueUserId) && kandidatenUserIds.Contains(x.NeueUserId!, StringComparer.OrdinalIgnoreCase)) ||
                    (x.EreignisTypCode == AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert &&
                     !string.IsNullOrWhiteSpace(x.VorherigeUserId) &&
                     kandidatenUserIds.Contains(x.VorherigeUserId!, StringComparer.OrdinalIgnoreCase))
                ))
            .ToArray();

        var eintraege = kandidatenUserIds
            .Select(kandidatUserId =>
            {
                var positiveEreignisse = relevanteEreignisse
                    .Where(x => string.Equals(x.NeueUserId, kandidatUserId, StringComparison.OrdinalIgnoreCase))
                    .Where(x =>
                        x.EreignisTypCode == AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert ||
                        x.EreignisTypCode == AutoplanLernereignisTypCode.AutoplanVorschlagBestaetigt)
                    .ToArray();

                var negativeEreignisse = relevanteEreignisse
                    .Where(x => x.EreignisTypCode == AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert)
                    .Where(x => string.Equals(x.VorherigeUserId, kandidatUserId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                var passendePositiveEreignisse = positiveEreignisse
                    .Where(x => PasstZumKontext(x, kontext))
                    .ToArray();

                var passendeNegativeEreignisse = negativeEreignisse
                    .Where(x => PasstZumKontext(x, kontext))
                    .ToArray();

                var gesamtanzahl = positiveEreignisse.Length;
                var anzahlPassenderKombinationen = passendePositiveEreignisse.Length;
                var anzahlPassenderKorrekturen = passendeNegativeEreignisse.Length;

                var gewichteteGesamtanzahl = positiveEreignisse.Sum(x => BerechnePositivesGesamtgewicht(x, query.BewertungsDatum));
                var gewichtetePassendeKombinationen = passendePositiveEreignisse.Sum(x => BerechnePositivesGesamtgewicht(x, query.BewertungsDatum));
                var gewichteteGesamtanzahlNegative = negativeEreignisse.Sum(x => BerechneZeitgewicht(x.ErfasstAm, query.BewertungsDatum));
                var gewichtetePassendeKorrekturen = passendeNegativeEreignisse.Sum(x => BerechneZeitgewicht(x.ErfasstAm, query.BewertungsDatum));

                var anteil = gewichteteGesamtanzahl <= 0m
                    ? 0m
                    : Math.Round(gewichtetePassendeKombinationen / gewichteteGesamtanzahl, 4, MidpointRounding.AwayFromZero);

                var negativAnteil = gewichteteGesamtanzahlNegative <= 0m
                    ? 0m
                    : Math.Round(gewichtetePassendeKorrekturen / gewichteteGesamtanzahlNegative, 4, MidpointRounding.AwayFromZero);

                var hatTeamPraeferenz = anzahlPassenderKombinationen >= query.Mindestanzahl;
                var hatNegativesKorrekturmuster = anzahlPassenderKorrekturen >= query.MindestanzahlNegativeKorrekturen;

                var positiverLernbonus = hatTeamPraeferenz
                    ? BerechnePositivenLernbonus(gewichtetePassendeKombinationen, anteil, kontext.Count)
                    : 0m;

                var negativerLernmalus = hatNegativesKorrekturmuster
                    ? BerechneNegativenLernmalus(gewichtetePassendeKorrekturen, negativAnteil, kontext.Count)
                    : 0m;

                var lernbonus = Math.Round(positiverLernbonus - negativerLernmalus, 2, MidpointRounding.AwayFromZero);

                return new AutoplanTeamkombinationsScoreEintrag
                {
                    KandidatUserId = kandidatUserId,
                    HatTeamPraeferenz = hatTeamPraeferenz,
                    HatNegativesKorrekturmuster = hatNegativesKorrekturmuster,
                    AnzahlPassenderKombinationen = anzahlPassenderKombinationen,
                    AnzahlPassenderKorrekturen = anzahlPassenderKorrekturen,
                    GesamtanzahlKandidatImSlot = gesamtanzahl,
                    Anteil = anteil,
                    NegativAnteil = negativAnteil,
                    GewichtetePassendeKombinationen = Math.Round(gewichtetePassendeKombinationen, 4, MidpointRounding.AwayFromZero),
                    GewichtetePassendeKorrekturen = Math.Round(gewichtetePassendeKorrekturen, 4, MidpointRounding.AwayFromZero),
                    LernBonus = Math.Max(-15m, Math.Min(15m, lernbonus))
                };
            })
            .OrderByDescending(x => x.LernBonus)
            .ThenByDescending(x => x.AnzahlPassenderKombinationen)
            .ThenBy(x => x.AnzahlPassenderKorrekturen)
            .ThenByDescending(x => x.Anteil)
            .ThenBy(x => x.KandidatUserId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ReadAutoplanTeamkombinationsScoreResult.Erfolg(eintraege);
    }

    private static Dictionary<string, string> ErzeugeRelevantenKontext(
        ReadAutoplanTeamkombinationsScoreQuery query)
    {
        var kontext = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        switch (query.BesetzungsSlotCode)
        {
            case DienstbesetzungsSlotCode.Arzt:
                FuegeKontextHinzu(kontext, "Notfallsanitaeter1", query.KontextNotfallsanitaeter1UserId);
                FuegeKontextHinzu(kontext, "Notfallsanitaeter2", query.KontextNotfallsanitaeter2UserId);
                break;

            case DienstbesetzungsSlotCode.Notfallsanitaeter1:
                FuegeKontextHinzu(kontext, "Arzt", query.KontextArztUserId);
                FuegeKontextHinzu(kontext, "Notfallsanitaeter2", query.KontextNotfallsanitaeter2UserId);
                break;

            case DienstbesetzungsSlotCode.Notfallsanitaeter2:
                FuegeKontextHinzu(kontext, "Arzt", query.KontextArztUserId);
                FuegeKontextHinzu(kontext, "Notfallsanitaeter1", query.KontextNotfallsanitaeter1UserId);
                break;
        }

        return kontext;
    }

    private static bool PasstZumKontext(
        AutoplanLernereignis lernereignis,
        IReadOnlyDictionary<string, string> kontext)
    {
        foreach (var eintrag in kontext)
        {
            var kontextUserIdImEreignis = eintrag.Key switch
            {
                "Arzt" => lernereignis.KontextArztUserId,
                "Notfallsanitaeter1" => lernereignis.KontextNotfallsanitaeter1UserId,
                "Notfallsanitaeter2" => lernereignis.KontextNotfallsanitaeter2UserId,
                _ => null
            };

            if (!string.Equals(kontextUserIdImEreignis, eintrag.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<IReadOnlyList<AutoplanLernereignis>> LadeGrundbesetzungsLernereignisseAsync(
        CancellationToken cancellationToken)
    {
        if (_grundbesetzungsLernereignisseCache is not null)
        {
            return _grundbesetzungsLernereignisseCache;
        }

        _grundbesetzungsLernereignisseCache =
            await _autoplanLernereignisRepository.GetGrundbesetzungsLernereignisseAsync(cancellationToken);

        return _grundbesetzungsLernereignisseCache;
    }

    private static decimal BerechnePositivesGesamtgewicht(
        AutoplanLernereignis lernereignis,
        DateOnly? bewertungsDatum)
    {
        return BerechneZeitgewicht(lernereignis.ErfasstAm, bewertungsDatum) *
               BerechneEreignisTypGewicht(lernereignis.EreignisTypCode);
    }

    private static decimal BerechneEreignisTypGewicht(
        AutoplanLernereignisTypCode ereignisTypCode)
    {
        return ereignisTypCode == AutoplanLernereignisTypCode.AutoplanVorschlagBestaetigt
            ? 0.75m
            : 1m;
    }

    private static decimal BerechnePositivenLernbonus(
        decimal gewichtetePassendeKombinationen,
        decimal anteil,
        int kontextTrefferTiefe)
    {
        if (gewichtetePassendeKombinationen <= 0m || anteil <= 0m || kontextTrefferTiefe <= 0)
        {
            return 0m;
        }

        var bonus = (anteil * 8m)
                    + Math.Min(gewichtetePassendeKombinationen, 5m)
                    + (kontextTrefferTiefe - 1m);

        return Math.Min(15m, Math.Round(bonus, 2, MidpointRounding.AwayFromZero));
    }

    private static decimal BerechneNegativenLernmalus(
        decimal gewichtetePassendeKorrekturen,
        decimal negativAnteil,
        int kontextTrefferTiefe)
    {
        if (gewichtetePassendeKorrekturen <= 0m || negativAnteil <= 0m || kontextTrefferTiefe <= 0)
        {
            return 0m;
        }

        var malus = (negativAnteil * 8m)
                    + Math.Min(gewichtetePassendeKorrekturen, 5m)
                    + (kontextTrefferTiefe - 1m);

        return Math.Min(15m, Math.Round(malus, 2, MidpointRounding.AwayFromZero));
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

    private static void FuegeKontextHinzu(
        IDictionary<string, string> kontext,
        string schluessel,
        string? userId)
    {
        var normalisierteUserId = NormalisiereUserId(userId);

        if (!string.IsNullOrWhiteSpace(normalisierteUserId))
        {
            kontext[schluessel] = normalisierteUserId;
        }
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