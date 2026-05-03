namespace ITW.Web.Configuration.Bootstrap;

public sealed class InitialIdentityUserOptions
{
    public string Benutzername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Passwort { get; set; } = string.Empty;

    public string Bereich { get; set; } = string.Empty;

    public string Rolle { get; set; } = string.Empty;

    public string Fuehrungsverantwortung { get; set; } = "Keine";
}