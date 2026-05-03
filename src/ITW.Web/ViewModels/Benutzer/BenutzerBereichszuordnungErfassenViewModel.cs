using System.ComponentModel.DataAnnotations;
using ITW.Application.Organisation.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.ViewModels.Benutzer;

public sealed class BenutzerBereichszuordnungErfassenViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public string BereichName { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte wählen Sie ein Benutzerkonto aus.")]
    [Display(Name = "Benutzerkonto")]
    public string UserId { get; set; } = string.Empty;

    [Display(Name = "Rolle")]
    public BereichsrolleCode Rolle { get; set; }

    [Display(Name = "Führungsverantwortung")]
    public FuehrungsverantwortungCode Fuehrungsverantwortung { get; set; } = FuehrungsverantwortungCode.Keine;

    [Display(Name = "Bestehende primäre Zuordnung ersetzen")]
    public bool BestehendePrimaereZuordnungErsetzen { get; set; }

    public bool HatVerfuegbareBenutzerkonten { get; set; }

    public IReadOnlyList<SelectListItem> BenutzerkontoOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> RollenOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> FuehrungsverantwortungsOptionen { get; set; }
        = Array.Empty<SelectListItem>();
}