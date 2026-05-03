using ITW.Dienstplan.Domain.Entities;

namespace ITW.Dienstplan.Application.Contracts;

public interface IFreelancerMonatswunschRepository
{
    Task<FreelancerMonatswunsch?> GetAsync(
        Guid dienstplanPeriodeId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FreelancerMonatswunsch>> GetAlleFuerPeriodeAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        FreelancerMonatswunsch eintrag,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}