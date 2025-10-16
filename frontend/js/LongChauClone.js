function banner() {
    const images = document.querySelectorAll('.lc-banner-slider img');
    const dots = document.querySelectorAll('.lc-banner-dots .dot');
    let current = 0;
    let total = images.length;
    let interval;
    function showSlide(idx) {
        images.forEach((img, i) => {
            img.classList.toggle('active', i === idx);
        });
        dots.forEach((dot, i) => {
            dot.classList.toggle('active', i === idx);
        });
    }
    function nextSlide() {
        current = (current + 1) % total;
        showSlide(current);
    }
    function startAuto() {
        interval = setInterval(nextSlide, 3500);
    }
    function stopAuto() {
        clearInterval(interval);
    }
    dots.forEach((dot, i) => {
        dot.addEventListener('click', () => {
            stopAuto();
            current = i;
            showSlide(current);
            startAuto();
        });
    });
    showSlide(current);
    startAuto();
}