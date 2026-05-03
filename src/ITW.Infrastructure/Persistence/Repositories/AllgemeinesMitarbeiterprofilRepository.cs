using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class AllgemeinesMitarbeiterprofilRepository : IAllgemeinesMitarbeiterprofilRepository
{
    private readonly PlatformDbContext _dbContext;

    public AllgemeinesMitarbeiterprofilRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<AllgemeinesMitarbeiterprofil?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult<AllgemeinesMitarbeiterprofil?>(null);
        }

        return _dbContext.Set<AllgemeinesMitarbeiterprofil>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<AllgemeinesMitarbeiterprofil>> GetByUserIdsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return Array.Empty<AllgemeinesMitarbeiterprofil>();
        }

        return await _dbContext.Set<AllgemeinesMitarbeiterprofil>()
            .AsNoTracking()
            .Where(x => userIds.Contains(x.UserId))
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(
        string userId,
        string vorname,
        string nachname,
        MitarbeiterBeschaeftigungsart beschaeftigungsart,
        string? telefonnummer,
        string? strasse,
        string? hausnummer,
        string? postleitzahl,
        string? ort,
        DateTimeOffset aktualisiertAm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        var profil = await _dbContext.Set<AllgemeinesMitarbeiterprofil>()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (profil is null)
        {
            profil = new AllgemeinesMitarbeiterprofil(Guid.NewGuid(), userId, aktualisiertAm);
            profil.AktualisiereStammdaten(
                vorname,
                nachname,
                beschaeftigungsart,
                telefonnummer,
                strasse,
                hausnummer,
                postleitzahl,
                ort,
                aktualisiertAm);

            await _dbContext.Set<AllgemeinesMitarbeiterprofil>().AddAsync(profil, cancellationToken);
            return;
        }

        profil.AktualisiereStammdaten(
            vorname,
            nachname,
            beschaeftigungsart,
            telefonnummer,
            strasse,
            hausnummer,
            postleitzahl,
            ort,
            aktualisiertAm);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}