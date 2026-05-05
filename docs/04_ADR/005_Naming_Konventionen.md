# ADR 005 – Naming-Konventionen

## Status
Akzeptiert

## Datum
2026-05-05

---

## Kontext

Die ITW-Suite ist eine deutschsprachige Fachanwendung für Intensivtransport-Teams. Domänenbegriffe sind deutsch (Dienstplan, Fahrzeug, Einsatz). Die technische Infrastruktur ist englischsprachig (ASP.NET Core, Entity Framework Core, C#).

Ohne klare Konvention entsteht ein Mix aus deutschen und englischen Bezeichnern, der schwer lesbar ist und Verwechslungen erzeugt.

---

## Entscheidung

Für die ITW-Suite gilt folgende Sprachregel:

- **Domänenbegriffe → Deutsch**
- **Technische Begriffe → Englisch**

---

## Konkrete Regeln

### Klassen- und Typnamen

| Kategorie | Sprache | Beispiel |
|---|---|---|
| Entities | Deutsch | `Fahrzeug`, `DienstplanPeriode`, `Dienst` |
| Value Objects | Deutsch | `Kennzeichen`, `FahrzeugStatus` |
| Enums | Deutsch | `FahrzeugStatus.Aktiv`, `DienstTyp.Frueh` |
| Application Services | Deutsch (Fach) + Englisch (Tech) | `ReadFahrzeugUebersichtService`, `CreateDienstService` |
| Commands / Queries | Deutsch (Fach) | `CreateFahrzeugCommand`, `ReadDienstplanQuery` |
| ViewModels | Deutsch (Fach) | `FahrzeugeIndexViewModel`, `DienstplanMonatsViewModel` |
| Repository-Interfaces | Deutsch (Fach) | `IFahrzeugRepository`, `IDienstplanPeriodeRepository` |
| TagHelpers | Englisch (Tech) | `AppButtonTagHelper`, `AppPageHeaderTagHelper` |
| Infrastructure-Klassen | Englisch (Tech) | `PlatformDbContext`, `FahrzeugEntityConfiguration` |

### Methoden

Methoden beginnen mit einem Verb, das die Aktion beschreibt. Bei Domänenmethoden werden deutsche Verben bevorzugt, bei technischen Hilfsmethoden englische.

| Verb | Bedeutung | Beispiel |
|---|---|---|
| `Read*` | Abfragen | `ReadFahrzeugDetailService` |
| `Create*` | Anlegen | `CreateFahrzeugCommand` |
| `Save*` | Persistieren | `SaveFahrzeugPruefungService` |
| `Delete*` | Entfernen | `DeleteFahrzeugDokumentService` |
| `Baue*` | ViewModel zusammenbauen | `BaueNavigation(...)` |
| `Ermittle*` | Berechnen / Auflösen | `ErmittleStatusText(...)` |

### Properties und Felder

- Fachliche Properties folgen dem Domänenbegriff: `Kennzeichen`, `InterneNummer`, `FahrtDatum`
- Technische Properties folgen englischen Konventionen: `Id`, `CreatedAt`

### Namespaces

- Namespaces folgen dem Modulschnitt und sind englisch (technische Kategorie):
  `ITW.Fahrzeugmanagement.Domain.Entities`, `ITW.Dienstplan.Application.Dienste`
- Deutsche Namespaces werden nicht verwendet.

---

## Konsequenzen

- Neue Entwickler müssen beide Welten kennen (Deutsch für Domain, Englisch für Tech).
- Code-Reviews prüfen Namensgebung explizit.
- Neue Klassen folgen dieser Konvention ohne Ausnahme.

---

## Abgelehnte Alternativen

### Vollständig Englisch
Abgelehnt, weil:
- Domänenbegriffe wie `Dienstplan`, `Wachleiter`, `Fahrzeugakte` keine treffenden englischen Entsprechungen haben,
- Übersetzungen Bedeutung und Präzision verlieren.

### Vollständig Deutsch
Abgelehnt, weil:
- Technische Begriffe (DbContext, TagHelper, Repository) englischsprachig sind,
- eine Mischung aus deutschen Verben und englischen Framework-Typen unleserlich wird.

---

## Zusammenfassung

Domänenbegriffe sind deutsch, technische Begriffe sind englisch. Diese Konvention gibt Lesbarkeit und Präzision — ohne Phantom-Übersetzungen oder inkonsistente Mischung.
