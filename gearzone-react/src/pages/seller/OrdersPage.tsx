import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface SubOrder {
  id: string;
  orderCode?: number;
  status: string;
  totalPrice: number;
  shippingFee?: number;
  createdAt: string;
  buyerName: string;
  recipientName?: string;
  recipientPhone?: string;
  shippingAddress?: string;
  trackingNumber?: string;
  items: Array<{ productName: string; variantName?: string; quantity: number; price: number; imageUrl?: string }>;
}

const STATUS_COLORS: Record<string, string> = {
  Pending: '#ed8936', Approved: '#3182ce', Processing: '#805ad5',
  Shipping: '#d69e2e', Delivered: '#38a169', Completed: '#38a169', Cancelled: '#e53e3e',
};

const ALL_TABS = ['All', 'Pending', 'Approved', 'Processing', 'Shipping', 'Delivered', 'Completed', 'Cancelled'];

export default function SellerOrdersPage() {
  const [orders, setOrders] = useState<SubOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState('All');
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchOrders = () => {
    setLoading(true);
    sellerApi.orders.list()
      .then(d => setOrders((d as { items?: SubOrder[] }).items ?? (d as SubOrder[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchOrders(); }, []);

  const handleAction = async (id: string, action: 'approve' | 'reject' | 'markProcessing' | 'markDelivered') => {
    setProcessing(id);
    try {
      const map = {
        approve: () => sellerApi.orders.approve(id),
        reject: () => sellerApi.orders.reject(id, 'Rejected by seller'),
        markProcessing: () => sellerApi.orders.markProcessing(id),
        markDelivered: () => sellerApi.orders.markDelivered(id),
      };
      await map[action]();
      fetchOrders();
    } finally { setProcessing(null); }
  };

  const actionButtons: Record<string, Array<{ label: string; action: 'approve' | 'reject' | 'markProcessing' | 'markDelivered'; bg: string }>> = {
    Pending: [
      { label: 'Approve', action: 'approve', bg: '#38a169' },
      { label: 'Reject', action: 'reject', bg: '#e53e3e' },
    ],
    Approved: [{ label: 'Mark Processing', action: 'markProcessing', bg: '#3182ce' }],
    Processing: [{ label: 'Mark Delivered', action: 'markDelivered', bg: '#38a169' }],
  };

  const tabCounts = ALL_TABS.reduce((acc, t) => {
    acc[t] = t === 'All' ? orders.length : orders.filter(o => o.status === t).length;
    return acc;
  }, {} as Record<string, number>);

  const filtered = tab === 'All' ? orders : orders.filter(o => o.status === tab);

  const stats = [
    { label: 'Total Orders', value: orders.length, color: '#3182ce' },
    { label: 'Pending', value: orders.filter(o => o.status === 'Pending').length, color: '#ed8936' },
    { label: 'Processing', value: orders.filter(o => o.status === 'Processing' || o.status === 'Approved').length, color: '#805ad5' },
    { label: 'Delivered', value: orders.filter(o => o.status === 'Delivered' || o.status === 'Completed').length, color: '#38a169' },
  ];

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1 style={{ marginBottom: '1.25rem' }}>Orders</h1>

      {/* Stats */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {stats.map(s => (
          <div key={s.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.3rem' }}>{s.label}</div>
            <div style={{ fontWeight: 700, fontSize: 20, color: s.color }}>{s.value}</div>
          </div>
        ))}
      </div>

      {/* Status tabs */}
      <div style={{ display: 'flex', gap: '0.25rem', marginBottom: '1.25rem', flexWrap: 'wrap', borderBottom: '1px solid #e2e8f0', paddingBottom: '0' }}>
        {ALL_TABS.filter(t => tabCounts[t] > 0 || t === 'All').map(t => (
          <button key={t} onClick={() => setTab(t)}
            style={{
              padding: '0.5rem 1rem', border: 'none', borderBottom: tab === t ? '2px solid #3182ce' : '2px solid transparent',
              background: 'none', cursor: 'pointer', fontWeight: tab === t ? 700 : 400,
              color: tab === t ? '#3182ce' : '#555', fontSize: 13, whiteSpace: 'nowrap',
            }}>
            {t}
            {tabCounts[t] > 0 && t !== 'All' && (
              <span style={{ marginLeft: '0.3rem', background: tab === t ? '#3182ce' : '#e2e8f0', color: tab === t ? '#fff' : '#555', borderRadius: 10, fontSize: 11, padding: '1px 5px' }}>
                {tabCounts[t]}
              </span>
            )}
          </button>
        ))}
      </div>

      {filtered.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '3rem', color: '#888' }}>No orders in this status.</div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          {filtered.map(order => (
            <div key={order.id} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, overflow: 'hidden' }}>
              {/* Row header */}
              <div onClick={() => setExpandedId(expandedId === order.id ? null : order.id)}
                style={{ padding: '0.85rem 1.25rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', cursor: 'pointer', background: expandedId === order.id ? '#f7faff' : '#fafafa' }}>
                <div style={{ display: 'flex', gap: '1rem', alignItems: 'center', flexWrap: 'wrap' }}>
                  <span style={{ fontWeight: 700, fontFamily: 'monospace' }}>
                    #{order.orderCode ?? order.id.slice(0, 8).toUpperCase()}
                  </span>
                  <span style={{ color: '#888', fontSize: 13 }}>{new Date(order.createdAt).toLocaleDateString()}</span>
                  <span style={{ fontSize: 13 }}>{order.buyerName}</span>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
                  <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[order.status] ?? '#555', fontWeight: 600, fontSize: 12 }}>
                    {order.status}
                  </span>
                  <span style={{ fontWeight: 700 }}>{order.totalPrice?.toLocaleString()} ₫</span>
                  <span style={{ color: '#aaa', fontSize: 12 }}>{expandedId === order.id ? '▲' : '▼'}</span>
                </div>
              </div>

              {/* Expanded detail */}
              {expandedId === order.id && (
                <div style={{ padding: '1rem 1.25rem', borderTop: '1px solid #f0f0f0' }}>
                  {/* Recipient info */}
                  {(order.recipientName || order.shippingAddress) && (
                    <div style={{ fontSize: 13, color: '#555', marginBottom: '0.75rem' }}>
                      {order.recipientName && <span style={{ marginRight: '1rem' }}><strong>Recipient:</strong> {order.recipientName} {order.recipientPhone}</span>}
                      {order.shippingAddress && <span><strong>Address:</strong> {order.shippingAddress}</span>}
                    </div>
                  )}

                  {/* Items */}
                  <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13, marginBottom: '0.75rem' }}>
                    <thead>
                      <tr style={{ color: '#888', borderBottom: '1px solid #f0f0f0' }}>
                        <th style={{ textAlign: 'left', padding: '0.4rem 0', fontWeight: 500 }}>Product</th>
                        <th style={{ textAlign: 'right', padding: '0.4rem 0.5rem', fontWeight: 500 }}>Qty</th>
                        <th style={{ textAlign: 'right', padding: '0.4rem 0', fontWeight: 500 }}>Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      {order.items?.map((item, i) => (
                        <tr key={i} style={{ borderBottom: '1px solid #f9f9f9' }}>
                          <td style={{ padding: '0.4rem 0' }}>
                            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                              {item.imageUrl && <img src={item.imageUrl} alt="" style={{ width: 30, height: 30, objectFit: 'cover', borderRadius: 4 }} />}
                              <div>
                                <div>{item.productName}</div>
                                {item.variantName && <div style={{ color: '#aaa', fontSize: 11 }}>{item.variantName}</div>}
                              </div>
                            </div>
                          </td>
                          <td style={{ textAlign: 'right', padding: '0.4rem 0.5rem' }}>×{item.quantity}</td>
                          <td style={{ textAlign: 'right', padding: '0.4rem 0', fontWeight: 600 }}>{(item.price * item.quantity).toLocaleString()} ₫</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>

                  {/* Tracking + subtotal */}
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '0.5rem' }}>
                    <div>
                      {order.trackingNumber && (
                        <span style={{ fontSize: 13, color: '#555' }}>Tracking: <strong style={{ fontFamily: 'monospace' }}>{order.trackingNumber}</strong></span>
                      )}
                    </div>
                    <div style={{ fontSize: 13, color: '#555', textAlign: 'right' }}>
                      {order.shippingFee != null && <span style={{ marginRight: '1rem' }}>Shipping: {order.shippingFee.toLocaleString()} ₫</span>}
                      <strong>Total: {order.totalPrice?.toLocaleString()} ₫</strong>
                    </div>
                  </div>

                  {/* Action buttons */}
                  {actionButtons[order.status] && (
                    <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.75rem', paddingTop: '0.75rem', borderTop: '1px solid #f0f0f0' }}>
                      {actionButtons[order.status].map(btn => (
                        <button key={btn.action} onClick={() => handleAction(order.id, btn.action)}
                          disabled={processing === order.id}
                          style={{ padding: '0.4rem 1rem', background: btn.bg, color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600, fontSize: 13 }}>
                          {processing === order.id ? '…' : btn.label}
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
