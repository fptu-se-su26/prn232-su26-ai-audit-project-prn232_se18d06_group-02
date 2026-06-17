import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface AuditEntry {
  status: string;
  timestamp: string;
  actor?: string;
  description?: string;
}

interface TransactionDetail {
  id: string;
  transactionCode?: string;
  storeName: string;
  storeId: string;
  storeStatus?: string;
  ownerName: string;
  ownerEmail?: string;
  ownerPhone?: string;
  commissionRate?: number;
  amount: number;
  grossRevenue?: number;
  commissionAmount?: number;
  netAmount?: number;
  status: string;
  batchCode?: string;
  batchId?: string;
  batchStatus?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankAccountName?: string;
  errorMessage?: string;
  createdAt: string;
  processedAt?: string;
  periodStart?: string;
  periodEnd?: string;
  auditTrail?: AuditEntry[];
}

const STATUS_COLORS: Record<string, string> = {
  Completed: '#38a169', Approved: '#38a169',
  Pending: '#ed8936', Processing: '#3182ce',
  Failed: '#e53e3e', Excluded: '#718096',
};

function Row({ label, value }: { label: string; value?: string | null }) {
  if (!value) return null;
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', padding: '0.45rem 0', borderBottom: '1px solid #f5f5f5', fontSize: 13 }}>
      <span style={{ color: '#888' }}>{label}</span>
      <span style={{ fontWeight: 500, textAlign: 'right', maxWidth: '60%' }}>{value}</span>
    </div>
  );
}

export default function AdminPayoutsTransactionDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [tx, setTx] = useState<TransactionDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id) return;
    adminApi.payouts.getTransaction(id)
      .then(d => setTx(d as TransactionDetail))
      .catch(() => navigate('/admin/payouts'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!tx) return null;

  const gross = tx.grossRevenue ?? tx.amount;
  const commission = tx.commissionAmount ?? 0;
  const net = tx.netAmount ?? tx.amount;

  return (
    <div style={{ padding: '2rem', maxWidth: 780, margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        {tx.batchId
          ? <Link to={`/admin/payouts/batches/${tx.batchId}`} style={{ color: '#3182ce', textDecoration: 'none' }}>← {tx.batchCode ?? 'Batch'}</Link>
          : <Link to="/admin/payouts" style={{ color: '#3182ce', textDecoration: 'none' }}>← Payouts</Link>
        }
        <h1 style={{ margin: 0, flex: 1, fontFamily: 'monospace', fontSize: 18 }}>{tx.transactionCode ?? 'Transaction'}</h1>
        <span style={{ padding: '0.25rem 0.75rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[tx.status] ?? '#555', fontWeight: 700, fontSize: 13 }}>
          {tx.status}
        </span>
      </div>

      {tx.errorMessage && (
        <div style={{ background: '#fff5f5', border: '1px solid #feb2b2', borderRadius: 4, padding: '0.75rem', marginBottom: '1.25rem', fontSize: 13 }}>
          Error: {tx.errorMessage}
        </div>
      )}

      {/* Payout calculation card */}
      {tx.grossRevenue != null && (
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1.25rem', marginBottom: '1.5rem' }}>
          <div style={{ fontWeight: 600, color: '#555', marginBottom: '1rem' }}>Payout Calculation</div>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr auto 1fr auto 1fr', gap: '0.5rem', alignItems: 'center' }}>
            <div style={{ background: '#ebf8ff', borderRadius: 8, padding: '1rem', textAlign: 'center' }}>
              <div style={{ fontSize: 11, color: '#888', marginBottom: '0.25rem' }}>Gross Revenue</div>
              <div style={{ fontWeight: 700, fontSize: 18 }}>{gross.toLocaleString()} ₫</div>
              <div style={{ fontSize: 11, color: '#aaa', marginTop: '0.25rem' }}>Total orders value</div>
            </div>
            <div style={{ textAlign: 'center', fontWeight: 700, fontSize: 20, color: '#888' }}>−</div>
            <div style={{ background: '#fff5f5', borderRadius: 8, padding: '1rem', textAlign: 'center' }}>
              <div style={{ fontSize: 11, color: '#888', marginBottom: '0.25rem' }}>Commission {tx.commissionRate != null ? `(${tx.commissionRate}%)` : ''}</div>
              <div style={{ fontWeight: 700, fontSize: 18, color: '#e53e3e' }}>{commission.toLocaleString()} ₫</div>
              <div style={{ fontSize: 11, color: '#aaa', marginTop: '0.25rem' }}>Platform fee</div>
            </div>
            <div style={{ textAlign: 'center', fontWeight: 700, fontSize: 20, color: '#888' }}>=</div>
            <div style={{ background: '#f0fff4', borderRadius: 8, padding: '1rem', textAlign: 'center' }}>
              <div style={{ fontSize: 11, color: '#888', marginBottom: '0.25rem' }}>Net Payout</div>
              <div style={{ fontWeight: 700, fontSize: 18, color: '#38a169' }}>{net.toLocaleString()} ₫</div>
              <div style={{ fontSize: 11, color: '#aaa', marginTop: '0.25rem' }}>Disbursed to seller</div>
            </div>
          </div>
        </div>
      )}

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem', marginBottom: '1.5rem' }}>
        {/* Transaction info */}
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem', gridColumn: '1/-1' }}>
          <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Transaction Info</div>
          <Row label="Transaction Code" value={tx.transactionCode} />
          <Row label="Batch Code" value={tx.batchCode} />
          <Row label="Period" value={tx.periodStart ? `${new Date(tx.periodStart).toLocaleDateString()} – ${new Date(tx.periodEnd!).toLocaleDateString()}` : undefined} />
          <Row label="Created" value={new Date(tx.createdAt).toLocaleString()} />
          <Row label="Processed" value={tx.processedAt ? new Date(tx.processedAt).toLocaleString() : undefined} />
        </div>

        {/* Store & bank info */}
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
          <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Store</div>
          <div style={{ fontWeight: 600, marginBottom: '0.35rem' }}>{tx.storeName}</div>
          {tx.storeStatus && (
            <span style={{ display: 'inline-block', padding: '0.1rem 0.5rem', borderRadius: 12, background: '#f0f4ff', fontSize: 11, color: STATUS_COLORS[tx.storeStatus] ?? '#555', marginBottom: '0.5rem' }}>
              {tx.storeStatus}
            </span>
          )}
          <Row label="Owner" value={tx.ownerName} />
          <Row label="Email" value={tx.ownerEmail} />
          <Row label="Phone" value={tx.ownerPhone} />
          {tx.commissionRate != null && <Row label="Commission Rate" value={`${tx.commissionRate}%`} />}
          {tx.storeId && (
            <Link to={`/admin/stores/${tx.storeId}`} style={{ fontSize: 12, color: '#3182ce', textDecoration: 'none', display: 'block', marginTop: '0.5rem' }}>
              View Store →
            </Link>
          )}
        </div>

        {/* Bank info */}
        {(tx.bankName || tx.bankAccountNumber) && (
          <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
              <div style={{ fontWeight: 600, color: '#555' }}>Bank Info</div>
              <span style={{ fontSize: 11, background: '#c6f6d5', color: '#276749', borderRadius: 10, padding: '1px 6px', fontWeight: 600 }}>Recorded</span>
            </div>
            <Row label="Bank" value={tx.bankName} />
            <Row label="Account Number" value={tx.bankAccountNumber} />
            <Row label="Account Name" value={tx.bankAccountName} />
            <div style={{ marginTop: '0.5rem', fontSize: 11, color: '#aaa' }}>Bank info snapshot at time of payout</div>
          </div>
        )}
      </div>

      {/* Audit trail */}
      {(tx.auditTrail ?? []).length > 0 && (
        <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1.25rem', marginBottom: '1.5rem' }}>
          <div style={{ fontWeight: 600, marginBottom: '1rem', color: '#555' }}>Audit Trail</div>
          <div style={{ position: 'relative' }}>
            <div style={{ position: 'absolute', left: 9, top: 0, bottom: 0, width: 2, background: 'linear-gradient(to bottom, #3182ce 0%, #e2e8f0 100%)' }} />
            {(tx.auditTrail ?? []).map((entry, i) => (
              <div key={i} style={{ display: 'flex', gap: '0.75rem', marginBottom: '1rem', position: 'relative' }}>
                <div style={{
                  width: 20, height: 20, borderRadius: '50%',
                  background: i === 0 ? '#3182ce' : '#e2e8f0',
                  border: '2px solid #fff', flexShrink: 0, zIndex: 1, marginTop: 1
                }} />
                <div style={{ flex: 1 }}>
                  <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', flexWrap: 'wrap' }}>
                    <span style={{ fontWeight: 600, fontSize: 13 }}>{entry.status}</span>
                    {entry.actor && <span style={{ background: '#e2e8f0', borderRadius: 10, padding: '0.1rem 0.45rem', fontSize: 11, color: '#555' }}>{entry.actor}</span>}
                  </div>
                  <div style={{ fontSize: 11, color: '#aaa', marginTop: '0.1rem' }}>{new Date(entry.timestamp).toLocaleString()}</div>
                  {entry.description && <div style={{ fontSize: 12, color: '#555', marginTop: '0.25rem' }}>{entry.description}</div>}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {tx.batchId && (
        <Link to={`/admin/payouts/batches/${tx.batchId}`} style={{ color: '#3182ce', fontSize: 13 }}>
          View parent batch ({tx.batchCode}) →
        </Link>
      )}
    </div>
  );
}
