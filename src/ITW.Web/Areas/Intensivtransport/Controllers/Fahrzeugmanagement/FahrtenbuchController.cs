using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Fahrzeugmanagement.Application.Fahrtenbuch;
using ITW.Fahrzeugmanagement.Application.Fahrzeuge;
using ITW.Fahrzeugmanagement.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using ITW.Web.UI.Feedback;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Fahrzeugmanagement;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Fahrzeugmanagement)]
public sealed class FahrtenbuchController : IntensivtransportFahrzeugmanagementControllerBase
{
    private readonly ReadFahrzeugDetailService _readFahrzeugDetailService;
    private readonly ReadFahrtenbuchService _readFahrtenbuchService;
    private readonly CreateFahrtenbuchEintragService _createFahrtenbuchEintragService;
    private readonly ReadFahrtenbuchEintragDetailService _readFahrtenbuchEintragDetailService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public FahrtenbuchController(
        ReadFahrzeugDetailService readFahrzeugDetailService,
        ReadFahrtenbuchService readFahrtenbuchService,
        CreateFahrtenbuchEintragService createFahrtenbuchEintragService,
        ReadFahrtenbuchEintragDetailService readFahrtenbuchEintragDetailService,
        ICurrentUserContextAccessor currentUserContextAccessor,
        IDateTimeProvider dateTimeProvider)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readFahrzeugDetailService);
        _readFahrzeugDetailService = readFahrzeugDetailService;

        ArgumentNullException.ThrowIfNull(readFahrtenbuchService);
        _readFahrtenbuchService = readFahrtenbuchService;

        ArgumentNullException.ThrowIfNull(createFahrtenbuchEintragService);
        _createFahrtenbuchEintragService = createFahrtenbuchEintragService;

        ArgumentNullException.ThrowIfNull(readFahrtenbuchEintragDetailService);
        _readFahrtenbuchEintragDetailService = readFahrtenbuchEintragDetailService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
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

        var detail = await _readFahrzeugDetailService.ExecuteAsync(id, cancellationToken);

        if (detail is null)
        {
            return RedirectToAction("Index", "Fahrzeuge");
        }

        var fahrtenbuchResult = await _readFahrtenbuchService.ExecuteAsync(id, cancellationToken);

        return View(BaueFahrtenbuchViewModel(detail, fahrtenbuchResult));
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
            TempData[FlashKeys.FahrtenbuchFehler] = "Das Fahrzeug konnte nicht ermittelt werden.";
            return RedirectToAction("Index", "Fahrzeuge");
        }

        if (!input.Startzeit.HasValue)
        {
            TempData[FlashKeys.FahrtenbuchFehler] = "Bitte eine Startzeit eingeben.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        if (!input.Endzeit.HasValue)
        {
            TempData[FlashKeys.FahrtenbuchFehler] = "Bitte eine Endzeit eingeben.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        if (input.Endzeit.Value < input.Startzeit.Value)
        {
            TempData[FlashKeys.FahrtenbuchFehler] = "Die Endzeit darf nicht vor der Startzeit liegen.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        if (input.EndKilometerstand is null)
        {
            TempData[FlashKeys.FahrtenbuchFehler] = "Bitte einen Endkilometerstand eingeben.";
            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        if (input.EndKilometerstand.Value < input.StartKilometerstand)
        {
            TempData[FlashKeys.FahrtenbuchFehler] = "Der Endkilometerstand darf nicht kleiner als der Startkilometerstand sein.";
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
            TempData[FlashKeys.FahrtenbuchFehler] =
                result.ErrorMessage ?? "Der Fahrtenbucheintrag konnte nicht gespeichert werden.";

            return RedirectToAction(nameof(Fahrtenbuch), new { id = input.FahrzeugId });
        }

        TempData[FlashKeys.FahrtenbuchErfolg] = "Der Fahrtenbucheintrag wurde gespeichert.";

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

        var fahrzeug = await _readFahrzeugDetailService.ExecuteAsync(id, cancellationToken);

        if (fahrzeug is null)
        {
            return RedirectToAction("Index", "Fahrzeuge");
        }

        var eintrag = await _readFahrtenbuchEintragDetailService.ExecuteAsync(
            id,
            eintragId,
            cancellationToken);

        if (eintrag is null)
        {
            TempData[FlashKeys.FahrtenbuchFehler] = "Der Fahrtenbucheintrag wurde nicht gefunden.";
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult FahrtenbuchEintragAbschliessen(
        Guid id,
        Guid eintragId)
    {
        TempData[FlashKeys.FahrtenbuchFehler] =
            "Der separate Abschluss eines Fahrtenbucheintrags wird nicht mehr verwendet. Einträge werden direkt vollständig gespeichert.";

        if (id == Guid.Empty)
        {
            return RedirectToAction("Index", "Fahrzeuge");
        }

        if (eintragId == Guid.Empty)
        {
            return RedirectToAction(nameof(Fahrtenbuch), new { id });
        }

        return RedirectToAction(
            nameof(FahrtenbuchDetails),
            new { id, eintragId });
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
            Startzeit = _dateTimeProvider.Today.ToDateTime(new TimeOnly(7, 0)),
            Endzeit = _dateTimeProvider.UtcNow.LocalDateTime,
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
        var lokaleZeitMitKind = DateTime.SpecifyKind(lokaleZeit, DateTimeKind.Local);
        return new DateTimeOffset(lokaleZeitMitKind).ToUniversalTime();
    }
}
