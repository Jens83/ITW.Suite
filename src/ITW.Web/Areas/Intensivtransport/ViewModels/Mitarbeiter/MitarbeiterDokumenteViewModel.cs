// Datei: src/ITW.Web/Areas/Intensivtransport/ViewModels/Mitarbeiter/MitarbeiterDokumenteViewModel.cs
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ITW.Web.Areas.Intensivtransport.ViewModels.Mitarbeiter;

public sealed class MitarbeiterDokumenteViewModel
{
    public string Titel { get; set; } = string.Empty;

    public string Beschreibung { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public string Kategorie { get; set; } = string.Empty;

    public IFormFile? Datei { get; set; }

    public IReadOnlyList<SelectListItem> KategorieOptionen { get; set; }
        = Array.Empty<SelectListItem>();

    public IReadOnlyList<MitarbeiterDokumentEintragViewModel> Dokumente { get; set; }
        = Array.Empty<MitarbeiterDokumentEintragViewModel>();

    public MitarbeiterDetailNavigationViewModel Navigation { get; set; } = new();
}