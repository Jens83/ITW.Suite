using System.ComponentModel.DataAnnotations;
using ITW.Domain.Personnel.Enums;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter;

public sealed class MitarbeiterStammdatenBearbeitenViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    [Required(ErrorMessage = "Die UserId ist erforderlich.")]
    public string UserId { get; set; } = string.Empty;

    public Guid? ProfilId { get; set; }

    public string Benutzername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IstGesperrt { get; set; }

    [Required(ErrorMessage = "Bitte einen Vornamen angeben.")]
    [Display(Name = "Vorname")]
    [StringLength(100)]
    public string Vorname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte einen Nachnamen angeben.")]
    [Display(Name = "Nachname")]
    [StringLength(100)]
    public string Nachname { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    [Display(Name = "Beschäftigungsart")]
    public MitarbeiterBeschaeftigungsart Beschaeftigungsart { get; set; } = MitarbeiterBeschaeftigungsart.Unbekannt;

    [Display(Name = "Telefonnummer")]
    [StringLength(50)]
    public string Telefonnummer { get; set; } = string.Empty;

    [Display(Name = "Straße")]
    [StringLength(200)]
    public string Strasse { get; set; } = string.Empty;

    [Display(Name = "Hausnummer")]
    [StringLength(20)]
    public string Hausnummer { get; set; } = string.Empty;

    [Display(Name = "PLZ")]
    [StringLength(20)]
    public string Postleitzahl { get; set; } = string.Empty;

    [Display(Name = "Ort")]
    [StringLength(100)]
    public string Ort { get; set; } = string.Empty;

    public DateTimeOffset? LetzteAktualisierung { get; set; }

    public MitarbeiterDetailNavigationViewModel Navigation { get; set; } = new();
}