import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface Store {
  id: string;
  name: string;
  slug: string;
  ownerName: string;
  ownerEmail: string;
  ownerPhone?: string;
  productCount: number;
  status: string;
  isActive: boolean;
  createdAt: string;
}

const STATUS_COLORS: Record<string, string> = {
  Approved: '#38a169', Pending: '#ed8936',
  Locked: '#e53e3e', Rejected: '#e53e3e', Draft: '#718096',
};

export default function AdminStoresPage() {
  const navigate = useNavigate();
  const [stores, setStores] = useState<Store[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const fetchStores = () => {
    setLoading(true);
    adminApi.stores.list({ search: search || undefined, status: statusFilter || undefined })
      .then(d => setStores((d as { items?: Store[] }).items ?? (d as Store[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchStores(); }, []);

  const stats = [
    { label: 'Total Stores', value: stores.length, color: '#3182ce' },
    { label: 'Active', value: stores.filter(s => s.status === 'Approved' || s.isActive).length, color: '#38a169' },
    { label: 'Pending', value: stores.filter(s => s.status === 'Pending').length, color: '#ed8936' },
    { label: 'Locked / Rejected', value: stores.filter(s => s.status === 'Locked' || s.status === 'Rejected').length, color: '#e53e3e' },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      <h1 style={{ marginBottom: '1.25rem' }}>Stores</h1>

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
        <input placeholder="Search store name, owner…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchStores()}
          style={{ padding: '0.5rem', flex: '1 1 280px', borderRadius: 4, border: '1px solid #ccc' }} />
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 160 }}>
          <option value="">All Statuses</option>
          {['Approved', 'Pending', 'Locked', 'Rejected', 'Draft'].map(s => <option key={s}>{s}</option>)}
        </select>
        <button onClick={fetchStores}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Filter
        </button>
      </div>

      {loading ? <div style={{ padding: '2rem', textAlign: 'center' }}>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Owner</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Products</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Joined</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {stores.map(s => (
              <tr key={s.id} style={{ borderBottom: '1px solid #eee' }}>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ fontWeight: 600 }}>{s.name}</div>
                  <div style={{ fontSize: 12, color: '#888' }}>{s.slug}</div>
                </td>
                <td style={{ padding: '0.75rem' }}>
                  <div>{s.ownerName}</div>
                  <div style={{ fontSize: 12, color: '#888' }}>{s.ownerEmail}</div>
                  {s.ownerPhone && <div style={{ fontSize: 12, color: '#aaa' }}>{s.ownerPhone}</div>}
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right' }}>{s.productCount}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[s.status] ?? '#555', fontWeight: 600, fontSize: 12 }}>
                    {s.status}
                  </span>
                </td>
                <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(s.createdAt).toLocaleDateString()}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <button onClick={() => navigate(`/admin/stores/${s.id}`)}
                    style={{ padding: '0.25rem 0.75rem', background: '#ebf8ff', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#2b6cb0', fontSize: 12 }}>
                    Detail
                  </button>
                </td>
              </tr>
            ))}
            {stores.length === 0 && (
              <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No stores found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
