using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Planung;

public enum AutomatischePlanungAusfuehrungsmodus
{
    NurOffeneSlotsFuellen = 1,
    KomplettePeriodeNeuBerechnen = 2
}

public sealed record GenerateAutomatischePlanungCommand(
    Guid DienstplanPeriodeId,
    string BearbeitetVonUserId,
    AutomatischePlanungAusfuehrungsmodus Modus);

public sealed class GenerateAutomatischePlanungResult
{
    private GenerateAutomatischePlanungResult(
        bool isSuccess,
        string? errorMessage,
        int anzahlGespeicherteTage,
        int anzahlKonflikttage,
        int anzahlTageMitOffenenSlots)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        AnzahlGespeicherteTage = anzahlGespeicherteTage;
        AnzahlKonflikttage = anzahlKonflikttage;
        AnzahlTageMitOffenenSlots = anzahlTageMitOffenenSlots;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public int AnzahlGespeicherteTage { get; }

    public int AnzahlKonflikttage { get; }

    public int AnzahlTageMitOffenenSlots { get; }

    public static GenerateAutomatischePlanungResult Erfolg(
        int anzahlGespeicherteTage,
        int anzahlKonflikttage,
        int anzahlTageMitOffenenSlots)
        => new(
            true,
            null,
            anzahlGespeicherteTage,
            anzahlKonflikttage,
            anzahlTageMitOffenenSlots);

    public static GenerateAutomatischePlanungResult Fehler(string message)
        => new(
            false,
            message,
            0,
            0,
            0);
}

public sealed class GenerateAutomatischePlanungService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IDienstplanMitarbeiterPlanungsRepository _dienstplanMitarbeiterPlanungsRepository;
    private readonly IDienstwunschRepository _dienstwunschRepository;
    private readonly IFreelancerMonatswunschRepository _freelancerMonatswunschRepository;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstbesetzungsAusfallRepository _dienstbesetzungsAusfallRepository;
    private readonly IDienstplanUrlaubszeitraumRepository _dienstplanUrlaubszeitraumRepository;
    private readonly IAutoplanLernereignisRepository? _autoplanLernereignisRepository;

    public GenerateAutomatischePlanungService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IDienstplanMitarbeiterPlanungsRepository dienstplanMitarbeiterPlanungsRepository,
        IDienstwunschRepository dienstwunschRepository,
        IFreelancerMonatswunschRepository freelancerMonatswunschRepository,
        IGeplanterDienstTagRepository geplanterDienstTagRepository,
        IDienstbesetzungsAusfallRepository dienstbesetzungsAusfallRepository,
        IDienstplanUrlaubszeitraumRepository dienstplanUrlaubszeitraumRepository,
        IAutoplanLernereignisRepository? autoplanLernereignisRepository = null)
    {
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository
            ?? throw new ArgumentNullException(nameof(dienstplanPeriodeRepository));

        _dienstplanMitarbeiterPlanungsRepository = dienstplanMitarbeiterPlanungsRepository
            ?? throw new ArgumentNullException(nameof(dienstplanMitarbeiterPlanungsRepository));

        _dienstwunschRepository = dienstwunschRepository
            ?? throw new ArgumentNullException(nameof(dienstwunschRepository));

        _freelancerMonatswunschRepository = freelancerMonatswunschRepository
            ?? throw new ArgumentNullException(nameof(freelancerMonatswunschRepository));

        _geplanterDienstTagRepository = geplanterDienstTagRepository
            ?? throw new ArgumentNullException(nameof(geplanterDienstTagRepository));

        _dienstbesetzungsAusfallRepository = dienstbesetzungsAusfallRepository
            ?? throw new ArgumentNullException(nameof(dienstbesetzungsAusfallRepository));

        _dienstplanUrlaubszeitraumRepository = dienstplanUrlaubszeitraumRepository
            ?? throw new ArgumentNullException(nameof(dienstplanUrlaubszeitraumRepository));

        _autoplanLernereignisRepository = autoplanLernereignisRepository;
    }

    public async Task<GenerateAutomatischePlanungResult> ExecuteAsync(
        GenerateAutomatischePlanungCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.DienstplanPeriodeId == Guid.Empty)
        {
            return GenerateAutomatischePlanungResult.Fehler("Die Dienstplanperiode ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(command.BearbeitetVonUserId))
        {
            return GenerateAutomatischePlanungResult.Fehler("Der bearbeitende Benutzer ist ungültig.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            command.DienstplanPeriodeId,
            cancellationToken);

        if (periode is null)
        {
            return GenerateAutomatischePlanungResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        var bestehendeAusfaelle = await _dienstbesetzungsAusfallRepository.GetAlleFuerPeriodeAsync(
            command.DienstplanPeriodeId,
            cancellationToken);

        if (bestehendeAusfaelle.Count > 0)
        {
            return GenerateAutomatischePlanungResult.Fehler(
                "Für diese Periode bestehen bereits Krank-, Urlaubs- oder Vertretungseinträge. Bitte bereinigen Sie diese zuerst oder planen Sie die betroffenen Tage manuell.");
        }

        var vorschauService = new ReadAutomatischePlanungsvorschauService(
            _dienstplanPeriodeRepository,
            _dienstplanMitarbeiterPlanungsRepository,
            _dienstwunschRepository,
            _freelancerMonatswunschRepository,
            _geplanterDienstTagRepository,
            _dienstplanUrlaubszeitraumRepository,
            _autoplanLernereignisRepository);

        var vorschauResult = await vorschauService.ExecuteAsync(
            command.DienstplanPeriodeId,
            command.Modus,
            cancellationToken);

        if (!vorschauResult.IsSuccess)
        {
            return GenerateAutomatischePlanungResult.Fehler(
                vorschauResult.ErrorMessage ?? "Der Autoplan konnte nicht berechnet werden.");
        }

        var bestehendeTage = await _geplanterDienstTagRepository.GetAlleFuerPeriodeAsync(
            command.DienstplanPeriodeId,
            cancellationToken);

        var bestehendeTageLookup = bestehendeTage.ToDictionary(x => x.DienstDatum);
        var bestaetigungsLernereignisseGeschrieben = false;

        foreach (var tag in vorschauResult.Tage.OrderBy(x => x.Datum))
        {
            var arztUserId = NormalisiereUserId(tag.ArztUserId);
            var nfs1UserId = NormalisiereUserId(tag.Notfallsanitaeter1UserId);
            var nfs2UserId = NormalisiereUserId(tag.Notfallsanitaeter2UserId);
            var allesLeer = string.IsNullOrWhiteSpace(arztUserId)
                            && string.IsNullOrWhiteSpace(nfs1UserId)
                            && string.IsNullOrWhiteSpace(nfs2UserId);

            var bearbeitetAm = DateTimeOffset.UtcNow;

            if (bestehendeTageLookup.TryGetValue(tag.Datum, out var bestehenderTag))
            {
                var vorherigerArztUserId = bestehenderTag.ArztUserId;
                var vorherigerNfs1UserId = bestehenderTag.Notfallsanitaeter1UserId;
                var vorherigerNfs2UserId = bestehenderTag.Notfallsanitaeter2UserId;

                if (command.Modus == AutomatischePlanungAusfuehrungsmodus.KomplettePeriodeNeuBerechnen && allesLeer)
                {
                    _geplanterDienstTagRepository.Remove(bestehenderTag);
                    continue;
                }

                if (allesLeer)
                {
                    continue;
                }

                try
                {
                    bestehenderTag.AktualisiereBesatzung(
                        arztUserId,
                        nfs1UserId,
                        nfs2UserId,
                        command.BearbeitetVonUserId,
                        bearbeitetAm);
                }
                catch (ArgumentException ex)
                {
                    return GenerateAutomatischePlanungResult.Fehler(ex.Message);
                }

                bestaetigungsLernereignisseGeschrieben =
                    await SchreibeAutoplanBestaetigungsLernereignisseAsync(
                        command.DienstplanPeriodeId,
                        tag,
                        vorherigerArztUserId,
                        vorherigerNfs1UserId,
                        vorherigerNfs2UserId,
                        command.BearbeitetVonUserId,
                        bearbeitetAm,
                        cancellationToken)
                    || bestaetigungsLernereignisseGeschrieben;

                continue;
            }

            if (allesLeer)
            {
                continue;
            }

            try
            {
                var neuerTag = new GeplanterDienstTag(
                    Guid.NewGuid(),
                    command.DienstplanPeriodeId,
                    tag.Datum,
                    arztUserId,
                    nfs1UserId,
                    nfs2UserId,
                    command.BearbeitetVonUserId,
                    bearbeitetAm);

                await _geplanterDienstTagRepository.AddAsync(neuerTag, cancellationToken);
            }
            catch (ArgumentException ex)
            {
                return GenerateAutomatischePlanungResult.Fehler(ex.Message);
            }

            bestaetigungsLernereignisseGeschrieben =
                await SchreibeAutoplanBestaetigungsLernereignisseAsync(
                    command.DienstplanPeriodeId,
                    tag,
                    null,
                    null,
                    null,
                    command.BearbeitetVonUserId,
                    bearbeitetAm,
                    cancellationToken)
                || bestaetigungsLernereignisseGeschrieben;
        }

        await _geplanterDienstTagRepository.SaveChangesAsync(cancellationToken);

        if (bestaetigungsLernereignisseGeschrieben && _autoplanLernereignisRepository is not null)
        {
            await _autoplanLernereignisRepository.SaveChangesAsync(cancellationToken);
        }

        var gespeicherteTage = vorschauResult.Tage.Count(x =>
            !string.IsNullOrWhiteSpace(x.ArztUserId) ||
            !string.IsNullOrWhiteSpace(x.Notfallsanitaeter1UserId) ||
            !string.IsNullOrWhiteSpace(x.Notfallsanitaeter2UserId));

        var konfliktTage = vorschauResult.Tage.Count(x => x.HatKonflikt);

        var tageMitOffenenSlots = vorschauResult.Tage.Count(x =>
            string.IsNullOrWhiteSpace(x.ArztUserId) ||
            string.IsNullOrWhiteSpace(x.Notfallsanitaeter1UserId) ||
            string.IsNullOrWhiteSpace(x.Notfallsanitaeter2UserId));

        return GenerateAutomatischePlanungResult.Erfolg(
            gespeicherteTage,
            konfliktTage,
            tageMitOffenenSlots);
    }

    private async Task<bool> SchreibeAutoplanBestaetigungsLernereignisseAsync(
        Guid dienstplanPeriodeId,
        AutomatischePlanungsvorschauTag tag,
        string? vorherigerArztUserId,
        string? vorherigerNfs1UserId,
        string? vorherigerNfs2UserId,
        string bearbeitetVonUserId,
        DateTimeOffset bearbeitetAm,
        CancellationToken cancellationToken)
    {
        if (_autoplanLernereignisRepository is null)
        {
            return false;
        }

        var normalisierterArztUserId = NormalisiereUserId(tag.ArztUserId);
        var normalisierterNfs1UserId = NormalisiereUserId(tag.Notfallsanitaeter1UserId);
        var normalisierterNfs2UserId = NormalisiereUserId(tag.Notfallsanitaeter2UserId);

        var entscheidungLookup = tag.SlotEntscheidungen
            .Select(x => new
            {
                SlotCode = ErmittleBesetzungsSlotCode(x.SlotBezeichnung),
                Entscheidung = x
            })
            .Where(x => x.SlotCode.HasValue)
            .ToDictionary(
                x => x.SlotCode!.Value,
                x => x.Entscheidung);

        var hatGeschrieben = false;

        hatGeschrieben = await SchreibeAutoplanBestaetigungsLernereignisFallsPassendAsync(
            dienstplanPeriodeId,
            tag.Datum,
            DienstbesetzungsSlotCode.Arzt,
            entscheidungLookup,
            vorherigerArztUserId,
            normalisierterArztUserId,
            normalisierterArztUserId,
            normalisierterNfs1UserId,
            normalisierterNfs2UserId,
            bearbeitetVonUserId,
            bearbeitetAm,
            cancellationToken) || hatGeschrieben;

        hatGeschrieben = await SchreibeAutoplanBestaetigungsLernereignisFallsPassendAsync(
            dienstplanPeriodeId,
            tag.Datum,
            DienstbesetzungsSlotCode.Notfallsanitaeter1,
            entscheidungLookup,
            vorherigerNfs1UserId,
            normalisierterNfs1UserId,
            normalisierterArztUserId,
            normalisierterNfs1UserId,
            normalisierterNfs2UserId,
            bearbeitetVonUserId,
            bearbeitetAm,
            cancellationToken) || hatGeschrieben;

        hatGeschrieben = await SchreibeAutoplanBestaetigungsLernereignisFallsPassendAsync(
            dienstplanPeriodeId,
            tag.Datum,
            DienstbesetzungsSlotCode.Notfallsanitaeter2,
            entscheidungLookup,
            vorherigerNfs2UserId,
            normalisierterNfs2UserId,
            normalisierterArztUserId,
            normalisierterNfs1UserId,
            normalisierterNfs2UserId,
            bearbeitetVonUserId,
            bearbeitetAm,
            cancellationToken) || hatGeschrieben;

        return hatGeschrieben;
    }

    private async Task<bool> SchreibeAutoplanBestaetigungsLernereignisFallsPassendAsync(
        Guid dienstplanPeriodeId,
        DateOnly dienstDatum,
        DienstbesetzungsSlotCode besetzungsSlotCode,
        IReadOnlyDictionary<DienstbesetzungsSlotCode, AutomatischePlanungSlotEntscheidung> entscheidungLookup,
        string? vorherigeUserId,
        string? neueUserId,
        string? kontextArztUserId,
        string? kontextNotfallsanitaeter1UserId,
        string? kontextNotfallsanitaeter2UserId,
        string bearbeitetVonUserId,
        DateTimeOffset bearbeitetAm,
        CancellationToken cancellationToken)
    {
        if (_autoplanLernereignisRepository is null)
        {
            return false;
        }

        if (!entscheidungLookup.TryGetValue(besetzungsSlotCode, out var entscheidung))
        {
            return false;
        }

        if (!IstBestaetigbareAutoplanEntscheidung(entscheidung, neueUserId))
        {
            return false;
        }

        var normalisierteVorherigeUserId = NormalisiereUserId(vorherigeUserId);
        var normalisierteNeueUserId = NormalisiereUserId(neueUserId);

        if (string.Equals(normalisierteVorherigeUserId, normalisierteNeueUserId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var lernereignis = new AutoplanLernereignis(
            Guid.NewGuid(),
            dienstplanPeriodeId,
            dienstDatum,
            besetzungsSlotCode,
            AutoplanLernereignisTypCode.AutoplanVorschlagBestaetigt,
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

        return true;
    }

    private static bool IstBestaetigbareAutoplanEntscheidung(
        AutomatischePlanungSlotEntscheidung entscheidung,
        string? neueUserId)
    {
        if (string.IsNullOrWhiteSpace(neueUserId))
        {
            return false;
        }

        if (entscheidung.Art == AutomatischePlanungZuweisungsArt.BestehendePlanung)
        {
            return false;
        }

        return entscheidung.AenderungsArt is
            AutomatischePlanungAenderungsArt.NeuGesetzt or
            AutomatischePlanungAenderungsArt.Ersetzt;
    }

    private static DienstbesetzungsSlotCode? ErmittleBesetzungsSlotCode(string slotBezeichnung)
    {
        return slotBezeichnung switch
        {
            "Arzt" => DienstbesetzungsSlotCode.Arzt,
            "NFS 1" => DienstbesetzungsSlotCode.Notfallsanitaeter1,
            "NFS 2" => DienstbesetzungsSlotCode.Notfallsanitaeter2,
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