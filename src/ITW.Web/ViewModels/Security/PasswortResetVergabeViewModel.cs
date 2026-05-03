// Datei: src/ITW.Web/ViewModels/Security/PasswortResetVergabeViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ITW.Web.ViewModels.Security;

public sealed class PasswortResetVergabeViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public string BereichName { get; set; } = string.Empty;

    [Required]
    public Guid AnfrageId { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Benutzername { get; set; } = string.Empty;

    public string Vollname { get; set; } = string.Empty;

    public DateTimeOffset AngefordertAm { get; set; }

    [Required(ErrorMessage = "Bitte ein temporäres Passwort eingeben.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Das temporäre Passwort muss mindestens 8 Zeichen lang sein.")]
    [Display(Name = "Temporäres Passwort")]
    public string TemporaeresPasswort { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte das temporäre Passwort bestätigen.")]
    [DataType(DataType.Password)]
    [Compare(nameof(TemporaeresPasswort), ErrorMessage = "Die Passwortbestätigung stimmt nicht überein.")]
    [Display(Name = "Temporäres Passwort bestätigen")]
    public string TemporaeresPasswortBestaetigung { get; set; } = string.Empty;
}