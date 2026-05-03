using ITW.Dienstplan.Application.Contracts;
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Enums;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Persistence.Repositories;

public sealed class DienstplanMitarbeiterPlanungsRepository : IDienstplanMitarbeiterPlanungsRepository
{
    private readonly PlatformDbContext _dbContext;

    public DienstplanMitarbeiterPlanungsRepository(PlatformDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<IReadOnlyList<DienstplanMitarbeiterPlanungsstammdaten>> GetAktivePlanungsmitarbeiterAsync(
        CancellationToken cancellationToken = default)
    {
        var jetzt = DateTimeOffset.UtcNow;

        var basisdaten = await (
                from zuordnung in _dbContext.BenutzerBereichszuordnungen.AsNoTracking()
                join konto in _dbContext.Users.AsNoTracking()
                    on zuordnung.UserId equals konto.Id
                join allgemeinesProfil in _dbContext.AllgemeineMitarbeiterprofile.AsNoTracking()
                    on zuordnung.UserId equals allgemeinesProfil.UserId into allgemeineProfile
                from allgemeinesProfil in allgemeineProfile.DefaultIfEmpty()
                join itwProfil in _dbContext.ItwMitarbeiterprofile.AsNoTracking()
                    on zuordnung.UserId equals itwProfil.UserId into itwProfile
                from itwProfil in itwProfile.DefaultIfEmpty()
                where zuordnung.IsActive
                      && zuordnung.IsPrimary
                      && zuordnung.Bereich == Organisationsbereich.Intensivtransport
                select new BasisdatenRow
                {
                    UserId = konto.Id,
                    Benutzername = konto.UserName ?? string.Empty,
                    IstGesperrt = konto.LockoutEnabled && konto.LockoutEnd.HasValue && konto.LockoutEnd.Value > jetzt,
                    Vorname = allgemeinesProfil != null ? allgemeinesProfil.Vorname : null,
                    Nachname = allgemeinesProfil != null ? allgemeinesProfil.Nachname : null,
                    DisplayName = allgemeinesProfil != null ? allgemeinesProfil.DisplayName : null,
                    Beschaeftigungsart = allgemeinesProfil != null
                        ? allgemeinesProfil.Beschaeftigungsart
                        : MitarbeiterBeschaeftigungsart.Unbekannt,
                    HatStammdatenprofil = allgemeinesProfil != null,
                    ItwMitarbeiterprofilId = itwProfil != null ? itwProfil.Id : null,
                    HatItwProfil = itwProfil != null
                })
            .ToListAsync(cancellationToken);

        if (basisdaten.Count == 0)
        {
            return Array.Empty<DienstplanMitarbeiterPlanungsstammdaten>();
        }

        var profilIds = basisdaten
            .Where(x => x.ItwMitarbeiterprofilId.HasValue)
            .Select(x => x.ItwMitarbeiterprofilId!.Value)
            .Distinct()
            .ToArray();

        var hauptqualifikationen = await (
                from mitarbeiterQualifikation in _dbContext.Set<ItwMitarbeiterQualifikation>().AsNoTracking()
                join qualifikation in _dbContext.ItwQualifikationen.AsNoTracking()
                    on mitarbeiterQualifikation.QualifikationId equals qualifikation.Id
                where profilIds.Contains(mitarbeiterQualifikation.ItwMitarbeiterprofilId)
                      && mitarbeiterQualifikation.IstHauptqualifikation
                select new HauptqualifikationRow
                {
                    ItwMitarbeiterprofilId = mitarbeiterQualifikation.ItwMitarbeiterprofilId,
                    Code = qualifikation.Code,
                    Bezeichnung = qualifikation.Bezeichnung
                })
            .ToListAsync(cancellationToken);

        var hauptqualifikationLookup = hauptqualifikationen.ToDictionary(
            x => x.ItwMitarbeiterprofilId,
            x => x);

        var result = basisdaten
            .Select(x =>
            {
                var anzeigeName = !string.IsNullOrWhiteSpace(x.DisplayName)
                    ? x.DisplayName!.Trim()
                    : $"{x.Vorname} {x.Nachname}".Trim();

                if (string.IsNullOrWhiteSpace(anzeigeName))
                {
                    anzeigeName = x.Benutzername;
                }

                HauptqualifikationRow? hauptqualifikation = null;

                if (x.ItwMitarbeiterprofilId.HasValue)
                {
                    hauptqualifikationLookup.TryGetValue(x.ItwMitarbeiterprofilId.Value, out hauptqualifikation);
                }

                return new DienstplanMitarbeiterPlanungsstammdaten
                {
                    UserId = x.UserId,
                    AnzeigeName = anzeigeName,
                    Beschaeftigungsart = x.Beschaeftigungsart,
                    HauptqualifikationCode = hauptqualifikation?.Code ?? string.Empty,
                    HauptqualifikationBezeichnung = hauptqualifikation?.Bezeichnung ?? string.Empty,
                    IstGesperrt = x.IstGesperrt,
                    HatStammdatenprofil = x.HatStammdatenprofil,
                    HatItwProfil = x.HatItwProfil
                };
            })
            .OrderBy(x => x.HauptqualifikationBezeichnung, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.AnzeigeName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return result;
    }

    private sealed class BasisdatenRow
    {
        public string UserId { get; init; } = string.Empty;

        public string Benutzername { get; init; } = string.Empty;

        public bool IstGesperrt { get; init; }

        public string? Vorname { get; init; }

        public string? Nachname { get; init; }

        public string? DisplayName { get; init; }

        public MitarbeiterBeschaeftigungsart Beschaeftigungsart { get; init; }

        public bool HatStammdatenprofil { get; init; }

        public Guid? ItwMitarbeiterprofilId { get; init; }

        public bool HatItwProfil { get; init; }
    }

    private sealed class HauptqualifikationRow
    {
        public Guid ItwMitarbeiterprofilId { get; init; }

        public string Code { get; init; } = string.Empty;

        public string Bezeichnung { get; init; } = string.Empty;
    }
}