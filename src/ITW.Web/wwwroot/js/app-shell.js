/* app-shell.js — Drawer-Navigation + Navbar-Dropdowns */
(function () {
    'use strict';

    /* ---------------------------------------------------------------
     * Drawer (Mobile)
     * ------------------------------------------------------------- */

    var drawer      = document.getElementById('itwDrawer');
    var overlay     = document.getElementById('itwDrawerOverlay');
    var ham         = document.getElementById('itwDrawerToggle');
    var closeBtn    = document.getElementById('itwDrawerClose');

    function openDrawer() {
        if (!drawer || !overlay) return;
        drawer.classList.add('is-open');
        overlay.classList.add('is-open');
        document.body.style.overflow = 'hidden';
        if (ham) ham.setAttribute('aria-expanded', 'true');
    }

    function closeDrawer() {
        if (!drawer || !overlay) return;
        drawer.classList.remove('is-open');
        overlay.classList.remove('is-open');
        document.body.style.overflow = '';
        if (ham) ham.setAttribute('aria-expanded', 'false');
    }

    if (ham) {
        ham.addEventListener('click', function () {
            drawer && drawer.classList.contains('is-open') ? closeDrawer() : openDrawer();
        });
    }

    if (overlay) {
        overlay.addEventListener('click', closeDrawer);
    }

    if (closeBtn) {
        closeBtn.addEventListener('click', closeDrawer);
    }

    /* ---------------------------------------------------------------
     * Drawer Accordion (itw-dg)
     * ------------------------------------------------------------- */

    if (drawer) {
        /* Close drawer when a nav link (itw-dg-item) is clicked */
        drawer.addEventListener('click', function (e) {
            if (e.target.closest('.itw-dg-item, .itw-nav__footer-link')) {
                closeDrawer();
            }
        });

        drawer.addEventListener('click', function (e) {
            var trigger = e.target.closest('[data-drawer-toggle]');
            if (!trigger) return;

            var targetId = trigger.getAttribute('data-drawer-toggle');
            var body = document.getElementById(targetId);
            if (!body) return;

            var isCollapsed = body.classList.contains('collapsed');

            /* Toggle body */
            body.classList.toggle('collapsed', !isCollapsed);

            /* Toggle button state */
            trigger.classList.toggle('collapsed', !isCollapsed);
            trigger.setAttribute('aria-expanded', String(isCollapsed));
        });
    }

    /* ---------------------------------------------------------------
     * Navbar Dropdown-Menüs (Desktop)
     * ------------------------------------------------------------- */

    function closeAllDropdowns() {
        document.querySelectorAll('[data-nav-dropdown]').forEach(function (item) {
            var dd = item.querySelector('.itw-nav-menu__dropdown');
            var btn = item.querySelector('.itw-nav-menu__btn');
            if (dd) dd.classList.remove('is-open');
            if (btn) {
                btn.classList.remove('is-open');
                btn.setAttribute('aria-expanded', 'false');
            }
        });
    }

    document.querySelectorAll('[data-nav-dropdown]').forEach(function (item) {
        var btn      = item.querySelector('.itw-nav-menu__btn');
        var dropdown = item.querySelector('.itw-nav-menu__dropdown');
        if (!btn || !dropdown) return;

        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            var isOpen = dropdown.classList.contains('is-open');
            closeAllDropdowns();
            if (!isOpen) {
                dropdown.classList.add('is-open');
                btn.classList.add('is-open');
                btn.setAttribute('aria-expanded', 'true');
            }
        });
    });

    document.addEventListener('click', closeAllDropdowns);

    document.querySelectorAll('.itw-nav-menu__dropdown').forEach(function (d) {
        d.addEventListener('click', function (e) { e.stopPropagation(); });
    });

    /* ---------------------------------------------------------------
     * ESC-Taste
     * ------------------------------------------------------------- */

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeDrawer();
            closeAllDropdowns();
        }
    });

}());
