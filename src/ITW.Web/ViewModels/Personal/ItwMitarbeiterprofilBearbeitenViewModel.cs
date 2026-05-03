using System.ComponentModel.DataAnnotations;
using ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.ViewModels.Personal;

public sealed class ItwMitarbeiterprofilBearbeitenViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    [Required(ErrorMessage = "Die UserId ist erforderlich.")]
    public string UserId { get; set; } = string.Empty;

    public Guid? ProfilId { get; set; }

    public string Benutzername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IstGesperrt { get; set; }

    [Required(ErrorMessage = "Bitte eine Hauptqualifikation auswählen.")]
    [Display(Name = "Hauptqualifikation")]
    public Guid? HauptqualifikationId { get; set; }

    [Display(Name = "Zusatzqualifikationen")]
    public List<Guid> ZusatzqualifikationIds { get; set; } = new();

    public DateTimeOffset? LetzteAktualisierung { get; set; }

    public IReadOnlyList<SelectListItem> HauptqualifikationsOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<SelectListItem> ZusatzqualifikationsOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public MitarbeiterDetailNavigationViewModel Navigation { get; set; } = new();
}