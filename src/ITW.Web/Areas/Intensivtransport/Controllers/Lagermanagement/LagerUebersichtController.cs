using ITW.Lagermanagement.Application.Contracts;
using ITW.Lagermanagement.Domain.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Lagermanagement;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Lagermanagement;

public sealed class LagerUebersichtController : LagermanagementControllerBase
{
    private const int O2VorwarnungTage      = 30;
    private const int MedikamentVorwarnTage = 30;

    private readonly ILagerArtikelRepository       _artikelRepo;
    private readonly IArtikelBestandRepository     _bestandRepo;
    private readonly IArtikelChargeRepository      _chargeRepo;
    private readonly ISauerstoffFlascheRepository  _sauerstoffRepo;
    private readonly ISauerstoffLieferungRepository _lieferungRepo;

    public LagerUebersichtController(
        ILagerArtikelRepository artikelRepo,
        IArtikelBestandRepository bestandRepo,
        IArtikelChargeRepository chargeRepo,
        ISauerstoffFlascheRepository sauerstoffRepo,
        ISauerstoffLieferungRepository lieferungRepo,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(artikelRepo);
        ArgumentNullException.ThrowIfNull(bestandRepo);
        ArgumentNullException.ThrowIfNull(chargeRepo);
        ArgumentNullException.ThrowIfNull(sauerstoffRepo);
        ArgumentNullException.ThrowIfNull(lieferungRepo);
        _artikelRepo    = artikelRepo;
        _bestandRepo    = bestandRepo;
        _chargeRepo     = chargeRepo;
        _sauerstoffRepo = sauerstoffRepo;
        _lieferungRepo  = lieferungRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var redirectResult = await PruefeBereichszugriffAsync(cancellationToken);
        if (redirectResult is not null)
            return redirectResult;

        var heute       = DateOnly.FromDateTime(DateTime.UtcNow);
        var alleFlaschen = await _sauerstoffRepo.GetAktiveAsync(cancellationToken);
        var lieferungen  = await _lieferungRepo.GetAlleAsync(cancellationToken);
        var lieferungMap = lieferungen.ToDictionary(l => l.Id);

        var o2Warnungen = alleFlaschen
            .Where(f => f.IstImDepot && f.Status == SauerstoffFlaschenStatus.Voll)
            .Select(f =>
            {
                lieferungMap.TryGetValue(f.LieferungId, out var lief);
                var liefDatum = lief?.Lieferdatum ?? heute;
                var tage      = f.TageImSystem(liefDatum, heute);
                return (flasche: f, tage, liefDatum);
            })
            .Where(x => x.flasche.HatLangzeitmietRisiko(x.liefDatum, heute, O2VorwarnungTage))
            .Select(x => new O2WarnungViewModel
            {
                FlascheId   = x.flasche.Id,
                Bezeichnung = string.IsNullOrEmpty(x.flasche.FlaschenNummer)
                    ? $"{x.flasche.Groesse switch { SauerstoffFlaschenGroesse.L2 => "2 L", SauerstoffFlaschenGroesse.L5 => "5 L", _ => "10 L" }} Flasche"
                    : $"Nr. {x.flasche.FlaschenNummer}",
                TageImSystem = x.tage,
                IstKritisch  = x.tage >= 180
            })
            .OrderByDescending(w => w.TageImSystem)
            .ToList();

        var bestaende     = await _bestandRepo.GetUnterschrittenerMindestbestandAsync(cancellationToken);
        var alleArtikel   = await _artikelRepo.GetAktiveAsync(cancellationToken);
        var artikelById   = alleArtikel.ToDictionary(a => a.Id);

        var bestandsWarnungen = bestaende
            .Where(b => artikelById.TryGetValue(b.ArtikelId, out _))
            .Select(b => new BestandsWarnungViewModel
            {
                ArtikelId      = b.ArtikelId,
                ArtikelName    = artikelById[b.ArtikelId].Name,
                Lagerort       = b.Lagerort,
                Menge          = b.Menge,
                Mindestbestand = artikelById[b.ArtikelId].Mindestbestand,
                Einheit        = artikelById[b.ArtikelId].BasisEinheit
            })
            .ToList();

        var baldAblaufend = await _chargeRepo.GetBaldAblaufendeAsync(heute, MedikamentVorwarnTage, cancellationToken);
        var abgelaufen    = await _chargeRepo.GetAbgelaufeneAsync(heute, cancellationToken);

        var chargeWarnungen = abgelaufen
            .Select(c => new ChargeWarnungViewModel
            {
                ChargeId      = c.Id,
                ArtikelName   = artikelById.TryGetValue(c.ArtikelId, out var a) ? a.Name : "Unbekannt",
                Ablaufdatum   = c.Ablaufdatum,
                Menge         = c.Menge,
                Einheit       = artikelById.TryGetValue(c.ArtikelId, out var a2) ? a2.BasisEinheit : "",
                IstAbgelaufen = true
            })
            .Concat(baldAblaufend.Select(c => new ChargeWarnungViewModel
            {
                ChargeId      = c.Id,
                ArtikelName   = artikelById.TryGetValue(c.ArtikelId, out var a) ? a.Name : "Unbekannt",
                Ablaufdatum   = c.Ablaufdatum,
                Menge         = c.Menge,
                Einheit       = artikelById.TryGetValue(c.ArtikelId, out var a2) ? a2.BasisEinheit : "",
                IstAbgelaufen = false
            }))
            .OrderBy(c => c.Ablaufdatum)
            .ToList();

        return LagerView("Uebersicht/Index", new LagerUebersichtViewModel
        {
            O2Warnungen       = o2Warnungen,
            BestandsWarnungen = bestandsWarnungen,
            ChargeWarnungen   = chargeWarnungen,
            AnzahlO2Voll      = alleFlaschen.Count(f => f.IstImDepot && f.Status == SauerstoffFlaschenStatus.Voll),
            AnzahlO2Leer      = alleFlaschen.Count(f => f.IstImDepot && f.Status == SauerstoffFlaschenStatus.Leer),
            AnzahlO2ImFahrzeug = alleFlaschen.Count(f => f.IstImFahrzeug)
        });
    }
}
