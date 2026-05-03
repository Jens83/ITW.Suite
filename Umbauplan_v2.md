# Backend- und Frontend-Umbauplan der ITW-Suite – v2

## 1. Zweck dieses Dokuments

Begleitet den strukturierten Umbau der ITW-Suite und stellt sicher, dass:

- Best Practices verbindlich gelten, bevor der Umbau startet,
- ein automatisiertes Sicherheitsnetz (Tests + CI + Analyzer) Refactorings absichert,
- die Phasen in einer Reihenfolge stehen, die Folgefehler verhindert,
- jeder Schritt messbare Akzeptanzkriterien hat,
- die App auf gesundes Wachstum vorbereitet ist (drittes/viertes Modul, Tablet-Nutzung, mehr Entwickler).

**Grundlage:** `ITW_Suite_Audit.docx`, `ITW_Suite_Frontend_Audit.docx`, aktueller Repo-Stand, ADRs in `docs/`.

---

## 2. Grundsatzentscheidung

Die ITW-Suite wird **nicht** neu gebaut. Der Umbau erfolgt innerhalb der bestehenden Projektstruktur (`ITW.Web`, `ITW.Application`, `ITW.Domain`, `ITW.Infrastructure`, `ITW.Dienstplan`, `ITW.Einsatz`, `ITW.Fahrzeugmanagement`).

**Wachstum ist eingeplant.** Das bedeutet: Investitionen in Konventionen, Tests und CI sind keine „Bonus-Themen", sondern Vorinvestitionen, ohne die spätere Refactorings überproportional teuer werden.

---

## 3. Verbindliche Leitplanken

- bestehende Struktur beibehalten
- keine Logik duplizieren
- keine Schichtvermischung
- Fachlogik bleibt in den Fachmodulen
- Web koordiniert und zeigt an
- Infrastructure speichert und implementiert technische Details
- `ITW.Infrastructure` darf `ITW.Web` nicht kennen
- gemeinsame Web-Logik bleibt im Web-Projekt
- CSS bleibt zentral steuerbar (Tokens als Single Source of Truth)
- keine neuen CSS-Dateien pro View
- Views enthalten Struktur und Daten, aber keine Sonderlogik
- Änderungen erfolgen schrittweise und rückbausicher

**Best-Practice-Leitplanken**

- **Tests vor strukturellem Refactoring.** Vor dem Aufteilen einer Klasse muss eine grüne Testbasis vorhanden sein.
- **Konventionen vor neuen Dateien.** Keine Datei wird im neuen Stil erstellt, solange `.editorconfig` / `Directory.Build.props` nicht gelten.
- **ADR vor Architektur-Entscheidung.** Strukturentscheidungen werden vor der Umsetzung als ADR dokumentiert.
- **Banned-Symbols statt Selbstdisziplin.** Wo eine Konvention durch Tools erzwingbar ist, wird sie erzwungen (Stylelint, BannedSymbols.txt, Roslyn-Analyzer).
- **CI als Gatekeeper.** Pull-Requests laufen erst grün, wenn Build + Tests + Lint grün sind.

---

## 4. Aktuelle Hauptprobleme – kompakte Übersicht

| ID | Befund | Quelle | Priorität | Phase |
|---|---|---|---|---|
| F1 | ADR/Code auseinander (Einsatz leer, Fahrzeugmanagement nicht im ADR) | Backend-Audit | Hoch | 16 |
| F2/F3 | Infrastructure↔Module-Kopplung, God-DbContext | Backend-Audit | Mittel | 16 (ADR), später (Code) |
| F4 | Web-Orchestrierung wird zweiter Layer | Backend-Audit | Mittel | 11/16 |
| F5 | Geister-Testprojekt `tests/` | Backend-Audit | Niedrig | 2 |
| F7 | God-Controller (FahrzeugeController u.a.) | Backend-Audit | Hoch | 11 |
| F8 | 282× Konstruktor-Boilerplate | Backend-Audit | Hoch | 5 |
| F9 | TempData-Magic-Strings | Backend-Audit | Mittel | 8 |
| F10 | Logging fehlt | Backend-Audit | Hoch | 4 |
| F11 | Direkte DateTime-Zugriffe | Backend-Audit | Mittel | 6 |
| F12 | Async ohne CancellationToken | Backend-Audit | Mittel | 1 (Analyzer) |
| F13 | Sprachmischung Deutsch/Englisch | Backend-Audit | Mittel | 16 (NAMING.md) |
| F14/16/17 | Fehlende Repo-Konventionen | Backend-Audit | Mittel | 1 |
| F18 | Test-Lücken | Backend-Audit | Mittel | 3 |
| FE1/FE6 | Hex-Farben + doppelte Tokens | Frontend-Audit | Hoch | 7 |
| FE2 | 3 fast identische Layouts | Frontend-Audit | Hoch | 9 |
| FE3 | Bootstrap-Button-Reste | Frontend-Audit | Mittel | 10 |
| FE4/FE9 | Modul-CSS überschreibt zu viel | Frontend-Audit | Mittel | 14 |
| FE5 | Kein CSS-Bundling | Frontend-Audit | Niedrig | später |
| FE7 | Keine TagHelper / View-Components | Frontend-Audit | Niedrig | 12 |
| FE8 | Status-Message dezentral | Frontend-Audit | Niedrig | 8/9 |
| FE10 | Kein CSS-Linter | Frontend-Audit | Mittel | 7 |
| neu | PWA-Installation aufs Tablet | Wunsch Jens | Mittel | 13 |

---

## 5. Umsetzungsphasen

### Phase 0 – Bestätigung & Branch-Strategie

**Status:** Offen.

**Aufgaben:**

- [ ] Plan v2 final bestätigen (oder gewünschte Änderungen markieren).
- [ ] Git-Branch-Strategie festlegen: `main` (geschützt), `develop` (integration), `feature/<thema>`.
- [ ] Branch-Protection auf `main`: nur via PR mergen, CI muss grün sein.
- [ ] Tag `pre-refactor-2026-05` auf den aktuellen `main` setzen, damit ein Rollback-Punkt existiert.
- [ ] Convention: jede Phase wird in einem eigenen Branch `feature/phase-<n>-<kurz>` umgesetzt, in einem PR gemergt.

**Akzeptanzkriterien:** Branch-Strategie aktiv, Tag gesetzt, PR-Template vorhanden.

---

### Phase 1 – Repo-Konventionen

**Begründung:** Alle nachfolgenden Phasen profitieren von einheitlicher Formatierung und Analyzer-Hinweisen. Wer hier später aufräumt, refactort dieselben Dateien zweimal.

**Betroffene Zuständigkeit:** Solution-Root.

**Neue Dateien:**

- `.editorconfig`
- `.gitignore`
- `Directory.Build.props`
- `Directory.Packages.props`
- `BannedSymbols.txt`
- `README.md`

**Aufgaben:**

- [ ] `.editorconfig` mit `indent_style=space`, `indent_size=4` (csproj `=2`), file-scoped namespaces, expression-bodied-Members-Regeln.
- [ ] `Directory.Build.props` zentralisiert: `<TargetFramework>net9.0</TargetFramework>`, `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<LangVersion>latest</LangVersion>`, `<AnalysisLevel>latest-Recommended</AnalysisLevel>`, `<EnableNETAnalyzers>true</EnableNETAnalyzers>`, `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`.
- [ ] `Directory.Packages.props` für zentrales Versions-Pinning (alle EF-/Microsoft-Pakete einmal definiert).
- [ ] `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` zunächst als `<WarningsAsErrors>nullable;CS8600;CS8601;CS8602;CS8603;CS8604;CA2016</WarningsAsErrors>` (selektiv, statt alles hart).
- [ ] `BannedSymbols.txt` initial leer angelegt – wird in Phase 6 befüllt.
- [ ] `.gitignore` Standard-VS/.NET-Template.
- [ ] `README.md` minimal: Was ist die App, wie starten (`dotnet run --project src/ITW.Web`), wie Migrationen ausführen, wo liegen die ADRs.
- [ ] `dotnet format` einmalig solution-weit laufen lassen – **eigener Commit** „chore: dotnet format".
- [ ] `dotnet build` muss grün bleiben (sonst Analyzer-Set zurücknehmen).

**Akzeptanzkriterien:**

- `dotnet build` grün, alle 9 csprojs ohne Eigenheiten formatiert.
- Neue Dateien folgen automatisch der `.editorconfig`.
- Mind. 5 Analyzer-Regeln aktiv als Warning.

**Rollback:** Branch verwerfen, kein Code geändert außer Formatierung.

---

### Phase 2 – Geister-Testprojekt entfernen

**Status:** Offen. **Aufwand:** 5 Minuten.

**Aufgaben:**

- [ ] `tests/` löschen (`ITW.Diensplan.tests.csproj` ist nicht in der Solution).
- [ ] In `README.md` Konvention dokumentieren: *„Test-Projekte liegen unter `src/ITW.<Modul>.Test`"*.

**Akzeptanzkriterien:** `tests/` weg, Build grün, README-Hinweis vorhanden.

---

### Phase 3 – Test-Sicherheitsnetz aufbauen

**Begründung:** Phasen 8/11 (Statusmeldungen, Controller-Split) ändern Verhalten. Ohne Tests merkt niemand, wenn dabei etwas kaputtgeht.

**Betroffene Zuständigkeit:** Test-Projekte, CI.

**Neue Dateien:**

- `src/ITW.Application.Test/ITW.Application.Test.csproj` (xUnit, FluentAssertions, NSubstitute oder Moq).
- `.github/workflows/ci.yml` *oder* `azure-pipelines.yml`.

**Aufgaben:**

- [ ] `ITW.Application.Test` anlegen, in `.sln` aufnehmen.
- [ ] Pro Use-Case-Service in `ITW.Application` mind. **einen Happy-Path-Test und einen Failure-Test** schreiben (CreateBenutzerkonto, ActivateUser, LockUser, AssignUser, ChangeUserAreaRole etc.).
- [ ] Für `FahrzeugeController` (Pilot vor Phase 11) eine erste Smoke-Test-Klasse: GET-Actions liefern 200, POST-Actions ohne Pflichtfelder liefern Validierungsfehler.
- [ ] CI-Pipeline anlegen: bei jedem Push und jedem PR auf `main`/`develop`:
  - `dotnet restore`
  - `dotnet build --configuration Release`
  - `dotnet test --no-build --collect:"XPlat Code Coverage"`
  - `dotnet format --verify-no-changes` (verhindert Format-Drift)
- [ ] Coverage-Report als CI-Artefakt; Schwelle setzen wir bewusst noch nicht hart.

**Akzeptanzkriterien:**

- `ITW.Application.Test` läuft grün mit ≥ 20 Tests.
- CI läuft auf jedem PR; Build, Test und Format-Check sind verpflichtend.
- Coverage-Bericht für `ITW.Application` ≥ 50 %.

---

### Phase 4 – Logging-Grundlage

**Begründung:** Spätere Refactorings (Controller-Split, Status-Message-Migration) sind ohne Logs schwer zu validieren. Logging ist außerdem reine Addition und hat keinen Refactoring-Charakter.

**Betroffene Zuständigkeit:** `ITW.Application`, Modul-Application-Schichten, `ITW.Web`.

**Aufgaben:**

- [ ] Convention dokumentieren in `docs/05_Konventionen/01_Logging.md`:
  - Pro Use-Case-Service ein `ILogger<TService>`.
  - Templates: `LogInformation("UseCase {UseCase} begonnen", nameof(...))`, `LogInformation("UseCase {UseCase} erfolgreich, {Result}", ...)`, `LogWarning("UseCase {UseCase} fehlgeschlagen: {Reason}")`.
  - Keine personenbezogenen Daten (Passwort, vollständige E-Mail) loggen.
  - Audit-Events (Phase 4 in `Audit/`) bleiben separat von Diagnose-Logs.
- [ ] `ILogger<T>` per Konstruktor in jeden Service in `ITW.Application/**/*Service.cs` und `ITW.<Modul>/Application/**/*Service.cs` injizieren.
- [ ] Standard-Console+Debug-Logger reicht zunächst; **Serilog optional** als ADR 003-Diskussion.
- [ ] `appsettings.json` und `appsettings.Development.json` mit `Logging:LogLevel`-Konfiguration prüfen.

**Akzeptanzkriterien:**

- ≥ 90 % der Use-Case-Services in `ITW.Application` und Modul-Application haben `ILogger<T>`.
- Beim lokalen Start einer Demo-Aktion (z. B. Login) erscheinen strukturierte Log-Einträge.
- Build grün, Tests grün.

---

### Phase 5 – Konstruktor-Boilerplate eliminieren

**Begründung:** 282 manuelle Null-Checks. Replacement ist scriptbar und macht jede Klasse 30–50 % kürzer und lesbarer. Idealer Termin nach Phase 4 (so haben die neu eingefügten `ILogger`-Konstruktoren denselben Stil).

**Aufgaben:**

- [ ] Klassen mit ≤ 5 DI-Abhängigkeiten: auf **Primary Constructors (C# 12)** umstellen.
- [ ] Klassen mit > 5 Abhängigkeiten (vor allem Controller): `ArgumentNullException.ThrowIfNull(...)` im Konstruktor – kürzer und semantisch identisch.
- [ ] Migration in zwei Schritten pro Projekt: erst `ThrowIfNull`-Form (mechanisch), danach Pilot-Klassen (z. B. `CreateBenutzerkontoService`) auf Primary Constructor.
- [ ] CI: `dotnet build` muss weiterhin grün sein.

**Akzeptanzkriterien:**

- `grep -rn 'throw new ArgumentNullException(nameof' src --include='*.cs' | wc -l` < 30 (Audit-Wert: 282).
- Tests grün.

---

### Phase 6 – DateTime-Sanity

**Begründung:** Direkt nach den Konventionen, weil `BannedSymbols.txt` aus Phase 1 hier scharfgeschaltet wird. Schützt alle nachfolgenden Phasen davor, neue `DateTime.Now`-Aufrufe einzuschmuggeln.

**Aufgaben:**

- [ ] `BannedSymbols.txt` ergänzen:
  - `M:System.DateTime.get_Now;Stattdessen IDateTimeProvider verwenden`
  - `M:System.DateTime.get_Today;Stattdessen IDateTimeProvider verwenden`
  - `M:System.DateTime.get_UtcNow;Stattdessen IDateTimeProvider verwenden`
- [ ] `Directory.Build.props` ergänzen: NuGet `Microsoft.CodeAnalysis.BannedApiAnalyzers` zentral referenzieren.
- [ ] Per Build-Output sehen, welche Treffer es noch gibt (Audit: 25 produktive Stellen).
- [ ] Treffer migrieren auf `IDateTimeProvider`.
- [ ] Ausnahme dokumentieren: `SystemDateTimeProvider` selbst und Tests dürfen `DateTime.Now` nutzen (`#pragma warning disable RS0030`).
- [ ] Pro migrierter Klasse einen Zeit-bezogenen Test in `ITW.Application.Test` ergänzen (Beispiel: Wunschphase-Stichtag).

**Akzeptanzkriterien:**

- `dotnet build` grün, **inklusive aktivierter BannedSymbols** als Errors.
- Direkte `DateTime.Now/Today/UtcNow`-Aufrufe nur noch im Provider und in Tests.

---

### Phase 7 – Frontend: Token-Set finalisieren + Stylelint

**Begründung:** Token-First ist die Grundlage für alle weiteren Frontend-Phasen. Stylelint verhindert sofort, dass spätere Phasen neue Hex-Farben einschmuggeln.

**Aufgaben:**

- [ ] `app-theme.css` kuratieren – siehe Anhang A des Frontend-Audits (`text-color-subtle` ergänzen, `info-color` als Alias auf `accent-color` deklarieren, Spacing-Tokens neu, `font-size-100..400` neu).
- [ ] `package.json` + `.stylelintrc.json` im Web-Projekt anlegen (Vorlage Frontend-Audit Anhang D).
- [ ] CI-Schritt ergänzen: `npm run lint:css`.
- [ ] **Mass-Replace** für die häufigsten Hex-Werte (siehe Frontend-Audit Tabelle 3.3):
  - `#0F6CBD` → `var(--accent-color)`
  - `#15803D` → `var(--success-color)`
  - `#64748B` → `var(--text-color-muted)`
  - `#0F172A` → `var(--text-color-strong)`
  - `#94A3B8` → `var(--text-color-subtle)`
  - `#475569` → `var(--accent-color)` (im Kontext `.theme-admin`)
- [ ] Visuelle Stichprobe pro Bereich (Login, Dashboard, Wachleiterkalender, Fahrzeugübersicht) – Screenshot-Vergleich vor/nach.

**Akzeptanzkriterien:**

- `npm run lint:css` grün; `color-no-hex`-Regel verbietet neue Hex-Werte außerhalb `app-theme.css`.
- Hex-Farben in `wwwroot/css/*.css` (außer `app-theme.css`): von 462 auf < 100.
- Visuell keine Regression sichtbar.

---

### Phase 8 – Statusmeldungen vereinheitlichen

**Aufgaben:**

- [ ] `src/ITW.Web/UI/Feedback/FlashKeys.cs` (Konstanten `Success`, `Error`, `Info`).
- [ ] `src/ITW.Web/UI/Feedback/ITempDataNotifier.cs`, `TempDataNotifier.cs`.
- [ ] DI-Registrierung in `WebCoreServiceRegistrationExtensions.cs`.
- [ ] Controller schrittweise migrieren (49 `Success/Error`-Treffer): immer pro Controller-PR.
- [ ] Anzeige bleibt in `_AppStatusMessage.cshtml`, das in Phase 9 zentral eingehängt wird.

**Akzeptanzkriterien:**

- Keine direkten `TempData["SuccessMessage"]`/`["ErrorMessage"]`-Strings mehr in Controllern (Suche grep grün).
- Bestehende Status-Anzeigen funktionieren unverändert (Smoke-Test je Bereich).

---

### Phase 9 – Frontend: gemeinsames Layout + Metadaten

**Aufgaben:**

- [ ] `src/ITW.Web/Views/Shared/_AppLayout.cshtml` neu (Vorlage Frontend-Audit Anhang B).
- [ ] `src/ITW.Web/Layout/IAppLayoutMetadataProvider.cs` + Implementierung.
- [ ] DI-Registrierung; pro Bereich eine Metadata-Konfiguration.
- [ ] `_AppStatusMessage.cshtml` zentral im neuen Layout einhängen (FE8).
- [ ] Pro Area `_ViewStart.cshtml` umstellen (Layout = `_AppLayout`, ViewData["AppShell"] = await Provider.BuildAsync(...)).
- [ ] **Reihenfolge:** Verwaltung zuerst (kleinste Area, geringste Komplexität), dann Geschäftsführung, dann Intensivtransport.
- [ ] Alte Layouts erst löschen, wenn alle drei Bereiche grün laufen.

**Akzeptanzkriterien:**

- `_LayoutIntensivtransport.cshtml`, `_LayoutVerwaltung.cshtml`, `_LayoutGeschaeftsfuehrung.cshtml` gelöscht.
- Smoke-Test pro Bereich: Login, Hauptdashboard, mind. eine Detail-Seite.
- Ein Theme-Switch testweise im DevTools setzt Akzentfarbe sichtbar in **allen** Bereichen.

---

### Phase 10 – Bootstrap-Buttons aufräumen

**Aufgaben:**

- [ ] 6 Treffer `class="btn btn-*"` in Views durch `app-btn`-Pendants ersetzen.
- [ ] `btn-white` und `btn-xs` aus `ITW.Dienstplan.css` als `app-btn`-Variants neu schreiben oder löschen.
- [ ] Stylelint-Regel ergänzen: keine neuen Klassen mit Pattern `^btn-` außerhalb explizit erlaubter Liste.

**Akzeptanzkriterien:** `grep -rn 'class="btn btn-' src/ITW.Web --include='*.cshtml' | wc -l` = 0.

---

### Phase 11 – God-Controller systematisch splitten

**Begründung:** v1 erwähnt nur `FahrzeugeController`, das Audit nennt 5 Kandidaten. Wir bauen jetzt eine wiederholbare Vorgehensweise.

**Vorgehensweise pro Controller:**

1. Smoke-Tests bestehender Actions in der Test-Suite ergänzen, falls noch nicht vorhanden.
2. Actions fachlich gruppieren.
3. Pro Gruppe einen neuen Controller anlegen, Actions schneiden (inkl. Routen).
4. Gemeinsame Zugriffsprüfung in der jeweiligen Base-Klasse halten.
5. Smoke-Tests + manuelle Stichprobe.

**Pilot:** `FahrzeugeController` (1.088 LOC, 14 Deps, 20 Actions) → `FahrzeugeController`, `FahrzeugDokumenteController`, `FahrtenbuchController`, `FahrzeugPruefungenController`.

**Folge-Splits:**

- `PersonalController` (723 LOC) → Stammdaten / Dokumente / Urlaubsanspruch
- `AutoplanController` (579 LOC) → Vorschau / Lernereignisse / Einstellungen
- `DienstplanWachleiterController` (529 LOC) → Periode / Wunschphase / Tagesplanung / Ausfall
- `UrlaubsplanerController` (473 LOC) → nach Bedarf

**Akzeptanzkriterien:**

- Kein Controller > 400 LOC.
- Kein Controller mit > 8 Konstruktor-Abhängigkeiten.
- Tests pro Pilot-Controller grün.

---

### Phase 12 – TagHelper-Pilot

**Begründung:** „App wird wachsen" → typsichere UI-Bausteine sind eine günstige Vorinvestition.

**Aufgaben:**

- [ ] `src/ITW.Web/UI/TagHelpers/AppButtonTagHelper.cs` (Vorlage Frontend-Audit Anhang C).
- [ ] `src/ITW.Web/UI/TagHelpers/AppPageHeaderTagHelper.cs`.
- [ ] In `Views/_ViewImports.cshtml` registrieren (`@addTagHelper *, ITW.Web`).
- [ ] In zwei Views pilotieren (z. B. Login + Fahrzeugübersicht).
- [ ] Bei positiver Erfahrung: weitere TagHelper für `<app-card>`, `<app-empty-state>`, `<app-status-message>`.

**Akzeptanzkriterien:** Mind. ein TagHelper produktiv im Einsatz, Build grün, IntelliSense in VS funktioniert.

---

### Phase 13 – PWA-Setup

**Begründung:** Tablet-Nutzung ist Kernszenario. PWA ist additive Arbeit ohne Architektur-Risiko.

**Aufgaben:**

- [ ] `wwwroot/site.webmanifest`, `wwwroot/sw.js`, `wwwroot/js/app-pwa.js` (siehe `PWA_Setup_Prompt.md` im Repo).
- [ ] `wwwroot/img/pwa/` mit Icons (192/512/maskable/apple-touch).
- [ ] Erweiterungen in `_AppHeadResources.cshtml` und `_AppScriptResources.cshtml`.
- [ ] `Program.cs`: `FileExtensionContentTypeProvider` für `.webmanifest`.
- [ ] Tablet-Test: Android-Chrome (Installieren-Hinweis), iPad-Safari (manueller Add-to-Home-Screen).

**Akzeptanzkriterien:**

- Lighthouse PWA-Audit ≥ 90 Punkte (oder „Installable"-Badge grün).
- App installierbar auf einem Test-Tablet, Standalone-Anzeige korrekt.

---

### Phase 14 – Modul-CSS verschlanken

**Aufgaben:**

- [ ] `ITW.Dienstplan.css` (3.115 LOC) systematisch durchgehen.
- [ ] Komponenten-artige Definitionen (Buttons, Cards, Tabs, Tags, Forms) nach `app-components.css` heben.
- [ ] `!important`-Hot-Spots beheben (häufig durch Selektor-Reduktion).
- [ ] Gleiche Operation für `ITW.Personal.css` und `ITW.Fahrzeugmanagement.css`.

**Akzeptanzkriterien:**

- `ITW.Dienstplan.css` < 2.000 LOC.
- `!important`-Treffer in Modul-CSS halbiert.
- Keine optische Regression (Stichprobe pro Modul).

---

### Phase 15 – Web-Orchestrierung absichern

**Aufgaben:**

- [ ] `docs/05_Konventionen/02_Web_Orchestration.md` mit klaren Regeln (was darf, was nicht).
- [ ] **Architektur-Test** mit `NetArchTest.Rules` in `ITW.Web.Test`:
  - Klassen in `ITW.Web.Areas.*.Services` dürfen kein `DbContext`, kein Repository direkt referenzieren.
  - Klassen in `ITW.Web.Areas.*` dürfen keine Klasse aus `ITW.Domain.*.Entities` direkt zurückgeben (View-Models verwenden).
- [ ] Test wird Teil der CI.

**Akzeptanzkriterien:** Architektur-Test grün; bei Verstoß bricht der Build.

---

### Phase 16 – Architektur-Dokumentation aktualisieren

**Aufgaben:**

- [ ] `docs/04_ADR/001_Modularer_Monolith.md` ergänzen: `ITW.Fahrzeugmanagement` als aktives Modul, `ITW.Einsatz` als geplantes Modul mit Status.
- [ ] `docs/04_ADR/004_Shared_Persistence_PlatformDbContext.md` neu: dokumentiert die bewusste Entscheidung für einen Shared-DbContext, beschreibt die Trigger für einen späteren Split (4. Modul, > 50 DbSets, > 3 Entwickler).
- [ ] `docs/04_ADR/005_Naming_Konventionen.md` neu: regelt Deutsch (Fachbegriffe) vs. Englisch (Tech-Begriffe), Verben in Methoden.
- [ ] `docs/01_Architektur/02_Projektgrenzen.md` ergänzen um Web-Orchestrierungs-Regeln (ergänzend zu Phase 15).

**Akzeptanzkriterien:** ADRs liegen vor, sind im Solution-Folder `04_ADR` referenziert, Code und Doku stimmen wieder überein.

---

### Phase 17 – Watchpoints für später

Wird nicht jetzt umgesetzt, aber **bewusst dokumentiert**, damit der Plan nicht vergessen wird:

- **DbContext-Split:** ab Trigger-Bedingungen aus ADR 004 (Phase 16) – dann eigener Plan v3.
- **CSS-Bundling (FE5):** wenn Initial-Bundle > 200 KB CSS gzipped wird.
- **Mobile App (MAUI):** wenn native Sensoren oder echtes Offline-Tracking nötig werden.
- **Microservices:** bewusst **nicht**, solange ein Team das System pflegt.

---

## 6. Reihenfolge der Umsetzung (Kurzfassung)

```
0  Bestätigung & Branch-Strategie
1  Repo-Konventionen (.editorconfig, Directory.Build.props, Analyzer)
2  Geister-Testprojekt löschen
3  Test-Sicherheitsnetz + CI-Pipeline
4  Logging-Grundlage
5  Konstruktor-Boilerplate eliminieren
6  DateTime-Sanity (mit BannedSymbols)
7  Frontend: Token-Set finalisieren + Stylelint + Mass-Replace
8  Statusmeldungen vereinheitlichen (Notifier)
9  Frontend: gemeinsames Layout + Metadaten
10 Bootstrap-Buttons aufräumen
11 God-Controller systematisch splitten
12 TagHelper-Pilot
13 PWA-Setup
14 Modul-CSS verschlanken
15 Web-Orchestrierung absichern (ArchTest)
16 Architektur-Dokumentation aktualisieren
17 Watchpoints festhalten
```

---

## 7. Definition of Done – pro Phase

Eine Phase gilt als erledigt, wenn **alle** zutreffen:

| Kriterium | Quelle |
|---|---|
| Phase-Akzeptanzkriterien explizit erfüllt | jede Phase oben |
| `dotnet build` grün | CI |
| `dotnet test` grün | CI |
| `dotnet format --verify-no-changes` grün | CI |
| `npm run lint:css` grün (sofern relevant) | CI |
| PR ist gemergt, gegen Branch-Protection | GitHub/Azure |
| Phase-Häkchen in diesem Dokument gesetzt | manuell |

---

## 8. Nicht-Ziele

Bewusst **außerhalb** dieses Plans:

- Microservices
- MediatR als Pflichtdienst (kann später als ADR diskutiert werden)
- vollständiger DbContext-Split (siehe Phase 17)
- neues Frontend-Framework (Blazor, React, Vue)
- neues CSS-Framework (Tailwind, SASS-Migration)
- neue Projektstruktur
- View-spezifische CSS-Dateien
- Umbau der Fachlogik ohne fachlichen Anlass

---

## 9. Aktueller nächster Schritt

1. **Diesen Plan v2 final bestätigen** (Phasen-Streichungen/-Ergänzungen einarbeiten).
2. Branch `feature/phase-1-konventionen` anlegen.
3. `.editorconfig`, `Directory.Build.props`, `Directory.Packages.props`, `.gitignore`, `README.md` ablegen.
4. Solution-weit `dotnet format` laufen lassen, separat committen.
5. PR aufmachen, ohne CI noch (CI kommt in Phase 3).

---

## 10. Anhang – Phasenabhängigkeiten

```
Phase 0
   │
   ▼
Phase 1 ──────────────────────────┐
   │                              │
   ▼                              │
Phase 2                           │
   │                              │
   ▼                              │
Phase 3 (CI)  ◄───── braucht 1 ───┘
   │
   ▼
Phase 4 (Logging)
   │
   ▼
Phase 5 (Boilerplate)
   │
   ▼
Phase 6 (DateTime)  ◄────── braucht 1 (BannedSymbols)
   │
   ▼
Phase 7 (Tokens + Stylelint)  ──┐
   │                            │
   ▼                            │
Phase 8 (Notifier)              │
   │                            │
   ▼                            ▼
Phase 9 (Layout) ◄── braucht 7 + 8
   │
   ▼
Phase 10 (Buttons)
   │
   ▼
Phase 11 (Controller-Split) ◄── braucht 3 (Tests)
   │
   ▼
Phase 12 (TagHelper) — parallel zu 13/14 möglich
Phase 13 (PWA)       — parallel zu 12/14 möglich
Phase 14 (Modul-CSS) — parallel zu 12/13 möglich
   │
   ▼
Phase 15 (ArchTest) ◄── braucht 3 (CI) + 11 (saubere Controller)
   │
   ▼
Phase 16 (ADRs)
   │
   ▼
Phase 17 (Watchpoints festhalten)
```
