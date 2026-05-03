# ITW.Suite

Interne Web-Anwendung für die Organisation des Intensivtransport-Bereichs
des DRK Rettungswache Neubrandenburg.

Die Anwendung deckt aktuell folgende Module ab:

- **Dienstplan** – Wunschphase, Tagesplanung, Wachleiter-Workflows, Auswertung.
- **Fahrzeugmanagement** – Fahrzeugakte, Dokumente, Fahrtenbuch, Tablet-Tracking.
- **Personal** – Mitarbeiterprofile, Qualifikationen, Urlaubsplaner.
- **Benutzerverwaltung** – Bereichsrollen, Modulzuweisungen, Passwort-Reset.

## Technischer Stack

- ASP.NET Core 9 MVC (Razor Views)
- EF Core 9 mit SQL Server
- ASP.NET Core Identity
- QuestPDF (Reports), QRCoder (Tracking-Setup)

## Projektstruktur

```
ITW.Suite/
├─ docs/                     # Architektur-Dokumentation, ADRs, Konventionen
│  ├─ 01_Architektur/
│  ├─ 02_Rechte/
│  ├─ 03_MVP/
│  └─ 04_ADR/
├─ src/
│  ├─ ITW.Suite.sln
│  ├─ ITW.Web/               # ASP.NET Core MVC Host
│  ├─ ITW.Application/       # Plattformweite Use Cases
│  ├─ ITW.Domain/            # Kernbegriffe & Value Objects
│  ├─ ITW.Infrastructure/    # EF Core, Identity, Persistenz
│  ├─ ITW.Dienstplan/        # Fachmodul Dienstplan
│  ├─ ITW.Einsatz/           # Fachmodul Einsatz (geplant, aktuell leer)
│  ├─ ITW.Fahrzeugmanagement/# Fachmodul Fahrzeugmanagement
│  ├─ ITW.Dienstplan.Test/
│  ├─ ITW.Fahrzeugmanagement.Test/
│  └─ ITW.Web.Test/
├─ ITW_Suite_Audit.docx              # Backend-Audit (Mai 2026)
├─ ITW_Suite_Frontend_Audit.docx     # Frontend-Audit (Mai 2026)
├─ Umbauplan_v2.md                    # Aktueller Umbauplan
├─ PWA_Setup_Prompt.md                # Prompt-Vorlage für PWA-Setup
├─ Directory.Build.props              # Solution-weite MSBuild-Defaults
├─ .editorconfig                      # Code-Style-Konventionen
├─ .gitignore
└─ README.md
```

## Architektur

Die Anwendung ist ein **modularer Monolith**. Die verbindlichen
Projektgrenzen und Referenzrichtungen stehen in:

- [`docs/04_ADR/001_Modularer_Monolith.md`](docs/04_ADR/001_Modularer_Monolith.md)
- [`docs/01_Architektur/02_Projektgrenzen.md`](docs/01_Architektur/02_Projektgrenzen.md)

Aktueller Umbau-Stand und Best-Practice-Plan:

- [`Umbauplan_v2.md`](Umbauplan_v2.md)

## Entwicklung

### Voraussetzungen

- .NET 9 SDK
- SQL Server (LocalDB oder Express reicht für die Entwicklung)
- Visual Studio 2022 (oder VS Code mit C# Dev Kit)

### Erst-Einrichtung

```powershell
# Repository klonen / aktualisieren
cd C:\Pfad\zu\ITW.Suite

# Wiederherstellung der NuGet-Pakete
dotnet restore src\ITW.Suite.sln

# Datenbank-Migrationen anwenden
# (genauer Befehl folgt, sobald Migrationen-Workflow dokumentiert ist)

# Anwendung starten
dotnet run --project src\ITW.Web
```

### Tests

```powershell
dotnet test src\ITW.Suite.sln
```

### Code-Style

Der Solution-weite Code-Style ist in `.editorconfig` definiert. Vor
einem Commit prüfen oder automatisch korrigieren:

```powershell
dotnet format src\ITW.Suite.sln
```

## Konventionen

- **Test-Projekte** liegen unter `src/ITW.<Modul>.Test`. Andere Pfade
  (z. B. ein Wurzel-Ordner `tests/`) sind nicht vorgesehen.
- **Fachlogik** gehört in die jeweiligen Fachmodule
  (`ITW.Dienstplan`, `ITW.Fahrzeugmanagement`, ...) – nicht in
  `ITW.Web`.
- **CSS-Tokens**: zentrale Farben/Schatten/Radien stehen in
  `src/ITW.Web/wwwroot/css/app-theme.css`. Außerhalb dieser Datei
  werden ausschließlich `var(--token)`-Werte verwendet
  (siehe Frontend-Audit).
- **Naming**: Fachbegriffe deutsch (Bereich, Wachleiter, Wunschphase),
  technische Bezeichner englisch (Service, Controller, Repository,
  Result, Command, Query).

## Dokumentation

Alle Architektur-Entscheidungen werden als **ADR**
(Architecture Decision Records) in `docs/04_ADR/` festgehalten.
Eine Entscheidung gilt erst, wenn ein zugehöriges ADR existiert.

## Lizenz / Vertraulichkeit

Internes System, kein öffentlicher Quellcode.
