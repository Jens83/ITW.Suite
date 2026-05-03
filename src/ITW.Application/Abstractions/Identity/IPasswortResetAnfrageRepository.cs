// Datei: src/ITW.Application/Abstractions/Persistence/IPasswortResetAnfrageRepository.cs
using ITW.Domain.Organisation.Enums;
using ITW.Domain.Security.Entities;

namespace ITW.Application.Abstractions.Persistence;

public interface IPasswortResetAnfrageRepository
{
    Task<PasswortResetAnfrage?> GetOffeneAnfrageFuerUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<PasswortResetAnfrage?> GetOffeneAnfrageByIdAsync(
        Guid anfrageId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PasswortResetAnfrage>> GetOffeneAnfragenByBereichAsync(
        Organisationsbereich bereich,
        CancellationToken cancellationToken = default);

    Task<int> CountOffeneAnfragenByBereichAsync(
        Organisationsbereich bereich,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PasswortResetAnfrage anfrage,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}