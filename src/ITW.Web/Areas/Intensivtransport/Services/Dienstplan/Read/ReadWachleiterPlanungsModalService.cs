using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Perioden;
using ITW.Dienstplan.Application.Planung;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Shared;
using ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Read;

public sealed record ReadWachleiterPlanungsModalQuery(
    DienstplanPeriodeListenEintrag Periode,
    DienstplanKalenderTagViewModel Tag,
    GeplanterDienstTag? GeplanterDienstTag,
    IReadOnlyList<GeplanterDienstTagAusfall> TagesAusfaelle,
    string? ReturnUrl);

public sealed class ReadWachleiterPlanungsModalService
{
    private static readonly CultureInfo DeutscheKultur = CultureInfo.GetCultureInfo("de-DE");

    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly IDienstwunschRepository _dienstwunschRepository;
    private readonly IMitarbeiterUrlaubszeitraumRepository _mitarbeiterUrlaubszeitraumRepository;
    private readonly ReadAutoplanVertreterPraeferenzScoreService _readAutoplanVertreterPraeferenzScoreService;
    private readonly ReadAutoplanAllgemeinerVertreterPraeferenzScoreService _readAutoplanAllgemeinerVertreterPraeferenzScoreService;

    public ReadWachleiterPlanungsModalService(
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        IDienstwunschRepository dienstwunschRepository,
        IMitarbeiterUrlaubszeitraumRepository mitarbeiterUrlaubszeitraumRepository,
        ReadAutoplanVertreterPraeferenzScoreService readAutoplanVertreterPraeferenzScoreService,
        ReadAutoplanAllgemeinerVertreterPraeferenzScoreService readAutoplanAllgemeinerVertreterPraeferenzScoreService)
    {
        ArgumentNullException.ThrowIfNull(readItwMitarbeiterprofileService);
        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService;

        ArgumentNullException.ThrowIfNull(dienstwunschRepository);
        _dienstwunschRepository = dienstwunschRepository;

        ArgumentNullException.ThrowIfNull(mitarbeiterUrlaubszeitraumRepository);
        _mitarbeiterUrlaubszeitraumRepository = mitarbeiterUrlaubszeitraumRepository;

        ArgumentNullException.ThrowIfNull(readAutoplanVertreterPraeferenzScoreService);
        _readAutoplanVertreterPraeferenzScoreService = readAutoplanVertreterPraeferenzScoreService;

        ArgumentNullException.ThrowIfNull(readAutoplanAllgemeinerVertreterPraeferenzScoreService);
        _readAutoplanAllgemeinerVertreterPraeferenzScoreService = readAutoplanAllgemeinerVertreterPraeferenzScoreService;
    }

    public async Task<WachleiterPlanungsModalViewModel> ExecuteAsync(
        ReadWachleiterPlanungsModalQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

        var modal = new WachleiterPlanungsModalViewModel
        {
            IsSuccess = mitarbeiterResult.IsSuccess,
            ErrorMessage = mitarbeiterResult.ErrorMessage,
            PeriodeId = query.Periode.Id,
            PeriodeBezeichnung = query.Periode.Bezeichnung,
            Datum = query.Tag.Datum,
            DatumAnzeige = query.Tag.Datum.ToDateTime(TimeOnly.MinValue).ToString("dddd, dd.MM.yyyy", DeutscheKultur),
            ReturnUrl = query.ReturnUrl,
            Tag = query.Tag
        };

        if (!mitarbeiterResult.IsSuccess)
        {
            return modal;
        }

        var alleProfile = mitarbeiterResult.Profile;

        var aktiveProfile = alleProfile
            .Where(x => !x.IstGesperrt && x.HatProfil)
            .OrderBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var festangestellteImUrlaubIds = await DienstplanPlanungsHelper.ErmittleFestangestellteImUrlaubUserIdsAsync(
            aktiveProfile,
            query.Tag.Datum,
            _mitarbeiterUrlaubszeitraumRepository,
            cancellationToken);

        var aktiveProfileFuerGrundbesetzung = aktiveProfile
            .Where(x => !festangestellteImUrlaubIds.Contains(x.UserId))
            .ToArray();

        var arztProfileFuerGrundbesetzung = aktiveProfileFuerGrundbesetzung
            .Where(DienstplanPlanungsHelper.IstArzt)
            .ToArray();

        var nfsProfileFuerGrundbesetzung = aktiveProfileFuerGrundbesetzung
            .Where(DienstplanPlanungsHelper.IstNotfallsanitaeter)
            .ToArray();

        var arztProfileFuerVertretung = aktiveProfile
            .Where(DienstplanPlanungsHelper.IstArzt)
            .ToArray();

        var nfsProfileFuerVertretung = aktiveProfile
            .Where(DienstplanPlanungsHelper.IstNotfallsanitaeter)
            .ToArray();

        var profilLookup = alleProfile
            .ToDictionary(x => x.UserId, StringComparer.OrdinalIgnoreCase);

        modal.ArztUserId = query.GeplanterDienstTag?.ArztUserId;
        modal.Notfallsanitaeter1UserId = query.GeplanterDienstTag?.Notfallsanitaeter1UserId;
        modal.Notfallsanitaeter2UserId = query.GeplanterDienstTag?.Notfallsanitaeter2UserId;

        modal.ArztOptionen = BaueMitarbeiterOptionen(
            arztProfileFuerGrundbesetzung,
            query.GeplanterDienstTag?.ArztUserId);

        modal.NotfallsanitaeterOptionen = BaueMitarbeiterOptionen(
            nfsProfileFuerGrundbesetzung,
            null);

        var aktuellGeplanteIds = new[]
            {
                query.GeplanterDienstTag?.ArztUserId,
                query.GeplanterDienstTag?.Notfallsanitaeter1UserId,
                query.GeplanterDienstTag?.Notfallsanitaeter2UserId
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var gewuenschteMitarbeiter = await _dienstwunschRepository.GetAlleFuerTagAsync(
            query.Periode.Id,
            query.Tag.Datum,
            DienstwunschTyp.Wunsch,
            cancellationToken);

        modal.WunschMitarbeiter = gewuenschteMitarbeiter
            .Select(x =>
            {
                if (profilLookup.TryGetValue(x.UserId, out var profil))
                {
                    if (festangestellteImUrlaubIds.Contains(profil.UserId))
                    {
                        return null;
                    }

                    return new WunschMitarbeiterEintragViewModel
                    {
                        UserId = profil.UserId,
                        AnzeigeName = profil.AnzeigeName,
                        Hauptqualifikation = profil.Hauptqualifikation,
                        IstBereitsGeplant = aktuellGeplanteIds.Contains(profil.UserId),
                        KannAlsArztGeplantWerden = DienstplanPlanungsHelper.IstArzt(profil),
                        KannAlsNotfallsanitaeterGeplantWerden = DienstplanPlanungsHelper.IstNotfallsanitaeter(profil)
                    };
                }

                return new WunschMitarbeiterEintragViewModel
                {
                    UserId = x.UserId,
                    AnzeigeName = x.UserId,
                    Hauptqualifikation = "Unbekannt",
                    IstBereitsGeplant = aktuellGeplanteIds.Contains(x.UserId),
                    KannAlsArztGeplantWerden = false,
                    KannAlsNotfallsanitaeterGeplantWerden = false
                };
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderBy(x => x.Hauptqualifikation, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var arztAusfall = query.TagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Arzt);
        var nfs1Ausfall = query.TagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Notfallsanitaeter1);
        var nfs2Ausfall = query.TagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Notfallsanitaeter2);

        var vertreterPraeferenzLookupBySlot = await ErstelleVertreterPraeferenzLookupBySlotAsync(
            query.GeplanterDienstTag,
            arztAusfall,
            nfs1Ausfall,
            nfs2Ausfall,
            arztProfileFuerVertretung,
            nfsProfileFuerVertretung,
            cancellationToken);

        modal.PlanungsSlots = BaueEinfachePlanungsSlots(
            query.GeplanterDienstTag,
            query.TagesAusfaelle,
            profilLookup,
            arztProfileFuerVertretung,
            nfsProfileFuerVertretung,
            vertreterPraeferenzLookupBySlot);

        return modal;
    }

    private sealed class VertreterPraeferenzHinweis
    {
        public decimal SortierBonus { get; init; }

        public string HinweisText { get; init; } = string.Empty;
    }

    private async Task<IReadOnlyDictionary<DienstbesetzungsSlotCode, IReadOnlyDictionary<string, VertreterPraeferenzHinweis>>> ErstelleVertreterPraeferenzLookupBySlotAsync(
        GeplanterDienstTag? geplanterDienstTag,
        GeplanterDienstTagAusfall? arztAusfall,
        GeplanterDienstTagAusfall? nfs1Ausfall,
        GeplanterDienstTagAusfall? nfs2Ausfall,
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> arztProfileFuerVertretung,
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> nfsProfileFuerVertretung,
        CancellationToken cancellationToken)
    {
        var lookup = new Dictionary<DienstbesetzungsSlotCode, IReadOnlyDictionary<string, VertreterPraeferenzHinweis>>();
        var bewertungsDatum = geplanterDienstTag?.DienstDatum;

        lookup[DienstbesetzungsSlotCode.Arzt] = await ErmittleVertreterPraeferenzLookupAsync(
            arztAusfall?.UrspruenglichGeplanterUserId ?? geplanterDienstTag?.ArztUserId,
            DienstbesetzungsSlotCode.Arzt,
            arztAusfall?.AusfallGrundCode,
            bewertungsDatum,
            arztProfileFuerVertretung,
            cancellationToken);

        lookup[DienstbesetzungsSlotCode.Notfallsanitaeter1] = await ErmittleVertreterPraeferenzLookupAsync(
            nfs1Ausfall?.UrspruenglichGeplanterUserId ?? geplanterDienstTag?.Notfallsanitaeter1UserId,
            DienstbesetzungsSlotCode.Notfallsanitaeter1,
            nfs1Ausfall?.AusfallGrundCode,
            bewertungsDatum,
            nfsProfileFuerVertretung,
            cancellationToken);

        lookup[DienstbesetzungsSlotCode.Notfallsanitaeter2] = await ErmittleVertreterPraeferenzLookupAsync(
            nfs2Ausfall?.UrspruenglichGeplanterUserId ?? geplanterDienstTag?.Notfallsanitaeter2UserId,
            DienstbesetzungsSlotCode.Notfallsanitaeter2,
            nfs2Ausfall?.AusfallGrundCode,
            bewertungsDatum,
            nfsProfileFuerVertretung,
            cancellationToken);

        return lookup;
    }

    private async Task<IReadOnlyDictionary<string, VertreterPraeferenzHinweis>> ErmittleVertreterPraeferenzLookupAsync(
        string? urspruenglichGeplanterUserId,
        DienstbesetzungsSlotCode slotCode,
        DienstausfallGrundCode? ausfallGrundCode,
        DateOnly? bewertungsDatum,
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> kandidaten,
        CancellationToken cancellationToken)
    {
        if (kandidaten.Count == 0)
        {
            return new Dictionary<string, VertreterPraeferenzHinweis>(StringComparer.OrdinalIgnoreCase);
        }

        var kandidatIds = kandidaten
            .Select(x => x.UserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (kandidatIds.Length == 0)
        {
            return new Dictionary<string, VertreterPraeferenzHinweis>(StringComparer.OrdinalIgnoreCase);
        }

        var lookup = new Dictionary<string, VertreterPraeferenzHinweis>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(urspruenglichGeplanterUserId))
        {
            var konkreterResult = await _readAutoplanVertreterPraeferenzScoreService.ExecuteAsync(
                new ReadAutoplanVertreterPraeferenzScoreQuery(
                    UrspruenglichGeplanterUserId: urspruenglichGeplanterUserId,
                    BesetzungsSlotCode: slotCode,
                    KandidatenUserIds: kandidatIds,
                    Mindestanzahl: 2,
                    AusfallGrundCode: ausfallGrundCode,
                    BewertungsDatum: bewertungsDatum),
                cancellationToken);

            if (konkreterResult.IsSuccess)
            {
                foreach (var eintrag in konkreterResult.Eintraege.Where(x => x.LernBonus > 0m))
                {
                    lookup[eintrag.KandidatUserId] = new VertreterPraeferenzHinweis
                    {
                        SortierBonus = (eintrag.HatAusfallgrundPraeferenz ? 12m : 10m) + eintrag.LernBonus,
                        HinweisText = BaueVertreterPraeferenzHinweisText(
                            ausfallGrundCode,
                            eintrag.HatAusfallgrundPraeferenz)
                    };
                }
            }
        }

        var allgemeinerResult = await _readAutoplanAllgemeinerVertreterPraeferenzScoreService.ExecuteAsync(
            new ReadAutoplanAllgemeinerVertreterPraeferenzScoreQuery(
                BesetzungsSlotCode: slotCode,
                KandidatenUserIds: kandidatIds,
                Mindestanzahl: 3),
            cancellationToken);

        if (allgemeinerResult.IsSuccess)
        {
            foreach (var eintrag in allgemeinerResult.Eintraege.Where(x => x.LernBonus > 0m))
            {
                if (lookup.ContainsKey(eintrag.KandidatUserId))
                {
                    continue;
                }

                lookup[eintrag.KandidatUserId] = new VertreterPraeferenzHinweis
                {
                    SortierBonus = eintrag.LernBonus,
                    HinweisText = "oft gewählt"
                };
            }
        }

        return lookup;
    }

    private static string BaueVertreterPraeferenzHinweisText(
        DienstausfallGrundCode? ausfallGrundCode,
        bool hatAusfallgrundPraeferenz)
    {
        if (!hatAusfallgrundPraeferenz || ausfallGrundCode is null)
        {
            return "bevorzugt";
        }

        return ausfallGrundCode.Value switch
        {
            DienstausfallGrundCode.Krankheit => "bevorzugt bei Krankheit",
            DienstausfallGrundCode.Urlaub => "bevorzugt bei Urlaub",
            _ => "bevorzugt"
        };
    }

    private static IReadOnlyList<SelectListItem> BaueMitarbeiterOptionen(
        IEnumerable<ItwMitarbeiterprofilUebersichtDto> mitarbeiter,
        string? ausgewaehlteUserId)
    {
        var optionen = new List<SelectListItem>
        {
            new()
            {
                Value = string.Empty,
                Text = "-- bitte auswählen --",
                Selected = string.IsNullOrWhiteSpace(ausgewaehlteUserId)
            }
        };

        optionen.AddRange(
            mitarbeiter
                .OrderBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SelectListItem
                {
                    Value = x.UserId,
                    Text = $"{x.AnzeigeName} ({x.Hauptqualifikation})",
                    Selected = string.Equals(x.UserId, ausgewaehlteUserId, StringComparison.OrdinalIgnoreCase)
                }));

        return optionen;
    }

    private static IReadOnlyList<SelectListItem> BaueVertretungsOptionen(
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> mitarbeiter,
        string? ausgewaehlteId,
        string? urspruenglichGeplanteId,
        IEnumerable<string?> gesperrteVertretungsUserIds,
        IReadOnlyDictionary<string, VertreterPraeferenzHinweis>? vertreterPraeferenzLookup)
    {
        var gesperrteIds = gesperrteVertretungsUserIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(urspruenglichGeplanteId))
        {
            gesperrteIds.Add(urspruenglichGeplanteId);
        }

        if (!string.IsNullOrWhiteSpace(ausgewaehlteId))
        {
            gesperrteIds.Remove(ausgewaehlteId);
        }

        var praeferenzLookup = vertreterPraeferenzLookup
                               ?? new Dictionary<string, VertreterPraeferenzHinweis>(StringComparer.OrdinalIgnoreCase);

        var optionen = new List<SelectListItem>
        {
            new()
            {
                Value = string.Empty,
                Text = "-- keine Vertretung --",
                Selected = string.IsNullOrWhiteSpace(ausgewaehlteId)
            }
        };

        optionen.AddRange(
            mitarbeiter
                .Where(x =>
                    string.Equals(x.UserId, ausgewaehlteId, StringComparison.OrdinalIgnoreCase)
                    || !gesperrteIds.Contains(x.UserId))
                .Select(x =>
                {
                    praeferenzLookup.TryGetValue(x.UserId, out var hinweis);

                    return new
                    {
                        Profil = x,
                        SortierBonus = hinweis?.SortierBonus ?? 0m,
                        hinweis?.HinweisText
                    };
                })
                .OrderByDescending(x => x.SortierBonus)
                .ThenBy(x => x.Profil.AnzeigeName, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SelectListItem
                {
                    Value = x.Profil.UserId,
                    Text = string.IsNullOrWhiteSpace(x.HinweisText)
                        ? $"{x.Profil.AnzeigeName} ({x.Profil.Hauptqualifikation})"
                        : $"{x.Profil.AnzeigeName} ({x.Profil.Hauptqualifikation}) · {x.HinweisText}",
                    Selected = string.Equals(x.Profil.UserId, ausgewaehlteId, StringComparison.OrdinalIgnoreCase)
                }));

        return optionen;
    }

    private static IReadOnlyList<PlanungsSlotViewModel> BaueEinfachePlanungsSlots(
        GeplanterDienstTag? geplanterDienstTag,
        IReadOnlyList<GeplanterDienstTagAusfall> tagesAusfaelle,
        IReadOnlyDictionary<string, ItwMitarbeiterprofilUebersichtDto> profilLookup,
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> arztProfile,
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> nfsProfile,
        IReadOnlyDictionary<DienstbesetzungsSlotCode, IReadOnlyDictionary<string, VertreterPraeferenzHinweis>> vertreterPraeferenzLookupBySlot)
    {
        var arztAusfall = tagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Arzt);
        var nfs1Ausfall = tagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Notfallsanitaeter1);
        var nfs2Ausfall = tagesAusfaelle.FirstOrDefault(x => x.BesetzungsSlotCode == DienstbesetzungsSlotCode.Notfallsanitaeter2);

        return new[]
        {
            BaueEinfachenPlanungsSlot(
                DienstbesetzungsSlotCode.Arzt,
                "Arzt",
                geplanterDienstTag?.ArztUserId,
                arztAusfall,
                profilLookup,
                arztProfile,
                new[]
                {
                    geplanterDienstTag?.Notfallsanitaeter1UserId,
                    geplanterDienstTag?.Notfallsanitaeter2UserId
                },
                vertreterPraeferenzLookupBySlot.TryGetValue(DienstbesetzungsSlotCode.Arzt, out var arztScoreLookup)
                    ? arztScoreLookup
                    : null),

            BaueEinfachenPlanungsSlot(
                DienstbesetzungsSlotCode.Notfallsanitaeter1,
                "Notfallsanitäter 1",
                geplanterDienstTag?.Notfallsanitaeter1UserId,
                nfs1Ausfall,
                profilLookup,
                nfsProfile,
                new[]
                {
                    geplanterDienstTag?.ArztUserId,
                    geplanterDienstTag?.Notfallsanitaeter2UserId
                },
                vertreterPraeferenzLookupBySlot.TryGetValue(DienstbesetzungsSlotCode.Notfallsanitaeter1, out var nfs1ScoreLookup)
                    ? nfs1ScoreLookup
                    : null),

            BaueEinfachenPlanungsSlot(
                DienstbesetzungsSlotCode.Notfallsanitaeter2,
                "Notfallsanitäter 2",
                geplanterDienstTag?.Notfallsanitaeter2UserId,
                nfs2Ausfall,
                profilLookup,
                nfsProfile,
                new[]
                {
                    geplanterDienstTag?.ArztUserId,
                    geplanterDienstTag?.Notfallsanitaeter1UserId
                },
                vertreterPraeferenzLookupBySlot.TryGetValue(DienstbesetzungsSlotCode.Notfallsanitaeter2, out var nfs2ScoreLookup)
                    ? nfs2ScoreLookup
                    : null)
        };
    }

    private static PlanungsSlotViewModel BaueEinfachenPlanungsSlot(
        DienstbesetzungsSlotCode slotCode,
        string slotBezeichnung,
        string? aktuelleUserId,
        GeplanterDienstTagAusfall? ausfall,
        IReadOnlyDictionary<string, ItwMitarbeiterprofilUebersichtDto> profilLookup,
        IReadOnlyList<ItwMitarbeiterprofilUebersichtDto> vertretungsProfile,
        IReadOnlyList<string?> gesperrteVertretungsUserIds,
        IReadOnlyDictionary<string, VertreterPraeferenzHinweis>? vertreterPraeferenzLookup)
    {
        var aktuelleBesetzung = DienstplanPlanungsHelper.ErmittleGeplantenMitarbeiter(profilLookup, aktuelleUserId);

        var urspruenglichGeplanterUserId = ausfall?.UrspruenglichGeplanterUserId ?? aktuelleUserId;

        var urspruenglichGeplanteBesetzung = string.IsNullOrWhiteSpace(urspruenglichGeplanterUserId)
            ? aktuelleBesetzung
            : DienstplanPlanungsHelper.ErmittleGeplantenMitarbeiter(profilLookup, urspruenglichGeplanterUserId);

        return new PlanungsSlotViewModel
        {
            SlotCode = slotCode,
            SlotBezeichnung = slotBezeichnung,
            AktuelleBesetzungAnzeigeName = aktuelleBesetzung.AnzeigeName,
            AktuelleBesetzungQualifikation = aktuelleBesetzung.Hauptqualifikation,
            UrspruenglichGeplanteAnzeigeName = urspruenglichGeplanteBesetzung.AnzeigeName,
            UrspruenglichGeplanteQualifikation = urspruenglichGeplanteBesetzung.Hauptqualifikation,
            HatAusfall = ausfall is not null,
            AusfallGrundCode = ausfall?.AusfallGrundCode,
            StatusText = BestimmeEinfachenSlotStatusText(ausfall),
            VertretungsUserId = ausfall?.VertretungsUserId,
            VertretungsOptionen = BaueVertretungsOptionen(
                vertretungsProfile,
                ausfall?.VertretungsUserId,
                urspruenglichGeplanteId: urspruenglichGeplanterUserId,
                gesperrteVertretungsUserIds,
                vertreterPraeferenzLookup)
        };
    }

    private static string BestimmeEinfachenSlotStatusText(GeplanterDienstTagAusfall? ausfall)
    {
        if (ausfall is null)
        {
            return "Regulär geplant";
        }

        return ausfall.AusfallGrundCode == DienstausfallGrundCode.Krankheit
            ? "Krank gemeldet"
            : "Urlaub eingetragen";
    }
}