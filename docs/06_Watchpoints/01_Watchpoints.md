# Architektur-Watchpoints der ITW-Suite

> Stand: 2026-05-05 | Verantwortlich: Umbauplan v2, Phase 17

Diese Watchpoints werden **bewusst nicht jetzt umgesetzt**. Sie sind dokumentiert, damit sie beim richtigen Trigger nicht vergessen werden. Jeder Watchpoint nennt die konkrete Bedingung, bei der er aktiv wird.

---

## WP-01 – DbContext-Split

**Trigger:** Einer der Schwellwerte aus [ADR 004](../04_ADR/004_Shared_Persistence_PlatformDbContext.md) ist erreicht:

| Bedingung | Schwellwert |
|---|---|
| Anzahl aktiver Fachmodule | ≥ 4 |
| Anzahl DbSets im PlatformDbContext | > 50 |
| Anzahl aktiver Entwickler | > 3 |
| Migrationskonflikte häufen sich | wiederkehrendes Problem |

**Maßnahme bei Auslösung:**
- Evaluierung modulspezifischer DbContexts (z. B. `DienstplanDbContext`, `FahrzeugDbContext`)
- Ggf. eigener Umbauplan v3
- Migrations-Strategie definieren (eine gemeinsame Datenbank, getrennte Kontexte — oder separate Datenbanken)

**Aktueller Stand (2026-05-05):** 2 aktive Fachmodule, PlatformDbContext deutlich unter 50 DbSets — Trigger nicht aktiv.

---

## WP-02 – CSS-Bundling

**Trigger:** Das unkomprimierte Initial-CSS-Bundle übersteigt **200 KB gzipped**.

**Hintergrund:**  
Aktuell werden CSS-Dateien (app-components.css, app-tokens.css, ITW.Dienstplan.css, ITW.Fahrzeugmanagement.css) als statische Einzeldateien ausgeliefert. Solange das Budget nicht überschritten ist, ist ein Build-Schritt unnötige Komplexität.

**Maßnahme bei Auslösung:**
- Einführung eines CSS-Bundle-Build-Schritts (z. B. esbuild, Vite oder ASP.NET Bundling)
- Code-Splitting nach Area / Modul prüfen
- Critical CSS für LCP-Elemente evaluieren

**Messung:** `gzip -k --best wwwroot/css/*.css && du -sh wwwroot/css/*.css.gz`

**Aktueller Stand (2026-05-05):** Budget unkritisch — Trigger nicht aktiv.

---

## WP-03 – Mobile App (MAUI)

**Trigger:** Eines der folgenden Szenarien tritt ein:
- Native Gerätesensoren (Kamera, Barcode, NFC) werden für Fahrzeuginspektionen oder Einsatzdokumentation benötigt.
- Echtes Offline-Tracking ist erforderlich (GPS-Daten lokal puffern, wenn keine Verbindung besteht).
- Die browserbasierte Tablet-Seite reicht für den operativen Wachleiter-Einsatz nicht mehr aus.

**Hintergrund:**  
Das aktuelle Tracking ist browserbasiert (Progressive Web App auf dem Tablet). Für einfache GPS-Übermittlung reicht das. MAUI wäre nötig, wenn nativer Gerätezugriff oder robustes Offline-Verhalten unumgänglich werden.

**Maßnahme bei Auslösung:**
- MAUI-Projekt `ITW.Mobile` evaluieren
- API-Schicht für Mobile-Clients definieren (REST oder SignalR)
- Offline-Sync-Strategie festlegen

**Aktueller Stand (2026-05-05):** PWA-Tracking ausreichend — Trigger nicht aktiv.

---

## WP-04 – Microservices

**Entscheidung: Bewusst nicht**, solange folgende Bedingungen gelten:
- Ein Team pflegt das gesamte System.
- Kein fachliches Modul muss unabhängig skaliert oder deployed werden.
- Keine organisatorischen Teamgrenzen erzwingen Service-Autonomie.

**Begründung:**  
Microservices lösen Organisations- und Skalierungsprobleme, die die ITW-Suite derzeit nicht hat. Der modulare Monolith hält Modulgrenzen ohne Netzwerk-Overhead. Siehe [ADR 001](../04_ADR/001_Modularer_Monolith.md).

**Trigger für Neubewertung:** Nur wenn alle drei zutreffen:
1. Mehrere unabhängige Teams mit getrennten Deployment-Zyklen
2. Mindestens ein Modul mit drastisch unterschiedlichen Skalierungsanforderungen
3. Organisatorische Entscheidung für Service-Autonomie

**Aktueller Stand (2026-05-05):** Kein Trigger in Sicht — Microservices bleiben Nicht-Ziel.

---

## Überblick

| ID | Watchpoint | Trigger-Status (2026-05-05) |
|---|---|---|
| WP-01 | DbContext-Split | nicht aktiv |
| WP-02 | CSS-Bundling | nicht aktiv |
| WP-03 | Mobile App (MAUI) | nicht aktiv |
| WP-04 | Microservices | bewusst ausgeschlossen |

Diese Tabelle wird bei jeder neuen Umbauplan-Iteration (v3+) aktualisiert.
