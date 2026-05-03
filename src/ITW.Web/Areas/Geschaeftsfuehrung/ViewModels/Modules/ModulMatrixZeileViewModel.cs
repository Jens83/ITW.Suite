using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Areas.Geschaeftsfuehrung.ViewModels.Modules;

public sealed class ModulMatrixZeileViewModel
{
    public ModulCode Modul { get; init; }

    public string Anzeigename { get; init; } = string.Empty;

    public IReadOnlyList<ModulZuweisungZelleViewModel> Zellen { get; init; }
        = Array.Empty<ModulZuweisungZelleViewModel>();
}