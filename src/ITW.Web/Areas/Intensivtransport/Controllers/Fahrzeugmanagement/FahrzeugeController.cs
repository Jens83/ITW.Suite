using ITW.Application.Organisation.Contracts;
using ITW.Fahrzeugmanagement.Application.Fahrtenbuch;
using ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;
using ITW.Fahrzeugmanagement.Application.Fahrzeuge;
using ITW.Fahrzeugmanagement.Application.FahrzeugPruefungen;
using ITW.Fahrzeugmanagement.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Fahrzeugmanagement;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Fahrzeugmanagement)]
public sealed class FahrzeugeController : IntensivtransportFahrzeugmanagementControllerBase
{
    private readonly ReadFahrzeugUebersichtService _readFahrzeugUebersichtService;
    private readonly CreateFahrzeugService _createFahrzeugService;
    private readonly ReadFahrzeugDetailService _readFahrzeugDetailService;
    private readonly UpdateFahrzeugStammdatenService _updateFahrzeugStammdatenService;

    private readonly ReadFahrzeugDokumenteService _readFahrzeugDokumenteService;
    private readonly UploadFahrzeugDokumentService _uploadFahrzeugDokumentService;
    private readonly DownloadFahrzeugDokumentService _downloadFahrzeugDokumentService;
    private readonly DeleteFahrzeugDokumentService _deleteFahrzeugDokumentService;

    private readonly ReadFahrtenbuchService _readFahrtenbuchService;
    private readonly CreateFahrtenbuchEintragService _createFahrtenbuchEintragService;
    private readonly ReadFahrtenbuchEintragDetailService _readFahrtenbuchEintragDetailService;

    private readonly ReadFahrzeugPruefstatusService _readFahrzeugPruefstatusService;
    private readonly SaveFahrzeugPruefungService _saveFahrzeugPruefungService;

    public FahrzeugeController(
        ReadFahrzeugUebersichtService readFahrzeugUebersichtService,
        CreateFahrzeugService createFahrzeugService,
        ReadFahrzeugDetailService readFahrzeugDetailService,
        UpdateFahrzeugStammdatenService updateFahrzeugStammdatenService,
        ICurrentUserContextAccessor currentUserContextAccessor,
        ReadFahrzeugDokumenteService readFahrzeugDokumenteService,
        UploadFahrzeugDokumentService uploadFahrzeugDokumentService,
        DownloadFahrzeugDokumentService downloadFahrzeugDokumentService,
        DeleteFahrzeugDokumentService deleteFahrzeugDokumentService,
        ReadFahrtenbuchService readFahrtenbuchService,
        CreateFahrtenbuchEintragService createFahrtenbuchEintragService,
        ReadFahrtenbuchEintragDetailService readFahrtenbuchEintragDetailService,
        ReadFahrzeugPruefstatusService readFahrzeugPruefstatusService,
        SaveFahrzeugPruefungService saveFahrzeugPruefungService)
        : base(currentUserContextAccessor)
    {
        _readFahrzeugUebersichtService = readFahrzeugUebersichtService
            ?? throw new ArgumentNullException(nameof(readFahrzeugUebersichtService));

        _createFahrzeugService = createFahrzeugService
            ?? throw new ArgumentNullException(nameof(createFahrzeugService));

        _readFahrzeugDetailService = readFahrzeugDetailService
            ?? throw new ArgumentNullException(nameof(readFahrzeugDetailService));

        _updateFahrzeugStammdatenService = updateFahrzeugStammdatenService
            ?? throw new ArgumentNullException(nameof(updateFahrzeugStammdatenService));

        _readFahrzeugDokumenteService = readFahrzeugDokumenteService
            ?? throw new ArgumentNullException(nameof(readFahrzeugDokumenteService));

        _uploadFahrzeugDokumentService = uploadFahrzeugDokumentService
            ?? throw new ArgumentNullException(nameof(uploadFahrzeugDokumentService));

        _downloadFahrzeugDokumentService = downloadFahrzeugDokumentService
            ?? throw new ArgumentNullException(nameof(downloadFahrzeugDokumentService));

        _deleteFahrzeugDokumentService = deleteFahrzeugDokumentService
            ?? throw new ArgumentNullException(nameof(deleteFahrzeugDokumentService));

        _readFahrtenbuchService = readFahrtenbuchService
            ?? throw new ArgumentNullException(nameof(readFahrtenbuchService));

        _createFahrtenbuchEintragService = createFahrtenbuchEintragService
            ?? throw new ArgumentNullException(nameof(createFahrtenbuchEintragService));

        _readFahrtenbuchEintragDetailService = readFahrtenbuchEintragDetailService
            ?? throw new ArgumentNullException(nameof(readFahrtenbuchEintragDetailService));

        _readFahrzeugPruefstatusService = readFahrzeugPruefstatusService
            ?? throw new ArgumentNullException(nameof(readFahrzeugPruefstatusService));

        _saveFahrzeugPruefungService = saveFahrzeugPruefungService
            ?? throw new ArgumentNullException(nameof(saveFahrzeugPruefungService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var uebersicht = await _readFahrzeugUebersichtService.ExecuteAsync(cancellationToken);

        var viewModel = new FahrzeugeIndexViewModel
        {
            Fahrzeuge = uebersicht.Fahrzeuge
                .Select(x => new FahrzeugIndexItemViewModel
                {
                    FahrzeugId = x.FahrzeugId,
                    InterneNummer = x.InterneNummer,
                    Kennzeichen = x.Kennzeichen,
                    Hersteller = x.Hersteller,
                    Modell = x.Modell,
                    Fahrzeugtyp = x.Fahrzeugtyp,
                    Baujahr = x.Baujahr,
                    Erstzulassung = x.Erstzulassung,
                    Kraftstoffart = x.Kraftstoffart,
                    LeistungKw = x.LeistungKw,
                    KilometerstandAktuell = x.KilometerstandAktuell,
                    Status = x.Status,
                    StandardStandort = string.IsNullOrWhiteSpace(x.StandardStandort)
                        ? "-"
                        : x.StandardStandort
                })
                .ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(
        Guid id,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var detail = await _readFahrzeugDetailService.ExecuteAsync(
            id,
            cancellationToken);

        if (detail is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var dokumenteResult = await _readFahrzeugDokumenteService.ExecuteAsync(
            id,
            cancellationToken);

        var pruefstatusResult = await _readFahrzeugPruefstatusService.ExecuteAsync(
            id,
            cancellationToken);

        var viewModel = new FahrzeugDetailViewModel
        {
            FahrzeugId = detail.FahrzeugId,
            Titel = detail.Kennzeichen,
            Beschreibung = $"{detail.Hersteller} {detail.Modell}".Trim(),
            InterneNummer = detail.InterneNummer,
            Kennzeichen = detail.Kennzeichen,
            Vin = detail.Vin,
            Hersteller = detail.Hersteller,
            Modell = detail.Modell,
            Fahrzeugtyp = detail.Fahrzeugtyp,
            Baujahr = detail.Baujahr,
            Erstzulassung = detail.Erstzulassung,
            Kraftstoffart = detail.Kraftstoffart,
            LeistungKw = detail.LeistungKw,
            KilometerstandAktuell = detail.KilometerstandAktuell,
            Status = detail.Status,
            StandardStandort = string.IsNullOrWhiteSpace(detail.StandardStandort)
                ? "-"
                : detail.StandardStandort,
            Navigation = BaueNavigation(detail, "Details"),
            Dokumente = dokumenteResult.Dokumente
                .Where(x => x.Kategorie == FahrzeugDokumentKategorie.Tankbeleg)
                .OrderByDescending(x => x.HochgeladenAm)
                .Select(x => new FahrzeugDokumentItemViewModel
                {
                    DokumentId = x.DokumentId,
                    Kategorie = x.Kategorie,
                    Dateiname = x.Dateiname,
                    Bezeichnung = x.Bezeichnung,
                    ContentType = x.ContentType,
                    GueltigBis = x.GueltigBis,
                    HochgeladenAm = x.HochgeladenAm
                })
                .ToList(),
            Pruefungen = BaueFahrzeugPruefstatusItems(pruefstatusResult)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Anlegen(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var viewModel = new FahrzeugFormViewModel
        {
            Titel = "Fahrzeug anlegen",
            Beschreibung = "Neues Fahrzeug für den Intensivtransport erfassen.",
            Fahrzeugtyp = "Intensivtransportwagen",
            Kraftstoffart = Kraftstoffart.Diesel,
            Status = FahrzeugStatus.Aktiv
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anlegen(
        FahrzeugFormViewModel input,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        if (zugriff.CurrentUser is null)
        {
            return RedirectToKeinZugriff();
        }

        var result = await _createFahrzeugService.ExecuteAsync(
            new CreateFahrzeugCommand(
                input.InterneNummer,
                input.Kennzeichen,
                input.Vin,
                input.Hersteller,
                input.Modell,
                input.Fahrzeugtyp,
                input.Baujahr,
                input.Erstzulassung,
                input.Kraftstoffart,
                input.LeistungKw,
                input.KilometerstandAktuell,
                input.Status,
                input.StandardStandort,
                zugriff.CurrentUser.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            input.Titel = "Fahrzeug anlegen";
            input.Beschreibung = "Neues Fahrzeug für den Intensivtransport erfassen.";
            input.Fehlermeldung = result.ErrorMessage ?? "Das Fahrzeug konnte nicht angelegt werden.";

            return View(input);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Bearbeiten(
        Guid id,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var detail = await _readFahrzeugDetailService.ExecuteAsync(
            id,
            cancellationToken);

        if (detail is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new FahrzeugFormViewModel
        {
            FahrzeugId = detail.FahrzeugId,
            Titel = "Stammdaten",
            Beschreibung = "Stammdaten und Status des Fahrzeugs bearbeiten.",
            InterneNummer = detail.InterneNummer,
            Kennzeichen = detail.Kennzeichen,
            Vin = detail.Vin,
            Hersteller = detail.Hersteller,
            Modell = detail.Modell,
            Fahrzeugtyp = detail.Fahrzeugtyp,
            Baujahr = detail.Baujahr,
            Erstzulassung = detail.Erstzulassung,
            Kraftstoffart = detail.Kraftstoffart,
            LeistungKw = detail.LeistungKw,
            KilometerstandAktuell = detail.KilometerstandAktuell,
            Status = detail.Status,
            StandardStandort = detail.StandardStandort,
            Navigation = BaueNavigation(detail, "Stammdaten")
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bearbeiten(
        FahrzeugFormViewModel input,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        if (zugriff.CurrentUser is null)
        {
            return RedirectToKeinZugriff();
        }

        input.Titel = "Stammdaten";
        input.Beschreibung = "Stammdaten und Status des Fahrzeugs bearbeiten.";
        input.Navigation = BaueNavigation(input, "Stammdaten");

        if (input.FahrzeugId == Guid.Empty)
        {
            input.Fehlermeldung = "Das Fahrzeug konnte nicht ermittelt werden.";
            return View(input);
        }

        var result = await _updateFahrzeugStammdatenService.ExecuteAsync(
            new UpdateFahrzeugStammdatenCommand(
                input.FahrzeugId,
                input.InterneNummer,
                input.Kennzeichen,
                input.Vin,
                input.Hersteller,
                input.Modell,
                input.Fahrzeugtyp,
                input.Baujahr,
                input.Erstzulassung,
                input.Kraftstoffart,
                input.LeistungKw,
                input.KilometerstandAktuell,
                input.Status,
                input.StandardStandort,
                zugriff.CurrentUser.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            input.Fehlermeldung = result.ErrorMessage ?? "Das Fahrzeug konnte nicht gespeichert werden.";
            return View(input);
        }

        return RedirectToAction(nameof(Details), new { id = input.FahrzeugId });
    }

    [HttpGet]
    public async Task<IActionResult> Tankbelege(
        Guid id,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var detail = await _readFahrzeugDetailService.ExecuteAsync(
            id,
            cancellationToken);

        if (detail is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var dokumenteResult = await _readFahrzeugDokumenteService.ExecuteAsync(
            id,
            cancellationToken);

        var viewModel = BaueFahrzeugDokumenteViewModel(
            detail,
            dokumenteResult);

        return View("Dokumente", viewModel);
    }

    [HttpGet]
    public IActionResult Dokumente(Guid id)
    {
        return RedirectToAction(nameof(Tankbelege), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DokumentHochladen(
        FahrzeugDokumenteViewModel input,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (zugriff.CurrentUser is null)
        {
            return RedirectToKeinZugriff();
        }

        if (input.FahrzeugId == Guid.Empty)
        {
            TempData["FahrzeugDokumente.Fehler"] = "Das Fahrzeug konnte nicht ermittelt werden.";
            return RedirectToAction(nameof(Index));
        }

        if (input.Datei is null || input.Datei.Length == 0)
        {
            TempData["FahrzeugDokumente.Fehler"] = "Bitte eine Datei auswählen.";
            return RedirectToAction(nameof(Tankbelege), new { id = input.FahrzeugId });
        }

        byte[] dateiinhalt;

        await using (var memoryStream = new MemoryStream())
        {
            await input.Datei.CopyToAsync(memoryStream, cancellationToken);
            dateiinhalt = memoryStream.ToArray();
        }

        var result = await _uploadFahrzeugDokumentService.ExecuteAsync(
            new UploadFahrzeugDokumentCommand(
                input.FahrzeugId,
                FahrzeugDokumentKategorie.Tankbeleg,
                input.Bezeichnung,
                input.Datei.FileName,
                dateiinhalt,
                null,
                zugriff.CurrentUser.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["FahrzeugDokumente.Fehler"] =
                result.ErrorMessage ?? "Der Tankbeleg konnte nicht hochgeladen werden.";

            return RedirectToAction(nameof(Tankbelege), new { id = input.FahrzeugId });
        }

        TempData["FahrzeugDokumente.Erfolg"] = "Der Tankbeleg wurde erfolgreich hochgeladen.";

        return RedirectToAction(nameof(Tankbelege), new { id = input.FahrzeugId });
    }

    [HttpGet]
    public async Task<IActionResult> DokumentHerunterladen(
        Guid id,
        Guid dokumentId,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _downloadFahrzeugDokumentService.ExecuteAsync(
            new DownloadFahrzeugDokumentQuery(
                id,
                dokumentId),
            cancellationToken);

        if (!result.IsSuccess ||
            result.Dateiinhalt is null ||
            string.IsNullOrWhiteSpace(result.Dateiname) ||
            string.IsNullOrWhiteSpace(result.ContentType))
        {
            TempData["FahrzeugDokumente.Fehler"] =
                result.ErrorMessage ?? "Der Tankbeleg konnte nicht heruntergeladen werden.";

            return RedirectToAction(nameof(Tankbelege), new { id });
        }

        return File(
            result.Dateiinhalt,
            result.ContentType,
            result.Dateiname);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DokumentLoeschen(
        Guid id,
        Guid dokumentId,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _deleteFahrzeugDokumentService.ExecuteAsync(
            new DeleteFahrzeugDokumentCommand(
                id,
                dokumentId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["FahrzeugDokumente.Fehler"] =
                result.ErrorMessage ?? "Der Tankbeleg konnte nicht gelöscht werden.";

            return RedirectToAction(nameof(Tankbelege), new { id });
        }

        TempData["FahrzeugDokumente.Erfolg"] = "Der Tankbeleg wurde gelöscht.";

        return RedirectToAction(nameof(Tankbelege), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Pruefstatus(
        Guid id,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var detail = await _readFahrzeugDetailService.ExecuteAsync(
            id,
            cancellationToken);

        if (detail is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var result = await _readFahrzeugPruefstatusService.ExecuteAsync(
            id,
            cancellationToken);

        var viewModel = BaueFahrzeugPruefstatusViewModel(
            detail,
            result);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PruefungSpeichern(
        FahrzeugPruefstatusViewModel input,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (zugriff.CurrentUser is null)
        {
            return RedirectToKeinZugriff();
        }

        if (input.FahrzeugId == Guid.Empty)
        {
            TempData["FahrzeugPruefstatus.Fehler"] = "Das Fahrzeug konnte nicht ermittelt werden.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _saveFahrzeugPruefungService.ExecuteAsync(
            new SaveFahrzeugPruefungCommand(
                input.FahrzeugId,
                input.Typ,
                input.FaelligAm,
                input.LetzteErledigungAm,
                input.Bemerkung,
                zugriff.CurrentUser.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["FahrzeugPruefstatus.Fehler"] =
                result.ErrorMessage ?? "Der Prüfstatus konnte nicht gespeichert werden.";

            return RedirectToAction(nameof(Pruefstatus), new { id = input.FahrzeugId });
        }

        TempData["FahrzeugPruefstatus.Erfolg"] = "Der Prüfstatus wurde gespeichert.";

        return RedirectToAction(nameof(Pruefstatus), new { id = input.FahrzeugId });
    }

    [HttpGet]
    public async Task<IActionResult> Fahrtenbuch(
        Guid id,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var detail = await _readFahrzeugDetailService.ExecuteAsync(
            id,
            cancellationToken);

        if (detail is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var fahrtenbuchResult = await _readFahrtenbuchService.ExecuteAsync(
            id,
            cancellationToken);

        var viewModel = BaueFahrtenbuchViewModel(
            detail,
            fahrtenbuchResult);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FahrtenbuchEintragAnlegen(
        FahrtenbuchViewModel input,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (zugriff.CurrentUser is null)
        {
            return RedirectToKeinZugriff();
        }

        if (input.FahrzeugId == Guid.Empty)
        {
            TempData["Fahrtenbuch.Fehler"] = "Das Fahrzeug konnte nicht ermittelt werden.";
            return RedirectToAction(nameof(Index));
        }

        if (!input.Startzeit.HasValue)
        {
            TempData["Fahrtenbuch.Fehler"] = "Bitte eine Startzeit eingeben.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        if (!input.Endzeit.HasValue)
        {
            TempData["Fahrtenbuch.Fehler"] = "Bitte eine Endzeit eingeben.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        if (input.Endzeit.Value < input.Startzeit.Value)
        {
            TempData["Fahrtenbuch.Fehler"] = "Die Endzeit darf nicht vor der Startzeit liegen.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        if (input.EndKilometerstand is null)
        {
            TempData["Fahrtenbuch.Fehler"] = "Bitte einen Endkilometerstand eingeben.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        if (input.EndKilometerstand.Value < input.StartKilometerstand)
        {
            TempData["Fahrtenbuch.Fehler"] = "Der Endkilometerstand darf nicht kleiner als der Startkilometerstand sein.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        var startzeitUtc = KonvertiereLokaleZeitNachUtc(input.Startzeit.Value);
        var endzeitUtc = KonvertiereLokaleZeitNachUtc(input.Endzeit.Value);

        var result = await _createFahrtenbuchEintragService.ExecuteAsync(
            new CreateFahrtenbuchEintragCommand(
                input.FahrzeugId,
                zugriff.CurrentUser.UserId,
                input.FahrerName,
                input.BeifahrerName,
                input.FahrtKategorie,
                input.Fahrtzweck,
                startzeitUtc,
                endzeitUtc,
                input.Startort,
                input.Zielort,
                input.StartKilometerstand,
                input.EndKilometerstand.Value,
                input.Bemerkung,
                zugriff.CurrentUser.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Fahrtenbuch.Fehler"] =
                result.ErrorMessage ?? "Der Fahrtenbucheintrag konnte nicht gespeichert werden.";

            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        TempData["Fahrtenbuch.Erfolg"] = "Der Fahrtenbucheintrag wurde gespeichert.";

        return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
    }

    [HttpGet]
    public async Task<IActionResult> FahrtenbuchDetails(
        Guid id,
        Guid eintragId,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var fahrzeug = await _readFahrzeugDetailService.ExecuteAsync(
            id,
            cancellationToken);

        if (fahrzeug is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var eintrag = await _readFahrtenbuchEintragDetailService.ExecuteAsync(
            id,
            eintragId,
            cancellationToken);

        if (eintrag is null)
        {
            TempData["Fahrtenbuch.Fehler"] = "Der Fahrtenbucheintrag wurde nicht gefunden.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id });
        }

        var fahrzeugText = $"{fahrzeug.InterneNummer} · {fahrzeug.Kennzeichen}".Trim();

        var viewModel = new FahrtenbuchDetailsViewModel
        {
            FahrzeugId = fahrzeug.FahrzeugId,
            EintragId = eintrag.EintragId,
            FahrzeugText = fahrzeugText,
            FahrerName = eintrag.FahrerName,
            BeifahrerName = string.IsNullOrWhiteSpace(eintrag.BeifahrerName)
                ? "-"
                : eintrag.BeifahrerName,
            FahrtKategorie = eintrag.FahrtKategorie,
            Fahrtzweck = eintrag.Fahrtzweck,
            StartzeitUtc = eintrag.StartzeitUtc,
            EndzeitUtc = eintrag.EndzeitUtc,
            Startort = eintrag.Startort,
            Zielort = eintrag.Zielort,
            StartKilometerstand = eintrag.StartKilometerstand,
            EndKilometerstand = eintrag.EndKilometerstand,
            GefahreneKilometer = eintrag.GefahreneKilometer,
            TankmengeLiter = eintrag.TankmengeLiter,
            KilometerstandBeimTanken = eintrag.KilometerstandBeimTanken,
            Status = eintrag.Status,
            IstAutomatischVorbelegt = eintrag.IstAutomatischVorbelegt,
            Bemerkung = eintrag.Bemerkung,
            Navigation = BaueNavigation(fahrzeug, "Fahrtenbuch")
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Vertraege(Guid id)
    {
        TempData["Fahrzeuge.Hinweis"] =
            "Verträge gehören künftig in die Verwaltungs-Fahrzeugverwaltung und sind im Wachleiterbereich nicht mehr vorgesehen.";

        return id == Guid.Empty
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VertragAnlegen(
        Guid fahrzeugId,
        Guid id)
    {
        TempData["Fahrzeuge.Hinweis"] =
            "Verträge gehören künftig in die Verwaltungs-Fahrzeugverwaltung und sind im Wachleiterbereich nicht mehr vorgesehen.";

        var zielFahrzeugId = fahrzeugId != Guid.Empty
            ? fahrzeugId
            : id;

        return zielFahrzeugId == Guid.Empty
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Details), new { id = zielFahrzeugId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult VertragLoeschen(
        Guid id,
        Guid fahrzeugId)
    {
        TempData["Fahrzeuge.Hinweis"] =
            "Verträge gehören künftig in die Verwaltungs-Fahrzeugverwaltung und sind im Wachleiterbereich nicht mehr vorgesehen.";

        var zielFahrzeugId = fahrzeugId != Guid.Empty
            ? fahrzeugId
            : id;

        return zielFahrzeugId == Guid.Empty
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Details), new { id = zielFahrzeugId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FahrtenbuchEintragAbschliessen(
        Guid id,
        Guid eintragId)
    {
        TempData["Fahrtenbuch.Fehler"] =
            "Der separate Abschluss eines Fahrtenbucheintrags wird nicht mehr verwendet. Einträge werden direkt vollständig gespeichert.";

        if (id == Guid.Empty)
        {
            return RedirectToAction(nameof(Index));
        }

        if (eintragId == Guid.Empty)
        {
            return RedirectToAction(nameof(Fahrtenbuch), new { id });
        }

        return RedirectToAction(
            nameof(FahrtenbuchDetails),
            new
            {
                id,
                eintragId
            });
    }

    private static FahrzeugDetailNavigationViewModel BaueNavigation(
        FahrzeugDetail detail,
        string aktiveSeite)
    {
        return new FahrzeugDetailNavigationViewModel
        {
            FahrzeugId = detail.FahrzeugId,
            Kennzeichen = detail.Kennzeichen,
            InterneNummer = detail.InterneNummer,
            Fahrzeugname = $"{detail.Hersteller} {detail.Modell}".Trim(),
            StatusText = ErmittleStatusText(detail.Status),
            AktiveSeite = aktiveSeite
        };
    }

    private static FahrzeugDetailNavigationViewModel BaueNavigation(
        FahrzeugFormViewModel input,
        string aktiveSeite)
    {
        return new FahrzeugDetailNavigationViewModel
        {
            FahrzeugId = input.FahrzeugId,
            Kennzeichen = string.IsNullOrWhiteSpace(input.Kennzeichen)
                ? "Fahrzeug"
                : input.Kennzeichen,
            InterneNummer = input.InterneNummer,
            Fahrzeugname = $"{input.Hersteller} {input.Modell}".Trim(),
            StatusText = ErmittleStatusText(input.Status),
            AktiveSeite = aktiveSeite
        };
    }

    private static string ErmittleStatusText(FahrzeugStatus status)
    {
        return status switch
        {
            FahrzeugStatus.Aktiv => "Aktiv",
            FahrzeugStatus.InWartung => "In Wartung",
            FahrzeugStatus.AusserBetrieb => "Außer Betrieb",
            FahrzeugStatus.Reserviert => "Reserviert",
            FahrzeugStatus.Archiviert => "Archiviert",
            _ => "Unbekannt"
        };
    }

    private FahrzeugDokumenteViewModel BaueFahrzeugDokumenteViewModel(
        FahrzeugDetail detail,
        ReadFahrzeugDokumenteResult dokumenteResult)
    {
        return new FahrzeugDokumenteViewModel
        {
            FahrzeugId = detail.FahrzeugId,
            Titel = "Tankbelege",
            Beschreibung = "Tankbelege für dieses Fahrzeug hochladen und verwalten.",
            Kategorie = FahrzeugDokumentKategorie.Tankbeleg,
            Navigation = BaueNavigation(detail, "Tankbelege"),
            KategorieOptionen = BaueFahrzeugDokumentKategorieOptionen(),
            Dokumente = dokumenteResult.Dokumente
                .Where(x => x.Kategorie == FahrzeugDokumentKategorie.Tankbeleg)
                .OrderByDescending(x => x.HochgeladenAm)
                .Select(x => new FahrzeugDokumentItemViewModel
                {
                    DokumentId = x.DokumentId,
                    Kategorie = x.Kategorie,
                    Dateiname = x.Dateiname,
                    Bezeichnung = x.Bezeichnung,
                    ContentType = x.ContentType,
                    GueltigBis = x.GueltigBis,
                    HochgeladenAm = x.HochgeladenAm
                })
                .ToList()
        };
    }

    private static IReadOnlyList<SelectListItem> BaueFahrzeugDokumentKategorieOptionen()
    {
        return
        [
            new SelectListItem("Tankbeleg", FahrzeugDokumentKategorie.Tankbeleg.ToString())
        ];
    }

    private FahrtenbuchViewModel BaueFahrtenbuchViewModel(
        FahrzeugDetail detail,
        ReadFahrtenbuchResult fahrtenbuchResult)
    {
        var fahrzeugText = $"{detail.InterneNummer} · {detail.Kennzeichen}".Trim();

        return new FahrtenbuchViewModel
        {
            FahrzeugId = detail.FahrzeugId,
            FahrzeugText = fahrzeugText,
            Titel = "Fahrtenbuch",
            Beschreibung = "Fahrten und Kilometerstände zum Fahrzeug dokumentieren.",
            Startzeit = DateTime.Today.AddHours(7),
            Endzeit = DateTime.Now,
            Navigation = BaueNavigation(detail, "Fahrtenbuch"),
            FahrtKategorieOptionen = BaueFahrtKategorieOptionen(),
            Eintraege = fahrtenbuchResult.Eintraege
                .OrderByDescending(x => x.StartzeitUtc)
                .Select(x => new FahrtenbuchEintragItemViewModel
                {
                    EintragId = x.EintragId,
                    FahrzeugText = fahrzeugText,
                    FahrerName = x.FahrerName,
                    BeifahrerName = string.IsNullOrWhiteSpace(x.BeifahrerName)
                        ? "-"
                        : x.BeifahrerName,
                    FahrtKategorie = x.FahrtKategorie,
                    Fahrtzweck = x.Fahrtzweck,
                    StartzeitUtc = x.StartzeitUtc,
                    EndzeitUtc = x.EndzeitUtc,
                    Startort = x.Startort,
                    Zielort = x.Zielort,
                    StartKilometerstand = x.StartKilometerstand,
                    EndKilometerstand = x.EndKilometerstand,
                    GefahreneKilometer = x.GefahreneKilometer,
                    TankmengeLiter = x.TankmengeLiter,
                    KilometerstandBeimTanken = x.KilometerstandBeimTanken,
                    Status = x.Status,
                    Bemerkung = x.Bemerkung
                })
                .ToList()
        };
    }

    private static IReadOnlyList<SelectListItem> BaueFahrtKategorieOptionen()
    {
        return
        [
            new SelectListItem("Einsatzfahrt", FahrtKategorie.Einsatzfahrt.ToString()),
            new SelectListItem("Dienstfahrt", FahrtKategorie.Dienstfahrt.ToString()),
            new SelectListItem("Werkstattfahrt", FahrtKategorie.Werkstattfahrt.ToString()),
            new SelectListItem("Tankfahrt", FahrtKategorie.Tankfahrt.ToString()),
            new SelectListItem("Überführungsfahrt", FahrtKategorie.Ueberfuehrungsfahrt.ToString()),
            new SelectListItem("Sonstige Fahrt", FahrtKategorie.Sonstige.ToString())
        ];
    }

    private static DateTimeOffset KonvertiereLokaleZeitNachUtc(DateTime lokaleZeit)
    {
        var lokaleZeitMitKind = DateTime.SpecifyKind(
            lokaleZeit,
            DateTimeKind.Local);

        return new DateTimeOffset(lokaleZeitMitKind).ToUniversalTime();
    }

    private FahrzeugPruefstatusViewModel BaueFahrzeugPruefstatusViewModel(
        FahrzeugDetail detail,
        ReadFahrzeugPruefstatusResult result)
    {
        return new FahrzeugPruefstatusViewModel
        {
            FahrzeugId = detail.FahrzeugId,
            Navigation = BaueNavigation(detail, "Pruefstatus"),
            PruefungTypOptionen = BaueFahrzeugPruefungTypOptionen(),
            Pruefungen = BaueFahrzeugPruefstatusItems(result)
        };
    }

    private static IReadOnlyList<FahrzeugPruefstatusItemViewModel> BaueFahrzeugPruefstatusItems(
        ReadFahrzeugPruefstatusResult result)
    {
        var vorhandenePruefungen = result.Pruefungen
            .ToDictionary(x => x.Typ, x => x);

        var alleTypen = new[]
        {
            FahrzeugPruefungTyp.HuAu,
            FahrzeugPruefungTyp.SicherheitspruefungElektrischeAnlage,
            FahrzeugPruefungTyp.SicherheitspruefungSauerstoffanlage,
            FahrzeugPruefungTyp.SicherheitspruefungAufbau,
            FahrzeugPruefungTyp.Service
        };

        return alleTypen
            .Select(typ =>
            {
                vorhandenePruefungen.TryGetValue(typ, out var pruefung);

                return new FahrzeugPruefstatusItemViewModel
                {
                    PruefungId = pruefung?.PruefungId,
                    Typ = typ,
                    FaelligAm = pruefung?.FaelligAm,
                    LetzteErledigungAm = pruefung?.LetzteErledigungAm,
                    Bemerkung = pruefung?.Bemerkung
                };
            })
            .ToList();
    }

    private static IReadOnlyList<SelectListItem> BaueFahrzeugPruefungTypOptionen()
    {
        return
        [
            new SelectListItem("HU/AU", FahrzeugPruefungTyp.HuAu.ToString()),
            new SelectListItem("Sicherheitsprüfung elektrische Anlage", FahrzeugPruefungTyp.SicherheitspruefungElektrischeAnlage.ToString()),
            new SelectListItem("Sicherheitsprüfung Sauerstoffanlage", FahrzeugPruefungTyp.SicherheitspruefungSauerstoffanlage.ToString()),
            new SelectListItem("Sicherheitsprüfung Aufbau", FahrzeugPruefungTyp.SicherheitspruefungAufbau.ToString()),
            new SelectListItem("Service allgemein", FahrzeugPruefungTyp.Service.ToString())
        ];
    }
}