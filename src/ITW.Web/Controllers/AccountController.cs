// Datei: src/ITW.Web/Controllers/AccountController.cs
using ITW.Application.Abstractions.Identity;
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Users.RequestPasswordReset;
using ITW.Infrastructure.Identity;
using ITW.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ITW.Web.Controllers;

public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAllgemeinesMitarbeiterprofilRepository _allgemeinesMitarbeiterprofilRepository;
    private readonly IBenutzerkontoRepository _benutzerkontoRepository;
    private readonly SubmitPasswortResetAnfrageService _submitPasswortResetAnfrageService;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IAllgemeinesMitarbeiterprofilRepository allgemeinesMitarbeiterprofilRepository,
        IBenutzerkontoRepository benutzerkontoRepository,
        SubmitPasswortResetAnfrageService submitPasswortResetAnfrageService)
    {
        _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _allgemeinesMitarbeiterprofilRepository = allgemeinesMitarbeiterprofilRepository
            ?? throw new ArgumentNullException(nameof(allgemeinesMitarbeiterprofilRepository));
        _benutzerkontoRepository = benutzerkontoRepository
            ?? throw new ArgumentNullException(nameof(benutzerkontoRepository));
        _submitPasswortResetAnfrageService = submitPasswortResetAnfrageService
            ?? throw new ArgumentNullException(nameof(submitPasswortResetAnfrageService));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await _signInManager.PasswordSignInAsync(
            viewModel.Benutzername,
            viewModel.Passwort,
            viewModel.AngemeldetBleiben,
            lockoutOnFailure: false);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(
                string.Empty,
                "Dieses Benutzerkonto ist derzeit gesperrt.");

            return View(viewModel);
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                "Die Anmeldung ist fehlgeschlagen. Bitte Benutzername und Passwort prüfen.");

            return View(viewModel);
        }

        var user = await _userManager.FindByNameAsync(viewModel.Benutzername);
        if (user is not null)
        {
            var profil = await _allgemeinesMitarbeiterprofilRepository.GetByUserIdAsync(user.Id, cancellationToken);
            if (profil is not null)
            {
                await _benutzerkontoRepository.SynchronisiereNamensClaimsAsync(
                    user.Id,
                    profil.Vorname,
                    profil.Nachname,
                    cancellationToken);

                await _signInManager.RefreshSignInAsync(user);
            }

            if (await MussPasswortAendernAsync(user))
            {
                return RedirectToAction(
                    nameof(PasswortAendern),
                    new
                    {
                        returnUrl = SanitizeReturnUrl(viewModel.ReturnUrl)
                    });
            }
        }

        if (!string.IsNullOrWhiteSpace(viewModel.ReturnUrl) &&
            Url.IsLocalUrl(viewModel.ReturnUrl))
        {
            return LocalRedirect(viewModel.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult PasswortVergessen()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new PasswortVergessenViewModel());
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasswortVergessen(
        PasswortVergessenViewModel viewModel,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var result = await _submitPasswortResetAnfrageService.ExecuteAsync(
            new SubmitPasswortResetAnfrageCommand(
                viewModel.Benutzername,
                viewModel.Vorname,
                viewModel.Nachname),
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Die Passwort-Reset-Anfrage konnte nicht erfasst werden.");

            return View(viewModel);
        }

        TempData["SuccessMessage"] = result.Bestaetigungsnachricht;
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult PasswortAendern(string? returnUrl = null)
    {
        var istErzwungen = User.HasClaim(BenutzerkontoClaimTypes.MussPasswortAendern, "true");

        return View(new PasswortAendernViewModel
        {
            ReturnUrl = SanitizeReturnUrl(returnUrl),
            IstErzwungen = istErzwungen
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasswortAendern(
        PasswortAendernViewModel viewModel,
        CancellationToken cancellationToken)
    {
        var istErzwungen = User.HasClaim(BenutzerkontoClaimTypes.MussPasswortAendern, "true");
        viewModel.IstErzwungen = istErzwungen;
        viewModel.ReturnUrl = SanitizeReturnUrl(viewModel.ReturnUrl);

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            viewModel.AktuellesPasswort,
            viewModel.NeuesPasswort);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(viewModel);
        }

        await _benutzerkontoRepository.EntfernePasswortwechselPflichtAsync(
            user.Id,
            cancellationToken);

        await _signInManager.RefreshSignInAsync(user);

        TempData["SuccessMessage"] = "Das Passwort wurde erfolgreich geändert.";

        if (!string.IsNullOrWhiteSpace(viewModel.ReturnUrl) &&
            Url.IsLocalUrl(viewModel.ReturnUrl))
        {
            return LocalRedirect(viewModel.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult KeinZugriff()
    {
        return View();
    }

    private async Task<bool> MussPasswortAendernAsync(ApplicationUser user)
    {
        var claims = await _userManager.GetClaimsAsync(user);

        return claims.Any(x =>
            string.Equals(x.Type, BenutzerkontoClaimTypes.MussPasswortAendern, StringComparison.Ordinal) &&
            string.Equals(x.Value, "true", StringComparison.OrdinalIgnoreCase));
    }

    private string? SanitizeReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : null;
    }
}