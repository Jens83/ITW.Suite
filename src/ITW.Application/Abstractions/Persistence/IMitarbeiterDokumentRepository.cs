// Datei: src/ITW.Application/Abstractions/Persistence/IMitarbeiterDokumentRepository.cs
using ITW.Domain.Personnel.Entities;

namespace ITW.Application.Abstractions.Persistence;

public interface IMitarbeiterDokumentRepository
{
    Task<IReadOnlyList<MitarbeiterDokument>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<MitarbeiterDokument?> GetByIdAsync(
        Guid dokumentId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        MitarbeiterDokument dokument,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}