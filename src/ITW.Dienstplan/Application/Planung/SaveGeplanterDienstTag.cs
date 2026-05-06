using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Application.Kalender;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Planung;

public sealed record SaveGeplanterDienstTagCommand(
    Guid DienstplanPeriodeId,
    DateOnly DienstDatum,
    string? ArztUserId,
    string? Notfallsanitaeter1UserId,
    string? Notfallsanitaeter2UserId,
    string BearbeitetVonUserId);

public sealed class SaveGeplanterDienstTagResult
{
    private SaveGeplanterDienstTagResult(
        bool isSuccess,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public static SaveGeplanterDienstTagResult Erfolg()
        => new(true, null);

    public static SaveGeplanterDienstTagResult Fehler(string message)
        => new(false, message);
}

public sealed class SaveGeplanterDienstTagService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstbesetzungsAusfallRepository _dienstbesetzungsAusfallRepository;
    private readonly IAutoplanLernereignisRepository _autoplanLernereignisRepository;

    public SaveGeplanterDienstTagService(
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

    public async Task<SaveGeplanterDienstTagResult> ExecuteAsync(
        SaveGeplanterDienstTagCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.DienstplanPeriodeId == Guid.Empty)
        {
            return SaveGeplanterDienstTagResult.Fehler("Die Dienstplanperiode ist erforderlich.");
        }

        if (string.IsNullOrWhiteSpace(command.BearbeitetVonUserId))
        {
            return SaveGeplanterDienstTagResult.Fehler("Die UserId des Bearbeiters ist erforderlich.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            command.DienstplanPeriodeId,
            cancellationToken);

        if (periode is null)
        {
            return SaveGeplanterDienstTagResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        if (command.DienstDatum.Year != periode.Jahr || command.DienstDatum.Month != periode.Monat)
        {
            return SaveGeplanterDienstTagResult.Fehler("Der ausgewählte Tag gehört nicht zur ausgewählten Dienstplanperiode.");
        }

        var bearbeitetAm = DateTimeOffset.UtcNow;

        var vorhandenerEintrag = await _geplanterDienstTagRepository.GetAsync(
            command.DienstplanPeriodeId,
            command.DienstDatum,
            cancellationToken);

        var vorhandeneAusfaelle = await _dienstbesetzungsAusfallRepository.GetAlleFuerTagAsync(
            command.DienstplanPeriodeId,
            command.DienstDatum,
            cancellationToken);

        var ausfallLookup = vorhandeneAusfaelle.ToDictionary(
            x => x.BesetzungsSlotCode,
            x => x);

        var aktuellerArztUserId = vorhandenerEintrag?.ArztUserId;
        var aktuellerNfs1UserId = vorhandenerEintrag?.Notfallsanitaeter1UserId;
        var aktuellerNfs2UserId = vorhandenerEintrag?.Notfallsanitaeter2UserId;

        var arztUserId = NormalisiereUserId(command.ArztUserId);
        var nfs1UserId = NormalisiereUserId(command.Notfallsanitaeter1UserId);
        var nfs2UserId = NormalisiereUserId(command.Notfallsanitaeter2UserId);

        var slotFehler = ErmittleSlotschutzFehler(
            ausfallLookup,
            aktuellerArztUserId,
            aktuellerNfs1UserId,
            aktuellerNfs2UserId,
            arztUserId,
            nfs1UserId,
            nfs2UserId);

        if (!string.IsNullOrWhiteSpace(slotFehler))
        {
            return SaveGeplanterDienstTagResult.Fehler(slotFehler);
        }

        var finaleArztUserId = HatAusfallImSlot(ausfallLookup, DienstbesetzungsSlotCode.Arzt)
            ? aktuellerArztUserId
            : arztUserId;

        var finaleNfs1UserId = HatAusfallImSlot(ausfallLookup, DienstbesetzungsSlotCode.Notfallsanitaeter1)
            ? aktuellerNfs1UserId
            : nfs1UserId;

        var finaleNfs2UserId = HatAusfallImSlot(ausfallLookup, DienstbesetzungsSlotCode.Notfallsanitaeter2)
            ? aktuellerNfs2UserId
            : nfs2UserId;

        var vorherigerArztUserId = aktuellerArztUserId;
        var vorherigerNfs1UserId = aktuellerNfs1UserId;
        var vorherigerNfs2UserId = aktuellerNfs2UserId;

        var allesLeer = string.IsNullOrWhiteSpace(finaleArztUserId)
                        && string.IsNullOrWhiteSpace(finaleNfs1UserId)
                        && string.IsNullOrWhiteSpace(finaleNfs2UserId);

        if (allesLeer)
        {
            if (vorhandenerEintrag is not null)
            {
                await SchreibeGrundbesetzungsLernereignisseAsync(
                    command.DienstplanPeriodeId,
                    command.DienstDatum,
                    vorherigerArztUserId,
                    null,
                    vorherigerNfs1UserId,
                    null,
                    vorherigerNfs2UserId,
                    null,
                    command.BearbeitetVonUserId,
                    bearbeitetAm,
                    cancellationToken);

                _geplanterDienstTagRepository.Remove(vorhandenerEintrag);
                await _geplanterDienstTagRepository.SaveChangesAsync(cancellationToken);
            }

            return SaveGeplanterDienstTagResult.Erfolg();
        }

        try
        {
            if (vorhandenerEintrag is null)
            {
                var neuerEintrag = new GeplanterDienstTag(
                    Guid.NewGuid(),
                    command.DienstplanPeriodeId,
                    command.DienstDatum,
                    finaleArztUserId,
                    finaleNfs1UserId,
                    finaleNfs2UserId,
                    command.BearbeitetVonUserId,
                    bearbeitetAm);

                await _geplanterDienstTagRepository.AddAsync(neuerEintrag, cancellationToken);
            }
            else
            {
                vorhandenerEintrag.AktualisiereBesatzung(
                    finaleArztUserId,
                    finaleNfs1UserId,
                    finaleNfs2UserId,
                    command.BearbeitetVonUserId,
                    bearbeitetAm);
            }
        }
        catch (ArgumentException ex)
        {
            return SaveGeplanterDienstTagResult.Fehler(ex.Message);
        }

        await SchreibeGrundbesetzungsLernereignisseAsync(
            command.DienstplanPeriodeId,
            command.DienstDatum,
            vorherigerArztUserId,
            finaleArztUserId,
            vorherigerNfs1UserId,
            finaleNfs1UserId,
            vorherigerNfs2UserId,
            finaleNfs2UserId,
            command.BearbeitetVonUserId,
            bearbeitetAm,
            cancellationToken);

        await _geplanterDienstTagRepository.SaveChangesAsync(cancellationToken);

        return SaveGeplanterDienstTagResult.Erfolg();
    }

    private static string? ErmittleSlotschutzFehler(
        IReadOnlyDictionary<DienstbesetzungsSlotCode, GeplanterDienstTagAusfall> ausfallLookup,
        string? aktuellerArztUserId,
        string? aktuellerNfs1UserId,
        string? aktuellerNfs2UserId,
        string? angeforderterArztUserId,
        string? angeforderterNfs1UserId,
        string? angeforderterNfs2UserId)
    {
        var gesperrteSlots = new List<string>();

        PruefeSlot(
            DienstbesetzungsSlotCode.Arzt,
            "Arzt",
            aktuellerArztUserId,
            angeforderterArztUserId);

        PruefeSlot(
            DienstbesetzungsSlotCode.Notfallsanitaeter1,
            "NFS 1",
            aktuellerNfs1UserId,
            angeforderterNfs1UserId);

        PruefeSlot(
            DienstbesetzungsSlotCode.Notfallsanitaeter2,
            "NFS 2",
            aktuellerNfs2UserId,
            angeforderterNfs2UserId);

        if (gesperrteSlots.Count == 0)
        {
            return null;
        }

        if (gesperrteSlots.Count == 1)
        {
            return $"Im Slot {gesperrteSlots[0]} liegt bereits ein Krank-/Urlaubs- oder Vertretungseintrag vor. Bitte ändern Sie diesen Slot unten im Bereich „Ausfall & Vertretung“.";
        }

        return $"In den Slots {string.Join(", ", gesperrteSlots)} liegen bereits Krank-/Urlaubs- oder Vertretungseinträge vor. Bitte ändern Sie diese Slots unten im Bereich „Ausfall & Vertretung“.";

        void PruefeSlot(
            DienstbesetzungsSlotCode slotCode,
            string slotBezeichnung,
            string? aktuelleUserId,
            string? angeforderteUserId)
        {
            if (!ausfallLookup.ContainsKey(slotCode))
            {
                return;
            }

            var normalisierteAktuelleUserId = NormalisiereUserId(aktuelleUserId);
            var normalisierteAngeforderteUserId = NormalisiereUserId(angeforderteUserId);

            if (!string.Equals(normalisierteAktuelleUserId, normalisierteAngeforderteUserId, StringComparison.OrdinalIgnoreCase))
            {
                gesperrteSlots.Add(slotBezeichnung);
            }
        }
    }

    private static bool HatAusfallImSlot(
        IReadOnlyDictionary<DienstbesetzungsSlotCode, GeplanterDienstTagAusfall> ausfallLookup,
        DienstbesetzungsSlotCode slotCode)
    {
        return ausfallLookup.ContainsKey(slotCode);
    }

    private async Task SchreibeGrundbesetzungsLernereignisseAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        string? vorherigerArztUserId,
        string? neuerArztUserId,
        string? vorherigerNfs1UserId,
        string? neuerNfs1UserId,
        string? vorherigerNfs2UserId,
        string? neuerNfs2UserId,
        string bearbeitetVonUserId,
        DateTimeOffset bearbeitetAm,
        CancellationToken cancellationToken)
    {
        var normalisierterArztUserId = NormalisiereUserId(neuerArztUserId);
        var normalisierterNfs1UserId = NormalisiereUserId(neuerNfs1UserId);
        var normalisierterNfs2UserId = NormalisiereUserId(neuerNfs2UserId);

        await SchreibeGrundbesetzungsLernereignisFallsGeaendertAsync(
            dienstplanPeriodeId,
            dienstDatum,
            DienstbesetzungsSlotCode.Arzt,
            vorherigerArztUserId,
            normalisierterArztUserId,
            normalisierterArztUserId,
            normalisierterNfs1UserId,
            normalisierterNfs2UserId,
            bearbeitetVonUserId,
            bearbeitetAm,
            cancellationToken);

        await SchreibeGrundbesetzungsLernereignisFallsGeaendertAsync(
            dienstplanPeriodeId,
            dienstDatum,
            DienstbesetzungsSlotCode.Notfallsanitaeter1,
            vorherigerNfs1UserId,
            normalisierterNfs1UserId,
            normalisierterArztUserId,
            normalisierterNfs1UserId,
            normalisierterNfs2UserId,
            bearbeitetVonUserId,
            bearbeitetAm,
            cancellationToken);

        await SchreibeGrundbesetzungsLernereignisFallsGeaendertAsync(
            dienstplanPeriodeId,
            dienstDatum,
            DienstbesetzungsSlotCode.Notfallsanitaeter2,
            vorherigerNfs2UserId,
            normalisierterNfs2UserId,
            normalisierterArztUserId,
            normalisierterNfs1UserId,
            normalisierterNfs2UserId,
            bearbeitetVonUserId,
            bearbeitetAm,
            cancellationToken);
    }

    private async Task SchreibeGrundbesetzungsLernereignisFallsGeaendertAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        DienstbesetzungsSlotCode besetzungsSlotCode,
        string? vorherigeUserId,
        string? neueUserId,
        string? kontextArztUserId,
        string? kontextNotfallsanitaeter1UserId,
        string? kontextNotfallsanitaeter2UserId,
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
            AutoplanLernereignisTypCode.GrundbesetzungManuellGeaendert,
            normalisierteVorherigeUserId,
            normalisierteNeueUserId,
            null,
            kontextArztUserId,
            kontextNotfallsanitaeter1UserId,
            kontextNotfallsanitaeter2UserId,
            null,
            bearbeitetVonUserId,
            bearbeitetAm);

        await _autoplanLernereignisRepository.AddAsync(lernereignis, cancellationToken);
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