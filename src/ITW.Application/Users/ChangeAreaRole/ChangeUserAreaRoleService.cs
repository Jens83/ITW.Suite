using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.ChangeAreaRole;

public sealed class ChangeUserAreaRoleService
{
    private readonly IBenutzerBereichszuordnungRepository _repository;
    private readonly ILogger<ChangeUserAreaRoleService> _logger;

    public ChangeUserAreaRoleService(IBenutzerBereichszuordnungRepository repository, ILogger<ChangeUserAreaRoleService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ChangeUserAreaRoleResult> ExecuteAsync(
        ChangeUserAreaRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("UseCase {UseCase} begonnen", nameof(ChangeUserAreaRoleService));

        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: UserId leer", nameof(ChangeUserAreaRoleService));
            return ChangeUserAreaRoleResult.Fehler("Die UserId darf nicht leer sein.");
        }

        var zuordnung = await _repository.GetAktivePrimaereZuordnungAsync(
            command.UserId,
            cancellationToken);

        if (zuordnung is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ChangeUserAreaRoleService), "Keine aktive primäre Bereichszuordnung gefunden");
            return ChangeUserAreaRoleResult.Fehler(
                "Für den Benutzer wurde keine aktive primäre Bereichszuordnung gefunden.");
        }

        zuordnung.RolleAendern(
            command.NeueRolle.ToDomain(),
            command.NeueFuehrungsverantwortung.ToDomain());

        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(ChangeUserAreaRoleService));
        return ChangeUserAreaRoleResult.Erfolg();
    }
}