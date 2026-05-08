using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Application.Einsatz;
using ITW.Lagermanagement.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Lagermanagement;
using ITW.Web.Security.CurrentUser;
using ITW.Web.UI.Feedback;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Lagermanagement;

public sealed class EinsatzVerbrauchController : LagermanagementControllerBase
{
    private readonly IEinsatzVerbrauchRepository   _verbrauchRepo;
    private readonly ILagerArtikelRepository       _artikelRepo;
    private readonly ErfasseEinsatzVerbrauchService _erfasseService;

    public EinsatzVerbrauchController(
        IEinsatzVerbrauchRepository verbrauchRepo,
        ILagerArtikelRepository artikelRepo,
        ErfasseEinsatzVerbrauchService erfasseService,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(verbrauchRepo);
        ArgumentNullException.ThrowIfNull(artikelRepo);
        ArgumentNullException.ThrowIfNull(erfasseService);
        _verbrauchRepo  = verbrauchRepo;
        _artikelRepo    = artikelRepo;
        _erfasseService = erfasseService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var bis  = DateOnly.FromDateTime(DateTime.Today);
        var von  = bis.AddDays(-30);
        var liste = await _verbrauchRepo.GetByDatumBereichAsync(von, bis, cancellationToken);

        var viewModel = new EinsatzVerbrauchIndexViewModel
        {
            Eintraege = liste.Select(v => new EinsatzVerbrauchZeileViewModel
            {
                Id        = v.Id,
                Datum     = v.Datum,
                Fahrzeug  = v.Fahrzeug,
                Patienten = v.Patienten,
                Bemerkung = v.Bemerkung
            }).ToList()
        };

        return LagerView("EinsatzVerbrauch/Index", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Neu(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var artikel = await _artikelRepo.GetAktiveAsync(cancellationToken);

        var form = new EinsatzVerbrauchFormViewModel
        {
            Datum    = DateOnly.FromDateTime(DateTime.Today),
            Fahrzeug = Lagerort.Fahrzeug1,
            VerfuegbareArtikel = artikel
                .Where(a => !a.HatAblaufdatum)
                .Select(a => new ArtikelAuswahlViewModel
                {
                    Id                  = a.Id,
                    Name                = a.Name,
                    Einheit             = a.BasisEinheit,
                    VerbrauchProPatient = a.VerbrauchProPatient
                }).ToList()
        };

        return LagerView("EinsatzVerbrauch/Neu", form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Neu(EinsatzVerbrauchFormViewModel form, CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        var manuellePositionen = form.Positionen
            .Where(p => p.Menge > 0)
            .Select(p => new EinsatzVerbrauchArtikelCommand(p.ArtikelId, p.Menge, false))
            .ToList();

        var result = await _erfasseService.ExecuteAsync(
            new ErfasseEinsatzVerbrauchCommand(
                form.Datum,
                form.Fahrzeug,
                form.Patienten,
                form.Bemerkung,
                manuellePositionen,
                userCtx.CurrentUser!.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage;
            return RedirectToAction(nameof(Neu));
        }

        TempData[FlashKeys.Success] = "Einsatzverbrauch wurde erfasst.";
        return RedirectToAction(nameof(Index));
    }
}
