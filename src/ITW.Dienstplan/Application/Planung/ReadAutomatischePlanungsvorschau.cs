using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Kalender;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Domain.Personnel.Enums;

namespace ITW.Dienstplan.Application.Planung;

public enum AutomatischePlanungKonfliktArt
{
    Unbekannt = 0,
    MindestbesetzungNichtErreichbar = 1,
    MehrPflichtfaelleAlsFreieSlots = 2,
    FreelancerSollNichtErreichbar = 3,
    FreelancerDienstNichtKonfliktfreiVerteilbar = 4,
    BestehendePlanungKollidiertMitUrlaub = 5
}

public enum AutomatischePlanungZuweisungsArt
{
    Unbekannt = 0,
    BestehendePlanung = 1,
    PflichtfallFreelancer = 2,
    FreelancerNachTageskritik = 3,
    HonorarkraftWunsch = 4,
    MitarbeiterWunsch = 5,
    MitarbeiterWunschblock = 6,
    MitarbeiterLueckenfueller = 7
}

public enum AutomatischePlanungAenderungsArt
{
    Unbekannt = 0,
    Unveraendert = 1,
    NeuGesetzt = 2,
    Ersetzt = 3,
    WirdOffen = 4,
    BleibtOffen = 5
}

public sealed class AutomatischePlanungKonfliktEintrag
{
    public AutomatischePlanungKonfliktArt Art { get; init; }

    public string Nachricht { get; init; } = string.Empty;
}

public sealed class AutomatischePlanungSlotEntscheidung
{
    public string SlotBezeichnung { get; init; } = string.Empty;

    public string? UserId { get; init; }

    public string? VorherigerUserId { get; init; }

    public AutomatischePlanungZuweisungsArt Art { get; init; }

    public AutomatischePlanungAenderungsArt AenderungsArt { get; init; }

    public string Nachricht { get; init; } = string.Empty;
}

public sealed class AutomatischePlanungsvorschauTag
{
    public DateOnly Datum { get; init; }

    public string? ArztUserId { get; init; }

    public string? Notfallsanitaeter1UserId { get; init; }

    public string? Notfallsanitaeter2UserId { get; init; }

    public bool HatKonflikt { get; init; }

    public string KonfliktText { get; init; } = string.Empty;

    public IReadOnlyList<AutomatischePlanungKonfliktEintrag> Konflikte { get; init; }
        = Array.Empty<AutomatischePlanungKonfliktEintrag>();

    public IReadOnlyList<AutomatischePlanungSlotEntscheidung> SlotEntscheidungen { get; init; }
        = Array.Empty<AutomatischePlanungSlotEntscheidung>();
}

public sealed class ReadAutomatischePlanungsvorschauResult
{
    private ReadAutomatischePlanungsvorschauResult(
        bool isSuccess,
        string? errorMessage,
        Guid periodeId,
        int jahr,
        int monat,
        IReadOnlyList<AutomatischePlanungsvorschauTag> tage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        PeriodeId = periodeId;
        Jahr = jahr;
        Monat = monat;
        Tage = tage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public Guid PeriodeId { get; }

    public int Jahr { get; }

    public int Monat { get; }

    public IReadOnlyList<AutomatischePlanungsvorschauTag> Tage { get; }

    public static ReadAutomatischePlanungsvorschauResult Erfolg(
        Guid periodeId,
        int jahr,
        int monat,
        IReadOnlyList<AutomatischePlanungsvorschauTag> tage)
        => new(
            true,
            null,
            periodeId,
            jahr,
            monat,
            tage);

    public static ReadAutomatischePlanungsvorschauResult Fehler(string message)
        => new(
            false,
            message,
            Guid.Empty,
            0,
            0,
            Array.Empty<AutomatischePlanungsvorschauTag>());
}

public sealed class ReadAutomatischePlanungsvorschauService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IDienstplanMitarbeiterPlanungsRepository _dienstplanMitarbeiterPlanungsRepository;
    private readonly IDienstwunschRepository _dienstwunschRepository;
    private readonly IFreelancerMonatswunschRepository _freelancerMonatswunschRepository;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstplanUrlaubszeitraumRepository _dienstplanUrlaubszeitraumRepository;
    private readonly IAutoplanLernereignisRepository? _autoplanLernereignisRepository;

    public ReadAutomatischePlanungsvorschauService(
    IDienstplanPeriodeRepository dienstplanPeriodeRepository,
    IDienstplanMitarbeiterPlanungsRepository dienstplanMitarbeiterPlanungsRepository,
    IDienstwunschRepository dienstwunschRepository,
    IFreelancerMonatswunschRepository freelancerMonatswunschRepository,
    IGeplanterDienstTagRepository geplanterDienstTagRepository,
    IDienstplanUrlaubszeitraumRepository dienstplanUrlaubszeitraumRepository,
    IAutoplanLernereignisRepository? autoplanLernereignisRepository = null)
    {
        ArgumentNullException.ThrowIfNull(dienstplanPeriodeRepository);
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository;

        ArgumentNullException.ThrowIfNull(dienstplanMitarbeiterPlanungsRepository);
        _dienstplanMitarbeiterPlanungsRepository = dienstplanMitarbeiterPlanungsRepository;

        ArgumentNullException.ThrowIfNull(dienstwunschRepository);
        _dienstwunschRepository = dienstwunschRepository;

        ArgumentNullException.ThrowIfNull(freelancerMonatswunschRepository);
        _freelancerMonatswunschRepository = freelancerMonatswunschRepository;

        ArgumentNullException.ThrowIfNull(geplanterDienstTagRepository);
        _geplanterDienstTagRepository = geplanterDienstTagRepository;

        ArgumentNullException.ThrowIfNull(dienstplanUrlaubszeitraumRepository);
        _dienstplanUrlaubszeitraumRepository = dienstplanUrlaubszeitraumRepository;

        _autoplanLernereignisRepository = autoplanLernereignisRepository;
    }

    public async Task<ReadAutomatischePlanungsvorschauResult> ExecuteAsync(
     Guid periodeId,
     AutomatischePlanungAusfuehrungsmodus modus = AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen,
     CancellationToken cancellationToken = default)
    {
        if (periodeId == Guid.Empty)
        {
            return ReadAutomatischePlanungsvorschauResult.Fehler("Die Dienstplanperiode ist ungültig.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            periodeId,
            cancellationToken);

        if (periode is null)
        {
            return ReadAutomatischePlanungsvorschauResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        var grundlagenService = new ReadAutomatischePlanungsgrundlageService(
            _dienstplanPeriodeRepository,
            _dienstplanMitarbeiterPlanungsRepository,
            _dienstwunschRepository,
            _freelancerMonatswunschRepository);

        var grundlagenResult = await grundlagenService.ExecuteAsync(
            periodeId,
            cancellationToken);

        if (!grundlagenResult.IsSuccess)
        {
            return ReadAutomatischePlanungsvorschauResult.Fehler(
                grundlagenResult.ErrorMessage ?? "Die Planungsgrundlage konnte nicht gelesen werden.");
        }

        var bestehendePlaene = await _geplanterDienstTagRepository.GetAlleFuerPeriodeAsync(
            periodeId,
            cancellationToken);

        var manuellBearbeiteteTage = await ErmittleManuellBearbeiteteTageAsync(
            periodeId,
            cancellationToken);

        var planbareTage = ErmittlePlanbareTage(periode.Jahr, periode.Monat);
        var urlaubsUserIdsProTag = await ErmittleUrlaubsUserIdsProTagAsync(
            planbareTage,
            cancellationToken);

        var urlaubstageProUser = ErstelleUrlaubstageProUserLookup(urlaubsUserIdsProTag);
        var mitarbeiterLookup = grundlagenResult.Mitarbeiter.ToDictionary(
            x => x.UserId,
            x => x,
            StringComparer.OrdinalIgnoreCase);

        var beruecksichtigeBestehendePlanungen =
            modus == AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen;

        var urspruenglichePlanLookup = bestehendePlaene.ToDictionary(x => x.DienstDatum);

        var bereitsGeplanteTageLookup = beruecksichtigeBestehendePlanungen
            ? ErmittleBereitsGeplanteTageLookup(bestehendePlaene)
            : new Dictionary<string, HashSet<DateOnly>>(StringComparer.OrdinalIgnoreCase);

        var kandidaten = grundlagenResult.Mitarbeiter
            .ToDictionary(
                x => x.UserId,
                x =>
                {
                    bereitsGeplanteTageLookup.TryGetValue(x.UserId, out var geplanteTage);
                    urlaubstageProUser.TryGetValue(x.UserId, out var urlaubstage);

                    return new KandidatState(
                        x,
                        geplanteTage ?? new HashSet<DateOnly>(),
                        urlaubstage ?? new HashSet<DateOnly>());
                },
                StringComparer.OrdinalIgnoreCase);

        var tage = planbareTage.ToDictionary(
            datum => datum,
            datum =>
            {
                urspruenglichePlanLookup.TryGetValue(datum, out var urspruenglicherTag);

                var urspruenglichArzt = urspruenglicherTag?.ArztUserId;
                var urspruenglichNfs1 = urspruenglicherTag?.Notfallsanitaeter1UserId;
                var urspruenglichNfs2 = urspruenglicherTag?.Notfallsanitaeter2UserId;

                var istManuellBearbeitet = manuellBearbeiteteTage.Contains(datum);

                var initialArzt = (beruecksichtigeBestehendePlanungen || istManuellBearbeitet)
                    ? urspruenglichArzt
                    : null;

                var initialNfs1 = (beruecksichtigeBestehendePlanungen || istManuellBearbeitet)
                    ? urspruenglichNfs1
                    : null;

                var initialNfs2 = (beruecksichtigeBestehendePlanungen || istManuellBearbeitet)
                    ? urspruenglichNfs2
                    : null;

                return new PlanungstagBuilder(
                    datum,
                    urspruenglichArzt,
                    urspruenglichNfs1,
                    urspruenglichNfs2,
                    initialArzt,
                    initialNfs1,
                    initialNfs2);
            });

        var teamkombinationsScoreService = _autoplanLernereignisRepository is null
            ? null
            : new ReadAutoplanTeamkombinationsScoreService(_autoplanLernereignisRepository);

        var slotPraeferenzService = _autoplanLernereignisRepository is null
            ? null
            : new ReadAutoplanSlotPraeferenzService(_autoplanLernereignisRepository);

        foreach (var datum in planbareTage)
        {
            var tag = tage[datum];

            if (manuellBearbeiteteTage.Contains(datum))
            {
                continue;
            }

            if (beruecksichtigeBestehendePlanungen)
            {
                MarkiereBestehendeUrlaubskonflikte(
                    tag,
                    datum,
                    urlaubsUserIdsProTag,
                    mitarbeiterLookup);
            }

            WeiseVerpflichtendeFreelancerZu(tag, tage, kandidaten.Values, SlotTyp.Arzt, datum);
            WeiseVerpflichtendeFreelancerZu(tag, tage, kandidaten.Values, SlotTyp.Notfallsanitaeter, datum);

            await FuelleArztSlot(
                tag,
                tage,
                kandidaten.Values,
                datum,
                teamkombinationsScoreService,
                slotPraeferenzService,
                cancellationToken);

            await FuelleNotfallsanitaeterSlots(
                tag,
                tage,
                kandidaten.Values,
                datum,
                teamkombinationsScoreService,
                slotPraeferenzService,
                cancellationToken);

            if (tag.FreieArztSlots > 0 || tag.FreieNotfallsanitaeterSlots > 0)
            {
                var teile = new List<string>();

                if (tag.FreieArztSlots > 0)
                {
                    teile.Add("Arzt fehlt");
                }

                if (tag.FreieNotfallsanitaeterSlots > 0)
                {
                    teile.Add($"{tag.FreieNotfallsanitaeterSlots} Notfallsanitäter fehlen");
                }

                tag.AddKonflikt(
                    AutomatischePlanungKonfliktArt.MindestbesetzungNichtErreichbar,
                    $"Mindestbesetzung nicht erreichbar: {string.Join(", ", teile)}.");
            }
        }

        MarkiereFreelancerKonflikte(tage, kandidaten.Values);

        IReadOnlyList<AutomatischePlanungKonfliktEintrag> ErmittleSichtbareKonflikte(PlanungstagBuilder tag)
        {
            if (manuellBearbeiteteTage.Contains(tag.Datum))
            {
                return Array.Empty<AutomatischePlanungKonfliktEintrag>();
            }

            return tag.Konflikte.ToArray();
        }

        var resultTage = tage.Values
            .OrderBy(x => x.Datum)
            .Select(x =>
            {
                var sichtbareKonflikte = ErmittleSichtbareKonflikte(x);

                return new AutomatischePlanungsvorschauTag
                {
                    Datum = x.Datum,
                    ArztUserId = x.ArztUserId,
                    Notfallsanitaeter1UserId = x.Notfallsanitaeter1UserId,
                    Notfallsanitaeter2UserId = x.Notfallsanitaeter2UserId,
                    HatKonflikt = sichtbareKonflikte.Count > 0,
                    KonfliktText = string.Join(" ", sichtbareKonflikte.Select(k => k.Nachricht)),
                    Konflikte = sichtbareKonflikte,
                    SlotEntscheidungen = x.BaueSlotEntscheidungen()
                };
            })
            .ToArray();

        return ReadAutomatischePlanungsvorschauResult.Erfolg(
            periode.Id,
            periode.Jahr,
            periode.Monat,
            resultTage);
    }

    private async Task<HashSet<DateOnly>> ErmittleManuellBearbeiteteTageAsync(
    Guid periodeId,
    CancellationToken cancellationToken)
    {
        var result = new HashSet<DateOnly>();

        if (_autoplanLernereignisRepository is null)
        {
            return result;
        }

        var grundbesetzungsLernereignisse =
            await _autoplanLernereignisRepository.GetGrundbesetzungsLernereignisseAsync(cancellationToken);

        foreach (var lernereignis in grundbesetzungsLernereignisse)
        {
            if (lernereignis.DienstplanPeriodeId != periodeId)
            {
                continue;
            }

            if (lernereignis.EreignisTypCode != AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert)
            {
                continue;
            }

            result.Add(lernereignis.DienstDatum);
        }

        var vertretungsLernereignisse =
            await _autoplanLernereignisRepository.GetVertretungsLernereignisseAsync(cancellationToken);

        foreach (var lernereignis in vertretungsLernereignisse)
        {
            if (lernereignis.DienstplanPeriodeId != periodeId)
            {
                continue;
            }

            if (lernereignis.EreignisTypCode != AutoplanLernereignisTypCode.VertretungManuellGeaendert)
            {
                continue;
            }

            result.Add(lernereignis.DienstDatum);
        }

        return result;
    }

   

    private async Task<Dictionary<DateOnly, HashSet<string>>> ErmittleUrlaubsUserIdsProTagAsync(
        IReadOnlyList<DateOnly> planbareTage,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<DateOnly, HashSet<string>>();

        foreach (var datum in planbareTage)
        {
            var userIds = await _dienstplanUrlaubszeitraumRepository.GetAktiveUserIdsFuerDatumAsync(
                datum,
                cancellationToken);

            result[datum] = userIds
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return result;
    }

    private static Dictionary<string, HashSet<DateOnly>> ErstelleUrlaubstageProUserLookup(
        IReadOnlyDictionary<DateOnly, HashSet<string>> urlaubsUserIdsProTag)
    {
        var result = new Dictionary<string, HashSet<DateOnly>>(StringComparer.OrdinalIgnoreCase);

        foreach (var eintrag in urlaubsUserIdsProTag)
        {
            foreach (var userId in eintrag.Value)
            {
                if (!result.TryGetValue(userId, out var tage))
                {
                    tage = new HashSet<DateOnly>();
                    result[userId] = tage;
                }

                tage.Add(eintrag.Key);
            }
        }

        return result;
    }

    private static void MarkiereBestehendeUrlaubskonflikte(
        PlanungstagBuilder tag,
        DateOnly datum,
        IReadOnlyDictionary<DateOnly, HashSet<string>> urlaubsUserIdsProTag,
        IReadOnlyDictionary<string, AutomatischePlanungsgrundlageMitarbeiter> mitarbeiterLookup)
    {
        if (!urlaubsUserIdsProTag.TryGetValue(datum, out var urlaubsUserIds) || urlaubsUserIds.Count == 0)
        {
            return;
        }

        MarkiereUrlaubskonfliktFuerSlot(
            tag,
            urlaubsUserIds,
            mitarbeiterLookup,
            tag.ArztUserId,
            "Arzt");

        MarkiereUrlaubskonfliktFuerSlot(
            tag,
            urlaubsUserIds,
            mitarbeiterLookup,
            tag.Notfallsanitaeter1UserId,
            "NFS 1");

        MarkiereUrlaubskonfliktFuerSlot(
            tag,
            urlaubsUserIds,
            mitarbeiterLookup,
            tag.Notfallsanitaeter2UserId,
            "NFS 2");
    }

    private static void MarkiereUrlaubskonfliktFuerSlot(
        PlanungstagBuilder tag,
        IReadOnlySet<string> urlaubsUserIds,
        IReadOnlyDictionary<string, AutomatischePlanungsgrundlageMitarbeiter> mitarbeiterLookup,
        string? userId,
        string slotBezeichnung)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var normalisierteUserId = userId.Trim();

        if (!urlaubsUserIds.Contains(normalisierteUserId))
        {
            return;
        }

        var anzeigeName = mitarbeiterLookup.TryGetValue(normalisierteUserId, out var mitarbeiter)
            ? mitarbeiter.AnzeigeName
            : normalisierteUserId;

        tag.AddKonflikt(
            AutomatischePlanungKonfliktArt.BestehendePlanungKollidiertMitUrlaub,
            $"Bestehende Planung kollidiert mit Urlaubsplaner: {anzeigeName} ({slotBezeichnung}) ist im hinterlegten Urlaub.");
    }

    private static void WeiseVerpflichtendeFreelancerZu(
        PlanungstagBuilder tag,
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        IEnumerable<KandidatState> kandidaten,
        SlotTyp slotTyp,
        DateOnly datum)
    {
        var verpflichtendeKandidaten = kandidaten
            .Where(x => IstPflichtkandidatFuerSlotAnTag(x, tag, tage, slotTyp, datum))
            .OrderBy(x => ErmittlePflichtdruck(x, tage, slotTyp, datum))
            .ThenBy(x => x.GesamtZuweisungen)
            .ThenBy(x => x.Mitarbeiter.AnzeigeName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var freieSlots = slotTyp == SlotTyp.Arzt
            ? tag.FreieArztSlots
            : tag.FreieNotfallsanitaeterSlots;

        if (verpflichtendeKandidaten.Length > freieSlots)
        {
            var rollenText = slotTyp == SlotTyp.Arzt ? "Arzt" : "Notfallsanitäter";

            tag.AddKonflikt(
                AutomatischePlanungKonfliktArt.MehrPflichtfaelleAlsFreieSlots,
                $"Konflikt: Mehr verpflichtende Freelancer-Wünsche für {rollenText} als freie Slots vorhanden.");
        }

        foreach (var kandidat in verpflichtendeKandidaten)
        {
            if (!KannAnTagFuerSlotGeplantWerden(kandidat, tag, slotTyp, datum))
            {
                continue;
            }

            var begruendung =
                $"Pflichtfall: {kandidat.RestbedarfFreelancer} Freelancer-Dienst(e) sind noch offen und genau so viele regelkonforme Wunschtage bleiben ab diesem Datum übrig.";

            if (slotTyp == SlotTyp.Arzt)
            {
                if (!string.IsNullOrWhiteSpace(tag.ArztUserId))
                {
                    break;
                }

                tag.SetzeArzt(
                    kandidat.Mitarbeiter.UserId,
                    AutomatischePlanungZuweisungsArt.PflichtfallFreelancer,
                    begruendung);

                kandidat.MarkiereZuweisung(datum);
                continue;
            }

            if (tag.FreieNotfallsanitaeterSlots <= 0)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(tag.Notfallsanitaeter1UserId))
            {
                tag.SetzeNotfallsanitaeter1(
                    kandidat.Mitarbeiter.UserId,
                    AutomatischePlanungZuweisungsArt.PflichtfallFreelancer,
                    begruendung);

                kandidat.MarkiereZuweisung(datum);
                continue;
            }

            if (string.IsNullOrWhiteSpace(tag.Notfallsanitaeter2UserId))
            {
                tag.SetzeNotfallsanitaeter2(
                    kandidat.Mitarbeiter.UserId,
                    AutomatischePlanungZuweisungsArt.PflichtfallFreelancer,
                    begruendung);

                kandidat.MarkiereZuweisung(datum);
            }
        }
    }

    private async Task FuelleArztSlot(
     PlanungstagBuilder tag,
     IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
     IEnumerable<KandidatState> kandidaten,
     DateOnly datum,
     ReadAutoplanTeamkombinationsScoreService? teamkombinationsScoreService,
     ReadAutoplanSlotPraeferenzService? slotPraeferenzService,
     CancellationToken cancellationToken)
    {
        while (tag.FreieArztSlots > 0)
        {
            var entscheidung = await WaehleBestenKandidaten(
                kandidaten,
                tage,
                tag,
                SlotTyp.Arzt,
                datum,
                teamkombinationsScoreService,
                slotPraeferenzService,
                cancellationToken);

            if (entscheidung is null)
            {
                break;
            }

            tag.SetzeArzt(
                entscheidung.Kandidat.Mitarbeiter.UserId,
                entscheidung.ZuweisungsArt,
                entscheidung.Begruendung);

            entscheidung.Kandidat.MarkiereZuweisung(datum);
        }
    }

    private async Task FuelleNotfallsanitaeterSlots(
    PlanungstagBuilder tag,
    IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
    IEnumerable<KandidatState> kandidaten,
    DateOnly datum,
    ReadAutoplanTeamkombinationsScoreService? teamkombinationsScoreService,
    ReadAutoplanSlotPraeferenzService? slotPraeferenzService,
    CancellationToken cancellationToken)
    {
        while (tag.FreieNotfallsanitaeterSlots > 0)
        {
            var entscheidung = await WaehleBestenKandidaten(
                kandidaten,
                tage,
                tag,
                SlotTyp.Notfallsanitaeter,
                datum,
                teamkombinationsScoreService,
                slotPraeferenzService,
                cancellationToken);

            if (entscheidung is null)
            {
                break;
            }

            if (string.IsNullOrWhiteSpace(tag.Notfallsanitaeter1UserId))
            {
                tag.SetzeNotfallsanitaeter1(
                    entscheidung.Kandidat.Mitarbeiter.UserId,
                    entscheidung.ZuweisungsArt,
                    entscheidung.Begruendung);
            }
            else
            {
                tag.SetzeNotfallsanitaeter2(
                    entscheidung.Kandidat.Mitarbeiter.UserId,
                    entscheidung.ZuweisungsArt,
                    entscheidung.Begruendung);
            }

            entscheidung.Kandidat.MarkiereZuweisung(datum);
        }
    }

    private async Task<Auswahlentscheidung?> WaehleBestenKandidaten(
    IEnumerable<KandidatState> kandidaten,
    IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
    PlanungstagBuilder tag,
    SlotTyp slotTyp,
    DateOnly datum,
    ReadAutoplanTeamkombinationsScoreService? teamkombinationsScoreService,
    ReadAutoplanSlotPraeferenzService? slotPraeferenzService,
    CancellationToken cancellationToken)
    {
        var aktuelleTageskritikalitaet = ErmittleTageskritikalitaet(
            kandidaten,
            tage,
            slotTyp,
            datum);

        var zulassigeKandidaten = kandidaten
            .Where(x => KannAnTagFuerSlotGeplantWerden(x, tag, slotTyp, datum))
            .Where(x => !SollFreelancerHeuteZurueckgestelltWerden(x, kandidaten, tage, tag, slotTyp, datum, aktuelleTageskritikalitaet))
            .ToArray();

        var teamLernbonusLookup = await ErmittleTeamLernbonusLookupAsync(
            zulassigeKandidaten,
            tag,
            slotTyp,
            teamkombinationsScoreService,
            cancellationToken);

        var slotPraeferenzBonusLookup = await ErmittleSlotPraeferenzBonusLookupAsync(
            zulassigeKandidaten,
            tag,
            slotTyp,
            slotPraeferenzService,
            cancellationToken);

        var entscheidung = zulassigeKandidaten
            .Select(x =>
            {
                var basisPrioritaet = ErmittleBasisPrioritaet(x, datum);
                var pflichtdruck = ErmittlePflichtdruck(x, tage, slotTyp, datum);
                var alternativenVorteil = ErmittleAlternativenVorteil(x, kandidaten, tage, slotTyp, datum, aktuelleTageskritikalitaet);
                var blockBonus = ErmittleBlockBonus(x, datum);

                teamLernbonusLookup.TryGetValue(x.Mitarbeiter.UserId, out var teamLernBonus);
                slotPraeferenzBonusLookup.TryGetValue(x.Mitarbeiter.UserId, out var slotPraeferenzBonus);

                return new
                {
                    Kandidat = x,
                    BasisPrioritaet = basisPrioritaet,
                    Pflichtdruck = pflichtdruck,
                    AlternativenVorteil = alternativenVorteil,
                    BlockBonus = blockBonus,
                    TeamLernBonus = teamLernBonus,
                    SlotPraeferenzBonus = slotPraeferenzBonus,
                    GesamtZuweisungen = x.GesamtZuweisungen,
                    AnzeigeName = x.Mitarbeiter.AnzeigeName,
                    Entscheidung = BaueAuswahlentscheidung(
                        x,
                        datum,
                        aktuelleTageskritikalitaet,
                        alternativenVorteil,
                        blockBonus,
                        teamLernBonus,
                        slotPraeferenzBonus)
                };
            })
            .OrderBy(x => x.BasisPrioritaet)
            .ThenBy(x => x.Pflichtdruck)
            .ThenBy(x => x.AlternativenVorteil)
            .ThenByDescending(x => x.BlockBonus)
            .ThenByDescending(x => x.TeamLernBonus)
            .ThenByDescending(x => x.SlotPraeferenzBonus)
            .ThenBy(x => x.GesamtZuweisungen)
            .ThenBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        return entscheidung?.Entscheidung;
    }

    private static Auswahlentscheidung BaueAuswahlentscheidung(
    KandidatState kandidat,
    DateOnly datum,
    int aktuelleTageskritikalitaet,
    int alternativenVorteil,
    int blockBonus,
    decimal teamLernBonus,
    decimal slotPraeferenzBonus)
    {
        if (kandidat.IstFreelancer)
        {
            var begruendung = alternativenVorteil > 0
                ? $"Freelancer-Wunschtag wurde priorisiert, weil dieser Tag kritischer ist als andere noch mögliche Wunschtage. Tageskritik: {aktuelleTageskritikalitaet}."
                : $"Freelancer-Wunschtag wurde innerhalb der verfügbaren Wunschmenge priorisiert. Tageskritik: {aktuelleTageskritikalitaet}.";

            return new Auswahlentscheidung(
                kandidat,
                AutomatischePlanungZuweisungsArt.FreelancerNachTageskritik,
                ErweitereBegruendungMitLernboni(begruendung, teamLernBonus, slotPraeferenzBonus));
        }

        if (kandidat.IstHonorarkraft)
        {
            return new Auswahlentscheidung(
                kandidat,
                AutomatischePlanungZuweisungsArt.HonorarkraftWunsch,
                ErweitereBegruendungMitLernboni(
                    "Wunsch der Honorarkraft wurde berücksichtigt.",
                    teamLernBonus,
                    slotPraeferenzBonus));
        }

        if (kandidat.IstFestangestellt && kandidat.HatWunschAm(datum))
        {
            if (blockBonus >= 20)
            {
                var blockLaenge = kandidat.ErmittleWunschblockLaenge(datum);

                return new Auswahlentscheidung(
                    kandidat,
                    AutomatischePlanungZuweisungsArt.MitarbeiterWunschblock,
                    ErweitereBegruendungMitLernboni(
                        $"Zusammenhängender Wunschblock wurde bevorzugt berücksichtigt. Blocklänge: {blockLaenge} Tage.",
                        teamLernBonus,
                        slotPraeferenzBonus));
            }

            return new Auswahlentscheidung(
                kandidat,
                AutomatischePlanungZuweisungsArt.MitarbeiterWunsch,
                ErweitereBegruendungMitLernboni(
                    "Mitarbeiterwunsch wurde berücksichtigt.",
                    teamLernBonus,
                    slotPraeferenzBonus));
        }

        return new Auswahlentscheidung(
            kandidat,
            AutomatischePlanungZuweisungsArt.MitarbeiterLueckenfueller,
            ErweitereBegruendungMitLernboni(
                "Als Lückenfüller gesetzt, weil kein höher priorisierter Wunschkandidat verfügbar war.",
                teamLernBonus,
                slotPraeferenzBonus));
    }

    private async Task<Dictionary<string, decimal>> ErmittleTeamLernbonusLookupAsync(
    IReadOnlyCollection<KandidatState> kandidaten,
    PlanungstagBuilder tag,
    SlotTyp slotTyp,
    ReadAutoplanTeamkombinationsScoreService? teamkombinationsScoreService,
    CancellationToken cancellationToken)
    {
        if (teamkombinationsScoreService is null || kandidaten.Count == 0)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        if (!HatRelevantenTeamkontext(tag, slotTyp))
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        var kandidatenUserIds = kandidaten
            .Select(x => x.Mitarbeiter.UserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (kandidatenUserIds.Length == 0)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        var scoreResult = await teamkombinationsScoreService.ExecuteAsync(
            new ReadAutoplanTeamkombinationsScoreQuery(
                BesetzungsSlotCode: ErmittleBesetzungsSlotCodeFuerAktuelleAuswahl(tag, slotTyp),
                KandidatenUserIds: kandidatenUserIds,
                KontextArztUserId: tag.ArztUserId,
                KontextNotfallsanitaeter1UserId: tag.Notfallsanitaeter1UserId,
                KontextNotfallsanitaeter2UserId: tag.Notfallsanitaeter2UserId,
                BewertungsDatum: tag.Datum,
                Mindestanzahl: 2,
                MindestanzahlNegativeKorrekturen: 2),
            cancellationToken);

        if (!scoreResult.IsSuccess)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        return scoreResult.Eintraege.ToDictionary(
            x => x.KandidatUserId,
            x => x.LernBonus,
            StringComparer.OrdinalIgnoreCase);
    }

    private static bool HatRelevantenTeamkontext(
        PlanungstagBuilder tag,
        SlotTyp slotTyp)
    {
        if (slotTyp == SlotTyp.Arzt)
        {
            return !string.IsNullOrWhiteSpace(tag.Notfallsanitaeter1UserId)
                   || !string.IsNullOrWhiteSpace(tag.Notfallsanitaeter2UserId);
        }

        return !string.IsNullOrWhiteSpace(tag.ArztUserId)
               || !string.IsNullOrWhiteSpace(tag.Notfallsanitaeter1UserId)
               || !string.IsNullOrWhiteSpace(tag.Notfallsanitaeter2UserId);
    }

    private static DienstbesetzungsSlotCode ErmittleBesetzungsSlotCodeFuerAktuelleAuswahl(
        PlanungstagBuilder tag,
        SlotTyp slotTyp)
    {
        if (slotTyp == SlotTyp.Arzt)
        {
            return DienstbesetzungsSlotCode.Arzt;
        }

        return string.IsNullOrWhiteSpace(tag.Notfallsanitaeter1UserId)
            ? DienstbesetzungsSlotCode.Notfallsanitaeter1
            : DienstbesetzungsSlotCode.Notfallsanitaeter2;
    }

    private async Task<Dictionary<string, decimal>> ErmittleSlotPraeferenzBonusLookupAsync(
    IReadOnlyCollection<KandidatState> kandidaten,
    PlanungstagBuilder tag,
    SlotTyp slotTyp,
    ReadAutoplanSlotPraeferenzService? slotPraeferenzService,
    CancellationToken cancellationToken)
    {
        if (slotPraeferenzService is null || kandidaten.Count == 0)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        // Nur aktiv wenn noch kein Team-Kontext vorhanden ist.
        // Sobald mindestens ein Slot besetzt ist, übernimmt der TeamLernBonus die Führung.
        if (HatRelevantenTeamkontext(tag, slotTyp))
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        var kandidatenUserIds = kandidaten
            .Select(x => x.Mitarbeiter.UserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (kandidatenUserIds.Length == 0)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        var besetzungsSlotCode = ErmittleBesetzungsSlotCodeFuerAktuelleAuswahl(tag, slotTyp);

        var scoreResult = await slotPraeferenzService.ExecuteAsync(
            new ReadAutoplanSlotPraeferenzQuery(
                BesetzungsSlotCode: besetzungsSlotCode,
                KandidatenUserIds: kandidatenUserIds,
                BewertungsDatum: tag.Datum),
            cancellationToken);

        if (!scoreResult.IsSuccess)
        {
            return new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        }

        return scoreResult.Eintraege
            .Where(x => x.PraeferenzBonus > 0m)
            .ToDictionary(
                x => x.KandidatUserId,
                x => x.PraeferenzBonus,
                StringComparer.OrdinalIgnoreCase);
    }

    private static string ErweitereBegruendungMitLernboni(
    string begruendung,
    decimal teamLernBonus,
    decimal slotPraeferenzBonus)
    {
        var teile = new List<string>();

        if (teamLernBonus > 0m)
        {
            teile.Add($"Lernbonus aus ähnlicher manuell bestätigter Teamkonstellation: {teamLernBonus:0.##}");
        }
        else if (teamLernBonus < 0m)
        {
            teile.Add($"Lernmalus aus ähnlicher manuell korrigierter Teamkonstellation: {Math.Abs(teamLernBonus):0.##}");
        }

        if (slotPraeferenzBonus > 0m)
        {
            teile.Add($"Slotpräferenz aus historischer Planung: {slotPraeferenzBonus:0.##}");
        }

        return teile.Count == 0
            ? begruendung
            : $"{begruendung} {string.Join(". ", teile)}.";
    }


    private static bool SollFreelancerHeuteZurueckgestelltWerden(
        KandidatState kandidat,
        IEnumerable<KandidatState> kandidaten,
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        PlanungstagBuilder tag,
        SlotTyp slotTyp,
        DateOnly datum,
        int aktuelleTageskritikalitaet)
    {
        if (!kandidat.IstFreelancer)
        {
            return false;
        }

        if (IstPflichtkandidatFuerSlotAnTag(kandidat, tag, tage, slotTyp, datum))
        {
            return false;
        }

        var hoechsteAlternativeKritikalitaet = ErmittleHoechsteAlternativeTageskritikalitaet(
            kandidat,
            kandidaten,
            tage,
            slotTyp,
            datum);

        if (hoechsteAlternativeKritikalitaet <= aktuelleTageskritikalitaet)
        {
            return false;
        }

        return kandidaten.Any(anderer =>
            !ReferenceEquals(anderer, kandidat) &&
            KannAnTagFuerSlotGeplantWerden(anderer, tag, slotTyp, datum) &&
            !anderer.IstFreelancer &&
            (anderer.HatWunschAm(datum) || anderer.IstFestangestellt));
    }

    private static bool KannAnTagFuerSlotGeplantWerden(
        KandidatState kandidat,
        PlanungstagBuilder tag,
        SlotTyp slotTyp,
        DateOnly datum)
    {
        if (!kandidat.Mitarbeiter.IstFuerAutomatischePlanungGeeignet)
        {
            return false;
        }

        if (tag.BelegteUserIds.Contains(kandidat.Mitarbeiter.UserId))
        {
            return false;
        }

        if (kandidat.HatUrlaubAm(datum))
        {
            return false;
        }

        if (kandidat.HatDienstAm(datum))
        {
            return false;
        }

        if (!HatPassendeQualifikation(kandidat, slotTyp))
        {
            return false;
        }

        if (!HatVerbleibendeKapazitaet(kandidat))
        {
            return false;
        }

        if (kandidat.IstFestangestellt)
        {
            return !kandidat.HatBlockAm(datum);
        }

        if (kandidat.IstFreelancer || kandidat.IstHonorarkraft)
        {
            return kandidat.HatWunschAm(datum);
        }

        return false;
    }

    private static bool HatPassendeQualifikation(KandidatState kandidat, SlotTyp slotTyp)
    {
        return slotTyp == SlotTyp.Arzt
            ? kandidat.Mitarbeiter.IstArzt
            : kandidat.Mitarbeiter.IstNotfallsanitaeter;
    }

    private static bool HatVerbleibendeKapazitaet(KandidatState kandidat)
    {
        if (!kandidat.IstFreelancer)
        {
            return true;
        }

        return kandidat.GesamtZuweisungen < kandidat.MaxAutomatischeFreelancerDienste;
    }

    private static bool IstPflichtkandidatFuerSlotAnTag(
        KandidatState kandidat,
        PlanungstagBuilder tag,
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        SlotTyp slotTyp,
        DateOnly datum)
    {
        if (!kandidat.IstFreelancer)
        {
            return false;
        }

        if (!KannAnTagFuerSlotGeplantWerden(kandidat, tag, slotTyp, datum))
        {
            return false;
        }

        if (kandidat.RestbedarfFreelancer <= 0)
        {
            return false;
        }

        var verbleibendeMoeglicheTage = ErmittleVerbleibendeMoeglicheTage(
            kandidat,
            tage,
            slotTyp,
            datum);

        return verbleibendeMoeglicheTage == kandidat.RestbedarfFreelancer;
    }

    private static int ErmittleBasisPrioritaet(
        KandidatState kandidat,
        DateOnly datum)
    {
        if (kandidat.IstFreelancer && kandidat.HatWunschAm(datum))
        {
            return 0;
        }

        if (kandidat.IstHonorarkraft && kandidat.HatWunschAm(datum))
        {
            return 1;
        }

        if (kandidat.IstFestangestellt && kandidat.HatWunschAm(datum))
        {
            return 2;
        }

        if (kandidat.IstFestangestellt)
        {
            return 3;
        }

        return 99;
    }

    private static int ErmittlePflichtdruck(
        KandidatState kandidat,
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        SlotTyp slotTyp,
        DateOnly datum)
    {
        if (kandidat.IstFreelancer)
        {
            var verbleibendeMoeglicheTage = ErmittleVerbleibendeMoeglicheTage(
                kandidat,
                tage,
                slotTyp,
                datum);

            return Math.Max(0, verbleibendeMoeglicheTage - kandidat.RestbedarfFreelancer);
        }

        if (kandidat.IstFestangestellt && kandidat.HatWunschAm(datum))
        {
            return 0;
        }

        if (kandidat.IstHonorarkraft && kandidat.HatWunschAm(datum))
        {
            return 0;
        }

        return 999;
    }

    private static int ErmittleAlternativenVorteil(
        KandidatState kandidat,
        IEnumerable<KandidatState> kandidaten,
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        SlotTyp slotTyp,
        DateOnly datum,
        int aktuelleTageskritikalitaet)
    {
        if (!kandidat.IstFreelancer)
        {
            return 0;
        }

        var besteAlternative = ErmittleHoechsteAlternativeTageskritikalitaet(
            kandidat,
            kandidaten,
            tage,
            slotTyp,
            datum);

        return Math.Max(0, besteAlternative - aktuelleTageskritikalitaet);
    }

    private static int ErmittleHoechsteAlternativeTageskritikalitaet(
        KandidatState kandidat,
        IEnumerable<KandidatState> kandidaten,
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        SlotTyp slotTyp,
        DateOnly datum)
    {
        var alternativeTage = tage.Keys
            .Where(x => x > datum)
            .OrderBy(x => x)
            .ToArray();

        var besteAlternative = 0;

        foreach (var alternativesDatum in alternativeTage)
        {
            if (!tage.TryGetValue(alternativesDatum, out var alternativesTag))
            {
                continue;
            }

            if (!KannAnTagFuerSlotGeplantWerden(kandidat, alternativesTag, slotTyp, alternativesDatum))
            {
                continue;
            }

            var kritikalitaet = ErmittleTageskritikalitaet(
                kandidaten,
                tage,
                slotTyp,
                alternativesDatum);

            if (kritikalitaet > besteAlternative)
            {
                besteAlternative = kritikalitaet;
            }
        }

        return besteAlternative;
    }

    private static int ErmittleTageskritikalitaet(
        IEnumerable<KandidatState> kandidaten,
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        SlotTyp slotTyp,
        DateOnly datum)
    {
        if (!tage.TryGetValue(datum, out var tag))
        {
            return 0;
        }

        var freieSlots = slotTyp == SlotTyp.Arzt
            ? tag.FreieArztSlots
            : tag.FreieNotfallsanitaeterSlots;

        if (freieSlots <= 0)
        {
            return 0;
        }

        var verfuegbareKandidaten = kandidaten
            .Where(x => KannAnTagFuerSlotGeplantWerden(x, tag, slotTyp, datum))
            .ToArray();

        var anzahlVerfuegbareKandidaten = verfuegbareKandidaten.Length;
        var anzahlPflichtkandidaten = verfuegbareKandidaten.Count(x =>
            x.IstFreelancer &&
            x.RestbedarfFreelancer > 0 &&
            ErmittleVerbleibendeMoeglicheTage(x, tage, slotTyp, datum) == x.RestbedarfFreelancer);

        var kritikalitaet = 0;

        if (anzahlVerfuegbareKandidaten <= freieSlots)
        {
            kritikalitaet += 100;
        }
        else if (anzahlVerfuegbareKandidaten == freieSlots + 1)
        {
            kritikalitaet += 70;
        }
        else if (anzahlVerfuegbareKandidaten <= freieSlots + 3)
        {
            kritikalitaet += 40;
        }
        else
        {
            kritikalitaet += 10;
        }

        kritikalitaet += anzahlPflichtkandidaten * 10;

        return kritikalitaet;
    }

    private static int ErmittleBlockBonus(
        KandidatState kandidat,
        DateOnly datum)
    {
        if (!kandidat.IstFestangestellt || !kandidat.HatWunschAm(datum))
        {
            return 0;
        }

        var blockLaenge = kandidat.ErmittleWunschblockLaenge(datum);
        var nachbarDienste = 0;

        var vortag = datum.AddDays(-1);
        var folgetag = datum.AddDays(1);

        if (kandidat.HatWunschAm(vortag) && kandidat.HatDienstAm(vortag))
        {
            nachbarDienste++;
        }

        if (kandidat.HatWunschAm(folgetag) && kandidat.HatDienstAm(folgetag))
        {
            nachbarDienste++;
        }

        return (blockLaenge * 10) + (nachbarDienste * 20);
    }

    private static void MarkiereFreelancerKonflikte(
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        IEnumerable<KandidatState> kandidaten)
    {
        foreach (var kandidat in kandidaten.Where(x => x.IstFreelancer))
        {
            if (kandidat.MaxAutomatischeFreelancerDienste <= 0)
            {
                continue;
            }

            if (kandidat.RestbedarfFreelancer <= 0)
            {
                continue;
            }

            var slotTyp = kandidat.Mitarbeiter.IstArzt
                ? SlotTyp.Arzt
                : SlotTyp.Notfallsanitaeter;

            var naechsterWunschtag = kandidat.WunschTage
                .Where(tage.ContainsKey)
                .OrderBy(x => x)
                .FirstOrDefault();

            if (naechsterWunschtag == default)
            {
                continue;
            }

            var verbleibendeMoeglicheTage = ErmittleVerbleibendeMoeglicheTage(
                kandidat,
                tage,
                slotTyp,
                naechsterWunschtag);

            if (verbleibendeMoeglicheTage < kandidat.RestbedarfFreelancer)
            {
                tage[naechsterWunschtag].AddKonflikt(
                    AutomatischePlanungKonfliktArt.FreelancerSollNichtErreichbar,
                    $"{kandidat.Mitarbeiter.AnzeigeName}: Nur noch {verbleibendeMoeglicheTage} regelkonforme Wunschtage für {kandidat.RestbedarfFreelancer} fehlende Freelancer-Dienste verfügbar.");

                continue;
            }

            tage[naechsterWunschtag].AddKonflikt(
                AutomatischePlanungKonfliktArt.FreelancerDienstNichtKonfliktfreiVerteilbar,
                $"{kandidat.Mitarbeiter.AnzeigeName}: {kandidat.RestbedarfFreelancer} gewünschter Freelancer-Dienst konnte nicht konfliktfrei verteilt werden.");
        }
    }

    private static int ErmittleVerbleibendeMoeglicheTage(
        KandidatState kandidat,
        IReadOnlyDictionary<DateOnly, PlanungstagBuilder> tage,
        SlotTyp slotTyp,
        DateOnly abDatum)
    {
        var anzahl = 0;

        foreach (var eintrag in tage.OrderBy(x => x.Key))
        {
            if (eintrag.Key < abDatum)
            {
                continue;
            }

            if (KannAnTagFuerSlotGeplantWerden(kandidat, eintrag.Value, slotTyp, eintrag.Key))
            {
                anzahl++;
            }
        }

        return anzahl;
    }

    private static IReadOnlyList<DateOnly> ErmittlePlanbareTage(int jahr, int monat)
    {
        var feiertage = MecklenburgVorpommernFeiertage.GetFeiertage(jahr);
        var start = new DateOnly(jahr, monat, 1);
        var ende = start.AddMonths(1).AddDays(-1);

        var tage = new List<DateOnly>();

        for (var datum = start; datum <= ende; datum = datum.AddDays(1))
        {
            if (datum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            if (feiertage.ContainsKey(datum))
            {
                continue;
            }

            tage.Add(datum);
        }

        return tage;
    }

    private static Dictionary<string, HashSet<DateOnly>> ErmittleBereitsGeplanteTageLookup(
        IReadOnlyList<GeplanterDienstTag> bestehendePlaene)
    {
        var lookup = new Dictionary<string, HashSet<DateOnly>>(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in bestehendePlaene)
        {
            FuegeGeplantenTagHinzu(lookup, tag.ArztUserId, tag.DienstDatum);
            FuegeGeplantenTagHinzu(lookup, tag.Notfallsanitaeter1UserId, tag.DienstDatum);
            FuegeGeplantenTagHinzu(lookup, tag.Notfallsanitaeter2UserId, tag.DienstDatum);
        }

        return lookup;
    }

    private static void FuegeGeplantenTagHinzu(
        IDictionary<string, HashSet<DateOnly>> lookup,
        string? userId,
        DateOnly datum)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var normalisierteUserId = userId.Trim();

        if (!lookup.TryGetValue(normalisierteUserId, out var tage))
        {
            tage = new HashSet<DateOnly>();
            lookup[normalisierteUserId] = tage;
        }

        tage.Add(datum);
    }

    private enum SlotTyp
    {
        Arzt = 1,
        Notfallsanitaeter = 2
    }

    private sealed record Auswahlentscheidung(
        KandidatState Kandidat,
        AutomatischePlanungZuweisungsArt ZuweisungsArt,
        string Begruendung);

    private sealed class PlanungstagBuilder
    {
        private readonly string? _urspruenglichArztUserId;
        private readonly string? _urspruenglichNotfallsanitaeter1UserId;
        private readonly string? _urspruenglichNotfallsanitaeter2UserId;

        private AutomatischePlanungZuweisungsArt _arztArt;
        private AutomatischePlanungZuweisungsArt _nfs1Art;
        private AutomatischePlanungZuweisungsArt _nfs2Art;

        private string _arztNachricht = string.Empty;
        private string _nfs1Nachricht = string.Empty;
        private string _nfs2Nachricht = string.Empty;

        public PlanungstagBuilder(
            DateOnly datum,
            string? urspruenglichArztUserId,
            string? urspruenglichNotfallsanitaeter1UserId,
            string? urspruenglichNotfallsanitaeter2UserId,
            string? initialArztUserId,
            string? initialNotfallsanitaeter1UserId,
            string? initialNotfallsanitaeter2UserId)
        {
            Datum = datum;

            _urspruenglichArztUserId = NormalisiereUserId(urspruenglichArztUserId);
            _urspruenglichNotfallsanitaeter1UserId = NormalisiereUserId(urspruenglichNotfallsanitaeter1UserId);
            _urspruenglichNotfallsanitaeter2UserId = NormalisiereUserId(urspruenglichNotfallsanitaeter2UserId);

            ArztUserId = NormalisiereUserId(initialArztUserId);
            Notfallsanitaeter1UserId = NormalisiereUserId(initialNotfallsanitaeter1UserId);
            Notfallsanitaeter2UserId = NormalisiereUserId(initialNotfallsanitaeter2UserId);

            if (!string.IsNullOrWhiteSpace(ArztUserId))
            {
                _arztArt = AutomatischePlanungZuweisungsArt.BestehendePlanung;
                _arztNachricht = "Bestehende Planung wurde übernommen.";
            }

            if (!string.IsNullOrWhiteSpace(Notfallsanitaeter1UserId))
            {
                _nfs1Art = AutomatischePlanungZuweisungsArt.BestehendePlanung;
                _nfs1Nachricht = "Bestehende Planung wurde übernommen.";
            }

            if (!string.IsNullOrWhiteSpace(Notfallsanitaeter2UserId))
            {
                _nfs2Art = AutomatischePlanungZuweisungsArt.BestehendePlanung;
                _nfs2Nachricht = "Bestehende Planung wurde übernommen.";
            }
        }

        public DateOnly Datum { get; }

        public string? ArztUserId { get; private set; }

        public string? Notfallsanitaeter1UserId { get; private set; }

        public string? Notfallsanitaeter2UserId { get; private set; }

        public List<AutomatischePlanungKonfliktEintrag> Konflikte { get; } = new();

        public int FreieArztSlots => string.IsNullOrWhiteSpace(ArztUserId) ? 1 : 0;

        public int FreieNotfallsanitaeterSlots
        {
            get
            {
                var frei = 0;

                if (string.IsNullOrWhiteSpace(Notfallsanitaeter1UserId))
                {
                    frei++;
                }

                if (string.IsNullOrWhiteSpace(Notfallsanitaeter2UserId))
                {
                    frei++;
                }

                return frei;
            }
        }

        public HashSet<string> BelegteUserIds =>
            new[]
                {
                    ArztUserId,
                    Notfallsanitaeter1UserId,
                    Notfallsanitaeter2UserId
                }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        public void SetzeArzt(
            string userId,
            AutomatischePlanungZuweisungsArt art,
            string nachricht)
        {
            ArztUserId = NormalisiereUserId(userId);
            _arztArt = art;
            _arztNachricht = nachricht.Trim();
        }

        public void SetzeNotfallsanitaeter1(
            string userId,
            AutomatischePlanungZuweisungsArt art,
            string nachricht)
        {
            Notfallsanitaeter1UserId = NormalisiereUserId(userId);
            _nfs1Art = art;
            _nfs1Nachricht = nachricht.Trim();
        }

        public void SetzeNotfallsanitaeter2(
            string userId,
            AutomatischePlanungZuweisungsArt art,
            string nachricht)
        {
            Notfallsanitaeter2UserId = NormalisiereUserId(userId);
            _nfs2Art = art;
            _nfs2Nachricht = nachricht.Trim();
        }

        public void AddKonflikt(
            AutomatischePlanungKonfliktArt art,
            string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            if (Konflikte.Any(x =>
                    x.Art == art &&
                    string.Equals(x.Nachricht, message, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            Konflikte.Add(new AutomatischePlanungKonfliktEintrag
            {
                Art = art,
                Nachricht = message.Trim()
            });
        }

        public IReadOnlyList<AutomatischePlanungSlotEntscheidung> BaueSlotEntscheidungen()
        {
            return new[]
            {
                BaueSlotEntscheidung("Arzt", _urspruenglichArztUserId, ArztUserId, _arztArt, _arztNachricht),
                BaueSlotEntscheidung("NFS 1", _urspruenglichNotfallsanitaeter1UserId, Notfallsanitaeter1UserId, _nfs1Art, _nfs1Nachricht),
                BaueSlotEntscheidung("NFS 2", _urspruenglichNotfallsanitaeter2UserId, Notfallsanitaeter2UserId, _nfs2Art, _nfs2Nachricht)
            };
        }

        private static AutomatischePlanungSlotEntscheidung BaueSlotEntscheidung(
            string slotBezeichnung,
            string? vorherigerUserId,
            string? aktuellerUserId,
            AutomatischePlanungZuweisungsArt art,
            string nachricht)
        {
            var aenderungsArt = ErmittleAenderungsArt(vorherigerUserId, aktuellerUserId);

            var finalNachricht = !string.IsNullOrWhiteSpace(nachricht)
                ? nachricht
                : aenderungsArt switch
                {
                    AutomatischePlanungAenderungsArt.Unveraendert => "Besetzung bleibt unverändert.",
                    AutomatischePlanungAenderungsArt.NeuGesetzt => "Slot wird neu automatisch besetzt.",
                    AutomatischePlanungAenderungsArt.Ersetzt => "Bisherige Besetzung würde durch den Autoplan ersetzt.",
                    AutomatischePlanungAenderungsArt.WirdOffen => "Bisherige Besetzung würde entfallen und der Slot bliebe offen.",
                    AutomatischePlanungAenderungsArt.BleibtOffen => "Für diesen Slot wurde keine regelkonforme automatische Besetzung gefunden.",
                    _ => "Automatische Entscheidung wurde getroffen."
                };

            return new AutomatischePlanungSlotEntscheidung
            {
                SlotBezeichnung = slotBezeichnung,
                UserId = aktuellerUserId,
                VorherigerUserId = vorherigerUserId,
                Art = art,
                AenderungsArt = aenderungsArt,
                Nachricht = finalNachricht
            };
        }

        private static AutomatischePlanungAenderungsArt ErmittleAenderungsArt(
            string? vorherigerUserId,
            string? aktuellerUserId)
        {
            var alt = NormalisiereUserId(vorherigerUserId);
            var neu = NormalisiereUserId(aktuellerUserId);

            if (string.IsNullOrWhiteSpace(alt) && string.IsNullOrWhiteSpace(neu))
            {
                return AutomatischePlanungAenderungsArt.BleibtOffen;
            }

            if (string.IsNullOrWhiteSpace(alt) && !string.IsNullOrWhiteSpace(neu))
            {
                return AutomatischePlanungAenderungsArt.NeuGesetzt;
            }

            if (!string.IsNullOrWhiteSpace(alt) && string.IsNullOrWhiteSpace(neu))
            {
                return AutomatischePlanungAenderungsArt.WirdOffen;
            }

            if (string.Equals(alt, neu, StringComparison.OrdinalIgnoreCase))
            {
                return AutomatischePlanungAenderungsArt.Unveraendert;
            }

            return AutomatischePlanungAenderungsArt.Ersetzt;
        }

        private static string? NormalisiereUserId(string? userId)
        {
            return string.IsNullOrWhiteSpace(userId)
                ? null
                : userId.Trim();
        }
    }

    private sealed class KandidatState
    {
        private readonly HashSet<DateOnly> _wunschTage;
        private readonly HashSet<DateOnly> _blockierteTage;
        private readonly HashSet<DateOnly> _bereitsGeplanteTage;
        private readonly HashSet<DateOnly> _urlaubstage;
        private readonly HashSet<DateOnly> _vorgeschlageneTage = new();

        public KandidatState(
            AutomatischePlanungsgrundlageMitarbeiter mitarbeiter,
            HashSet<DateOnly> bereitsGeplanteTage,
            HashSet<DateOnly> urlaubstage)
        {
            Mitarbeiter = mitarbeiter;
            _wunschTage = mitarbeiter.WunschTage.ToHashSet();
            _blockierteTage = mitarbeiter.BlockierteTage.ToHashSet();
            _bereitsGeplanteTage = bereitsGeplanteTage;
            _urlaubstage = urlaubstage;
        }

        public AutomatischePlanungsgrundlageMitarbeiter Mitarbeiter { get; }

        public IEnumerable<DateOnly> WunschTage => _wunschTage;

        public bool IstFreelancer => Mitarbeiter.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Freelancer;

        public bool IstFestangestellt => Mitarbeiter.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Festangestellt;

        public bool IstHonorarkraft => Mitarbeiter.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Honorarkraft;

        public int GesamtZuweisungen => _bereitsGeplanteTage.Count + _vorgeschlageneTage.Count;

        public int MaxAutomatischeFreelancerDienste
        {
            get
            {
                if (!IstFreelancer)
                {
                    return 0;
                }

                var gewuenschteDienste = Mitarbeiter.FreelancerGewuenschteDienste.GetValueOrDefault();
                return Math.Clamp(gewuenschteDienste, 0, 3);
            }
        }

        public int RestbedarfFreelancer =>
            !IstFreelancer
                ? 0
                : Math.Max(0, MaxAutomatischeFreelancerDienste - GesamtZuweisungen);

        public bool HatWunschAm(DateOnly datum) => _wunschTage.Contains(datum);

        public bool HatBlockAm(DateOnly datum) => _blockierteTage.Contains(datum);

        public bool HatUrlaubAm(DateOnly datum) => _urlaubstage.Contains(datum);

        public bool HatDienstAm(DateOnly datum) =>
            _bereitsGeplanteTage.Contains(datum) || _vorgeschlageneTage.Contains(datum);

        public int ErmittleWunschblockLaenge(DateOnly datum)
        {
            if (!_wunschTage.Contains(datum))
            {
                return 0;
            }

            var laenge = 1;

            for (var tag = datum.AddDays(-1); _wunschTage.Contains(tag); tag = tag.AddDays(-1))
            {
                laenge++;
            }

            for (var tag = datum.AddDays(1); _wunschTage.Contains(tag); tag = tag.AddDays(1))
            {
                laenge++;
            }

            return laenge;
        }

        public void MarkiereZuweisung(DateOnly datum)
        {
            _vorgeschlageneTage.Add(datum);
        }

        
    }
}