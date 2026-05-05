// Datei: src/ITW.Application/Users/ReadOffenePasswortResetAnfrageDetail/ReadOffenePasswortResetAnfrageDetailService.cs
using ITW.Application.Abstractions.Persistence;
using ITW.Application.Organisation.Contracts;
using Microsoft.Extensions.Logging;

namespace ITW.Application.Users.ReadOffenePasswortResetAnfrageDetail;

public sealed class ReadOffenePasswortResetAnfrageDetailService
{
    private readonly IPasswortResetAnfrageRepository _passwortResetAnfrageRepository;
    private readonly ILogger<ReadOffenePasswortResetAnfrageDetailService> _logger;

    public ReadOffenePasswortResetAnfrageDetailService(
        IPasswortResetAnfrageRepository passwortResetAnfrageRepository,
        ILogger<ReadOffenePasswortResetAnfrageDetailService> logger)
    {
        ArgumentNullException.ThrowIfNull(passwortResetAnfrageRepository);
        _passwortResetAnfrageRepository = passwortResetAnfrageRepository;
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async Task<ReadOffenePasswortResetAnfrageDetailResult> ExecuteAsync(
        ReadOffenePasswortResetAnfrageDetailQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.AnfrageId == Guid.Empty)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: AnfrageId leer", nameof(ReadOffenePasswortResetAnfrageDetailService));
            return ReadOffenePasswortResetAnfrageDetailResult.Fehler(
                "Die Passwort-Reset-Anfrage ist ungültig.");
        }

        var anfrage = await _passwortResetAnfrageRepository.GetOffeneAnfrageByIdAsync(
            query.AnfrageId,
            cancellationToken);

        if (anfrage is null)
        {
            _logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(ReadOffenePasswortResetAnfrageDetailService), "Anfrage nicht gefunden oder bereits bearbeitet");
            return ReadOffenePasswortResetAnfrageDetailResult.Fehler(
                "Die offene Passwort-Reset-Anfrage wurde nicht gefunden oder wurde bereits bearbeitet.");
        }

        var dto = new OffenePasswortResetAnfrageDetailDto(
            anfrage.Id,
            anfrage.UserId,
            anfrage.Benutzername,
            anfrage.Vorname,
            anfrage.Nachname,
            anfrage.Bereich.ToApplication(),
            anfrage.AngefordertAm);

        return ReadOffenePasswortResetAnfrageDetailResult.Erfolg(dto);
    }
}