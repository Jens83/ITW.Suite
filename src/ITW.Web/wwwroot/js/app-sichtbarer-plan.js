document.addEventListener("DOMContentLoaded", () => {
    const buttons = document.querySelectorAll("[data-plan-target]");
    const sections = document.querySelectorAll("[data-plan-section]");

    if (!buttons.length || !sections.length) {
        return;
    }

    function activateSection(targetId) {
        sections.forEach((section) => {
            const isTarget = section.id === targetId;
            section.classList.toggle("d-none", !isTarget);
        });

        buttons.forEach((button) => {
            const isTarget = button.getAttribute("data-plan-target") === targetId;
            button.classList.toggle("app-btn-accent", isTarget);
            button.classList.toggle("app-btn-outline-accent", !isTarget);
            button.setAttribute("aria-pressed", isTarget ? "true" : "false");
        });
    }

    buttons.forEach((button) => {
        button.addEventListener("click", () => {
            const targetId = button.getAttribute("data-plan-target");

            if (!targetId) {
                return;
            }

            activateSection(targetId);
        });
    });
});