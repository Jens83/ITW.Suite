using ITW.Dienstplan.Domain.Entities;

namespace ITW.Dienstplan.Application.Contracts;

public interface IDienstplanPeriodeRepository
{
    Task<bool> ExistiertAsync(
        int jahr,
        int monat,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        DienstplanPeriode periode,
        CancellationToken cancellationToken = default);

    Task<DienstplanPeriode?> GetByIdAsync(
        Guid periodeId,
        CancellationToken cancellationToken = default);

    Task<DienstplanPeriode?> GetAktuelleOffeneAsync(
        CancellationToken cancellationToken = default);

    Task<int> CountOffeneAsync(
        CancellationToken cancellationToken = default);

    Task<int> CountOffeneFuerBenutzerOhneWuenscheAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DienstplanPeriode>> GetOffeneAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DienstplanPeriode>> GetAlleAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}