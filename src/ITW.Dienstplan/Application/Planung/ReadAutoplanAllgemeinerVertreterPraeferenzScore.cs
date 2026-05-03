using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Planung;

public sealed record ReadAutoplanAllgemeinerVertreterPraeferenzScoreQuery(
    DienstbesetzungsSlotCode BesetzungsSlotCode,
    IReadOnlyCollection<string> KandidatenUserIds,
    int Mindestanzahl = 3);

public sealed class AutoplanAllgemeinerVertreterPraeferenzScoreEintrag
{
    public string KandidatUserId { get; init; } = string.Empty;

    public bool HatPraeferenz { get; init; }

    public int Anzahl { get; init; }

    public int GesamtanzahlImSlot { get; init; }

    public decimal Anteil { get; init; }

    public decimal LernBonus { get; init; }
}

public sealed class ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult
{
    private ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult(
        bool isSuccess,
        string? errorMessage,
        IReadOnlyList<AutoplanAllgemeinerVertreterPraeferenzScoreEintrag> eintraege)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Eintraege = eintraege;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<AutoplanAllgemeinerVertreterPraeferenzScoreEintrag> Eintraege { get; }

    public static ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult Erfolg(
        IReadOnlyList<AutoplanAllgemeinerVertreterPraeferenzScoreEintrag> eintraege)
        => new(true, null, eintraege);

    public static ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult Fehler(string message)
        => new(false, message, Array.Empty<AutoplanAllgemeinerVertreterPraeferenzScoreEintrag>());
}

public sealed class ReadAutoplanAllgemeinerVertreterPraeferenzScoreService
{
    private readonly IAutoplanLernereignisRepository _autoplanLernereignisRepository;

    public ReadAutoplanAllgemeinerVertreterPraeferenzScoreService(
        IAutoplanLernereignisRepository autoplanLernereignisRepository)
    {
        _autoplanLernereignisRepository = autoplanLernereignisRepository
            ?? throw new ArgumentNullException(nameof(autoplanLernereignisRepository));
    }

    public async Task<ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult> ExecuteAsync(
        ReadAutoplanAllgemeinerVertreterPraeferenzScoreQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (query.KandidatenUserIds is null || query.KandidatenUserIds.Count == 0)
        {
            return ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult.Fehler(
                "Es muss mindestens ein Kandidat angegeben werden.");
        }

        if (query.Mindestanzahl < 1)
        {
            return ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult.Fehler(
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
            return ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult.Fehler(
                "Es muss mindestens ein gültiger Kandidat angegeben werden.");
        }

        var lernereignisse = await _autoplanLernereignisRepository.GetVertretungsLernereignisseAsync(cancellationToken);

        var relevanteSlotEreignisse = lernereignisse
            .Where(x =>
                x.BesetzungsSlotCode == query.BesetzungsSlotCode &&
                !string.IsNullOrWhiteSpace(x.NeueUserId))
            .Select(x => x.NeueUserId!.Trim())
            .ToArray();

        var gesamtanzahlImSlot = relevanteSlotEreignisse.Length;

        var eintraege = kandidatenUserIds
            .Select(kandidatUserId =>
            {
                var anzahl = relevanteSlotEreignisse.Count(x =>
                    string.Equals(x, kandidatUserId, StringComparison.OrdinalIgnoreCase));

                var hatPraeferenz = anzahl >= query.Mindestanzahl;

                var anteil = gesamtanzahlImSlot == 0
                    ? 0m
                    : Math.Round((decimal)anzahl / gesamtanzahlImSlot, 4, MidpointRounding.AwayFromZero);

                return new AutoplanAllgemeinerVertreterPraeferenzScoreEintrag
                {
                    KandidatUserId = kandidatUserId,
                    HatPraeferenz = hatPraeferenz,
                    Anzahl = anzahl,
                    GesamtanzahlImSlot = gesamtanzahlImSlot,
                    Anteil = anteil,
                    LernBonus = hatPraeferenz
                        ? BerechneLernBonus(anzahl, anteil)
                        : 0m
                };
            })
            .OrderByDescending(x => x.LernBonus)
            .ThenByDescending(x => x.Anzahl)
            .ThenByDescending(x => x.Anteil)
            .ThenBy(x => x.KandidatUserId)
            .ToArray();

        return ReadAutoplanAllgemeinerVertreterPraeferenzScoreResult.Erfolg(eintraege);
    }

    private static decimal BerechneLernBonus(int anzahl, decimal anteil)
    {
        if (anzahl <= 0 || anteil <= 0m)
        {
            return 0m;
        }

        var bonus = (anteil * 6m) + Math.Min(anzahl, 4) - 1m;

        return Math.Min(8m, Math.Round(bonus, 2, MidpointRounding.AwayFromZero));
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