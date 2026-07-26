import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface Product {
  id: string;
  name: string;
  imageUrl?: string;
  basePrice: number;
  brandName: string;
  categoryName?: string;
  storeName: string;
  stockQuantity?: number;
  status: string;
  isActive: boolean;
  isApproved: boolean;
  createdAt?: string;
}

const STATUS_COLORS: Record<string, string> = {
  Active: '#38a169', Approved: '#38a169',
  Pending: '#ed8936', Draft: '#718096',
  Inactive: '#718096', Rejected: '#e53e3e',
};

export default function AdminProductsPage() {
  const navigate = useNavigate();
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchProducts = () => {
    setLoading(true);
    adminApi.products.list({ search: search || undefined, status: statusFilter || undefined })
      .then(d => setProducts((d as { items?: Product[] }).items ?? (d as Product[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchProducts(); }, []);

  const handleApprove = async (id: string) => {
    setProcessing(id);
    try { await adminApi.products.approve(id); fetchProducts(); }
    finally { setProcessing(null); }
  };

  const stats = [
    { label: 'Total Products', value: products.length, color: '#3182ce' },
    { label: 'Active', value: products.filter(p => p.isActive).length, color: '#38a169' },
    { label: 'Pending Approval', value: products.filter(p => !p.isApproved && p.status !== 'Rejected').length, color: '#ed8936' },
    { label: 'Out of Stock', value: products.filter(p => (p.stockQuantity ?? 1) === 0).length, color: '#e53e3e' },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      <h1 style={{ marginBottom: '1.25rem' }}>Products</h1>

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
        <input placeholder="Search products…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchProducts()}
          style={{ padding: '0.5rem', flex: '1 1 280px', borderRadius: 4, border: '1px solid #ccc' }} />
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 160 }}>
          <option value="">All Statuses</option>
          {['Active', 'Pending', 'Draft', 'Inactive', 'Rejected', 'Archived'].map(s => <option key={s}>{s}</option>)}
        </select>
        <button onClick={fetchProducts}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Filter
        </button>
      </div>

      {loading ? <div style={{ padding: '2rem', textAlign: 'center' }}>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Product</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Category</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Price</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Stock</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Created</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {products.map(p => (
              <tr key={p.id} style={{ borderBottom: '1px solid #eee' }}>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    {p.imageUrl
                      ? <img src={p.imageUrl} alt="" style={{ width: 36, height: 36, objectFit: 'cover', borderRadius: 4, flexShrink: 0 }} />
                      : <div style={{ width: 36, height: 36, borderRadius: 4, background: '#f0f0f0', flexShrink: 0 }} />
                    }
                    <div>
                      <div style={{ fontWeight: 500 }}>{p.name}</div>
                      <div style={{ fontSize: 12, color: '#888' }}>{p.brandName}</div>
                    </div>
                  </div>
                </td>
                <td style={{ padding: '0.75rem', fontSize: 13, color: '#555' }}>{p.categoryName ?? '—'}</td>
                <td style={{ padding: '0.75rem', fontSize: 13, color: '#555' }}>{p.storeName}</td>
                <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 600 }}>{p.basePrice?.toLocaleString()} ₫</td>
                <td style={{ padding: '0.75rem', textAlign: 'right' }}>
                  <span style={{ color: (p.stockQuantity ?? 1) === 0 ? '#e53e3e' : (p.stockQuantity ?? 99) < 5 ? '#ed8936' : '#38a169', fontWeight: 600 }}>
                    {p.stockQuantity ?? '—'}
                  </span>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[p.status] ?? '#555', fontWeight: 600, fontSize: 12 }}>
                    {p.status}
                  </span>
                </td>
                <td style={{ padding: '0.75rem', color: '#888', fontSize: 12 }}>{p.createdAt ? new Date(p.createdAt).toLocaleDateString() : '—'}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <div style={{ display: 'flex', gap: '0.4rem', justifyContent: 'center' }}>
                    <button onClick={() => navigate(`/admin/products/${p.id}`)}
                      style={{ padding: '0.2rem 0.6rem', background: '#ebf8ff', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#2b6cb0', fontSize: 12 }}>
                      Detail
                    </button>
                    {!p.isApproved && p.status !== 'Rejected' && (
                      <button onClick={() => handleApprove(p.id)} disabled={processing === p.id}
                        style={{ padding: '0.2rem 0.6rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749', fontSize: 12 }}>
                        {processing === p.id ? '…' : 'Approve'}
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {products.length === 0 && (
              <tr><td colSpan={8} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No products found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
