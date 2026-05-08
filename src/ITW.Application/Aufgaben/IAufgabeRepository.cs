using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Aufgaben;

public interface IAufgabeRepository
{
    Task<IReadOnlyList<Aufgabe>> GetOffeneByBereichAsync(
        OrganisationsbereichCode bereich,
        CancellationToken cancellationToken = default);

    Task<Aufgabe?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistiertOffeneSystemaufgabeAsync(
        string systemSchluessel,
        CancellationToken cancellationToken = default);

    Task<bool> ExistiertSystemaufgabeAsync(
        string systemSchluessel,
        CancellationToken cancellationToken = default);

    Task<Aufgabe?> GetBySystemSchluesselAsync(
        string systemSchluessel,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Aufgabe aufgabe,
        CancellationToken cancellationToken = default);

    void Remove(Aufgabe aufgabe);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
