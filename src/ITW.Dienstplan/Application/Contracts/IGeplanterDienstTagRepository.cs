using ITW.Dienstplan.Domain.Entities;

namespace ITW.Dienstplan.Application.Contracts;

public interface IGeplanterDienstTagRepository
{
    Task<GeplanterDienstTag?> GetAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeplanterDienstTag>> GetAlleFuerPeriodeAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        GeplanterDienstTag geplanterDienstTag,
        CancellationToken cancellationToken = default);

    void Remove(GeplanterDienstTag geplanterDienstTag);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}