using ITW.Application.Abstractions.Identity;

namespace ITW.Application.Users.CreateUser;

public sealed class CreateBenutzerkontoService
{
    private readonly IBenutzerkontoRepository _repository;

    public CreateBenutzerkontoService(IBenutzerkontoRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<CreateBenutzerkontoResult> ExecuteAsync(
        CreateBenutzerkontoCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Benutzername))
        {
            return CreateBenutzerkontoResult.Fehler("Der Benutzername darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            return CreateBenutzerkontoResult.Fehler("Die E-Mail-Adresse darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(command.Passwort))
        {
            return CreateBenutzerkontoResult.Fehler("Das Passwort darf nicht leer sein.");
        }

        var result = await _repository.CreateAsync(
            command.Benutzername.Trim(),
            command.Email.Trim(),
            command.Passwort,
            cancellationToken);

        if (!result.IsSuccess || result.Benutzerkonto is null)
        {
            return CreateBenutzerkontoResult.Fehler(
                result.ErrorMessage ?? "Das Benutzerkonto konnte nicht angelegt werden.");
        }

        return CreateBenutzerkontoResult.Erfolg(result.Benutzerkonto);
    }
}