using Azure.Core;
using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Read;
using ITW.Web.Areas.Intensivtransport.Services.Dienstplan.Write;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;
using ITW.Web.UI.Feedback;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Dienstplan;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Dienstplan)]
[Route("Intensivtransport/Dienstplan")]
public sealed class DienstplanMitarbeiterController : IntensivtransportDienstplanControllerBase
{
    private readonly ReadDienstplanIndexViewModelService _readDienstplanIndexViewModelService;
    private readonly ReadSichtbarerDienstplanViewModelService _readSichtbarerDienstplanViewModelService;
    private readonly ToggleMitarbeiterWunschService _toggleMitarbeiterWunschService;
    private readonly SaveFreelancerMonatswunschMitarbeiterService _saveFreelancerMonatswunschMitarbeiterService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DienstplanMitarbeiterController(
        ReadDienstplanIndexViewModelService readDienstplanIndexViewModelService,
        ReadSichtbarerDienstplanViewModelService readSichtbarerDienstplanViewModelService,
        ToggleMitarbeiterWunschService toggleMitarbeiterWunschService,
        SaveFreelancerMonatswunschMitarbeiterService saveFreelancerMonatswunschMitarbeiterService,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readDienstplanIndexViewModelService);
        _readDienstplanIndexViewModelService = readDienstplanIndexViewModelService;

        ArgumentNullException.ThrowIfNull(readSichtbarerDienstplanViewModelService);
        _readSichtbarerDienstplanViewModelService = readSichtbarerDienstplanViewModelService;

        ArgumentNullException.ThrowIfNull(toggleMitarbeiterWunschService);
        _toggleMitarbeiterWunschService = toggleMitarbeiterWunschService;

        ArgumentNullException.ThrowIfNull(saveFreelancerMonatswunschMitarbeiterService);
        _saveFreelancerMonatswunschMitarbeiterService = saveFreelancerMonatswunschMitarbeiterService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
    }

    [HttpGet("Mitarbeiter")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: false,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (zugriff.CurrentUser!.Rolle == BereichsrolleCode.Wachleiter)
        {
            return RedirectToAction("Index", "DienstplanWachleiter");
        }

        var viewModel = await _readDienstplanIndexViewModelService.ExecuteAsync(
            new ReadDienstplanIndexViewModelQuery(
                zugriff.CurrentUser,
                _dateTimeProvider.Today.Month,
                _dateTimeProvider.Today.Year,
                WunschphaseDirektOeffnen: true,
                ModalAutomatischOeffnen: false),
            cancellationToken);

        return BereichsView("Index", viewModel);
    }

    [HttpGet("Plan")]
    public async Task<IActionResult> Plan(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: false,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var viewModel = await _readSichtbarerDienstplanViewModelService.ExecuteAsync(
            new ReadSichtbarerDienstplanViewModelQuery(zugriff.CurrentUser!.UserId),
            cancellationToken);

        return BereichsView("Plan", viewModel);
    }

    [HttpPost("ToggleWunsch")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleWunsch(
        Guid periodeId,
        int jahr,
        int monat,
        int tag,
        string? wunschTyp,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: false,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var isAjaxRequest = string.Equals(
            Request.Headers["X-Requested-With"].ToString(),
            "XMLHttpRequest",
            StringComparison.OrdinalIgnoreCase);

        IActionResult Fehler(string message)
        {
            if (isAjaxRequest)
            {
                return BadRequest(new { message });
            }

            TempData[FlashKeys.Error] = message;
            return RedirectToDienstplanEinstieg();
        }

        IActionResult Erfolg()
        {
            if (isAjaxRequest)
            {
                return Ok(new { success = true });
            }

            return RedirectToDienstplanEinstieg();
        }

        if (periodeId == Guid.Empty)
        {
            return Fehler("Die aktive Dienstplanperiode ist ungültig.");
        }

        if (!TryBaueDatum(jahr, monat, tag, out var datum))
        {
            return Fehler("Der ausgewählte Tag ist ungültig.");
        }

        var result = await _toggleMitarbeiterWunschService.ExecuteAsync(
            new ToggleMitarbeiterWunschCommand(
                periodeId,
                zugriff.CurrentUser!.UserId,
                datum,
                wunschTyp),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return Fehler(result.ErrorMessage ?? "Der Wunsch konnte nicht gespeichert werden.");
        }

        return Erfolg();
    }

    [HttpPost("FreelancerMonatswunschSpeichern")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FreelancerMonatswunschSpeichern(
        Guid periodeId,
        int gewuenschteDienste,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeDienstplanzugriffAsync(
            erfordertWachleiter: false,
            cancellationToken);

        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _saveFreelancerMonatswunschMitarbeiterService.ExecuteAsync(
            new SaveFreelancerMonatswunschMitarbeiterCommand(
                periodeId,
                zugriff.CurrentUser!.UserId,
                gewuenschteDienste),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Die gewünschte Monatsanzahl konnte nicht gespeichert werden.";
            return RedirectToDienstplanEinstieg();
        }

        TempData[FlashKeys.Success] =
            $"Die gewünschte Monatsanzahl wurde auf {gewuenschteDienste} {(gewuenschteDienste == 1 ? "Dienst" : "Dienste")} gespeichert.";

        return RedirectToDienstplanEinstieg();
    }
}