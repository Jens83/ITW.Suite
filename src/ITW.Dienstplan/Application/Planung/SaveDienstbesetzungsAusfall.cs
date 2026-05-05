using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Kalender;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Planung;

public sealed record SaveDienstbesetzungsAusfallCommand(
    Guid DienstplanPeriodeId,
    DateOnly DienstDatum,
    DienstbesetzungsSlotCode BesetzungsSlotCode,
    DienstausfallGrundCode? AusfallGrundCode,
    string? VertretungsUserId,
    string BearbeitetVonUserId);

public sealed class SaveDienstbesetzungsAusfallResult
{
    private SaveDienstbesetzungsAusfallResult(bool isSuccess, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveDienstbesetzungsAusfallResult Erfolg()
        => new(true, null);

    public static SaveDienstbesetzungsAusfallResult Fehler(string message)
        => new(false, message);
}

public sealed class SaveDienstbesetzungsAusfallService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstbesetzungsAusfallRepository _dienstbesetzungsAusfallRepository;
    private readonly IAutoplanLernereignisRepository _autoplanLernereignisRepository;

    public SaveDienstbesetzungsAusfallService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IGeplanterDienstTagRepository geplanterDienstTagRepository,
        IDienstbesetzungsAusfallRepository dienstbesetzungsAusfallRepository,
        IAutoplanLernereignisRepository autoplanLernereignisRepository)
    {
        ArgumentNullException.ThrowIfNull(dienstplanPeriodeRepository);
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository;

        ArgumentNullException.ThrowIfNull(geplanterDienstTagRepository);
        _geplanterDienstTagRepository = geplanterDienstTagRepository;

        ArgumentNullException.ThrowIfNull(dienstbesetzungsAusfallRepository);
        _dienstbesetzungsAusfallRepository = dienstbesetzungsAusfallRepository;

        ArgumentNullException.ThrowIfNull(autoplanLernereignisRepository);
        _autoplanLernereignisRepository = autoplanLernereignisRepository;
    }

    public async Task<SaveDienstbesetzungsAusfallResult> ExecuteAsync(
        SaveDienstbesetzungsAusfallCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.DienstplanPeriodeId == Guid.Empty)
        {
            return SaveDienstbesetzungsAusfallResult.Fehler("Die Dienstplanperiode ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.BearbeitetVonUserId))
        {
            return SaveDienstbesetzungsAusfallResult.Fehler("Die UserId des Bearbeiters ist erforderlich.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            command.DienstplanPeriodeId,
            cancellationToken);

        if (periode is null)
        {
            return SaveDienstbesetzungsAusfallResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        if (command.DienstDatum.Year != periode.Jahr || command.DienstDatum.Month != periode.Monat)
        {
            return SaveDienstbesetzungsAusfallResult.Fehler("Der ausgewählte Tag gehört nicht zur ausgewählten Dienstplanperiode.");
        }

        if (command.DienstDatum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return SaveDienstbesetzungsAusfallResult.Fehler("Für Wochenenden können keine Krank- oder Urlaubseinträge gespeichert werden.");
        }

        if (MecklenburgVorpommernFeiertage.TryGetFeiertagsname(command.DienstDatum, out var feiertagsname))
        {
            return SaveDienstbesetzungsAusfallResult.Fehler(
                $"Für Feiertage können keine Krank- oder Urlaubseinträge gespeichert werden ({feiertagsname}).");
        }

        var geplanterDienstTag = await _geplanterDienstTagRepository.GetAsync(
            command.DienstplanPeriodeId,
            command.DienstDatum,
            cancellationToken);

        if (geplanterDienstTag is null)
        {
            return SaveDienstbesetzungsAusfallResult.Fehler("Für diesen Tag existiert noch keine Besetzung.");
        }

        var vorhandenerAusfall = await _dienstbesetzungsAusfallRepository.GetAsync(
            command.DienstplanPeriodeId,
            command.DienstDatum,
            command.BesetzungsSlotCode,
            cancellationToken);

        var aktuellImSlotGeplant = BestimmeUserIdFuerSlot(
            geplanterDienstTag,
            command.BesetzungsSlotCode);

        var urspruenglichGeplanterUserId = vorhandenerAusfall?.UrspruenglichGeplanterUserId
                                           ?? aktuellImSlotGeplant;

        if (string.IsNullOrWhiteSpace(urspruenglichGeplanterUserId))
        {
            return SaveDienstbesetzungsAusfallResult.Fehler("Für den ausgewählten Besetzungsslot ist aktuell niemand geplant.");
        }

        var vertretungsUserId = NormalisiereUserId(command.VertretungsUserId);
        var bearbeitetAm = DateTimeOffset.UtcNow;

        if (command.AusfallGrundCode is null)
        {
            if (vorhandenerAusfall is null)
            {
                return SaveDienstbesetzungsAusfallResult.Erfolg();
            }

            try
            {
                SetzeUserIdImSlot(
                    geplanterDienstTag,
                    command.BesetzungsSlotCode,
                    vorhandenerAusfall.UrspruenglichGeplanterUserId,
                    command.BearbeitetVonUserId,
                    bearbeitetAm);
            }
            catch (ArgumentException ex)
            {
                return SaveDienstbesetzungsAusfallResult.Fehler(ex.Message);
            }

            var neueUserIdNachAenderung = BestimmeUserIdFuerSlot(
                geplanterDienstTag,
                command.BesetzungsSlotCode);

            await SchreibeVertretungsLernereignisFallsGeaendertAsync(
                command.DienstplanPeriodeId,
                command.DienstDatum,
                command.BesetzungsSlotCode,
                aktuellImSlotGeplant,
                neueUserIdNachAenderung,
                vorhandenerAusfall.UrspruenglichGeplanterUserId,
                geplanterDienstTag,
                vorhandenerAusfall.AusfallGrundCode,
                command.BearbeitetVonUserId,
                bearbeitetAm,
                cancellationToken);

            _dienstbesetzungsAusfallRepository.Remove(vorhandenerAusfall);
            await _dienstbesetzungsAusfallRepository.SaveChangesAsync(cancellationToken);

            return SaveDienstbesetzungsAusfallResult.Erfolg();
        }

        if (!string.IsNullOrWhiteSpace(vertretungsUserId) &&
            string.Equals(vertretungsUserId, urspruenglichGeplanterUserId, StringComparison.OrdinalIgnoreCase))
        {
            return SaveDienstbesetzungsAusfallResult.Fehler("Die Vertretung darf nicht identisch mit der ursprünglich geplanten Person sein.");
        }

        try
        {
            SetzeUserIdImSlot(
                geplanterDienstTag,
                command.BesetzungsSlotCode,
                vertretungsUserId,
                command.BearbeitetVonUserId,
                bearbeitetAm);
        }
        catch (ArgumentException ex)
        {
            return SaveDienstbesetzungsAusfallResult.Fehler(ex.Message);
        }

        try
        {
            if (vorhandenerAusfall is null)
            {
                var neuerAusfall = new GeplanterDienstTagAusfall(
                    Guid.NewGuid(),
                    command.DienstplanPeriodeId,
                    command.DienstDatum,
                    command.BesetzungsSlotCode,
                    urspruenglichGeplanterUserId,
                    command.AusfallGrundCode.Value,
                    vertretungsUserId,
                    command.BearbeitetVonUserId,
                    bearbeitetAm);

                await _dienstbesetzungsAusfallRepository.AddAsync(neuerAusfall, cancellationToken);
            }
            else
            {
                vorhandenerAusfall.Aktualisiere(
                    vorhandenerAusfall.UrspruenglichGeplanterUserId,
                    command.AusfallGrundCode.Value,
                    vertretungsUserId,
                    command.BearbeitetVonUserId,
                    bearbeitetAm);
            }
        }
        catch (ArgumentException ex)
        {
            return SaveDienstbesetzungsAusfallResult.Fehler(ex.Message);
        }

        var neueUserIdNachAenderungMitAusfall = BestimmeUserIdFuerSlot(
            geplanterDienstTag,
            command.BesetzungsSlotCode);

        await SchreibeVertretungsLernereignisFallsGeaendertAsync(
            command.DienstplanPeriodeId,
            command.DienstDatum,
            command.BesetzungsSlotCode,
            aktuellImSlotGeplant,
            neueUserIdNachAenderungMitAusfall,
            urspruenglichGeplanterUserId,
            geplanterDienstTag,
            command.AusfallGrundCode,
            command.BearbeitetVonUserId,
            bearbeitetAm,
            cancellationToken);

        await _dienstbesetzungsAusfallRepository.SaveChangesAsync(cancellationToken);

        return SaveDienstbesetzungsAusfallResult.Erfolg();
    }

    private async Task SchreibeVertretungsLernereignisFallsGeaendertAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        DienstbesetzungsSlotCode besetzungsSlotCode,
        string? vorherigeUserId,
        string? neueUserId,
        string? urspruenglichGeplanterUserId,
        GeplanterDienstTag geplanterDienstTag,
        DienstausfallGrundCode? ausfallGrundCode,
        string bearbeitetVonUserId,
        DateTimeOffset bearbeitetAm,
        CancellationToken cancellationToken)
    {
        var normalisierteVorherigeUserId = NormalisiereUserId(vorherigeUserId);
        var normalisierteNeueUserId = NormalisiereUserId(neueUserId);

        if (string.Equals(normalisierteVorherigeUserId, normalisierteNeueUserId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var lernereignis = new AutoplanLernereignis(
            Guid.NewGuid(),
            dienstplanPeriodeId,
            dienstDatum,
            besetzungsSlotCode,
            AutoplanLernereignisTypCode.VertretungManuellGeaendert,
            normalisierteVorherigeUserId,
            normalisierteNeueUserId,
            urspruenglichGeplanterUserId,
            geplanterDienstTag.ArztUserId,
            geplanterDienstTag.Notfallsanitaeter1UserId,
            geplanterDienstTag.Notfallsanitaeter2UserId,
            ausfallGrundCode,
            bearbeitetVonUserId,
            bearbeitetAm);

        await _autoplanLernereignisRepository.AddAsync(lernereignis, cancellationToken);
    }

    private static void SetzeUserIdImSlot(
        GeplanterDienstTag geplanterDienstTag,
        DienstbesetzungsSlotCode slotCode,
        string? neueUserId,
        string bearbeitetVonUserId,
        DateTimeOffset bearbeitetAm)
    {
        var arztUserId = geplanterDienstTag.ArztUserId;
        var nfs1UserId = geplanterDienstTag.Notfallsanitaeter1UserId;
        var nfs2UserId = geplanterDienstTag.Notfallsanitaeter2UserId;

        switch (slotCode)
        {
            case DienstbesetzungsSlotCode.Arzt:
                arztUserId = neueUserId;
                break;

            case DienstbesetzungsSlotCode.Notfallsanitaeter1:
                nfs1UserId = neueUserId;
                break;

            case DienstbesetzungsSlotCode.Notfallsanitaeter2:
                nfs2UserId = neueUserId;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(slotCode), slotCode, null);
        }

        geplanterDienstTag.AktualisiereBesatzung(
            arztUserId,
            nfs1UserId,
            nfs2UserId,
            bearbeitetVonUserId,
            bearbeitetAm);
    }

    private static string? BestimmeUserIdFuerSlot(
        GeplanterDienstTag geplanterDienstTag,
        DienstbesetzungsSlotCode slotCode)
    {
        return slotCode switch
        {
            DienstbesetzungsSlotCode.Arzt => geplanterDienstTag.ArztUserId,
            DienstbesetzungsSlotCode.Notfallsanitaeter1 => geplanterDienstTag.Notfallsanitaeter1UserId,
            DienstbesetzungsSlotCode.Notfallsanitaeter2 => geplanterDienstTag.Notfallsanitaeter2UserId,
            _ => null
        };
    }

    private static string? NormalisiereUserId(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return userId.Trim();
    }
}