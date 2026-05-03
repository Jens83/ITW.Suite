// Datei: src/ITW.Infrastructure/Identity/UserManagement/PasswortResetBenutzerLookupRepository.cs
using ITW.Application.Abstractions.Identity;
using Microsoft.AspNetCore.Identity;

namespace ITW.Infrastructure.Identity.UserManagement;

public sealed class PasswortResetBenutzerLookupRepository : IPasswortResetBenutzerLookupRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public PasswortResetBenutzerLookupRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<BenutzerkontoDto?> GetByBenutzernameAsync(
        string benutzername,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(benutzername))
        {
            return null;
        }

        var user = await _userManager.FindByNameAsync(benutzername.Trim());
        if (user is null)
        {
            return null;
        }

        var jetzt = DateTimeOffset.UtcNow;

        return new BenutzerkontoDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > jetzt);
    }
}