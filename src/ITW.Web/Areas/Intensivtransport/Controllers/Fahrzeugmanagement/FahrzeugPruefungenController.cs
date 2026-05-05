using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Fahrzeugmanagement.Application.Fahrzeuge;
using ITW.Fahrzeugmanagement.Application.FahrzeugPruefungen;
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
public sealed class FahrzeugPruefungenController : IntensivtransportFahrzeugmanagementControllerBase
{
    private readonly ReadFahrzeugDetailService _readFahrzeugDetailService;
    private readonly ReadFahrzeugPruefstatusService _readFahrzeugPruefstatusService;
    private readonly SaveFahrzeugPruefungService _saveFahrzeugPruefungService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public FahrzeugPruefungenController(
        ReadFahrzeugDetailService readFahrzeugDetailService,
        ReadFahrzeugPruefstatusService readFahrzeugPruefstatusService,
        SaveFahrzeugPruefungService saveFahrzeugPruefungService,
        ICurrentUserContextAccessor currentUserContextAccessor,
        IDateTimeProvider dateTimeProvider)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readFahrzeugDetailService);
        _readFahrzeugDetailService = readFahrzeugDetailService;

        ArgumentNullException.ThrowIfNull(readFahrzeugPruefstatusService);
        _readFahrzeugPruefstatusService = readFahrzeugPruefstatusService;

        ArgumentNullException.ThrowIfNull(saveFahrzeugPruefungService);
        _saveFahrzeugPruefungService = saveFahrzeugPruefungService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
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

        var detail = await _readFahrzeugDetailService.ExecuteAsync(id, cancellationToken);

        if (detail is null)
        {
            return RedirectToAction("Index", "Fahrzeuge");
        }

        var result = await _readFahrzeugPruefstatusService.ExecuteAsync(id, cancellationToken);

        return View(BaueFahrzeugPruefstatusViewModel(detail, result));
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
            TempData[FlashKeys.FahrzeugPruefstatusFehler] = "Das Fahrzeug konnte nicht ermittelt werden.";
            return RedirectToAction("Index", "Fahrzeuge");
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
            TempData[FlashKeys.FahrzeugPruefstatusFehler] =
                result.ErrorMessage ?? "Der Prüfstatus konnte nicht gespeichert werden.";

            return RedirectToAction(nameof(Pruefstatus), new { id = input.FahrzeugId });
        }

        TempData[FlashKeys.FahrzeugPruefstatusErfolg] = "Der Prüfstatus wurde gespeichert.";

        return RedirectToAction(nameof(Pruefstatus), new { id = input.FahrzeugId });
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
            Pruefungen = BaueFahrzeugPruefstatusItems(result, _dateTimeProvider.Today)
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
