using ITW.Application.Personnel.Urlaub.Contracts;
using ITW.Domain.Personnel.Enums;

namespace ITW.Application.Personnel.Urlaub;

public sealed class ReadMitarbeiterUrlaubsplanerQuery
{
    public string UserId { get; init; } = string.Empty;

    public int Jahr { get; init; }

    public MitarbeiterBeschaeftigungsart Beschaeftigungsart { get; init; }
}

public sealed class ReadMitarbeiterUrlaubsplanerZeitraumDto
{
    public Guid Id { get; init; }

    public DateOnly Von { get; init; }

    public DateOnly Bis { get; init; }

    public int Urlaubstage { get; init; }

    public string? Notiz { get; init; }
}

public sealed class ReadMitarbeiterUrlaubsplanerResult
{
    private ReadMitarbeiterUrlaubsplanerResult(
        bool isSuccess,
        string? errorMessage,
        string userId,
        int jahr,
        int anspruchstage,
        int uebertragstage,
        int genommeneUrlaubstage,
        int resturlaubstage,
        bool anspruchIstStandardwert,
        IReadOnlyList<ReadMitarbeiterUrlaubsplanerZeitraumDto> zeitraeume)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        UserId = userId;
        Jahr = jahr;
        Anspruchstage = anspruchstage;
        Uebertragstage = uebertragstage;
        GenommeneUrlaubstage = genommeneUrlaubstage;
        Resturlaubstage = resturlaubstage;
        AnspruchIstStandardwert = anspruchIstStandardwert;
        Zeitraeume = zeitraeume;
    }

    public bool IsSuccess { get; }

    public string? ErrorMessage { get; }

    public string UserId { get; }

    public int Jahr { get; }

    public int Anspruchstage { get; }

    public int Uebertragstage { get; }

    public int GenommeneUrlaubstage { get; }

    public int Resturlaubstage { get; }

    public bool AnspruchIstStandardwert { get; }

    public IReadOnlyList<ReadMitarbeiterUrlaubsplanerZeitraumDto> Zeitraeume { get; }

    public static ReadMitarbeiterUrlaubsplanerResult Erfolg(
        string userId,
        int jahr,
        int anspruchstage,
        int uebertragstage,
        int genommeneUrlaubstage,
        bool anspruchIstStandardwert,
        IReadOnlyList<ReadMitarbeiterUrlaubsplanerZeitraumDto> zeitraeume)
        => new(
            true,
            null,
            userId,
            jahr,
            anspruchstage,
            uebertragstage,
            genommeneUrlaubstage,
            anspruchstage + uebertragstage - genommeneUrlaubstage,
            anspruchIstStandardwert,
            zeitraeume);

    public static ReadMitarbeiterUrlaubsplanerResult Fehler(string message)
        => new(
            false,
            message,
            string.Empty,
            0,
            0,
            0,
            0,
            0,
            false,
            Array.Empty<ReadMitarbeiterUrlaubsplanerZeitraumDto>());
}

public sealed class ReadMitarbeiterUrlaubsplanerService
{
    private readonly IMitarbeiterUrlaubsanspruchRepository _urlaubsanspruchRepository;
    private readonly IMitarbeiterUrlaubszeitraumRepository _urlaubszeitraumRepository;

    public ReadMitarbeiterUrlaubsplanerService(
        IMitarbeiterUrlaubsanspruchRepository urlaubsanspruchRepository,
        IMitarbeiterUrlaubszeitraumRepository urlaubszeitraumRepository)
    {
        _urlaubsanspruchRepository = urlaubsanspruchRepository
            ?? throw new ArgumentNullException(nameof(urlaubsanspruchRepository));

        _urlaubszeitraumRepository = urlaubszeitraumRepository
            ?? throw new ArgumentNullException(nameof(urlaubszeitraumRepository));
    }

    public async Task<ReadMitarbeiterUrlaubsplanerResult> ExecuteAsync(
        ReadMitarbeiterUrlaubsplanerQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query is null)
        {
            return ReadMitarbeiterUrlaubsplanerResult.Fehler("Die Anfrage ist ungültig.");
        }

        if (string.IsNullOrWhiteSpace(query.UserId))
        {
            return ReadMitarbeiterUrlaubsplanerResult.Fehler("Es wurde kein Mitarbeiter ausgewählt.");
        }

        if (query.Jahr is < 2000 or > 2100)
        {
            return ReadMitarbeiterUrlaubsplanerResult.Fehler("Das ausgewählte Jahr ist ungültig.");
        }

        var anspruch = await _urlaubsanspruchRepository.GetAsync(
            query.UserId,
            query.Jahr,
            cancellationToken);

        var zeitraeume = await _urlaubszeitraumRepository.GetAlleFuerBenutzerUndJahrAsync(
            query.UserId,
            query.Jahr,
            cancellationToken);

        var anspruchstage = anspruch?.Anspruchstage ?? ErmittleStandardUrlaubsanspruch(query.Beschaeftigungsart);
        var uebertragstage = anspruch?.Uebertragstage ?? 0;
        var anspruchIstStandardwert = anspruch is null;

        var zeitraumDtos = zeitraeume
            .OrderBy(x => x.Von)
            .ThenBy(x => x.Bis)
            .Select(x => new ReadMitarbeiterUrlaubsplanerZeitraumDto
            {
                Id = x.Id,
                Von = x.Von,
                Bis = x.Bis,
                Urlaubstage = ZaehleUrlaubstage(x.Von, x.Bis, query.Jahr),
                Notiz = x.Notiz
            })
            .ToArray();

        var genommeneUrlaubstage = zeitraumDtos.Sum(x => x.Urlaubstage);

        return ReadMitarbeiterUrlaubsplanerResult.Erfolg(
            query.UserId,
            query.Jahr,
            anspruchstage,
            uebertragstage,
            genommeneUrlaubstage,
            anspruchIstStandardwert,
            zeitraumDtos);
    }

    private static int ErmittleStandardUrlaubsanspruch(MitarbeiterBeschaeftigungsart beschaeftigungsart)
    {
        return beschaeftigungsart switch
        {
            MitarbeiterBeschaeftigungsart.Festangestellt => 30,
            MitarbeiterBeschaeftigungsart.Freelancer => 3,
            MitarbeiterBeschaeftigungsart.Honorarkraft => 0,
            _ => 0
        };
    }

    private static int ZaehleUrlaubstage(DateOnly von, DateOnly bis, int jahr)
    {
        var start = von.Year < jahr ? new DateOnly(jahr, 1, 1) : von;
        var ende = bis.Year > jahr ? new DateOnly(jahr, 12, 31) : bis;

        if (ende < start)
        {
            return 0;
        }

        var feiertage = ITW.Domain.Kalender.MecklenburgVorpommernFeiertage.GetFeiertage(jahr);
        var zaehler = 0;

        for (var tag = start; tag <= ende; tag = tag.AddDays(1))
        {
            if (tag.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            if (feiertage.ContainsKey(tag))
            {
                continue;
            }

            zaehler++;
        }

        return zaehler;
    }
}