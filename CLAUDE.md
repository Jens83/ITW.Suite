# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Design-Regeln

Nutze das `AskUserQuestion`-Tool, um den Nutzer über das Websitedesign zu interviewen, damit du die Vorstellungen des Nutzers genau abbilden kannst.

Nutze `DESIGN.md` für Design-System-Generierung. Keine generischen AI-Aesthetics. Bold, distinctive Design-Choices. Performance-optimiert (Core Web Vitals).

## Commands

```powershell
# Build
dotnet build src\ITW.Suite.sln

# Run
dotnet run --project src\ITW.Web

# Run all tests
dotnet test src\ITW.Suite.sln

# Run a single test project
dotnet test src\ITW.Dienstplan.Test

# Format check
dotnet format src\ITW.Suite.sln --verify-no-changes

# CSS linting
npm run lint:css           # check
npm run lint:css:fix       # fix
```

## Architecture

**Modular Monolith** – .NET 9, ASP.NET Core MVC, EF Core 9, SQL Server.

### Projects

| Project | Role |
|---------|------|
| `ITW.Web` | MVC host: Controllers, Razor Views, wwwroot, DI wiring |
| `ITW.Application` | Platform-wide contracts and cross-cutting use cases |
| `ITW.Domain` | Core entities, value objects, enums |
| `ITW.Infrastructure` | EF Core DbContext, Identity, Repositories, Schema Bootstrappers |
| `ITW.Dienstplan` | Module: Shift planning, wish phase, auto-planner, calendar |
| `ITW.Fahrzeugmanagement` | Module: Vehicle management, logbook, documents |
| `ITW.Lagermanagement` | Module: Inventory/warehouse management |
| `ITW.Einsatz` | Module: Case management (stub) |
| `ITW.*.Test` | xUnit test projects (one per module) |

Project boundary rules are documented in `docs/01_Architektur/02_Projektgrenzen.md` and `docs/04_ADR/001_Modularer_Monolith.md`. Business logic lives in module projects, **never** in `ITW.Web`.

### Layering (per module)

```
Domain (entities, enums, rules)
    ↓
Application (interfaces, service classes, commands/queries)
    ↓
Infrastructure (repositories, DbContext, persistence configs)
    ↓
Web (Controllers, ViewModels, Razor views, DI extensions)
```

### Key Patterns

**Service classes** – one class per operation, named by action (e.g. `SaveFreelancerMonatswunschService`). Input is a Command/Query record; return is a Result object with `IsSuccess` and `ErrorMessage`. All methods are `ExecuteAsync(command, CancellationToken)`.

**Repository pattern** – interfaces in `Application/Contracts/`, EF Core implementations in `Infrastructure/Persistence/Repositories/`. Fluent API configs in `Persistence/Configurations/`.

**DI extensions** – one `Web*ServiceRegistrationExtensions` per module, all called from `Program.cs` via `builder.Services.AddWebApplicationServices()`.

**ASP.NET Areas** – three areas reflecting org structure:
- `Intensivtransport` – Shift planning, vehicles, warehouse
- `Verwaltung` – Personnel, user management
- `Geschaeftsfuehrung` – Executive dashboard, reports

**Schema Bootstrappers** – ensure tables exist on startup; wired in `ApplicationStartupExtensions.cs`.

### Naming Conventions

German business terms (`Bereich`, `Wachleiter`, `Dienstwunsch`, `Fahrzeug`) combined with English technical terms (`Service`, `Repository`, `Controller`, `Command`, `Query`, `Result`). Classes PascalCase, private fields camelCase with `_` prefix.

### Tests

**Framework**: xUnit with manual fake repositories (no mocking library).

Test naming: `ExecuteAsync_[Scenario]` (German scenario descriptions, e.g. `ExecuteAsync_SpeichertMonatswunschWennBenutzerFreelancerIst`). Arrange/Act/Assert, `CancellationToken.None` in tests. Fake repos are sealed inner classes implementing the repository interface.

### Code Quality

`Directory.Build.props` enforces C# 12, analyzers, and null-safety warnings as errors (CS8600–8604, CA2016). `BannedSymbols.txt` prevents unsafe patterns. All I/O is async with `CancellationToken`.

### Design System

Tokens defined in `src/ITW.Web/wwwroot/css/app-theme.css`. Only `var(--token)` used outside that file. Dark mode via `@media (prefers-color-scheme: dark)`. Full specification in `DESIGN.md`.
