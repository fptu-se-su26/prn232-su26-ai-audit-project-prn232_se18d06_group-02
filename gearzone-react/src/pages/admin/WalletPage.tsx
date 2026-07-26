import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface WalletData {
  platformBalance: number;
  totalHeld: number;
  totalPaidOut: number;
  recentTransactions?: Array<{
    id: string;
    type: string;
    amount: number;
    createdAt: string;
    description?: string;
  }>;
}

export default function AdminWalletPage() {
  const [data, setData] = useState<WalletData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    adminApi.getWallet()
      .then(d => setData(d as WalletData))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!data) return <div style={{ padding: '2rem' }}>Failed to load wallet data.</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Platform Wallet</h1>

      <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '2rem' }}>
        {[
          { label: 'Platform Balance', value: data.platformBalance, color: '#38a169' },
          { label: 'Held for Payouts', value: data.totalHeld, color: '#ed8936' },
          { label: 'Total Paid Out', value: data.totalPaidOut, color: '#3182ce' },
        ].map(s => (
          <div key={s.label} style={{ flex: '1 1 200px', padding: '1.5rem', background: '#f9f9f9', borderRadius: 8, borderLeft: `4px solid ${s.color}` }}>
            <p style={{ margin: 0, color: '#888', fontSize: 13 }}>{s.label}</p>
            <p style={{ margin: '0.5rem 0 0', fontSize: 22, fontWeight: 700, color: s.color }}>{(s.value ?? 0).toLocaleString()} VND</p>
          </div>
        ))}
      </div>

      <div style={{ marginBottom: '1.5rem' }}>
        <Link to="/admin/transactions" style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', borderRadius: 4, textDecoration: 'none', fontWeight: 600 }}>
          View All Transactions
        </Link>
      </div>

      {data.recentTransactions && data.recentTransactions.length > 0 && (
        <>
          <h2>Recent Transactions</h2>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#f0f0f0' }}>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>Date</th>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>Type</th>
                <th style={{ padding: '0.75rem', textAlign: 'right' }}>Amount</th>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>Description</th>
              </tr>
            </thead>
            <tbody>
              {data.recentTransactions.map(t => (
                <tr key={t.id} style={{ borderBottom: '1px solid #eee' }}>
                  <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(t.createdAt).toLocaleDateString()}</td>
                  <td style={{ padding: '0.75rem' }}>{t.type}</td>
                  <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 600 }}>{t.amount?.toLocaleString()} VND</td>
                  <td style={{ padding: '0.75rem', color: '#555', fontSize: 13 }}>{t.description ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
}
