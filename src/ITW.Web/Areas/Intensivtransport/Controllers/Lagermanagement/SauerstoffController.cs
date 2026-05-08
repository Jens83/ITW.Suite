using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Enums;
using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Application.Sauerstoff;
using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Lagermanagement;
using ITW.Web.Security.CurrentUser;
using ITW.Web.UI.Feedback;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Lagermanagement;

public sealed class SauerstoffController : LagermanagementControllerBase
{
    private const int VorwarnungTage = 30;

    private readonly ISauerstoffFlascheRepository  _flaschenRepo;
    private readonly ISauerstoffLieferungRepository _lieferungRepo;
    private readonly BewegeSauerstoffFlascheService _bewegeService;
    private readonly IFahrzeugRepository            _fahrzeugRepo;

    public SauerstoffController(
        ISauerstoffFlascheRepository flaschenRepo,
        ISauerstoffLieferungRepository lieferungRepo,
        BewegeSauerstoffFlascheService bewegeService,
        IFahrzeugRepository fahrzeugRepo,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(flaschenRepo);
        ArgumentNullException.ThrowIfNull(lieferungRepo);
        ArgumentNullException.ThrowIfNull(bewegeService);
        ArgumentNullException.ThrowIfNull(fahrzeugRepo);

        _flaschenRepo  = flaschenRepo;
        _lieferungRepo = lieferungRepo;
        _bewegeService = bewegeService;
        _fahrzeugRepo  = fahrzeugRepo;
    }

    // ── INDEX ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var flaschen    = await _flaschenRepo.GetAktiveAsync(cancellationToken);
        var lieferungen = await _lieferungRepo.GetAlleAsync(cancellationToken);
        var fahrzeuge   = await _fahrzeugRepo.GetFahrzeugeAsync(cancellationToken);
        var heute       = DateOnly.FromDateTime(DateTime.UtcNow);

        var aktiveFahrzeuge = fahrzeuge
            .Where(f => f.Status == FahrzeugStatus.Aktiv)
            .Select(f => new FahrzeugKurzinfo(f.Id, f.Kennzeichen))
            .ToList();

        var lieferungMap = lieferungen.ToDictionary(l => l.Id);

        var vms = flaschen.Select(f =>
        {
            lieferungMap.TryGetValue(f.LieferungId, out var lief);
            var liefDatum   = lief?.Lieferdatum ?? heute;
            var tage        = f.TageImSystem(liefDatum, heute);
            var hatRisiko   = f.HatLangzeitmietRisiko(liefDatum, heute, VorwarnungTage);
            var istKritisch = tage >= 180;
            var liefInfo    = lief is not null
                ? $"Ls. {lief.LieferscheinNummer} · {lief.Lieferdatum:dd.MM.yyyy}"
                : "–";

            return new SauerstoffFlascheViewModel
            {
                Id              = f.Id,
                LieferungId     = f.LieferungId,
                FahrzeugId      = f.FahrzeugId,
                Status          = f.Status,
                Groesse         = f.Groesse,
                FlaschenNummer  = f.FlaschenNummer,
                TageImSystem    = tage,
                HatRisiko       = hatRisiko,
                IstKritisch     = istKritisch,
                TageVerbleibend = Math.Max(0, 180 - tage),
                LieferungInfo   = liefInfo
            };
        }).ToList();

        var fahrzeugBestaende = aktiveFahrzeuge.Select(fz => new FahrzeugBestandInfo(
            fz.Id,
            fz.Kennzeichen,
            flaschen.Count(f => f.FahrzeugId == fz.Id))).ToList();

        var offeneLieferungen = lieferungen
            .Take(10)
            .Select(l => new SauerstoffLieferungKurzinfo(l.Id, l.LieferscheinNummer, l.Lieferdatum))
            .ToList();

        return LagerView("Sauerstoff/Index", new SauerstoffIndexViewModel
        {
            Flaschen          = vms,
            AktiveFahrzeuge   = aktiveFahrzeuge,
            OffeneLieferungen = offeneLieferungen,
            AnzahlVollDepot   = flaschen.Count(f => f.IstImDepot && f.Status == SauerstoffFlaschenStatus.Voll),
            AnzahlLeerDepot   = flaschen.Count(f => f.IstImDepot && f.Status == SauerstoffFlaschenStatus.Leer),
            FahrzeugBestaende = fahrzeugBestaende
        });
    }

    // ── SCAN ─────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Scan(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var fahrzeuge = await _fahrzeugRepo.GetFahrzeugeAsync(cancellationToken);
        var aktiveFahrzeuge = fahrzeuge
            .Where(f => f.Status == FahrzeugStatus.Aktiv)
            .Select(f => new FahrzeugKurzinfo(f.Id, f.Kennzeichen))
            .ToList();

        return LagerView("Sauerstoff/Scan", new SauerstoffScanViewModel
        {
            AktiveFahrzeuge = aktiveFahrzeuge
        });
    }

    [HttpGet]
    public async Task<IActionResult> ScanLookup(string flaschenNummer, CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return Json(new SauerstoffScanResultViewModel { Gefunden = false, FehlerMeldung = "Kein Zugriff." });

        if (string.IsNullOrWhiteSpace(flaschenNummer))
            return Json(new SauerstoffScanResultViewModel { Gefunden = false, FehlerMeldung = "Keine Flaschennummer angegeben." });

        var flasche = await _flaschenRepo.GetByFlaschenNummerAsync(flaschenNummer.Trim(), cancellationToken);
        if (flasche is null)
            return Json(new SauerstoffScanResultViewModel
            {
                Gefunden      = false,
                FehlerMeldung = $"Flasche mit Nummer '{flaschenNummer}' nicht gefunden. Bitte prüfen oder manuell erfassen."
            });

        var lieferungen = await _lieferungRepo.GetAlleAsync(cancellationToken);
        var lief        = lieferungen.FirstOrDefault(l => l.Id == flasche.LieferungId);
        var heute       = DateOnly.FromDateTime(DateTime.UtcNow);
        var liefDatum   = lief?.Lieferdatum ?? heute;
        var tage        = flasche.TageImSystem(liefDatum, heute);

        var fahrzeuge = await _fahrzeugRepo.GetFahrzeugeAsync(cancellationToken);
        var aktiveFahrzeuge = fahrzeuge
            .Where(f => f.Status == FahrzeugStatus.Aktiv)
            .Select(f => new FahrzeugKurzinfo(f.Id, f.Kennzeichen))
            .ToList();

        var bezeichnung = string.IsNullOrEmpty(flasche.FlaschenNummer)
            ? $"{flasche.Groesse switch { SauerstoffFlaschenGroesse.L2 => "2 L", SauerstoffFlaschenGroesse.L5 => "5 L", _ => "10 L" }} Flasche"
            : $"{flasche.Groesse switch { SauerstoffFlaschenGroesse.L2 => "2 L", SauerstoffFlaschenGroesse.L5 => "5 L", _ => "10 L" }} · Nr. {flasche.FlaschenNummer}";

        var standort = flasche.IstImDepot ? "Depot" :
            fahrzeuge.FirstOrDefault(f => f.Id == flasche.FahrzeugId)?.Kennzeichen ?? "Fahrzeug";

        return Json(new SauerstoffScanResultViewModel
        {
            Gefunden          = true,
            FlascheId         = flasche.Id,
            Bezeichnung       = bezeichnung,
            StatusAnzeige     = flasche.Status == SauerstoffFlaschenStatus.Voll ? "Voll" : "Leer",
            StandortAnzeige   = standort,
            TageImSystem      = tage,
            IstKritisch       = tage >= 180,
            KannNachFahrzeug  = flasche.IstImDepot && flasche.Status == SauerstoffFlaschenStatus.Voll,
            KannLeerZurueck   = flasche.IstImFahrzeug,
            AktiveFahrzeuge   = aktiveFahrzeuge
        });
    }

    // ── FLASCHE REGISTRIEREN ─────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FlascheRegistrieren(
        SauerstoffFlaschenGroesse groesse,
        string? flaschenNummer,
        Guid lieferungId,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        if (lieferungId == Guid.Empty)
        {
            TempData[FlashKeys.Error] = "Bitte eine Lieferung auswählen.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrWhiteSpace(flaschenNummer) &&
            await _flaschenRepo.ExistsByFlaschenNummerAsync(flaschenNummer, cancellationToken))
        {
            TempData[FlashKeys.Error] = $"Flasche mit Nummer '{flaschenNummer}' ist bereits registriert.";
            return RedirectToAction(nameof(LieferungDetail), new { id = lieferungId });
        }

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        var flasche = new SauerstoffFlasche(
            Guid.NewGuid(),
            groesse,
            flaschenNummer,
            lieferungId,
            DateTimeOffset.UtcNow,
            userCtx.CurrentUser!.UserId);

        await _flaschenRepo.AddAsync(flasche, cancellationToken);
        await _flaschenRepo.AddBewegungAsync(
            new SauerstoffBewegung(
                Guid.NewGuid(),
                flasche.Id,
                SauerstoffBewegungsTyp.EinlagerungVoll,
                null,
                null,
                DateTimeOffset.UtcNow,
                userCtx.CurrentUser!.UserId),
            cancellationToken);
        await _flaschenRepo.SaveChangesAsync(cancellationToken);

        TempData[FlashKeys.Success] = "Flasche wurde registriert.";
        return RedirectToAction(nameof(LieferungDetail), new { id = lieferungId });
    }

    // ── BEWEGUNGEN ───────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NachFahrzeug(
        Guid flascheId,
        Guid fahrzeugId,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        var (isSuccess, errorMessage) = await _bewegeService.NachFahrzeugAsync(
            flascheId, fahrzeugId, userCtx.CurrentUser!.UserId, cancellationToken);

        if (!isSuccess)
            TempData[FlashKeys.Error] = errorMessage;
        else
            TempData[FlashKeys.Success] = "Flasche wurde dem Fahrzeug zugewiesen.";

        return Request.Headers.TryGetValue("Referer", out var referer) && referer.ToString().Contains("/Scan")
            ? Ok()
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlsLeerZurueck(Guid flascheId, CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        var (isSuccess, errorMessage) = await _bewegeService.AlsLeerZurueckgebenAsync(
            flascheId, userCtx.CurrentUser!.UserId, cancellationToken);

        if (!isSuccess)
            TempData[FlashKeys.Error] = errorMessage;
        else
            TempData[FlashKeys.Success] = "Flasche als leer ins Depot zurückgebucht.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AbgebenAnLieferant(
        SauerstoffAbgabeViewModel form,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var userCtx = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        var (isSuccess, errorMessage) = await _bewegeService.AbgebenAnLieferantAsync(
            form.LeereFlaschenIds, userCtx.CurrentUser!.UserId, cancellationToken);

        if (!isSuccess)
            TempData[FlashKeys.Error] = errorMessage;
        else
            TempData[FlashKeys.Success] = $"{form.LeereFlaschenIds.Count} Flasche(n) an Lieferant abgegeben.";

        return RedirectToAction(nameof(Index));
    }

    // ── LIEFERUNGEN ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Lieferungen(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var lieferungen = await _lieferungRepo.GetAlleAsync(cancellationToken);
        var flaschen    = await _flaschenRepo.GetAktiveAsync(cancellationToken);

        var items = lieferungen.Select(l => new SauerstoffLieferungListItem
        {
            Id                 = l.Id,
            LieferscheinNummer = l.LieferscheinNummer,
            Lieferdatum        = l.Lieferdatum,
            Bemerkung          = l.Bemerkung,
            AnzahlFlaschen     = flaschen.Count(f => f.LieferungId == l.Id)
        }).ToList();

        return LagerView("Sauerstoff/Lieferungen", new SauerstoffLieferungenViewModel
        {
            Lieferungen = items
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LieferungNeu(
        string lieferscheinNummer,
        DateOnly lieferdatum,
        string? bemerkung,
        CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        if (string.IsNullOrWhiteSpace(lieferscheinNummer))
        {
            TempData[FlashKeys.Error] = "Lieferscheinnummer ist erforderlich.";
            return RedirectToAction(nameof(Lieferungen));
        }

        if (await _lieferungRepo.ExistsByLieferscheinNummerAsync(lieferscheinNummer, cancellationToken))
        {
            TempData[FlashKeys.Error] = $"Lieferung '{lieferscheinNummer}' existiert bereits.";
            return RedirectToAction(nameof(Lieferungen));
        }

        var userCtx  = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        var lieferung = new SauerstoffLieferung(
            Guid.NewGuid(),
            lieferscheinNummer,
            lieferdatum,
            bemerkung,
            DateTimeOffset.UtcNow,
            userCtx.CurrentUser!.UserId);

        await _lieferungRepo.AddAsync(lieferung, cancellationToken);
        await _lieferungRepo.SaveChangesAsync(cancellationToken);

        TempData[FlashKeys.Success] = $"Lieferung '{lieferscheinNummer}' angelegt.";
        return RedirectToAction(nameof(LieferungDetail), new { id = lieferung.Id });
    }

    [HttpGet]
    public async Task<IActionResult> LieferungDetail(Guid id, CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var lieferung = await _lieferungRepo.GetByIdAsync(id, cancellationToken);
        if (lieferung is null)
        {
            TempData[FlashKeys.Error] = "Lieferung nicht gefunden.";
            return RedirectToAction(nameof(Lieferungen));
        }

        var flaschen = await _flaschenRepo.GetByLieferungIdAsync(id, cancellationToken);
        var heute    = DateOnly.FromDateTime(DateTime.UtcNow);

        var flaschenVms = flaschen.Select(f =>
        {
            var tage = f.TageImSystem(lieferung.Lieferdatum, heute);
            return new SauerstoffFlascheViewModel
            {
                Id             = f.Id,
                LieferungId    = f.LieferungId,
                FahrzeugId     = f.FahrzeugId,
                Status         = f.Status,
                Groesse        = f.Groesse,
                FlaschenNummer = f.FlaschenNummer,
                TageImSystem   = tage,
                HatRisiko      = f.HatLangzeitmietRisiko(lieferung.Lieferdatum, heute, VorwarnungTage),
                IstKritisch    = tage >= 180,
                TageVerbleibend = Math.Max(0, 180 - tage)
            };
        }).ToList();

        return LagerView("Sauerstoff/LieferungDetail", new SauerstoffLieferungDetailViewModel
        {
            Id                 = lieferung.Id,
            LieferscheinNummer = lieferung.LieferscheinNummer,
            Lieferdatum        = lieferung.Lieferdatum,
            Bemerkung          = lieferung.Bemerkung,
            Flaschen           = flaschenVms
        });
    }
}
