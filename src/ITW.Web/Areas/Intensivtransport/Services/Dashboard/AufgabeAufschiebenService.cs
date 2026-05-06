using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Areas.Intensivtransport.Services.Dashboard;

public sealed class AufgabeAufschiebenService
{
    private readonly IAufgabeRepository _aufgaben;

    public AufgabeAufschiebenService(IAufgabeRepository aufgaben)
    {
        ArgumentNullException.ThrowIfNull(aufgaben);
        _aufgaben = aufgaben;
    }

    public async Task<bool> ExecuteAsync(
        Guid aufgabeId,
        OrganisationsbereichCode bereich,
        CancellationToken cancellationToken = default)
    {
        var aufgabe = await _aufgaben.GetByIdAsync(aufgabeId, cancellationToken);

        if (aufgabe is null || aufgabe.Bereich != bereich)
            return false;

        var heute = DateOnly.FromDateTime(DateTime.Today);
        var basis = (aufgabe.Faelligkeitsdatum.HasValue && aufgabe.Faelligkeitsdatum.Value >= heute)
            ? aufgabe.Faelligkeitsdatum.Value
            : heute;

        aufgabe.AktualisiereFaelligkeitsdatum(basis.AddDays(7));
        await _aufgaben.SaveChangesAsync(cancellationToken);
        return true;
    }
}
