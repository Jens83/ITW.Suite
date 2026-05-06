using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Planung;

public sealed record ReadAutoplanSlotPraeferenzQuery(
    DienstbesetzungsSlotCode BesetzungsSlotCode,
    IReadOnlyCollection<string> KandidatenUserIds,
    DateOnly? BewertungsDatum = null,
    int Mindestanzahl = 2);

public sealed class AutoplanSlotPraeferenzEintrag
{
    public string KandidatUserId { get; init; } = string.Empty;

    public int Anzahl { get; init; }

    public decimal GewichteteAnzahl { get; init; }

    public decimal PraeferenzBonus { get; init; }
}

public sealed class ReadAutoplanSlotPraeferenzResult
{
    private ReadAutoplanSlotPraeferenzResult(
        bool isSuccess,
        string? errorMessage,
        IReadOnlyList<AutoplanSlotPraeferenzEintrag> eintraege)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Eintraege = eintraege;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<AutoplanSlotPraeferenzEintrag> Eintraege { get; }

    public static ReadAutoplanSlotPraeferenzResult Erfolg(
        IReadOnlyList<AutoplanSlotPraeferenzEintrag> eintraege)
        => new(true, null, eintraege);

    public static ReadAutoplanSlotPraeferenzResult Fehler(string message)
        => new(false, message, Array.Empty<AutoplanSlotPraeferenzEintrag>());
}

/// <summary>
/// Berechnet für jeden Kandidaten einen individuellen Präferenzbonus basierend darauf,
/// wie häufig er historisch für den angegebenen Besetzungsslot gewählt wurde.
/// Wird als Tiebreaker eingesetzt wenn noch kein Team-Kontext vorhanden ist.
/// </summary>
public sealed class ReadAutoplanSlotPraeferenzService
{
    private readonly IAutoplanLernereignisRepository _autoplanLernereignisRepository;
    private IReadOnlyList<AutoplanLernereignis>? _cache;

    public ReadAutoplanSlotPraeferenzService(
        IAutoplanLernereignisRepository autoplanLernereignisRepository)
    {
        ArgumentNullException.ThrowIfNull(autoplanLernereignisRepository);
        _autoplanLernereignisRepository = autoplanLernereignisRepository;
    }

    public async Task<ReadAutoplanSlotPraeferenzResult> ExecuteAsync(
        ReadAutoplanSlotPraeferenzQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var kandidatenUserIds = query.KandidatenUserIds
            .Select(NormalisiereUserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToArray();

        if (kandidatenUserIds.Length == 0)
        {
            return ReadAutoplanSlotPraeferenzResult.Fehler(
                "Es muss mindestens ein gültiger Kandidat angegeben werden.");
        }

        var lernereignisse = await LadeGrundbesetzungsLernereignisseAsync(cancellationToken);

        var relevanteEreignisse = lernereignisse
            .Where(x =>
                x.BesetzungsSlotCode == query.BesetzungsSlotCode &&
                !string.IsNullOrWhiteSpace(x.NeueUserId) &&
                (x.EreignisTypCode == AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert ||
                 x.EreignisTypCode == AutoplanLernereignisTypCode.AutoplanVorschlagBestaetigt))
            .ToArray();

        var eintraege = kandidatenUserIds
            .Select(kandidatUserId =>
            {
                var eigeneEreignisse = relevanteEreignisse
                    .Where(x => string.Equals(x.NeueUserId, kandidatUserId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                var anzahl = eigeneEreignisse.Length;
                var gewichteteAnzahl = eigeneEreignisse.Sum(x =>
                    BerechneZeitgewicht(x.ErfasstAm, query.BewertungsDatum) *
                    BerechneEreignisTypGewicht(x.EreignisTypCode));

                var praeferenzBonus = anzahl >= query.Mindestanzahl
                    ? BerechnePraeferenzBonus(gewichteteAnzahl)
                    : 0m;

                return new AutoplanSlotPraeferenzEintrag
                {
                    KandidatUserId = kandidatUserId,
                    Anzahl = anzahl,
                    GewichteteAnzahl = Math.Round(gewichteteAnzahl, 4, MidpointRounding.AwayFromZero),
                    PraeferenzBonus = praeferenzBonus
                };
            })
            .OrderByDescending(x => x.PraeferenzBonus)
            .ThenByDescending(x => x.GewichteteAnzahl)
            .ThenBy(x => x.KandidatUserId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ReadAutoplanSlotPraeferenzResult.Erfolg(eintraege);
    }

    private async Task<IReadOnlyList<AutoplanLernereignis>> LadeGrundbesetzungsLernereignisseAsync(
        CancellationToken cancellationToken)
    {
        if (_cache is not null)
        {
            return _cache;
        }

        _cache = await _autoplanLernereignisRepository.GetGrundbesetzungsLernereignisseAsync(cancellationToken);
        return _cache;
    }

    // Bewusst klein gehalten (max 6), damit der Bonus keine Wunsch-Prioritäten überstimmt.
    // Logarithmisch: 2 Ereignisse → 0.5, 10 → 2.5, 20 → ~4.0, 30+ → max 6.0
    private static decimal BerechnePraeferenzBonus(decimal gewichteteAnzahl)
        => Math.Min(6m, Math.Round(gewichteteAnzahl * 0.3m, 2, MidpointRounding.AwayFromZero));

    // Manuelle Entscheidungen des Wachleiters zählen mehr als bestätigte Autoplan-Vorschläge
    private static decimal BerechneEreignisTypGewicht(AutoplanLernereignisTypCode ereignisTypCode)
        => ereignisTypCode == AutoplanLernereignisTypCode.AutoplanVorschlagBestaetigt ? 0.75m : 1m;

    private static decimal BerechneZeitgewicht(DateTimeOffset erfasstAm, DateOnly? bewertungsDatum)
    {
        if (!bewertungsDatum.HasValue)
        {
            return 1m;
        }

        var alterInTagen = (bewertungsDatum.Value.ToDateTime(TimeOnly.MinValue) - erfasstAm.Date).TotalDays;

        if (alterInTagen <= 31)  return 1m;
        if (alterInTagen <= 92)  return 0.75m;
        if (alterInTagen <= 183) return 0.5m;
        return 0.25m;
    }

    private static string? NormalisiereUserId(string? userId)
        => string.IsNullOrWhiteSpace(userId) ? null : userId.Trim();
}
