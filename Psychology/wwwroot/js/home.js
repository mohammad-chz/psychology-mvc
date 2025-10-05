$(function () {
    const section = document.querySelector("#stats");
    const observer = new IntersectionObserver(entries => {
        if (entries[0].isIntersecting) {
            section.querySelectorAll(".counter").forEach(el => animateCounter(el, 2000)); // 2 sec for all
            observer.disconnect(); // run once
        }
    }, { threshold: 0.3 });

    observer.observe(section);
});

function animateCounter(el, duration = 2000) {
    const target = +el.dataset.target;
    const startTime = performance.now();

    function update(now) {
        const elapsed = now - startTime;
        const progress = Math.min(elapsed / duration, 1); // 0 → 1
        const value = Math.floor(progress * target);

        el.textContent = value;

        if (progress < 1) {
            requestAnimationFrame(update);
        }
    }

    requestAnimationFrame(update);
}
