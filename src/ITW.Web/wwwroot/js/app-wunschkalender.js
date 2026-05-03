document.addEventListener("DOMContentLoaded", () => {
    const calendar = document.querySelector(".js-wunsch-calendar");

    if (!calendar) {
        return;
    }

    const wishCountElement = document.querySelector("[data-wunsch-count]");

    function setWishCount(nextValue) {
        if (!wishCountElement) {
            return;
        }

        wishCountElement.textContent = String(Math.max(0, nextValue));
    }

    function getWishCount() {
        if (!wishCountElement) {
            return 0;
        }

        const parsed = parseInt(wishCountElement.textContent || "0", 10);
        return Number.isNaN(parsed) ? 0 : parsed;
    }

    function renderStatusIcon(type) {
        if (type === "Wunsch") {
            return '<i class="bi bi-heart-fill text-accent"></i><span>Wunsch aktiv</span>';
        }

        if (type === "NichtVerfuegbar") {
            return '<i class="bi bi-slash-circle-fill text-danger"></i><span>Nicht verfügbar</span>';
        }

        return '<span>Noch keine Auswahl</span>';
    }

    calendar.addEventListener("submit", async (event) => {
        const form = event.target.closest(".js-wunsch-toggle-form");

        if (!form) {
            return;
        }

        event.preventDefault();

        const dayCell = form.closest("[data-wunsch-day]");
        const button = form.querySelector("[data-wunsch-button]");
        const statusBox = dayCell?.querySelector("[data-wunsch-status]");
        const formData = new FormData(form);
        const wunschTyp = formData.get("wunschTyp");

        if (!dayCell || !button || !wunschTyp) {
            return;
        }

        const hadWishBefore = dayCell.classList.contains("is-wish");
        const clickedButtonWasActive = button.classList.contains("is-active");
        const buttons = dayCell.querySelectorAll("[data-wunsch-button]");

        dayCell.classList.add("is-busy");
        buttons.forEach((btn) => btn.setAttribute("disabled", "disabled"));

        try {
            const response = await fetch(form.action, {
                method: "POST",
                body: formData,
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                }
            });

            const payload = await response.json().catch(() => null);

            if (!response.ok) {
                window.alert(payload?.message ?? "Der Wunsch konnte nicht gespeichert werden.");
                return;
            }

            dayCell.classList.remove("is-wish", "is-blocked");
            dayCell.querySelectorAll("[data-wunsch-button]").forEach((btn) => {
                btn.classList.remove("is-active");
            });

            if (clickedButtonWasActive) {
                if (hadWishBefore) {
                    setWishCount(getWishCount() - 1);
                }

                if (statusBox) {
                    statusBox.innerHTML = renderStatusIcon(null);
                }

                return;
            }

            if (wunschTyp === "Wunsch") {
                dayCell.classList.add("is-wish");
                button.classList.add("is-active");

                if (!hadWishBefore) {
                    setWishCount(getWishCount() + 1);
                }

                if (statusBox) {
                    statusBox.innerHTML = renderStatusIcon("Wunsch");
                }

                return;
            }

            dayCell.classList.add("is-blocked");
            button.classList.add("is-active");

            if (hadWishBefore) {
                setWishCount(getWishCount() - 1);
            }

            if (statusBox) {
                statusBox.innerHTML = renderStatusIcon("NichtVerfuegbar");
            }
        } catch (error) {
            console.error(error);
            window.alert("Beim Speichern des Wunsches ist ein Fehler aufgetreten.");
        } finally {
            dayCell.classList.remove("is-busy");
            buttons.forEach((btn) => btn.removeAttribute("disabled"));
        }
    });
});