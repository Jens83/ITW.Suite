using ITW.Domain.Organisation.Entities;
using ITW.Domain.Organisation.Enums;

namespace ITW.Application.Abstractions.Persistence;

public interface IBenutzerBereichszuordnungRepository
{
    Task<BenutzerBereichszuordnung?> GetAktivePrimaereZuordnungAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BenutzerBereichszuordnung>> GetAktivePrimaereZuordnungenByBereichAsync(
        Organisationsbereich bereich,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BenutzerBereichszuordnung zuordnung,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}