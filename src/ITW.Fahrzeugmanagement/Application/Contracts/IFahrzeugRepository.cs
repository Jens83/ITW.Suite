using ITW.Fahrzeugmanagement.Domain.Entities;

namespace ITW.Fahrzeugmanagement.Application.Contracts;

public interface IFahrzeugRepository
{
    Task<IReadOnlyList<Fahrzeug>> GetFahrzeugeAsync(
        CancellationToken cancellationToken = default);

    Task<Fahrzeug?> GetFahrzeugByIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByKennzeichenAsync(
        string kennzeichen,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByKennzeichenAsync(
        string kennzeichen,
        Guid ausgenommeneFahrzeugId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByVinAsync(
        string vin,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByVinAsync(
        string vin,
        Guid ausgenommeneFahrzeugId,
        CancellationToken cancellationToken = default);

    Task AddFahrzeugAsync(
        Fahrzeug fahrzeug,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}