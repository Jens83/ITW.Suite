using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Enums;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Qualifications;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class DienstwunschAuswertungRepository : IDienstwunschAuswertungRepository
{
    private readonly PlatformDbContext _dbContext;

    public DienstwunschAuswertungRepository(PlatformDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DienstwunschTagesstatistik>> GetTagesstatistikAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default)
    {
        if (dienstplanPeriodeId == Guid.Empty)
        {
            return Array.Empty<DienstwunschTagesstatistik>();
        }

        var wuensche = await _dbContext.DienstplanWuensche
            .AsNoTracking()
            .Where(x => x.DienstplanPeriodeId == dienstplanPeriodeId && x.WunschTyp == DienstwunschTyp.Wunsch)
            .Select(x => new
            {
                x.UserId,
                x.WunschDatum
            })
            .ToListAsync(cancellationToken);

        if (wuensche.Count == 0)
        {
            return Array.Empty<DienstwunschTagesstatistik>();
        }

        var userIds = wuensche
            .Select(x => x.UserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var hauptqualifikationen = await (
                from profil in _dbContext.ItwMitarbeiterprofile.AsNoTracking()
                join mitarbeiterQualifikation in _dbContext.Set<ItwMitarbeiterQualifikation>().AsNoTracking()
                    on profil.Id equals mitarbeiterQualifikation.ItwMitarbeiterprofilId
                join qualifikation in _dbContext.ItwQualifikationen.AsNoTracking()
                    on mitarbeiterQualifikation.QualifikationId equals qualifikation.Id
                where userIds.Contains(profil.UserId)
                      && mitarbeiterQualifikation.IstHauptqualifikation
                select new
                {
                    profil.UserId,
                    qualifikation.Code
                })
            .ToDictionaryAsync(
                x => x.UserId,
                x => x.Code,
                StringComparer.OrdinalIgnoreCase,
                cancellationToken);

        var result = wuensche
            .GroupBy(x => x.WunschDatum)
            .Select(gruppe =>
            {
                var anzahlAerzte = 0;
                var anzahlNotfallsanitaeter = 0;

                foreach (var eintrag in gruppe)
                {
                    if (!hauptqualifikationen.TryGetValue(eintrag.UserId, out var qualifikationsCode))
                    {
                        continue;
                    }

                    if (string.Equals(
                        qualifikationsCode,
                        ItwQualifikationsCodes.Arzt,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        anzahlAerzte++;
                        continue;
                    }

                    if (string.Equals(
                        qualifikationsCode,
                        ItwQualifikationsCodes.Notfallsanitaeter,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        anzahlNotfallsanitaeter++;
                    }
                }

                return new DienstwunschTagesstatistik
                {
                    Datum = gruppe.Key,
                    AnzahlWuenscheGesamt = gruppe.Count(),
                    AnzahlAerzte = anzahlAerzte,
                    AnzahlNotfallsanitaeter = anzahlNotfallsanitaeter
                };
            })
            .OrderBy(x => x.Datum)
            .ToArray();

        return result;
    }
}