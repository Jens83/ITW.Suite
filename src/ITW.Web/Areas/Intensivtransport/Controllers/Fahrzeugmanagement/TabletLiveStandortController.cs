using ITW.Application.Organisation.Contracts;
using ITW.Fahrzeugmanagement.Application.Tracking;
using ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Fahrzeugmanagement;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Fahrzeugmanagement)]
public sealed class TabletLiveStandortController : IntensivtransportFahrzeugmanagementControllerBase
{
    private readonly ReadTabletLiveStandortOverviewService _readOverviewService;

    public TabletLiveStandortController(
        ReadTabletLiveStandortOverviewService readOverviewService,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readOverviewService);
        _readOverviewService = readOverviewService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var viewModel = await BaueViewModelAsync(cancellationToken);

        return View(viewModel);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Daten(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Kein Zugriff auf den Fahrzeugstandort."
            });
        }

        var viewModel = await BaueViewModelAsync(cancellationToken);

        return Json(new
        {
            success = true,
            aktualisiertAm = DateTimeOffset.Now.ToString("dd.MM.yyyy HH:mm:ss"),
            fahrzeug = BaueFahrzeugstandortDto(viewModel.FokusTablet)
        });
    }

    private async Task<TabletLiveStandortIndexViewModel> BaueViewModelAsync(
        CancellationToken cancellationToken)
    {
        var overview = await _readOverviewService.ExecuteAsync(cancellationToken);

        var tablets = overview.Tablets
            .Select(x => new TabletLiveStandortViewModel
            {
                TrackingGeraetId = x.TrackingGeraetId,
                DeviceIdentifier = x.DeviceIdentifier,
                IstAktiv = x.IstAktiv,
                IstOnline = x.IstOnline,
                HatStandort = x.HatStandort,
                IstInBewegung = x.IstInBewegung,
                RouteSessionId = x.RouteSessionId,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                SpeedKmh = x.SpeedKmh,
                GefahreneStreckeKm = x.GefahreneStreckeKm,
                ErfasstAmUtc = x.ErfasstAmUtc,
                LetzterKontaktAm = x.LetzterKontaktAm,
                RouteHistorie = x.RouteHistorie
                    .Select(p => new TabletRoutePointViewModel
                    {
                        Latitude = p.Latitude,
                        Longitude = p.Longitude,
                        SpeedKmh = p.SpeedKmh,
                        ErfasstAmUtc = p.ErfasstAmUtc
                    })
                    .ToList()
            })
            .ToList();

        var fokusTablet =
            tablets.FirstOrDefault(x => x.IstOnline && x.HatStandort) ??
            tablets.FirstOrDefault(x => x.HatStandort) ??
            tablets.FirstOrDefault();

        return new TabletLiveStandortIndexViewModel
        {
            AktualisiertAm = DateTimeOffset.Now,
            FokusTablet = fokusTablet,
            Tablets = tablets
        };
    }

    private static object? BaueFahrzeugstandortDto(
        TabletLiveStandortViewModel? tablet)
    {
        if (tablet is null)
        {
            return null;
        }

        return new
        {
            hatStandort = tablet.HatStandort,
            istOnline = tablet.IstOnline,
            istInBewegung = tablet.IstInBewegung,
            latitude = tablet.Latitude,
            longitude = tablet.Longitude,
            speedText = tablet.SpeedText,
            gefahreneStreckeText = tablet.GefahreneStreckeText,
            bewegungsstatusText = tablet.BewegungsstatusText,
            letzterKontaktText = tablet.LetzterKontaktText,
            erfasstAmText = tablet.ErfasstAmText,
            routeHistorie = tablet.RouteHistorie.Select(p => new
            {
                latitude = p.Latitude,
                longitude = p.Longitude,
                speedKmh = p.SpeedKmh,
                erfasstAmText = p.ErfasstAmText
            })
        };
    }
}