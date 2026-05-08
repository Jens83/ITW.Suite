using ITW.Lagermanagement.Application.Artikel;
using ITW.Lagermanagement.Application.Bestand;
using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Lagermanagement;
using ITW.Web.Security.CurrentUser;
using ITW.Web.UI.Feedback;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Lagermanagement;

public sealed class LagerArtikelController : LagermanagementControllerBase
{
    private readonly ILagerArtikelRepository   _artikelRepo;
    private readonly IArtikelBestandRepository _bestandRepo;
    private readonly IArtikelChargeRepository  _chargeRepo;
    private readonly CreateLagerArtikelService _createService;
    private readonly UpdateLagerArtikelService _updateService;
    private readonly EinbuchenArtikelService   _einbuchenService;
    private readonly AusbuchenArtikelService   _ausbuchenService;

    public LagerArtikelController(
        ILagerArtikelRepository artikelRepo,
        IArtikelBestandRepository bestandRepo,
        IArtikelChargeRepository chargeRepo,
        CreateLagerArtikelService createService,
        UpdateLagerArtikelService updateService,
        EinbuchenArtikelService einbuchenService,
        AusbuchenArtikelService ausbuchenService,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(artikelRepo);
        ArgumentNullException.ThrowIfNull(bestandRepo);
        ArgumentNullException.ThrowIfNull(chargeRepo);
        ArgumentNullException.ThrowIfNull(createService);
        ArgumentNullException.ThrowIfNull(updateService);
        ArgumentNullException.ThrowIfNull(einbuchenService);
        ArgumentNullException.ThrowIfNull(ausbuchenService);
        _artikelRepo      = artikelRepo;
        _bestandRepo      = bestandRepo;
        _chargeRepo       = chargeRepo;
        _createService    = createService;
        _updateService    = updateService;
        _einbuchenService = einbuchenService;
        _ausbuchenService = ausbuchenService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var artikel    = await _artikelRepo.GetAlleAsync(cancellationToken);
        var bestaende  = await _bestandRepo.GetAlleAsync(cancellationToken);
        var heute      = DateOnly.FromDateTime(DateTime.Today);
        var chargen    = await _chargeRepo.GetAktiveAsync(cancellationToken);

        var bestandByArtikelLagerort = bestaende
            .GroupBy(b => (b.ArtikelId, b.Lagerort))
            .ToDictionary(g => g.Key, g => g.Sum(b => b.Menge));

        var chargenBestandByArtikelLagerort = chargen
            .GroupBy(c => (c.ArtikelId, c.Lagerort))
            .ToDictionary(g => g.Key, g => g.Sum(c => c.Menge));

        var zeilen = artikel.Select(a =>
        {
            int MengeFor(Lagerort l) =>
                a.HatAblaufdatum
                    ? chargenBestandByArtikelLagerort.TryGetValue((a.Id, l), out var m) ? m : 0
                    : bestandByArtikelLagerort.TryGetValue((a.Id, l), out var m2) ? m2 : 0;

            var gesamtbestand = MengeFor(Lagerort.Depot) + MengeFor(Lagerort.Fahrzeug1) + MengeFor(Lagerort.Fahrzeug2);

            return new ArtikelZeileViewModel
            {
                Id               = a.Id,
                Name             = a.Name,
                Kategorie        = a.Kategorie,
                Einheit          = a.BasisEinheit,
                PackungsGroesse  = a.PackungsGroesse,
                PackungsEinheit  = a.PackungsEinheit,
                VerbrauchProPatient = a.VerbrauchProPatient,
                HatAblaufdatum   = a.HatAblaufdatum,
                Mindestbestand   = a.Mindestbestand,
                IstAktiv         = a.IstAktiv,
                MengeDepot       = MengeFor(Lagerort.Depot),
                MengeFahrzeug1   = MengeFor(Lagerort.Fahrzeug1),
                MengeFahrzeug2   = MengeFor(Lagerort.Fahrzeug2),
                Gesamtbestand    = gesamtbestand,
                IstUnterMindest  = gesamtbestand < a.Mindestbestand && a.Mindestbestand > 0
            };
        }).ToList();

        return LagerView("Artikel/Index", new LagerArtikelIndexViewModel { Zeilen = zeilen });
    }

    [HttpGet]
    public async Task<IActionResult> Neu(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        return LagerView("Artikel/NeuBearbeiten", new ArtikelFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Neu(ArtikelFormViewModel form, CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        var result = await _createService.ExecuteAsync(
            new CreateLagerArtikelCommand(
                form.Name,
                form.Kategorie,
                form.BasisEinheit,
                form.PackungsGroesse,
                form.PackungsEinheit,
                form.VerbrauchProPatient,
                form.HatAblaufdatum,
                form.Mindestbestand,
                userCtx.CurrentUser!.UserId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage;
            return LagerView("Artikel/NeuBearbeiten", form);
        }

        TempData[FlashKeys.Success] = $"Artikel '{form.Name}' wurde angelegt.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Bearbeiten(Guid id, CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var artikel = await _artikelRepo.GetByIdAsync(id, cancellationToken);
        if (artikel is null)
            return RedirectToAction(nameof(Index));

        var form = new ArtikelFormViewModel
        {
            Id                  = artikel.Id,
            Name                = artikel.Name,
            Kategorie           = artikel.Kategorie,
            BasisEinheit        = artikel.BasisEinheit,
            PackungsGroesse     = artikel.PackungsGroesse,
            PackungsEinheit     = artikel.PackungsEinheit,
            VerbrauchProPatient = artikel.VerbrauchProPatient,
            HatAblaufdatum      = artikel.HatAblaufdatum,
            Mindestbestand      = artikel.Mindestbestand
        };

        return LagerView("Artikel/NeuBearbeiten", form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bearbeiten(ArtikelFormViewModel form, CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        var (isSuccess, errorMessage) = await _updateService.ExecuteAsync(
            new UpdateLagerArtikelCommand(
                form.Id,
                form.Name,
                form.Kategorie,
                form.BasisEinheit,
                form.PackungsGroesse,
                form.PackungsEinheit,
                form.VerbrauchProPatient,
                form.HatAblaufdatum,
                form.Mindestbestand,
                userCtx.CurrentUser!.UserId),
            cancellationToken);

        if (!isSuccess)
        {
            TempData[FlashKeys.Error] = errorMessage;
            return LagerView("Artikel/NeuBearbeiten", form);
        }

        TempData[FlashKeys.Success] = $"Artikel '{form.Name}' wurde gespeichert.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Einbuchen(
        Guid artikelId,
        Lagerort lagerort,
        int menge,
        bool istPackung,
        DateOnly? ablaufdatum,
        string? chargeNummer,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var artikel = await _artikelRepo.GetByIdAsync(artikelId, cancellationToken);
        var mengeInBasis = istPackung && artikel?.PackungsGroesse.HasValue == true
            ? menge * artikel!.PackungsGroesse!.Value
            : menge;

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        var (isSuccess, errorMessage) = await _einbuchenService.ExecuteAsync(
            new EinbuchenArtikelCommand(
                artikelId,
                lagerort,
                mengeInBasis,
                userCtx.CurrentUser!.UserId,
                ablaufdatum,
                chargeNummer),
            cancellationToken);

        if (!isSuccess)
            TempData[FlashKeys.Error] = errorMessage;
        else
            TempData[FlashKeys.Success] = $"{mengeInBasis} {artikel?.BasisEinheit} eingebucht.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ausbuchen(
        Guid artikelId,
        Lagerort lagerort,
        int menge,
        Guid? chargeId,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        var (isSuccess, errorMessage) = await _ausbuchenService.ExecuteAsync(
            new AusbuchenArtikelCommand(artikelId, lagerort, menge, userCtx.CurrentUser!.UserId, chargeId),
            cancellationToken);

        if (!isSuccess)
            TempData[FlashKeys.Error] = errorMessage;
        else
            TempData[FlashKeys.Success] = $"{menge} Einheiten ausgebucht.";

        return RedirectToAction(nameof(Index));
    }
}
