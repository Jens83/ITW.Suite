
using System.Security.Claims;
using ITW.Application.Abstractions.Identity;
using ITW.Infrastructure.Persistence.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ITW.Infrastructure.Identity.UserManagement;

public sealed class BenutzerkontoRepository : IBenutzerkontoRepository
{
    private readonly PlatformDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public BenutzerkontoRepository(
        PlatformDbContext dbContext,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    public async Task<IReadOnlyList<BenutzerkontoDto>> GetByIdsAsync(
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(userIds);

        var ids = userIds
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ids.Length == 0)
        {
            return Array.Empty<BenutzerkontoDto>();
        }

        var jetzt = DateTimeOffset.UtcNow;

        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new BenutzerkontoDto(
                x.Id,
                x.UserName ?? string.Empty,
                x.Email ?? string.Empty,
                x.LockoutEnabled && x.LockoutEnd.HasValue && x.LockoutEnd.Value > jetzt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BenutzerkontoDto>> GetNichtZugeordneteBenutzerkontenAsync(
        CancellationToken cancellationToken = default)
    {
        var aktivePrimaereZuordnungen = _dbContext.BenutzerBereichszuordnungen
            .Where(x => x.IsActive && x.IsPrimary)
            .Select(x => x.UserId);

        var jetzt = DateTimeOffset.UtcNow;

        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => !aktivePrimaereZuordnungen.Contains(x.Id))
            .OrderBy(x => x.UserName)
            .ThenBy(x => x.Email)
            .Select(x => new BenutzerkontoDto(
                x.Id,
                x.UserName ?? string.Empty,
                x.Email ?? string.Empty,
                x.LockoutEnabled && x.LockoutEnd.HasValue && x.LockoutEnd.Value > jetzt))
            .ToListAsync(cancellationToken);
    }

    
    public async Task<CreateBenutzerkontoRepositoryResult> CreateAsync(
        string benutzername,
        string email,
        string passwort,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(benutzername))
        {
            return CreateBenutzerkontoRepositoryResult.Fehler("Der Benutzername darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return CreateBenutzerkontoRepositoryResult.Fehler("Die E-Mail-Adresse darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(passwort))
        {
            return CreateBenutzerkontoRepositoryResult.Fehler("Das Passwort darf nicht leer sein.");
        }

        var existingByName = await _userManager.FindByNameAsync(benutzername);
        if (existingByName is not null)
        {
            return CreateBenutzerkontoRepositoryResult.Fehler("Der Benutzername ist bereits vergeben.");
        }

        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
        {
            return CreateBenutzerkontoRepositoryResult.Fehler("Die E-Mail-Adresse ist bereits vergeben.");
        }

        var user = new ApplicationUser
        {
            UserName = benutzername,
            Email = email,
            LockoutEnabled = true
        };

        var result = await _userManager.CreateAsync(user, passwort);

        if (!result.Succeeded)
        {
            var errorMessage = string.Join(" ", result.Errors.Select(x => x.Description));
            return CreateBenutzerkontoRepositoryResult.Fehler(
                string.IsNullOrWhiteSpace(errorMessage)
                    ? "Das Benutzerkonto konnte nicht angelegt werden."
                    : errorMessage);
        }

        var claimResult = await _userManager.AddClaimAsync(
            user,
            new System.Security.Claims.Claim(
                BenutzerkontoClaimTypes.MussPasswortAendern,
                "true"));

        if (!claimResult.Succeeded)
        {
            var deleteResult = await _userManager.DeleteAsync(user);

            var claimError = string.Join(" ", claimResult.Errors.Select(x => x.Description));
            var message = string.IsNullOrWhiteSpace(claimError)
                ? "Das Benutzerkonto wurde angelegt, aber die Passwortwechselpflicht konnte nicht gesetzt werden."
                : claimError;

            if (!deleteResult.Succeeded)
            {
                var deleteError = string.Join(" ", deleteResult.Errors.Select(x => x.Description));
                message += string.IsNullOrWhiteSpace(deleteError)
                    ? " Das angelegte Benutzerkonto konnte anschließend nicht automatisch zurückgerollt werden."
                    : $" Das angelegte Benutzerkonto konnte anschließend nicht automatisch zurückgerollt werden: {deleteError}";
            }

            return CreateBenutzerkontoRepositoryResult.Fehler(message);
        }

        var benutzerkonto = new BenutzerkontoDto(
            user.Id,
            user.UserName ?? string.Empty,
            user.Email ?? string.Empty,
            false);

        return CreateBenutzerkontoRepositoryResult.Erfolg(benutzerkonto);
    }

    public async Task<UpdateBenutzerkontoStatusRepositoryResult> SperrenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return UpdateBenutzerkontoStatusRepositoryResult.Fehler("Das Benutzerkonto wurde nicht gefunden.");
        }

        if (!user.LockoutEnabled)
        {
            user.LockoutEnabled = true;

            var enableResult = await _userManager.UpdateAsync(user);
            if (!enableResult.Succeeded)
            {
                return UpdateBenutzerkontoStatusRepositoryResult.Fehler(
                    ErzeugeFehlermeldung(enableResult, "Der Status des Benutzerkontos konnte nicht geändert werden."));
            }
        }

        var lockResult = await _userManager.SetLockoutEndDateAsync(
            user,
            DateTimeOffset.UtcNow.AddYears(100));

        if (!lockResult.Succeeded)
        {
            return UpdateBenutzerkontoStatusRepositoryResult.Fehler(
                ErzeugeFehlermeldung(lockResult, "Der Status des Benutzerkontos konnte nicht geändert werden."));
        }

        return UpdateBenutzerkontoStatusRepositoryResult.Erfolg();
    }

    public async Task<UpdateBenutzerkontoStatusRepositoryResult> AktivierenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return UpdateBenutzerkontoStatusRepositoryResult.Fehler("Das Benutzerkonto wurde nicht gefunden.");
        }

        if (!user.LockoutEnabled)
        {
            user.LockoutEnabled = true;

            var enableResult = await _userManager.UpdateAsync(user);
            if (!enableResult.Succeeded)
            {
                return UpdateBenutzerkontoStatusRepositoryResult.Fehler(
                    ErzeugeFehlermeldung(enableResult, "Der Status des Benutzerkontos konnte nicht geändert werden."));
            }
        }

        var activateResult = await _userManager.SetLockoutEndDateAsync(user, null);

        if (!activateResult.Succeeded)
        {
            return UpdateBenutzerkontoStatusRepositoryResult.Fehler(
                ErzeugeFehlermeldung(activateResult, "Der Status des Benutzerkontos konnte nicht geändert werden."));
        }

        return UpdateBenutzerkontoStatusRepositoryResult.Erfolg();
    }

    public async Task<SetTemporaeresPasswortRepositoryResult> SetzeTemporaeresPasswortAsync(
        string userId,
        string temporaeresPasswort,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return SetTemporaeresPasswortRepositoryResult.Fehler("Die UserId ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(temporaeresPasswort))
        {
            return SetTemporaeresPasswortRepositoryResult.Fehler("Das temporäre Passwort ist erforderlich.");
        }

        var user = await _userManager.FindByIdAsync(userId.Trim());
        if (user is null)
        {
            return SetTemporaeresPasswortRepositoryResult.Fehler("Das Benutzerkonto wurde nicht gefunden.");
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetResult = await _userManager.ResetPasswordAsync(
            user,
            resetToken,
            temporaeresPasswort.Trim());

        if (!resetResult.Succeeded)
        {
            return SetTemporaeresPasswortRepositoryResult.Fehler(
                ErzeugeFehlermeldung(resetResult, "Das temporäre Passwort konnte nicht gesetzt werden."));
        }

        var bestehendeClaims = await _userManager.GetClaimsAsync(user);

        await SynchronisiereClaimAsync(
            user,
            bestehendeClaims,
            BenutzerkontoClaimTypes.MussPasswortAendern,
            "true");

        var securityStampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!securityStampResult.Succeeded)
        {
            return SetTemporaeresPasswortRepositoryResult.Fehler(
                ErzeugeFehlermeldung(securityStampResult, "Die Passwortwechselpflicht konnte nicht gesetzt werden."));
        }

        return SetTemporaeresPasswortRepositoryResult.Erfolg();
    }

    public async Task EntfernePasswortwechselPflichtAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        var user = await _userManager.FindByIdAsync(userId.Trim());
        if (user is null)
        {
            throw new InvalidOperationException("Das Benutzerkonto wurde nicht gefunden.");
        }

        var bestehendeClaims = await _userManager.GetClaimsAsync(user);

        await SynchronisiereClaimAsync(
            user,
            bestehendeClaims,
            BenutzerkontoClaimTypes.MussPasswortAendern,
            null);

        var securityStampResult = await _userManager.UpdateSecurityStampAsync(user);
        if (!securityStampResult.Succeeded)
        {
            throw new InvalidOperationException(
                ErzeugeFehlermeldung(securityStampResult, "Die Passwortwechselpflicht konnte nicht entfernt werden."));
        }
    }

    public async Task SynchronisiereNamensClaimsAsync(
        string userId,
        string? vorname,
        string? nachname,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("Die UserId ist erforderlich.", nameof(userId));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException("Das Benutzerkonto wurde nicht gefunden.");
        }

        var bestehendeClaims = await _userManager.GetClaimsAsync(user);

        await SynchronisiereClaimAsync(
            user,
            bestehendeClaims,
            BenutzerkontoClaimTypes.FirstName,
            LeereAlsNull(vorname));

        await SynchronisiereClaimAsync(
            user,
            bestehendeClaims,
            BenutzerkontoClaimTypes.LastName,
            LeereAlsNull(nachname));
    }

    private async Task SynchronisiereClaimAsync(
        ApplicationUser user,
        IList<Claim> bestehendeClaims,
        string claimType,
        string? neuerWert)
    {
        var claimsVomTyp = bestehendeClaims
            .Where(x => string.Equals(x.Type, claimType, StringComparison.Ordinal))
            .ToArray();

        if (string.IsNullOrWhiteSpace(neuerWert))
        {
            if (claimsVomTyp.Length > 0)
            {
                var removeResult = await _userManager.RemoveClaimsAsync(user, claimsVomTyp);
                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        ErzeugeFehlermeldung(removeResult, "Der Benutzerclaim konnte nicht entfernt werden."));
                }
            }

            return;
        }

        if (claimsVomTyp.Length == 1 &&
            string.Equals(claimsVomTyp[0].Value, neuerWert, StringComparison.Ordinal))
        {
            return;
        }

        if (claimsVomTyp.Length > 0)
        {
            var removeResult = await _userManager.RemoveClaimsAsync(user, claimsVomTyp);
            if (!removeResult.Succeeded)
            {
                throw new InvalidOperationException(
                    ErzeugeFehlermeldung(removeResult, "Der Benutzerclaim konnte nicht aktualisiert werden."));
            }
        }

        var addResult = await _userManager.AddClaimAsync(user, new Claim(claimType, neuerWert));
        if (!addResult.Succeeded)
        {
            throw new InvalidOperationException(
                ErzeugeFehlermeldung(addResult, "Der Benutzerclaim konnte nicht gesetzt werden."));
        }
    }

    private static string? LeereAlsNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string ErzeugeFehlermeldung(
        IdentityResult result,
        string standardText)
    {
        var errorMessage = string.Join(" ", result.Errors.Select(x => x.Description));

        return string.IsNullOrWhiteSpace(errorMessage)
            ? standardText
            : errorMessage;
    }
}