// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfragen/ReadOffenePasswortResetAnfragenService.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;

namespace ITW.Application.Users.ReadOffenePasswortResetAnfragen;

public sealed class ReadOffenePasswortResetAnfragenService
{
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;

    public ReadOffenePasswortResetAnfragenService(
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository)
    {
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository
            ?? throw new ArgumentNullException(nameof(passwortResetAnfrageRepository));
    }

    public async Task<ReadOffenePasswortResetAnfragenResult> ExecuteAsync(
        ReadOffenePasswortResetAnfragenQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.Bereich == OrganisationsbereichCode.Unbekannt)
        {
            return ReadOffenePasswortResetAnfragenResult.Fehler(
                "Der Bereich für die Passwort-Reset-Anfragen ist ungültig.");
        }

        var anfragen = await _passwortResetAnfrageRepository.GetOffeneAnfragenByBereichAsync(
            query.Bereich.ToDomain(),
            cancellationToken);

        var result = anfragen
            .Select(x => new OffenePasswortResetAnfrageDto(
                x.Id,
                x.UserId,
                x.Benutzername,
                x.Vorname,
                x.Nachname,
                x.AngefordertAm))
            .ToList();

        return ReadOffenePasswortResetAnfragenResult.Erfolg(result);
    }
}