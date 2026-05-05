using ITW.Application.Organisation.Contracts;
using ITW.Application.Organisation.VisibilityScopes;
using ITW.Application.Users.ActivateUser;
using ITW.Application.Users.AssignArea;
using ITW.Application.Users.ChangeAreaRole;
using ITW.Application.Users.CreateUser;
using ITW.Application.Users.LockUser;
using ITW.Application.Users.ReadNichtZugeordneteBenutzerkonten;
using ITW.Application.Users.ReadUsersByScope;
using ITW.Web.Security.CurrentUser;
using ITW.Web.ViewModels.Benutzer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Controllers.Base;

[Authorize]
public abstract class BereichsBenutzerControllerBase : BereichsControllerBase
{
    private readonly ReadUsersByScopeService _readUsersByScopeService;
    private readonly AssignUserToPrimaryAreaService _assignUserToPrimaryAreaService;
    private readonly ChangeUserAreaRoleService _changeUserAreaRoleService;
    private readonly ReadNichtZugeordneteBenutzerkontenService _readNichtZugeordneteBenutzerkontenService;
    private readonly CreateBenutzerkontoService _createBenutzerkontoService;
    private readonly LockUserService _lockUserService;
    private readonly ActivateUserService _activateUserService;
    private readonly BenutzerSichtbarkeitsScopeErmittler _benutzerSichtbarkeitsScopeErmittler;


    protected BereichsBenutzerControllerBase(
    ReadUsersByScopeService readUsersByScopeService,
    AssignUserToPrimaryAreaService assignUserToPrimaryAreaService,
    ChangeUserAreaRoleService changeUserAreaRoleService,
    ReadNichtZugeordneteBenutzerkontenService readNichtZugeordneteBenutzerkontenService,
    CreateBenutzerkontoService createBenutzerkontoService,
    LockUserService lockUserService,
    ActivateUserService activateUserService,
    ICurrentUserContextAccessor currentUserContextAccessor,
    BenutzerSichtbarkeitsScopeErmittler benutzerSichtbarkeitsScopeErmittler)
    : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readUsersByScopeService);
        _readUsersByScopeService = readUsersByScopeService;
        ArgumentNullException.ThrowIfNull(assignUserToPrimaryAreaService);
        _assignUserToPrimaryAreaService = assignUserToPrimaryAreaService;
        ArgumentNullException.ThrowIfNull(changeUserAreaRoleService);
        _changeUserAreaRoleService = changeUserAreaRoleService;
        ArgumentNullException.ThrowIfNull(readNichtZugeordneteBenutzerkontenService);
        _readNichtZugeordneteBenutzerkontenService = readNichtZugeordneteBenutzerkontenService;
        ArgumentNullException.ThrowIfNull(createBenutzerkontoService);
        _createBenutzerkontoService = createBenutzerkontoService;
        ArgumentNullException.ThrowIfNull(lockUserService);
        _lockUserService = lockUserService;
        ArgumentNullException.ThrowIfNull(activateUserService);
        _activateUserService = activateUserService;
        ArgumentNullException.ThrowIfNull(benutzerSichtbarkeitsScopeErmittler);
        _benutzerSichtbarkeitsScopeErmittler = benutzerSichtbarkeitsScopeErmittler;
    }

    protected override abstract OrganisationsbereichCode Bereich { get; }

    protected abstract string BereichName { get; }

    protected abstract string ListenTitel { get; }

    protected abstract string ListenBeschreibung { get; }

    protected abstract string AreaLayoutPath { get; }

    protected abstract IReadOnlyList<BereichsrolleCode> ErlaubteRollen { get; }



    [HttpGet]
    public virtual async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var aktuellerBenutzer = zugriff.CurrentUser!;

        var result = await _readUsersByScopeService.ExecuteAsync(
            new ReadUsersByScopeQuery(
                Bereich,
                aktuellerBenutzer.Rolle),
            cancellationToken);

        var benutzer = result.Benutzer
            .Select(x => new BereichsbenutzerEintragViewModel
            {
                BereichszuordnungId = x.BereichszuordnungId,
                UserId = x.UserId,
                Benutzername = x.Benutzername,
                Email = x.Email,
                Bereich = ITW.Web.UI.OrganisationDisplayText.Fuer(x.Bereich),
                Rolle = ITW.Web.UI.OrganisationDisplayText.Fuer(x.Rolle),
                Fuehrungsverantwortung = ITW.Web.UI.OrganisationDisplayText.Fuer(x.Fuehrungsverantwortung),
                IsPrimary = x.IsPrimary,
                IsActive = x.IsActive,
                IstGesperrt = x.IstGesperrt,
                ZugewiesenAm = x.ZugewiesenAm
            })
            .ToList();

        var viewModel = new BereichsbenutzerlisteViewModel
        {
            Titel = ListenTitel,
            Beschreibung = ListenBeschreibung,
            BereichName = BereichName,
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            Benutzer = benutzer
        };

        return BereichsView("Index", viewModel);
    }
    
    
    [HttpGet]
    public async Task<IActionResult> Anlegen(
    string? returnUrl,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var viewModel = new BenutzerBereichszuordnungErfassenViewModel
        {
            Titel = $"Mitarbeiter in {BereichName} zuordnen",
            Beschreibung = $"Ein vorhandenes Benutzerkonto als primären Benutzer für den Bereich {BereichName} zuordnen.",
            ReturnUrl = returnUrl ?? string.Empty
        };

        await BefuelleErfassungsoptionenAsync(viewModel, cancellationToken);

        return BereichsView("Anlegen", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anlegen(
    BenutzerBereichszuordnungErfassenViewModel viewModel,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (string.IsNullOrWhiteSpace(viewModel.UserId))
        {
            ModelState.AddModelError(
                nameof(viewModel.UserId),
                "Bitte wählen Sie ein Benutzerkonto aus.");
        }

        if (!ErlaubteRollen.Contains(viewModel.Rolle))
        {
            ModelState.AddModelError(
                nameof(viewModel.Rolle),
                "Die ausgewählte Rolle ist für diesen Bereich nicht zulässig.");
        }

        if (!ModelState.IsValid)
        {
            await BefuelleErfassungsoptionenAsync(viewModel, cancellationToken);
            return BereichsView("Anlegen", viewModel);
        }

        var result = await _assignUserToPrimaryAreaService.ExecuteAsync(
            new AssignUserToPrimaryAreaCommand(
                viewModel.UserId,
                Bereich,
                viewModel.Rolle,
                viewModel.Fuehrungsverantwortung,
                viewModel.BestehendePrimaereZuordnungErsetzen),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Die Zuordnung konnte nicht gespeichert werden.");

            await BefuelleErfassungsoptionenAsync(viewModel, cancellationToken);
            return BereichsView("Anlegen", viewModel);
        }

        TempData["SuccessMessage"] = "Das Benutzerkonto wurde dem Bereich erfolgreich zugeordnet.";
        return RedirectToLocalOrIndex(viewModel.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BenutzerkontoAnlegen(
     BenutzerkontoNeuAnlegenViewModel viewModel,
     CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (!ErlaubteRollen.Contains(viewModel.Rolle))
        {
            ModelState.AddModelError(
                nameof(viewModel.Rolle),
                "Die ausgewählte Rolle ist für diesen Bereich nicht zulässig.");
        }

        if (!ModelState.IsValid)
        {
            BefuelleBenutzerkontoErstellungsoptionen(viewModel);
            return BereichsView("BenutzerkontoAnlegen", viewModel);
        }

        var createResult = await _createBenutzerkontoService.ExecuteAsync(
            new CreateBenutzerkontoCommand(
                viewModel.Benutzername,
                viewModel.Email,
                viewModel.Passwort),
            cancellationToken);

        if (!createResult.IsSuccess || createResult.Benutzerkonto is null)
        {
            ModelState.AddModelError(
                string.Empty,
                createResult.ErrorMessage ?? "Das Benutzerkonto konnte nicht angelegt werden.");

            BefuelleBenutzerkontoErstellungsoptionen(viewModel);
            return BereichsView("BenutzerkontoAnlegen", viewModel);
        }

        var assignResult = await _assignUserToPrimaryAreaService.ExecuteAsync(
            new AssignUserToPrimaryAreaCommand(
                createResult.Benutzerkonto.UserId,
                Bereich,
                viewModel.Rolle,
                viewModel.Fuehrungsverantwortung,
                BestehendePrimaereZuordnungErsetzen: false),
            cancellationToken);

        if (!assignResult.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                $"Das Benutzerkonto wurde angelegt, aber die Bereichszuordnung konnte nicht gespeichert werden: {assignResult.ErrorMessage}");

            BefuelleBenutzerkontoErstellungsoptionen(viewModel);
            return BereichsView("BenutzerkontoAnlegen", viewModel);
        }

        TempData["SuccessMessage"] =
            "Das Benutzerkonto wurde angelegt und dem Bereich erfolgreich zugeordnet. Der Mitarbeiter muss das initiale Passwort beim ersten Login sofort ändern.";

        return RedirectToLocalOrIndex(viewModel.ReturnUrl);
    }

    [HttpGet]
    public async Task<IActionResult> BenutzerkontoAnlegen(
    string? returnUrl,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var viewModel = new BenutzerkontoNeuAnlegenViewModel
        {
            Titel = $"Benutzerkonto für {BereichName} anlegen",
            Beschreibung = $"Ein neues zentrales Benutzerkonto anlegen und direkt dem Bereich {BereichName} zuordnen.",
            BereichName = BereichName,
            ReturnUrl = returnUrl ?? string.Empty
        };

        BefuelleBenutzerkontoErstellungsoptionen(viewModel);

        return BereichsView("BenutzerkontoAnlegen", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> RolleAendern(
    string userId,
    string? returnUrl,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var aktuellerBenutzer = zugriff.CurrentUser!;

        var benutzer = await HoleBenutzerImBereichAsync(
            userId,
            aktuellerBenutzer.Rolle,
            cancellationToken);

        if (benutzer is null)
        {
            return NotFound();
        }

        var viewModel = new BenutzerBereichsrolleAendernViewModel
        {
            Titel = $"Rechte in {BereichName} ändern",
            Beschreibung = $"Die Rechte für den Benutzer im Bereich {BereichName} anpassen.",
            BereichName = BereichName,
            UserId = benutzer.UserId,
            Benutzername = benutzer.Benutzername,
            Email = benutzer.Email,
            Rolle = benutzer.Rolle,
            Fuehrungsverantwortung = benutzer.Fuehrungsverantwortung,
            ReturnUrl = returnUrl ?? string.Empty
        };

        BefuelleRollenAenderungsoptionen(viewModel);

        return BereichsView("RolleAendern", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RolleAendern(
    BenutzerBereichsrolleAendernViewModel viewModel,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var aktuellerBenutzer = zugriff.CurrentUser!;

        if (!ErlaubteRollen.Contains(viewModel.Rolle))
        {
            ModelState.AddModelError(
                nameof(viewModel.Rolle),
                "Die ausgewählten Rechte sind für diesen Bereich nicht zulässig.");
        }

        if (!ModelState.IsValid)
        {
            BefuelleRollenAenderungsoptionen(viewModel);
            return BereichsView("RolleAendern", viewModel);
        }

        var benutzer = await HoleBenutzerImBereichAsync(
            viewModel.UserId,
            aktuellerBenutzer.Rolle,
            cancellationToken);

        if (benutzer is null)
        {
            return NotFound();
        }

        var result = await _changeUserAreaRoleService.ExecuteAsync(
            new ChangeUserAreaRoleCommand(
                viewModel.UserId,
                viewModel.Rolle,
                viewModel.Fuehrungsverantwortung),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Die Rechte konnten nicht geändert werden.");

            viewModel.Benutzername = benutzer.Benutzername;
            viewModel.Email = benutzer.Email;
            BefuelleRollenAenderungsoptionen(viewModel);

            return BereichsView("RolleAendern", viewModel);
        }

        TempData["SuccessMessage"] = "Die Rechte wurden erfolgreich geändert.";
        return RedirectToLocalOrIndex(viewModel.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Sperren(
    string userId,
    string? returnUrl,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var aktuellerBenutzer = zugriff.CurrentUser!;

        if (string.Equals(aktuellerBenutzer.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Das eigene Benutzerkonto kann nicht gesperrt werden.";
            return RedirectToLocalOrIndex(returnUrl);
        }

        var benutzer = await HoleBenutzerImBereichAsync(
            userId,
            aktuellerBenutzer.Rolle,
            cancellationToken);

        if (benutzer is null)
        {
            return NotFound();
        }

        var result = await _lockUserService.ExecuteAsync(
            new LockUserCommand(userId),
            cancellationToken);

        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? $"Das Benutzerkonto von '{benutzer.Benutzername}' wurde gesperrt."
            : result.ErrorMessage ?? "Das Benutzerkonto konnte nicht gesperrt werden.";

        return RedirectToLocalOrIndex(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aktivieren(
    string userId,
    string? returnUrl,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefeBenutzerverwaltungszugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var aktuellerBenutzer = zugriff.CurrentUser!;

        var benutzer = await HoleBenutzerImBereichAsync(
            userId,
            aktuellerBenutzer.Rolle,
            cancellationToken);

        if (benutzer is null)
        {
            return NotFound();
        }

        var result = await _activateUserService.ExecuteAsync(
            new ActivateUserCommand(userId),
            cancellationToken);

        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? $"Das Benutzerkonto von '{benutzer.Benutzername}' wurde aktiviert."
            : result.ErrorMessage ?? "Das Benutzerkonto konnte nicht aktiviert werden.";

        return RedirectToLocalOrIndex(returnUrl);
    }

    private IActionResult BereichsView(string viewName, object model)
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
        return View($"~/Views/Shared/BereichsBenutzer/{viewName}.cshtml", model);
    }

    private async Task BefuelleErfassungsoptionenAsync(
    BenutzerBereichszuordnungErfassenViewModel viewModel,
    CancellationToken cancellationToken)
    {
        viewModel.BereichName = BereichName;
        viewModel.RollenOptionen = BaueRollenOptionen();
        viewModel.FuehrungsverantwortungsOptionen = BaueFuehrungsverantwortungsOptionen();

        var result = await _readNichtZugeordneteBenutzerkontenService.ExecuteAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            viewModel.HatVerfuegbareBenutzerkonten = false;
            viewModel.BenutzerkontoOptionen = new[]
            {
            new SelectListItem
            {
                Value = string.Empty,
                Text = "Benutzerkonten konnten nicht geladen werden."
            }
        };

            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Die verfügbaren Benutzerkonten konnten nicht geladen werden.");

            return;
        }

        var optionen = result.Benutzerkonten
            .OrderBy(x => x.Benutzername, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
            .Select(x => new SelectListItem
            {
                Value = x.UserId,
                Text = BaueBenutzerkontoText(x.Benutzername, x.Email, x.IstGesperrt)
            })
            .ToList();

        optionen.Insert(0, new SelectListItem
        {
            Value = string.Empty,
            Text = optionen.Count == 0
                ? "Keine verfügbaren Benutzerkonten vorhanden"
                : "Bitte Benutzerkonto auswählen"
        });

        viewModel.HatVerfuegbareBenutzerkonten = result.Benutzerkonten.Count > 0;
        viewModel.BenutzerkontoOptionen = optionen;
    }

    private void BefuelleBenutzerkontoErstellungsoptionen(
    BenutzerkontoNeuAnlegenViewModel viewModel)
    {
        viewModel.BereichName = BereichName;
        viewModel.RollenOptionen = BaueRollenOptionen();
        viewModel.FuehrungsverantwortungsOptionen = BaueFuehrungsverantwortungsOptionen();
    }

   

   

    private void BefuelleRollenAenderungsoptionen(
        BenutzerBereichsrolleAendernViewModel viewModel)
    {
        viewModel.BereichName = BereichName;
        viewModel.RollenOptionen = BaueRollenOptionen();
        viewModel.FuehrungsverantwortungsOptionen = BaueFuehrungsverantwortungsOptionen();
    }

    
    private IReadOnlyList<SelectListItem> BaueRollenOptionen()
    {
        return ErlaubteRollen
            .Select(x => new SelectListItem
            {
                Value = ((int)x).ToString(),
                Text = ITW.Web.UI.OrganisationDisplayText.Fuer(x)
            })
            .ToList();
    }

   
    private static IReadOnlyList<SelectListItem> BaueFuehrungsverantwortungsOptionen()
    {
        var werte = Enum.GetValues<FuehrungsverantwortungCode>();

        return werte
            .Select(x => new SelectListItem
            {
                Value = ((int)x).ToString(),
                Text = ITW.Web.UI.OrganisationDisplayText.Fuer(x)
            })
            .ToList();
    }

    private static string BaueBenutzerkontoText(
        string benutzername,
        string email,
        bool istGesperrt)
    {
        var basisText = string.IsNullOrWhiteSpace(email)
            ? benutzername
            : $"{benutzername} ({email})";

        return istGesperrt
            ? $"{basisText} [gesperrt]"
            : basisText;
    }

    private async Task<BenutzerBereichsuebersichtDto?> HoleBenutzerImBereichAsync(
     string userId,
     BereichsrolleCode aufrufendeRolle,
     CancellationToken cancellationToken)
    {
        var result = await _readUsersByScopeService.ExecuteAsync(
            new ReadUsersByScopeQuery(Bereich, aufrufendeRolle),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return null;
        }

        return result.Benutzer.FirstOrDefault(x =>
            string.Equals(x.UserId, userId, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<BenutzerverwaltungszugriffResult> PruefeBenutzerverwaltungszugriffAsync(
    CancellationToken cancellationToken)
    {
        var result = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        if (!result.IsSuccess || result.CurrentUser is null)
        {
            return BenutzerverwaltungszugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        var aktuellerBenutzer = result.CurrentUser;

        if (aktuellerBenutzer.Bereich != Bereich)
        {
            var fremdbereichScope = _benutzerSichtbarkeitsScopeErmittler.ErmittleFuerBenutzerlisten(
                aktuellerBenutzer.Bereich,
                aktuellerBenutzer.Rolle);

            if (fremdbereichScope.DarfBenutzerlistenLesen &&
                fremdbereichScope.Zielbereich.HasValue)
            {
                return BenutzerverwaltungszugriffResult.MitErgebnis(
                    ErzeugeBereichsWeiterleitung(
                        fremdbereichScope.Zielbereich.Value,
                        "Benutzer",
                        "Index",
                        "Benutzerverwaltung",
                        BereichName,
                        $"Sie haben die Benutzerverwaltung für den Bereich {BereichName} geöffnet. Aufgrund Ihrer Bereichszuordnung wird jetzt die für Sie zuständige Benutzerverwaltung geladen."));
            }

            return BenutzerverwaltungszugriffResult.MitErgebnis(
                ErzeugeBereichsWeiterleitung(
                    aktuellerBenutzer.Bereich,
                    "Dashboard",
                    "Index",
                    "Benutzerverwaltung",
                    BereichName,
                    $"Sie haben die Benutzerverwaltung für den Bereich {BereichName} geöffnet. Für Ihre aktuelle Rolle ist stattdessen Ihr eigenes Dashboard vorgesehen."));
        }

        var sameAreaScope = _benutzerSichtbarkeitsScopeErmittler.ErmittleFuerBenutzerlisten(
            aktuellerBenutzer.Bereich,
            aktuellerBenutzer.Rolle);

        if (!sameAreaScope.DarfBenutzerlistenLesen ||
            !sameAreaScope.Zielbereich.HasValue ||
            sameAreaScope.Zielbereich.Value != Bereich)
        {
            return BenutzerverwaltungszugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        return BenutzerverwaltungszugriffResult.MitBenutzer(aktuellerBenutzer);
    }

   

  

    private sealed record BenutzerverwaltungszugriffResult(
        CurrentUserContext? CurrentUser,
        IActionResult? EarlyResult)
    {
        public static BenutzerverwaltungszugriffResult MitBenutzer(CurrentUserContext currentUser)
            => new(currentUser, null);

        public static BenutzerverwaltungszugriffResult MitErgebnis(IActionResult earlyResult)
            => new(null, earlyResult);
    }

    private IActionResult RedirectToLocalOrIndex(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

}