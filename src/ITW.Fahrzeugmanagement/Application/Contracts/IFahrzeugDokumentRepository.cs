using ITW.Fahrzeugmanagement.Domain.Entities;

namespace ITW.Fahrzeugmanagement.Application.Contracts;

public interface IFahrzeugDokumentRepository
{
    Task<IReadOnlyList<FahrzeugDokument>> GetByFahrzeugIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default);

    Task<FahrzeugDokument?> GetByIdAsync(
        Guid dokumentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        FahrzeugDokument dokument,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid dokumentId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}