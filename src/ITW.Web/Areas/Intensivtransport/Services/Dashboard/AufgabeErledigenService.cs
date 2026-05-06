using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Areas.Intensivtransport.Services.Dashboard;

public sealed class AufgabeErledigenService
{
    private readonly IAufgabeRepository _aufgaben;

    public AufgabeErledigenService(IAufgabeRepository aufgaben)
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

        aufgabe.AlsErledigtMarkieren(DateTimeOffset.UtcNow);
        await _aufgaben.SaveChangesAsync(cancellationToken);
        return true;
    }
}
