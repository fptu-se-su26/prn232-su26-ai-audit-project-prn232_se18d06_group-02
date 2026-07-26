import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface Application {
  id: string;
  applicantName: string;
  applicantEmail: string;
  applicantPhone?: string;
  storeName: string;
  taxCode?: string;
  businessType?: string;
  status: string;
  submittedAt: string;
}

const STATUS_COLORS: Record<string, string> = {
  PendingReview: '#ed8936', Pending: '#ed8936',
  Approved: '#38a169', Rejected: '#e53e3e',
  RequestInfo: '#3182ce',
};

const STATUS_LABEL: Record<string, string> = {
  PendingReview: 'Pending Review',
};

export default function AdminStoreApplicationsPage() {
  const navigate = useNavigate();
  const [applications, setApplications] = useState<Application[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState('');
  const [processing, setProcessing] = useState<string | null>(null);
  const [rejectId, setRejectId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState('');

  const fetchApps = () => {
    setLoading(true);
    adminApi.storeApplications.list({ status: statusFilter || undefined })
      .then(d => setApplications((d as { items?: Application[] }).items ?? (d as Application[]) ?? []))
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchApps(); }, []);

  const handleApprove = async (id: string) => {
    setProcessing(id);
    try { await adminApi.storeApplications.approve(id); fetchApps(); }
    finally { setProcessing(null); }
  };

  const handleReject = async () => {
    if (!rejectId || !rejectReason) return;
    setProcessing(rejectId);
    try {
      await adminApi.storeApplications.reject(rejectId, rejectReason);
      setRejectId(null); setRejectReason(''); fetchApps();
    } finally { setProcessing(null); }
  };

  const stats = [
    { label: 'Total Applications', value: applications.length, color: '#3182ce' },
    { label: 'Pending Review', value: applications.filter(a => a.status === 'PendingReview' || a.status === 'Pending').length, color: '#ed8936' },
    { label: 'Approved', value: applications.filter(a => a.status === 'Approved').length, color: '#38a169' },
    { label: 'Rejected', value: applications.filter(a => a.status === 'Rejected').length, color: '#e53e3e' },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      {/* Reject modal */}
      {rejectId && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: '#fff', padding: '2rem', borderRadius: 8, maxWidth: 420, width: '90%' }}>
            <h3 style={{ marginTop: 0 }}>Reject Application</h3>
            <textarea value={rejectReason} onChange={e => setRejectReason(e.target.value)}
              placeholder="Rejection reason…" rows={4}
              style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', marginBottom: '1rem', borderRadius: 4, border: '1px solid #ccc' }} />
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button onClick={handleReject} disabled={!rejectReason || !!processing}
                style={{ flex: 1, padding: '0.5rem', background: '#e53e3e', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
                {processing ? '…' : 'Confirm Reject'}
              </button>
              <button onClick={() => { setRejectId(null); setRejectReason(''); }}
                style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      <h1 style={{ marginBottom: '1.25rem' }}>Store Applications</h1>

      {/* Stats */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {stats.map(s => (
          <div key={s.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.3rem' }}>{s.label}</div>
            <div style={{ fontWeight: 700, fontSize: 20, color: s.color }}>{s.value}</div>
          </div>
        ))}
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 180 }}>
          <option value="">All Statuses</option>
          <option value="PendingReview">Pending Review</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
          <option value="RequestInfo">Request Info</option>
        </select>
        <button onClick={fetchApps}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Filter
        </button>
      </div>

      {loading ? <div style={{ padding: '2rem', textAlign: 'center' }}>Loading…</div> : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ background: '#f0f0f0' }}>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Applicant</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Store</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Tax Code</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Business Type</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
              <th style={{ padding: '0.75rem', textAlign: 'left' }}>Submitted</th>
              <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {applications.map(a => (
              <tr key={a.id} style={{ borderBottom: '1px solid #eee' }}>
                <td style={{ padding: '0.75rem' }}>
                  <div style={{ fontWeight: 500 }}>{a.applicantName}</div>
                  <div style={{ fontSize: 12, color: '#888' }}>{a.applicantEmail}</div>
                  {a.applicantPhone && <div style={{ fontSize: 12, color: '#aaa' }}>{a.applicantPhone}</div>}
                </td>
                <td style={{ padding: '0.75rem', fontWeight: 500 }}>{a.storeName}</td>
                <td style={{ padding: '0.75rem', fontSize: 13, color: '#555', fontFamily: 'monospace' }}>{a.taxCode ?? '—'}</td>
                <td style={{ padding: '0.75rem', fontSize: 13, color: '#555' }}>{a.businessType ?? '—'}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <span style={{ padding: '0.15rem 0.6rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[a.status] ?? '#555', fontWeight: 600, fontSize: 12 }}>
                    {STATUS_LABEL[a.status] ?? a.status}
                  </span>
                </td>
                <td style={{ padding: '0.75rem', color: '#888', fontSize: 13 }}>{new Date(a.submittedAt).toLocaleDateString()}</td>
                <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                  <div style={{ display: 'flex', gap: '0.4rem', justifyContent: 'center' }}>
                    <button onClick={() => navigate(`/admin/store-applications/${a.id}`)}
                      style={{ padding: '0.2rem 0.6rem', background: '#ebf8ff', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#2b6cb0', fontSize: 12 }}>
                      View
                    </button>
                    {(a.status === 'PendingReview' || a.status === 'Pending') && (
                      <>
                        <button onClick={() => handleApprove(a.id)} disabled={processing === a.id}
                          style={{ padding: '0.2rem 0.6rem', background: '#c6f6d5', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#276749', fontSize: 12 }}>
                          {processing === a.id ? '…' : 'Approve'}
                        </button>
                        <button onClick={() => setRejectId(a.id)}
                          style={{ padding: '0.2rem 0.6rem', background: '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: '#c53030', fontSize: 12 }}>
                          Reject
                        </button>
                      </>
                    )}
                  </div>
                </td>
              </tr>
            ))}
            {applications.length === 0 && (
              <tr><td colSpan={7} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No applications found.</td></tr>
            )}
          </tbody>
        </table>
      )}
    </div>
  );
}
