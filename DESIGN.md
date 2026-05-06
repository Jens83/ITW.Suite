---
name: ITW Corporate Admin
description: Zentrales Managementsystem — Intensivtransport Neubrandenburg
colors:
  # --- Surfaces (Light Mode) ---
  background:             '#ffffff'
  surface-0:              '#ffffff'
  surface-1:              '#f9fafb'
  surface-2:              '#f3f4f6'
  surface-3:              '#e5e7eb'
  surface-4:              '#d1d5db'
  # --- Text (Light Mode) ---
  on-surface:             '#262626'
  on-surface-variant:     '#374151'
  outline:                '#6b7280'
  outline-variant:        '#e5e7eb'
  # --- Primary — Amber/Gelb ---
  primary:                '#f59e0b'
  primary-dark:           '#d97706'
  primary-fixed:          '#fffbeb'
  on-primary:             '#1c1917'
  # --- Secondary — Violett ---
  secondary:              '#7c3aed'
  secondary-fixed:        '#ede9fe'
  on-secondary-container: '#f5f3ff'
  # --- Tertiary — Grün (Erfolg/OK) ---
  tertiary:               '#16a34a'
  tertiary-dark:          '#15803d'
  on-tertiary:            '#ffffff'
  on-tertiary-container:  '#f0fdf4'
  tertiary-fixed-dim:     '#4ade80'
  # --- Error — Rot ---
  error:                  '#ef4444'
  error-container:        '#fee2e2'
  on-error-container:     '#b91c1c'
  # --- Warning — Orange ---
  warning:                '#ea580c'
  warning-container:      '#fff7ed'
  warning-border:         '#fed7aa'
  # --- Surfaces (Dark Mode) ---
  dark-background:        '#0c0a09'
  dark-surface-0:         '#1c1917'
  dark-surface-1:         '#292524'
  dark-surface-2:         '#3c3836'
  dark-surface-3:         '#44403c'
  dark-surface-4:         '#57534e'
  # --- Text (Dark Mode) ---
  dark-on-surface:        '#fafaf9'
  dark-on-surface-variant:'#e7e5e4'
  dark-outline:           '#a8a29e'
  dark-outline-variant:   '#44403c'
  # --- Primary Dark Mode ---
  dark-primary:           '#fbbf24'
  dark-primary-dark:      '#f59e0b'
  dark-on-primary:        '#1c1917'
  # --- Tertiary Dark Mode ---
  dark-tertiary:          '#4ade80'
  dark-tertiary-container:'#166534'
  # --- Error Dark Mode ---
  dark-error:             '#f87171'
  dark-error-container:   'rgba(248,113,113,0.15)'
typography:
  h1:
    fontFamily: Plus Jakarta Sans
    fontSize: 28px
    fontWeight: '700'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  h2:
    fontFamily: Plus Jakarta Sans
    fontSize: 22px
    fontWeight: '700'
    lineHeight: '1.3'
  body-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 14px
    fontWeight: '500'
    lineHeight: '1.6'
  body-sm:
    fontFamily: Plus Jakarta Sans
    fontSize: 12px
    fontWeight: '400'
    lineHeight: '1.5'
  label-caps:
    fontFamily: Plus Jakarta Sans
    fontSize: 11px
    fontWeight: '600'
    lineHeight: '1'
    letterSpacing: 0.05em
  stat-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 24px
    fontWeight: '700'
    lineHeight: '1.1'
rounded:
  sm:      0.25rem
  DEFAULT: 0.5rem
  md:      0.75rem
  lg:      1rem
  xl:      1.5rem
  full:    9999px
spacing:
  container-padding: 2rem
  card-gap:          1.5rem
  card-padding:      1.25rem
  element-margin:    0.75rem
layout:
  navbar-height:     56px
  sidebar-width:     288px
  content-max-width: 1240px
buttons:
  height:        36px
  height-sm:     30px
  height-lg:     44px
  border-width:  2px
  border-radius: 0.75rem
  font-weight:   '600'
shadows:
  card:  '0 2px 20px rgba(245,158,11,0.08), 0 1px 3px rgba(0,0,0,0.04)'
  hover: '0 4px 28px rgba(245,158,11,0.13), 0 2px 6px rgba(0,0,0,0.05)'
  drop:  '0 8px 32px rgba(245,158,11,0.15), 0 2px 8px rgba(0,0,0,0.07)'
---

## Brand & Kontext

Dieses Design-System trägt die Verwaltungssoftware der **Intensivtransport Neubrandenburg**. Das visuelle Konzept kombiniert klare Funktionalität mit einem selbstbewussten, wiedererkennbaren Auftritt — **kein generisches Admin-Dashboard**, sondern ein System mit eigenem Charakter.

Die Tonalität ist professionell und direkt: Amber als Primärfarbe kommuniziert Energie und Sichtbarkeit, ohne auf generische Blau-Grau-Enterprise-Ästhetik zurückzufallen. Dark Mode wird systembasiert automatisch aktiviert (`prefers-color-scheme`).

## Farben

Das Farbsystem basiert auf **Amber als Primary**, ergänzt durch semantische Rollen:

- **Primary (Amber `#f59e0b`)** — Haupt-CTA, aktive Zustände, Akzente. Immer mit dunkler (`#1c1917`) Schrift — kein Weiß auf Gelb.
- **Tertiary (Grün `#16a34a`)** — Erfolg, Bestätigung, OK-Zustände.
- **Error (Rot `#ef4444`)** — Fehler, kritische Aktionen, Löschoperationen.
- **Warning (Orange `#ea580c`)** — Warnhinweise, ausstehende Aktionen. Visuell von Amber trennbar.
- **Secondary (Violett `#7c3aed`)** — Sekundäre Kategorisierung, Modul-Akzente (Fahrzeugmanagement).
- **Neutrals** — Flächen und Text über fünf Surface-Stufen (`surface-0` bis `surface-4`).

**Dark Mode** (Tailwind Stone-Palette, warm-dunkel): Alle Token werden via `@media (prefers-color-scheme: dark)` überschrieben. Amber wird auf `#fbbf24` aufgehellt. Schatten werden neutral-dunkel.

**Bereichsfarben** überschreiben `--area-color` per Theme-Klasse:
- `.theme-itw` → Amber (Primary)
- `.theme-dienstplan` → Grün (Tertiary)
- `.theme-fahrzeug` → Violett (Secondary)
- `.theme-admin` → Neutral (Outline)
- `.theme-gf` → Rot (Error)

## Typografie

**Plus Jakarta Sans** — geometrisch-modern, exzellente Lesbarkeit in datendichten Umgebungen.

- Headings: Bold (700), enge Letter-Spacing für klare Abschnittanker.
- Body: Medium (500) — ausreichend Gewicht gegen helle Hintergründe.
- Labels/Chips: Semibold (600), Kapitälchen, 0.05em Tracking — für Kategorien und Status-Badges.
- Stat-Zahlen: Bold (700), erhöhte Schriftgröße — sofort ablesbar.

## Layout & Spacing

8px-Basis-Grid. Alle Abstände als Token (`--sp-cont`, `--sp-gap`, `--sp-pad`, `--sp-el`).

- **Navbar:** Sticky, 56px Höhe, `z-index: 200`. Desktop: horizontale Dropdown-Navigation. Mobile: Hamburger öffnet Drawer von links.
- **Drawer:** 288px breit, `position: fixed`, via `translateX(-100%)` verborgen. Kein Schließen-Button im Header — Schließen via Overlay-Klick oder ESC.
- **Content:** Max-Width 1240px, zentriert, 32px seitliches Padding.
- **Cards:** 16px Radius, weißer Hintergrund, Amber-getönter Ambient-Shadow.

## Elevation & Schatten

Amber-getönte Ambient-Schatten erzeugen Tiefe ohne harte Kanten:

- `card` — 2px/20px Blur, 8% Amber + 4% Schwarz
- `hover` — 4px/28px Blur, 13% Amber
- `drop` — 8px/32px Blur, 15% Amber

Dark Mode: neutrale schwarze Schatten (35–58% Opacity).

## Komponenten

**Buttons**
- Solid Primary: Amber-Hintergrund (`#f59e0b`), schwarze Schrift (`#1c1917`), 2px Border.
- Outline: Grauer Rand (`--outline`), dunkler Text. Hover → füllt sich amber (= solid).
- Border immer 2px — sichtbar, nicht filigran.
- Hover-Effekt: `box-shadow` mit Amber-Tint, kein reiner Hintergrundwechsel.

**Navigation (aktive Zustände)**
- Hintergrund: 12% Amber-Tint (`rgba(245,158,11,0.12)`)
- Schrift: `--on-s` (Schwarz) — kein Amber-Text auf Amber-Hintergrund.
- Icons im aktiven Zustand: ebenfalls dunkel.
- Hover-Icons: Amber als visueller Hinweis, Text bleibt dunkel.

**Badges & Pills**
- Alle Badges tragen **schwarze Schrift** (`--on-s`) — unabhängig von der Badge-Hintergrundfarbe.
- Hintergrundfarbe signalisiert die Semantik (grün = OK, rot = Fehler, amber = Info).
- Keine farbige Schrift auf farbigem Hintergrund.

**Formulare**
- Input-Höhe: 44px, Radius 0.75rem.
- Label: Kapitälchen, 11px, `--outline`.
- Focus: 2px Amber-Outline.

**Tabellen**
- Header: Kapitälchen-Labels, `--surface-1` Hintergrund.
- Rows: Hover-State `--surface-1`, keine farbige Markierung.
