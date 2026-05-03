using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Qualifications;

namespace ITW.Application.Abstractions.Persistence;

public interface IItwMitarbeiterprofilRepository
{
    Task EnsureStandardqualifikationenAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItwQualifikation>> GetAktiveQualifikationenAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ItwMitarbeiterprofil>> GetByUserIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default);

    Task<ItwMitarbeiterprofil?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task UpsertQualifikationenAsync(
        string userId,
        Guid hauptqualifikationId,
        IReadOnlyCollection<Guid> zusatzqualifikationIds,
        DateTimeOffset aktualisiertAm,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}