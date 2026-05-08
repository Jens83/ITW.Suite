using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Contracts;

public interface ILagerArtikelRepository
{
    Task<IReadOnlyList<LagerArtikel>> GetAlleAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LagerArtikel>> GetAktiveAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LagerArtikel>> GetAktiveByKategorieAsync(
        ArtikelKategorie kategorie,
        CancellationToken cancellationToken = default);

    Task<LagerArtikel?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid ausgenommeneId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        LagerArtikel artikel,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
