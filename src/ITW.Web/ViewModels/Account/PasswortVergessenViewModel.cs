// Datei: src/ITW.Web/ViewModels/Account/PasswortVergessenViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ITW.Web.ViewModels.Account;

public sealed class PasswortVergessenViewModel
{
    [Required(ErrorMessage = "Bitte einen Benutzernamen eingeben.")]
    [Display(Name = "Benutzername")]
    public string Benutzername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte einen Vornamen eingeben.")]
    [Display(Name = "Vorname")]
    public string Vorname { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte einen Nachnamen eingeben.")]
    [Display(Name = "Nachname")]
    public string Nachname { get; set; } = string.Empty;
}