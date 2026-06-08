import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface Order {
  id: string;
  orderCode?: number;
  buyerName: string;
  recipientName?: string;
  totalPrice: number;
  status: string;
  paymentStatus: string;
  isPaid?: boolean;
  subOrderCount: number;
  createdAt: string;
}

const STATUS_COLORS: Record<string, string> = {
  Pending: '#ed8936', AwaitingPayment: '#ed8936',
  Processing: '#3182ce', Shipped: '#3182ce',
  Delivered: '#38a169', Completed: '#38a169',
  Cancelled: '#e53e3e', Rejected: '#e53e3e',
};

export default function AdminOrdersPage() {
  const navigate = useNavigate();
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [paymentFilter, setPaymentFilter] = useState('');

  const fetchOrders = () => {
    setLoading(true);
    adminApi.orders.list({ search: search || undefined, status: statusFilter || undefined, paymentStatus: paymentFilter || undefined })
      .then(d => setOrders((d as { items?: Order[] }).items ?? (d as Order[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchOrders(); }, []);

  const paid = orders.filter(o => o.isPaid ?? o.paymentStatus === 'Paid');
  const stats = [
    { label: 'Total Orders', value: orders.length, color: '#3182ce' },
    { label: 'Paid', value: paid.length, color: '#38a169' },
    { label: 'Unpaid', value: orders.length - paid.length, color: '#e53e3e' },
    { label: 'Total Revenue', value: `${orders.reduce((s, o) => s + (o.totalPrice ?? 0), 0).toLocaleString()} ₫`, color: '#805ad5' },
  ];

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

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <input placeholder="Search buyer, order code…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchOrders()}
          style={{ padding: '0.5rem', flex: '1 1 280px', borderRadius: 4, border: '1px solid #ccc' }} />
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 160 }}>
          <option value="">All Statuses</option>
          {['Pending', 'AwaitingPayment', 'Processing', 'Shipped', 'Delivered', 'Completed', 'Cancelled'].map(s =>
            <option key={s}>{s}</option>)}
        </select>
        <select value={paymentFilter} onChange={e => setPaymentFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 160 }}>
          <option value="">All Payment Statuses</option>
          <option value="Paid">Paid</option>
          <option value="Unpaid">Unpaid</option>
        </select>
        <button onClick={fetchOrders}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Filter
        </button>
      </div>

      {loading ? <div style={{ padding: '2rem', textAlign: 'center' }}>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Order</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Customer</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Total</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Payment</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Date</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {orders.map(o => (
              <tr key={o.id} style={{ borderBottom: '1px solid #eee' }}>
                <td style={{ padding: '0.75rem', fontFamily: 'monospace', fontSize: 13 }}>
                  #{o.orderCode ?? o.id.slice(0, 8).toUpperCase()}
                  {o.subOrderCount > 1 && <span style={{ marginLeft: '0.4rem', background: '#e2e8f0', borderRadius: 10, fontSize: 11, padding: '1px 5px' }}>{o.subOrderCount} stores</span>}
                </td>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ fontWeight: 500 }}>{o.buyerName}</div>
                  {o.recipientName && o.recipientName !== o.buyerName && <div style={{ fontSize: 12, color: '#888' }}>{o.recipientName}</div>}
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 600 }}>{o.totalPrice?.toLocaleString()} ₫</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[o.status] ?? '#555', fontWeight: 600, fontSize: 12 }}>
                    {o.status}
                  </span>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, fontSize: 12, fontWeight: 600,
                    background: (o.isPaid ?? o.paymentStatus === 'Paid') ? '#c6f6d5' : '#fed7d7',
                    color: (o.isPaid ?? o.paymentStatus === 'Paid') ? '#276749' : '#c53030' }}>
                    {o.isPaid ?? o.paymentStatus === 'Paid' ? 'Paid' : 'Unpaid'}
                  </span>
                </td>
                <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(o.createdAt).toLocaleDateString()}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <button onClick={() => navigate(`/admin/orders/${o.id}`)}
                    style={{ padding: '0.25rem 0.75rem', background: '#ebf8ff', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#2b6cb0', fontSize: 12 }}>
                    Detail
                  </button>
                </td>
              </tr>
            ))}
            {orders.length === 0 && (
              <tr><td colSpan={7} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No orders found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
