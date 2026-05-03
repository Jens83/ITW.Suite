using System.ComponentModel.DataAnnotations;
using ITW.Application.Organisation.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.ViewModels.Benutzer;

public sealed class BenutzerBereichsrolleAendernViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public string BereichName { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Die UserId ist erforderlich.")]
    public string UserId { get; set; } = string.Empty;

    public string Benutzername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Display(Name = "Rolle")]
    public BereichsrolleCode Rolle { get; set; }

    [Display(Name = "Führungsverantwortung")]
    public FuehrungsverantwortungCode Fuehrungsverantwortung { get; set; } = FuehrungsverantwortungCode.Keine;

    public IReadOnlyList<SelectListItem> RollenOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> FuehrungsverantwortungsOptionen { get; set; }
        = Array.Empty<SelectListItem>();
}