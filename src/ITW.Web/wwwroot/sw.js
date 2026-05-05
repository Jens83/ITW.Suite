/*
 * ITW NB – Service Worker (minimal, bewusst klein gehalten)
 * ============================================================
 *
 * Strategie:
 *   - HTML-Navigationen: NETWORK-FIRST (immer aktuelle Razor-Seite holen,
 *     nichts cachen). Verhindert, dass alte Versionen mit alten
 *     AntiForgery-Tokens, alten Auth-Cookies oder alten Sessions
 *     ausgeliefert werden.
 *   - Statische Assets unter /css/, /js/, /lib/, /img/: CACHE-FIRST,
 *     aber Cache wird beim Activate-Event jeder neuen SW-Version
 *     verworfen.
 *   - Alles andere (POST, Login, API): unverändert ans Netzwerk.
 *
 * Cache-Bust: bei jeder substantiellen Änderung an dieser Datei
 * den CACHE-Namen hochzählen ("itw-suite-v2", "itw-suite-v3", ...).
 */

const CACHE = "itw-suite-v1";

const STATIC_PREFIXES = ["/css/", "/js/", "/lib/", "/img/"];

self.addEventListener("install", (event) => {
    // Sofort aktiv werden, ohne auf Tab-Schließung zu warten.
    self.skipWaiting();
});

self.addEventListener("activate", (event) => {
    event.waitUntil(
        (async () => {
            const keys = await caches.keys();
            await Promise.all(
                keys
                    .filter((key) => key !== CACHE)
                    .map((key) => caches.delete(key))
            );
            await self.clients.claim();
        })()
    );
});

function isStaticAsset(url) {
    return STATIC_PREFIXES.some((prefix) => url.pathname.startsWith(prefix));
}

self.addEventListener("fetch", (event) => {
    const request = event.request;

    // Nur GET wird gecacht. POST/PUT/DELETE/etc. immer durchreichen.
    if (request.method !== "GET") {
        return;
    }

    const url = new URL(request.url);

    // Nur eigene Origin behandeln (kein Cross-Origin-Caching).
    if (url.origin !== self.location.origin) {
        return;
    }

    // HTML-Navigationen: Network-First, kein Caching.
    if (request.mode === "navigate") {
        event.respondWith(fetch(request));
        return;
    }

    // Statische Assets: Cache-First mit Background-Refresh.
    if (isStaticAsset(url)) {
        event.respondWith(cacheFirst(request));
    }
});

async function cacheFirst(request) {
    const cache = await caches.open(CACHE);
    const cached = await cache.match(request);

    if (cached) {
        // Stille Hintergrund-Aktualisierung (Stale-While-Revalidate)
        fetch(request)
            .then((response) => {
                if (response && response.ok) {
                    cache.put(request, response.clone());
                }
            })
            .catch(() => { /* Offline – kein Drama */ });
        return cached;
    }

    try {
        const response = await fetch(request);
        if (response && response.ok) {
            cache.put(request, response.clone());
        }
        return response;
    } catch (err) {
        // Wenn das Asset nicht erreichbar UND nicht im Cache ist,
        // gibt es nichts mehr zu retten.
        return Response.error();
    }
}
