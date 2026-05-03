using ITW.Application.Organisation.Contracts;
using ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Read;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Dienstplan)]
[Route("Intensivtransport/Dienstplan")]
public sealed class DienstplanAuswertungController : IntensivtransportDienstplanControllerBase
{
    private readonly ReadMonatsauswertungViewModelService _readMonatsauswertungViewModelService;

    public DienstplanAuswertungController(
        ReadMonatsauswertungViewModelService readMonatsauswertungViewModelService,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        _readMonatsauswertungViewModelService = readMonatsauswertungViewModelService
            ?? throw new ArgumentNullException(nameof(readMonatsauswertungViewModelService));
    }

    [HttpGet("Monatsauswertung")]
    public async Task<IActionResult> Monatsauswertung(
        Guid? periodeId,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var viewModel = await _readMonatsauswertungViewModelService.ExecuteAsync(
            new ReadMonatsauswertungViewModelQuery(periodeId),
            cancellationToken);

        return BereichsView("Monatsauswertung", viewModel);
    }

    [HttpGet("BuchhaltungPdf")]
    public async Task<IActionResult> BuchhaltungPdf(
        Guid? periodeId,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: true,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var viewModel = await _readMonatsauswertungViewModelService.ExecuteAsync(
            new ReadMonatsauswertungViewModelQuery(periodeId),
            cancellationToken);

        if (!viewModel.IsSuccess || !viewModel.AusgewaehltePeriodeId.HasValue)
        {
            TempData["ErrorMessage"] = viewModel.ErrorMessage ?? "Die Buchhaltungs-PDF konnte nicht erstellt werden.";
            return RedirectToAction(nameof(Monatsauswertung), new { periodeId });
        }

        var document = new ITW.Web.Areas.Intensivtransport.Pdf.MonatsauswertungBuchhaltungPdfDocument(viewModel);
        var pdfBytes = document.Generate();

        var dateiname = $"buchhaltung-monatsauswertung-{DateTime.Now:yyyy-MM-dd-HHmm}.pdf";

        return File(pdfBytes, "application/pdf", dateiname);
    }
}