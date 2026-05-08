using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Contracts;

public interface IArtikelChargeRepository
{
    Task<IReadOnlyList<ArtikelCharge>> GetAktiveByArtikelAsync(
        Guid artikelId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtikelCharge>> GetAktiveByArtikelUndLagerortAsync(
        Guid artikelId,
        Lagerort lagerort,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtikelCharge>> GetAktiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtikelCharge>> GetBaldAblaufendeAsync(
        DateOnly heute,
        int vorwarnungTage,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtikelCharge>> GetAbgelaufeneAsync(
        DateOnly heute,
        CancellationToken cancellationToken = default);

    Task<ArtikelCharge?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ArtikelCharge charge,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
