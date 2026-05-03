namespace ITW.Dienstplan.Application.Kalender;

public static class MecklenburgVorpommernFeiertage
{
    public static IReadOnlyDictionary<DateOnly, string> GetFeiertage(int jahr)
        => ITW.Domain.Kalender.MecklenburgVorpommernFeiertage.GetFeiertage(jahr);

    public static bool TryGetFeiertagsname(DateOnly datum, out string feiertagsname)
        => ITW.Domain.Kalender.MecklenburgVorpommernFeiertage.TryGetFeiertagsname(datum, out feiertagsname);
}