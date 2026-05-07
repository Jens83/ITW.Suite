using ITW.Domain.Personnel.Entities;

namespace ITW.Application.Personnel.Urlaub.Contracts;

public interface IMitarbeiterUrlaubszeitraumRepository
{
    Task<MitarbeiterUrlaubszeitraum?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MitarbeiterUrlaubszeitraum>> GetAlleFuerBenutzerAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MitarbeiterUrlaubszeitraum>> GetAlleFuerBenutzerUndJahrAsync(
        string userId,
        int jahr,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MitarbeiterUrlaubszeitraum>> GetAllAusstehendAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAktiveUserIdsFuerDatumAsync(
        DateOnly datum,
        CancellationToken cancellationToken = default);

    Task<bool> HatUeberschneidungAsync(
        string userId,
        DateOnly von,
        DateOnly bis,
        Guid? ausnahmeId = null,
        CancellationToken cancellationToken = default);

    Task AddOrUpdateAsync(
        MitarbeiterUrlaubszeitraum zeitraum,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
