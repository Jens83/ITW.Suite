using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class FreelancerMonatswunschRepository : IFreelancerMonatswunschRepository
{
    private readonly PlatformDbContext _dbContext;

    public FreelancerMonatswunschRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<FreelancerMonatswunsch?> GetAsync(
        Guid dienstplanPeriodeId,
        string userId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.FreelancerMonatswuensche
            .FirstOrDefaultAsync(
                x => x.DienstplanPeriodeId == dienstplanPeriodeId
                     && x.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<FreelancerMonatswunsch>> GetAlleFuerPeriodeAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default)
    {
        if (dienstplanPeriodeId == Guid.Empty)
        {
            return Array.Empty<FreelancerMonatswunsch>();
        }

        return await _dbContext.FreelancerMonatswuensche
            .AsNoTracking()
            .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId)
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        FreelancerMonatswunsch eintrag,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eintrag);

        await _dbContext.FreelancerMonatswuensche.AddAsync(eintrag, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}