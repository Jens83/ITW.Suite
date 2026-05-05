using ITW.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.LockUser;

public sealed class LockUserService
{
    private readonly IBenutzerkontoRepository _repository;
    private readonly ILogger<LockUserService> _logger;

    public LockUserService(IBenutzerkontoRepository repository, ILogger<LockUserService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<LockUserResult> ExecuteAsync(
        LockUserCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(LockUserService));

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(LockUserService));
            return LockUserResult.Fehler("Die UserId darf nicht leer sein.");
        }

        var result = await _repository.SperrenAsync(command.UserId, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(LockUserService), result.ErrorMessage);
            return LockUserResult.Fehler(result.ErrorMessage ?? "Das Benutzerkonto konnte nicht gesperrt werden.");
        }

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(LockUserService));
        return LockUserResult.Erfolg();
    }
}