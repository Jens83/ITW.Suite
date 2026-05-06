using ITW.Application.Abstractions.DateTime;
using ITW.Application.Aktivitaet;
using ITW.Application.Organisation.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Kalender;
using ITW.Dienstplan.Application.Perioden;
using ITW.Dienstplan.Application.Wunschphase;
using ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Read;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Write;
using ITW.Web.Areas.Intensivtransport.ViewModels.Dienstplan;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using ITW.Web.UI.Feedback;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Dienstplan)]
[Route("Intensivtransport/Dienstplan")]
public sealed class DienstplanWachleiterController : IntensivtransportDienstplanControllerBase
{
    private readonly CreateDienstplanPeriodeService _createDienstplanPeriodeService;
    private readonly SetWunschphaseStatusService _setWunschphaseStatusService;
    private readonly SetPlanfreigabeStatusService _setPlanfreigabeStatusService;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstbesetzungsAusfallRepository _dienstbesetzungsAusfallRepository;
    private readonly ReadWachleiterKalenderService _readWachleiterKalenderService;
    private readonly ReadWachleiterPlanungsModalService _readWachleiterPlanungsModalService;
    private readonly ReadDienstplanIndexViewModelService _readDienstplanIndexViewModelService;
    private readonly SaveWachleiterTagesplanungService _saveWachleiterTagesplanungService;
    private readonly SaveWachleiterAusfallService _saveWachleiterAusfallService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IAktivitaetsLogRepository _aktivitaetsLogRepository;

    public DienstplanWachleiterController(
        CreateDienstplanPeriodeService createDienstplanPeriodeService,
        SetWunschphaseStatusService setWunschphaseStatusService,
        SetPlanfreigabeStatusService setPlanfreigabeStatusService,
        IGeplanterDienstTagRepository geplanterDienstTagRepository,
        IDienstbesetzungsAusfallRepository dienstbesetzungsAusfallRepository,
        ReadWachleiterKalenderService readWachleiterKalenderService,
        ReadWachleiterPlanungsModalService readWachleiterPlanungsModalService,
        ReadDienstplanIndexViewModelService readDienstplanIndexViewModelService,
        SaveWachleiterTagesplanungService saveWachleiterTagesplanungService,
        SaveWachleiterAusfallService saveWachleiterAusfallService,
        IDateTimeProvider dateTimeProvider,
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IAktivitaetsLogRepository aktivitaetsLogRepository,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(createDienstplanPeriodeService);
        _createDienstplanPeriodeService = createDienstplanPeriodeService;

        ArgumentNullException.ThrowIfNull(setWunschphaseStatusService);
        _setWunschphaseStatusService = setWunschphaseStatusService;

        ArgumentNullException.ThrowIfNull(setPlanfreigabeStatusService);
        _setPlanfreigabeStatusService = setPlanfreigabeStatusService;

        ArgumentNullException.ThrowIfNull(geplanterDienstTagRepository);
        _geplanterDienstTagRepository = geplanterDienstTagRepository;

        ArgumentNullException.ThrowIfNull(dienstbesetzungsAusfallRepository);
        _dienstbesetzungsAusfallRepository = dienstbesetzungsAusfallRepository;

        ArgumentNullException.ThrowIfNull(readWachleiterKalenderService);
        _readWachleiterKalenderService = readWachleiterKalenderService;

        ArgumentNullException.ThrowIfNull(readWachleiterPlanungsModalService);
        _readWachleiterPlanungsModalService = readWachleiterPlanungsModalService;

        ArgumentNullException.ThrowIfNull(readDienstplanIndexViewModelService);
        _readDienstplanIndexViewModelService = readDienstplanIndexViewModelService;

        ArgumentNullException.ThrowIfNull(saveWachleiterTagesplanungService);
        _saveWachleiterTagesplanungService = saveWachleiterTagesplanungService;

        ArgumentNullException.ThrowIfNull(saveWachleiterAusfallService);
        _saveWachleiterAusfallService = saveWachleiterAusfallService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;

        ArgumentNullException.ThrowIfNull(dienstplanPeriodeRepository);
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository;

        ArgumentNullException.ThrowIfNull(aktivitaetsLogRepository);
        _aktivitaetsLogRepository = aktivitaetsLogRepository;
    }

    [HttpGet("Wachleiter")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var viewModel = await _readDienstplanIndexViewModelService.ExecuteAsync(
            new ReadDienstplanIndexViewModelQuery(
                zugriff.CurrentUser!,
                _dateTimeProvider.Today.Month,
                _dateTimeProvider.Today.Year,
                WunschphaseDirektOeffnen: true,
                ModalAutomatischOeffnen: false),
            cancellationToken);

        return BereichsView("WachleiterIndex", viewModel);
    }

    [HttpGet("Wachleiterkalender")]
    public async Task<IActionResult> Wachleiterkalender(
        Guid? periodeId,
        bool planungsModal,
        int? planJahr,
        int? planMonat,
        int? planTag,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var kalenderResult = await _readWachleiterKalenderService.ExecuteAsync(
            new ReadWachleiterKalenderQuery(periodeId, _dateTimeProvider.Today),
            cancellationToken);

        var kalenderWochen = MappeWachleiterKalenderWochen(kalenderResult.KalenderWochen);
        var ausgewaehltePeriode = kalenderResult.AusgewaehltePeriode;

        WachleiterPlanungsModalViewModel? planungsModalViewModel = null;

        if (ausgewaehltePeriode is not null &&
            planungsModal &&
            TryBaueDatum(planJahr, planMonat, planTag, out var planDatum) &&
            planDatum.Year == ausgewaehltePeriode.Jahr &&
            planDatum.Month == ausgewaehltePeriode.Monat)
        {
            var tagViewModel = kalenderWochen
                .SelectMany(x => x.Tage)
                .FirstOrDefault(x => x.IstImAktivenMonat && x.Datum == planDatum);

            if (tagViewModel is not null)
            {
                var geplanterDienstTag = await _geplanterDienstTagRepository.GetAsync(
                    ausgewaehltePeriode.Id,
                    planDatum,
                    cancellationToken);

                var tagesAusfaelle = await _dienstbesetzungsAusfallRepository.GetAlleFuerTagAsync(
                    ausgewaehltePeriode.Id,
                    planDatum,
                    cancellationToken);

                planungsModalViewModel = await _readWachleiterPlanungsModalService.ExecuteAsync(
                    new ReadWachleiterPlanungsModalQuery(
                        ausgewaehltePeriode,
                        tagViewModel,
                        geplanterDienstTag,
                        tagesAusfaelle,
                        returnUrl),
                    cancellationToken);
            }
        }

        var viewModel = BaueWachleiterkalenderViewModel(
            kalenderResult,
            kalenderWochen,
            planungsModalViewModel);

        return BereichsView("Wachleiterkalender", viewModel);
    }

    [HttpPost("PeriodeAnlegen")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PeriodeAnlegen(
        DienstplanIndexViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await BaueAktuellesIndexViewModelAsync(
                zugriff.CurrentUser!,
                viewModel.Monat,
                viewModel.Jahr,
                viewModel.WunschphaseDirektOeffnen,
                modalAutomatischOeffnen: true,
                cancellationToken);

            return BereichsView("WachleiterIndex", invalidModel);
        }

        var result = await _createDienstplanPeriodeService.ExecuteAsync(
            new CreateDienstplanPeriodeCommand(
                viewModel.Jahr,
                viewModel.Monat,
                viewModel.WunschphaseDirektOeffnen,
                zugriff.CurrentUser!.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Die Dienstplanperiode konnte nicht angelegt werden.");

            var invalidModel = await BaueAktuellesIndexViewModelAsync(
                zugriff.CurrentUser!,
                viewModel.Monat,
                viewModel.Jahr,
                viewModel.WunschphaseDirektOeffnen,
                modalAutomatischOeffnen: true,
                cancellationToken);

            return BereichsView("WachleiterIndex", invalidModel);
        }

        TempData[FlashKeys.Success] = "Die Dienstplanperiode wurde erfolgreich angelegt.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("WunschphaseStatusAendern")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WunschphaseStatusAendern(
        Guid periodeId,
        bool oeffnen,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _setWunschphaseStatusService.ExecuteAsync(
            new SetWunschphaseStatusCommand(periodeId, oeffnen),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Der Status der Wunschphase konnte nicht geändert werden.";
            return RedirectToLocalOrIndex(returnUrl);
        }

        await LogWunschphaseAktivitaetAsync(periodeId, oeffnen, cancellationToken);

        TempData[FlashKeys.Success] = oeffnen
            ? "Die Wunschphase wurde geöffnet. Andere offene Perioden wurden dabei automatisch geschlossen."
            : "Die Wunschphase wurde geschlossen.";

        return RedirectToLocalOrIndex(returnUrl);
    }

    [HttpPost("PlanFreigabeAendern")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PlanFreigabeAendern(
        Guid periodeId,
        bool freigeben,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _setPlanfreigabeStatusService.ExecuteAsync(
            new SetPlanfreigabeStatusCommand(
                periodeId,
                freigeben,
                zugriff.CurrentUser!.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Die Planfreigabe konnte nicht geändert werden.";
            return RedirectToLocalOrIndex(returnUrl);
        }

        await LogPlanfreigabeAktivitaetAsync(periodeId, freigeben, cancellationToken);

        TempData[FlashKeys.Success] = freigeben
            ? "Der Plan wurde freigegeben und ist jetzt für Mitarbeiter sichtbar."
            : "Die Planfreigabe wurde zurückgenommen.";

        return RedirectToLocalOrIndex(returnUrl);
    }

    [HttpPost("TagesplanungSpeichern")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TagesplanungSpeichern(
        Guid periodeId,
        int jahr,
        int monat,
        int tag,
        string? arztUserId,
        string? notfallsanitaeter1UserId,
        string? notfallsanitaeter2UserId,
        bool planungLeeren,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (periodeId == Guid.Empty)
        {
            TempData[FlashKeys.Error] = "Die ausgewählte Dienstplanperiode ist ungültig.";
            return RedirectToAction(nameof(Wachleiterkalender));
        }

        if (!TryBaueDatum(jahr, monat, tag, out var datum))
        {
            TempData[FlashKeys.Error] = "Der ausgewählte Planungstag ist ungültig.";
            return RedirectToAction(nameof(Wachleiterkalender), new { periodeId });
        }

        var result = await _saveWachleiterTagesplanungService.ExecuteAsync(
            new SaveWachleiterTagesplanungCommand(
                periodeId,
                datum,
                arztUserId,
                notfallsanitaeter1UserId,
                notfallsanitaeter2UserId,
                planungLeeren,
                zugriff.CurrentUser!.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Die Tagesplanung konnte nicht gespeichert werden.";
            return RedirectToPlanungsModal(periodeId, datum, returnUrl);
        }

        TempData[FlashKeys.Success] = "Die Tagesplanung wurde gespeichert.";
        return RedirectToReturnUrlOderPlanungsModal(returnUrl, periodeId, datum);
    }

    [HttpPost("AusfallSpeichern")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AusfallSpeichern(
        Guid periodeId,
        int jahr,
        int monat,
        int tag,
        string slotCode,
        string? ausfallGrundCode,
        string? vertretungsUserId,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (periodeId == Guid.Empty)
        {
            TempData[FlashKeys.Error] = "Die ausgewählte Dienstplanperiode ist ungültig.";
            return RedirectToAction(nameof(Wachleiterkalender));
        }

        if (!TryBaueDatum(jahr, monat, tag, out var datum))
        {
            TempData[FlashKeys.Error] = "Der ausgewählte Planungstag ist ungültig.";
            return RedirectToAction(nameof(Wachleiterkalender), new { periodeId });
        }

        var result = await _saveWachleiterAusfallService.ExecuteAsync(
            new SaveWachleiterAusfallCommand(
                periodeId,
                datum,
                slotCode,
                ausfallGrundCode,
                vertretungsUserId,
                zugriff.CurrentUser!.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Der Ausfall konnte nicht gespeichert werden.";
            return RedirectToPlanungsModal(periodeId, datum, returnUrl);
        }

        TempData[FlashKeys.Success] = "Ausfall und Vertretung wurden gespeichert.";
        return RedirectToReturnUrlOderPlanungsModal(returnUrl, periodeId, datum);
    }

    private async Task LogWunschphaseAktivitaetAsync(
        Guid periodeId,
        bool oeffnen,
        CancellationToken cancellationToken)
    {
        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(periodeId, cancellationToken);
        if (periode is null) return;

        var text = oeffnen
            ? $"Wunschphase <strong>{periode.Bezeichnung}</strong> wurde geöffnet."
            : $"Wunschphase <strong>{periode.Bezeichnung}</strong> wurde geschlossen.";
        var icon     = oeffnen ? "bi bi-calendar-plus" : "bi bi-calendar-minus";
        var kategorie = oeffnen ? AktivitaetsKategorie.Info : AktivitaetsKategorie.Warnung;

        var eintrag = new AktivitaetsEintrag(
            Guid.NewGuid(),
            OrganisationsbereichCode.Intensivtransport,
            text,
            kategorie,
            icon,
            DateTimeOffset.UtcNow);

        await _aktivitaetsLogRepository.AddAsync(eintrag, cancellationToken);
        await _aktivitaetsLogRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task LogPlanfreigabeAktivitaetAsync(
        Guid periodeId,
        bool freigeben,
        CancellationToken cancellationToken)
    {
        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(periodeId, cancellationToken);
        if (periode is null) return;

        var text = freigeben
            ? $"Dienstplan <strong>{periode.Bezeichnung}</strong> wurde freigegeben."
            : $"Dienstplan <strong>{periode.Bezeichnung}</strong> – Freigabe zurückgenommen.";
        var icon      = freigeben ? "bi bi-unlock" : "bi bi-lock";
        var kategorie = freigeben ? AktivitaetsKategorie.Erfolg : AktivitaetsKategorie.Warnung;

        var eintrag = new AktivitaetsEintrag(
            Guid.NewGuid(),
            OrganisationsbereichCode.Intensivtransport,
            text,
            kategorie,
            icon,
            DateTimeOffset.UtcNow);

        await _aktivitaetsLogRepository.AddAsync(eintrag, cancellationToken);
        await _aktivitaetsLogRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<DienstplanIndexViewModel> BaueAktuellesIndexViewModelAsync(
        CurrentUserContext currentUser,
        int monat,
        int jahr,
        bool wunschphaseDirektOeffnen,
        bool modalAutomatischOeffnen,
        CancellationToken cancellationToken)
    {
        return await _readDienstplanIndexViewModelService.ExecuteAsync(
            new ReadDienstplanIndexViewModelQuery(
                currentUser,
                monat,
                jahr,
                wunschphaseDirektOeffnen,
                modalAutomatischOeffnen),
            cancellationToken);
    }

    private static WachleiterWunschkalenderViewModel BaueWachleiterkalenderViewModel(
        ReadWachleiterKalenderResult kalenderResult,
        IReadOnlyList<DienstplanKalenderWocheViewModel> kalenderWochen,
        WachleiterPlanungsModalViewModel? planungsModalViewModel)
    {
        var ausgewaehltePeriode = kalenderResult.AusgewaehltePeriode;

        return new WachleiterWunschkalenderViewModel
        {
            Titel = "Wachleiterkalender",
            Beschreibung = "Kalenderübersicht mit Wünschen, Besetzung sowie Krank-/Urlaubs- und Vertretungsverwaltung pro Tag.",
            IsSuccess = kalenderResult.IsSuccess,
            ErrorMessage = kalenderResult.ErrorMessage,
            AusgewaehltePeriodeId = ausgewaehltePeriode?.Id,
            AusgewaehltePeriodeBezeichnung = ausgewaehltePeriode?.Bezeichnung ?? string.Empty,
            WunschphaseIstOffen = ausgewaehltePeriode?.WunschphaseIstOffen ?? false,
            PlanIstFreigegeben = ausgewaehltePeriode?.PlanIstFreigegeben ?? false,
            PlanFreigegebenAm = ausgewaehltePeriode?.PlanFreigegebenAm,
            PlanungsModalAutomatischOeffnen = planungsModalViewModel is not null,
            PlanungsModal = planungsModalViewModel,
            PeriodenOptionen = kalenderResult.Perioden
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Bezeichnung,
                    Selected = x.Id == ausgewaehltePeriode?.Id
                })
                .ToArray(),
            KalenderWochen = kalenderWochen
        };
    }

    private static IReadOnlyList<DienstplanKalenderWocheViewModel> MappeWachleiterKalenderWochen(
        IReadOnlyList<WachleiterKalenderWocheEintrag> kalenderWochen)
    {
        return kalenderWochen
            .Select(woche => new DienstplanKalenderWocheViewModel
            {
                Tage = woche.Tage
                    .Select(MappeWachleiterKalenderTag)
                    .ToArray()
            })
            .ToArray();
    }

    private static DienstplanKalenderTagViewModel MappeWachleiterKalenderTag(
        WachleiterKalenderTagEintrag tag)
    {
        return new DienstplanKalenderTagViewModel
        {
            Datum = tag.Datum,
            IstImAktivenMonat = tag.IstImAktivenMonat,
            IstHeute = tag.IstHeute,
            IstAlsWunschMarkiert = tag.IstAlsWunschMarkiert,
            IstAlsNichtVerfuegbarMarkiert = tag.IstAlsNichtVerfuegbarMarkiert,
            IstFeiertag = tag.IstFeiertag,
            IstWochenende = tag.IstWochenende,
            IstAuswaehlbar = tag.IstAuswaehlbar,
            FeiertagsName = tag.FeiertagsName,
            AnzahlWuenscheGesamt = tag.AnzahlWuenscheGesamt,
            AnzahlAerzte = tag.AnzahlAerzte,
            AnzahlNotfallsanitaeter = tag.AnzahlNotfallsanitaeter,
            AnzahlGeplanteAerzte = tag.AnzahlGeplanteAerzte,
            AnzahlGeplanteNotfallsanitaeter = tag.AnzahlGeplanteNotfallsanitaeter,
            HatAutomatikKonflikt = tag.HatAutomatikKonflikt,
            AutomatikKonfliktText = tag.AutomatikKonfliktText
        };
    }  

    private IActionResult RedirectToPlanungsModal(Guid periodeId, DateOnly datum, string? returnUrl = null)
    {
        return RedirectToAction(nameof(Wachleiterkalender), new
        {
            periodeId,
            planungsModal = true,
            planJahr = datum.Year,
            planMonat = datum.Month,
            planTag = datum.Day,
            returnUrl
        });
    }

    private IActionResult RedirectToReturnUrlOderPlanungsModal(string? returnUrl, Guid periodeId, DateOnly datum)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToPlanungsModal(periodeId, datum);
    }

    private IActionResult RedirectToLocalOrIndex(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToDienstplanEinstieg();
    }
}