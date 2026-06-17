import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface RevenueData {
  totalRevenue: number;
  pendingPayout: number;
  completedPayout: number;
  transactions: Array<{
    id: string;
    amount: number;
    type: string;
    status: string;
    createdAt: string;
    note?: string;
  }>;
  monthlyRevenue?: Array<{ month: string; revenue: number }>;
}

export default function SellerRevenuePage() {
  const [data, setData] = useState<RevenueData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    sellerApi.getRevenue()
      .then(d => setData(d as RevenueData))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!data) return <div style={{ padding: '2rem' }}>Failed to load revenue data.</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Revenue</h1>

      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '2rem' }}>
        {[
          { label: 'Total Revenue', value: data.totalRevenue, color: '#38a169' },
          { label: 'Pending Payout', value: data.pendingPayout, color: '#ed8936' },
          { label: 'Completed Payout', value: data.completedPayout, color: '#3182ce' },
        ].map(s => (
          <div key={s.label} style={{ flex: '1 1 200px', padding: '1.5rem', background: '#f9f9f9', borderRadius: 8, borderLeft: `4px solid ${s.color}` }}>
            <p style={{ margin: 0, color: '#888', fontSize: 13 }}>{s.label}</p>
            <p style={{ margin: '0.5rem 0 0', fontSize: 22, fontWeight: 700, color: s.color }}>{(s.value ?? 0).toLocaleString()} VND</p>
          </div>
        ))}
      </div>

      <h2>Payout Transactions</h2>
      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Date</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Type</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Amount</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Note</th>
          </tr>
        </thead>
        <tbody>
          {data.transactions?.map(t => (
            <tr key={t.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(t.createdAt).toLocaleDateString()}</td>
              <td style={{ padding: '0.75rem' }}>{t.type}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 600 }}>{t.amount?.toLocaleString()} VND</td>
              <td style={{ padding: '0.75rem' }}>
                <span style={{ color: t.status === 'Completed' ? '#38a169' : t.status === 'Pending' ? '#ed8936' : '#e53e3e' }}>{t.status}</span>
              </td>
              <td style={{ padding: '0.75rem', color: '#555', fontSize: 13 }}>{t.note ?? '—'}</td>
            </tr>
          ))}
          {(!data.transactions || data.transactions.length === 0) && (
            <tr><td colSpan={5} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No transactions yet.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
