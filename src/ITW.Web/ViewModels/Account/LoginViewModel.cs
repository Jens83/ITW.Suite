using System.ComponentModel.DataAnnotations;

namespace ITW.Web.ViewModels.Account;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Bitte einen Benutzernamen eingeben.")]
    [Display(Name = "Benutzername")]
    public string Benutzername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte ein Passwort eingeben.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string Passwort { get; set; } = string.Empty;

    [Display(Name = "Angemeldet bleiben")]
    public bool AngemeldetBleiben { get; set; }

    public string? ReturnUrl { get; set; }
}