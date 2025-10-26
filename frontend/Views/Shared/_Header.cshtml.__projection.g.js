/* BEGIN EXTERNAL SOURCE */

document.addEventListener('DOMContentLoaded', function() {
    const toggleBtn = document.getElementById('mobileMenuToggle');
    const closeBtn = document.getElementById('mobileMenuClose');
    const overlay = document.getElementById('mobileMenuOverlay');
    const drawer = document.getElementById('mobileMenuDrawer');
    const body = document.body;
    function openMenu() {
        if (drawer) drawer.classList.add('is-open');
        if (overlay) overlay.classList.add('is-open');
        body.style.overflow = 'hidden';
    }

    function closeMenu() {
        if (drawer) drawer.classList.remove('is-open');
        if (overlay) overlay.classList.remove('is-open');
        body.style.overflow = '';
    }

    if (toggleBtn) toggleBtn.addEventListener('click', openMenu);
    if (closeBtn) closeBtn.addEventListener('click', closeMenu);
    if (overlay) overlay.addEventListener('click', closeMenu);

    const subMenuToggles = document.querySelectorAll('.mobile-menu-toggle-sub');

    subMenuToggles.forEach(toggle => {
        toggle.addEventListener('click', function(e) {
            e.preventDefault();
            const parentLi = this.closest('.mobile-menu-dropdown');
            if (parentLi) {
                parentLi.classList.toggle('is-open');
            }
        });
    });
});

/* END EXTERNAL SOURCE */
