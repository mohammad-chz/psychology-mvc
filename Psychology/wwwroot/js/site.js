$(function () {
    const $header = $("#header");

    $(function () {
        const $html = $("html");
        const $body = $("body");
        const $toggle = $(".menu-toggle");
        const $nav = $("#site-nav");
        const $backdrop = $(".nav-backdrop");

        if (!$toggle.length || !$nav.length || !$backdrop.length) return;

        let lastFocused = null;

        function firstFocusable($container) {
            return $container.find("a, button, input, [tabindex]:not([tabindex='-1'])").first();
        }

        function openNav() {
            const $navLinks = $("#nav-list a");

            if (!$navLinks.hasClass("custom-text-green")) {
                $navLinks.addClass("custom-text-green");
                //$header.addClass("custom-scroll-header");
            }

            lastFocused = document.activeElement;
            $html.addClass("nav-open");
            $toggle.attr("aria-expanded", "true");
            $backdrop.prop("hidden", false);
            $body.css("overflow", "hidden");

            const $f = firstFocusable($nav);
            if ($f.length) $f.focus();
        }

        function closeNav() {
            $html.removeClass("nav-open");
            $toggle.attr("aria-expanded", "false");
            $body.css("overflow", "");
            setTimeout(() => { $backdrop.prop("hidden", true); }, 250);
            if (lastFocused && typeof lastFocused.focus === "function") lastFocused.focus();
        }

        // Toggle button
        $toggle.on("click", function () {
            const isOpen = $html.hasClass("nav-open");
            isOpen ? closeNav() : openNav();
        });

        // Click backdrop to close
        $backdrop.on("click", closeNav);

        // Close with Esc
        $(window).on("keydown", function (e) {
            if (e.key === "Escape" && $html.hasClass("nav-open")) closeNav();
        });

        // Auto-close when resizing to desktop
        $(window).on("resize", function () {
            if ($(window).width() > 992 && $html.hasClass("nav-open")) {
                closeNav();
            }
        });
    });

    $(function () {
        const $header = $("#header");
        const $imgWhite = $("#logo .logo-white");
        const $imgBlack = $("#logo .logo-black");
        const $textLogoWhite = $("#logo .logo-text-white");
        const $textLogoBlack = $("#logo .logo-text-black");

        const $hamburgerWhite = $("#header .hamburger-white");
        const $hamburgerBlack = $("#header .hamburger-black");
        const $closeWhite = $("#header .close-white");
        const $closeBlack = $("#header .close-black");

        const $navLinks = $("#nav-list a");
        const $btnLogin = $("#nav-list .btn-login");
        const $btnStart = $("#nav-list .btn-start");
        const THRESHOLD_PX = 32;

        let lastOn = null;

        function apply(scrollY) {
            const on = scrollY > THRESHOLD_PX;
            if (on === lastOn) return;

            // Header + nav styles
            $header.toggleClass("custom-scroll-header", on);
            $navLinks.toggleClass("custom-text-green", on);

            if ($(window).width() > 768) {
                $btnLogin.toggleClass("scrolled", on);
                $btnStart.toggleClass("scrolled", on);
            }

            // Swap logos (black on scroll)
            $imgBlack.toggleClass("d-none", !on).attr("aria-hidden", !on);
            $imgWhite.toggleClass("d-none", on).attr("aria-hidden", on);

            $textLogoBlack.toggleClass("d-none", !on).attr("aria-hidden", !on);

            $textLogoWhite.toggleClass("d-none", on).attr("aria-hidden", on);

            // Swap hamburger color
            $hamburgerBlack.toggleClass("d-none", !on).attr("aria-hidden", !on);
            $hamburgerWhite.toggleClass("d-none", on).attr("aria-hidden", on);

            // (Optional) swap close icon color too
            $closeBlack.toggleClass("d-none", !on).attr("aria-hidden", !on);
            $closeWhite.toggleClass("d-none", on).attr("aria-hidden", on);

            lastOn = on;
        }

        // Initial state
        apply($(window).scrollTop());

        // Scroll (rAF-throttled)
        let ticking = false;
        $(window).on("scroll", function () {
            const y = $(this).scrollTop();
            if (!ticking) {
                requestAnimationFrame(() => { apply(y); ticking = false; });
                ticking = true;
            }
        });
    });
});



