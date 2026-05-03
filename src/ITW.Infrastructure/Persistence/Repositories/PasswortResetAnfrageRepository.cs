// Datei: src/ITW.Infrastructure/Persistence/Repositories/PasswortResetAnfrageRepository.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Security.Entities;
using ITW.Domain.Security.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class PasswortResetAnfrageRepository : IPasswortResetAnfrageRepository
{
    private readonly PlatformDbContext _dbContext;

    public PasswortResetAnfrageRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<PasswortResetAnfrage?> GetOffeneAnfrageFuerUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        return await _dbContext.PasswortResetAnfragen
            .FirstOrDefaultAsync(
                x => x.UserId == userId.Trim() &&
                     x.Status == PasswortResetAnfrageStatus.Offen,
                cancellationToken);
    }

    public async Task<PasswortResetAnfrage?> GetOffeneAnfrageByIdAsync(
        Guid anfrageId,
        CancellationToken cancellationToken = default)
    {
        if (anfrageId == Guid.Empty)
        {
            return null;
        }

        return await _dbContext.PasswortResetAnfragen
            .FirstOrDefaultAsync(
                x => x.Id == anfrageId &&
                     x.Status == PasswortResetAnfrageStatus.Offen,
                cancellationToken);
    }

    public async Task<IReadOnlyList<PasswortResetAnfrage>> GetOffeneAnfragenByBereichAsync(
        Organisationsbereich bereich,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.PasswortResetAnfragen
            .AsNoTracking()
            .Where(x => x.Bereich == bereich &&
                        x.Status == PasswortResetAnfrageStatus.Offen)
            .OrderBy(x => x.AngefordertAm)
            .ThenBy(x => x.Nachname)
            .ThenBy(x => x.Vorname)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountOffeneAnfragenByBereichAsync(
        Organisationsbereich bereich,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.PasswortResetAnfragen
            .AsNoTracking()
            .CountAsync(
                x => x.Bereich == bereich &&
                     x.Status == PasswortResetAnfrageStatus.Offen,
                cancellationToken);
    }

    public async Task AddAsync(
        PasswortResetAnfrage anfrage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(anfrage);

        await _dbContext.PasswortResetAnfragen.AddAsync(anfrage, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}