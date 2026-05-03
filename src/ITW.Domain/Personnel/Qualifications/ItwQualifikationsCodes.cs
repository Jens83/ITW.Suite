namespace ITW.Domain.Personnel.Qualifications;

public static class ItwQualifikationsCodes
{
    public const string Arzt = "ARZT";
    public const string Notfallsanitaeter = "NOTSAN";

    public const string FacharztAnaesthesie = "FACHAERZT_ANAESTHESIE";
    public const string DiviQualifizierung = "DIVI";
    public const string Praxisanleiter = "PRAXISANLEITER";
    public const string MedizinprodukteBeauftragter = "MPB";

    public static bool IstHauptqualifikationCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return string.Equals(code, Arzt, StringComparison.OrdinalIgnoreCase)
            || string.Equals(code, Notfallsanitaeter, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IstZusatzqualifikationCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        return !IstHauptqualifikationCode(code);
    }
}