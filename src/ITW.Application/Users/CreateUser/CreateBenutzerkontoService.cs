using ITW.Application.Abstractions.Identity;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.CreateUser;

public sealed class CreateBenutzerkontoService
{
    private readonly IBenutzerkontoRepository _repository;
    private readonly ILogger<CreateBenutzerkontoService> _logger;

    public CreateBenutzerkontoService(IBenutzerkontoRepository repository, ILogger<CreateBenutzerkontoService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateBenutzerkontoResult> ExecuteAsync(
        CreateBenutzerkontoCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(CreateBenutzerkontoService));

        if (string.IsNullOrWhiteSpace(command.Benutzername))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Benutzername leer", nameof(CreateBenutzerkontoService));
            return CreateBenutzerkontoResult.Fehler("Der Benutzername darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(command.Email))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: E-Mail leer", nameof(CreateBenutzerkontoService));
            return CreateBenutzerkontoResult.Fehler("Die E-Mail-Adresse darf nicht leer sein.");
        }

        if (string.IsNullOrWhiteSpace(command.Passwort))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: Passwort leer", nameof(CreateBenutzerkontoService));
            return CreateBenutzerkontoResult.Fehler("Das Passwort darf nicht leer sein.");
        }

        var result = await _repository.CreateAsync(
            command.Benutzername.Trim(),
            command.Email.Trim(),
            command.Passwort,
            cancellationToken);

        if (!result.IsSuccess || result.Benutzerkonto is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(CreateBenutzerkontoService), result.ErrorMessage);
            return CreateBenutzerkontoResult.Fehler(
                result.ErrorMessage ?? "Das Benutzerkonto konnte nicht angelegt werden.");
        }

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(CreateBenutzerkontoService));
        return CreateBenutzerkontoResult.Erfolg(result.Benutzerkonto);
    }
}