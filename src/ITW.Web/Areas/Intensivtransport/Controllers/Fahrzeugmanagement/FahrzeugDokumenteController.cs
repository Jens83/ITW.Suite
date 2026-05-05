using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Fahrzeugmanagement.Application.FahrzeugDokumente;
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
public sealed class FahrzeugDokumenteController : IntensivtransportFahrzeugmanagementControllerBase
{
    private readonly ReadFahrzeugDetailService _readFahrzeugDetailService;
    private readonly ReadFahrzeugDokumenteService _readFahrzeugDokumenteService;
    private readonly UploadFahrzeugDokumentService _uploadFahrzeugDokumentService;
    private readonly DownloadFahrzeugDokumentService _downloadFahrzeugDokumentService;
    private readonly DeleteFahrzeugDokumentService _deleteFahrzeugDokumentService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public FahrzeugDokumenteController(
        ReadFahrzeugDetailService readFahrzeugDetailService,
        ReadFahrzeugDokumenteService readFahrzeugDokumenteService,
        UploadFahrzeugDokumentService uploadFahrzeugDokumentService,
        DownloadFahrzeugDokumentService downloadFahrzeugDokumentService,
        DeleteFahrzeugDokumentService deleteFahrzeugDokumentService,
        ICurrentUserContextAccessor currentUserContextAccessor,
        IDateTimeProvider dateTimeProvider)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readFahrzeugDetailService);
        _readFahrzeugDetailService = readFahrzeugDetailService;

        ArgumentNullException.ThrowIfNull(readFahrzeugDokumenteService);
        _readFahrzeugDokumenteService = readFahrzeugDokumenteService;

        ArgumentNullException.ThrowIfNull(uploadFahrzeugDokumentService);
        _uploadFahrzeugDokumentService = uploadFahrzeugDokumentService;

        ArgumentNullException.ThrowIfNull(downloadFahrzeugDokumentService);
        _downloadFahrzeugDokumentService = downloadFahrzeugDokumentService;

        ArgumentNullException.ThrowIfNull(deleteFahrzeugDokumentService);
        _deleteFahrzeugDokumentService = deleteFahrzeugDokumentService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
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

        var detail = await _readFahrzeugDetailService.ExecuteAsync(id, cancellationToken);

        if (detail is null)
        {
            return RedirectToAction("Index", "Fahrzeuge");
        }

        var dokumenteResult = await _readFahrzeugDokumenteService.ExecuteAsync(id, cancellationToken);

        return View("Dokumente", BaueFahrzeugDokumenteViewModel(detail, dokumenteResult));
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
            TempData[FlashKeys.FahrzeugDokumenteFehler] = "Das Fahrzeug konnte nicht ermittelt werden.";
            return RedirectToAction("Index", "Fahrzeuge");
        }

        if (input.Datei is null || input.Datei.Length == 0)
        {
            TempData[FlashKeys.FahrzeugDokumenteFehler] = "Bitte eine Datei auswählen.";
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
            TempData[FlashKeys.FahrzeugDokumenteFehler] =
                result.ErrorMessage ?? "Der Tankbeleg konnte nicht hochgeladen werden.";

            return RedirectToAction(nameof(Tankbelege), new { id = input.FahrzeugId });
        }

        TempData[FlashKeys.FahrzeugDokumenteErfolg] = "Der Tankbeleg wurde erfolgreich hochgeladen.";

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
            new DownloadFahrzeugDokumentQuery(id, dokumentId),
            cancellationToken);

        if (!result.IsSuccess ||
            result.Dateiinhalt is null ||
            string.IsNullOrWhiteSpace(result.Dateiname) ||
            string.IsNullOrWhiteSpace(result.ContentType))
        {
            TempData[FlashKeys.FahrzeugDokumenteFehler] =
                result.ErrorMessage ?? "Der Tankbeleg konnte nicht heruntergeladen werden.";

            return RedirectToAction(nameof(Tankbelege), new { id });
        }

        return File(result.Dateiinhalt, result.ContentType, result.Dateiname);
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
            new DeleteFahrzeugDokumentCommand(id, dokumentId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.FahrzeugDokumenteFehler] =
                result.ErrorMessage ?? "Der Tankbeleg konnte nicht gelöscht werden.";

            return RedirectToAction(nameof(Tankbelege), new { id });
        }

        TempData[FlashKeys.FahrzeugDokumenteErfolg] = "Der Tankbeleg wurde gelöscht.";

        return RedirectToAction(nameof(Tankbelege), new { id });
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
                    HochgeladenAm = x.HochgeladenAm,
                    Heute = _dateTimeProvider.Today
                })
                .ToList()
        };
    }

    private static IReadOnlyList<SelectListItem> BaueFahrzeugDokumentKategorieOptionen()
    {
        return [new SelectListItem("Tankbeleg", FahrzeugDokumentKategorie.Tankbeleg.ToString())];
    }
}
