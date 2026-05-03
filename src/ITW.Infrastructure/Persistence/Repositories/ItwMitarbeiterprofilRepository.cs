using ITW.Application.Abstractions.Persistence;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Qualifications;
using ITW.Infrastructure.Persistence.DbContexts;
using ITW.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class ItwMitarbeiterprofilRepository : IItwMitarbeiterprofilRepository
{
    private readonly PlatformDbContext _dbContext;

    public ItwMitarbeiterprofilRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task EnsureStandardqualifikationenAsync(CancellationToken cancellationToken = default)
    {
        var vorhandeneCodes = await _dbContext.ItwQualifikationen
            .AsNoTracking()
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        var vorhandeneCodeSet = new HashSet<string>(
            vorhandeneCodes,
            StringComparer.OrdinalIgnoreCase);

        var fehlendeQualifikationen = ItwQualifikationSeedData.GetStandardqualifikationen()
            .Where(x => !vorhandeneCodeSet.Contains(x.Code))
            .ToArray();

        if (fehlendeQualifikationen.Length == 0)
        {
            return;
        }

        await _dbContext.ItwQualifikationen.AddRangeAsync(
            fehlendeQualifikationen,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ItwQualifikation>> GetAktiveQualifikationenAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ItwQualifikationen
            .AsNoTracking()
            .Where(x => x.IsAktiv)
            .OrderBy(x => x.Sortierung)
            .ThenBy(x => x.Bezeichnung)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ItwMitarbeiterprofil>> GetByUserIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var ids = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ids.Length == 0)
        {
            return Array.Empty<ItwMitarbeiterprofil>();
        }

        return await _dbContext.ItwMitarbeiterprofile
            .AsNoTracking()
            .Include(x => x.Qualifikationen)
            .Where(x => ids.Contains(x.UserId))
            .ToListAsync(cancellationToken);
    }

    public Task<ItwMitarbeiterprofil?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult<ItwMitarbeiterprofil?>(null);
        }

        return _dbContext.ItwMitarbeiterprofile
            .AsNoTracking()
            .Include(x => x.Qualifikationen)
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task UpsertQualifikationenAsync(
        string userId,
        Guid hauptqualifikationId,
        IReadOnlyCollection<Guid> zusatzqualifikationIds,
        DateTimeOffset aktualisiertAm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        if (hauptqualifikationId == Guid.Empty)
        {
            throw new ArgumentException("Die Hauptqualifikation ist erforderlich.", nameof(hauptqualifikationId));
        }

        var bereinigteZusatzqualifikationen = zusatzqualifikationIds
            .Where(x => x != Guid.Empty && x != hauptqualifikationId)
            .Distinct()
            .ToArray();

        var profil = await _dbContext.ItwMitarbeiterprofile
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (profil is null)
        {
            profil = new ItwMitarbeiterprofil(Guid.NewGuid(), userId, aktualisiertAm);
            await _dbContext.ItwMitarbeiterprofile.AddAsync(profil, cancellationToken);
        }
        else
        {
            profil.MarkiereAktualisiert(aktualisiertAm);

            var bestehendeQualifikationen = await _dbContext.Set<ItwMitarbeiterQualifikation>()
                .Where(x => x.ItwMitarbeiterprofilId == profil.Id)
                .ToListAsync(cancellationToken);

            if (bestehendeQualifikationen.Count > 0)
            {
                _dbContext.Set<ItwMitarbeiterQualifikation>().RemoveRange(bestehendeQualifikationen);
            }
        }

        var neueQualifikationen = new List<ItwMitarbeiterQualifikation>
        {
            new(
                Guid.NewGuid(),
                profil.Id,
                hauptqualifikationId,
                true,
                aktualisiertAm)
        };

        foreach (var zusatzqualifikationId in bereinigteZusatzqualifikationen)
        {
            neueQualifikationen.Add(new ItwMitarbeiterQualifikation(
                Guid.NewGuid(),
                profil.Id,
                zusatzqualifikationId,
                false,
                aktualisiertAm));
        }

        await _dbContext.Set<ItwMitarbeiterQualifikation>().AddRangeAsync(
            neueQualifikationen,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}