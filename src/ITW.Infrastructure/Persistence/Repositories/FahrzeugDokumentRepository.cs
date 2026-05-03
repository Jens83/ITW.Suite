using ITW.Fahrzeugmanagement.Application.Contracts;
using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class FahrzeugDokumentRepository : IFahrzeugDokumentRepository
{
    private readonly PlatformDbContext _dbContext;

    public FahrzeugDokumentRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<FahrzeugDokument>> GetByFahrzeugIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default)
    {
        if (fahrzeugId == Guid.Empty)
        {
            return [];
        }

        return await _dbContext.FahrzeugDokumente
            .AsNoTracking()
            .Where(x => x.FahrzeugId == fahrzeugId)
            .OrderByDescending(x => x.HochgeladenAm)
            .ThenBy(x => x.Dateiname)
            .ToListAsync(cancellationToken);
    }

    public Task<FahrzeugDokument?> GetByIdAsync(
        Guid dokumentId,
        CancellationToken cancellationToken = default)
    {
        if (dokumentId == Guid.Empty)
        {
            return Task.FromResult<FahrzeugDokument?>(null);
        }

        return _dbContext.FahrzeugDokumente
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dokumentId, cancellationToken);
    }

    public async Task AddAsync(
        FahrzeugDokument dokument,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dokument);

        await _dbContext.FahrzeugDokumente.AddAsync(dokument, cancellationToken);
    }

    public async Task DeleteAsync(
        Guid dokumentId,
        CancellationToken cancellationToken = default)
    {
        if (dokumentId == Guid.Empty)
        {
            return;
        }

        var dokument = await _dbContext.FahrzeugDokumente
            .FirstOrDefaultAsync(x => x.Id == dokumentId, cancellationToken);

        if (dokument is null)
        {
            return;
        }

        _dbContext.FahrzeugDokumente.Remove(dokument);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}