using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Contracts;

public interface IEinsatzVerbrauchRepository
{
    Task<IReadOnlyList<EinsatzVerbrauch>> GetByFahrzeugAsync(
        Lagerort fahrzeug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EinsatzVerbrauch>> GetByDatumBereichAsync(
        DateOnly von,
        DateOnly bis,
        CancellationToken cancellationToken = default);

    Task<EinsatzVerbrauch?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        EinsatzVerbrauch verbrauch,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
