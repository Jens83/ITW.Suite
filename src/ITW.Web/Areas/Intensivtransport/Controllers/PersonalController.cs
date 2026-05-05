using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.ProfileQueries;
using ITW.Application.Personnel.Profiles;
using ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter;
using ITW.Web.Authorization.Modules;
using ITW.Web.Controllers.Base;
using ITW.Web.Navigation.AreaNavigation;
using ITW.Web.Security.CurrentUser;
using ITW.Web.ViewModels.Personal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ITW.Web.UI.Feedback;

namespace ITW.Web.Areas.Intensivtransport.Controllers;

[Area("Intensivtransport")]
[RequireModule(ModulCode.Personal)]
public sealed class PersonalController : BereichsControllerBase
{
    private const string AreaLayoutPath = "~/Views/Shared/_AppLayout.cshtml";

    private readonly ReadItwMitarbeiterprofileService _readItwMitarbeiterprofileService;
    private readonly ReadItwMitarbeiterprofilDetailService _readItwMitarbeiterprofilDetailService;
    private readonly ReadItwMitarbeiterDetailUebersichtService _readItwMitarbeiterDetailUebersichtService;
    private readonly ReadAllgemeinesMitarbeiterprofilDetailService _readAllgemeinesMitarbeiterprofilDetailService;
    private readonly SaveItwMitarbeiterprofilService _saveItwMitarbeiterprofilService;
    private readonly SaveAllgemeinesMitarbeiterprofilService _saveAllgemeinesMitarbeiterprofilService;

    public PersonalController(
        ReadItwMitarbeiterprofileService readItwMitarbeiterprofileService,
        ReadItwMitarbeiterprofilDetailService readItwMitarbeiterprofilDetailService,
        ReadItwMitarbeiterDetailUebersichtService readItwMitarbeiterDetailUebersichtService,
        ReadAllgemeinesMitarbeiterprofilDetailService readAllgemeinesMitarbeiterprofilDetailService,
        SaveItwMitarbeiterprofilService saveItwMitarbeiterprofilService,
        SaveAllgemeinesMitarbeiterprofilService saveAllgemeinesMitarbeiterprofilService,
        ICurrentUserContextAccessor currentUserContextAccessor)
        : base(currentUserContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(readItwMitarbeiterprofileService);
        _readItwMitarbeiterprofileService = readItwMitarbeiterprofileService;
        ArgumentNullException.ThrowIfNull(readItwMitarbeiterprofilDetailService);
        _readItwMitarbeiterprofilDetailService = readItwMitarbeiterprofilDetailService;
        ArgumentNullException.ThrowIfNull(readItwMitarbeiterDetailUebersichtService);
        _readItwMitarbeiterDetailUebersichtService = readItwMitarbeiterDetailUebersichtService;
        ArgumentNullException.ThrowIfNull(readAllgemeinesMitarbeiterprofilDetailService);
        _readAllgemeinesMitarbeiterprofilDetailService = readAllgemeinesMitarbeiterprofilDetailService;
        ArgumentNullException.ThrowIfNull(saveItwMitarbeiterprofilService);
        _saveItwMitarbeiterprofilService = saveItwMitarbeiterprofilService;
        ArgumentNullException.ThrowIfNull(saveAllgemeinesMitarbeiterprofilService);
        _saveAllgemeinesMitarbeiterprofilService = saveAllgemeinesMitarbeiterprofilService;
    }

    protected override OrganisationsbereichCode Bereich => OrganisationsbereichCode.Intensivtransport;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _readItwMitarbeiterprofileService.ExecuteAsync(cancellationToken);

        var viewModel = new ItwMitarbeiterprofilListeViewModel
        {
            Titel = "Personalprofile",
            Beschreibung = "Allgemeine Stammdaten und ITW-Qualifikationen werden getrennt von den zentralen Benutzerkonten gepflegt.",
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            Profile = result.Profile
                .Select(x => new ItwMitarbeiterprofilEintragViewModel
                {
                    UserId = x.UserId,
                    AnzeigeName = x.AnzeigeName,
                    Benutzername = x.Benutzername,
                    Email = x.Email,
                    IstGesperrt = x.IstGesperrt,
                    HatStammdatenprofil = x.HatStammdatenprofil,
                    StammdatenAktualisiertAm = x.StammdatenAktualisiertAm,
                    BeschaeftigungsartText = FormatiereBeschaeftigungsart(x.Beschaeftigungsart),
                    HatProfil = x.HatProfil,
                    Hauptqualifikation = x.Hauptqualifikation,
                    Zusatzqualifikationen = x.Zusatzqualifikationen,
                    ProfilAktualisiertAm = x.ProfilAktualisiertAm
                })
                .ToArray()
        };

        return BereichsView(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Details(
     string userId,
     [FromServices] ITW.Application.Personnel.Documents.ReadMitarbeiterDokumenteService readMitarbeiterDokumenteService,
     CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _readItwMitarbeiterDetailUebersichtService.ExecuteAsync(userId, cancellationToken);
        if (!result.IsSuccess || result.Mitarbeiter is null)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Die Mitarbeiterdetails konnten nicht geladen werden.";
            return RedirectToAction(nameof(Index));
        }

        var dokumenteResult = await readMitarbeiterDokumenteService.ExecuteAsync(userId, cancellationToken);

        var mitarbeiter = result.Mitarbeiter;

        var viewModel = new MitarbeiterDetailViewModel
        {
            Titel = "Mitarbeiterdetails",
            Beschreibung = "Gebündelte Übersicht der aktuell verfügbaren Mitarbeiterbereiche im Intensivtransport.",
            UserId = mitarbeiter.UserId,
            Benutzername = mitarbeiter.Benutzername,
            Email = mitarbeiter.Email,
            IstGesperrt = mitarbeiter.IstGesperrt,
            Rolle = mitarbeiter.Rolle,
            Fuehrungsverantwortung = mitarbeiter.Fuehrungsverantwortung,
            HatItwProfil = mitarbeiter.HatItwProfil,
            Hauptqualifikation = mitarbeiter.Hauptqualifikation,
            Zusatzqualifikationen = mitarbeiter.Zusatzqualifikationen,
            ProfilAktualisiertAm = mitarbeiter.ProfilAktualisiertAm,
            HatStammdatenprofil = mitarbeiter.HatStammdatenprofil,
            AnzeigeName = mitarbeiter.AnzeigeName,
            Vorname = mitarbeiter.Vorname,
            Nachname = mitarbeiter.Nachname,
            BeschaeftigungsartText = FormatiereBeschaeftigungsart(mitarbeiter.Beschaeftigungsart),
            Telefonnummer = mitarbeiter.Telefonnummer,
            AnschriftKurz = mitarbeiter.AnschriftKurz,
            StammdatenAktualisiertAm = mitarbeiter.StammdatenAktualisiertAm,
            ZugewiesenAm = mitarbeiter.ZugewiesenAm,
            Dokumente = dokumenteResult.IsSuccess
                ? dokumenteResult.Dokumente
                    .Select(x => new MitarbeiterDokumentEintragViewModel
                    {
                        DokumentId = x.DokumentId,
                        Kategorie = x.Kategorie,
                        DateinameOriginal = x.DateinameOriginal,
                        Inhaltstyp = x.Inhaltstyp,
                        DateigroesseText = FormatiereDateigroesse(x.DateigroesseBytes),
                        HochgeladenAm = x.HochgeladenAm
                    })
                    .ToArray()
                : Array.Empty<MitarbeiterDokumentEintragViewModel>(),
            Navigation = await LadeNavigationAsync(mitarbeiter.UserId, mitarbeiter.Benutzername, "Details", cancellationToken)
        };

        if (!dokumenteResult.IsSuccess && !string.IsNullOrWhiteSpace(dokumenteResult.ErrorMessage))
        {
            TempData[FlashKeys.Error] = dokumenteResult.ErrorMessage;
        }

        return BereichsView("Details", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Bearbeiten(string userId, CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _readItwMitarbeiterprofilDetailService.ExecuteAsync(userId, cancellationToken);
        if (!result.IsSuccess || result.Profil is null)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Das Profil konnte nicht geladen werden.";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = ErzeugeBearbeitenViewModel(result);
        viewModel.Navigation = await LadeNavigationAsync(userId, viewModel.Benutzername, "Qualifikationen", cancellationToken);

        return BereichsView("Bearbeiten", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bearbeiten(
    ItwMitarbeiterprofilBearbeitenViewModel viewModel,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (!viewModel.HauptqualifikationId.HasValue || viewModel.HauptqualifikationId.Value == Guid.Empty)
        {
            ModelState.AddModelError(
                nameof(viewModel.HauptqualifikationId),
                "Bitte eine Hauptqualifikation auswählen.");
        }

        if (!ModelState.IsValid)
        {
            await BefuelleBearbeitenViewModelAsync(viewModel, cancellationToken);
            viewModel.Navigation = await LadeNavigationAsync(viewModel.UserId, viewModel.Benutzername, "Qualifikationen", cancellationToken);
            return BereichsView("Bearbeiten", viewModel);
        }

        var result = await _saveItwMitarbeiterprofilService.ExecuteAsync(
            new SaveItwMitarbeiterprofilCommand(
                viewModel.UserId,
                viewModel.HauptqualifikationId!.Value,
                viewModel.ZusatzqualifikationIds),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Das Profil konnte nicht gespeichert werden.");
            await BefuelleBearbeitenViewModelAsync(viewModel, cancellationToken);
            viewModel.Navigation = await LadeNavigationAsync(viewModel.UserId, viewModel.Benutzername, "Qualifikationen", cancellationToken);
            return BereichsView("Bearbeiten", viewModel);
        }

        TempData[FlashKeys.Success] = "Das Mitarbeiterprofil wurde erfolgreich gespeichert.";
        return RedirectToAction(nameof(Details), new { userId = viewModel.UserId });
    }

    [HttpGet]
    public async Task<IActionResult> Stammdaten(string userId, CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await _readAllgemeinesMitarbeiterprofilDetailService.ExecuteAsync(userId, cancellationToken);
        if (!result.IsSuccess || result.Profil is null)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Die Stammdaten konnten nicht geladen werden.";
            return RedirectToAction(nameof(Index));
        }

        var profil = result.Profil;

        var viewModel = new MitarbeiterStammdatenBearbeitenViewModel
        {
            Titel = "Stammdaten",
            Beschreibung = "Pflege der allgemeinen Mitarbeiter-Stammdaten getrennt vom zentralen Benutzerkonto und den ITW-Qualifikationen.",
            UserId = profil.UserId,
            ProfilId = profil.ProfilId,
            Benutzername = profil.Benutzername,
            Email = profil.Email,
            IstGesperrt = profil.IstGesperrt,
            Vorname = profil.Vorname,
            Nachname = profil.Nachname,
            DisplayName = profil.DisplayName,
            Beschaeftigungsart = profil.Beschaeftigungsart,
            Telefonnummer = profil.Telefonnummer,
            Strasse = profil.Strasse,
            Hausnummer = profil.Hausnummer,
            Postleitzahl = profil.Postleitzahl,
            Ort = profil.Ort,
            LetzteAktualisierung = profil.AktualisiertAm,
            Navigation = await LadeNavigationAsync(profil.UserId, profil.Benutzername, "Stammdaten", cancellationToken)
        };

        return BereichsView("Stammdaten", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Stammdaten(
    MitarbeiterStammdatenBearbeitenViewModel viewModel,
    CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        viewModel.DisplayName = $"{viewModel.Vorname} {viewModel.Nachname}".Trim();

        if (!ModelState.IsValid)
        {
            viewModel.Navigation = await LadeNavigationAsync(viewModel.UserId, viewModel.Benutzername, "Stammdaten", cancellationToken);
            return BereichsView("Stammdaten", viewModel);
        }

        var result = await _saveAllgemeinesMitarbeiterprofilService.ExecuteAsync(
            new SaveAllgemeinesMitarbeiterprofilCommand(
                viewModel.UserId,
                viewModel.Vorname,
                viewModel.Nachname,
                viewModel.Beschaeftigungsart,
                LeereAlsNull(viewModel.Telefonnummer),
                LeereAlsNull(viewModel.Strasse),
                LeereAlsNull(viewModel.Hausnummer),
                LeereAlsNull(viewModel.Postleitzahl),
                LeereAlsNull(viewModel.Ort)),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Die Stammdaten konnten nicht gespeichert werden.");
            viewModel.Navigation = await LadeNavigationAsync(viewModel.UserId, viewModel.Benutzername, "Stammdaten", cancellationToken);
            return BereichsView("Stammdaten", viewModel);
        }

        TempData[FlashKeys.Success] = "Die allgemeinen Mitarbeiter-Stammdaten wurden erfolgreich gespeichert.";
        return RedirectToAction(nameof(Details), new { userId = viewModel.UserId });
    }

    [HttpGet]
    public async Task<IActionResult> Dokumente(
        string userId,
        [FromServices] ITW.Application.Personnel.Documents.ReadMitarbeiterDokumenteService readMitarbeiterDokumenteService,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var viewModel = await LadeDokumenteViewModelAsync(
            userId,
            readMitarbeiterDokumenteService,
            cancellationToken);

        return BereichsView("Dokumente", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DokumentHochladen(
        ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter.MitarbeiterDokumenteViewModel viewModel,
        [FromServices] ITW.Application.Personnel.Documents.ReadMitarbeiterDokumenteService readMitarbeiterDokumenteService,
        [FromServices] ITW.Application.Personnel.Documents.UploadMitarbeiterDokumentService uploadMitarbeiterDokumentService,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        if (string.IsNullOrWhiteSpace(viewModel.Kategorie))
        {
            ModelState.AddModelError(nameof(viewModel.Kategorie), "Bitte eine Dokumentkategorie auswählen.");
        }

        if (viewModel.Datei is null || viewModel.Datei.Length == 0)
        {
            ModelState.AddModelError(nameof(viewModel.Datei), "Bitte eine Datei auswählen.");
        }

        if (!ModelState.IsValid)
        {
            var invalidModel = await LadeDokumenteViewModelAsync(
                viewModel.UserId,
                readMitarbeiterDokumenteService,
                cancellationToken,
                viewModel.Kategorie);

            return BereichsView("Dokumente", invalidModel);
        }

        byte[] dateiinhalt;

        await using (var memoryStream = new System.IO.MemoryStream())
        {
            await viewModel.Datei!.CopyToAsync(memoryStream, cancellationToken);
            dateiinhalt = memoryStream.ToArray();
        }

        var result = await uploadMitarbeiterDokumentService.ExecuteAsync(
            new ITW.Application.Personnel.Documents.UploadMitarbeiterDokumentCommand(
                viewModel.UserId,
                zugriff.CurrentUser!.UserId,
                viewModel.Kategorie,
                viewModel.Datei!.FileName,
                dateiinhalt),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Das Dokument konnte nicht hochgeladen werden.");

            var invalidModel = await LadeDokumenteViewModelAsync(
                viewModel.UserId,
                readMitarbeiterDokumenteService,
                cancellationToken,
                viewModel.Kategorie);

            return BereichsView("Dokumente", invalidModel);
        }

        TempData[FlashKeys.Success] = "Das Dokument wurde erfolgreich hochgeladen.";
        return RedirectToAction(nameof(Dokumente), new { userId = viewModel.UserId });
    }

    [HttpGet]
    public async Task<IActionResult> DokumentHerunterladen(
        string userId,
        Guid dokumentId,
        [FromServices] ITW.Application.Personnel.Documents.DownloadMitarbeiterDokumentService downloadMitarbeiterDokumentService,
        CancellationToken cancellationToken)
    {
        var zugriff = await PruefePersonalzugriffAsync(cancellationToken);
        if (zugriff.EarlyResult is not null)
        {
            return zugriff.EarlyResult;
        }

        var result = await downloadMitarbeiterDokumentService.ExecuteAsync(
            new ITW.Application.Personnel.Documents.DownloadMitarbeiterDokumentQuery(
                userId,
                dokumentId),
            cancellationToken);

        if (!result.IsSuccess || result.Dateiinhalt is null)
        {
            TempData[FlashKeys.Error] = result.ErrorMessage ?? "Das Dokument konnte nicht geladen werden.";
            return RedirectToAction(nameof(Dokumente), new { userId });
        }

        return File(
            result.Dateiinhalt,
            result.Inhaltstyp,
            result.DateinameOriginal);
    }

    private async Task BefuelleBearbeitenViewModelAsync(
        ItwMitarbeiterprofilBearbeitenViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var result = await _readItwMitarbeiterprofilDetailService.ExecuteAsync(viewModel.UserId, cancellationToken);
        if (!result.IsSuccess || result.Profil is null)
        {
            return;
        }

        viewModel.Titel = "Qualifikationen";
        viewModel.Beschreibung = "Pflege der fachlichen Qualifikationen getrennt vom zentralen Benutzerkonto.";
        viewModel.ProfilId ??= result.Profil.ProfilId;
        viewModel.Benutzername = result.Profil.Benutzername;
        viewModel.Email = result.Profil.Email;
        viewModel.IstGesperrt = result.Profil.IstGesperrt;
        viewModel.LetzteAktualisierung ??= result.Profil.AktualisiertAm;

        if (viewModel.ZusatzqualifikationIds.Count == 0)
        {
            viewModel.ZusatzqualifikationIds = result.Profil.ZusatzqualifikationIds.ToList();
        }

        if ((!viewModel.HauptqualifikationId.HasValue || viewModel.HauptqualifikationId.Value == Guid.Empty)
            && result.Profil.HauptqualifikationId.HasValue)
        {
            viewModel.HauptqualifikationId = result.Profil.HauptqualifikationId.Value;
        }

        viewModel.HauptqualifikationsOptionen = result.VerfuegbareHauptqualifikationen
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Bezeichnung,
                Selected = viewModel.HauptqualifikationId.HasValue && viewModel.HauptqualifikationId.Value == x.Id
            })
            .ToArray();

        viewModel.ZusatzqualifikationsOptionen = result.VerfuegbareZusatzqualifikationen
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Bezeichnung,
                Selected = viewModel.ZusatzqualifikationIds.Contains(x.Id)
            })
            .ToArray();
    }

    private static ItwMitarbeiterprofilBearbeitenViewModel ErzeugeBearbeitenViewModel(
        ReadItwMitarbeiterprofilDetailResult result)
    {
        var profil = result.Profil!;

        return new ItwMitarbeiterprofilBearbeitenViewModel
        {
            Titel = "Qualifikationen",
            Beschreibung = "Pflege der fachlichen Qualifikationen getrennt vom zentralen Benutzerkonto.",
            UserId = profil.UserId,
            ProfilId = profil.ProfilId,
            Benutzername = profil.Benutzername,
            Email = profil.Email,
            IstGesperrt = profil.IstGesperrt,
            HauptqualifikationId = profil.HauptqualifikationId,
            ZusatzqualifikationIds = profil.ZusatzqualifikationIds.ToList(),
            LetzteAktualisierung = profil.AktualisiertAm,
            HauptqualifikationsOptionen = result.VerfuegbareHauptqualifikationen
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Bezeichnung,
                    Selected = profil.HauptqualifikationId.HasValue && profil.HauptqualifikationId.Value == x.Id
                })
                .ToArray(),
            ZusatzqualifikationsOptionen = result.VerfuegbareZusatzqualifikationen
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Bezeichnung,
                    Selected = profil.ZusatzqualifikationIds.Contains(x.Id)
                })
                .ToArray()
        };
    }

    private IActionResult BereichsView(object model)
    {
        return BereichsView("Index", model);
    }

    private IActionResult BereichsView(string viewName, object model)
    {
        ViewData["AreaLayoutPath"] = AreaLayoutPath;
        return View($"~/Areas/Intensivtransport/Views/Personal/{viewName}.cshtml", model);
    }

    private async Task<PersonalzugriffResult> PruefePersonalzugriffAsync(CancellationToken cancellationToken)
    {
        var result = await HoleAktuellenBenutzerkontextAsync(cancellationToken);
        if (!result.IsSuccess || result.CurrentUser is null)
        {
            return PersonalzugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        var aktuellerBenutzer = result.CurrentUser;

        if (aktuellerBenutzer.Bereich != Bereich)
        {
            return PersonalzugriffResult.MitErgebnis(
                ErzeugeBereichsWeiterleitung(
                    aktuellerBenutzer.Bereich,
                    "Dashboard",
                    "Index",
                    "ITW-Personal",
                    BereichsRoutingHelper.GetBereichsname(Bereich),
                    "Die Personalverwaltung steht nur im Bereich Intensivtransport zur Verfügung. Ihr zuständiger Bereich wird jetzt geöffnet."));
        }

        if (aktuellerBenutzer.Rolle != BereichsrolleCode.Wachleiter)
        {
            return PersonalzugriffResult.MitErgebnis(RedirectToKeinZugriff());
        }

        return PersonalzugriffResult.MitBenutzer(aktuellerBenutzer);
    }

    private static string? LeereAlsNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private sealed record PersonalzugriffResult(CurrentUserContext? CurrentUser, IActionResult? EarlyResult)
    {
        public static PersonalzugriffResult MitBenutzer(CurrentUserContext currentUser)
            => new(currentUser, null);

        public static PersonalzugriffResult MitErgebnis(IActionResult earlyResult)
            => new(null, earlyResult);
    }

    private async Task<MitarbeiterDetailNavigationViewModel> LadeNavigationAsync(
    string userId,
    string benutzernameFallback,
    string aktiveSeite,
    CancellationToken cancellationToken)
    {
        var result = await _readItwMitarbeiterDetailUebersichtService.ExecuteAsync(userId, cancellationToken);
        if (!result.IsSuccess || result.Mitarbeiter is null)
        {
            return new MitarbeiterDetailNavigationViewModel
            {
                UserId = userId,
                Benutzername = benutzernameFallback,
                AnzeigeName = string.Empty,
                Hauptqualifikation = string.Empty,
                AktiveSeite = aktiveSeite
            };
        }

        return ErzeugeNavigation(result.Mitarbeiter, aktiveSeite);
    }

    private static MitarbeiterDetailNavigationViewModel ErzeugeNavigation(
    ItwMitarbeiterDetailUebersichtDto mitarbeiter,
    string aktiveSeite)
    {
        return new MitarbeiterDetailNavigationViewModel
        {
            UserId = mitarbeiter.UserId,
            Benutzername = mitarbeiter.Benutzername,
            AnzeigeName = BestimmeAnzeigeName(mitarbeiter),
            Hauptqualifikation = BestimmeHauptqualifikation(mitarbeiter),
            AktiveSeite = aktiveSeite
        };
    }

    private static string BestimmeAnzeigeName(ItwMitarbeiterDetailUebersichtDto mitarbeiter)
    {
        if (!mitarbeiter.HatStammdatenprofil)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(mitarbeiter.AnzeigeName)
            ? string.Empty
            : mitarbeiter.AnzeigeName.Trim();
    }

    private static string BestimmeHauptqualifikation(ItwMitarbeiterDetailUebersichtDto mitarbeiter)
    {
        if (!mitarbeiter.HatItwProfil)
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(mitarbeiter.Hauptqualifikation))
        {
            return string.Empty;
        }

        if (string.Equals(mitarbeiter.Hauptqualifikation, "Noch kein Profil", StringComparison.OrdinalIgnoreCase)
            || string.Equals(mitarbeiter.Hauptqualifikation, "Keine Hauptqualifikation", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        return mitarbeiter.Hauptqualifikation.Trim();
    }
       
    private async Task<ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter.MitarbeiterDokumenteViewModel> LadeDokumenteViewModelAsync(
        string userId,
        ITW.Application.Personnel.Documents.ReadMitarbeiterDokumenteService readMitarbeiterDokumenteService,
        CancellationToken cancellationToken,
        string? ausgewaehlteKategorie = null)
    {
        var result = await readMitarbeiterDokumenteService.ExecuteAsync(
            userId,
            cancellationToken);

        return new ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter.MitarbeiterDokumenteViewModel
        {
            Titel = "Dokumente",
            Beschreibung = "Upload und Download von mitarbeiterbezogenen Dokumenten im Bereich Intensivtransport.",
            UserId = userId,
            Kategorie = string.IsNullOrWhiteSpace(ausgewaehlteKategorie) ? string.Empty : ausgewaehlteKategorie.Trim(),
            IsSuccess = result.IsSuccess,
            ErrorMessage = result.ErrorMessage,
            KategorieOptionen = BaueDokumentKategorieOptionen(ausgewaehlteKategorie),
            Dokumente = result.Dokumente
                .Select(x => new ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter.MitarbeiterDokumentEintragViewModel
                {
                    DokumentId = x.DokumentId,
                    Kategorie = x.Kategorie,
                    DateinameOriginal = x.DateinameOriginal,
                    Inhaltstyp = x.Inhaltstyp,
                    DateigroesseText = FormatiereDateigroesse(x.DateigroesseBytes),
                    HochgeladenAm = x.HochgeladenAm
                })
                .ToArray(),
            Navigation = await LadeNavigationAsync(userId, string.Empty, "Dokumente", cancellationToken)
        };
    }

    private static string FormatiereDateigroesse(long dateigroesseBytes)
    {
        const double kb = 1024d;
        const double mb = 1024d * 1024d;

        if (dateigroesseBytes >= mb)
        {
            return $"{dateigroesseBytes / mb:0.##} MB";
        }

        if (dateigroesseBytes >= kb)
        {
            return $"{dateigroesseBytes / kb:0.##} KB";
        }

        return $"{dateigroesseBytes} Bytes";
    }
    
    private static IReadOnlyList<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> BaueDokumentKategorieOptionen(
        string? ausgewaehlteKategorie)
    {
        return ITW.Application.Personnel.Documents.MitarbeiterDokumentKategorien.Alle
            .Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = x,
                Text = x,
                Selected = string.Equals(x, ausgewaehlteKategorie?.Trim(), StringComparison.OrdinalIgnoreCase)
            })
            .ToArray();
    }

    private static string FormatiereBeschaeftigungsart(ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart beschaeftigungsart)
    {
        return beschaeftigungsart switch
        {
            ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Festangestellt => "Festangestellt",
            ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Freelancer => "Freelancer",
            ITW.Domain.Personnel.Enums.MitarbeiterBeschaeftigungsart.Honorarkraft => "Honorarkraft",
            _ => "Unbekannt"
        };
    }
}