using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;
using ITW.Fahrzeugmanagement.Application.Fahrzeuge;
using ITW.Fahrzeugmanagement.Application.FahrzeugPruefungen;
using ITW.Fahrzeugmanagement.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using ITW.Web.UI.Feedback;
using Microsoft.AspNetCore.Mvc;

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
    private readonly ReadFahrzeugPruefstatusService _readFahrzeugPruefstatusService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public FahrzeugeController(
        ReadFahrzeugUebersichtService readFahrzeugUebersichtService,
        CreateFahrzeugService createFahrzeugService,
        ReadFahrzeugDetailService readFahrzeugDetailService,
        UpdateFahrzeugStammdatenService updateFahrzeugStammdatenService,
        ICurrentUserContextAccessor currentUserContextAccessor,
        ReadFahrzeugDokumenteService readFahrzeugDokumenteService,
        ReadFahrzeugPruefstatusService readFahrzeugPruefstatusService,
        IDateTimeProvider dateTimeProvider)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readFahrzeugUebersichtService);
        _readFahrzeugUebersichtService = readFahrzeugUebersichtService;

        ArgumentNullException.ThrowIfNull(createFahrzeugService);
        _createFahrzeugService = createFahrzeugService;

        ArgumentNullException.ThrowIfNull(readFahrzeugDetailService);
        _readFahrzeugDetailService = readFahrzeugDetailService;

        ArgumentNullException.ThrowIfNull(updateFahrzeugStammdatenService);
        _updateFahrzeugStammdatenService = updateFahrzeugStammdatenService;

        ArgumentNullException.ThrowIfNull(readFahrzeugDokumenteService);
        _readFahrzeugDokumenteService = readFahrzeugDokumenteService;

        ArgumentNullException.ThrowIfNull(readFahrzeugPruefstatusService);
        _readFahrzeugPruefstatusService = readFahrzeugPruefstatusService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
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

        var detail = await _readFahrzeugDetailService.ExecuteAsync(id, cancellationToken);

        if (detail is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var dokumenteResult = await _readFahrzeugDokumenteService.ExecuteAsync(id, cancellationToken);
        var pruefstatusResult = await _readFahrzeugPruefstatusService.ExecuteAsync(id, cancellationToken);

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
                    HochgeladenAm = x.HochgeladenAm,
                    Heute = _dateTimeProvider.Today
                })
                .ToList(),
            Pruefungen = BaueFahrzeugPruefstatusItems(pruefstatusResult, _dateTimeProvider.Today)
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

        var detail = await _readFahrzeugDetailService.ExecuteAsync(id, cancellationToken);

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
    public IActionResult Vertraege(Guid id)
    {
        TempData[FlashKeys.FahrzeugeHinweis] =
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
        TempData[FlashKeys.FahrzeugeHinweis] =
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
        TempData[FlashKeys.FahrzeugeHinweis] =
            "Verträge gehören künftig in die Verwaltungs-Fahrzeugverwaltung und sind im Wachleiterbereich nicht mehr vorgesehen.";

        var zielFahrzeugId = fahrzeugId != Guid.Empty
            ? fahrzeugId
            : id;

        return zielFahrzeugId == Guid.Empty
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Details), new { id = zielFahrzeugId });
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

    private static IReadOnlyList<FahrzeugPruefstatusItemViewModel> BaueFahrzeugPruefstatusItems(
        ReadFahrzeugPruefstatusResult result,
        DateOnly heute)
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
                    Bemerkung = pruefung?.Bemerkung,
                    Heute = heute
                };
            })
            .ToList();
    }
}
