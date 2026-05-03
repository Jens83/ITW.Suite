using ITW.Application.Organisation.Contracts;
using ITW.Fahrzeugmanagement.Application.Tracking;
using ITW.Web.Areas.Intensivtransport.ViewModels.Fahrzeugmanagement;
using ITW.Web.Authorization.Modules;
using ITW.Web.Security.CurrentUser;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace ITW.Web.Areas.Intensivtransport.Controllers.Fahrzeugmanagement;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Fahrzeugmanagement)]
public sealed class TrackingGeraeteController : IntensivtransportFahrzeugmanagementControllerBase
{
    private readonly ReadTabletLiveStandortOverviewService _readOverviewService;
    private readonly CreateTrackingGeraetSetupCodeService _createSetupCodeService;
    private readonly IConfiguration _configuration;

    public TrackingGeraeteController(
        ReadTabletLiveStandortOverviewService readOverviewService,
        CreateTrackingGeraetSetupCodeService createSetupCodeService,
        ICurrentUserContextAccessor currentUserContextAccessor,
        IConfiguration configuration)
        : base(currentUserContextAccessor)
    {
        _readOverviewService = readOverviewService
            ?? throw new ArgumentNullException(nameof(readOverviewService));

        _createSetupCodeService = createSetupCodeService
            ?? throw new ArgumentNullException(nameof(createSetupCodeService));

        _configuration = configuration
            ?? throw new ArgumentNullException(nameof(configuration));
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

        var viewModel = await BaueViewModelAsync(
            erfolgsmeldung: TempData["TrackingGeraete.Erfolg"]?.ToString(),
            fehlermeldung: TempData["TrackingGeraete.Fehler"]?.ToString(),
            cancellationToken: cancellationToken);

        return View(viewModel);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Neu(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeFahrzeugmanagementzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        SetzeBereichslayout();

        var result = await _createSetupCodeService.ExecuteAsync(
            new CreateTrackingGeraetSetupCodeCommand(
                TabletName: "Einsatz-Tablet",
                ErstelltVonUserId: zugriff.CurrentUser?.UserId),
            cancellationToken);

        if (!result.IsSuccess ||
            string.IsNullOrWhiteSpace(result.EinrichtungscodeAnzeige))
        {
            TempData["TrackingGeraete.Fehler"] =
                result.ErrorMessage ?? "Der QR-Code konnte nicht erzeugt werden.";

            return RedirectToAction(nameof(Index));
        }

        var setupLink = ErzeugeSetupLink(result.EinrichtungscodeAnzeige);

        var viewModel = new TrackingGeraetQrSetupViewModel
        {
            SetupLink = setupLink,
            QrCodeSvg = ErzeugeQrCodeSvg(setupLink),
            GueltigBisText = result.GueltigBisUtc.HasValue
                ? result.GueltigBisUtc.Value.ToLocalTime().ToString("HH:mm")
                : "-"
        };

        return View("Neu", viewModel);
    }

    private async Task<TrackingGeraeteIndexViewModel> BaueViewModelAsync(
        string? erfolgsmeldung,
        string? fehlermeldung,
        CancellationToken cancellationToken)
    {
        var overview = await _readOverviewService.ExecuteAsync(cancellationToken);

        return new TrackingGeraeteIndexViewModel
        {
            Erfolgsmeldung = erfolgsmeldung,
            Fehlermeldung = fehlermeldung,
            Geraete = overview.Tablets
                .Select(x => new TrackingGeraetIndexItemViewModel
                {
                    TrackingGeraetId = x.TrackingGeraetId,
                    DeviceIdentifier = x.DeviceIdentifier,
                    IstAktiv = x.IstAktiv,
                    IstOnline = x.IstOnline,
                    HatStandort = x.HatStandort,
                    IstInBewegung = x.IstInBewegung,
                    LetzterKontaktText = x.LetzterKontaktAm.HasValue
                        ? x.LetzterKontaktAm.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")
                        : "-",
                    GeschwindigkeitText = x.SpeedKmh.HasValue
                        ? $"{x.SpeedKmh.Value:0.0} km/h"
                        : "-",
                    StreckeText = $"{x.GefahreneStreckeKm:0.00} km"
                })
                .ToList()
        };
    }

    private string ErzeugeSetupLink(string einrichtungscode)
    {
        var relativerPfad =
            $"/tablet/setup?code={Uri.EscapeDataString(einrichtungscode)}&autostart=true";

        var publicBaseUrl = _configuration["App:PublicBaseUrl"]?.Trim();

        if (!string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            return $"{publicBaseUrl.TrimEnd('/')}{relativerPfad}";
        }

        var setupLink = Url.Action(
            action: "Index",
            controller: "TabletSetup",
            values: new
            {
                code = einrichtungscode,
                autostart = true
            },
            protocol: Request.Scheme);

        if (!string.IsNullOrWhiteSpace(setupLink))
        {
            return setupLink;
        }

        return relativerPfad;
    }

    private static string ErzeugeQrCodeSvg(string setupLink)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(
            setupLink,
            QRCodeGenerator.ECCLevel.Q);

        var qrCode = new SvgQRCode(qrCodeData);

        return qrCode.GetGraphic(12);
    }
}