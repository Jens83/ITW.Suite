using ITW.Domain.Organisation.Entities;
using ITW.Domain.Organisation.Enums;

namespace ITW.Application.Organisation.Contracts;

public interface IModulZuweisungRepository
{
    Task<ModulZuweisung?> GetByModulBereichRolleAsync(
        Modul modul,
        Organisationsbereich bereich,
        Bereichsrolle rolle,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModulZuweisung>> GetAktiveModuleFuerBereichUndRolleAsync(
        Organisationsbereich bereich,
        Bereichsrolle rolle,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ModulZuweisung>> GetAlleAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ModulZuweisung zuweisung,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}