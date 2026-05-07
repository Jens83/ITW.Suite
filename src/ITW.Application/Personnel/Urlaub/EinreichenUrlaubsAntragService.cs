using ITW.Application.Aktivitaet;
using ITW.Application.Aufgaben;
using ITW.Application.Organisation.Contracts;
using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Entities;
using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.Urlaub;

public sealed record EinreichenUrlaubsAntragCommand(
    string UserId,
    string AntragstellerAnzeigeName,
    DateOnly Von,
    DateOnly Bis,
    string? Notiz);

public sealed class EinreichenUrlaubsAntragResult
{
    private EinreichenUrlaubsAntragResult(bool isSuccess, string? errorMessage, Guid? zeitraumId = null)
    {
        IsSuccess   = isSuccess;
        ErrorMessage = errorMessage;
        ZeitraumId  = zeitraumId;
    }

    public bool    IsSuccess    { get; }
    public string? ErrorMessage { get; }
    public Guid?   ZeitraumId  { get; }

    public static EinreichenUrlaubsAntragResult Erfolg(Guid zeitraumId)
        => new(true, null, zeitraumId);

    public static EinreichenUrlaubsAntragResult Fehler(string errorMessage)
        => new(false, errorMessage);
}

public sealed class EinreichenUrlaubsAntragService
{
    private readonly IMitarbeiterUrlaubszeitraumRepository _urlaubszeitraumRepository;
    private readonly IAufgabeRepository _aufgabenRepository;
    private readonly IAktivitaetsLogRepository _aktivitaetsLog;

    public EinreichenUrlaubsAntragService(
        IMitarbeiterUrlaubszeitraumRepository urlaubszeitraumRepository,
        IAufgabeRepository aufgabenRepository,
        IAktivitaetsLogRepository aktivitaetsLog)
    {
        ArgumentNullException.ThrowIfNull(urlaubszeitraumRepository);
        _urlaubszeitraumRepository = urlaubszeitraumRepository;

        ArgumentNullException.ThrowIfNull(aufgabenRepository);
        _aufgabenRepository = aufgabenRepository;

        ArgumentNullException.ThrowIfNull(aktivitaetsLog);
        _aktivitaetsLog = aktivitaetsLog;
    }

    public async Task<EinreichenUrlaubsAntragResult> ExecuteAsync(
        EinreichenUrlaubsAntragCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.UserId))
            return EinreichenUrlaubsAntragResult.Fehler("Der aktuelle Benutzer konnte nicht ermittelt werden.");

        if (command.Von > command.Bis)
            return EinreichenUrlaubsAntragResult.Fehler("Das Startdatum darf nicht nach dem Enddatum liegen.");

        if (command.Von < DateOnly.FromDateTime(DateTime.Today))
            return EinreichenUrlaubsAntragResult.Fehler("Urlaubsanträge können nicht in der Vergangenheit gestellt werden.");

        var hatUeberschneidung = await _urlaubszeitraumRepository.HatUeberschneidungAsync(
            command.UserId, command.Von, command.Bis, null, cancellationToken);

        if (hatUeberschneidung)
            return EinreichenUrlaubsAntragResult.Fehler("Im gewählten Zeitraum liegt bereits ein Urlaubsantrag oder genehmigter Urlaub.");

        var jetzt      = DateTimeOffset.UtcNow;
        var zeitraumId = Guid.NewGuid();

        var zeitraum = new MitarbeiterUrlaubszeitraum
        {
            Id                   = zeitraumId,
            UserId               = command.UserId.Trim(),
            Von                  = command.Von,
            Bis                  = command.Bis,
            Notiz                = string.IsNullOrWhiteSpace(command.Notiz) ? null : command.Notiz.Trim(),
            IstAktiv             = true,
            ErstelltAmUtc        = jetzt,
            AktualisiertAmUtc    = jetzt,
            Status               = UrlaubszeitraumStatus.Ausstehend,
            EingereichtVonUserId = command.UserId.Trim()
        };

        await _urlaubszeitraumRepository.AddOrUpdateAsync(zeitraum, cancellationToken);

        // Aufgabe für Wachleiter
        var aufgabe = new Aufgabe(
            Guid.NewGuid(),
            OrganisationsbereichCode.Intensivtransport,
            $"Urlaub von {command.AntragstellerAnzeigeName} genehmigen",
            AufgabePrioritaet.Hoch,
            AufgabeQuelle.System,
            command.Von,
            $"itw:urlaub-genehmigen:{zeitraumId}",
            jetzt);

        await _aufgabenRepository.AddAsync(aufgabe, cancellationToken);
        await _aufgabenRepository.SaveChangesAsync(cancellationToken);

        // Aktivitätslog
        var logText = $"{command.AntragstellerAnzeigeName} hat Urlaub eingereicht "
            + $"({command.Von:dd.MM.} – {command.Bis:dd.MM.yyyy})";

        var logEintrag = new AktivitaetsEintrag(
            Guid.NewGuid(),
            OrganisationsbereichCode.Intensivtransport,
            logText,
            AktivitaetsKategorie.Info,
            "bi bi-calendar-plus",
            jetzt);

        await _aktivitaetsLog.AddAsync(logEintrag, cancellationToken);
        await _aktivitaetsLog.SaveChangesAsync(cancellationToken);

        return EinreichenUrlaubsAntragResult.Erfolg(zeitraumId);
    }
}
