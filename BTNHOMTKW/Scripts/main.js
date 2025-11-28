$(document).ready(function () {
    const sidebar = $('#sidebar');
    const mainContent = $('#mainContent');
    const overlay = $('#sidebarOverlay');
    const toggleBtn = $('#sidebarToggle');

    // --- 1. ACTIVE MENU LOGIC ---
    function setActiveMenu() {
        const path = window.location.pathname.toLowerCase();

        $('.menu-link').each(function () {
            const href = $(this).attr('href')?.toLowerCase();
            // Kiểm tra active logic
            if (href && href !== '/' && path.includes(href)) {
                $('.menu-link').removeClass('active');
                $(this).addClass('active');
                return false;
            }
        });
    }

    // --- 2. SIDEBAR TOGGLE ---
    toggleBtn.click(function () {
        if ($(window).width() < 768) {
            sidebar.toggleClass('mobile-open');
            if (sidebar.hasClass('mobile-open')) {
                overlay.fadeIn(200);
            } else {
                overlay.fadeOut(200);
            }
        } else {
            sidebar.toggleClass('collapsed');
            mainContent.toggleClass('expanded');
        }
    });

    // Click overlay để đóng menu mobile
    overlay.click(function () {
        sidebar.removeClass('mobile-open');
        overlay.fadeOut(200);
    });

    // --- 3. INIT ---
    setActiveMenu();

    $(window).resize(function () {
        if ($(window).width() >= 768) {
            sidebar.removeClass('mobile-open');
            overlay.hide();
        }
    });
});