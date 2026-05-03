namespace ITW.Domain.Kalender;

public static class MecklenburgVorpommernFeiertage
{
    public static IReadOnlyDictionary<DateOnly, string> GetFeiertage(int jahr)
    {
        if (jahr < 2000 || jahr > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(jahr), "Das Jahr ist ungültig.");
        }

        var ostersonntag = BerechneOstersonntag(jahr);

        var feiertage = new Dictionary<DateOnly, string>
        {
            [new DateOnly(jahr, 1, 1)] = "Neujahr",
            [new DateOnly(jahr, 3, 8)] = "Internationaler Frauentag",
            [ostersonntag.AddDays(-2)] = "Karfreitag",
            [ostersonntag.AddDays(1)] = "Ostermontag",
            [new DateOnly(jahr, 5, 1)] = "Tag der Arbeit",
            [ostersonntag.AddDays(39)] = "Christi Himmelfahrt",
            [ostersonntag.AddDays(50)] = "Pfingstmontag",
            [new DateOnly(jahr, 10, 3)] = "Tag der Deutschen Einheit",
            [new DateOnly(jahr, 10, 31)] = "Reformationstag",
            [new DateOnly(jahr, 12, 25)] = "1. Weihnachtstag",
            [new DateOnly(jahr, 12, 26)] = "2. Weihnachtstag"
        };

        return feiertage;
    }

    public static bool TryGetFeiertagsname(DateOnly datum, out string feiertagsname)
    {
        var feiertage = GetFeiertage(datum.Year);
        return feiertage.TryGetValue(datum, out feiertagsname!);
    }

    private static DateOnly BerechneOstersonntag(int jahr)
    {
        var a = jahr % 19;
        var b = jahr / 100;
        var c = jahr % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var monat = (h + l - 7 * m + 114) / 31;
        var tag = ((h + l - 7 * m + 114) % 31) + 1;

        return new DateOnly(jahr, monat, tag);
    }
}