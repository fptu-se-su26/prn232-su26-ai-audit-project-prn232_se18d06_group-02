import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Category {
  id: string;
  name: string;
  slug: string;
  parentId?: string;
  parentName?: string;
  productCount?: number;
  revenue?: number;
  isActive?: boolean;
  isDeleted?: boolean;
}

export default function AdminCategoriesPage() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState({ name: '', slug: '', parentId: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchCategories = (q = search, s = statusFilter) => {
    setLoading(true);
    adminApi.categories.list({ search: q || undefined, isActive: s || undefined })
      .then(d => setCategories((d as { items?: Category[] }).items ?? (d as Category[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchCategories(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      const payload = { name: form.name, slug: form.slug, parentId: form.parentId || undefined };
      if (editId) {
        await adminApi.categories.update(editId, payload);
      } else {
        await adminApi.categories.create(payload);
      }
      setShowForm(false); setEditId(null); setForm({ name: '', slug: '', parentId: '' });
      fetchCategories();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save category.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this category?')) return;
    await adminApi.categories.delete(id);
    fetchCategories();
  };

  const rootCategories = categories.filter(c => !c.parentId);
  const subCategories = categories.filter(c => !!c.parentId);

  const StatusBadge = ({ c }: { c: Category }) => {
    if (c.isDeleted) return <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#fff5f5', color: '#e53e3e', fontWeight: 600, fontSize: 12 }}>Deleted</span>;
    if (c.isActive === false) return <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f7fafc', color: '#718096', fontWeight: 600, fontSize: 12 }}>Inactive</span>;
    return <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0fff4', color: '#38a169', fontWeight: 600, fontSize: 12 }}>Active</span>;
  };

  return (
    <div style={{ padding: '2rem' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>Category Management</h1>
        <button onClick={() => { setShowForm(true); setEditId(null); setForm({ name: '', slug: '', parentId: '' }); }}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          + Add Category
        </button>
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <input placeholder="Search by name or slug…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchCategories(search, statusFilter)}
          style={{ padding: '0.5rem', flex: '1 1 220px', borderRadius: 4, border: '1px solid #ccc' }} />
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 130 }}>
          <option value="">All Status</option>
          <option value="true">Active</option>
          <option value="false">Inactive</option>
        </select>
        <button onClick={() => fetchCategories(search, statusFilter)}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Filter
        </button>
      </div>

      {/* Add/Edit Form */}
      {showForm && (
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem', background: '#f9f9f9', border: '1px solid #e2e8f0', borderRadius: 8, marginBottom: '1.5rem', display: 'flex', flexDirection: 'column', gap: '0.75rem', maxWidth: 400 }}>
          <h3 style={{ margin: 0 }}>{editId ? 'Edit Category' : 'New Category'}</h3>
          <input placeholder="Name" value={form.name} onChange={e => setForm(f => ({ ...f, name: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          <input placeholder="Slug (e.g. gaming-mice)" value={form.slug} onChange={e => setForm(f => ({ ...f, slug: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          <select value={form.parentId} onChange={e => setForm(f => ({ ...f, parentId: e.target.value }))} style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }}>
            <option value="">No parent (root category)</option>
            {rootCategories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
          {error && <p style={{ color: 'red', margin: 0, fontSize: 13 }}>{error}</p>}
          <div style={{ display: 'flex', gap: '0.5rem' }}>
            <button type="submit" disabled={saving} style={{ flex: 1, padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
              {saving ? 'Saving…' : 'Save'}
            </button>
            <button type="button" onClick={() => setShowForm(false)} style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
          </div>
        </form>
      )}

      {loading ? <div style={{ textAlign: 'center', padding: '2rem' }}>Loading…</div> : (
        <>
          <h2 style={{ marginBottom: '0.5rem' }}>Root Categories <span style={{ fontSize: 14, color: '#888', fontWeight: 400 }}>({rootCategories.length})</span></h2>
          <table style={{ width: '100%', borderCollapse: 'collapse', marginBottom: '2rem' }}>
            <thead>
              <tr style={{ background: '#f0f0f0' }}>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>Name</th>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>Slug</th>
                <th style={{ padding: '0.75rem', textAlign: 'center' }}>Level</th>
                <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
                <th style={{ padding: '0.75rem', textAlign: 'right' }}>Products</th>
                <th style={{ padding: '0.75rem' }}></th>
              </tr>
            </thead>
            <tbody>
              {rootCategories.map(c => (
                <tr key={c.id} style={{ borderBottom: '1px solid #eee' }}>
                  <td style={{ padding: '0.75rem', fontWeight: 500 }}>{c.name}</td>
                  <td style={{ padding: '0.75rem', color: '#888', fontSize: 13, fontFamily: 'monospace' }}>{c.slug}</td>
                  <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                    <span style={{ padding: '0.15rem 0.5rem', borderRadius: 4, background: '#ebf8ff', color: '#2b6cb0', fontWeight: 600, fontSize: 11 }}>Root</span>
                  </td>
                  <td style={{ padding: '0.75rem', textAlign: 'center' }}><StatusBadge c={c} /></td>
                  <td style={{ padding: '0.75rem', textAlign: 'right' }}>{c.productCount ?? 0}</td>
                  <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                    <button onClick={() => { setEditId(c.id); setForm({ name: c.name, slug: c.slug, parentId: c.parentId ?? '' }); setShowForm(true); window.scrollTo({ top: 0, behavior: 'smooth' }); }} style={{ marginRight: '0.5rem', padding: '0.25rem 0.75rem', background: '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Edit</button>
                    <button onClick={() => handleDelete(c.id)} style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>Delete</button>
                  </td>
                </tr>
              ))}
              {rootCategories.length === 0 && <tr><td colSpan={6} style={{ padding: '1.5rem', textAlign: 'center', color: '#888' }}>No root categories.</td></tr>}
            </tbody>
          </table>

          {subCategories.length > 0 && (
            <>
              <h2 style={{ marginBottom: '0.5rem' }}>Sub-categories <span style={{ fontSize: 14, color: '#888', fontWeight: 400 }}>({subCategories.length})</span></h2>
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ background: '#f0f0f0' }}>
                    <th style={{ padding: '0.75rem', textAlign: 'left' }}>Name</th>
                    <th style={{ padding: '0.75rem', textAlign: 'left' }}>Slug</th>
                    <th style={{ padding: '0.75rem', textAlign: 'center' }}>Level</th>
                    <th style={{ padding: '0.75rem', textAlign: 'left' }}>Parent</th>
                    <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
                    <th style={{ padding: '0.75rem', textAlign: 'right' }}>Products</th>
                    <th style={{ padding: '0.75rem' }}></th>
                  </tr>
                </thead>
                <tbody>
                  {subCategories.map(c => (
                    <tr key={c.id} style={{ borderBottom: '1px solid #eee' }}>
                      <td style={{ padding: '0.75rem', paddingLeft: '1.5rem' }}>↳ {c.name}</td>
                      <td style={{ padding: '0.75rem', color: '#888', fontSize: 13, fontFamily: 'monospace' }}>{c.slug}</td>
                      <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                        <span style={{ padding: '0.15rem 0.5rem', borderRadius: 4, background: '#f0e8ff', color: '#6b46c1', fontWeight: 600, fontSize: 11 }}>Sub 1</span>
                      </td>
                      <td style={{ padding: '0.75rem', color: '#555', fontSize: 13 }}>{c.parentName}</td>
                      <td style={{ padding: '0.75rem', textAlign: 'center' }}><StatusBadge c={c} /></td>
                      <td style={{ padding: '0.75rem', textAlign: 'right' }}>{c.productCount ?? 0}</td>
                      <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                        <button onClick={() => { setEditId(c.id); setForm({ name: c.name, slug: c.slug, parentId: c.parentId ?? '' }); setShowForm(true); window.scrollTo({ top: 0, behavior: 'smooth' }); }} style={{ marginRight: '0.5rem', padding: '0.25rem 0.75rem', background: '#bee3f8', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Edit</button>
                        <button onClick={() => handleDelete(c.id)} style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>Delete</button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </>
          )}

          <div style={{ marginTop: '1rem', fontSize: 13, color: '#888' }}>
            Total: {categories.length} categories ({rootCategories.length} root, {subCategories.length} sub)
          </div>
        </>
      )}
    </div>
  );
}
