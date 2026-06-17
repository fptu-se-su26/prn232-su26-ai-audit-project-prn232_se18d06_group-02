import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface Voucher {
  id: string;
  code: string;
  discountType: string;
  discountValue: number;
  minOrderValue?: number;
  usageLimit?: number;
  usageCount: number;
  expiresAt?: string;
  isActive: boolean;
}

export default function SellerVouchersPage() {
  const [vouchers, setVouchers] = useState<Voucher[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ code: '', discountType: 'Percentage', discountValue: '', minOrderValue: '', usageLimit: '', expiresAt: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchVouchers = () => {
    sellerApi.vouchers.list()
      .then(d => setVouchers((d as { items?: Voucher[] }).items ?? (d as Voucher[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchVouchers(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      await sellerApi.vouchers.create({
        code: form.code,
        discountType: form.discountType,
        discountValue: Number(form.discountValue),
        minOrderValue: form.minOrderValue ? Number(form.minOrderValue) : undefined,
        usageLimit: form.usageLimit ? Number(form.usageLimit) : undefined,
        expiresAt: form.expiresAt || undefined,
      });
      setShowForm(false);
      setForm({ code: '', discountType: 'Percentage', discountValue: '', minOrderValue: '', usageLimit: '', expiresAt: '' });
      fetchVouchers();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to create voucher.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this voucher?')) return;
    await sellerApi.vouchers.delete(id);
    fetchVouchers();
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>Vouchers</h1>
        <button onClick={() => setShowForm(true)}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          + Create Voucher
        </button>
      </div>

      {showForm && (
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem', background: '#f9f9f9', borderRadius: 8, marginBottom: '1.5rem', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', maxWidth: 600 }}>
          <h3 style={{ gridColumn: '1/-1', margin: 0 }}>New Voucher</h3>
          <input placeholder="Code (e.g. SUMMER20)" value={form.code} onChange={e => setForm(f => ({ ...f, code: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <select value={form.discountType} onChange={e => setForm(f => ({ ...f, discountType: e.target.value }))} style={{ padding: '0.5rem' }}>
            <option value="Percentage">Percentage (%)</option>
            <option value="Fixed">Fixed Amount (VND)</option>
          </select>
          <input type="number" placeholder="Discount Value" value={form.discountValue} onChange={e => setForm(f => ({ ...f, discountValue: e.target.value }))} required style={{ padding: '0.5rem' }} />
          <input type="number" placeholder="Min Order (VND, optional)" value={form.minOrderValue} onChange={e => setForm(f => ({ ...f, minOrderValue: e.target.value }))} style={{ padding: '0.5rem' }} />
          <input type="number" placeholder="Usage Limit (optional)" value={form.usageLimit} onChange={e => setForm(f => ({ ...f, usageLimit: e.target.value }))} style={{ padding: '0.5rem' }} />
          <input type="date" placeholder="Expires At" value={form.expiresAt} onChange={e => setForm(f => ({ ...f, expiresAt: e.target.value }))} style={{ padding: '0.5rem' }} />
          {error && <p style={{ gridColumn: '1/-1', color: 'red', margin: 0 }}>{error}</p>}
          <div style={{ gridColumn: '1/-1', display: 'flex', gap: '0.5rem' }}>
            <button type="submit" disabled={saving} style={{ flex: 1, padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
              {saving ? 'Creating…' : 'Create'}
            </button>
            <button type="button" onClick={() => setShowForm(false)} style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
          </div>
        </form>
      )}

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Code</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Discount</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Used</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Expires</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem' }}></th>
          </tr>
        </thead>
        <tbody>
          {vouchers.map(v => (
            <tr key={v.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', fontWeight: 600, fontFamily: 'monospace' }}>{v.code}</td>
              <td style={{ padding: '0.75rem' }}>{v.discountType === 'Percentage' ? `${v.discountValue}%` : `${v.discountValue?.toLocaleString()} VND`}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right' }}>{v.usageCount}/{v.usageLimit ?? '∞'}</td>
              <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{v.expiresAt ? new Date(v.expiresAt).toLocaleDateString() : '—'}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: v.isActive ? '#38a169' : '#e53e3e', fontWeight: 600 }}>{v.isActive ? 'Active' : 'Inactive'}</span>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <button onClick={() => handleDelete(v.id)} style={{ padding: '0.25rem 0.75rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030' }}>Delete</button>
              </td>
            </tr>
          ))}
          {vouchers.length === 0 && (
            <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No vouchers yet.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
