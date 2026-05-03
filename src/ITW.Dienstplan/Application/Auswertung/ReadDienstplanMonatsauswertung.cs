using ITW.Dienstplan.Application.Contracts;
using ITW.Dienstplan.Domain.Entities;
using ITW.Dienstplan.Domain.Enums;

namespace ITW.Dienstplan.Application.Auswertung;

public sealed class DienstplanMonatsauswertungEintrag
{
    public string UserId { get; init; } = string.Empty;

    public int GeplanteDienste { get; init; }

    public int Krankheitstage { get; init; }

    public int Urlaubstage { get; init; }

    public int Vertretungen { get; init; }

    public int GefahreneDienste { get; init; }
}

public sealed class ReadDienstplanMonatsauswertungResult
{
    private ReadDienstplanMonatsauswertungResult(
        bool isSuccess,
        string? errorMessage,
        Guid periodeId,
        int jahr,
        int monat,
        string bezeichnung,
        IReadOnlyList<DienstplanMonatsauswertungEintrag> eintraege)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        PeriodeId = periodeId;
        Jahr = jahr;
        Monat = monat;
        Bezeichnung = bezeichnung;
        Eintraege = eintraege;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public Guid PeriodeId { get; }

    public int Jahr { get; }

    public int Monat { get; }

    public string Bezeichnung { get; }

    public IReadOnlyList<DienstplanMonatsauswertungEintrag> Eintraege { get; }

    public static ReadDienstplanMonatsauswertungResult Erfolg(
        Guid periodeId,
        int jahr,
        int monat,
        string bezeichnung,
        IReadOnlyList<DienstplanMonatsauswertungEintrag> eintraege)
        => new(
            true,
            null,
            periodeId,
            jahr,
            monat,
            bezeichnung,
            eintraege);

    public static ReadDienstplanMonatsauswertungResult Fehler(string message)
        => new(
            false,
            message,
            Guid.Empty,
            0,
            0,
            string.Empty,
            Array.Empty<DienstplanMonatsauswertungEintrag>());
}

public sealed class ReadDienstplanMonatsauswertungService
{
    private readonly IDienstplanPeriodeRepository _dienstplanPeriodeRepository;
    private readonly IGeplanterDienstTagRepository _geplanterDienstTagRepository;
    private readonly IDienstbesetzungsAusfallRepository _dienstbesetzungsAusfallRepository;

    public ReadDienstplanMonatsauswertungService(
        IDienstplanPeriodeRepository dienstplanPeriodeRepository,
        IGeplanterDienstTagRepository geplanterDienstTagRepository,
        IDienstbesetzungsAusfallRepository dienstbesetzungsAusfallRepository)
    {
        _dienstplanPeriodeRepository = dienstplanPeriodeRepository
            ?? throw new ArgumentNullException(nameof(dienstplanPeriodeRepository));

        _geplanterDienstTagRepository = geplanterDienstTagRepository
            ?? throw new ArgumentNullException(nameof(geplanterDienstTagRepository));

        _dienstbesetzungsAusfallRepository = dienstbesetzungsAusfallRepository
            ?? throw new ArgumentNullException(nameof(dienstbesetzungsAusfallRepository));
    }

    public async Task<ReadDienstplanMonatsauswertungResult> ExecuteAsync(
        Guid periodeId,
        CancellationToken cancellationToken = default)
    {
        if (periodeId == Guid.Empty)
        {
            return ReadDienstplanMonatsauswertungResult.Fehler("Die Dienstplanperiode ist ungültig.");
        }

        var periode = await _dienstplanPeriodeRepository.GetByIdAsync(
            periodeId,
            cancellationToken);

        if (periode is null)
        {
            return ReadDienstplanMonatsauswertungResult.Fehler("Die Dienstplanperiode wurde nicht gefunden.");
        }

        var geplanteDiensttage = await _geplanterDienstTagRepository.GetAlleFuerPeriodeAsync(
            periodeId,
            cancellationToken);

        var ausfaelle = await _dienstbesetzungsAusfallRepository.GetAlleFuerPeriodeAsync(
            periodeId,
            cancellationToken);

        var ausfallLookup = ausfaelle.ToDictionary(
            x => (x.DienstDatum, x.BesetzungsSlotCode),
            x => x);

        var statistik = new Dictionary<string, MutableMonatsauswertung>(StringComparer.OrdinalIgnoreCase);

        foreach (var diensttag in geplanteDiensttage)
        {
            VerarbeiteSlot(
                statistik,
                diensttag.DienstDatum,
                DienstbesetzungsSlotCode.Arzt,
                diensttag.ArztUserId,
                ausfallLookup);

            VerarbeiteSlot(
                statistik,
                diensttag.DienstDatum,
                DienstbesetzungsSlotCode.Notfallsanitaeter1,
                diensttag.Notfallsanitaeter1UserId,
                ausfallLookup);

            VerarbeiteSlot(
                statistik,
                diensttag.DienstDatum,
                DienstbesetzungsSlotCode.Notfallsanitaeter2,
                diensttag.Notfallsanitaeter2UserId,
                ausfallLookup);
        }

        var eintraege = statistik.Values
            .Select(x => new DienstplanMonatsauswertungEintrag
            {
                UserId = x.UserId,
                GeplanteDienste = x.GeplanteDienste,
                Krankheitstage = x.Krankheitstage,
                Urlaubstage = x.Urlaubstage,
                Vertretungen = x.Vertretungen,
                GefahreneDienste = x.GefahreneDienste
            })
            .OrderByDescending(x => x.GeplanteDienste)
            .ThenByDescending(x => x.GefahreneDienste)
            .ThenByDescending(x => x.Vertretungen)
            .ThenBy(x => x.UserId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return ReadDienstplanMonatsauswertungResult.Erfolg(
            periode.Id,
            periode.Jahr,
            periode.Monat,
            periode.Bezeichnung,
            eintraege);
    }

    private static void VerarbeiteSlot(
        IDictionary<string, MutableMonatsauswertung> statistik,
        DateOnly dienstDatum,
        DienstbesetzungsSlotCode slotCode,
        string? aktuelleUserId,
        IReadOnlyDictionary<(DateOnly Datum, DienstbesetzungsSlotCode SlotCode), GeplanterDienstTagAusfall> ausfallLookup)
    {
        if (ausfallLookup.TryGetValue((dienstDatum, slotCode), out var ausfall))
        {
            var original = HoleOderErzeuge(statistik, ausfall.UrspruenglichGeplanterUserId);
            original.GeplanteDienste++;

            switch (ausfall.AusfallGrundCode)
            {
                case DienstausfallGrundCode.Krankheit:
                    original.Krankheitstage++;
                    break;

                case DienstausfallGrundCode.Urlaub:
                    original.Urlaubstage++;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(ausfall.VertretungsUserId))
            {
                var vertretung = HoleOderErzeuge(statistik, ausfall.VertretungsUserId);
                vertretung.Vertretungen++;
                vertretung.GefahreneDienste++;
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(aktuelleUserId))
        {
            return;
        }

        var regulaer = HoleOderErzeuge(statistik, aktuelleUserId);
        regulaer.GeplanteDienste++;
        regulaer.GefahreneDienste++;
    }

    private static MutableMonatsauswertung HoleOderErzeuge(
        IDictionary<string, MutableMonatsauswertung> statistik,
        string userId)
    {
        if (statistik.TryGetValue(userId, out var vorhanden))
        {
            return vorhanden;
        }

        var neu = new MutableMonatsauswertung
        {
            UserId = userId
        };

        statistik[userId] = neu;
        return neu;
    }

    private sealed class MutableMonatsauswertung
    {
        public string UserId { get; set; } = string.Empty;

        public int GeplanteDienste { get; set; }

        public int Krankheitstage { get; set; }

        public int Urlaubstage { get; set; }

        public int Vertretungen { get; set; }

        public int GefahreneDienste { get; set; }
    }
}