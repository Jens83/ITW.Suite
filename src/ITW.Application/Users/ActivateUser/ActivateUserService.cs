using ITW.Application.Abstractions.Identity;

namespace ITW.Application.Users.ActivateUser;

public sealed class ActivateUserService
{
    private readonly IBenutzerkontoRepository _repository;

    public ActivateUserService(IBenutzerkontoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ActivateUserResult> ExecuteAsync(
        ActivateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return ActivateUserResult.Fehler("Die UserId darf nicht leer sein.");
        }

        var result = await _repository.AktivierenAsync(command.UserId, cancellationToken);

        return result.IsSuccess
            ? ActivateUserResult.Erfolg()
            : ActivateUserResult.Fehler(result.ErrorMessage ?? "Das Benutzerkonto konnte nicht aktiviert werden.");
    }
}