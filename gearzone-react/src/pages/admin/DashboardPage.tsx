import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface AdminStats {
  totalUsers: number;
  totalOrders: number;
  totalRevenue: number;
  totalStores: number;
  pendingStoreApplications: number;
  pendingPayouts: number;
}

export default function AdminDashboardPage() {
  const [stats, setStats] = useState<AdminStats | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminApi.getDashboard()
      .then(d => setStats(d as AdminStats))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!stats) return <div style={{ padding: '2rem' }}>Failed to load dashboard.</div>;

  const statCards = [
    { label: 'Total Users', value: stats.totalUsers, color: '#3182ce', link: '/admin/users' },
    { label: 'Total Orders', value: stats.totalOrders, color: '#ed8936', link: '/admin/orders' },
    { label: 'Total Revenue', value: `${(stats.totalRevenue ?? 0).toLocaleString()} VND`, color: '#38a169', link: '/admin/transactions' },
    { label: 'Active Stores', value: stats.totalStores, color: '#805ad5', link: '/admin/stores' },
    { label: 'Pending Applications', value: stats.pendingStoreApplications, color: '#e53e3e', link: '/admin/store-applications' },
    { label: 'Pending Payouts', value: stats.pendingPayouts, color: '#d69e2e', link: '/admin/payouts' },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Admin Dashboard</h1>

      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '2rem' }}>
        {statCards.map(s => (
          <Link key={s.label} to={s.link}
            style={{ flex: '1 1 200px', padding: '1.5rem', background: '#f9f9f9', borderRadius: 8, borderLeft: `4px solid ${s.color}`, textDecoration: 'none', color: 'inherit' }}>
            <p style={{ margin: 0, color: '#888', fontSize: 13 }}>{s.label}</p>
            <p style={{ margin: '0.5rem 0 0', fontSize: 24, fontWeight: 700, color: s.color }}>{s.value}</p>
          </Link>
        ))}
      </div>

      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
        {[
          { to: '/admin/products', label: 'Products' },
          { to: '/admin/brands', label: 'Brands' },
          { to: '/admin/categories', label: 'Categories' },
          { to: '/admin/vouchers', label: 'Vouchers' },
          { to: '/admin/settings', label: 'Settings' },
          { to: '/admin/wallet', label: 'Wallet' },
        ].map(l => (
          <Link key={l.to} to={l.to} style={{ padding: '0.5rem 1.5rem', border: '1px solid #ccc', borderRadius: 4, textDecoration: 'none', color: '#333' }}>
            {l.label}
          </Link>
        ))}
      </div>
    </div>
  );
}
