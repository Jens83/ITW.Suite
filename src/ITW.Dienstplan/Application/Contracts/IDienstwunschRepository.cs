using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Contracts;

public interface IDienstwunschRepository
{
    Task<Dienstwunsch?> GetAsync(
        Guid dienstplanPeriodeId,
        string userId,
        DateOnly wunschDatum,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerBenutzerAsync(
        Guid dienstplanPeriodeId,
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerPeriodeAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Dienstwunsch>> GetAlleFuerTagAsync(
        Guid dienstplanPeriodeId,
        DateOnly wunschDatum,
        DienstwunschTyp? wunschTyp = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Dienstwunsch dienstwunsch,
        CancellationToken cancellationToken = default);

    void Remove(Dienstwunsch dienstwunsch);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}