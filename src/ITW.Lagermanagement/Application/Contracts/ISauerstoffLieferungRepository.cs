using ITW.Lagermanagement.Domain.Entities;

namespace ITW.Lagermanagement.Application.Contracts;

public interface ISauerstoffLieferungRepository
{
    Task<IReadOnlyList<SauerstoffLieferung>> GetAlleAsync(
        CancellationToken cancellationToken = default);

    Task<SauerstoffLieferung?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByLieferscheinNummerAsync(
        string lieferscheinNummer,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SauerstoffLieferung lieferung,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
