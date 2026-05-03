(function () {
    const page = document.querySelector("[data-itw-tablet-setup-page]");

    if (!page) {
        return;
    }

    const completeEndpoint = page.dataset.completeEndpoint || "/api/tablet/setup/complete";
    const trackingUrl = page.dataset.trackingUrl || "/tablet/tracking";

    const elements = {
        message: document.getElementById("tabletSetupMessage"),
        statusBadge: document.getElementById("tabletSetupStatusBadge")
    };

    let setupIsRunning = false;

    function setMessage(cssClass, text) {
        if (!elements.message) {
            return;
        }

        elements.message.className = `alert ${cssClass}`;
        elements.message.textContent = text;
    }

    function setStatus(text, cssClass) {
        if (!elements.statusBadge) {
            return;
        }

        elements.statusBadge.className = `badge ${cssClass}`;
        elements.statusBadge.textContent = text;
    }

    function normalizeCode(value) {
        return (value || "").replace(/\D/g, "").slice(0, 6);
    }

    function getCodeFromUrl() {
        const query = new URLSearchParams(window.location.search);
        return normalizeCode(query.get("code") || "");
    }

    function clearUrl() {
        window.history.replaceState(
            {},
            document.title,
            window.location.pathname);
    }

    async function completeSetup(code) {
        if (setupIsRunning) {
            return;
        }

        if (code.length !== 6) {
            setStatus("QR-Code fehlt", "bg-danger");
            setMessage(
                "alert-danger",
                "Der QR-Code ist ungültig oder abgelaufen. Bitte in der ITW-Suite einen neuen QR-Code erzeugen.");

            return;
        }

        setupIsRunning = true;

        setStatus("Einrichtung läuft", "bg-primary");
        setMessage("alert-info", "Tablet wird eingerichtet...");

        try {
            const response = await fetch(completeEndpoint, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    einrichtungscode: code
                })
            });

            const responseText = await response.text();

            let result = null;

            try {
                result = JSON.parse(responseText);
            } catch {
                result = null;
            }

            if (!response.ok || !result || result.success !== true) {
                setStatus("Fehler", "bg-danger");
                setMessage(
                    "alert-danger",
                    result && result.message
                        ? result.message
                        : "Das Tablet konnte nicht eingerichtet werden. Bitte neuen QR-Code erzeugen.");

                setupIsRunning = false;
                return;
            }

            localStorage.setItem(
                "itwTabletTracking.deviceIdentifier",
                result.deviceIdentifier);

            localStorage.setItem(
                "itwTabletTracking.apiKey",
                result.apiKey);

            localStorage.setItem(
                "itwTabletTracking.intervalSeconds",
                "10");

            localStorage.setItem(
                "itwTabletTracking.autoStart",
                "true");

            setStatus("Eingerichtet", "bg-success");
            setMessage("alert-success", "Tablet ist eingerichtet. Tracking wird geöffnet...");

            window.setTimeout(() => {
                window.location.href = trackingUrl;
            }, 700);
        } catch {
            setStatus("Fehler", "bg-danger");
            setMessage(
                "alert-danger",
                "Die Einrichtung konnte nicht abgeschlossen werden. Bitte Verbindung prüfen.");

            setupIsRunning = false;
        }
    }

    const code = getCodeFromUrl();
    clearUrl();

    window.setTimeout(() => {
        completeSetup(code);
    }, 300);
})();