using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Contracts;

public interface IDienstbesetzungsAusfallRepository
{
    Task<GeplanterDienstTagAusfall?> GetAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        DienstbesetzungsSlotCode besetzungsSlotCode,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeplanterDienstTagAusfall>> GetAlleFuerTagAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeplanterDienstTagAusfall>> GetAlleFuerPeriodeAsync(
        Guid dienstplanPeriodeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateOnly>> GetUrlaubstageFuerBenutzerUndJahrAsync(
        string userId,
        int jahr,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        GeplanterDienstTagAusfall ausfall,
        CancellationToken cancellationToken = default);

    void Remove(GeplanterDienstTagAusfall ausfall);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}