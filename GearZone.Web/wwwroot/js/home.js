(function () {
    "use strict";

    function initHome() {
        const controller = new AbortController();
        const signal = controller.signal;
        const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        const cleanupTasks = [];

        document.querySelectorAll("[data-home-carousel]").forEach(function (root) {
            const slides = Array.from(root.querySelectorAll("[data-carousel-slide]"));
            const indicators = Array.from(root.querySelectorAll("[data-carousel-indicator]"));
            const previousButton = root.querySelector("[data-carousel-prev]");
            const nextButton = root.querySelector("[data-carousel-next]");
            const pauseButton = root.querySelector("[data-carousel-pause]");
            const pauseIcon = pauseButton?.querySelector(".material-symbols-outlined");

            if (slides.length < 2) {
                return;
            }

            const intervalMs = 5000;
            let activeIndex = Math.max(0, slides.findIndex(function (slide) {
                return slide.classList.contains("is-active");
            }));
            let timerId = null;
            let manuallyPaused = reducedMotion;
            let temporarilyPaused = false;

            function stopTimer() {
                if (timerId !== null) {
                    window.clearTimeout(timerId);
                    timerId = null;
                }
            }

            function canAutoAdvance() {
                return !manuallyPaused && !temporarilyPaused && !document.hidden && !reducedMotion;
            }

            function scheduleNext() {
                stopTimer();
                if (!canAutoAdvance()) {
                    return;
                }

                timerId = window.setTimeout(function () {
                    showSlide(activeIndex + 1, false);
                }, intervalMs);
            }

            function showSlide(nextIndex, userInitiated) {
                activeIndex = (nextIndex + slides.length) % slides.length;

                slides.forEach(function (slide, index) {
                    const isActive = index === activeIndex;
                    slide.classList.toggle("is-active", isActive);
                    slide.setAttribute("aria-hidden", isActive ? "false" : "true");
                });

                indicators.forEach(function (indicator, index) {
                    const isActive = index === activeIndex;
                    indicator.classList.toggle("is-active", isActive);
                    indicator.setAttribute("aria-current", isActive ? "true" : "false");
                });

                if (userInitiated) {
                    stopTimer();
                }
                scheduleNext();
            }

            function setPaused(paused) {
                manuallyPaused = paused;
                pauseButton?.setAttribute("aria-label", paused ? "Play carousel" : "Pause carousel");
                if (pauseIcon) {
                    pauseIcon.textContent = paused ? "play_arrow" : "pause";
                }

                if (paused) {
                    stopTimer();
                } else {
                    scheduleNext();
                }
            }

            indicators.forEach(function (indicator, index) {
                indicator.addEventListener("click", function () {
                    showSlide(index, true);
                }, { signal: signal });
            });

            previousButton?.addEventListener("click", function () {
                showSlide(activeIndex - 1, true);
            }, { signal: signal });

            nextButton?.addEventListener("click", function () {
                showSlide(activeIndex + 1, true);
            }, { signal: signal });

            pauseButton?.addEventListener("click", function () {
                setPaused(!manuallyPaused);
            }, { signal: signal });

            root.addEventListener("mouseenter", function () {
                temporarilyPaused = true;
                stopTimer();
            }, { signal: signal });

            root.addEventListener("mouseleave", function () {
                temporarilyPaused = false;
                scheduleNext();
            }, { signal: signal });

            root.addEventListener("focusin", function () {
                temporarilyPaused = true;
                stopTimer();
            }, { signal: signal });

            root.addEventListener("focusout", function (event) {
                if (!root.contains(event.relatedTarget)) {
                    temporarilyPaused = false;
                    scheduleNext();
                }
            }, { signal: signal });

            root.addEventListener("keydown", function (event) {
                if (event.key === "ArrowLeft") {
                    event.preventDefault();
                    showSlide(activeIndex - 1, true);
                } else if (event.key === "ArrowRight") {
                    event.preventDefault();
                    showSlide(activeIndex + 1, true);
                }
            }, { signal: signal });

            document.addEventListener("visibilitychange", function () {
                if (document.hidden) {
                    stopTimer();
                } else {
                    scheduleNext();
                }
            }, { signal: signal });

            setPaused(manuallyPaused);
            showSlide(activeIndex, false);
            cleanupTasks.push(stopTimer);
        });

        const revealItems = Array.from(document.querySelectorAll("[data-home-reveal]"));
        if (reducedMotion || !("IntersectionObserver" in window)) {
            revealItems.forEach(function (item) {
                item.classList.add("is-visible");
            });
        } else {
            const revealObserver = new IntersectionObserver(function (entries) {
                entries.forEach(function (entry) {
                    if (!entry.isIntersecting) {
                        return;
                    }

                    entry.target.classList.add("is-visible");
                    revealObserver.unobserve(entry.target);
                });
            }, {
                rootMargin: "0px 0px -8%",
                threshold: 0.12
            });

            revealItems.forEach(function (item) {
                revealObserver.observe(item);
            });
            cleanupTasks.push(function () {
                revealObserver.disconnect();
            });
        }

        const navToggle = document.querySelector("[data-home-nav-toggle]");
        const mobileNav = document.querySelector("[data-home-mobile-nav]");

        if (navToggle && mobileNav) {
            function setNavOpen(open) {
                mobileNav.dataset.open = open ? "true" : "false";
                mobileNav.setAttribute("aria-hidden", open ? "false" : "true");
                navToggle.setAttribute("aria-expanded", open ? "true" : "false");
                document.body.classList.toggle("gz-nav-open", open);

                const icon = navToggle.querySelector(".material-symbols-outlined");
                if (icon) {
                    icon.textContent = open ? "close" : "menu";
                }

                if (open) {
                    window.requestAnimationFrame(function () {
                        mobileNav.querySelector("input, a, button")?.focus();
                    });
                }
            }

            navToggle.addEventListener("click", function () {
                setNavOpen(mobileNav.dataset.open !== "true");
            }, { signal: signal });

            mobileNav.addEventListener("click", function (event) {
                if (event.target === mobileNav || event.target.closest("a")) {
                    setNavOpen(false);
                }
            }, { signal: signal });

            document.addEventListener("keydown", function (event) {
                if (event.key === "Escape" && mobileNav.dataset.open === "true") {
                    setNavOpen(false);
                    navToggle.focus();
                }
            }, { signal: signal });

            window.matchMedia("(min-width: 768px)").addEventListener("change", function (event) {
                if (event.matches) {
                    setNavOpen(false);
                }
            }, { signal: signal });

            cleanupTasks.push(function () {
                document.body.classList.remove("gz-nav-open");
            });
        }

        window.addEventListener("pagehide", function () {
            cleanupTasks.forEach(function (cleanup) {
                cleanup();
            });
            controller.abort();
        }, { once: true });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initHome, { once: true });
    } else {
        initHome();
    }
})();
