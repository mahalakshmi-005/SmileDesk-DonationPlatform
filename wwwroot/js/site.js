// Smile Desk — shared front-end behaviour

document.addEventListener('DOMContentLoaded', function () {
    // Auto-dismiss toasts after 5 seconds
    document.querySelectorAll('.sd-toast').forEach(function (toast) {
        setTimeout(function () {
            toast.style.opacity = '0';
            toast.style.transform = 'translateX(24px)';
            setTimeout(function () { toast.remove(); }, 300);
        }, 5000);
    });

    // Scroll-reveal: fade + rise elements marked with .reveal as they enter view
    var revealEls = document.querySelectorAll('.reveal');
    if (revealEls.length && 'IntersectionObserver' in window) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });
        revealEls.forEach(function (el) { observer.observe(el); });
    } else {
        revealEls.forEach(function (el) { el.classList.add('is-visible'); });
    }

    // Navbar shadow on scroll
    var navbar = document.querySelector('.sd-navbar');
    if (navbar) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 8) {
                navbar.style.boxShadow = '0 4px 16px rgba(15,23,42,.06)';
            } else {
                navbar.style.boxShadow = 'none';
            }
        });
    }
});
