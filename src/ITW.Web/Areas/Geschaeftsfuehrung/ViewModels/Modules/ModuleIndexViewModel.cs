namespace ITW.Web.Areas.Geschaeftsfuehrung.ViewModels.Modules;

public sealed class ModuleIndexViewModel
{
    public bool IsSuccess { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyList<ModulEmpfaengerSpalteViewModel> Empfaenger { get; init; }
        = Array.Empty<ModulEmpfaengerSpalteViewModel>();

    public IReadOnlyList<ModulMatrixZeileViewModel> Module { get; init; }
        = Array.Empty<ModulMatrixZeileViewModel>();
}