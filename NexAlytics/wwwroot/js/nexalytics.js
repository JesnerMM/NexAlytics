// NEXAlytics — Core JS
(function () {
    'use strict';

    // Sidebar toggle
    const toggleBtn = document.getElementById('sidebarToggle');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function () {
            document.body.classList.toggle('sidebar-collapsed');
        });
    }

    // Auto-dismiss alerts after 5s
    document.querySelectorAll('.alert.alert-success, .alert.alert-danger').forEach(function (el) {
        if (el.querySelector('.btn-close')) {
            setTimeout(function () {
                el.classList.remove('show');
                setTimeout(function () { el.remove(); }, 300);
            }, 5000);
        }
    });
})();
