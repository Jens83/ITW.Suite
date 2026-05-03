using System.ComponentModel.DataAnnotations;
using ITW.Application.Organisation.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.ViewModels.Benutzer;

public sealed class BenutzerkontoNeuAnlegenViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public string BereichName { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Der Benutzername ist erforderlich.")]
    [Display(Name = "Benutzername")]
    public string Benutzername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Die E-Mail-Adresse ist erforderlich.")]
    [EmailAddress(ErrorMessage = "Bitte geben Sie eine gültige E-Mail-Adresse ein.")]
    [Display(Name = "E-Mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Das Passwort ist erforderlich.")]
    [DataType(DataType.Password)]
    [Display(Name = "Initiales Passwort")]
    public string Passwort { get; set; } = string.Empty;

    [Display(Name = "Rolle")]
    public BereichsrolleCode Rolle { get; set; }

    [Display(Name = "Führungsverantwortung")]
    public FuehrungsverantwortungCode Fuehrungsverantwortung { get; set; } = FuehrungsverantwortungCode.Keine;

    public IReadOnlyList<SelectListItem> RollenOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> FuehrungsverantwortungsOptionen { get; set; }
        = Array.Empty<SelectListItem>();
}