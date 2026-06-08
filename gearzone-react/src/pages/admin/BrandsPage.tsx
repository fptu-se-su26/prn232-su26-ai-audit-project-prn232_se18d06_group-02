import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Brand {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string;
  productCount?: number;
  isApproved?: boolean;
}

export default function AdminBrandsPage() {
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', slug: '', logoUrl: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchBrands = (q = search, s = statusFilter) => {
    setLoading(true);
    adminApi.brands.list({ search: q || undefined, isApproved: s || undefined })
      .then(d => setBrands((d as { items?: Brand[] }).items ?? (d as Brand[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchBrands(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      if (editId) {
        await adminApi.brands.update(editId, form);
      } else {
        await adminApi.brands.create(form);
      }
      setShowForm(false); setEditId(null); setForm({ name: '', slug: '', logoUrl: '' });
      fetchBrands();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save brand.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this brand?')) return;
    await adminApi.brands.delete(id);
    fetchBrands();
  };

  const handleApprove = async (id: string) => {
    setProcessing(id);
    try { await adminApi.brands.approve(id); fetchBrands(); }
    finally { setProcessing(null); }
  };

  const stats = [
    { label: 'Total Brands', value: brands.length, color: '#3182ce' },
    { label: 'Approved', value: brands.filter(b => b.isApproved).length, color: '#38a169' },
    { label: 'Pending', value: brands.filter(b => !b.isApproved).length, color: '#ed8936' },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
        <h1 style={{ margin: 0 }}>Brand Management</h1>
        <button onClick={() => { setShowForm(true); setEditId(null); setForm({ name: '', slug: '', logoUrl: '' }); }}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          + Add Brand
        </button>
      </div>

      {/* Stats */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {stats.map(s => (
          <div key={s.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.3rem' }}>{s.label}</div>
            <div style={{ fontWeight: 700, fontSize: 20, color: s.color }}>{s.value}</div>
          </div>
        ))}
      </div>

      {/* Add/Edit Form */}
      {showForm && (
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem', background: '#f9f9f9', border: '1px solid #e2e8f0', borderRadius: 8, marginBottom: '1.5rem', display: 'flex', flexDirection: 'column', gap: '0.75rem', maxWidth: 400 }}>
          <h3 style={{ margin: 0 }}>{editId ? 'Edit Brand' : 'New Brand'}</h3>
          <input placeholder="Name" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          <input placeholder="Slug (e.g. asus)" value={form.slug} onChange={e => setForm(f => ({ ...f, slug: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          <input placeholder="Logo URL (optional)" value={form.logoUrl} onChange={e => setForm(f => ({ ...f, logoUrl: e.target.value }))} style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          {error && <p style={{ color: 'red', margin: 0, fontSize: 13 }}>{error}</p>}
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button type="submit" disabled={saving} style={{ flex: 1, padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
              {saving ? 'Saving…' : 'Save'}
            </button>
            <button type="button" onClick={() => setShowForm(false)} style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
          </div>
        </form>
      )}

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <input placeholder="Search brands…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchBrands(search, statusFilter)}
          style={{ padding: '0.5rem', flex: '1 1 220px', borderRadius: 4, border: '1px solid #ccc' }} />
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 150 }}>
          <option value="">All Statuses</option>
          <option value="true">Approved</option>
          <option value="false">Pending</option>
        </select>
        <button onClick={() => fetchBrands(search, statusFilter)}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Filter
        </button>
      </div>

      {loading ? <div style={{ textAlign: 'center', padding: '2rem' }}>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Brand</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Slug</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Products</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {brands.map(b => (
              <tr key={b.id} style={{ borderBottom: '1px solid #eee' }}>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                    {b.logoUrl
                      ? <img src={b.logoUrl} alt={b.name} style={{ width: 36, height: 36, objectFit: 'contain', borderRadius: 4, border: '1px solid #e2e8f0', padding: 2 }} />
                      : <div style={{ width: 36, height: 36, borderRadius: 4, background: '#f0f0f0', display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#aaa', fontSize: 10 }}>N/A</div>
                    }
                    <span style={{ fontWeight: 600 }}>{b.name}</span>
                  </div>
                </td>
                <td style={{ padding: '0.75rem', color: '#888', fontSize: 13, fontFamily: 'monospace' }}>{b.slug}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  {b.isApproved
                    ? <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0fff4', color: '#38a169', fontWeight: 600, fontSize: 12 }}>Approved</span>
                    : <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#fffaf0', color: '#ed8936', fontWeight: 600, fontSize: 12 }}>Pending</span>
                  }
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right' }}>{b.productCount ?? 0}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <div style={{ display: 'flex', gap: '0.4rem', justifyContent: 'center' }}>
                    {!b.isApproved && (
                      <button onClick={() => handleApprove(b.id)} disabled={processing === b.id}
                        style={{ padding: '0.2rem 0.6rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749', fontSize: 12 }}>
                        {processing === b.id ? '…' : 'Approve'}
                      </button>
                    )}
                    <button onClick={() => { setEditId(b.id); setForm({ name: b.name, slug: b.slug, logoUrl: b.logoUrl ?? '' }); setShowForm(true); window.scrollTo({ top: 0, behavior: 'smooth' }); }}
                      style={{ padding: '0.2rem 0.6rem', background: '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12 }}>Edit</button>
                    <button onClick={() => handleDelete(b.id)}
                      style={{ padding: '0.2rem 0.6rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030', fontSize: 12 }}>Delete</button>
                  </div>
                </td>
              </tr>
            ))}
            {brands.length === 0 && (
              <tr><td colSpan={5} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No brands found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
