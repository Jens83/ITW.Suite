using ITW.Application.Abstractions.DateTime;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Urlaub;
using ITW.Domain.Personnel.Enums;
using ITW.Web.Areas.Intensivtransport.ViewModels.Urlaubsplaner;
using ITW.Web.Authorization.Modules;
using ITW.Web.Controllers.Base;
using ITW.Web.Security.CurrentUser;
using ITW.Web.UI.Feedback;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Areas.Intensivtransport.Controllers;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Dienstplan)]
public sealed class MitarbeiterUrlaubController : BereichsControllerBase
{
    private const string AreaLayoutPath = "~/Views/Shared/_AppLayout.cshtml";

    private readonly ReadMitarbeiterUrlaubsplanerService _readService;
    private readonly EinreichenUrlaubsAntragService _einreichenService;
    private readonly BestaetigenUrlaubsEntscheidungService _bestaetigenService;
    private readonly ReadItwMitarbeiterprofileService _profileService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MitarbeiterUrlaubController(
        ReadMitarbeiterUrlaubsplanerService readService,
        EinreichenUrlaubsAntragService einreichenService,
        BestaetigenUrlaubsEntscheidungService bestaetigenService,
        ReadItwMitarbeiterprofileService profileService,
        IDateTimeProvider dateTimeProvider,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readService);
        _readService = readService;

        ArgumentNullException.ThrowIfNull(einreichenService);
        _einreichenService = einreichenService;

        ArgumentNullException.ThrowIfNull(bestaetigenService);
        _bestaetigenService = bestaetigenService;

        ArgumentNullException.ThrowIfNull(profileService);
        _profileService = profileService;

        ArgumentNullException.ThrowIfNull(dateTimeProvider);
        _dateTimeProvider = dateTimeProvider;
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    [HttpGet]
    public async Task<IActionResult> Index(int? jahr, CancellationToken cancellationToken)
    {
        var zugriff = await PruefeMitarbeiterzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var ausgewaehltesJahr = (jahr is >= 2000 and <= 2100) ? jahr.Value : _dateTimeProvider.Today.Year;
        var userId            = zugriff.CurrentUser!.UserId;

        var profil = await LadeProfilAsync(userId, cancellationToken);
        var kannUrlaub = profil?.Beschaeftigungsart == MitarbeiterBeschaeftigungsart.Festangestellt;

        var readResult = await _readService.ExecuteAsync(
            new ReadMitarbeiterUrlaubsplanerQuery
            {
                UserId              = userId,
                Jahr                = ausgewaehltesJahr,
                Beschaeftigungsart  = profil?.Beschaeftigungsart ?? MitarbeiterBeschaeftigungsart.Honorarkraft
            },
            cancellationToken);

        var ausstehendeTage = readResult.Zeitraeume
            .Where(z => z.Status == UrlaubszeitraumStatus.Ausstehend)
            .Sum(z => z.Urlaubstage);

        var antraege = readResult.Zeitraeume
            .Select(z => new MitarbeiterUrlaubAntragViewModel
            {
                Id             = z.Id,
                VonAnzeige     = z.Von.ToString("dd.MM.yyyy"),
                BisAnzeige     = z.Bis.ToString("dd.MM.yyyy"),
                Urlaubstage    = z.Urlaubstage,
                Notiz          = z.Notiz,
                Status         = z.Status,
                Begruendung    = z.Begruendung,
                Loesung        = z.Loesung,
                MussBestaetigt = z.Status != UrlaubszeitraumStatus.Ausstehend
                              && z.EingereichtVonUserId is not null
                              && !z.MitarbeiterBestaetigtAm.HasValue
            })
            .ToList();

        var viewModel = new MitarbeiterUrlaubViewModel
        {
            AusgewaehltesJahr       = ausgewaehltesJahr,
            IsSuccess               = readResult.IsSuccess,
            ErrorMessage            = readResult.ErrorMessage,
            Anspruchstage           = readResult.Anspruchstage,
            Uebertragstage          = readResult.Uebertragstage,
            GenommeneUrlaubstage    = readResult.GenommeneUrlaubstage,
            AusstehendeTage         = ausstehendeTage,
            Resturlaubstage         = readResult.Resturlaubstage,
            AnspruchIstStandardwert = readResult.AnspruchIstStandardwert,
            KannUrlaub              = kannUrlaub,
            AnzahlUnbestaetigt      = antraege.Count(a => a.MussBestaetigt),
            Antraege                = antraege
        };

        return BereichsView(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AntragStellen(
        DateOnly? von,
        DateOnly? bis,
        string? notiz,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefeMitarbeiterzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (!von.HasValue || !bis.HasValue)
        {
            TempData[FlashKeys.Error] = "Bitte einen gültigen Zeitraum angeben.";
            return RedirectToAction(nameof(Index));
        }

        var userId = zugriff.CurrentUser!.UserId;
        var profil = await LadeProfilAsync(userId, cancellationToken);
        var anzeigeName = profil?.AnzeigeName ?? userId;

        var result = await _einreichenService.ExecuteAsync(
            new EinreichenUrlaubsAntragCommand(
                userId,
                anzeigeName,
                von.Value,
                bis.Value,
                notiz),
            cancellationToken);

        if (!result.IsSuccess)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Der Urlaubsantrag konnte nicht eingereicht werden.";
        }
        else
        {
            TempData[FlashKeys.Success] = "Dein Urlaubsantrag wurde eingereicht und wartet auf Genehmigung.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bestaetigen(Guid zeitraumId, CancellationToken cancellationToken)
    {
        var zugriff = await PruefeMitarbeiterzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var userId  = zugriff.CurrentUser!.UserId;
        var profil  = await LadeProfilAsync(userId, cancellationToken);
        var name    = profil?.AnzeigeName ?? userId;

        var (isSuccess, errorMessage) = await _bestaetigenService.ExecuteAsync(
            new BestaetigenUrlaubsEntscheidungCommand(zeitraumId, userId, name),
            cancellationToken);

        if (!isSuccess)
        {
            TempData[FlashKeys.Error] = errorMessage ?? "Die Bestätigung konnte nicht gespeichert werden.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<ItwMitarbeiterprofilUebersichtDto?> LadeProfilAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var profileResult = await _profileService.ExecuteAsync(cancellationToken);

        if (!profileResult.IsSuccess)
        {
            return null;
        }

        return profileResult.Profile.FirstOrDefault(
            p => string.Equals(p.UserId, userId, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<MitarbeiterUrlaubZugriffResult> PruefeMitarbeiterzugriffAsync(
        CancellationToken cancellationToken)
    {
        var result = await HoleAktuellenBenutzerkontextAsync(cancellationToken);

        if (!result.IsSuccess || result.CurrentUser is null)
        {
            return MitarbeiterUrlaubZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        if (result.CurrentUser.Bereich != Bereich)
        {
            return MitarbeiterUrlaubZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        if (result.CurrentUser.Rolle != BereichsrolleCode.Mitarbeiter)
        {
            return MitarbeiterUrlaubZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        var profil = await LadeProfilAsync(result.CurrentUser.UserId, cancellationToken);
        if (profil?.Beschaeftigungsart != MitarbeiterBeschaeftigungsart.Festangestellt)
        {
            return MitarbeiterUrlaubZugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        return MitarbeiterUrlaubZugriffResult.MitBenutzer(result.CurrentUser);
    }

    private IActionResult BereichsView(object model)
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
        return View("~/Areas/Intensivtransport/Views/MitarbeiterUrlaub/Index.cshtml", model);
    }

    private sealed record MitarbeiterUrlaubZugriffResult(
        CurrentUserContext? CurrentUser,
        IActionResult? EarlyResult)
    {
        public static MitarbeiterUrlaubZugriffResult MitBenutzer(CurrentUserContext user)
            => new(user, null);

        public static MitarbeiterUrlaubZugriffResult MitErgebnis(IActionResult result)
            => new(null, result);
    }
}
