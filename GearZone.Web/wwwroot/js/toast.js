/**
 * Toast Notification System for GearZone
 * Usage: toast.success('Message'), toast.error('Message'), etc.
 */
class ToastManager {
    constructor() {
        this.containerId = 'toast-container';
        this._ensureContainerExists();
    }

    _ensureContainerExists() {
        if (!document.getElementById(this.containerId)) {
            const container = document.createElement('div');
            container.id = this.containerId;
            container.className = 'fixed top-6 right-6 z-[9999] flex flex-col gap-4 max-w-sm w-full sm:w-[360px] pointer-events-none';
            document.body.appendChild(container);
        }
    }

    show(message, type = 'info', duration = 5000) {
        const container = document.getElementById(this.containerId);
        const theme = this._getTheme(type);

        const toast = document.createElement('div');
        // Setting pointer-events-auto so we can click the close button inside the pointer-events-none container
        toast.className = `transform translate-x-full opacity-0 transition-all duration-500 cubic-bezier(0.16, 1, 0.3, 1) 
                           relative overflow-hidden flex items-center p-4 pr-3 bg-white dark:bg-gray-800 rounded-[1.25rem] shadow-[0_8px_30px_rgb(0,0,0,0.08)] pointer-events-auto border border-gray-100 dark:border-gray-700`;

        toast.innerHTML = `
            <div class="flex-shrink-0 mr-4">
                <div class="flex items-center justify-center w-[38px] h-[38px] rounded-full ${theme.iconBg}">
                    <span class="material-symbols-outlined text-[20px] text-white font-bold">${theme.icon}</span>
                </div>
            </div>
            <div class="flex-grow mr-2 pt-0.5">
                <h4 class="text-[16px] font-semibold ${theme.titleColor} m-0 leading-none">${theme.title}</h4>
                <p class="text-[14px] text-gray-500 dark:text-gray-400 m-0 mt-1 leading-snug">${message}</p>
            </div>
            <button class="flex-shrink-0 w-8 h-8 flex items-center justify-center rounded-full hover:bg-gray-100 dark:hover:bg-gray-700 text-gray-400 dark:text-gray-500 transition-colors">
                <span class="material-symbols-outlined text-[20px]">close</span>
            </button>
            <div class="absolute bottom-0 left-0 h-1 ${theme.progressBg} transition-all ease-linear js-toast-progress" style="width: 100%;"></div>
        `;

        container.appendChild(toast);

        // Animate progress bar
        const progressBar = toast.querySelector('.js-toast-progress');

        // Trigger animate in
        requestAnimationFrame(() => {
            toast.classList.remove('translate-x-full', 'opacity-0');
            toast.classList.add('translate-x-0', 'opacity-100');

            if (duration > 0 && progressBar) {
                // Must delay slightly to ensure transition starts from 100%
                setTimeout(() => {
                    progressBar.style.transitionDuration = `${duration}ms`;
                    progressBar.style.width = '0%';
                }, 50); // Small delay to allow the browser to paint
            }
        });

        const closeBtn = toast.querySelector('button');
        const removeToast = () => {
            if (!toast.parentNode) return;
            // First, slide out horizontally
            toast.classList.remove('translate-x-0', 'opacity-100');
            toast.classList.add('translate-x-full', 'opacity-0');

            // After horizontal slide is mostly done, collapse vertically to close the gap
            setTimeout(() => {
                if (!toast.parentNode) return;
                toast.style.marginTop = `-${toast.offsetHeight}px`;
                toast.style.marginBottom = '0';
                toast.style.pointerEvents = 'none';
            }, 300); // 100ms delay helps avoid diagonal movement feel

            setTimeout(() => toast.remove(), 500);
        };

        closeBtn.onclick = removeToast;

        if (duration > 0) {
            setTimeout(removeToast, duration);
        }
    }

    success(message) { this.show(message, 'success'); }
    error(message) { this.show(message, 'error', 7000); }
    warning(message) { this.show(message, 'warning'); }
    info(message) { this.show(message, 'info'); }

    _getTheme(type) {
        const themes = {
            success: {
                iconBg: 'bg-emerald-500',
                titleColor: 'text-emerald-500',
                progressBg: 'bg-emerald-500',
                icon: 'check',
                title: 'Success!'
            },
            error: {
                iconBg: 'bg-[rgb(255,64,87)]', // A slight custom red to match the image closer
                titleColor: 'text-[rgb(255,64,87)]',
                progressBg: 'bg-[rgb(255,64,87)]',
                icon: 'close',
                title: 'Error!'
            },
            warning: {
                iconBg: 'bg-amber-500',
                titleColor: 'text-amber-500',
                progressBg: 'bg-amber-500',
                icon: 'priority_high', // 'priority_high' looks like an exclamation mark
                title: 'Warning!'
            },
            info: {
                iconBg: 'bg-blue-500',
                titleColor: 'text-blue-500',
                progressBg: 'bg-blue-500',
                icon: 'info',
                title: 'Info!'
            }
        };
        return themes[type] || themes.info;
    }
}

const toast = new ToastManager();
window.toast = toast;

// Alias for global usage in existing scripts
window.showToast = function(type, message) {
    if (window.toast && typeof window.toast[type] === 'function') {
        window.toast[type](message);
    } else {
        console.warn('Toast system not initialized or type not found:', type, message);
    }
};
