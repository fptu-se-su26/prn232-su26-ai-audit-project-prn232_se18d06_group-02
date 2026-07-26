import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Transaction {
  id: string;
  type: string;
  amount: number;
  status: string;
  storeName?: string;
  orderId?: string;
  createdAt: string;
  description?: string;
}

export default function AdminTransactionsPage() {
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [typeFilter, setTypeFilter] = useState('');

  const fetchTransactions = () => {
    setLoading(true);
    adminApi.getTransactions({ type: typeFilter || undefined })
      .then(d => setTransactions((d as { items?: Transaction[] }).items ?? (d as Transaction[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchTransactions(); }, []);

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Transactions</h1>

      <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', alignItems: 'center' }}>
        <select value={typeFilter} onChange={e => setTypeFilter(e.target.value)} style={{ padding: '0.5rem' }}>
          <option value="">All Types</option>
          <option>Payment</option>
          <option>Payout</option>
          <option>Refund</option>
          <option>PlatformFee</option>
        </select>
        <button onClick={fetchTransactions} style={{ padding: '0.5rem 1rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Filter
        </button>
      </div>

      <table style={{ width: '100%', borderCollapse: 'collapse' }}>
        <thead>
          <tr style={{ background: '#f0f0f0' }}>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Date</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Type</th>
            <th style={{ padding: '0.75rem', textAlign: 'right' }}>Amount</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
            <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
            <th style={{ padding: '0.75rem', textAlign: 'left' }}>Description</th>
          </tr>
        </thead>
        <tbody>
          {transactions.map(t => (
            <tr key={t.id} style={{ borderBottom: '1px solid #eee' }}>
              <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(t.createdAt).toLocaleDateString()}</td>
              <td style={{ padding: '0.75rem' }}>{t.type}</td>
              <td style={{ padding: '0.75rem', textAlign: 'right', fontWeight: 600 }}>{t.amount?.toLocaleString()} VND</td>
              <td style={{ padding: '0.75rem', color: '#555', fontSize: 13 }}>{t.storeName ?? '—'}</td>
              <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                <span style={{ color: t.status === 'Completed' ? '#38a169' : t.status === 'Pending' ? '#ed8936' : '#e53e3e' }}>{t.status}</span>
              </td>
              <td style={{ padding: '0.75rem', color: '#555', fontSize: 13 }}>{t.description ?? '—'}</td>
            </tr>
          ))}
          {transactions.length === 0 && (
            <tr><td colSpan={6} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No transactions found.</td></tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
