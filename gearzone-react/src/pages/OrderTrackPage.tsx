import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { ordersApi } from '../api/orders';

interface TrackingStep { status: string; description: string; timestamp?: string; }
interface TrackingData {
  subOrderId: string;
  status: string;
  productNames: string[];
  timeline: TrackingStep[];
  estimatedDelivery?: string;
  trackingNumber?: string;
}

const STATUS_STYLE: Record<string, { bg: string; text: string; icon: string }> = {
  Pending:    { bg: 'bg-orange-50', text: 'text-orange-600', icon: 'schedule' },
  Processing: { bg: 'bg-blue-50',   text: 'text-blue-600',   icon: 'autorenew' },
  Shipping:   { bg: 'bg-purple-50', text: 'text-purple-600', icon: 'local_shipping' },
  Delivered:  { bg: 'bg-green-50',  text: 'text-green-600',  icon: 'check_circle' },
  Cancelled:  { bg: 'bg-red-50',    text: 'text-red-500',    icon: 'cancel' },
};

export default function OrderTrackPage() {
  const { subOrderId } = useParams<{ subOrderId: string }>();
  const [data, setData] = useState<TrackingData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!subOrderId) return;
    ordersApi.track(subOrderId).then(d => setData(d as TrackingData)).finally(() => setLoading(false));
  }, [subOrderId]);

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  );
  if (!data) return (
    <div className="text-center py-24 text-gray-500">
      <span className="material-symbols-outlined text-6xl text-gray-200 mb-4 block">search_off</span>
      <p className="font-medium">Order not found.</p>
    </div>
  );

  const statusStyle = STATUS_STYLE[data.status] ?? { bg: 'bg-gray-50', text: 'text-gray-600', icon: 'info' };

  return (
    <div className="max-w-2xl mx-auto px-4 sm:px-6 py-10">
      <div className="flex items-center gap-2 text-sm text-gray-500 mb-6">
        <Link to="/profile?tab=orders" className="hover:text-primary transition-colors">My Orders</Link>
        <span className="material-symbols-outlined text-[16px]">chevron_right</span>
        <span className="text-gray-900 font-medium">Order Tracking</span>
      </div>

      {/* Order summary card */}
      <div className="bg-white rounded-2xl border border-gray-200 p-6 mb-6">
        <div className="flex items-start justify-between mb-4">
          <div>
            <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-1">Order ID</p>
            <p className="font-mono font-bold text-gray-900">#{data.subOrderId.slice(0, 12).toUpperCase()}</p>
          </div>
          <span className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-sm font-bold ${statusStyle.bg} ${statusStyle.text}`}>
            <span className="material-symbols-outlined text-[16px]">{statusStyle.icon}</span>
            {data.status}
          </span>
        </div>

        <div className="grid grid-cols-2 gap-4 text-sm">
          {data.trackingNumber && (
            <div>
              <p className="text-xs text-gray-400 mb-0.5">Tracking Number</p>
              <p className="font-mono font-semibold text-gray-700">{data.trackingNumber}</p>
            </div>
          )}
          {data.estimatedDelivery && (
            <div>
              <p className="text-xs text-gray-400 mb-0.5">Estimated Delivery</p>
              <p className="font-semibold text-gray-700">{data.estimatedDelivery}</p>
            </div>
          )}
        </div>

        {data.productNames?.length > 0 && (
          <div className="mt-4 pt-4 border-t border-gray-100">
            <p className="text-xs text-gray-400 mb-1.5">Items</p>
            <p className="text-sm text-gray-700">{data.productNames.join(', ')}</p>
          </div>
        )}
      </div>

      {/* Timeline */}
      <div className="bg-white rounded-2xl border border-gray-200 p-6">
        <h2 className="text-lg font-bold text-gray-900 mb-6">Tracking Timeline</h2>
        <div className="relative">
          {data.timeline?.map((step, i) => {
            const isFirst = i === 0;
            const isLast = i === data.timeline.length - 1;
            return (
              <div key={i} className="relative flex gap-4 pb-6 last:pb-0">
                {/* Line */}
                {!isLast && <div className="absolute left-4 top-8 bottom-0 w-0.5 bg-gray-200" />}

                {/* Dot */}
                <div className={`relative z-10 flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center ${isFirst ? 'bg-primary' : 'bg-gray-200'}`}>
                  <span className={`material-symbols-outlined text-[14px] ${isFirst ? 'text-white' : 'text-gray-400'}`}
                    style={{ fontVariationSettings: "'FILL' 1" }}>
                    {isFirst ? 'radio_button_checked' : 'radio_button_unchecked'}
                  </span>
                </div>

                {/* Content */}
                <div className="flex-1 pt-1">
                  <p className={`font-semibold text-sm ${isFirst ? 'text-primary' : 'text-gray-700'}`}>{step.status}</p>
                  <p className="text-sm text-gray-500 mt-0.5">{step.description}</p>
                  {step.timestamp && (
                    <p className="text-xs text-gray-400 mt-1">{new Date(step.timestamp).toLocaleString('vi-VN')}</p>
                  )}
                </div>
              </div>
            );
          })}
          {(!data.timeline || data.timeline.length === 0) && (
            <p className="text-gray-400 text-sm">No tracking events yet.</p>
          )}
        </div>
      </div>
    </div>
  );
}
