using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ChangeAreaRole;

public sealed class ChangeUserAreaRoleService
{
    private readonly IBenutzerBereichszuordnungRepository _repository;

    public ChangeUserAreaRoleService(IBenutzerBereichszuordnungRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<ChangeUserAreaRoleResult> ExecuteAsync(
        ChangeUserAreaRoleCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
        {
            return ChangeUserAreaRoleResult.Fehler("Die UserId darf nicht leer sein.");
        }

        var zuordnung = await _repository.GetAktivePrimaereZuordnungAsync(
            command.UserId,
            cancellationToken);

        if (zuordnung is null)
        {
            return ChangeUserAreaRoleResult.Fehler(
                "Für den Benutzer wurde keine aktive primäre Bereichszuordnung gefunden.");
        }

        zuordnung.RolleAendern(
            command.NeueRolle.ToDomain(),
            command.NeueFuehrungsverantwortung.ToDomain());

        await _repository.SaveChangesAsync(cancellationToken);

        return ChangeUserAreaRoleResult.Erfolg();
    }
}