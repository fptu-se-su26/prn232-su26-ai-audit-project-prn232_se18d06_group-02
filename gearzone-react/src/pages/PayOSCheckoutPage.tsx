import { useLocation, useNavigate } from 'react-router-dom';
import { checkoutApi } from '../api/checkout';
import { useState } from 'react';

interface PayOSData {
  qrCode?: string;
  checkoutUrl?: string;
  orderCode?: string | number;
  amount?: number;
  description?: string;
}

export default function PayOSCheckoutPage() {
  const location = useLocation();
  const navigate = useNavigate();
  const payosData = (location.state as { payosData?: PayOSData })?.payosData;
  const [cancelling, setCancelling] = useState(false);

  if (!payosData) {
    navigate('/cart');
    return null;
  }

  const handleCancel = async () => {
    setCancelling(true);
    try {
      if (payosData.orderCode) {
        await checkoutApi.cancelPayment(String(payosData.orderCode));
      }
      navigate('/cart');
    } finally {
      setCancelling(false);
    }
  };

  return (
    <div className="max-w-md mx-auto px-4 py-16 text-center">
      {/* Header */}
      <div className="w-20 h-20 rounded-full bg-blue-50 flex items-center justify-center mx-auto mb-6">
        <span className="material-symbols-outlined text-4xl text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>qr_code_2</span>
      </div>
      <h1 className="text-2xl font-extrabold text-gray-900 mb-2">Complete Your Payment</h1>
      <p className="text-gray-500 text-sm mb-8">Scan the QR code or open the payment page to complete your order.</p>

      {/* Amount card */}
      {(payosData.amount || payosData.description) && (
        <div className="bg-white rounded-2xl border border-gray-200 p-5 mb-6 text-left">
          {payosData.amount && (
            <div className="flex items-center justify-between mb-3">
              <span className="text-sm text-gray-500">Amount Due</span>
              <span className="text-2xl font-extrabold text-primary">{payosData.amount.toLocaleString('vi-VN')} ₫</span>
            </div>
          )}
          {payosData.description && (
            <div className="flex items-start gap-2 pt-3 border-t border-gray-100">
              <span className="material-symbols-outlined text-[16px] text-gray-400 mt-0.5">info</span>
              <p className="text-sm text-gray-600">{payosData.description}</p>
            </div>
          )}
        </div>
      )}

      {/* QR Code */}
      {payosData.qrCode && (
        <div className="bg-white rounded-2xl border border-gray-200 p-6 mb-6 inline-block w-full">
          <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-4">Scan to Pay</p>
          <img
            src={payosData.qrCode}
            alt="Payment QR Code"
            className="w-56 h-56 mx-auto border border-gray-100 rounded-xl object-contain"
          />
        </div>
      )}

      {/* Open payment page */}
      {payosData.checkoutUrl && (
        <a
          href={payosData.checkoutUrl}
          target="_blank"
          rel="noopener noreferrer"
          className="flex items-center justify-center gap-2 w-full bg-primary hover:bg-blue-700 text-white font-bold py-4 rounded-xl transition-all shadow-[0_8px_20px_-6px_rgba(26,87,219,0.4)] mb-3 text-sm"
        >
          <span className="material-symbols-outlined text-[20px]">open_in_new</span>
          Open Payment Page
        </a>
      )}

      <p className="text-xs text-gray-400 mb-5">
        After completing payment, you will be redirected automatically.
      </p>

      <button
        onClick={handleCancel}
        disabled={cancelling}
        className="flex items-center justify-center gap-2 w-full border border-gray-300 bg-white hover:bg-gray-50 text-gray-600 font-semibold py-3 rounded-xl transition-colors text-sm disabled:opacity-50"
      >
        <span className="material-symbols-outlined text-[18px]">close</span>
        {cancelling ? 'Cancelling…' : 'Cancel Payment'}
      </button>
    </div>
  );
}
