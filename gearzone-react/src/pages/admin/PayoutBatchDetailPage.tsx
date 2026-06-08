import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface BatchTransaction {
  id: string;
  transactionCode?: string;
  storeName: string;
  storeAvatarUrl?: string;
  grossRevenue?: number;
  commissionAmount?: number;
  amount: number;
  bankReference?: string;
  status: string;
  errorMessage?: string;
}

interface BatchDetail {
  id: string;
  batchCode: string;
  status: string;
  totalTransactions: number;
  totalAmount: number;
  grossRevenue?: number;
  commissionTotal?: number;
  successCount?: number;
  failedCount?: number;
  periodStart?: string;
  periodEnd?: string;
  holdReason?: string;
  createdAt: string;
  approvedAt?: string;
  processedAt?: string;
  transactions: BatchTransaction[];
}

const STATUS_COLORS: Record<string, string> = {
  Completed: '#38a169', Approved: '#38a169',
  Pending: '#ed8936', Hold: '#ed8936',
  Processing: '#3182ce', Failed: '#e53e3e',
};

export default function AdminPayoutBatchDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [batch, setBatch] = useState<BatchDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string | null>(null);
  const [holdModal, setHoldModal] = useState(false);
  const [excludeModal, setExcludeModal] = useState<string | null>(null);
  const [reason, setReason] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  useEffect(() => {
    if (!id) return;
    adminApi.payouts.getBatch(id)
      .then(d => setBatch(d as BatchDetail))
      .catch(() => navigate('/admin/payouts/batches'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  const reload = () => {
    if (!id) return;
    adminApi.payouts.getBatch(id).then(d => setBatch(d as BatchDetail));
  };

  const handleApprove = async () => {
    if (!id) return;
    setBusy('approve');
    try { await adminApi.payouts.approveBatch(id); setSuccess('Batch approved.'); reload(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(null); }
  };

  const handleHold = async () => {
    if (!id || !reason) return;
    setBusy('hold');
    try { await adminApi.payouts.holdBatch(id, reason); setHoldModal(false); setReason(''); reload(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(null); }
  };

  const handleProcess = async () => {
    if (!id || !batch) return;
    setBusy('process');
    try { await adminApi.payouts.processBatch(id, batch.batchCode); setSuccess('Batch sent for processing.'); reload(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(null); }
  };

  const handleRetry = async (txId: string) => {
    if (!id) return;
    setBusy(txId);
    try { await adminApi.payouts.retryTransaction(id, txId); reload(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(null); }
  };

  const handleExclude = async () => {
    if (!id || !excludeModal || !reason) return;
    setBusy(excludeModal);
    try { await adminApi.payouts.excludeTransaction(id, excludeModal, reason); setExcludeModal(null); setReason(''); reload(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(null); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!batch) return null;

  const canApprove = batch.status === 'Pending';
  const canProcess = batch.status === 'Approved';
  const canHold = batch.status === 'Pending' || batch.status === 'Approved';

  return (
    <div style={{ padding: '2rem', maxWidth: 980, margin: '0 auto' }}>
      {/* Hold Modal */}
      {holdModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: '#fff', padding: '2rem', borderRadius: 8, maxWidth: 420, width: '90%' }}>
            <h3 style={{ marginTop: 0 }}>Hold Batch</h3>
            <textarea value={reason} onChange={e => setReason(e.target.value)} placeholder="Reason for hold…" rows={3}
              style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', marginBottom: '1rem', borderRadius: 4, border: '1px solid #ccc' }} />
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button onClick={handleHold} disabled={!reason || busy === 'hold'}
                style={{ flex: 1, padding: '0.5rem', background: '#ed8936', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                {busy === 'hold' ? '…' : 'Confirm Hold'}
              </button>
              <button onClick={() => { setHoldModal(false); setReason(''); }}
                style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
            </div>
          </div>
        </div>
      )}
      {/* Exclude Modal */}
      {excludeModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: '#fff', padding: '2rem', borderRadius: 8, maxWidth: 420, width: '90%' }}>
            <h3 style={{ marginTop: 0 }}>Exclude Transaction</h3>
            <textarea value={reason} onChange={e => setReason(e.target.value)} placeholder="Reason for exclusion…" rows={3}
              style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', marginBottom: '1rem', borderRadius: 4, border: '1px solid #ccc' }} />
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button onClick={handleExclude} disabled={!reason || !!busy}
                style={{ flex: 1, padding: '0.5rem', background: '#e53e3e', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                {busy === excludeModal ? '…' : 'Exclude'}
              </button>
              <button onClick={() => { setExcludeModal(null); setReason(''); }}
                style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>Cancel</button>
            </div>
          </div>
        </div>
      )}

      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <Link to="/admin/payouts/batches" style={{ color: '#3182ce', textDecoration: 'none' }}>← Batches</Link>
        <h1 style={{ margin: 0, flex: 1, fontFamily: 'monospace' }}>{batch.batchCode}</h1>
        <span style={{ padding: '0.25rem 0.75rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[batch.status] ?? '#555', fontWeight: 700, fontSize: 13 }}>
          {batch.status}
        </span>
      </div>

      {error && <div style={{ background: '#fed7d7', padding: '0.75rem', borderRadius: 4, marginBottom: '1rem', color: '#c53030' }}>{error}</div>}
      {success && <div style={{ background: '#c6f6d5', padding: '0.75rem', borderRadius: 4, marginBottom: '1rem', color: '#276749' }}>{success}</div>}

      {batch.holdReason && (
        <div style={{ background: '#fffaf0', border: '1px solid #fbd38d', borderRadius: 4, padding: '0.75rem', marginBottom: '1rem', fontSize: 13 }}>
          <strong style={{ color: '#744210' }}>Hold Reason:</strong> {batch.holdReason}
        </div>
      )}

      {/* Financial summary widgets */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {[
          { label: 'Status', value: batch.status, color: STATUS_COLORS[batch.status] ?? '#555' },
          { label: 'Gross Revenue', value: `${(batch.grossRevenue ?? batch.totalAmount)?.toLocaleString()} ₫`, color: '#3182ce' },
          { label: 'Commission', value: batch.commissionTotal != null ? `-${batch.commissionTotal?.toLocaleString()} ₫` : '—', color: '#e53e3e' },
          { label: 'Net Disbursed', value: `${batch.totalAmount?.toLocaleString()} ₫`, color: '#38a169' },
        ].map(w => (
          <div key={w.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.35rem' }}>{w.label}</div>
            <div style={{ fontWeight: 700, fontSize: 17, color: w.color }}>{w.value}</div>
          </div>
        ))}
      </div>

      {/* Batch info */}
      <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem', marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
          <div style={{ fontWeight: 600, color: '#555' }}>Batch Information</div>
          {batch.periodStart && <div style={{ fontSize: 13, color: '#888' }}>{new Date(batch.periodStart).toLocaleDateString()} – {new Date(batch.periodEnd!).toLocaleDateString()}</div>}
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '0.5rem', fontSize: 13 }}>
          <div><span style={{ color: '#888' }}>Transactions: </span><strong>{batch.totalTransactions}</strong></div>
          {batch.successCount != null && <div><span style={{ color: '#888' }}>Success: </span><span style={{ color: '#38a169', fontWeight: 600 }}>{batch.successCount}</span></div>}
          {batch.failedCount != null && <div><span style={{ color: '#888' }}>Failed: </span><span style={{ color: '#e53e3e', fontWeight: 600 }}>{batch.failedCount}</span></div>}
          <div><span style={{ color: '#888' }}>Created: </span>{new Date(batch.createdAt).toLocaleDateString()}</div>
          {batch.approvedAt && <div><span style={{ color: '#888' }}>Approved: </span>{new Date(batch.approvedAt).toLocaleDateString()}</div>}
          {batch.processedAt && <div><span style={{ color: '#888' }}>Processed: </span>{new Date(batch.processedAt).toLocaleDateString()}</div>}
        </div>
      </div>

      {/* Actions */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        {canApprove && (
          <button onClick={handleApprove} disabled={busy === 'approve'}
            style={{ padding: '0.5rem 1rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            {busy === 'approve' ? '…' : 'Approve'}
          </button>
        )}
        {canProcess && (
          <button onClick={handleProcess} disabled={busy === 'process'}
            style={{ padding: '0.5rem 1rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            {busy === 'process' ? '…' : 'Process Batch'}
          </button>
        )}
        {canHold && (
          <button onClick={() => setHoldModal(true)}
            style={{ padding: '0.5rem 1rem', background: '#ed8936', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            Hold
          </button>
        )}
      </div>

      {/* Transactions */}
      <h2 style={{ fontSize: 16, marginBottom: '0.75rem' }}>Transactions</h2>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Code</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Gross</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Commission</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Net Disbursed</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Bank Ref</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {(batch.transactions ?? []).map(tx => (
            <tr key={tx.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', fontFamily: 'monospace', fontSize: 12, color: '#555' }}>{tx.transactionCode ?? '—'}</td>
              <td style={{ padding: '0.75rem' }}>
                <div style={{ fontWeight: 500 }}>{tx.storeName}</div>
                {tx.errorMessage && <div style={{ fontSize: 12, color: '#e53e3e' }}>{tx.errorMessage}</div>}
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'right', fontSize: 13 }}>{tx.grossRevenue != null ? `${tx.grossRevenue.toLocaleString()} ₫` : '—'}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right', fontSize: 13, color: '#e53e3e' }}>{tx.commissionAmount != null ? `-${tx.commissionAmount.toLocaleString()} ₫` : '—'}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 700 }}>{tx.amount?.toLocaleString()} ₫</td>
              <td style={{ padding: '0.75rem', fontSize: 12, color: '#888', fontFamily: 'monospace' }}>{tx.bankReference ?? '—'}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: STATUS_COLORS[tx.status] ?? '#555', fontWeight: 600, fontSize: 13 }}>{tx.status}</span>
              </td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <div style={{ display: 'flex', gap: '0.4rem', justifyContent: 'center' }}>
                  <Link to={`/admin/payouts/transactions/${tx.id}`}
                    style={{ padding: '0.2rem 0.5rem', background: '#ebf8ff', borderRadius: 4, fontSize: 12, color: '#2b6cb0', textDecoration: 'none' }}>
                    View
                  </Link>
                  {tx.status === 'Failed' && (
                    <button onClick={() => handleRetry(tx.id)} disabled={busy === tx.id}
                      style={{ padding: '0.2rem 0.5rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749', fontSize: 12 }}>
                      {busy === tx.id ? '…' : 'Retry'}
                    </button>
                  )}
                  {tx.status !== 'Excluded' && tx.status !== 'Completed' && (
                    <button onClick={() => { setExcludeModal(tx.id); setReason(''); }}
                      style={{ padding: '0.2rem 0.5rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030', fontSize: 12 }}>
                      Exclude
                    </button>
                  )}
                </div>
              </td>
            </tr>
          ))}
          {(batch.transactions ?? []).length === 0 && (
            <tr><td colSpan={8} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No transactions.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
