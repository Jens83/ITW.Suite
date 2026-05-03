// Datei: src/ITW.Application/Abstractions/Identity/IPasswortResetBenutzerLookupRepository.cs
namespace ITW.Application.Abstractions.Identity;

public interface IPasswortResetBenutzerLookupRepository
{
    Task<BenutzerkontoDto?> GetByBenutzernameAsync(
        string benutzername,
        CancellationToken cancellationToken = default);
}