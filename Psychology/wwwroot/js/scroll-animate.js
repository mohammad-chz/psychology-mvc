(() => {
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const items = Array.from(document.querySelectorAll('.reveal'));

    if (!items.length) return;

    // If user prefers reduced motion, just show everything.
    if (reduceMotion || !('IntersectionObserver' in window)) {
        items.forEach(el => el.classList.add('in'));
        return;
    }

    const io = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            const el = entry.target;
            if (entry.isIntersecting) {
                // delay via data-delay (ms)
                const delay = parseInt(el.getAttribute('data-delay') || '0', 10);
                if (delay) el.style.transitionDelay = `${delay}ms`;

                el.classList.add('in');

                // animate once by default; set data-once="false" to keep observing
                if (el.getAttribute('data-once') !== 'false') {
                    io.unobserve(el);
                }
            } else {
                // if re-animate enabled
                if (el.getAttribute('data-once') === 'false') {
                    el.classList.remove('in');
                    el.style.transitionDelay = '';
                }
            }
        });
    }, {
        root: null,
        threshold: 0.15,         // fire when ~15% is visible
        rootMargin: '0px 0px -5% 0px' // start a bit before element fully enters
    });

    items.forEach(el => io.observe(el));
})();
