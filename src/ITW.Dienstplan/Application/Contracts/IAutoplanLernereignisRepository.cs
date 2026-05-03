using ITW.Dienstplan.Domain.Entities;

namespace ITW.Dienstplan.Application.Contracts;

public interface IAutoplanLernereignisRepository
{
    Task AddAsync(
        AutoplanLernereignis lernereignis,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutoplanLernereignis>> GetVertretungsLernereignisseAsync(
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AutoplanLernereignis>> GetGrundbesetzungsLernereignisseAsync(
    CancellationToken cancellationToken = default);
}