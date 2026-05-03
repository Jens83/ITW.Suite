(() => {
    const overlay = document.getElementById("appLoadingOverlay");

    if (!overlay) {
        return;
    }

    const messageElement = overlay.querySelector("[data-app-loading-message]");
    const defaultMessage = overlay.dataset.defaultMessage || "Inhalt wird geladen...";
    let downloadHideTimer = null;

    function setMessage(message) {
        if (!messageElement) {
            return;
        }

        messageElement.textContent = message || defaultMessage;
    }

    function show(message) {
        setMessage(message);
        overlay.classList.add("is-visible");
        overlay.setAttribute("aria-hidden", "false");
        document.body.classList.add("app-loading-active");
    }

    function resetMessage() {
        setMessage(defaultMessage);
    }

    function clearDownloadTracking() {
        if (downloadHideTimer) {
            window.clearTimeout(downloadHideTimer);
            downloadHideTimer = null;
        }
    }

    function hide() {
        clearDownloadTracking();
        overlay.classList.remove("is-visible");
        overlay.setAttribute("aria-hidden", "true");
        document.body.classList.remove("app-loading-active");
        document.body.classList.remove("app-loading-boot");
        resetMessage();
    }

    function isDownloadLink(link) {
        if (!link) {
            return false;
        }

        if (link.dataset.appLoadingDownload === "true") {
            return true;
        }

        const href = link.getAttribute("href") || "";

        if (href.includes("BuchhaltungPdf")) {
            return true;
        }

        if (link.hasAttribute("download")) {
            return true;
        }

        return false;
    }

    function isIgnorableLink(link, event) {
        if (!link) {
            return true;
        }

        if (event.defaultPrevented) {
            return true;
        }

        if (event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
            return true;
        }

        const href = link.getAttribute("href");

        if (!href || href.startsWith("#") || href.startsWith("javascript:")) {
            return true;
        }

        if ((link.getAttribute("target") || "").toLowerCase() === "_blank") {
            return true;
        }

        if (link.hasAttribute("download") && !isDownloadLink(link)) {
            return true;
        }

        return false;
    }

    function formIsClientValid(form) {
        if (!window.jQuery) {
            return true;
        }

        try {
            const $form = window.jQuery(form);

            if (typeof $form.valid === "function") {
                return $form.valid();
            }
        } catch {
            return true;
        }

        return true;
    }

    function scheduleDownloadHide() {
        clearDownloadTracking();

        const hideAfterDownload = () => {
            hide();
            window.removeEventListener("focus", hideAfterDownload);
            window.removeEventListener("pageshow", hideAfterDownload);
        };

        window.addEventListener("focus", hideAfterDownload, { once: true });
        window.addEventListener("pageshow", hideAfterDownload, { once: true });

        downloadHideTimer = window.setTimeout(() => {
            hideAfterDownload();
        }, 3000);
    }

    window.AppLoading = {
        show,
        hide
    };

    document.addEventListener(
        "click",
        (event) => {
            const link = event.target.closest("[data-app-loading-link]");

            if (isIgnorableLink(link, event)) {
                return;
            }

            show(link.dataset.appLoadingText);

            if (isDownloadLink(link)) {
                scheduleDownloadHide();
            }
        },
        true
    );

    document.addEventListener(
        "submit",
        (event) => {
            const form = event.target;

            if (!(form instanceof HTMLFormElement)) {
                return;
            }

            if (!form.matches("[data-app-loading-form]")) {
                return;
            }

            if (!formIsClientValid(form)) {
                return;
            }

            show(form.dataset.appLoadingText);
        },
        true
    );

    window.addEventListener("beforeunload", () => {
        show();
    });

    window.addEventListener("load", () => {
        hide();
    });

    window.addEventListener("pageshow", () => {
        hide();
    });
})();