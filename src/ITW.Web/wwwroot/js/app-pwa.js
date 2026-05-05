/*
 * ITW NB – PWA-Bootstrap
 * ============================================================
 *
 * Aufgabe:
 *   1) Service Worker registrieren, damit der Browser die Seite
 *      als installierbar erkennt.
 *   2) Das beforeinstallprompt-Event abfangen und global verfügbar
 *      machen, falls später ein eigener "Installieren"-Button im
 *      UI eingebaut werden soll.
 *
 * Hinweise:
 *   - Auf iOS gibt es kein beforeinstallprompt – Apple verlangt
 *     "Teilen → Zum Home-Bildschirm" manuell.
 *   - Auf Android (Chrome/Edge) erscheint i.d.R. nach kurzer
 *     Nutzung ein automatischer Installations-Hinweis.
 */

(() => {
    "use strict";

    // 1) Service Worker registrieren
    if ("serviceWorker" in navigator) {
        window.addEventListener("load", () => {
            navigator.serviceWorker
                .register("/sw.js", { scope: "/" })
                .then((registration) => {
                    // eslint-disable-next-line no-console
                    console.info("[PWA] Service Worker registriert, Scope:", registration.scope);
                })
                .catch((err) => {
                    // eslint-disable-next-line no-console
                    console.warn("[PWA] Service-Worker-Registrierung fehlgeschlagen:", err);
                });
        });
    }

    // 2) beforeinstallprompt für späteren UI-Trigger zwischenspeichern
    window.deferredInstallPrompt = null;

    window.addEventListener("beforeinstallprompt", (event) => {
        // Default-Hinweis des Browsers verhindern, damit wir später
        // selbst entscheiden können, wann der Prompt ausgelöst wird.
        event.preventDefault();
        window.deferredInstallPrompt = event;
        // eslint-disable-next-line no-console
        console.info("[PWA] Installation verfügbar – beforeinstallprompt zwischengespeichert.");
    });

    window.addEventListener("appinstalled", () => {
        window.deferredInstallPrompt = null;
        // eslint-disable-next-line no-console
        console.info("[PWA] App wurde installiert.");
    });
})();
