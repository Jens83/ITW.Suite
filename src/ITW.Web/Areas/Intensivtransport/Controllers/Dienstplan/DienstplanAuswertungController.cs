using ITW.Application.Abstractions.DateTime;
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
    private readonly IDateTimeProvider _dateTimeProvider;

    public DienstplanAuswertungController(
        ReadMonatsauswertungViewModelService readMonatsauswertungViewModelService,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readMonatsauswertungViewModelService);
        _readMonatsauswertungViewModelService = readMonatsauswertungViewModelService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
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

        var druckZeit = _dateTimeProvider.UtcNow;
        var document = new ITW.Web.Areas.Intensivtransport.Pdf.MonatsauswertungBuchhaltungPdfDocument(viewModel, druckZeit);
        var pdfBytes = document.Generate();

        var dateiname = $"buchhaltung-monatsauswertung-{druckZeit.LocalDateTime:yyyy-MM-dd-HHmm}.pdf";

        return File(pdfBytes, "application/pdf", dateiname);
    }
}