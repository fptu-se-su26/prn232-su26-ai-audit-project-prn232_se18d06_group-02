import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface Product {
  id: string;
  name: string;
  basePrice: number;
  imageUrl?: string;
  status: string;
  isActive: boolean;
  stockQuantity: number;
  categoryName?: string;
  brandName?: string;
}

const STATUS_COLORS: Record<string, string> = {
  Active: '#38a169', Approved: '#38a169',
  Pending: '#ed8936', Draft: '#718096',
  Inactive: '#718096', Rejected: '#e53e3e',
};

const STATUS_TABS = ['All', 'Active', 'Draft', 'Pending', 'Inactive', 'Rejected'];

export default function SellerProductsPage() {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [tab, setTab] = useState('All');
  const [search, setSearch] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', basePrice: '', stockQuantity: '', description: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchProducts = () => {
    setLoading(true);
    sellerApi.products.list()
      .then(d => setProducts((d as { items?: Product[] }).items ?? (d as Product[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchProducts(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    const payload = { name: form.name, basePrice: Number(form.basePrice), stockQuantity: Number(form.stockQuantity), description: form.description };
    try {
      if (editId) await sellerApi.products.update(editId, payload);
      else await sellerApi.products.create(payload);
      setShowForm(false); setEditId(null);
      setForm({ name: '', basePrice: '', stockQuantity: '', description: '' });
      fetchProducts();
    } catch (err) { setError(err instanceof Error ? err.message : 'Failed to save product.'); }
    finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this product?')) return;
    await sellerApi.products.delete(id);
    fetchProducts();
  };

  const handleEdit = (p: Product) => {
    setEditId(p.id);
    setForm({ name: p.name, basePrice: String(p.basePrice), stockQuantity: String(p.stockQuantity), description: '' });
    setShowForm(true);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const tabCounts = STATUS_TABS.reduce((acc, t) => {
    acc[t] = t === 'All' ? products.length : products.filter(p => p.status === t || (t === 'Active' && p.isActive && p.status === 'Approved')).length;
    return acc;
  }, {} as Record<string, number>);

  const filtered = products.filter(p => {
    const matchTab = tab === 'All' || p.status === tab || (tab === 'Active' && p.isActive && p.status === 'Approved');
    const matchSearch = !search || p.name.toLowerCase().includes(search.toLowerCase());
    return matchTab && matchSearch;
  });

  const stats = [
    { label: 'Total Products', value: products.length, color: '#3182ce' },
    { label: 'Active', value: products.filter(p => p.isActive).length, color: '#38a169' },
    { label: 'Out of Stock', value: products.filter(p => p.stockQuantity === 0).length, color: '#e53e3e' },
    { label: 'Draft / Pending', value: products.filter(p => p.status === 'Draft' || p.status === 'Pending').length, color: '#ed8936' },
  ];

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
        <h1 style={{ margin: 0 }}>My Products</h1>
        <button onClick={() => { setShowForm(true); setEditId(null); setForm({ name: '', basePrice: '', stockQuantity: '', description: '' }); }}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          + Add New Product
        </button>
      </div>

      {/* Stats */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {stats.map(s => (
          <div key={s.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.3rem' }}>{s.label}</div>
            <div style={{ fontWeight: 700, fontSize: 20, color: s.color }}>{s.value}</div>
          </div>
        ))}
      </div>

      {/* Add/Edit form */}
      {showForm && (
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem', background: '#f9f9f9', border: '1px solid #e2e8f0', borderRadius: 8, marginBottom: '1.5rem', maxWidth: 520 }}>
          <h3 style={{ margin: '0 0 1rem' }}>{editId ? 'Edit Product' : 'New Product'}</h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
            <input placeholder="Product Name" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required
              style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
            <input type="number" placeholder="Base Price (₫)" value={form.basePrice} onChange={e => setForm(f => ({ ...f, basePrice: e.target.value }))} required
              style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
            <input type="number" placeholder="Stock Quantity" value={form.stockQuantity} onChange={e => setForm(f => ({ ...f, stockQuantity: e.target.value }))} required
              style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
            <textarea placeholder="Description" value={form.description} onChange={e => setForm(f => ({ ...f, description: e.target.value }))} rows={3}
              style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', resize: 'vertical' }} />
            {error && <div style={{ color: '#e53e3e', fontSize: 13 }}>{error}</div>}
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button type="submit" disabled={saving}
                style={{ flex: 1, padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
                {saving ? 'Saving…' : 'Save'}
              </button>
              <button type="button" onClick={() => setShowForm(false)}
                style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                Cancel
              </button>
            </div>
          </div>
        </form>
      )}

      {/* Search + tabs */}
      <div style={{ marginBottom: '1rem' }}>
        <input placeholder="Search products…" value={search} onChange={e => setSearch(e.target.value)}
          style={{ padding: '0.5rem', width: '100%', maxWidth: 340, borderRadius: 4, border: '1px solid #ccc', marginBottom: '0.75rem' }} />
        <div style={{ display: 'flex', gap: '0.25rem', flexWrap: 'wrap', borderBottom: '1px solid #e2e8f0' }}>
          {STATUS_TABS.filter(t => tabCounts[t] > 0 || t === 'All').map(t => (
            <button key={t} onClick={() => setTab(t)}
              style={{
                padding: '0.5rem 1rem', border: 'none', borderBottom: tab === t ? '2px solid #3182ce' : '2px solid transparent',
                background: 'none', cursor: 'pointer', fontWeight: tab === t ? 700 : 400,
                color: tab === t ? '#3182ce' : '#555', fontSize: 13,
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
      </div>

      {/* Table */}
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Product</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Category</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Price</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Stock</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {filtered.map(p => (
            <tr key={p.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem' }}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                  {p.imageUrl
                    ? <img src={p.imageUrl} alt={p.name} style={{ width: 40, height: 40, objectFit: 'cover', borderRadius: 4, flexShrink: 0 }} />
                    : <div style={{ width: 40, height: 40, borderRadius: 4, background: '#f0f0f0', flexShrink: 0 }} />
                  }
                  <div>
                    <div style={{ fontWeight: 500 }}>{p.name}</div>
                    {p.brandName && <div style={{ fontSize: 12, color: '#888' }}>{p.brandName}</div>}
                  </div>
                </div>
              </td>
              <td style={{ padding: '0.75rem', fontSize: 13, color: '#555' }}>{p.categoryName ?? '—'}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 600 }}>{p.basePrice?.toLocaleString()} ₫</td>
              <td style={{ padding: '0.75rem', textAlign: 'right' }}>
                <span style={{ color: p.stockQuantity === 0 ? '#e53e3e' : p.stockQuantity < 5 ? '#ed8936' : '#38a169', fontWeight: 600 }}>
                  {p.stockQuantity}
                </span>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[p.status] ?? '#555', fontWeight: 600, fontSize: 12 }}>
                  {p.status}
                </span>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <div style={{ display: 'flex', gap: '0.4rem', justifyContent: 'center' }}>
                  <button onClick={() => handleEdit(p)}
                    style={{ padding: '0.2rem 0.6rem', background: '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12 }}>
                    Edit
                  </button>
                  <button onClick={() => handleDelete(p.id)}
                    style={{ padding: '0.2rem 0.6rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030', fontSize: 12 }}>
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          ))}
          {filtered.length === 0 && (
            <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No products found.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
