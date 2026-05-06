using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;

namespace ITW.Web.Areas.Intensivtransport.Services.Dashboard;

public sealed record AufgabeErstellenCommand(
    string Titel,
    AufgabePrioritaet Prioritaet,
    DateOnly? Faelligkeitsdatum);

public sealed class AufgabeErstellenService
{
    private readonly IAufgabeRepository _aufgaben;

    public AufgabeErstellenService(IAufgabeRepository aufgaben)
    {
        ArgumentNullException.ThrowIfNull(aufgaben);
        _aufgaben = aufgaben;
    }

    public async Task<bool> ExecuteAsync(
        AufgabeErstellenCommand command,
        OrganisationsbereichCode bereich,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Titel))
            return false;

        var aufgabe = new Aufgabe(
            Guid.NewGuid(),
            bereich,
            command.Titel,
            command.Prioritaet,
            AufgabeQuelle.Manuell,
            command.Faelligkeitsdatum,
            null,
            DateTimeOffset.UtcNow);

        await _aufgaben.AddAsync(aufgabe, cancellationToken);
        await _aufgaben.SaveChangesAsync(cancellationToken);
        return true;
    }
}
