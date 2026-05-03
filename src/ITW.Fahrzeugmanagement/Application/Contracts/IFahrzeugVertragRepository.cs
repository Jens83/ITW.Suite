using ITW.Fahrzeugmanagement.Domain.Entities;

namespace ITW.Fahrzeugmanagement.Application.Contracts;

public interface IFahrzeugVertragRepository
{
    Task<IReadOnlyList<FahrzeugVertrag>> GetByFahrzeugIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default);

    Task<FahrzeugVertrag?> GetByIdAsync(
        Guid vertragId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        FahrzeugVertrag vertrag,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid vertragId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}