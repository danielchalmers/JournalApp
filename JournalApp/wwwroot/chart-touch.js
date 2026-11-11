// MudBlazor charts only open a tooltip on mouse hover, and a finger can't hover, so touch drives the same hover on the day nearest the finger.
// Dragging sideways across a trend chart walks the tooltip from day to day and lifting the finger puts it away, the way native charts behave.
// Pressing and holding starts the same scrub, so it can then go in any direction, including straight down from a high day to a low one.
// A tap opens the nearest day's tooltip and leaves it up until the next touch somewhere else.
(() => {
    // A finger wobbles a few pixels on a tap, so a drag only picks a direction once it has really moved.
    const dragThreshold = 8;

    // Following the line from a high day down to a low one is steep, so a drag counts as a scrub unless it is mostly up or down.
    const maxScrubSlope = 2;

    // Long enough that a finger setting off to scroll the page is already moving, short enough not to feel like a wait.
    const holdDelay = 300;

    let drag = null;
    let pinned = null;
    let lastGestureWasScrub = false;

    // The chart's own mouseover handler draws the tooltip, so pointing at a day is a synthetic hover on its invisible hover circle.
    const nearestCircle = (chart, x) => {
        let nearest = null;
        let nearestDistance = Infinity;

        for (const circle of chart.querySelectorAll('circle.mud-chart-point[opacity]')) {
            const box = circle.getBoundingClientRect();
            const distance = Math.abs(box.left + box.width / 2 - x);

            if (distance < nearestDistance) {
                nearest = circle;
                nearestDistance = distance;
            }
        }

        return nearest;
    };

    const hover = circle => circle.dispatchEvent(new MouseEvent('mouseover', { bubbles: true }));
    const unhover = circle => circle?.dispatchEvent(new MouseEvent('mouseout', { bubbles: true }));

    // A touch anywhere else puts away the tooltip a tap left open.
    document.addEventListener('pointerdown', e => {
        if (e.pointerType !== 'mouse' && pinned && !pinned.chart.contains(e.target)) {
            unhover(pinned.circle);
            pinned = null;
        }
    }, { capture: true });

    window.attachChartTouch = chart => {
        const startScrub = () => {
            drag.scrub = true;
            clearTimeout(drag.hold);

            if (pinned?.chart === chart) {
                drag.hovered = pinned.circle;
                pinned = null;
            }
        };

        const scrubTo = x => {
            const circle = nearestCircle(chart, x);

            if (circle && circle !== drag.hovered) {
                hover(circle);
                drag.hovered = circle;
            }
        };

        chart.addEventListener('pointerdown', e => {
            if (e.pointerType === 'mouse') {
                return;
            }

            const current = drag = { x: e.clientX, y: e.clientY, scrub: null, hovered: null };
            current.hold = setTimeout(() => {
                if (drag === current && current.scrub === null) {
                    startScrub();
                    scrubTo(current.x);
                }
            }, holdDelay);
        });

        chart.addEventListener('pointermove', e => {
            if (!drag) {
                return;
            }

            if (drag.scrub === null) {
                const dx = Math.abs(e.clientX - drag.x);
                const dy = Math.abs(e.clientY - drag.y);

                if (Math.hypot(dx, dy) < dragThreshold) {
                    return;
                }

                if (dy < dx * maxScrubSlope) {
                    startScrub();
                } else {
                    drag.scrub = false;
                    clearTimeout(drag.hold);

                    // Scrolling the page away from a tapped day puts its tooltip away too.
                    if (pinned?.chart === chart) {
                        unhover(pinned.circle);
                        pinned = null;
                    }
                }
            }

            if (drag.scrub) {
                scrubTo(e.clientX);
            }
        });

        // Once a drag is a scrub the page must not scroll under it, even when the finger runs downhill with the line.
        chart.addEventListener('touchmove', e => {
            if (drag?.scrub && e.cancelable) {
                e.preventDefault();
            }
        }, { passive: false });

        const endDrag = () => {
            clearTimeout(drag?.hold);
            lastGestureWasScrub = drag?.scrub === true;

            if (lastGestureWasScrub) {
                unhover(drag.hovered);
            }

            drag = null;
        };

        chart.addEventListener('pointerup', endDrag);
        chart.addEventListener('pointercancel', endDrag);

        // A tap is handled on click because that comes after the browser's own emulated mouse events, which would otherwise mouseout the day just opened.
        chart.addEventListener('click', e => {
            if (!e.pointerType || e.pointerType === 'mouse' || lastGestureWasScrub) {
                return;
            }

            const circle = nearestCircle(chart, e.clientX);

            if (circle) {
                hover(circle);
                pinned = { chart, circle };
            }
        });
    };
})();
