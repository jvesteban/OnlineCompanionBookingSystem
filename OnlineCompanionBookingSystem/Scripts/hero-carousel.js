// hero-carousel.js: the image carousel in the landing page hero.
//
// The current image is in the middle and the previous and next images peek in from the sides. The images only slide:
// there is no fade. It never moves by itself; the visitor goes to another image with the arrow buttons, the dots, the
// left/right arrow keys (when the carousel has focus), a click on a side image, or a swipe on a touch screen.
//
// How the endless loop works: five image elements sit side by side (two off-screen on each end, so a picture always
// slides in from outside the screen). When the carousel moves one step, every element moves one place, and the element
// that was farthest on one end jumps unseen to the other end and takes the next picture. The sliding, and the soft blur
// of the pictures on the sides, are done by the CSS in Landing.css; this script only decides the position of each element
// (--pos) and whether it is in the middle (--side).
(function () {
    'use strict';

    var root = document.getElementById('heroCarousel');
    if (!root) return;

    var viewport = root.querySelector('.carousel-viewport');
    var originals = root.querySelectorAll('.carousel-slide');
    var dots = root.querySelectorAll('.carousel-dot');
    var status = root.querySelector('.carousel-status');
    var prev = root.querySelector('.carousel-prev');
    var next = root.querySelector('.carousel-next');
    var count = originals.length;
    if (count !== 3) return;   // the layout is made for exactly three pictures

    var SLIDE_MS = 600;        // keep equal to the transition time in Landing.css

    // The three pictures
    var pictures = [];
    for (var p = 0; p < count; p++) pictures.push({ src: originals[p].getAttribute('src'), alt: originals[p].getAttribute('alt') || '' });

    // Five elements, ordered by position -2, -1, 0, 1, 2. The three in the page are reused (the page has the first picture
    // in the middle, the second on the right, and the third on the left); two copies are added for the far ends.
    var current = 0;
    var ring = [originals[1].cloneNode(false), originals[2], originals[0], originals[1], originals[2].cloneNode(false)];
    viewport.insertBefore(ring[0], originals[0]);
    viewport.appendChild(ring[4]);
    // keep the arrow buttons after the pictures in the page order
    viewport.appendChild(prev);
    viewport.appendChild(next);

    var busy = false;

    function pictureAt(position) {
        return pictures[(((current + position) % count) + count) % count];
    }

    // Puts an element at a position and gives it the picture for that position. jump = move without animation.
    function place(node, position, jump) {
        var picture = pictureAt(position);
        if (jump) node.classList.add('is-jumping');
        node.removeAttribute('data-pos');
        node.style.setProperty('--pos', position);
        node.style.setProperty('--side', position === 0 ? 0 : 1);   // the middle picture is sharp, the side ones are blurred
        if (node.getAttribute('src') !== picture.src) node.setAttribute('src', picture.src);
        node.setAttribute('alt', position === 0 ? picture.alt : '');
        node.setAttribute('aria-hidden', position === 0 ? 'false' : 'true');
        node.style.cursor = position === 0 ? 'default' : 'pointer';
        node.removeAttribute('loading');
        if (jump) {
            void node.offsetWidth;                    // apply the move now, then switch the animation back on
            node.classList.remove('is-jumping');
        }
    }

    for (var i = 0; i < ring.length; i++) place(ring[i], i - 2, true);

    function updateIndicators() {
        for (var d = 0; d < dots.length; d++) {
            dots[d].classList.toggle('is-active', d === current);
            if (d === current) dots[d].setAttribute('aria-current', 'true'); else dots[d].removeAttribute('aria-current');
        }
        if (status) status.textContent = 'Image ' + (current + 1) + ' of ' + count;
    }

    // direction = 1 goes to the next picture (everything slides left), -1 to the previous one (everything slides right)
    function step(direction) {
        if (busy) return;   // wait for the slide to finish, so an element never jumps while it is still on screen
        busy = true;

        current = (current + direction + count) % count;

        var wrapped;
        if (direction === 1) { wrapped = ring.shift(); ring.push(wrapped); }
        else { wrapped = ring.pop(); ring.unshift(wrapped); }

        // The element that wrapped around is off-screen on both ends, so it jumps; the others slide one place
        for (var i = 0; i < ring.length; i++) place(ring[i], i - 2, ring[i] === wrapped);

        updateIndicators();

        // The next step is accepted once this slide has finished
        setTimeout(function () { busy = false; }, SLIDE_MS + 20);
    }

    // The pictures are large, so they are fetched once the page has finished loading
    window.addEventListener('load', function () {
        for (var i = 0; i < count; i++) { var img = new Image(); img.src = pictures[i].src; }
    });

    prev.addEventListener('click', function () { step(-1); });
    next.addEventListener('click', function () { step(1); });

    // A dot goes to its picture (with three pictures, every other picture is exactly one step away)
    for (var d = 0; d < dots.length; d++) {
        (function (n) {
            dots[n].addEventListener('click', function () {
                if (n === current) return;
                step(((n - current + count) % count) === 1 ? 1 : -1);
            });
        })(d);
    }

    // A click on a side picture brings it to the middle
    viewport.addEventListener('click', function (e) {
        var node = e.target;
        var position = node && node.style ? parseInt(node.style.getPropertyValue('--pos'), 10) : 0;
        if (position === 1) step(1);
        else if (position === -1) step(-1);
    });

    // Left / right arrow keys while the buttons or dots have focus
    root.addEventListener('keydown', function (e) {
        if (e.key === 'ArrowLeft') { step(-1); e.preventDefault(); }
        else if (e.key === 'ArrowRight') { step(1); e.preventDefault(); }
    });

    // Swipe on touch screens (a clear horizontal movement of at least 40 px)
    var startX = null, startY = null;
    root.addEventListener('touchstart', function (e) {
        startX = e.touches[0].clientX;
        startY = e.touches[0].clientY;
    }, { passive: true });
    root.addEventListener('touchend', function (e) {
        if (startX === null) return;
        var dx = e.changedTouches[0].clientX - startX;
        var dy = e.changedTouches[0].clientY - startY;
        startX = startY = null;
        if (Math.abs(dx) > 40 && Math.abs(dx) > Math.abs(dy)) step(dx < 0 ? 1 : -1);
    }, { passive: true });
})();
