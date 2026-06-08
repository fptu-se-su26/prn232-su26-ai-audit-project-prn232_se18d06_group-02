import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface StoreProduct {
  id: string;
  name: string;
  categoryName: string;
  basePrice: number;
  stockQuantity: number;
  status: string;
  createdAt: string;
}

interface StoreOrder {
  id: string;
  orderCode: number;
  buyerName: string;
  grandTotal: number;
  paymentStatus: string;
  createdAt: string;
}

interface StoreDetail {
  id: string;
  storeName: string;
  slug?: string;
  ownerName: string;
  ownerEmail: string;
  ownerPhone?: string;
  identityNumber?: string;
  status: string;
  province?: string;
  businessType?: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  taxCode?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankAccountName?: string;
  commissionRate?: number;
  totalProducts: number;
  totalOrders: number;
  totalRevenue: number;
  createdAt: string;
  approvedAt?: string;
  rejectedReason?: string;
  lockReason?: string;
  recentProducts?: StoreProduct[];
  recentOrders?: StoreOrder[];
}

const STATUS_OPTIONS = ['Approved', 'Locked', 'Rejected'];

const STATUS_COLORS: Record<string, string> = {
  Approved: '#38a169', Pending: '#ed8936',
  Locked: '#e53e3e', Rejected: '#e53e3e', Draft: '#718096',
};

type TabType = 'products' | 'orders';

export default function AdminStoreDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [store, setStore] = useState<StoreDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [statusModal, setStatusModal] = useState(false);
  const [newStatus, setNewStatus] = useState('');
  const [reason, setReason] = useState('');
  const [error, setError] = useState('');
  const [tab, setTab] = useState<TabType>('products');

  useEffect(() => {
    if (!id) return;
    adminApi.stores.get(id)
      .then(d => setStore(d as StoreDetail))
      .catch(() => navigate('/admin/stores'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  const reload = () => {
    if (!id) return;
    adminApi.stores.get(id).then(d => setStore(d as StoreDetail));
  };

  const handleChangeStatus = async () => {
    if (!id || !newStatus) return;
    setBusy(true);
    try {
      await adminApi.stores.changeStatus(id, newStatus, reason || undefined);
      setStatusModal(false); setNewStatus(''); setReason(''); reload();
    } catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(false); }
  };

  const needsReason = newStatus === 'Locked' || newStatus === 'Rejected';

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!store) return null;

  return (
    <div style={{ padding: '2rem', maxWidth: 980, margin: '0 auto' }}>
      {/* Change status modal */}
      {statusModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: '#fff', padding: '2rem', borderRadius: 8, maxWidth: 420, width: '90%' }}>
            <h3 style={{ marginTop: 0 }}>Change Store Status</h3>
            <select value={newStatus} onChange={e => setNewStatus(e.target.value)}
              style={{ width: '100%', padding: '0.5rem', marginBottom: '1rem', borderRadius: 4, border: '1px solid #ccc' }}>
              <option value="">Select status…</option>
              {STATUS_OPTIONS.map(s => <option key={s} value={s}>{s}</option>)}
            </select>
            {needsReason && (
              <textarea value={reason} onChange={e => setReason(e.target.value)}
                placeholder="Reason (required for Locked/Rejected)…" rows={3}
                style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', marginBottom: '1rem', borderRadius: 4, border: '1px solid #ccc' }} />
            )}
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button onClick={handleChangeStatus} disabled={busy || !newStatus || (needsReason && !reason)}
                style={{ flex: 1, padding: '0.5rem', background: newStatus === 'Approved' ? '#38a169' : '#e53e3e', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                {busy ? '…' : 'Confirm'}
              </button>
              <button onClick={() => { setStatusModal(false); setNewStatus(''); setReason(''); }}
                style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Store Hero Card */}
      <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, marginBottom: '1.5rem', overflow: 'hidden' }}>
        {/* Banner */}
        <div style={{ height: 120, background: store.bannerUrl ? `url(${store.bannerUrl}) center/cover` : 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', position: 'relative' }}>
          {store.logoUrl && (
            <img src={store.logoUrl} alt="" style={{
              width: 72, height: 72, borderRadius: '50%', objectFit: 'cover',
              border: '3px solid #fff', position: 'absolute', bottom: -28, left: '1.5rem',
              background: '#fff',
            }} />
          )}
        </div>
        <div style={{ padding: '0.75rem 1.5rem 1.25rem', paddingLeft: store.logoUrl ? '8rem' : '1.5rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '0.75rem' }}>
            <div>
              <div style={{ fontWeight: 700, fontSize: 20 }}>{store.storeName}</div>
              {store.slug && <div style={{ fontSize: 12, color: '#888' }}>gearzone.vn/store/{store.slug}</div>}
              <div style={{ marginTop: '0.5rem', display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
                {store.businessType && <span style={{ background: '#e2e8f0', borderRadius: 12, padding: '0.15rem 0.6rem', fontSize: 12 }}>{store.businessType}</span>}
                {store.province && <span style={{ background: '#e2e8f0', borderRadius: 12, padding: '0.15rem 0.6rem', fontSize: 12 }}>{store.province}</span>}
                {store.commissionRate != null && <span style={{ background: '#fefcbf', borderRadius: 12, padding: '0.15rem 0.6rem', fontSize: 12 }}>Commission: {store.commissionRate}%</span>}
              </div>
            </div>
            <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
              <span style={{ padding: '0.25rem 0.75rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[store.status] ?? '#555', fontWeight: 700, fontSize: 13 }}>
                {store.status}
              </span>
              <button onClick={() => setStatusModal(true)}
                style={{ padding: '0.4rem 0.9rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13 }}>
                Change Status
              </button>
            </div>
          </div>
          {store.description && <div style={{ marginTop: '0.75rem', fontSize: 13, color: '#555', lineHeight: 1.6 }}>{store.description}</div>}
        </div>
      </div>

      {error && <div style={{ background: '#fed7d7', padding: '0.75rem', borderRadius: 4, marginBottom: '1rem', color: '#c53030' }}>{error}</div>}
      {(store.rejectedReason || store.lockReason) && (
        <div style={{ background: '#fff5f5', border: '1px solid #feb2b2', borderRadius: 4, padding: '0.75rem', marginBottom: '1rem', fontSize: 13 }}>
          <strong style={{ color: '#c53030' }}>{store.lockReason ? 'Lock reason' : 'Rejection reason'}:</strong> {store.lockReason ?? store.rejectedReason}
        </div>
      )}

      {/* KPI Cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {[
          { label: 'Products', value: String(store.totalProducts ?? 0), color: '#3182ce' },
          { label: 'Orders', value: String(store.totalOrders ?? 0), color: '#ed8936' },
          { label: 'Revenue', value: `${(store.totalRevenue ?? 0).toLocaleString()} ₫`, color: '#38a169' },
          { label: 'Commission', value: store.commissionRate != null ? `${store.commissionRate}%` : '—', color: '#805ad5' },
        ].map(kpi => (
          <div key={kpi.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem', textAlign: 'center' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.25rem' }}>{kpi.label}</div>
            <div style={{ fontWeight: 700, fontSize: 20, color: kpi.color }}>{kpi.value}</div>
          </div>
        ))}
      </div>

      {/* Main info grid */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1.5rem' }}>
        {/* Store info */}
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
          <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Store Information</div>
          {([
            ['Name', store.storeName],
            ...(store.taxCode ? [['Tax Code', store.taxCode]] : []),
            ...(store.province ? [['Province', store.province]] : []),
            ['Member Since', new Date(store.createdAt).toLocaleDateString()],
            ...(store.approvedAt ? [['Approved', new Date(store.approvedAt).toLocaleDateString()]] : []),
          ] as [string, string][]).map(([k, v]) => (
            <div key={k} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.35rem', fontSize: 13 }}>
              <span style={{ color: '#888' }}>{k}</span><span style={{ fontWeight: 500, textAlign: 'right', maxWidth: '60%' }}>{v}</span>
            </div>
          ))}
        </div>
        {/* Owner info */}
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
          <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Owner Information</div>
          {([
            ['Name', store.ownerName],
            ['Email', store.ownerEmail],
            ...(store.ownerPhone ? [['Phone', store.ownerPhone]] : []),
            ...(store.identityNumber ? [['Identity No.', store.identityNumber]] : []),
          ] as [string, string][]).map(([k, v]) => (
            <div key={k} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.35rem', fontSize: 13 }}>
              <span style={{ color: '#888' }}>{k}</span><span style={{ fontWeight: 500, textAlign: 'right', maxWidth: '60%' }}>{v}</span>
            </div>
          ))}
        </div>
        {/* Banking */}
        {(store.bankName || store.bankAccountNumber) && (
          <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Banking & Financial</div>
            {([
              ...(store.bankName ? [['Bank', store.bankName]] : []),
              ...(store.bankAccountNumber ? [['Account No.', store.bankAccountNumber]] : []),
              ...(store.bankAccountName ? [['Account Name', store.bankAccountName]] : []),
              ...(store.commissionRate != null ? [['Commission Rate', `${store.commissionRate}%`]] : []),
            ] as [string, string][]).map(([k, v]) => (
              <div key={k} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.35rem', fontSize: 13 }}>
                <span style={{ color: '#888' }}>{k}</span><span style={{ fontWeight: 500, textAlign: 'right', maxWidth: '60%' }}>{v}</span>
              </div>
            ))}
          </div>
        )}
        {/* Navigation links */}
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem', justifyContent: 'center' }}>
          <Link to={`/admin/stores`} style={{ color: '#3182ce', fontSize: 13, textDecoration: 'none' }}>← Back to Stores</Link>
          <a href={`/store/${store.slug}`} target="_blank" rel="noreferrer" style={{ color: '#3182ce', fontSize: 13, textDecoration: 'none' }}>View Store Profile →</a>
        </div>
      </div>

      {/* Tabs: Products / Orders */}
      {((store.recentProducts ?? []).length > 0 || (store.recentOrders ?? []).length > 0) && (
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, overflow: 'hidden' }}>
          <div style={{ display: 'flex', borderBottom: '1px solid #e2e8f0' }}>
            {(['products', 'orders'] as TabType[]).map(t => (
              <button key={t} onClick={() => setTab(t)}
                style={{ padding: '0.75rem 1.5rem', border: 'none', borderBottom: tab === t ? '2px solid #3182ce' : '2px solid transparent', background: 'none', cursor: 'pointer', fontWeight: tab === t ? 600 : 400, color: tab === t ? '#3182ce' : '#555', textTransform: 'capitalize' }}>
                {t}
              </button>
            ))}
          </div>
          <div style={{ padding: '1rem' }}>
            {tab === 'products' && (
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <thead><tr style={{ color: '#888' }}>
                  <th style={{ textAlign: 'left', padding: '0.5rem 0', fontWeight: 500 }}>Name</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem', fontWeight: 500 }}>Category</th>
                  <th style={{ textAlign: 'right', padding: '0.5rem', fontWeight: 500 }}>Price</th>
                  <th style={{ textAlign: 'right', padding: '0.5rem', fontWeight: 500 }}>Stock</th>
                  <th style={{ textAlign: 'center', padding: '0.5rem', fontWeight: 500 }}>Status</th>
                </tr></thead>
                <tbody>
                  {(store.recentProducts ?? []).map(p => (
                    <tr key={p.id} style={{ borderTop: '1px solid #f5f5f5' }}>
                      <td style={{ padding: '0.5rem 0' }}>{p.name}</td>
                      <td style={{ padding: '0.5rem', color: '#888' }}>{p.categoryName}</td>
                      <td style={{ padding: '0.5rem', textAlign: 'right' }}>{p.basePrice?.toLocaleString()} ₫</td>
                      <td style={{ padding: '0.5rem', textAlign: 'right' }}>{p.stockQuantity}</td>
                      <td style={{ padding: '0.5rem', textAlign: 'center', color: STATUS_COLORS[p.status] ?? '#555', fontWeight: 600 }}>{p.status}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
            {tab === 'orders' && (
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <thead><tr style={{ color: '#888' }}>
                  <th style={{ textAlign: 'left', padding: '0.5rem 0', fontWeight: 500 }}>Order Code</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem', fontWeight: 500 }}>Customer</th>
                  <th style={{ textAlign: 'right', padding: '0.5rem', fontWeight: 500 }}>Total</th>
                  <th style={{ textAlign: 'center', padding: '0.5rem', fontWeight: 500 }}>Payment</th>
                  <th style={{ textAlign: 'left', padding: '0.5rem', fontWeight: 500 }}>Date</th>
                </tr></thead>
                <tbody>
                  {(store.recentOrders ?? []).map(o => (
                    <tr key={o.id} style={{ borderTop: '1px solid #f5f5f5' }}>
                      <td style={{ padding: '0.5rem 0', fontFamily: 'monospace' }}>#{o.orderCode}</td>
                      <td style={{ padding: '0.5rem' }}>{o.buyerName}</td>
                      <td style={{ padding: '0.5rem', textAlign: 'right', fontWeight: 700 }}>{o.grandTotal?.toLocaleString()} ₫</td>
                      <td style={{ padding: '0.5rem', textAlign: 'center', fontSize: 12 }}>{o.paymentStatus}</td>
                      <td style={{ padding: '0.5rem', color: '#888' }}>{new Date(o.createdAt).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
