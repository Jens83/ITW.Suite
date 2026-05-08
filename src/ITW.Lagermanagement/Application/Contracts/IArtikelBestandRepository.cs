using ITW.Lagermanagement.Domain.Entities;
using ITW.Lagermanagement.Domain.Enums;

namespace ITW.Lagermanagement.Application.Contracts;

public interface IArtikelBestandRepository
{
    Task<IReadOnlyList<ArtikelBestand>> GetAlleAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtikelBestand>> GetByLagerortAsync(
        Lagerort lagerort,
        CancellationToken cancellationToken = default);

    Task<ArtikelBestand?> GetByArtikelUndLagerortAsync(
        Guid artikelId,
        Lagerort lagerort,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ArtikelBestand>> GetUnterschrittenerMindestbestandAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ArtikelBestand bestand,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
