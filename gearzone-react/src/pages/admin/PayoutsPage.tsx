import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface PayoutTransaction {
  id: string;
  transactionCode?: string;
  batchCode?: string;
  storeName: string;
  storeEmail?: string;
  grossAmount?: number;
  commissionAmount?: number;
  netAmount?: number;
  amount?: number;
  bankName?: string;
  bankAccountNumber?: string;
  bankAccount?: string;
  status: string;
  createdAt?: string;
  requestedAt?: string;
}

const STATUS_COLORS: Record<string, string> = {
  Queued: '#718096', Processing: '#3182ce', Success: '#38a169',
  Completed: '#38a169', Failed: '#e53e3e', ManualRequired: '#ed8936',
  Excluded: '#718096', Pending: '#ed8936',
};

export default function AdminPayoutsPage() {
  const navigate = useNavigate();
  const [transactions, setTransactions] = useState<PayoutTransaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [dateFilter, setDateFilter] = useState('');
  const [processing, setProcessing] = useState<string | null>(null);

  const fetchPayouts = () => {
    setLoading(true);
    adminApi.payouts.list({ search: search || undefined, status: statusFilter || undefined, dateRangeType: dateFilter || undefined })
      .then(d => setTransactions((d as { items?: PayoutTransaction[] }).items ?? (d as PayoutTransaction[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchPayouts(); }, []);

  const handleProcess = async (id: string) => {
    setProcessing(id);
    try { await adminApi.payouts.process(id); fetchPayouts(); }
    finally { setProcessing(null); }
  };

  const handleReject = async (id: string) => {
    if (!confirm('Reject this payout?')) return;
    setProcessing(id);
    try { await adminApi.payouts.reject(id, 'Rejected by admin'); fetchPayouts(); }
    finally { setProcessing(null); }
  };

  const totalNet = transactions.reduce((sum, t) => sum + (t.netAmount ?? t.amount ?? 0), 0);
  const stats = [
    { label: 'Total Net Revenue', value: totalNet.toLocaleString() + ' ₫', color: '#3182ce', big: false },
    { label: 'Pending / Processing', value: transactions.filter(t => ['Pending', 'Queued', 'Processing'].includes(t.status)).length, color: '#ed8936', big: true },
    { label: 'Completed', value: transactions.filter(t => ['Success', 'Completed'].includes(t.status)).length, color: '#38a169', big: true },
    { label: 'Failed / Manual', value: transactions.filter(t => ['Failed', 'ManualRequired'].includes(t.status)).length, color: '#e53e3e', big: true },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      <h1 style={{ marginBottom: '1.25rem' }}>Payout Transactions</h1>

      {/* Stats */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {stats.map(s => (
          <div key={s.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.3rem' }}>{s.label}</div>
            <div style={{ fontWeight: 700, fontSize: s.big ? 20 : 16, color: s.color }}>{s.value}</div>
          </div>
        ))}
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <input placeholder="Search store or code…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchPayouts()}
          style={{ padding: '0.5rem', flex: '1 1 200px', borderRadius: 4, border: '1px solid #ccc' }} />
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 150 }}>
          <option value="">All Statuses</option>
          {['Queued', 'Processing', 'Success', 'Failed', 'ManualRequired', 'Excluded', 'Pending'].map(s => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
        <select value={dateFilter} onChange={e => setDateFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 130 }}>
          <option value="">All Time</option>
          <option value="Today">Today</option>
          <option value="Week">This Week</option>
          <option value="Month">This Month</option>
        </select>
        <button onClick={fetchPayouts}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Search
        </button>
      </div>

      {loading ? <div style={{ textAlign: 'center', padding: '2rem' }}>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Payout Code</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Gross</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Commission</th>
              <th style={{ padding: '0.75rem', textAlign: 'right' }}>Net Disbursed</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Bank Ref</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Date</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {transactions.map(t => (
              <tr key={t.id} style={{ borderBottom: '1px solid #eee', cursor: 'pointer' }}
                onClick={() => navigate(`/admin/payouts/transactions/${t.id}`)}>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ fontWeight: 600, fontFamily: 'monospace', fontSize: 13 }}>
                    {t.transactionCode ?? t.id.slice(0, 8).toUpperCase()}
                  </div>
                  {t.batchCode && <div style={{ fontSize: 11, color: '#aaa' }}>Batch: {t.batchCode}</div>}
                </td>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <div style={{ width: 32, height: 32, borderRadius: 4, background: '#ebf4ff', color: '#3182ce', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700, fontSize: 11, flexShrink: 0 }}>
                      {t.storeName.slice(0, 2).toUpperCase()}
                    </div>
                    <div>
                      <div style={{ fontWeight: 500, fontSize: 13 }}>{t.storeName}</div>
                      {t.storeEmail && <div style={{ fontSize: 11, color: '#888' }}>{t.storeEmail}</div>}
                    </div>
                  </div>
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right', color: '#555', fontSize: 13 }}>
                  {(t.grossAmount ?? t.amount ?? 0).toLocaleString()} ₫
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right', color: '#888', fontSize: 13 }}>
                  {(t.commissionAmount ?? 0).toLocaleString()} ₫
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 700, color: '#38a169', fontSize: 13 }}>
                  {(t.netAmount ?? t.amount ?? 0).toLocaleString()} ₫
                </td>
                <td style={{ padding: '0.75rem', fontSize: 13 }}>
                  {t.bankName && <div style={{ color: '#555' }}>{t.bankName}</div>}
                  {(t.bankAccountNumber || t.bankAccount) && (
                    <div style={{ color: '#888', fontFamily: 'monospace', fontSize: 11 }}>{t.bankAccountNumber ?? t.bankAccount}</div>
                  )}
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[t.status] ?? '#555', fontWeight: 600, fontSize: 12 }}>
                    {t.status}
                  </span>
                </td>
                <td style={{ padding: '0.75rem', fontSize: 13, color: '#555' }}>
                  {new Date(t.createdAt ?? t.requestedAt ?? '').toLocaleDateString()}
                </td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }} onClick={e => e.stopPropagation()}>
                  <div style={{ display: 'flex', gap: '0.4rem', justifyContent: 'center' }}>
                    <button onClick={() => navigate(`/admin/payouts/transactions/${t.id}`)}
                      style={{ padding: '0.2rem 0.6rem', background: '#ebf8ff', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#2b6cb0', fontSize: 12 }}>
                      View
                    </button>
                    {(t.status === 'Pending' || t.status === 'Queued') && (
                      <>
                        <button onClick={() => handleProcess(t.id)} disabled={processing === t.id}
                          style={{ padding: '0.2rem 0.6rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749', fontSize: 12 }}>
                          {processing === t.id ? '…' : 'Process'}
                        </button>
                        <button onClick={() => handleReject(t.id)} disabled={processing === t.id}
                          style={{ padding: '0.2rem 0.6rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030', fontSize: 12 }}>
                          Reject
                        </button>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {transactions.length === 0 && (
              <tr><td colSpan={9} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No payout transactions found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
