// Datei: src/ITW.Web/Controllers/Base/BereichsPasswortResetControllerBase.cs
using ITW.Application.Organisation.Contracts;
using ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;
using ITW.Application.Users.ReadOffenePasswortResetAnfragen;
using ITW.Application.Users.SetzeTemporaeresPasswort;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using ITW.Web.Security.PasswordReset;
using ITW.Web.ViewModels.Security;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Controllers.Base;

public abstract class BereichsPasswortResetControllerBase : BereichsControllerBase
{
    private readonly ReadOffenePasswortResetAnfragenService _readOffenePasswortResetAnfragenService;
    private readonly ReadOffenePasswortResetAnfrageDetailService _readOffenePasswortResetAnfrageDetailService;
    private readonly SetzeTemporaeresPasswortService _setzeTemporaeresPasswortService;

    protected BereichsPasswortResetControllerBase(
        ReadOffenePasswortResetAnfragenService readOffenePasswortResetAnfragenService,
        ReadOffenePasswortResetAnfrageDetailService readOffenePasswortResetAnfrageDetailService,
        SetzeTemporaeresPasswortService setzeTemporaeresPasswortService,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readOffenePasswortResetAnfragenService);
        _readOffenePasswortResetAnfragenService = readOffenePasswortResetAnfragenService;
        ArgumentNullException.ThrowIfNull(readOffenePasswortResetAnfrageDetailService);
        _readOffenePasswortResetAnfrageDetailService = readOffenePasswortResetAnfrageDetailService;
        ArgumentNullException.ThrowIfNull(setzeTemporaeresPasswortService);
        _setzeTemporaeresPasswortService = setzeTemporaeresPasswortService;
    }

    protected abstract string BereichName { get; }

    protected abstract string ListenTitel { get; }

    protected abstract string ListenBeschreibung { get; }

    protected abstract string AreaLayoutPath { get; }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var zugriff = await PruefePasswortResetZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _readOffenePasswortResetAnfragenService.ExecuteAsync(
            new ReadOffenePasswortResetAnfragenQuery(Bereich),
            cancellationToken);

        var viewModel = new PasswortResetAnfrageListeViewModel
        {
            Titel = ListenTitel,
            Beschreibung = ListenBeschreibung,
            BereichName = BereichName,
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            Anfragen = result.Anfragen
                .Select(x => new PasswortResetAnfrageEintragViewModel
                {
                    AnfrageId = x.AnfrageId,
                    UserId = x.UserId,
                    Benutzername = x.Benutzername,
                    Vollname = $"{x.Vorname} {x.Nachname}".Trim(),
                    AngefordertAm = x.AngefordertAm
                })
                .ToList()
        };

        return BereichsView("Index", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Vergabe(
        Guid anfrageId,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefePasswortResetZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var detailResult = await _readOffenePasswortResetAnfrageDetailService.ExecuteAsync(
            new ReadOffenePasswortResetAnfrageDetailQuery(anfrageId),
            cancellationToken);

        if (!detailResult.IsSuccess || detailResult.Anfrage is null)
        {
            TempData["ErrorMessage"] = detailResult.ErrorMessage ?? "Die Passwort-Reset-Anfrage konnte nicht geladen werden.";
            return RedirectToAction(nameof(Index));
        }

        if (detailResult.Anfrage.Bereich != Bereich)
        {
            return RedirectToKeinZugriff();
        }

        var viewModel = ErzeugeVergabeViewModel(detailResult.Anfrage);

        return BereichsView("Vergabe", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vergabe(
        PasswortResetVergabeViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefePasswortResetZugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var detailResult = await _readOffenePasswortResetAnfrageDetailService.ExecuteAsync(
            new ReadOffenePasswortResetAnfrageDetailQuery(viewModel.AnfrageId),
            cancellationToken);

        if (!detailResult.IsSuccess || detailResult.Anfrage is null)
        {
            TempData["ErrorMessage"] = detailResult.ErrorMessage ?? "Die Passwort-Reset-Anfrage konnte nicht geladen werden.";
            return RedirectToAction(nameof(Index));
        }

        if (detailResult.Anfrage.Bereich != Bereich)
        {
            return RedirectToKeinZugriff();
        }

        viewModel = FuellenVergabeViewModel(
            viewModel,
            detailResult.Anfrage);

        if (!ModelState.IsValid)
        {
            return BereichsView("Vergabe", viewModel);
        }

        var result = await _setzeTemporaeresPasswortService.ExecuteAsync(
            new SetzeTemporaeresPasswortCommand(
                viewModel.AnfrageId,
                zugriff.CurrentUser!.UserId,
                viewModel.TemporaeresPasswort),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Das temporäre Passwort konnte nicht gesetzt werden.");

            return BereichsView("Vergabe", viewModel);
        }

        TempData["SuccessMessage"] = "Das temporäre Passwort wurde gesetzt. Der Benutzer muss es beim nächsten Login sofort ändern.";
        return RedirectToAction(nameof(Index));
    }

    private PasswortResetVergabeViewModel ErzeugeVergabeViewModel(
        OffenePasswortResetAnfrageDetailDto anfrage)
    {
        return new PasswortResetVergabeViewModel
        {
            Titel = "Temporäres Passwort vergeben",
            Beschreibung = "Vergib ein temporäres Passwort für die offene Passwort-Reset-Anfrage. Beim nächsten Login muss der Benutzer das Passwort sofort ändern.",
            BereichName = BereichName,
            AnfrageId = anfrage.AnfrageId,
            UserId = anfrage.UserId,
            Benutzername = anfrage.Benutzername,
            Vollname = $"{anfrage.Vorname} {anfrage.Nachname}".Trim(),
            AngefordertAm = anfrage.AngefordertAm
        };
    }

    private PasswortResetVergabeViewModel FuellenVergabeViewModel(
        PasswortResetVergabeViewModel viewModel,
        OffenePasswortResetAnfrageDetailDto anfrage)
    {
        viewModel.Titel = "Temporäres Passwort vergeben";
        viewModel.Beschreibung = "Vergib ein temporäres Passwort für die offene Passwort-Reset-Anfrage. Beim nächsten Login muss der Benutzer das Passwort sofort ändern.";
        viewModel.BereichName = BereichName;
        viewModel.UserId = anfrage.UserId;
        viewModel.Benutzername = anfrage.Benutzername;
        viewModel.Vollname = $"{anfrage.Vorname} {anfrage.Nachname}".Trim();
        viewModel.AngefordertAm = anfrage.AngefordertAm;

        return viewModel;
    }

    private async Task<PasswortResetZugriffResult> PruefePasswortResetZugriffAsync(
        CancellationToken cancellationToken)
    {
        var currentUserLookup = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        if (!currentUserLookup.IsSuccess || currentUserLookup.CurrentUser is null)
        {
            return PasswortResetZugriffResult.Fehler(RedirectToKeinZugriff());
        }

        var currentUser = currentUserLookup.CurrentUser;

        if (currentUser.Bereich != Bereich)
        {
            return PasswortResetZugriffResult.Fehler(
                ErzeugeBereichsWeiterleitung(
                    currentUser.Bereich,
                    "PasswortReset",
                    "Index",
                    "Passwort-Reset-Anfragen",
                    BereichsRoutingHelper.GetBereichsname(Bereich),
                    $"Sie haben die Passwort-Reset-Anfragen für den Bereich {BereichsRoutingHelper.GetBereichsname(Bereich)} geöffnet. Aufgrund Ihrer aktuellen Zuordnung wird jetzt Ihr zuständiger Bereich geladen."));
        }

        if (!PasswortResetVerantwortungHelper.DarfAnfragenBearbeiten(currentUser, Bereich))
        {
            return PasswortResetZugriffResult.Fehler(RedirectToKeinZugriff());
        }

        return PasswortResetZugriffResult.Erfolg(currentUser);
    }

    private IActionResult BereichsView(string viewName, object model)
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
        return View($"~/Views/Shared/PasswortReset/{viewName}.cshtml", model);
    }

    private sealed record PasswortResetZugriffResult(
        CurrentUserContext? CurrentUser,
        IActionResult? EarlyResult)
    {
        public static PasswortResetZugriffResult Erfolg(CurrentUserContext currentUser) =>
            new(currentUser, null);

        public static PasswortResetZugriffResult Fehler(IActionResult earlyResult) =>
            new(null, earlyResult);
    }
}