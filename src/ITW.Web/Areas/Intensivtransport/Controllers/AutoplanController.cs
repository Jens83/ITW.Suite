using ITW.Application.Organisation.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Perioden;
using ITW.Dienstplan.Application.Planung;
using ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;
using ITW.Web.Authorization.Modules;
using ITW.Web.Controllers.Base;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;
using ITW.Web.UI.Feedback;

namespace ITW.Web.Areas.Intensivtransport.Controllers;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Dienstplan)]
public sealed class AutoplanController : BereichsControllerBase
{
    private const string AreaLayoutPath = "~/Views/Shared/_AppLayout.cshtml";
    private static readonly CultureInfo DeutscheKultur = CultureInfo.GetCultureInfo("de-DE");

    private readonly ReadDienstplanperiodenService _readDienstplanperiodenService;
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IDienstwunschRepository _dienstwunschRepository;
    private readonly IFreelancerMonatswunschRepository _freelancerMonatswunschRepository;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstbesetzungsAusfallRepository _dienstbesetzungsAusfallRepository;

    public AutoplanController(
        ReadDienstplanperiodenService readDienstplanperiodenService,
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IDienstwunschRepository dienstwunschRepository,
        IFreelancerMonatswunschRepository freelancerMonatswunschRepository,
        IGeplanterDienstTagRepository geplanterDienstTagRepository,
        IDienstbesetzungsAusfallRepository dienstbesetzungsAusfallRepository,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readDienstplanperiodenService);
        _readDienstplanperiodenService = readDienstplanperiodenService;

        ArgumentNullException.ThrowIfNull(dienstplanPeriodeRepository);
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository;

        ArgumentNullException.ThrowIfNull(dienstwunschRepository);
        _dienstwunschRepository = dienstwunschRepository;

        ArgumentNullException.ThrowIfNull(freelancerMonatswunschRepository);
        _freelancerMonatswunschRepository = freelancerMonatswunschRepository;

        ArgumentNullException.ThrowIfNull(geplanterDienstTagRepository);
        _geplanterDienstTagRepository = geplanterDienstTagRepository;

        ArgumentNullException.ThrowIfNull(dienstbesetzungsAusfallRepository);
        _dienstbesetzungsAusfallRepository = dienstbesetzungsAusfallRepository;
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    public async Task<IActionResult> Index(
    Guid? periodeId,
    AutomatischePlanungAusfuehrungsmodus? modus,
    [FromServices] IDienstplanMitarbeiterPlanungsRepository dienstplanMitarbeiterPlanungsRepository,
    [FromServices] IDienstplanUrlaubszeitraumRepository dienstplanUrlaubszeitraumRepository,
    [FromServices] IAutoplanLernereignisRepository autoplanLernereignisRepository,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeAutoplanZugriffAsync(cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var readPeriodenResult = await _readDienstplanperiodenService.ExecuteAsync(cancellationToken);

        var viewModel = await BaueViewModelAsync(
            readPeriodenResult,
            periodeId,
            modus ?? AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen,
            dienstplanMitarbeiterPlanungsRepository,
            dienstplanUrlaubszeitraumRepository,
            autoplanLernereignisRepository,
            cancellationToken);

        return BereichsView("Index", viewModel);
    }

    public async Task<IActionResult> Ausfuehren(
    Guid periodeId,
    AutomatischePlanungAusfuehrungsmodus modus,
    [FromServices] IDienstplanMitarbeiterPlanungsRepository dienstplanMitarbeiterPlanungsRepository,
    [FromServices] IDienstplanUrlaubszeitraumRepository dienstplanUrlaubszeitraumRepository,
    [FromServices] IAutoplanLernereignisRepository autoplanLernereignisRepository,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeAutoplanZugriffAsync(cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (periodeId == Guid.Empty)
        {
            TempData[FlashKeys.Error] = "Die ausgewählte Dienstplanperiode ist ungültig.";
            return RedirectToAction(nameof(Index), new { modus });
        }

        var service = new GenerateAutomatischePlanungService(
            _dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository,
            _dienstwunschRepository,
            _freelancerMonatswunschRepository,
            _geplanterDienstTagRepository,
            _dienstbesetzungsAusfallRepository,
            dienstplanUrlaubszeitraumRepository,
            autoplanLernereignisRepository);

        var result = await service.ExecuteAsync(
            new GenerateAutomatischePlanungCommand(
                periodeId,
                zugriff.CurrentUser!.UserId,
                modus),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Der Autoplan konnte nicht gespeichert werden.";
            return RedirectToAction(nameof(Index), new { periodeId, modus });
        }

        var modusText = modus == AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen
            ? "nur offene Slots gefüllt"
            : "Periode komplett neu berechnet";

        TempData[FlashKeys.Success] =
            $"Autoplan gespeichert ({modusText}): {result.AnzahlGespeicherteTage} Tage gesetzt, {result.AnzahlTageMitOffenenSlots} Tage weiterhin offen, {result.AnzahlKonflikttage} Konflikttage.";

        return RedirectToAction(nameof(Index), new { periodeId, modus });
    }

    private async Task<AutoplanIndexViewModel> BaueViewModelAsync(
ReadDienstplanperiodenResult readPeriodenResult,
Guid? periodeId,
AutomatischePlanungAusfuehrungsmodus modus,
IDienstplanMitarbeiterPlanungsRepository dienstplanMitarbeiterPlanungsRepository,
IDienstplanUrlaubszeitraumRepository dienstplanUrlaubszeitraumRepository,
IAutoplanLernereignisRepository autoplanLernereignisRepository,
CancellationToken cancellationToken)
    {
        var perioden = readPeriodenResult.Perioden;

        DienstplanPeriodeListenEintrag? ausgewaehltePeriode = null;

        if (periodeId.HasValue && periodeId.Value != Guid.Empty)
        {
            ausgewaehltePeriode = perioden.FirstOrDefault(x => x.Id == periodeId.Value);
        }

        ausgewaehltePeriode ??= perioden.FirstOrDefault(x => x.WunschphaseIstOffen);
        ausgewaehltePeriode ??= perioden.FirstOrDefault();

        var viewModel = new AutoplanIndexViewModel
        {
            Titel = "Autoplan",
            Beschreibung = "Der Autoplan zeigt verständlich, welche Tage bereits gut aussehen, wo nachgearbeitet werden muss und welche Änderungen beim Speichern entstehen würden.",
            IsSuccess = readPeriodenResult.IsSuccess,
            ErrorMessage = readPeriodenResult.ErrorMessage,
            AusgewaehltePeriodeId = ausgewaehltePeriode?.Id,
            AusgewaehltePeriodeBezeichnung = ausgewaehltePeriode?.Bezeichnung ?? string.Empty,
            WunschphaseIstOffen = ausgewaehltePeriode?.WunschphaseIstOffen ?? false,
            PlanIstFreigegeben = ausgewaehltePeriode?.PlanIstFreigegeben ?? false,
            PlanFreigegebenAm = ausgewaehltePeriode?.PlanFreigegebenAm,
            AusgewaehlterModus = modus,
            ModusOptionen = BaueModusOptionen(modus),
            PeriodenOptionen = perioden
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Bezeichnung,
                    Selected = ausgewaehltePeriode is not null && x.Id == ausgewaehltePeriode.Id
                })
                .ToArray()
        };

        if (ausgewaehltePeriode is null)
        {
            viewModel.IsSuccess = false;
            viewModel.ErrorMessage ??= "Es ist noch keine Dienstplanperiode vorhanden.";
            return viewModel;
        }

        var grundlagenService = new ReadAutomatischePlanungsgrundlageService(
            _dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository,
            _dienstwunschRepository,
            _freelancerMonatswunschRepository);

        var grundlagenResult = await grundlagenService.ExecuteAsync(
            ausgewaehltePeriode.Id,
            cancellationToken);

        if (!grundlagenResult.IsSuccess)
        {
            viewModel.IsSuccess = false;
            viewModel.ErrorMessage = grundlagenResult.ErrorMessage ?? "Die Autoplan-Grundlage konnte nicht geladen werden.";
            return viewModel;
        }

        var vorschauService = new ReadAutomatischePlanungsvorschauService(
            _dienstplanPeriodeRepository,
            dienstplanMitarbeiterPlanungsRepository,
            _dienstwunschRepository,
            _freelancerMonatswunschRepository,
            _geplanterDienstTagRepository,
            dienstplanUrlaubszeitraumRepository,
            autoplanLernereignisRepository);

        var vorschauResult = await vorschauService.ExecuteAsync(
            ausgewaehltePeriode.Id,
            modus,
            cancellationToken);

        if (!vorschauResult.IsSuccess)
        {
            viewModel.IsSuccess = false;
            viewModel.ErrorMessage = vorschauResult.ErrorMessage ?? "Die Autoplan-Vorschau konnte nicht geladen werden.";
            return viewModel;
        }

        var mitarbeiterLookup = grundlagenResult.Mitarbeiter.ToDictionary(
            x => x.UserId,
            x => new MitarbeiterAnzeigeInfo(
                x.AnzeigeName,
                x.HauptqualifikationBezeichnung),
            StringComparer.OrdinalIgnoreCase);

        var tage = vorschauResult.Tage
            .OrderBy(x => x.Datum)
            .Select(x =>
            {
                var hatVollstaendigeBesatzung =
                    !string.IsNullOrWhiteSpace(x.ArztUserId) &&
                    !string.IsNullOrWhiteSpace(x.Notfallsanitaeter1UserId) &&
                    !string.IsNullOrWhiteSpace(x.Notfallsanitaeter2UserId);

                var slotEntscheidungen = BaueSlotEntscheidungen(x.SlotEntscheidungen, mitarbeiterLookup);
                var konflikte = x.Konflikte
                    .Select(BaueKonfliktViewModel)
                    .ToArray();

                var statusText = x.HatKonflikt
                    ? "Konflikt vorhanden"
                    : hatVollstaendigeBesatzung
                        ? "Tag sieht gut aus"
                        : "Noch unvollständig";

                var statusCssClass = x.HatKonflikt
                    ? "app-pill--danger"
                    : hatVollstaendigeBesatzung
                        ? "app-pill--success"
                        : "app-pill--warning";

                var handlungsstatus = ErmittleHandlungsstatus(hatVollstaendigeBesatzung, x.HatKonflikt);
                var kurzbeschreibung = ErmittleKurzbeschreibung(hatVollstaendigeBesatzung, x.HatKonflikt, slotEntscheidungen);

                return new AutoplanTagViewModel
                {
                    Datum = x.Datum,
                    DatumAnzeige = x.Datum.ToDateTime(TimeOnly.MinValue).ToString("dddd, dd.MM.yyyy", DeutscheKultur),
                    HatVollstaendigeBesatzung = hatVollstaendigeBesatzung,
                    HatKonflikt = x.HatKonflikt,
                    StatusText = statusText,
                    StatusCssClass = statusCssClass,
                    HandlungsstatusText = handlungsstatus.Text,
                    HandlungsstatusCssClass = handlungsstatus.CssClass,
                    Kurzbeschreibung = kurzbeschreibung,
                    SlotEntscheidungen = slotEntscheidungen,
                    Konflikte = konflikte
                };
            })
            .ToArray();

        var alleSlots = tage.SelectMany(x => x.SlotEntscheidungen).ToArray();

        viewModel.VorschauVerfuegbar = true;
        viewModel.Tage = tage;
        viewModel.AnzahlPlanbareTage = tage.Length;
        viewModel.AnzahlTageOhneHandlungsbedarf = tage.Count(x => !x.HatKonflikt && x.HatVollstaendigeBesatzung);
        viewModel.AnzahlTageMitNacharbeit = tage.Count(x => !x.HatKonflikt && !x.HatVollstaendigeBesatzung);
        viewModel.AnzahlKritischeTage = tage.Count(x => x.HatKonflikt);
        viewModel.AnzahlUnveraenderteSlots = alleSlots.Count(x => x.AenderungsLabel == "Bleibt wie bisher");
        viewModel.AnzahlNeuGesetzteSlots = alleSlots.Count(x => x.AenderungsLabel == "Neu dazu");
        viewModel.AnzahlErsetzteSlots = alleSlots.Count(x => x.AenderungsLabel == "Wird ausgetauscht");
        viewModel.AnzahlWirdOffenSlots = alleSlots.Count(x => x.AenderungsLabel == "Fällt weg");
        viewModel.AnzahlBleibtOffenSlots = alleSlots.Count(x => x.AenderungsLabel == "Bleibt offen");

        return viewModel;
    }

    private async Task<AutoplanZugriffResult> PruefeAutoplanZugriffAsync(
        CancellationToken cancellationToken)
    {
        var result = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        if (!result.IsSuccess || result.CurrentUser is null)
        {
            return AutoplanZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        var aktuellerBenutzer = result.CurrentUser;

        if (aktuellerBenutzer.Bereich != Bereich)
        {
            return AutoplanZugriffResult.MitErgebnis(
                ErzeugeBereichsWeiterleitung(
                    aktuellerBenutzer.Bereich,
                    "Dashboard",
                    "Index",
                    "Autoplan",
                    BereichsRoutingHelper.GetBereichsname(Bereich),
                    "Der Autoplan steht aktuell nur im Bereich Intensivtransport zur Verfügung. Ihr zuständiger Bereich wird jetzt geöffnet."));
        }

        if (aktuellerBenutzer.Rolle != BereichsrolleCode.Wachleiter)
        {
            return AutoplanZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        return AutoplanZugriffResult.MitBenutzer(aktuellerBenutzer);
    }

    private IActionResult BereichsView(string viewName, object model)
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
        return View($"~/Areas/Intensivtransport/Views/Autoplan/{viewName}.cshtml", model);
    }

    private static IReadOnlyList<SelectListItem> BaueModusOptionen(
        AutomatischePlanungAusfuehrungsmodus ausgewaehlterModus)
    {
        return new[]
        {
            new SelectListItem
            {
                Value = AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen.ToString(),
                Text = "Nur offene Slots füllen",
                Selected = ausgewaehlterModus == AutomatischePlanungAusfuehrungsmodus.NurOffeneSlotsFuellen
            },
            new SelectListItem
            {
                Value = AutomatischePlanungAusfuehrungsmodus.KomplettePeriodeNeuBerechnen.ToString(),
                Text = "Komplette Periode neu berechnen",
                Selected = ausgewaehlterModus == AutomatischePlanungAusfuehrungsmodus.KomplettePeriodeNeuBerechnen
            }
        };
    }

    private static IReadOnlyList<AutoplanSlotEntscheidungViewModel> BaueSlotEntscheidungen(
        IReadOnlyList<AutomatischePlanungSlotEntscheidung> slotEntscheidungen,
        IReadOnlyDictionary<string, MitarbeiterAnzeigeInfo> mitarbeiterLookup)
    {
        return slotEntscheidungen
            .OrderBy(x => BestimmeSlotSortierung(x.SlotBezeichnung))
            .Select(x =>
            {
                var anzeigeName = !string.IsNullOrWhiteSpace(x.UserId) && mitarbeiterLookup.TryGetValue(x.UserId, out var info)
                    ? info.AnzeigeName
                    : string.IsNullOrWhiteSpace(x.UserId)
                        ? "Offen"
                        : x.UserId!;

                var hauptqualifikation = !string.IsNullOrWhiteSpace(x.UserId) && mitarbeiterLookup.TryGetValue(x.UserId, out var infoFuerQuali)
                    ? infoFuerQuali.Hauptqualifikation
                    : string.Empty;

                var vorherigeAnzeige = !string.IsNullOrWhiteSpace(x.VorherigerUserId) && mitarbeiterLookup.TryGetValue(x.VorherigerUserId, out var vorherigeInfo)
                    ? vorherigeInfo.AnzeigeName
                    : x.VorherigerUserId ?? string.Empty;

                return new AutoplanSlotEntscheidungViewModel
                {
                    SlotBezeichnung = x.SlotBezeichnung,
                    AnzeigeName = anzeigeName,
                    Hauptqualifikation = hauptqualifikation,
                    IstOffen = string.IsNullOrWhiteSpace(x.UserId),
                    HatVorherigeBesetzung = !string.IsNullOrWhiteSpace(x.VorherigerUserId),
                    VorherigeAnzeigeName = vorherigeAnzeige,
                    ZuweisungsLabel = FormatiereZuweisungsLabel(x.Art),
                    ZuweisungsCssClass = ErmittleZuweisungsCssClass(x.Art),
                    AenderungsLabel = FormatiereAenderungsLabel(x.AenderungsArt),
                    AenderungsCssClass = ErmittleAenderungsCssClass(x.AenderungsArt),
                    Nachricht = x.Nachricht
                };
            })
            .ToArray();
    }

    private static int BestimmeSlotSortierung(string slotBezeichnung)
    {
        return slotBezeichnung switch
        {
            "Arzt" => 1,
            "NFS 1" => 2,
            "NFS 2" => 3,
            _ => 99
        };
    }

    private static AutoplanKonfliktViewModel BaueKonfliktViewModel(
        AutomatischePlanungKonfliktEintrag konflikt)
    {
        return new AutoplanKonfliktViewModel
        {
            Label = FormatiereKonfliktLabel(konflikt.Art),
            CssClass = ErmittleKonfliktCssClass(konflikt.Art),
            Nachricht = konflikt.Nachricht
        };
    }

    private static (string Text, string CssClass) ErmittleHandlungsstatus(bool hatVollstaendigeBesatzung, bool hatKonflikt)
    {
        if (hatKonflikt && !hatVollstaendigeBesatzung)
        {
            return ("Sofort prüfen", "app-pill--danger");
        }

        if (hatKonflikt)
        {
            return ("Bitte prüfen", "app-pill--warning");
        }

        if (!hatVollstaendigeBesatzung)
        {
            return ("Nacharbeit nötig", "app-pill--warning");
        }

        return ("Kein Eingreifen nötig", "app-pill--success");
    }

    private static string ErmittleKurzbeschreibung(
        bool hatVollstaendigeBesatzung,
        bool hatKonflikt,
        IReadOnlyList<AutoplanSlotEntscheidungViewModel> slotEntscheidungen)
    {
        var neu = slotEntscheidungen.Count(x => x.AenderungsLabel == "Neu dazu");
        var ersetzt = slotEntscheidungen.Count(x => x.AenderungsLabel == "Wird ausgetauscht");
        var offen = slotEntscheidungen.Count(x =>
            x.AenderungsLabel == "Bleibt offen" || x.AenderungsLabel == "Fällt weg");

        if (hatKonflikt && !hatVollstaendigeBesatzung)
        {
            return "Hier reicht die automatische Besetzung noch nicht aus. Dieser Tag sollte manuell geprüft werden.";
        }

        if (hatKonflikt)
        {
            return "Die Besetzung steht, aber mindestens ein Konflikthinweis sollte geprüft werden.";
        }

        if (!hatVollstaendigeBesatzung)
        {
            return "Der Autoplan hat noch nicht alle Rollen besetzen können. Hier ist manuelle Nacharbeit nötig.";
        }

        if (ersetzt > 0)
        {
            return "Die Besetzung wäre vollständig, aber mindestens eine vorhandene Planung würde verändert.";
        }

        if (neu > 0)
        {
            return "Die Besetzung wäre vollständig und der Autoplan ergänzt passende Mitarbeiter.";
        }

        if (offen == 0)
        {
            return "Für diesen Tag ergibt sich aktuell kein zusätzlicher Handlungsbedarf.";
        }

        return "Der Tag sollte kurz geprüft werden.";
    }

    private static string FormatiereZuweisungsLabel(AutomatischePlanungZuweisungsArt art)
    {
        return art switch
        {
            AutomatischePlanungZuweisungsArt.BestehendePlanung => "Bereits gesetzt",
            AutomatischePlanungZuweisungsArt.PflichtfallFreelancer => "Pflichtfall",
            AutomatischePlanungZuweisungsArt.FreelancerNachTageskritik => "Wunschtag priorisiert",
            AutomatischePlanungZuweisungsArt.HonorarkraftWunsch => "Wunsch berücksichtigt",
            AutomatischePlanungZuweisungsArt.MitarbeiterWunsch => "Wunsch berücksichtigt",
            AutomatischePlanungZuweisungsArt.MitarbeiterWunschblock => "Block bevorzugt",
            AutomatischePlanungZuweisungsArt.MitarbeiterLueckenfueller => "Lücke gefüllt",
            _ => "Automatisch gesetzt"
        };
    }

    private static string ErmittleZuweisungsCssClass(AutomatischePlanungZuweisungsArt art)
    {
        return art switch
        {
            AutomatischePlanungZuweisungsArt.BestehendePlanung => "app-pill--neutral",
            AutomatischePlanungZuweisungsArt.PflichtfallFreelancer => "app-pill--danger",
            AutomatischePlanungZuweisungsArt.FreelancerNachTageskritik => "app-pill--warning",
            AutomatischePlanungZuweisungsArt.HonorarkraftWunsch => "app-pill--accent",
            AutomatischePlanungZuweisungsArt.MitarbeiterWunsch => "app-pill--accent",
            AutomatischePlanungZuweisungsArt.MitarbeiterWunschblock => "app-pill--success",
            AutomatischePlanungZuweisungsArt.MitarbeiterLueckenfueller => "app-pill--neutral",
            _ => "app-pill--neutral"
        };
    }

    private static string FormatiereAenderungsLabel(AutomatischePlanungAenderungsArt art)
    {
        return art switch
        {
            AutomatischePlanungAenderungsArt.Unveraendert => "Bleibt wie bisher",
            AutomatischePlanungAenderungsArt.NeuGesetzt => "Neu dazu",
            AutomatischePlanungAenderungsArt.Ersetzt => "Wird ausgetauscht",
            AutomatischePlanungAenderungsArt.WirdOffen => "Fällt weg",
            AutomatischePlanungAenderungsArt.BleibtOffen => "Bleibt offen",
            _ => "Änderung"
        };
    }

    private static string ErmittleAenderungsCssClass(AutomatischePlanungAenderungsArt art)
    {
        return art switch
        {
            AutomatischePlanungAenderungsArt.Unveraendert => "app-pill--neutral",
            AutomatischePlanungAenderungsArt.NeuGesetzt => "app-pill--success",
            AutomatischePlanungAenderungsArt.Ersetzt => "app-pill--warning",
            AutomatischePlanungAenderungsArt.WirdOffen => "app-pill--danger",
            AutomatischePlanungAenderungsArt.BleibtOffen => "app-pill--neutral",
            _ => "app-pill--neutral"
        };
    }

    private static string FormatiereKonfliktLabel(AutomatischePlanungKonfliktArt art)
    {
        return art switch
        {
            AutomatischePlanungKonfliktArt.MindestbesetzungNichtErreichbar => "Besetzung fehlt",
            AutomatischePlanungKonfliktArt.MehrPflichtfaelleAlsFreieSlots => "Zu viele Pflichtfälle",
            AutomatischePlanungKonfliktArt.FreelancerSollNichtErreichbar => "Freelancer-Ziel nicht mehr erreichbar",
            AutomatischePlanungKonfliktArt.FreelancerDienstNichtKonfliktfreiVerteilbar => "Freelancer konnte nicht sauber verteilt werden",
            AutomatischePlanungKonfliktArt.BestehendePlanungKollidiertMitUrlaub => "Urlaub kollidiert",
            _ => "Konflikt"
        };
    }

    private static string ErmittleKonfliktCssClass(AutomatischePlanungKonfliktArt art)
    {
        return art switch
        {
            AutomatischePlanungKonfliktArt.MindestbesetzungNichtErreichbar => "app-pill--danger",
            AutomatischePlanungKonfliktArt.MehrPflichtfaelleAlsFreieSlots => "app-pill--danger",
            AutomatischePlanungKonfliktArt.FreelancerSollNichtErreichbar => "app-pill--warning",
            AutomatischePlanungKonfliktArt.FreelancerDienstNichtKonfliktfreiVerteilbar => "app-pill--warning",
            AutomatischePlanungKonfliktArt.BestehendePlanungKollidiertMitUrlaub => "app-pill--neutral",
            _ => "app-pill--neutral"
        };
    }

    private sealed record MitarbeiterAnzeigeInfo(
        string AnzeigeName,
        string Hauptqualifikation);

    private sealed record AutoplanZugriffResult(CurrentUserContext? CurrentUser, IActionResult? EarlyResult)
    {
        public static AutoplanZugriffResult MitBenutzer(CurrentUserContext currentUser)
            => new(currentUser, null);

        public static AutoplanZugriffResult MitErgebnis(IActionResult earlyResult)
            => new(null, earlyResult);
    }
}