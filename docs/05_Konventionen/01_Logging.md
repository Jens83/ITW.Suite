# Logging-Konvention – ITW.Suite

## Grundprinzip

Jeder Use-Case-Service bekommt einen `ILogger<T>` per Konstruktor injiziert.  
Logs sind Diagnose-Werkzeuge, **keine Audit-Trails** für personenbezogene Daten.

---

## Log-Infrastruktur

| Sink | Format | Pfad | Aufbewahrung |
|---|---|---|---|
| Console | Lesbar (Text-Template) | stdout | — |
| Datei (CLEF) | Compact JSON (1 Zeile pro Event) | `logs/itw-suite-{Datum}.clef` | 30 Tage |

Das Log-Verzeichnis ist über `Logging:LogVerzeichnis` in `appsettings.json` konfigurierbar.

---

## Pflicht-Templates

### Mutations-Services (Create, Save, Assign, Delete …)

```csharp
// Beginn
_logger.LogInformation("UseCase {UseCase} begonnen", nameof(MeinService));

// Validierungsfehler
_logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(MeinService), "UserId leer");

// Repository-Fehler
_logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(MeinService), result.ErrorMessage);

// Erfolg
_logger.LogInformation("UseCase {UseCase} erfolgreich", nameof(MeinService));
```

### Read-Services

```csharp
// Nur bei nicht-trivialem Fehlerpfad
_logger.LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}", nameof(MeinService), "...");
```

---

## Was NICHT geloggt wird

- Passwörter, temporäre Passwörter
- Vollständige E-Mail-Adressen (nur UserId)
- Kreditkarten- oder Bankdaten

---

## Log-Viewer (Admin)

Verwaltungsbereich → **Systemprotokoll** (`/Verwaltung/SystemLog`)  
Zugang: nur Benutzer mit `FuehrungsverantwortungCode != Keine` im Verwaltungsbereich.

Der Viewer zeigt die letzten 300 Einträge aus den CLEF-Dateien und erlaubt Filter nach Level (Information / Warning / Error / Fatal).

---

## Serilog-Konfiguration (appsettings.json)

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "System": "Warning"
    }
  }
}
```

Das Minimum-Level kann pro Umgebung in `appsettings.{Environment}.json` überschrieben werden:

```json
"Serilog": {
  "MinimumLevel": { "Default": "Debug" }
}
```
