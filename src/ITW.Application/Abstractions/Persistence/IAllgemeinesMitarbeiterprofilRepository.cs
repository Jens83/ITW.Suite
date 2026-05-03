using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Abstractions.Persistence;

public interface IAllgemeinesMitarbeiterprofilRepository
{
    Task<AllgemeinesMitarbeiterprofil?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AllgemeinesMitarbeiterprofil>> GetByUserIdsAsync(
        IReadOnlyCollection<string> userIds,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        string userId,
        string vorname,
        string nachname,
        MitarbeiterBeschaeftigungsart beschaeftigungsart,
        string? telefonnummer,
        string? strasse,
        string? hausnummer,
        string? postleitzahl,
        string? ort,
        DateTimeOffset aktualisiertAm,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}