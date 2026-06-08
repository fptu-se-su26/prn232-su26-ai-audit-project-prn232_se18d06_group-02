import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface Batch {
  id: string;
  batchCode: string;
  status: string;
  totalTransactions: number;
  totalAmount: number;
  createdAt: string;
  processedAt?: string;
}

const STATUS_COLORS: Record<string, string> = {
  Approved: '#38a169', Processing: '#3182ce',
  Pending: '#ed8936', Hold: '#ed8936',
  Completed: '#38a169', Failed: '#e53e3e',
};

export default function AdminPayoutBatchesPage() {
  const navigate = useNavigate();
  const [batches, setBatches] = useState<Batch[]>([]);
  const [loading, setLoading] = useState(true);

  const fetch = () => {
    setLoading(true);
    adminApi.payouts.listBatches()
      .then(d => setBatches((d as { items?: Batch[] }).items ?? (d as Batch[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetch(); }, []);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
        <h1 style={{ margin: 0 }}>Payout Batches</h1>
        <a href="/admin/payouts/seller-summary"
          style={{ padding: '0.5rem 1rem', background: '#38a169', color: '#fff', textDecoration: 'none', borderRadius: 4, fontSize: 14 }}>
          Generate Batch
        </a>
      </div>

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Batch Code</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Transactions</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Total Amount</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Created</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {batches.map(b => (
            <tr key={b.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', fontWeight: 600, fontFamily: 'monospace' }}>{b.batchCode}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right' }}>{b.totalTransactions}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 700 }}>{b.totalAmount?.toLocaleString()} ₫</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: STATUS_COLORS[b.status] ?? '#555', fontWeight: 600 }}>{b.status}</span>
              </td>
              <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(b.createdAt).toLocaleDateString()}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <button onClick={() => navigate(`/admin/payouts/batches/${b.id}`)}
                  style={{ padding: '0.25rem 0.75rem', background: '#ebf8ff', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#2b6cb0' }}>
                  Details
                </button>
              </td>
            </tr>
          ))}
          {batches.length === 0 && (
            <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No payout batches found.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
