using ITW.Application.Abstractions.Identity;

namespace ITW.Application.Users.LockUser;

public sealed class LockUserService
{
    private readonly IBenutzerkontoRepository _repository;

    public LockUserService(IBenutzerkontoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<LockUserResult> ExecuteAsync(
        LockUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return LockUserResult.Fehler("Die UserId darf nicht leer sein.");
        }

        var result = await _repository.SperrenAsync(command.UserId, cancellationToken);

        return result.IsSuccess
            ? LockUserResult.Erfolg()
            : LockUserResult.Fehler(result.ErrorMessage ?? "Das Benutzerkonto konnte nicht gesperrt werden.");
    }
}