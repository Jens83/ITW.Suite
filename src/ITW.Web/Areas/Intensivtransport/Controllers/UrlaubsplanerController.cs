using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Dienstplan.Application.Contracts;
using ITW.Web.Areas.Intensivtransport.ViewModels.Urlaubsplaner;
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
public sealed class UrlaubsplanerController : BereichsControllerBase
{
    private const string AreaLayoutPath = "~/Views/Shared/_AppLayout.cshtml";
    private static readonly CultureInfo DeutscheKultur = CultureInfo.GetCultureInfo("de-DE");

    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly IMitarbeiterUrlaubsanspruchRepository _urlaubsanspruchRepository;
    private readonly IMitarbeiterUrlaubszeitraumRepository _urlaubszeitraumRepository;
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IDienstwunschRepository _dienstwunschRepository;
    private readonly SaveMitarbeiterUrlaubsanspruchService _saveAnspruchService;
    private readonly SaveMitarbeiterUrlaubszeitraumService _saveZeitraumService;
    private readonly DeleteMitarbeiterUrlaubszeitraumService _deleteZeitraumService;
    private readonly ReadMitarbeiterUrlaubsplanerService _readUrlaubsplanerService;
    private readonly ReadWachleiterUrlaubsUebersichtService _readUebersichtService;
    private readonly EntscheidenUrlaubsAntragService _entscheidenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UrlaubsplanerController(
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        IMitarbeiterUrlaubsanspruchRepository urlaubsanspruchRepository,
        IMitarbeiterUrlaubszeitraumRepository urlaubszeitraumRepository,
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IDienstwunschRepository dienstwunschRepository,
        SaveMitarbeiterUrlaubsanspruchService saveAnspruchService,
        SaveMitarbeiterUrlaubszeitraumService saveZeitraumService,
        DeleteMitarbeiterUrlaubszeitraumService deleteZeitraumService,
        ReadMitarbeiterUrlaubsplanerService readUrlaubsplanerService,
        ReadWachleiterUrlaubsUebersichtService readUebersichtService,
        EntscheidenUrlaubsAntragService entscheidenService,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readItwMitarbeiterprofileService);
        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService;
        ArgumentNullException.ThrowIfNull(urlaubsanspruchRepository);
        _urlaubsanspruchRepository = urlaubsanspruchRepository;
        ArgumentNullException.ThrowIfNull(urlaubszeitraumRepository);
        _urlaubszeitraumRepository = urlaubszeitraumRepository;
        ArgumentNullException.ThrowIfNull(dienstplanPeriodeRepository);
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository;
        ArgumentNullException.ThrowIfNull(dienstwunschRepository);
        _dienstwunschRepository = dienstwunschRepository;
        ArgumentNullException.ThrowIfNull(saveAnspruchService);
        _saveAnspruchService = saveAnspruchService;
        ArgumentNullException.ThrowIfNull(saveZeitraumService);
        _saveZeitraumService = saveZeitraumService;
        ArgumentNullException.ThrowIfNull(deleteZeitraumService);
        _deleteZeitraumService = deleteZeitraumService;
        ArgumentNullException.ThrowIfNull(readUrlaubsplanerService);
        _readUrlaubsplanerService = readUrlaubsplanerService;
        ArgumentNullException.ThrowIfNull(readUebersichtService);
        _readUebersichtService = readUebersichtService;
        ArgumentNullException.ThrowIfNull(entscheidenService);
        _entscheidenService = entscheidenService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    [HttpGet]
    public async Task<IActionResult> Index(
        string? userId,
        int? jahr,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeUrlaubsplanerZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

        var viewModel = await BaueViewModelAsync(
            mitarbeiterResult,
            userId,
            jahr ?? _dateTimeProvider.Today.Year,
            cancellationToken);

        return BereichsView("Index", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnspruchSpeichern(
        string userId,
        int jahr,
        int anspruchstage,
        int uebertragstage,
        string? bemerkung,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeUrlaubsplanerZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);
        var mitarbeiter = ErmittleUrlaubsrelevantenMitarbeiter(mitarbeiterResult, userId);

        if (mitarbeiter is null)
        {
            TempData[FlashKeys.Error] = "Der ausgewählte Mitarbeiter ist für den Urlaubsplaner ungültig.";
            return RedirectToAction(nameof(Index), new { userId, jahr });
        }

        var result = await _saveAnspruchService.ExecuteAsync(
            new SaveMitarbeiterUrlaubsanspruchCommand
            {
                UserId = userId,
                Jahr = jahr,
                Anspruchstage = anspruchstage,
                Uebertragstage = uebertragstage,
                Bemerkung = bemerkung
            },
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Der Urlaubsanspruch konnte nicht gespeichert werden.";
            return RedirectToAction(nameof(Index), new { userId, jahr });
        }

        TempData[FlashKeys.Success] = "Der Jahresurlaubsanspruch wurde gespeichert.";
        return RedirectToAction(nameof(Index), new { userId, jahr });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZeitraumSpeichern(
     string userId,
     int jahr,
     DateOnly? von,
     DateOnly? bis,
     string? notiz,
     CancellationToken cancellationToken)
    {
        var zugriff = await PruefeUrlaubsplanerZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);
        var mitarbeiter = ErmittleFestangestelltenMitarbeiter(mitarbeiterResult, userId);

        if (mitarbeiter is null)
        {
            TempData[FlashKeys.Error] = "Urlaubszeiträume können nur für festangestellte Mitarbeiter hinterlegt werden.";
            return RedirectToAction(nameof(Index), new { userId, jahr });
        }

        if (!von.HasValue || !bis.HasValue)
        {
            TempData[FlashKeys.Error] = "Bitte wählen Sie einen gültigen Zeitraum mit Start- und Enddatum aus.";
            return RedirectToAction(nameof(Index), new { userId, jahr });
        }

        var result = await _saveZeitraumService.ExecuteAsync(
            new SaveMitarbeiterUrlaubszeitraumCommand
            {
                UserId = userId,
                Von = von.Value,
                Bis = bis.Value,
                Notiz = notiz
            },
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Der Urlaubszeitraum konnte nicht gespeichert werden.";
            return RedirectToAction(nameof(Index), new { userId, jahr });
        }

        var entfernteWuensche = await BereinigeOffeneDienstwuenscheImUrlaubszeitraumAsync(
         userId,
         von.Value,
         bis.Value,
         cancellationToken);

        TempData[FlashKeys.Success] = entfernteWuensche > 0
            ? $"Der Urlaubszeitraum wurde hinterlegt. {entfernteWuensche} gespeicherte Dienstwünsche im Urlaubszeitraum wurden automatisch entfernt."
            : "Der Urlaubszeitraum wurde hinterlegt.";

        return RedirectToAction(nameof(Index), new { userId, jahr });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZeitraumLoeschen(
        Guid id,
        string userId,
        int jahr,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeUrlaubsplanerZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);
        var mitarbeiter = ErmittleFestangestelltenMitarbeiter(mitarbeiterResult, userId);

        if (mitarbeiter is null)
        {
            TempData[FlashKeys.Error] = "Urlaubszeiträume können nur für festangestellte Mitarbeiter gelöscht werden.";
            return RedirectToAction(nameof(Index), new { userId, jahr });
        }

        var result = await _deleteZeitraumService.ExecuteAsync(
            new DeleteMitarbeiterUrlaubszeitraumCommand
            {
                Id = id
            },
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Der Urlaubszeitraum konnte nicht gelöscht werden.";
            return RedirectToAction(nameof(Index), new { userId, jahr });
        }

        TempData[FlashKeys.Success] = "Der Urlaubszeitraum wurde gelöscht.";
        return RedirectToAction(nameof(Index), new { userId, jahr });
    }

    private async Task<UrlaubsplanerIndexViewModel> BaueViewModelAsync(
    ReadItwMitarbeiterprofileResult mitarbeiterResult,
    string? userId,
    int jahr,
    CancellationToken cancellationToken)
    {
        var urlaubsrelevanteMitarbeiter = mitarbeiterResult.IsSuccess
            ? mitarbeiterResult.Profile
                .Where(x =>
                    !x.IstGesperrt &&
                    x.HatProfil &&
                    (x.Beschaeftigungsart == ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt
                     || x.Beschaeftigungsart == ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer))
                .OrderBy(x => x.Beschaeftigungsart)
                .ThenBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
                .ToArray()
            : Array.Empty<ItwMitarbeiterprofilUebersichtDto>();

        var ausgewaehlterMitarbeiter = urlaubsrelevanteMitarbeiter.FirstOrDefault(x =>
            string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase))
            ?? urlaubsrelevanteMitarbeiter.FirstOrDefault();

        var ausgewaehlterUserId = ausgewaehlterMitarbeiter?.UserId;
        var ausgewaehltesJahr = jahr is >= 2000 and <= 2100 ? jahr : _dateTimeProvider.Today.Year;

        var viewModel = new UrlaubsplanerIndexViewModel
        {
            Titel = "Urlaubsplaner",
            Beschreibung = "Jahresanspruch, Übertrag und Urlaubszeiträume für Mitarbeiter im Intensivtransport.",
            IsSuccess = mitarbeiterResult.IsSuccess,
            ErrorMessage = mitarbeiterResult.ErrorMessage,
            AusgewaehlterUserId = ausgewaehlterUserId,
            AusgewaehltesJahr = ausgewaehltesJahr,
            MitarbeiterOptionen = urlaubsrelevanteMitarbeiter
                .Select(x => new SelectListItem
                {
                    Value = x.UserId,
                    Text = $"{x.AnzeigeName} ({FormatiereBeschaeftigungsart(x.Beschaeftigungsart)})",
                    Selected = string.Equals(x.UserId, ausgewaehlterUserId, StringComparison.OrdinalIgnoreCase)
                })
                .ToArray(),
            JahrOptionen = Enumerable.Range(_dateTimeProvider.Today.Year - 1, 5)
                .Select(x => new SelectListItem
                {
                    Value = x.ToString(),
                    Text = x.ToString(),
                    Selected = x == ausgewaehltesJahr
                })
                .ToArray()
        };

        if (ausgewaehlterMitarbeiter is null)
        {
            return viewModel;
        }

        var readResult = await _readUrlaubsplanerService.ExecuteAsync(
            new ReadMitarbeiterUrlaubsplanerQuery
            {
                UserId = ausgewaehlterMitarbeiter.UserId,
                Jahr = ausgewaehltesJahr,
                Beschaeftigungsart = ausgewaehlterMitarbeiter.Beschaeftigungsart
            },
            cancellationToken);

        viewModel.IsSuccess = viewModel.IsSuccess && readResult.IsSuccess;
        viewModel.ErrorMessage ??= readResult.ErrorMessage;
        viewModel.MitarbeiterAnzeigeName = ausgewaehlterMitarbeiter.AnzeigeName;
        viewModel.BeschaeftigungsartText = FormatiereBeschaeftigungsart(ausgewaehlterMitarbeiter.Beschaeftigungsart);
        viewModel.IstFestangestellt = ausgewaehlterMitarbeiter.Beschaeftigungsart == ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt;
        viewModel.IstFreelancer = ausgewaehlterMitarbeiter.Beschaeftigungsart == ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer;
        viewModel.Anspruchstage = readResult.Anspruchstage;
        viewModel.Uebertragstage = readResult.Uebertragstage;
        viewModel.GenommeneUrlaubstage = readResult.GenommeneUrlaubstage;
        viewModel.Resturlaubstage = readResult.Resturlaubstage;
        viewModel.AnspruchIstStandardwert = readResult.AnspruchIstStandardwert;
        viewModel.Zeitraeume = readResult.Zeitraeume
            .Select(x => new UrlaubszeitraumEintragViewModel
            {
                Id = x.Id,
                VonAnzeige = x.Von.ToDateTime(TimeOnly.MinValue).ToString("dd.MM.yyyy", DeutscheKultur),
                BisAnzeige = x.Bis.ToDateTime(TimeOnly.MinValue).ToString("dd.MM.yyyy", DeutscheKultur),
                Urlaubstage = x.Urlaubstage,
                Notiz = x.Notiz
            })
            .ToArray();

        return viewModel;
    }

    private async Task<int> BereinigeOffeneDienstwuenscheImUrlaubszeitraumAsync(
    string userId,
    DateOnly von,
    DateOnly bis,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId) || bis < von)
        {
            return 0;
        }

        var offenePerioden = await _dienstplanPeriodeRepository.GetOffeneAsync(cancellationToken);

        if (offenePerioden.Count == 0)
        {
            return 0;
        }

        var entfernteWuensche = 0;

        foreach (var periode in offenePerioden)
        {
            var periodenStart = new DateOnly(periode.Jahr, periode.Monat, 1);
            var periodenEnde = periodenStart.AddMonths(1).AddDays(-1);

            if (bis < periodenStart || von > periodenEnde)
            {
                continue;
            }

            var vorhandeneWuensche = await _dienstwunschRepository.GetAlleFuerBenutzerAsync(
                periode.Id,
                userId,
                cancellationToken);

            var zuEntfernendeWuensche = vorhandeneWuensche
                .Where(x => x.WunschDatum >= von && x.WunschDatum <= bis)
                .ToArray();

            if (zuEntfernendeWuensche.Length == 0)
            {
                continue;
            }

            foreach (var dienstwunsch in zuEntfernendeWuensche)
            {
                _dienstwunschRepository.Remove(dienstwunsch);
            }

            entfernteWuensche += zuEntfernendeWuensche.Length;
        }

        if (entfernteWuensche > 0)
        {
            await _dienstwunschRepository.SaveChangesAsync(cancellationToken);
        }

        return entfernteWuensche;
    }

    private static ItwMitarbeiterprofilUebersichtDto? ErmittleUrlaubsrelevantenMitarbeiter(
    ReadItwMitarbeiterprofileResult mitarbeiterResult,
    string? userId)
    {
        if (!mitarbeiterResult.IsSuccess || string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return mitarbeiterResult.Profile.FirstOrDefault(x =>
            string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase) &&
            !x.IstGesperrt &&
            x.HatProfil &&
            (x.Beschaeftigungsart == ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt
             || x.Beschaeftigungsart == ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer));
    }

    private static ItwMitarbeiterprofilUebersichtDto? ErmittleFestangestelltenMitarbeiter(
        ReadItwMitarbeiterprofileResult mitarbeiterResult,
        string? userId)
    {
        if (!mitarbeiterResult.IsSuccess || string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return mitarbeiterResult.Profile.FirstOrDefault(x =>
            string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase) &&
            !x.IstGesperrt &&
            x.HatProfil &&
            x.Beschaeftigungsart == ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt);
    }

    [HttpGet]
    public async Task<IActionResult> Antraege(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeUrlaubsplanerZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _readUebersichtService.ExecuteAsync(cancellationToken);

        var viewModel = new WachleiterUrlaubsUebersichtViewModel
        {
            Antraege = result.Antraege
                .Select(a => new WachleiterUrlaubsAntragViewModel
                {
                    ZeitraumId        = a.ZeitraumId,
                    MitarbeiterName   = a.MitarbeiterName,
                    VonAnzeige        = a.Von.ToString("dd.MM.yyyy"),
                    BisAnzeige        = a.Bis.ToString("dd.MM.yyyy"),
                    Urlaubstage       = BerechneArbeitstage(a.Von, a.Bis),
                    Notiz             = a.Notiz,
                    HatUeberschneidung = a.HatUeberschneidung
                })
                .ToList()
        };

        return BereichsView("Antraege", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entscheiden(
        Guid zeitraumId,
        bool genehmigt,
        string? begruendung,
        string? loesung,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeUrlaubsplanerZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var mitarbeiterResult = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);
        var zeitraum = await _urlaubszeitraumRepository.GetByIdAsync(zeitraumId, cancellationToken);

        var mitarbeiterName = zeitraum is not null
            ? mitarbeiterResult.Profile
                .FirstOrDefault(p => string.Equals(p.UserId, zeitraum.UserId, StringComparison.OrdinalIgnoreCase))
                ?.AnzeigeName ?? zeitraum.UserId
            : string.Empty;

        var wachleiterName = mitarbeiterResult.Profile
            .FirstOrDefault(p => string.Equals(p.UserId, zugriff.CurrentUser!.UserId, StringComparison.OrdinalIgnoreCase))
            ?.AnzeigeName ?? zugriff.CurrentUser!.UserId;

        var result = await _entscheidenService.ExecuteAsync(
            new EntscheidenUrlaubsAntragCommand(
                zeitraumId,
                genehmigt,
                begruendung,
                loesung,
                zugriff.CurrentUser!.UserId,
                wachleiterName,
                mitarbeiterName),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Der Urlaubsantrag konnte nicht bearbeitet werden.";
        }
        else
        {
            if (genehmigt && result.MitarbeiterUserId is not null)
            {
                // Dienstwünsche im genehmigten Zeitraum entfernen (Web-Layer hat Dienstplan-Zugriff)
                await BereinigeOffeneDienstwuenscheImUrlaubszeitraumAsync(
                    result.MitarbeiterUserId,
                    result.GenehmigterVon,
                    result.GenehmigterBis,
                    cancellationToken);
            }

            TempData[FlashKeys.Success] = genehmigt
                ? $"Urlaub von {mitarbeiterName} wurde genehmigt."
                : $"Urlaub von {mitarbeiterName} wurde abgelehnt.";
        }

        return RedirectToAction(nameof(Antraege));
    }

    private static int BerechneArbeitstage(DateOnly von, DateOnly bis)
    {
        var tage = 0;
        for (var datum = von; datum <= bis; datum = datum.AddDays(1))
        {
            if (datum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;
            tage++;
        }

        return tage;
    }

    private async Task<UrlaubsplanerZugriffResult> PruefeUrlaubsplanerZugriffAsync(
        CancellationToken cancellationToken)
    {
        var result = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        if (!result.IsSuccess || result.CurrentUser is null)
        {
            return UrlaubsplanerZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        var aktuellerBenutzer = result.CurrentUser;

        if (aktuellerBenutzer.Bereich != Bereich)
        {
            return UrlaubsplanerZugriffResult.MitErgebnis(
                ErzeugeBereichsWeiterleitung(
                    aktuellerBenutzer.Bereich,
                    "Dashboard",
                    "Index",
                    "Urlaubsplaner",
                    BereichsRoutingHelper.GetBereichsname(Bereich),
                    "Der Urlaubsplaner steht aktuell nur im Bereich Intensivtransport zur Verfügung. Ihr zuständiger Bereich wird jetzt geöffnet."));
        }

        if (aktuellerBenutzer.Rolle != BereichsrolleCode.Wachleiter)
        {
            return UrlaubsplanerZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        return UrlaubsplanerZugriffResult.MitBenutzer(aktuellerBenutzer);
    }

    private IActionResult BereichsView(string viewName, object model)
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
        return View($"~/Areas/Intensivtransport/Views/Urlaubsplaner/{viewName}.cshtml", model);
    }

    private static string FormatiereBeschaeftigungsart(
    ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart beschaeftigungsart)
    {
        return beschaeftigungsart switch
        {
            ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer => "Freelancer",
            ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt => "Festangestellt",
            ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Honorarkraft => "Honorarkraft",
            _ => "Unbekannt"
        };
    }

    private sealed record UrlaubsplanerZugriffResult(CurrentUserContext? CurrentUser, IActionResult? EarlyResult)
    {
        public static UrlaubsplanerZugriffResult MitBenutzer(CurrentUserContext currentUser)
            => new(currentUser, null);

        public static UrlaubsplanerZugriffResult MitErgebnis(IActionResult earlyResult)
            => new(null, earlyResult);
    }
}