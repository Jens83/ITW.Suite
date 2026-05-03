// Datei: src/ITW.Web/ViewModels/Security/PasswortResetAnfrageListeViewModel.cs
namespace ITW.Web.ViewModels.Security;

public sealed class PasswortResetAnfrageListeViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public string BereichName { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public IReadOnlyList<PasswortResetAnfrageEintragViewModel> Anfragen { get; set; }
        = Array.Empty<PasswortResetAnfrageEintragViewModel>();
}