using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Areas.Geschaeftsfuehrung.ViewModels.Modules;

public sealed class ModulEmpfaengerSpalteViewModel
{
    public OrganisationsbereichCode Bereich { get; init; }

    public BereichsrolleCode Rolle { get; init; }

    public string Titel { get; init; } = string.Empty;

    public string Untertitel { get; init; } = string.Empty;
}