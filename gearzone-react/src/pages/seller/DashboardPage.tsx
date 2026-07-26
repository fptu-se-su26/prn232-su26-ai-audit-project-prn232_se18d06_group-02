import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { sellerApi } from '../../api/seller';

interface DashboardData {
  totalRevenue: number;
  totalOrders: number;
  pendingOrders: number;
  totalProducts: number;
  recentOrders?: Array<{ id: string; status: string; totalPrice: number; createdAt: string }>;
}

export default function SellerDashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    sellerApi.getDashboard()
      .then(d => setData(d as DashboardData))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!data) return <div style={{ padding: '2rem' }}>Failed to load dashboard.</div>;

  const stats = [
    { label: 'Total Revenue', value: `${(data.totalRevenue ?? 0).toLocaleString()} VND`, color: '#38a169' },
    { label: 'Total Orders', value: data.totalOrders ?? 0, color: '#3182ce' },
    { label: 'Pending Orders', value: data.pendingOrders ?? 0, color: '#ed8936' },
    { label: 'Products Listed', value: data.totalProducts ?? 0, color: '#805ad5' },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Seller Dashboard</h1>

      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '2rem' }}>
        {stats.map(s => (
          <div key={s.label} style={{ flex: '1 1 200px', padding: '1.5rem', background: '#f9f9f9', borderRadius: 8, borderLeft: `4px solid ${s.color}` }}>
            <p style={{ margin: 0, color: '#888', fontSize: 13 }}>{s.label}</p>
            <p style={{ margin: '0.5rem 0 0', fontSize: 24, fontWeight: 700, color: s.color }}>{s.value}</p>
          </div>
        ))}
      </div>

      <div style={{ display: 'flex', gap: '1rem', marginBottom: '2rem', flexWrap: 'wrap' }}>
        <Link to="/seller/products" style={{ padding: '0.75rem 1.5rem', background: '#3182ce', color: '#fff', borderRadius: 6, textDecoration: 'none', fontWeight: 600 }}>Manage Products</Link>
        <Link to="/seller/orders" style={{ padding: '0.75rem 1.5rem', background: '#38a169', color: '#fff', borderRadius: 6, textDecoration: 'none', fontWeight: 600 }}>Manage Orders</Link>
        <Link to="/seller/revenue" style={{ padding: '0.75rem 1.5rem', background: '#805ad5', color: '#fff', borderRadius: 6, textDecoration: 'none', fontWeight: 600 }}>View Revenue</Link>
        <Link to="/seller/reports" style={{ padding: '0.75rem 1.5rem', background: '#dd6b20', color: '#fff', borderRadius: 6, textDecoration: 'none', fontWeight: 600 }}>View Reports</Link>
      </div>

      {data.recentOrders && data.recentOrders.length > 0 && (
        <div>
          <h2>Recent Orders</h2>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#f0f0f0' }}>
                <th style={{ padding: '0.5rem', textAlign: 'left' }}>Order ID</th>
                <th style={{ padding: '0.5rem', textAlign: 'left' }}>Status</th>
                <th style={{ padding: '0.5rem', textAlign: 'right' }}>Amount</th>
                <th style={{ padding: '0.5rem', textAlign: 'left' }}>Date</th>
              </tr>
            </thead>
            <tbody>
              {data.recentOrders.map(o => (
                <tr key={o.id} style={{ borderBottom: '1px solid #eee' }}>
                  <td style={{ padding: '0.5rem' }}>{o.id.slice(0, 8)}</td>
                  <td style={{ padding: '0.5rem' }}>{o.status}</td>
                  <td style={{ padding: '0.5rem', textAlign: 'right' }}>{o.totalPrice?.toLocaleString()} VND</td>
                  <td style={{ padding: '0.5rem', color: '#888', fontSize: 13 }}>{new Date(o.createdAt).toLocaleDateString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
