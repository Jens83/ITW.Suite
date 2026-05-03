// Datei: src/ITW.Application/Abstractions/Identity/IBenutzerkontoRepository.cs
namespace ITW.Application.Abstractions.Identity;

public interface IBenutzerkontoRepository
{
    Task<IReadOnlyList<BenutzerkontoDto>> GetByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BenutzerkontoDto>> GetNichtZugeordneteBenutzerkontenAsync(
        CancellationToken cancellationToken = default);

    Task<CreateBenutzerkontoRepositoryResult> CreateAsync(
        string benutzername,
        string email,
        string passwort,
        CancellationToken cancellationToken = default);

    Task<UpdateBenutzerkontoStatusRepositoryResult> SperrenAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<UpdateBenutzerkontoStatusRepositoryResult> AktivierenAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task<SetTemporaeresPasswortRepositoryResult> SetzeTemporaeresPasswortAsync(
        string userId,
        string temporaeresPasswort,
        CancellationToken cancellationToken = default);

    Task EntfernePasswortwechselPflichtAsync(
        string userId,
        CancellationToken cancellationToken = default);

    Task SynchronisiereNamensClaimsAsync(
        string userId,
        string? vorname,
        string? nachname,
        CancellationToken cancellationToken = default);
}