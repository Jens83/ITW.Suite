using ITW.Lagermanagement.Domain.Entities;

namespace ITW.Lagermanagement.Application.Contracts;

public interface ISauerstoffFlascheRepository
{
    Task<IReadOnlyList<SauerstoffFlasche>> GetAktiveAsync(
        CancellationToken cancellationToken = default);

    Task<SauerstoffFlasche?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SauerstoffFlasche?> GetByFlaschenNummerAsync(
        string flaschenNummer,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SauerstoffFlasche>> GetByLieferungIdAsync(
        Guid lieferungId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByFlaschenNummerAsync(
        string flaschenNummer,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SauerstoffFlasche flasche,
        CancellationToken cancellationToken = default);

    Task AddBewegungAsync(
        SauerstoffBewegung bewegung,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
