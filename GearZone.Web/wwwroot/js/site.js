// Global Add to Cart Function
async function addToCart(variantId, quantity = 1, buttonElement = null, isBuyNow = false) {
    if (!variantId) {
        window.showToast?.('error', 'Please select a product variant.');
        return false;
    }

    let originalHtml = '';
    if (buttonElement) {
        originalHtml = buttonElement.innerHTML;
        buttonElement.disabled = true;
        buttonElement.innerHTML = '<span class="material-symbols-outlined text-lg animate-spin">refresh</span>';
    }

    try {
        const response = await fetch('/api/cart/add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ variantId, quantity, isBuyNow })
        });

        const isRedirected = response.redirected || (response.status === 200 && response.url.toLowerCase().includes('login'));

        if (response.status === 401 || isRedirected) {
            showLoginModal();
            return false;
        }

        if (!response.ok) {
            const data = await response.json().catch(() => ({ error: 'An error occurred while adding to cart.' }));
            window.showToast?.('error', data.error || 'An error occurred while adding to cart.');
            return false;
        }

        const data = await response.json().catch(() => ({}));
        window.showToast?.('success', 'Product added to cart successfully!');

        // Update Header Cart Count
        const cartCountEl = document.getElementById('header-cart-count');
        if (cartCountEl) {
            cartCountEl.classList.remove('hidden');
            if (data.cartCount !== undefined) {
                cartCountEl.textContent = data.cartCount;
            } else {
                let currentCount = parseInt(cartCountEl.textContent) || 0;
                cartCountEl.textContent = currentCount + quantity;
            }
        }
        return data;
    } catch (error) {
        console.error('Error adding to cart:', error);
        window.showToast?.('error', 'Server connection error.');
        return false;
    } finally {
        if (buttonElement) {
            buttonElement.disabled = false;
            buttonElement.innerHTML = originalHtml;
        }
    }
}

// Custom Modal Logic
function showLoginModal() {
    const modal = document.getElementById('login-modal');
    const confirmBtn = document.getElementById('confirm-login-modal');
    const closeBtn = document.getElementById('close-login-modal');

    if (!modal || !confirmBtn || !closeBtn) return;

    const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
    confirmBtn.href = `/Auth/Login?returnUrl=${returnUrl}`;

    modal.classList.remove('hidden');
    modal.classList.add('flex');

    const closeModal = () => {
        modal.classList.add('hidden');
        modal.classList.remove('flex');
    };

    closeBtn.onclick = closeModal;
    modal.onclick = (e) => {
        if (e.target === modal) closeModal();
    };
}

// Custom Confirm Modal
function showConfirmModal(options = {}) {
    const confirmModal = document.getElementById('confirm-modal');
    const btnConfirmProceed = document.getElementById('btn-confirm-proceed');
    const btnConfirmCancel = document.getElementById('btn-confirm-cancel');

    if (!confirmModal || !btnConfirmProceed || !btnConfirmCancel) return;

    const title = options.title || 'Are you sure?';
    const message = options.message || 'Do you really want to proceed?';
    const confirmText = options.confirmText || 'Confirm';
    const confirmClass = options.confirmClass || 'bg-red-600';
    const icon = options.icon || 'delete';
    const iconClass = options.iconClass || 'bg-red-100 text-red-600';

    document.getElementById('confirm-modal-title').textContent = title;
    document.getElementById('confirm-modal-message').textContent = message;
    btnConfirmProceed.textContent = confirmText;

    document.getElementById('confirm-modal-icon').textContent = icon;
    const iconContainer = document.getElementById('confirm-modal-icon-container');
    iconContainer.className = `w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4 ${iconClass}`;
    btnConfirmProceed.className = `flex-1 px-4 py-2.5 text-white font-semibold rounded-xl transition-all shadow-lg ${confirmClass}`;

    confirmModal.classList.remove('hidden');
    confirmModal.classList.add('flex');

    const closeConfirmModal = () => {
        confirmModal.classList.add('hidden');
        confirmModal.classList.remove('flex');
    };

    btnConfirmProceed.onclick = () => {
        if (options.onConfirm) options.onConfirm();
        closeConfirmModal();
    };

    btnConfirmCancel.onclick = closeConfirmModal;
    confirmModal.onclick = (e) => {
        if (e.target === confirmModal) closeConfirmModal();
    };
}

window.showConfirmModal = showConfirmModal;

// Custom Reason Modal
function showReasonModal(options = {}) {
    const reasonModal = document.getElementById('reason-modal');
    const btnReasonProceed = document.getElementById('btn-reason-proceed');
    const btnReasonCancel = document.getElementById('btn-reason-cancel');
    const reasonText = document.getElementById('reason-text');

    if (!reasonModal || !btnReasonProceed || !btnReasonCancel || !reasonText) return;

    const title = options.title || 'Reason Required';
    const message = options.message || 'Please provide a reason for this action.';
    const confirmText = options.confirmText || 'Confirm';
    const confirmClass = options.confirmClass || 'bg-primary';
    const icon = options.icon || 'info';
    const iconClass = options.iconClass || 'bg-amber-100 text-amber-600';
    const placeholder = options.placeholder || 'Type the reason here...';

    document.getElementById('reason-modal-title').textContent = title;
    document.getElementById('reason-modal-message').textContent = message;
    btnReasonProceed.textContent = confirmText;
    reasonText.placeholder = placeholder;
    reasonText.value = ''; // Reset

    document.getElementById('reason-modal-icon').textContent = icon;
    const iconContainer = document.getElementById('reason-modal-icon-container');
    iconContainer.className = `w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4 ${iconClass}`;
    btnReasonProceed.className = `flex-1 px-4 py-2.5 text-white font-bold rounded-xl transition-all shadow-lg ${confirmClass}`;

    reasonModal.classList.remove('hidden');
    reasonModal.classList.add('flex');

    const closeReasonModal = () => {
        reasonModal.classList.add('hidden');
        reasonModal.classList.remove('flex');
    };

    btnReasonProceed.onclick = () => {
        const val = reasonText.value.trim();
        if (!val) {
            reasonText.classList.add('border-red-500');
            return;
        }
        reasonText.classList.remove('border-red-500');
        if (options.onConfirm) options.onConfirm(val);
        closeReasonModal();
    };

    btnReasonCancel.onclick = closeReasonModal;
    reasonModal.onclick = (e) => {
        if (e.target === reasonModal) closeReasonModal();
    };
}

window.showReasonModal = showReasonModal;

// Global Event Delegation for Grid Buttons
document.addEventListener('click', (e) => {
    const gridBtn = e.target.closest('.add-to-cart-grid-btn');
    if (gridBtn) {
        const variantId = gridBtn.getAttribute('data-variant-id');
        if (variantId) {
            addToCart(variantId, 1, gridBtn);
        }
    }
});
