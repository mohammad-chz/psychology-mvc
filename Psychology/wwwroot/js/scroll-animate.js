(() => {
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const items = Array.from(document.querySelectorAll('.reveal'));
    if (!items.length) return;

    if (reduceMotion || !('IntersectionObserver' in window)) {
        items.forEach(el => el.classList.add('in'));
        return;
    }

    const io = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            const el = entry.target;
            const once = el.getAttribute('data-once') === 'true'; // ← default is re-animate

            if (entry.isIntersecting) {
                const delay = parseInt(el.getAttribute('data-delay') || '0', 10);
                if (delay) el.style.transitionDelay = `${delay}ms`;
                el.classList.add('in');

                if (once) io.unobserve(el); // only stop observing if explicitly once
            } else {
                // toggle off when leaving viewport so it can animate again next time
                el.classList.remove('in');
                el.style.transitionDelay = '';
            }
        });
    }, {
        threshold: 0.15,
        rootMargin: '0px 0px -5% 0px'
    });

    items.forEach(el => io.observe(el));
})();
