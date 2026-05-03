using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Areas.Geschaeftsfuehrung.ViewModels.Modules;

public sealed class ModulZuweisungZelleViewModel
{
    public ModulCode Modul { get; init; }

    public OrganisationsbereichCode Bereich { get; init; }

    public BereichsrolleCode Rolle { get; init; }

    public bool IstAktiv { get; init; }
}