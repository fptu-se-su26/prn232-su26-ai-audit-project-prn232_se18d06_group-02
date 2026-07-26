import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface Voucher {
  id: string;
  code: string;
  name?: string;
  discountType: string;
  discountValue: number;
  usageCount: number;
  usageLimit?: number;
  expiresAt?: string;
  startAt?: string;
  endAt?: string;
  isActive: boolean;
  scope: 'Platform' | 'Store';
  status?: string;
  minOrderAmount?: number;
  maxDiscount?: number;
  categoryId?: string;
  categoryName?: string;
  voucherType?: string;
}

const STATUS_COLORS: Record<string, string> = {
  Active: '#38a169', Upcoming: '#3182ce', Expired: '#718096',
  Disabled: '#e53e3e', Finished: '#718096',
};
const STATUS_BG: Record<string, string> = {
  Active: '#f0fff4', Upcoming: '#ebf4ff', Expired: '#f7fafc',
  Disabled: '#fff5f5', Finished: '#f7fafc',
};
const ACCENT_BG: Record<string, string> = {
  Active: '#38a169', Upcoming: '#3182ce', Expired: '#a0aec0',
  Disabled: '#e53e3e', Finished: '#a0aec0',
};

const SORT_OPTIONS = [
  { value: 'newest', label: 'Newest First' },
  { value: 'oldest', label: 'Oldest First' },
  { value: 'name_asc', label: 'Name A-Z' },
  { value: 'name_desc', label: 'Name Z-A' },
  { value: 'discount_high', label: 'Highest Discount' },
  { value: 'discount_low', label: 'Lowest Discount' },
  { value: 'most_used', label: 'Most Used' },
  { value: 'expiring', label: 'Expiring Soon' },
];

export default function AdminVouchersPage() {
  const navigate = useNavigate();
  const [vouchers, setVouchers] = useState<Voucher[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusTab, setStatusTab] = useState('');
  const [scopeFilter, setScopeFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [sortBy, setSortBy] = useState('newest');
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ code: '', discountType: 'Percentage', discountValue: '', usageLimit: '', expiresAt: '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [toggling, setToggling] = useState<string | null>(null);

  const fetchVouchers = () => {
    setLoading(true);
    adminApi.vouchers.list({ search: search || undefined, status: statusTab || undefined, scope: scopeFilter || undefined })
      .then(d => setVouchers((d as { items?: Voucher[] }).items ?? (d as Voucher[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchVouchers(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true); setError('');
    try {
      await adminApi.vouchers.create({
        code: form.code, discountType: form.discountType,
        discountValue: Number(form.discountValue),
        usageLimit: form.usageLimit ? Number(form.usageLimit) : undefined,
        expiresAt: form.expiresAt || undefined,
      });
      setShowForm(false);
      setForm({ code: '', discountType: 'Percentage', discountValue: '', usageLimit: '', expiresAt: '' });
      fetchVouchers();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to create voucher.');
    } finally { setSaving(false); }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this voucher?')) return;
    await adminApi.vouchers.delete(id);
    fetchVouchers();
  };

  const handleToggle = async (id: string) => {
    setToggling(id);
    try { await adminApi.vouchers.toggleStatus(id); fetchVouchers(); }
    finally { setToggling(null); }
  };

  const getVoucherStatus = (v: Voucher): string => {
    if (v.status) return v.status;
    if (!v.isActive) return 'Disabled';
    const now = new Date();
    if (v.startAt && new Date(v.startAt) > now) return 'Upcoming';
    const end = v.endAt ?? v.expiresAt;
    if (end && new Date(end) < now) return 'Expired';
    return 'Active';
  };

  const filtered = vouchers.filter(v => {
    const status = getVoucherStatus(v);
    if (statusTab && status !== statusTab) return false;
    if (scopeFilter && v.scope !== scopeFilter) return false;
    if (typeFilter && v.voucherType !== typeFilter) return false;
    if (search) {
      const q = search.toLowerCase();
      return v.code.toLowerCase().includes(q) || (v.name ?? '').toLowerCase().includes(q);
    }
    return true;
  }).sort((a, b) => {
    if (sortBy === 'oldest') return new Date(a.startAt ?? a.expiresAt ?? '').getTime() - new Date(b.startAt ?? b.expiresAt ?? '').getTime();
    if (sortBy === 'name_asc') return (a.name ?? a.code).localeCompare(b.name ?? b.code);
    if (sortBy === 'name_desc') return (b.name ?? b.code).localeCompare(a.name ?? a.code);
    if (sortBy === 'discount_high') return b.discountValue - a.discountValue;
    if (sortBy === 'discount_low') return a.discountValue - b.discountValue;
    if (sortBy === 'most_used') return (b.usageCount ?? 0) - (a.usageCount ?? 0);
    if (sortBy === 'expiring') {
      const ae = a.endAt ?? a.expiresAt; const be = b.endAt ?? b.expiresAt;
      if (!ae) return 1; if (!be) return -1;
      return new Date(ae).getTime() - new Date(be).getTime();
    }
    return new Date(b.startAt ?? b.expiresAt ?? '').getTime() - new Date(a.startAt ?? a.expiresAt ?? '').getTime();
  });

  const activeCount = vouchers.filter(v => getVoucherStatus(v) === 'Active').length;
  const totalUsage = vouchers.reduce((s, v) => s + v.usageCount, 0);
  const totalLimit = vouchers.reduce((s, v) => s + (v.usageLimit ?? 0), 0);
  const redemptionRate = totalLimit > 0 ? Math.round((totalUsage / totalLimit) * 100) : 0;

  const tabs = [
    { key: '', label: 'All', count: vouchers.length },
    { key: 'Active', label: 'Active', count: vouchers.filter(v => getVoucherStatus(v) === 'Active').length },
    { key: 'Upcoming', label: 'Upcoming', count: vouchers.filter(v => getVoucherStatus(v) === 'Upcoming').length },
    { key: 'Expired', label: 'Expired', count: vouchers.filter(v => getVoucherStatus(v) === 'Expired').length },
    { key: 'Disabled', label: 'Disabled', count: vouchers.filter(v => getVoucherStatus(v) === 'Disabled').length },
  ];

  const stats = [
    { label: 'Total Vouchers', value: vouchers.length, color: '#3182ce' },
    { label: 'Active Today', value: activeCount, color: '#38a169' },
    { label: 'Redemption Rate', value: `${redemptionRate}%`, color: '#ed8936' },
    { label: 'Total Uses', value: totalUsage.toLocaleString(), color: '#6b46c1' },
  ];

  const fmtDate = (d?: string) => d ? new Date(d).toLocaleDateString('vi-VN') : '—';

  return (
    <div style={{ padding: '2rem' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>Platform Vouchers</h1>
        <button onClick={() => setShowForm(s => !s)}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          + Create Voucher
        </button>
      </div>

      {/* KPI Stats */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {stats.map(s => (
          <div key={s.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.3rem' }}>{s.label}</div>
            <div style={{ fontWeight: 700, fontSize: 20, color: s.color }}>{s.value}</div>
          </div>
        ))}
      </div>

      {/* Create Form */}
      {showForm && (
        <form onSubmit={handleSubmit} style={{ padding: '1.5rem', background: '#f9f9f9', border: '1px solid #e2e8f0', borderRadius: 8, marginBottom: '1.5rem', display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', maxWidth: 600 }}>
          <h3 style={{ gridColumn: '1/-1', margin: 0 }}>New Platform Voucher</h3>
          <input placeholder="Code" value={form.code} onChange={e => setForm(f => ({ ...f, code: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          <select value={form.discountType} onChange={e => setForm(f => ({ ...f, discountType: e.target.value }))} style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }}>
            <option value="Percentage">Percentage (%)</option>
            <option value="Fixed">Fixed Amount (VND)</option>
          </select>
          <input type="number" placeholder="Discount Value" value={form.discountValue} onChange={e => setForm(f => ({ ...f, discountValue: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          <input type="number" placeholder="Usage Limit (optional)" value={form.usageLimit} onChange={e => setForm(f => ({ ...f, usageLimit: e.target.value }))} style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          <input type="date" value={form.expiresAt} onChange={e => setForm(f => ({ ...f, expiresAt: e.target.value }))} style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', gridColumn: '1/-1' }} />
          {error && <p style={{ gridColumn: '1/-1', color: 'red', margin: 0 }}>{error}</p>}
          <div style={{ gridColumn: '1/-1', display: 'flex', gap: '0.5rem' }}>
            <button type="submit" disabled={saving} style={{ flex: 1, padding: '0.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
              {saving ? 'Creating…' : 'Create'}
            </button>
            <button type="button" onClick={() => setShowForm(false)} style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
          </div>
        </form>
      )}

      {/* Status Tabs */}
      <div style={{ display: 'flex', gap: 0, borderBottom: '2px solid #e2e8f0', marginBottom: '1.25rem' }}>
        {tabs.map(t => (
          <button key={t.key} onClick={() => setStatusTab(t.key)}
            style={{ padding: '0.6rem 1.25rem', border: 'none', borderBottom: statusTab === t.key ? '2px solid #3182ce' : '2px solid transparent', marginBottom: -2, background: 'none', cursor: 'pointer', fontWeight: statusTab === t.key ? 700 : 400, color: statusTab === t.key ? '#3182ce' : '#555', display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
            {t.label}
            <span style={{ padding: '0.1rem 0.5rem', borderRadius: 12, background: statusTab === t.key ? '#ebf4ff' : '#f0f0f0', color: statusTab === t.key ? '#3182ce' : '#888', fontSize: 11, fontWeight: 600 }}>{t.count}</span>
          </button>
        ))}
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <input placeholder="Search by code or name…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchVouchers()}
          style={{ padding: '0.5rem', flex: '1 1 200px', borderRadius: 4, border: '1px solid #ccc' }} />
        <select value={scopeFilter} onChange={e => setScopeFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 130 }}>
          <option value="">All Scopes</option>
          <option value="Platform">Platform</option>
          <option value="Store">Store</option>
        </select>
        <select value={typeFilter} onChange={e => setTypeFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 130 }}>
          <option value="">All Types</option>
          <option value="Percentage">Percentage</option>
          <option value="Fixed">Fixed Amount</option>
        </select>
        <select value={sortBy} onChange={e => setSortBy(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 160 }}>
          {SORT_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
        </select>
        <button onClick={fetchVouchers}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Search
        </button>
      </div>

      {/* Voucher Cards */}
      {loading ? <div style={{ textAlign: 'center', padding: '2rem' }}>Loading…</div> : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {filtered.map(v => {
            const status = getVoucherStatus(v);
            const accentColor = ACCENT_BG[status] ?? '#718096';
            const usedPct = v.usageLimit ? Math.min(100, Math.round((v.usageCount / v.usageLimit) * 100)) : 0;
            const endDate = v.endAt ?? v.expiresAt;

            return (
              <div key={v.id} style={{ display: 'flex', background: '#fff', borderRadius: 12, border: '1px solid #e2e8f0', overflow: 'hidden', height: 140 }}>
                {/* Left accent panel */}
                <div style={{ width: 140, background: accentColor, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', color: '#fff', flexShrink: 0 }}>
                  <div style={{ fontSize: 28, fontWeight: 800, lineHeight: 1 }}>
                    {v.discountType === 'Percentage' ? `${v.discountValue}%` : `${(v.discountValue / 1000).toFixed(0)}K`}
                  </div>
                  <div style={{ fontSize: 11, opacity: 0.85, marginTop: 2 }}>
                    {v.discountType === 'Percentage' ? 'OFF' : '₫ OFF'}
                  </div>
                  <div style={{ marginTop: 8, background: 'rgba(255,255,255,0.2)', borderRadius: 4, padding: '0.2rem 0.5rem', fontSize: 11, fontFamily: 'monospace', letterSpacing: 1 }}>
                    {v.code}
                  </div>
                </div>

                {/* Dashed separator */}
                <div style={{ width: 1, borderLeft: '2px dashed #e2e8f0', margin: '12px 0', flexShrink: 0 }} />

                {/* Right content */}
                <div style={{ flex: 1, padding: '0.875rem 1.25rem', display: 'flex', flexDirection: 'column', justifyContent: 'space-between', overflow: 'hidden' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                    <div>
                      <div style={{ fontWeight: 700, fontSize: 15, marginBottom: 2 }}>{v.name ?? v.code}</div>
                      <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', alignItems: 'center', marginTop: 4 }}>
                        <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: STATUS_BG[status] ?? '#f7fafc', color: STATUS_COLORS[status] ?? '#555', fontWeight: 600, fontSize: 11 }}>
                          {status}
                        </span>
                        <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: '#3182ce', fontWeight: 600, fontSize: 11 }}>
                          {v.scope}
                        </span>
                        {v.categoryName && (
                          <span style={{ fontSize: 11, color: '#718096' }}>📂 {v.categoryName}</span>
                        )}
                      </div>
                    </div>
                    <div style={{ display: 'flex', gap: '0.4rem', flexShrink: 0 }}>
                      <button onClick={() => navigate(`/admin/vouchers/${v.id}`)}
                        style={{ padding: '0.2rem 0.6rem', background: '#ebf8ff', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#2b6cb0', fontSize: 12 }}>
                        Edit
                      </button>
                      <button onClick={() => handleToggle(v.id)} disabled={toggling === v.id}
                        style={{ padding: '0.2rem 0.6rem', background: v.isActive ? '#fffaf0' : '#f0fff4', border: 'none', borderRadius: 4, cursor: 'pointer', color: v.isActive ? '#c05621' : '#276749', fontSize: 12 }}>
                        {toggling === v.id ? '…' : v.isActive ? 'Disable' : 'Enable'}
                      </button>
                      <button onClick={() => handleDelete(v.id)}
                        style={{ padding: '0.2rem 0.6rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030', fontSize: 12 }}>
                        Delete
                      </button>
                    </div>
                  </div>

                  <div>
                    {/* Details row */}
                    <div style={{ display: 'flex', gap: '1rem', fontSize: 12, color: '#718096', marginBottom: 6 }}>
                      {v.minOrderAmount != null && <span>Min: {v.minOrderAmount.toLocaleString()}₫</span>}
                      {v.maxDiscount != null && <span>Max: {v.maxDiscount.toLocaleString()}₫</span>}
                      <span>{fmtDate(v.startAt)} – {fmtDate(endDate)}</span>
                    </div>
                    {/* Usage progress bar */}
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                      <div style={{ flex: 1, height: 6, background: '#e2e8f0', borderRadius: 3, overflow: 'hidden' }}>
                        <div style={{ width: `${usedPct}%`, height: '100%', background: usedPct > 80 ? '#e53e3e' : accentColor, borderRadius: 3 }} />
                      </div>
                      <span style={{ fontSize: 11, color: '#718096', whiteSpace: 'nowrap' }}>
                        {v.usageCount}/{v.usageLimit ?? '∞'} used
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            );
          })}

          {filtered.length === 0 && (
            <div style={{ padding: '3rem', textAlign: 'center', color: '#888', background: '#fff', borderRadius: 8, border: '1px solid #e2e8f0' }}>
              No vouchers found.
            </div>
          )}
        </div>
      )}

      {/* Footer count */}
      {!loading && (
        <div style={{ marginTop: '1rem', fontSize: 13, color: '#888' }}>
          Showing {filtered.length} of {vouchers.length} vouchers
        </div>
      )}
    </div>
  );
}
