// Datei: src/ITW.Infrastructure/Persistence/Repositories/MitarbeiterDokumentRepository.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Personnel.Entities;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class MitarbeiterDokumentRepository : IMitarbeiterDokumentRepository
{
    private readonly PlatformDbContext _dbContext;

    public MitarbeiterDokumentRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MitarbeiterDokument>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Array.Empty<MitarbeiterDokument>();
        }

        return await _dbContext.Set<MitarbeiterDokument>()
            .AsNoTracking()
            .Where(x => x.UserId == userId.Trim())
            .OrderByDescending(x => x.HochgeladenAm)
            .ThenBy(x => x.DateinameOriginal)
            .ToListAsync(cancellationToken);
    }

    public Task<MitarbeiterDokument?> GetByIdAsync(
        Guid dokumentId,
        CancellationToken cancellationToken = default)
    {
        if (dokumentId == Guid.Empty)
        {
            return Task.FromResult<MitarbeiterDokument?>(null);
        }

        return _dbContext.Set<MitarbeiterDokument>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dokumentId, cancellationToken);
    }

    public async Task AddAsync(
        MitarbeiterDokument dokument,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dokument);

        await _dbContext.Set<MitarbeiterDokument>().AddAsync(dokument, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}