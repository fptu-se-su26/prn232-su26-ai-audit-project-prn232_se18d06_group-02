import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface OrderItem {
  productName: string;
  productImageUrl?: string;
  variantName?: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

interface SubOrder {
  subOrderId: string;
  storeName: string;
  status: string;
  subtotal: number;
  shippingFee: number;
  platformFeePercent?: number;
  platformFeeAmount?: number;
  netPayout?: number;
  trackingNumber?: string;
  shippingAgent?: string;
  items: OrderItem[];
}

interface StatusHistory {
  status: string;
  timestamp: string;
  note?: string;
  previousStatus?: string;
}

interface OrderDetail {
  orderId: string;
  orderCode: number;
  buyerName: string;
  buyerEmail: string;
  status: string;
  isPaid: boolean;
  paymentMethod: string;
  paymentStatus?: string;
  voucherCode?: string;
  createdAt: string;
  grandTotal: number;
  totalShipping?: number;
  recipientName?: string;
  recipientPhone?: string;
  shippingAddress: string;
  subOrders: SubOrder[];
  statusHistory?: StatusHistory[];
}

const STATUS_COLORS: Record<string, string> = {
  Pending: '#ed8936', AwaitingPayment: '#ed8936',
  Processing: '#3182ce', Shipped: '#3182ce',
  Delivered: '#38a169', Completed: '#38a169',
  Cancelled: '#e53e3e', Rejected: '#e53e3e',
};

export default function AdminOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [order, setOrder] = useState<OrderDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [copied, setCopied] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    adminApi.orders.get(id)
      .then(d => setOrder(d as OrderDetail))
      .catch(() => navigate('/admin/orders'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  const copy = (text: string, key: string) => {
    navigator.clipboard.writeText(text).then(() => { setCopied(key); setTimeout(() => setCopied(null), 2000); });
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!order) return null;

  const isFullyDelivered = (order.subOrders ?? []).every(s => s.status === 'Delivered' || s.status === 'Completed');
  const hasIssue = (order.subOrders ?? []).some(s => s.status === 'Cancelled' || s.status === 'Rejected');

  return (
    <div style={{ padding: '2rem', maxWidth: 980, margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <Link to="/admin/orders" style={{ color: '#3182ce', textDecoration: 'none' }}>← Orders</Link>
        <h1 style={{ margin: 0 }}>Order #{order.orderCode}</h1>
        <span style={{
          padding: '0.25rem 0.75rem', borderRadius: 12,
          background: order.isPaid ? '#c6f6d5' : '#fed7d7',
          color: order.isPaid ? '#276749' : '#c53030',
          fontWeight: 700, fontSize: 12,
        }}>
          {order.isPaid ? 'PAID' : 'UNPAID'}
        </span>
        <span style={{ padding: '0.25rem 0.75rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[order.status] ?? '#555', fontWeight: 600, fontSize: 13 }}>
          {order.status}
        </span>
        <span style={{ padding: '0.25rem 0.75rem', borderRadius: 12, background: isFullyDelivered ? '#c6f6d5' : hasIssue ? '#fed7d7' : '#fefcbf', color: isFullyDelivered ? '#276749' : hasIssue ? '#c53030' : '#744210', fontWeight: 600, fontSize: 12 }}>
          {isFullyDelivered ? 'FULLY DELIVERED' : hasIssue ? 'ATTENTION REQUIRED' : 'PROCESSING'}
        </span>
      </div>

      {/* Top info grid */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1.5rem' }}>
        {/* Buyer info */}
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
          <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Customer</div>
          <div style={{ fontWeight: 600, marginBottom: '0.25rem' }}>{order.buyerName}</div>
          <div style={{ color: '#888', fontSize: 13, marginBottom: '0.5rem' }}>{order.buyerEmail}</div>
          <div style={{ fontSize: 12, color: '#555' }}>
            <div style={{ marginBottom: '0.2rem' }}><strong>Recipient:</strong> {order.recipientName ?? order.buyerName}</div>
            {order.recipientPhone && <div style={{ marginBottom: '0.2rem' }}><strong>Phone:</strong> {order.recipientPhone}</div>}
            <div><strong>Address:</strong> {order.shippingAddress}</div>
          </div>
          <div style={{ marginTop: '0.5rem', fontSize: 12, color: '#aaa' }}>
            Placed: {new Date(order.createdAt).toLocaleString()}
          </div>
        </div>
        {/* Payment info */}
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
          <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Payment</div>
          {([
            ['Method', order.paymentMethod],
            ['Status', order.paymentStatus ?? (order.isPaid ? 'Paid' : 'Unpaid')],
            ...(order.voucherCode ? [['Voucher', order.voucherCode]] : []),
          ] as [string, string][]).map(([k, v]) => (
            <div key={k} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.3rem', fontSize: 13 }}>
              <span style={{ color: '#888' }}>{k}</span>
              <span style={{ fontWeight: 500 }}>{v}</span>
            </div>
          ))}
          <div style={{ borderTop: '1px solid #eee', paddingTop: '0.5rem', marginTop: '0.5rem' }}>
            {order.totalShipping != null && (
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: 13, marginBottom: '0.3rem' }}>
                <span style={{ color: '#888' }}>Shipping</span>
                <span>{(order.totalShipping ?? 0).toLocaleString()} ₫</span>
              </div>
            )}
            <div style={{ display: 'flex', justifyContent: 'space-between', fontWeight: 700 }}>
              <span>Grand Total</span>
              <span style={{ color: '#e53e3e', fontSize: 16 }}>{order.grandTotal?.toLocaleString()} ₫</span>
            </div>
          </div>
        </div>
      </div>

      {/* Sub-orders */}
      <h2 style={{ fontSize: 16, marginBottom: '0.75rem' }}>Sub-Orders ({(order.subOrders ?? []).length} store{(order.subOrders ?? []).length !== 1 ? 's' : ''})</h2>
      {(order.subOrders ?? []).map(sub => (
        <div key={sub.subOrderId} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem', marginBottom: '1rem' }}>
          {/* Sub-order header */}
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem', paddingBottom: '0.75rem', borderBottom: '1px solid #f0f0f0' }}>
            <div style={{ fontWeight: 600 }}>{sub.storeName}</div>
            <span style={{ padding: '0.2rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[sub.status] ?? '#555', fontWeight: 600, fontSize: 12 }}>
              {sub.status}
            </span>
          </div>

          {/* Items table */}
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
            <thead>
              <tr style={{ borderBottom: '1px solid #eee', color: '#888' }}>
                <th style={{ textAlign: 'left', padding: '0.4rem 0', fontWeight: 500 }}>Product</th>
                <th style={{ textAlign: 'right', padding: '0.4rem 0.5rem', fontWeight: 500 }}>Qty</th>
                <th style={{ textAlign: 'right', padding: '0.4rem 0.5rem', fontWeight: 500 }}>Unit Price</th>
                <th style={{ textAlign: 'right', padding: '0.4rem 0.5rem', fontWeight: 500 }}>Total</th>
              </tr>
            </thead>
            <tbody>
              {(sub.items ?? []).map((item, i) => (
                <tr key={i} style={{ borderBottom: '1px solid #f5f5f5' }}>
                  <td style={{ padding: '0.5rem 0', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    {item.productImageUrl && <img src={item.productImageUrl} alt="" style={{ width: 36, height: 36, objectFit: 'cover', borderRadius: 4, flexShrink: 0 }} />}
                    <div>
                      <div>{item.productName}</div>
                      {item.variantName && <div style={{ color: '#888', fontSize: 11 }}>{item.variantName}</div>}
                    </div>
                  </td>
                  <td style={{ textAlign: 'right', padding: '0.5rem' }}>{item.quantity}</td>
                  <td style={{ textAlign: 'right', padding: '0.5rem' }}>{item.unitPrice?.toLocaleString()} ₫</td>
                  <td style={{ textAlign: 'right', padding: '0.5rem', fontWeight: 600 }}>{item.lineTotal?.toLocaleString()} ₫</td>
                </tr>
              ))}
            </tbody>
          </table>

          {/* Financial summary */}
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '0.5rem', marginTop: '0.75rem', borderTop: '1px solid #f0f0f0', paddingTop: '0.75rem' }}>
            <div style={{ textAlign: 'center', background: '#f9f9f9', borderRadius: 6, padding: '0.5rem' }}>
              <div style={{ fontSize: 11, color: '#888', marginBottom: '0.2rem' }}>Subtotal (Gross)</div>
              <div style={{ fontWeight: 700 }}>{sub.subtotal?.toLocaleString()} ₫</div>
            </div>
            {sub.platformFeePercent != null && (
              <div style={{ textAlign: 'center', background: '#fff5f5', borderRadius: 6, padding: '0.5rem' }}>
                <div style={{ fontSize: 11, color: '#888', marginBottom: '0.2rem' }}>Platform Fee ({sub.platformFeePercent}%)</div>
                <div style={{ fontWeight: 700, color: '#e53e3e' }}>-{(sub.platformFeeAmount ?? 0).toLocaleString()} ₫</div>
              </div>
            )}
            {sub.netPayout != null && (
              <div style={{ textAlign: 'center', background: '#f0fff4', borderRadius: 6, padding: '0.5rem' }}>
                <div style={{ fontSize: 11, color: '#888', marginBottom: '0.2rem' }}>Store Net Payout</div>
                <div style={{ fontWeight: 700, color: '#38a169' }}>{sub.netPayout?.toLocaleString()} ₫</div>
              </div>
            )}
          </div>

          {/* Shipping info */}
          {(sub.trackingNumber || sub.shippingAgent) && (
            <div style={{ marginTop: '0.75rem', paddingTop: '0.75rem', borderTop: '1px solid #f0f0f0', fontSize: 13 }}>
              {sub.shippingAgent && <span style={{ color: '#888', marginRight: '0.75rem' }}>Carrier: <strong>{sub.shippingAgent}</strong></span>}
              {sub.trackingNumber && (
                <span>
                  Tracking: <strong style={{ fontFamily: 'monospace' }}>{sub.trackingNumber}</strong>
                  <button onClick={() => copy(sub.trackingNumber!, `track-${sub.subOrderId}`)}
                    style={{ marginLeft: '0.5rem', padding: '0.1rem 0.4rem', border: '1px solid #e2e8f0', borderRadius: 4, cursor: 'pointer', fontSize: 11, background: copied === `track-${sub.subOrderId}` ? '#c6f6d5' : '#fff', color: copied === `track-${sub.subOrderId}` ? '#276749' : '#555' }}>
                    {copied === `track-${sub.subOrderId}` ? 'Copied!' : 'Copy'}
                  </button>
                </span>
              )}
            </div>
          )}
        </div>
      ))}

      {/* Status history timeline */}
      {(order.statusHistory ?? []).length > 0 && (
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1.25rem', marginBottom: '1.5rem' }}>
          <h3 style={{ margin: '0 0 1rem', fontSize: 15 }}>Order Timeline</h3>
          <div style={{ position: 'relative' }}>
            <div style={{ position: 'absolute', left: 10, top: 0, bottom: 0, width: 2, background: 'linear-gradient(to bottom, #3182ce, #e2e8f0)' }} />
            {(order.statusHistory ?? []).map((h, i) => (
              <div key={i} style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', position: 'relative' }}>
                <div style={{ width: 22, height: 22, borderRadius: '50%', background: i === 0 ? '#3182ce' : '#e2e8f0', border: '2px solid #fff', flexShrink: 0, zIndex: 1, marginTop: 1 }} />
                <div>
                  <div style={{ fontWeight: 600, fontSize: 13 }}>{h.status}</div>
                  <div style={{ fontSize: 12, color: '#aaa' }}>{new Date(h.timestamp).toLocaleString()}</div>
                  {h.note && <div style={{ fontSize: 12, color: '#555', marginTop: '0.2rem' }}>{h.note}</div>}
                  {h.previousStatus && <div style={{ fontSize: 11, color: '#bbb' }}>from {h.previousStatus}</div>}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
