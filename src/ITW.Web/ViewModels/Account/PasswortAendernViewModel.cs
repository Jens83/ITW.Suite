// Datei: src/ITW.Web/ViewModels/Account/PasswortAendernViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ITW.Web.ViewModels.Account;

public sealed class PasswortAendernViewModel
{
    [Required(ErrorMessage = "Bitte das aktuelle Passwort eingeben.")]
    [DataType(DataType.Password)]
    [Display(Name = "Aktuelles Passwort")]
    public string AktuellesPasswort { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte ein neues Passwort eingeben.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Das neue Passwort muss mindestens 8 Zeichen lang sein.")]
    [Display(Name = "Neues Passwort")]
    public string NeuesPasswort { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte das neue Passwort bestätigen.")]
    [DataType(DataType.Password)]
    [Compare(nameof(NeuesPasswort), ErrorMessage = "Die Passwortbestätigung stimmt nicht überein.")]
    [Display(Name = "Neues Passwort bestätigen")]
    public string NeuesPasswortBestaetigung { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    public bool IstErzwungen { get; set; }
}