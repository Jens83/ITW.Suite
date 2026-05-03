using ITW.Fahrzeugmanagement.Domain.Entities;
using ITW.Fahrzeugmanagement.Domain.Enums;

namespace ITW.Fahrzeugmanagement.Application.Contracts;

public interface IFahrzeugPruefungRepository
{
    Task<IReadOnlyList<FahrzeugPruefung>> GetByFahrzeugIdAsync(
        Guid fahrzeugId,
        CancellationToken cancellationToken = default);

    Task<FahrzeugPruefung?> GetByFahrzeugIdUndTypAsync(
        Guid fahrzeugId,
        FahrzeugPruefungTyp typ,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        FahrzeugPruefung pruefung,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}