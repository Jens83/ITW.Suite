using ITW.Application.Organisation.Contracts;
using ITW.Application.Organisation.ReadModulZuweisungenUebersicht;
using ITW.Application.Organisation.SetModulZuweisungStatus;
using ITW.Web.Areas.Geschaeftsfuehrung.ViewModels.Modules;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using ITW.Web.UI;
using Microsoft.AspNetCore.Mvc;
using ITW.Web.UI.Feedback;

namespace ITW.Web.Areas.Geschaeftsfuehrung.Controllers;

[Area("Geschaeftsfuehrung")]
public sealed class ModuleController : BereichsDashboardControllerBase
{
    private readonly ReadModulZuweisungenUebersichtService _readModulZuweisungenUebersichtService;
    private readonly SetModulZuweisungStatusService _setModulZuweisungStatusService;

    public ModuleController(
        ReadModulZuweisungenUebersichtService readModulZuweisungenUebersichtService,
        SetModulZuweisungStatusService setModulZuweisungStatusService,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readModulZuweisungenUebersichtService);
        _readModulZuweisungenUebersichtService = readModulZuweisungenUebersichtService;
        ArgumentNullException.ThrowIfNull(setModulZuweisungStatusService);
        _setModulZuweisungStatusService = setModulZuweisungStatusService;
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Vorstand;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
        {
            return redirectResult;
        }

        var result = await _readModulZuweisungenUebersichtService.ExecuteAsync(cancellationToken);

        var model = new ModuleIndexViewModel
        {
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            Empfaenger = result.Empfaenger
                .Select(x => new ModulEmpfaengerSpalteViewModel
                {
                    Bereich = x.Bereich,
                    Rolle = x.Rolle,
                    Titel = OrganisationDisplayText.Fuer(x.Rolle),
                    Untertitel = OrganisationDisplayText.Fuer(x.Bereich)
                })
                .ToList(),
            Module = result.Module
                .Select(x => new ModulMatrixZeileViewModel
                {
                    Modul = x.Modul,
                    Anzeigename = x.Anzeigename,
                    Zellen = x.Zellen
                        .Select(z => new ModulZuweisungZelleViewModel
                        {
                            Modul = x.Modul,
                            Bereich = z.Bereich,
                            Rolle = z.Rolle,
                            IstAktiv = z.IstAktiv
                        })
                        .ToList()
                })
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZuweisungSetzen(
        ModulCode modul,
        OrganisationsbereichCode bereich,
        BereichsrolleCode rolle,
        bool istAktiv,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
        {
            return redirectResult;
        }

        var currentUserLookup = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        if (!currentUserLookup.IsSuccess || currentUserLookup.CurrentUser is null)
        {
            return RedirectToKeinZugriff();
        }

        var result = await _setModulZuweisungStatusService.ExecuteAsync(
            new SetModulZuweisungStatusCommand(
                modul,
                bereich,
                rolle,
                istAktiv,
                currentUserLookup.CurrentUser.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Die Modulzuweisung konnte nicht gespeichert werden.";
            return RedirectToAction(nameof(Index));
        }

        TempData[FlashKeys.Success] = istAktiv
            ? $"Das Modul „{modul.GetAnzeigeName()}“ wurde zugewiesen."
            : $"Das Modul „{modul.GetAnzeigeName()}“ wurde entzogen.";

        return RedirectToAction(nameof(Index));
    }
}