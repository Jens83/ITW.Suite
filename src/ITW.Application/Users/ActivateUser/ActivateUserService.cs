using ITW.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.ActivateUser;

public sealed class ActivateUserService
{
    private readonly IBenutzerkontoRepository _repository;
    private readonly ILogger<ActivateUserService> _logger;

    public ActivateUserService(IBenutzerkontoRepository repository, ILogger<ActivateUserService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ActivateUserResult> ExecuteAsync(
        ActivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(ActivateUserService));

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(ActivateUserService));
            return ActivateUserResult.Fehler("Die UserId darf nicht leer sein.");
        }

        var result = await _repository.AktivierenAsync(command.UserId, cancellationToken);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ActivateUserService), result.ErrorMessage);
            return ActivateUserResult.Fehler(result.ErrorMessage ?? "Das Benutzerkonto konnte nicht aktiviert werden.");
        }

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(ActivateUserService));
        return ActivateUserResult.Erfolg();
    }
}