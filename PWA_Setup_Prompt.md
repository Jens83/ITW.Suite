# Prompt: PWA-Setup für ITW.Suite (für Visual Studio Chat)

> **Vor dem Einfügen prüfen:**
> - App-Anzeigename (`ITW Suite`, `short_name: ITW`) – ggf. ersetzen.
> - Shortcut-Routen (`/Intensivtransport/Dienstplan/Wachleiterkalender`, `/Intensivtransport/TabletLiveStandort`) – auf reale URLs anpassen, falls abweichend.
> - Akzentfarbe `#0F6CBD` – nur ändern, falls du auch das CSS-Token `--accent-color` änderst.

---

## Kontext

Ich entwickle eine interne Web-Anwendung für eine Rettungsdienst-Organisation (ITW = Intensivtransportwagen).

- **Stack:** ASP.NET Core 9 MVC mit Razor Views, EF Core, SQL Server
- **Solution:** `ITW.Suite` (modularer Monolith) mit den Projekten `ITW.Web`, `ITW.Domain`, `ITW.Application`, `ITW.Infrastructure`, `ITW.Dienstplan`, `ITW.Fahrzeugmanagement`
- **Webprojekt-Pfad:** `src/ITW.Web/`
- **UI ist deutschsprachig**, läuft hauptsächlich auf Tablets im Fahrzeug
- **Akzentfarbe** (CSS-Token `--accent-color`): `#0F6CBD`
- **HTTPS** ist via `UseHttpsRedirection` in `Program.cs` bereits aktiv
- 3 Areas: `Intensivtransport`, `Verwaltung`, `Geschaeftsfuehrung`
- Zentrale Layout-Resource: `src/ITW.Web/Views/Shared/Komponenten/_AppHeadResources.cshtml`
- Zentrale Skript-Resource: `src/ITW.Web/Views/Shared/Komponenten/_AppScriptResources.cshtml`
- CSS-Token-Datei: `src/ITW.Web/wwwroot/css/app-theme.css`

## Ziel

Mache die Anwendung zu einer **Progressive Web App (PWA)**, so dass sie auf einem Android- oder iPad-Tablet aus dem Browser heraus auf den Homescreen installiert werden kann (wie eine native App). Beim Öffnen soll sie **standalone** laufen, ohne Browser-Adressleiste.

## Aufgaben (bitte einzeln zeigen, bevor du Änderungen speicherst)

### 1. Web App Manifest

Erstelle `src/ITW.Web/wwwroot/site.webmanifest` mit:

- `name`: `"ITW Suite"`
- `short_name`: `"ITW"`
- `description`: `"Intensivtransport Suite"`
- `lang`: `"de-DE"`
- `start_url`: `"/"`
- `scope`: `"/"`
- `display`: `"standalone"`
- `orientation`: `"any"`
- `background_color`: `"#F4F7FB"`
- `theme_color`: `"#0F6CBD"`
- `icons`-Array mit Verweisen auf `/img/pwa/icon-192.png`, `/img/pwa/icon-512.png`, `/img/pwa/icon-512-maskable.png` (purpose `"any"` bzw. `"maskable"`)
- `shortcuts`-Array mit zwei Einträgen:
  - `"Wachleiterkalender"` → `/Intensivtransport/Dienstplan/Wachleiterkalender`
  - `"Live-Standort"` → `/Intensivtransport/TabletLiveStandort`

### 2. Service Worker (minimal)

Erstelle `src/ITW.Web/wwwroot/sw.js`:

- `const CACHE = "itw-suite-v1";`
- **install**: `self.skipWaiting()`
- **activate**: alte Caches (Name ≠ aktueller `CACHE`) löschen, dann `self.clients.claim()`
- **fetch**:
  - **Network-First** für HTML-Navigations-Requests (`request.mode === "navigate"`); fällt im Offline-Fall zurück auf eine zwischengespeicherte `/offline`-Antwort, falls vorhanden
  - **Cache-First** für statische Assets unter `/css/`, `/js/`, `/lib/`, `/img/`
  - Sonstige Requests: einfach `fetch(event.request)` durchreichen, **nicht** cachen (insbesondere POST/AntiForgery-Endpunkte unangetastet lassen)

### 3. PWA-Registrierungsskript

Erstelle `src/ITW.Web/wwwroot/js/app-pwa.js`:

- registriert `/sw.js` bei `window.addEventListener("load", ...)` mit Try/Catch
- fängt `beforeinstallprompt` ab und speichert das Event in `window.deferredInstallPrompt` für späteren UI-Trigger (z. B. Button im Header)
- loggt `appinstalled` als `console.info`

### 4. Platzhalter-Icons

Erstelle Icons im Verzeichnis `src/ITW.Web/wwwroot/img/pwa/`:

- `icon-192.png` (192×192, Hintergrund `#0F6CBD`, weißer Schriftzug `ITW` zentriert, Bold)
- `icon-512.png` (512×512, gleiche Optik)
- `icon-512-maskable.png` (512×512 mit Safe-Zone von ~20 % Rand für Android-Maskable)
- `apple-touch-icon-180.png` (180×180, einfaches PNG, iOS rundet selbst)

Falls du keine PNG-Bytes generieren kannst: Erstelle stattdessen die SVG-Vorlagen unter dem gleichen Pfad mit Endung `.svg` und beschreibe mir den Konvertierungs-Befehl (`magick`, `inkscape` oder ähnliches).

### 5. Head-Resources erweitern

Ergänze `src/ITW.Web/Views/Shared/Komponenten/_AppHeadResources.cshtml` am Ende:

```html
<link rel="manifest" href="~/site.webmanifest" />
<meta name="theme-color" content="#0F6CBD" />
<meta name="application-name" content="ITW Suite" />
<meta name="apple-mobile-web-app-capable" content="yes" />
<meta name="apple-mobile-web-app-status-bar-style" content="default" />
<meta name="apple-mobile-web-app-title" content="ITW" />
<link rel="apple-touch-icon" sizes="180x180" href="~/img/pwa/apple-touch-icon-180.png" />
<link rel="icon" type="image/png" sizes="192x192" href="~/img/pwa/icon-192.png" />
```

### 6. Skript einbinden

Ergänze `src/ITW.Web/Views/Shared/Komponenten/_AppScriptResources.cshtml` am Ende:

```html
<script src="~/js/app-pwa.js" asp-append-version="true"></script>
```

### 7. Static-Files-Konfiguration

Stelle sicher, dass `site.webmanifest` und `sw.js` mit korrektem Content-Type ausgeliefert werden:

- In `Program.cs` einen `FileExtensionContentTypeProvider` registrieren, der `.webmanifest` → `application/manifest+json` mappt, falls nicht bereits vorhanden
- `sw.js` MUSS unter dem Web-Root liegen (also direkt `wwwroot/sw.js`, **nicht** `wwwroot/js/sw.js`), sonst greift der Service-Worker-Scope nicht für die ganze App

### 8. Verifikation

- `dotnet build` über die Solution ausführen, Build muss grün sein
- Zeige mir, wo ich `https://localhost:<port>/site.webmanifest` und `https://localhost:<port>/sw.js` im Browser direkt aufrufen kann, um die Auslieferung zu prüfen
- Erkläre mir kurz, wie ich nach dem Start die Installation teste:
  - Android-Tablet (Chrome): Drei-Punkte-Menü → „App installieren"
  - iPad (Safari): Teilen-Icon → „Zum Home-Bildschirm"
- Hinweis auf Chrome-DevTools → Application-Tab → Manifest / Service Workers für die Diagnose

## Constraints

- **Verändere KEINE** bestehende Razor-View, KEINEN Controller und KEIN Modul-csproj außerhalb der oben aufgeführten Aufgaben.
- Behalte deutsche Sprache in UI-Texten und Kommentaren bei.
- Nutze die bestehende Akzentfarbe `#0F6CBD` konsequent.
- Service-Worker bewusst minimal halten: **kein** aggressives Offline-Caching der Razor-Seiten, da das mit Anti-Forgery-Tokens, Authentifizierung und TempData unschön kollidiert.
- Keine neuen NuGet-Pakete installieren – PWA ist purely Static-Files + ein paar Meta-Tags, kein Server-Code-Aufwand.

## Output-Format

Zeige mir **vor dem Speichern** jeden geänderten oder neu erstellten Datei-Inhalt als Diff oder vollständige Datei. Frag bei Mehrdeutigkeit nach. Arbeite die Aufgaben in der Reihenfolge 1 → 8 ab.
