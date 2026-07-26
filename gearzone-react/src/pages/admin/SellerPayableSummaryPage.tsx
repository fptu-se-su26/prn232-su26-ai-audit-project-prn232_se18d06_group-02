import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface SellerPayable {
  storeId: string;
  storeName: string;
  storeLogoUrl?: string;
  ownerName: string;
  periodStart: string;
  periodEnd: string;
  totalOrders: number;
  grossRevenue: number;
  commissionAmount: number;
  netPayable: number;
  currency: string;
}

interface SummaryData {
  summary: SellerPayable[];
  periodStart: string;
  periodEnd: string;
  totalEligibleSellers?: number;
  totalOrders?: number;
  totalCommission?: number;
  totalNetPayable?: number;
  walletBalance?: number;
}

type RangeType = 'this-week' | 'last-week' | 'custom';

export default function AdminSellerPayableSummaryPage() {
  const [data, setData] = useState<SummaryData | null>(null);
  const [loading, setLoading] = useState(true);
  const [rangeType, setRangeType] = useState<RangeType>('this-week');
  const [customStart, setCustomStart] = useState('');
  const [customEnd, setCustomEnd] = useState('');
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [processing, setProcessing] = useState<string | null>(null);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const fetchSummary = () => {
    setLoading(true); setError('');
    const params: Record<string, unknown> = { rangeType };
    if (rangeType === 'custom') { params.customStart = customStart; params.customEnd = customEnd; }
    adminApi.payouts.sellerSummary(params)
      .then(d => setData(d as SummaryData))
      .catch(e => setError(e instanceof Error ? e.message : 'Failed to load'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchSummary(); }, []);

  const toggleSelect = (id: string) =>
    setSelected(prev => { const s = new Set(prev); s.has(id) ? s.delete(id) : s.add(id); return s; });

  const toggleAll = () =>
    setSelected(prev => prev.size === (data?.summary.length ?? 0)
      ? new Set() : new Set(data?.summary.map(s => s.storeId) ?? []));

  const handleProcessBulk = async () => {
    if (!selected.size) return;
    setProcessing('bulk');
    try {
      const params: Record<string, unknown> = { rangeType };
      if (rangeType === 'custom') { params.customStart = customStart; params.customEnd = customEnd; }
      const result = await adminApi.payouts.processBulk({ storeIds: Array.from(selected), ...params as { rangeType?: string; customStart?: string; customEnd?: string } });
      setSuccess(`Batch generated: ${(result as { batchCode: string }).batchCode}`);
      setSelected(new Set());
    } catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setProcessing(null); }
  };

  const handleProcessSingle = async (storeId: string) => {
    setProcessing(storeId);
    try {
      const params: Record<string, unknown> = { rangeType };
      if (rangeType === 'custom') { params.customStart = customStart; params.customEnd = customEnd; }
      const result = await adminApi.payouts.processSingle(storeId, params as { rangeType?: string; customStart?: string; customEnd?: string });
      setSuccess(`Batch generated: ${(result as { batchCode: string }).batchCode}`);
    } catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setProcessing(null); }
  };

  const summary = data?.summary ?? [];
  const selectedItems = summary.filter(s => selected.has(s.storeId));
  const totalPayable = selectedItems.reduce((a, s) => a + s.netPayable, 0);

  const grandTotals = {
    orders: summary.reduce((a, s) => a + s.totalOrders, 0),
    gross: summary.reduce((a, s) => a + s.grossRevenue, 0),
    commission: summary.reduce((a, s) => a + s.commissionAmount, 0),
    net: summary.reduce((a, s) => a + s.netPayable, 0),
  };

  const kpiCards = [
    { label: 'Eligible Sellers', value: String(data?.totalEligibleSellers ?? summary.length), color: '#3182ce' },
    { label: 'Total Orders', value: String(data?.totalOrders ?? grandTotals.orders), color: '#ed8936' },
    { label: 'Total Commission', value: `${(data?.totalCommission ?? grandTotals.commission).toLocaleString()} ₫`, color: '#e53e3e' },
    { label: 'Net Payable', value: `${(data?.totalNetPayable ?? grandTotals.net).toLocaleString()} ₫`, color: '#38a169' },
    { label: 'Wallet Balance', value: data?.walletBalance != null ? `${data.walletBalance.toLocaleString()} ₫` : '—', color: '#805ad5' },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', flexWrap: 'wrap', gap: '1rem' }}>
        <h1 style={{ margin: 0 }}>Seller Payable Summary</h1>
        <a href="/admin/payouts/batches" style={{ color: '#3182ce', fontSize: 13 }}>View Batches →</a>
      </div>

      {/* KPI Cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {kpiCards.map(k => (
          <div key={k.label} style={{ background: k.label === 'Wallet Balance' ? '#1a202c' : '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: k.label === 'Wallet Balance' ? '#a0aec0' : '#888', marginBottom: '0.35rem' }}>{k.label}</div>
            <div style={{ fontWeight: 700, fontSize: 17, color: k.label === 'Wallet Balance' ? '#fff' : k.color }}>{k.value}</div>
          </div>
        ))}
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.5rem', alignItems: 'flex-end', flexWrap: 'wrap', background: '#f9f9f9', padding: '1rem', borderRadius: 8 }}>
        <div>
          <label style={{ fontSize: 12, color: '#555', display: 'block', marginBottom: '0.25rem' }}>Period</label>
          <select value={rangeType} onChange={e => setRangeType(e.target.value as RangeType)}
            style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 140 }}>
            <option value="this-week">This Week</option>
            <option value="last-week">Last Week</option>
            <option value="custom">Custom Range</option>
          </select>
        </div>
        {rangeType === 'custom' && <>
          <div>
            <label style={{ fontSize: 12, color: '#555', display: 'block', marginBottom: '0.25rem' }}>From</label>
            <input type="date" value={customStart} onChange={e => setCustomStart(e.target.value)}
              style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          </div>
          <div>
            <label style={{ fontSize: 12, color: '#555', display: 'block', marginBottom: '0.25rem' }}>To</label>
            <input type="date" value={customEnd} onChange={e => setCustomEnd(e.target.value)}
              style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
          </div>
        </>}
        <button onClick={fetchSummary}
          style={{ padding: '0.5rem 1rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Refresh
        </button>
        {data?.periodStart && (
          <div style={{ fontSize: 12, color: '#888', paddingBottom: '0.25rem' }}>
            Period: {new Date(data.periodStart).toLocaleDateString()} – {new Date(data.periodEnd).toLocaleDateString()}
          </div>
        )}
      </div>

      {error && <div style={{ background: '#fed7d7', padding: '0.75rem', borderRadius: 4, marginBottom: '1rem', color: '#c53030' }}>{error}</div>}
      {success && <div style={{ background: '#c6f6d5', padding: '0.75rem', borderRadius: 4, marginBottom: '1rem', color: '#276749' }}>{success}</div>}

      {/* Bulk action bar */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem', flexWrap: 'wrap', gap: '0.5rem' }}>
        <div style={{ fontWeight: 600, fontSize: 15 }}>
          Seller Payable List <span style={{ color: '#888', fontWeight: 400, fontSize: 13 }}>({summary.length} sellers)</span>
          {selected.size > 0 && <span style={{ marginLeft: '0.75rem', fontSize: 13, color: '#3182ce' }}>{selected.size} selected · {totalPayable.toLocaleString()} ₫</span>}
        </div>
        <button onClick={handleProcessBulk} disabled={!selected.size || processing === 'bulk'}
          style={{ padding: '0.4rem 1rem', background: selected.size ? '#38a169' : '#e2e8f0', color: selected.size ? '#fff' : '#aaa', border: 'none', borderRadius: 4, cursor: selected.size ? 'pointer' : 'not-allowed', fontWeight: 600 }}>
          {processing === 'bulk' ? 'Processing…' : `Process Selected (${selected.size})`}
        </button>
      </div>

      {loading ? <div style={{ padding: '2rem', textAlign: 'center' }}>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'center', width: 40 }}>
                <input type="checkbox"
                  checked={selected.size === summary.length && summary.length > 0}
                  ref={el => { if (el) el.indeterminate = selected.size > 0 && selected.size < summary.length; }}
                  onChange={toggleAll} />
              </th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Seller</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Orders</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Gross</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Commission</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Net Payable</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {summary.map(s => (
              <tr key={s.storeId} style={{ borderBottom: '1px solid #eee', background: selected.has(s.storeId) ? '#f0f9ff' : undefined }}>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <input type="checkbox" checked={selected.has(s.storeId)} onChange={() => toggleSelect(s.storeId)} />
                </td>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    {s.storeLogoUrl
                      ? <img src={s.storeLogoUrl} alt="" style={{ width: 28, height: 28, borderRadius: '50%', objectFit: 'cover' }} />
                      : <div style={{ width: 28, height: 28, borderRadius: '50%', background: '#e2e8f0', flexShrink: 0 }} />
                    }
                    <div>
                      <div style={{ fontWeight: 600 }}>{s.storeName}</div>
                      <div style={{ fontSize: 12, color: '#888' }}>{s.ownerName}</div>
                    </div>
                  </div>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right' }}>
                  <span style={{ background: '#e2e8f0', borderRadius: 10, padding: '0.1rem 0.5rem', fontSize: 12 }}>{s.totalOrders}</span>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right' }}>{s.grossRevenue?.toLocaleString()} ₫</td>
                <td style={{ padding: '0.75rem', textAlign: 'right', color: '#e53e3e' }}>-{s.commissionAmount?.toLocaleString()} ₫</td>
                <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 700, color: '#38a169' }}>{s.netPayable?.toLocaleString()} ₫</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <button onClick={() => handleProcessSingle(s.storeId)} disabled={!!processing}
                    style={{ padding: '0.25rem 0.75rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749', fontSize: 12, fontWeight: 600 }}>
                    {processing === s.storeId ? '…' : 'Pay Out'}
                  </button>
                </td>
              </tr>
            ))}

            {/* Grand total row */}
            {summary.length > 0 && (
              <tr style={{ background: '#f9f9f9', borderTop: '2px solid #e2e8f0', fontWeight: 700 }}>
                <td colSpan={2} style={{ padding: '0.75rem', fontSize: 13 }}>Grand Total</td>
                <td style={{ padding: '0.75rem', textAlign: 'right', fontSize: 13 }}>{grandTotals.orders}</td>
                <td style={{ padding: '0.75rem', textAlign: 'right' }}>{grandTotals.gross.toLocaleString()} ₫</td>
                <td style={{ padding: '0.75rem', textAlign: 'right', color: '#e53e3e' }}>-{grandTotals.commission.toLocaleString()} ₫</td>
                <td style={{ padding: '0.75rem', textAlign: 'right', color: '#38a169' }}>{grandTotals.net.toLocaleString()} ₫</td>
                <td />
              </tr>
            )}

            {summary.length === 0 && (
              <tr><td colSpan={7} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No payable sellers for this period.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
