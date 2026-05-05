namespace ITW.Web.Logging;

public sealed record LogEintrag(
    DateTimeOffset Zeitstempel,
    string Level,
    string Nachricht,
    string? Quelle,
    string? Ausnahme);
