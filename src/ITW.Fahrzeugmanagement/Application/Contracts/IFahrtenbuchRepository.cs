using ITW.Fahrzeugmanagement.Domain.Entities;

namespace ITW.Fahrzeugmanagement.Application.Contracts;

public interface IFahrtenbuchRepository
{
    Task<IReadOnlyList<FahrtenbuchEintrag>> GetByFahrzeugIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default);

    Task<FahrtenbuchEintrag?> GetByIdAsync(
        Guid eintragId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        FahrtenbuchEintrag eintrag,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}