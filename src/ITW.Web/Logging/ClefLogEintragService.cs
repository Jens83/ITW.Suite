using System.Text.Json;

namespace ITW.Web.Logging;

public sealed class ClefLogEintragService : ILogEintragService
{
    private static readonly JsonDocumentOptions _jsonOpts = new() { AllowTrailingCommas = true };

    private readonly string _logVerzeichnis;

    public ClefLogEintragService(IConfiguration configuration)
    {
        _logVerzeichnis = configuration["Logging:LogVerzeichnis"]
            ?? Path.Combine(AppContext.BaseDirectory, "logs");
    }

    public Task<IReadOnlyList<LogEintrag>> GetRecentAsync(
        int maxEintraege = 200,
        string? levelFilter = null,
        CancellationToken cancellationToken = default)
    {
        var eintraege = LeseEintraege(maxEintraege, levelFilter);
        return Task.FromResult<IReadOnlyList<LogEintrag>>(eintraege);
    }

    private List<LogEintrag> LeseEintraege(int max, string? levelFilter)
    {
        var dateien = HoleLogDateien();
        var ergebnis = new List<LogEintrag>(max);

        foreach (var datei in dateien)
        {
            if (ergebnis.Count >= max)
                break;

            var zeilen = LeseLetztZeilen(datei, max - ergebnis.Count);

            foreach (var zeile in zeilen)
            {
                var eintrag = ParseZeile(zeile);
                if (eintrag is null)
                    continue;

                if (!string.IsNullOrWhiteSpace(levelFilter)
                    && !string.Equals(eintrag.Level, levelFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                ergebnis.Add(eintrag);
            }
        }

        ergebnis.Sort((a, b) => b.Zeitstempel.CompareTo(a.Zeitstempel));
        return ergebnis;
    }

    private IEnumerable<string> HoleLogDateien()
    {
        if (!Directory.Exists(_logVerzeichnis))
            return [];

        return Directory
            .GetFiles(_logVerzeichnis, "itw-suite-*.clef")
            .OrderByDescending(f => f);
    }

    private static string[] LeseLetztZeilen(string pfad, int anzahl)
    {
        try
        {
            using var stream = new FileStream(pfad, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            var alle = new List<string>();

            while (reader.ReadLine() is { } zeile)
            {
                if (!string.IsNullOrWhiteSpace(zeile))
                    alle.Add(zeile);
            }

            return alle.Count <= anzahl
                ? [.. alle]
                : [.. alle.GetRange(alle.Count - anzahl, anzahl)];
        }
        catch
        {
            return [];
        }
    }

    private static LogEintrag? ParseZeile(string zeile)
    {
        try
        {
            using var doc = JsonDocument.Parse(zeile, _jsonOpts);
            var root = doc.RootElement;

            var ts = root.TryGetProperty("@t", out var tEl) && tEl.ValueKind == JsonValueKind.String
                ? DateTimeOffset.Parse(tEl.GetString()!, System.Globalization.CultureInfo.InvariantCulture)
                : DateTimeOffset.UtcNow;

            var level = root.TryGetProperty("@l", out var lEl) && lEl.ValueKind == JsonValueKind.String
                ? lEl.GetString() ?? "Information"
                : "Information";

            var nachricht = root.TryGetProperty("@m", out var mEl) && mEl.ValueKind == JsonValueKind.String
                ? mEl.GetString() ?? string.Empty
                : root.TryGetProperty("@mt", out var mtEl)
                    ? mtEl.GetString() ?? string.Empty
                    : string.Empty;

            var quelle = root.TryGetProperty("SourceContext", out var scEl) && scEl.ValueKind == JsonValueKind.String
                ? KuerzQuelle(scEl.GetString())
                : null;

            var ausnahme = root.TryGetProperty("@x", out var xEl) && xEl.ValueKind == JsonValueKind.String
                ? xEl.GetString()
                : null;

            return new LogEintrag(ts, NormLevel(level), nachricht, quelle, ausnahme);
        }
        catch
        {
            return null;
        }
    }

    private static string NormLevel(string raw) => raw.ToUpperInvariant() switch
    {
        "VRB" or "VERBOSE" => "Verbose",
        "DBG" or "DEBUG" => "Debug",
        "INF" or "INFORMATION" => "Information",
        "WRN" or "WARNING" => "Warning",
        "ERR" or "ERROR" => "Error",
        "FTL" or "FATAL" => "Fatal",
        _ => raw
    };

    private static string? KuerzQuelle(string? quelle)
    {
        if (string.IsNullOrWhiteSpace(quelle))
            return null;

        var letzterPunkt = quelle.LastIndexOf('.');
        return letzterPunkt >= 0 ? quelle[(letzterPunkt + 1)..] : quelle;
    }
}
