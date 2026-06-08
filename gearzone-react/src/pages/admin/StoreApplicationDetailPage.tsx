import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface HistoryEntry {
  status: string;
  timestamp: string;
  note?: string;
}

interface StoreApplication {
  id: string;
  storeName: string;
  applicantName: string;
  applicantEmail: string;
  businessType?: string;
  phone?: string;
  email?: string;
  address?: string;
  province?: string;
  status: string;
  submittedAt: string;
  updatedAt?: string;
  rejectReason?: string;
  informNote?: string;
  identityCardFrontImageUrl?: string;
  identityCardBackImageUrl?: string;
  bankName?: string;
  bankAccountNumber?: string;
  bankAccountName?: string;
  fullName?: string;
  identityNumber?: string;
  identityIssuedDate?: string;
  identityIssuedPlace?: string;
  taxCode?: string;
  commissionRate?: number;
  history?: HistoryEntry[];
}

const STATUS_COLORS: Record<string, string> = {
  PendingReview: '#ed8936', Pending: '#ed8936',
  Approved: '#38a169', Rejected: '#e53e3e',
  RequestInfo: '#3182ce',
};

const REJECT_QUICK_REASONS = [
  'Incomplete documentation',
  'Invalid identity card',
  'Tax code does not match',
  'Business address unverifiable',
];

export default function AdminStoreApplicationDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [app, setApp] = useState<StoreApplication | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [modal, setModal] = useState<'reject' | 'request-info' | null>(null);
  const [input, setInput] = useState('');
  const [error, setError] = useState('');
  const [lightbox, setLightbox] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    adminApi.storeApplications.get(id)
      .then(d => setApp(d as StoreApplication))
      .catch(() => navigate('/admin/store-applications'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  const reload = () => {
    if (!id) return;
    adminApi.storeApplications.get(id).then(d => setApp(d as StoreApplication));
  };

  const handleApprove = async () => {
    if (!id) return;
    setBusy(true);
    try { await adminApi.storeApplications.approve(id); reload(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(false); }
  };

  const handleModalAction = async () => {
    if (!id || !input) return;
    setBusy(true);
    try {
      if (modal === 'reject') await adminApi.storeApplications.reject(id, input);
      else if (modal === 'request-info') await adminApi.storeApplications.requestInfo(id, input);
      setModal(null); setInput(''); reload();
    } catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(false); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!app) return null;

  const isPending = app.status === 'PendingReview' || app.status === 'Pending';
  const statusLabel = app.status === 'PendingReview' ? 'Pending Review' : app.status;

  return (
    <div style={{ padding: '2rem', maxWidth: 960, margin: '0 auto', paddingBottom: isPending ? '5rem' : '2rem' }}>
      {/* Lightbox */}
      {lightbox && (
        <div onClick={() => setLightbox(null)} style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.85)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 2000, cursor: 'pointer' }}>
          <img src={lightbox} alt="" style={{ maxWidth: '90vw', maxHeight: '90vh', borderRadius: 8 }} />
        </div>
      )}

      {/* Modal */}
      {modal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: '#fff', padding: '2rem', borderRadius: 8, maxWidth: 460, width: '90%' }}>
            <h3 style={{ marginTop: 0 }}>{modal === 'reject' ? 'Reject Application' : 'Request Information'}</h3>
            {modal === 'reject' && (
              <div style={{ display: 'flex', gap: '0.4rem', flexWrap: 'wrap', marginBottom: '0.75rem' }}>
                {REJECT_QUICK_REASONS.map(r => (
                  <button key={r} onClick={() => setInput(r)}
                    style={{ padding: '0.25rem 0.65rem', borderRadius: 12, border: '1px solid', borderColor: input === r ? '#e53e3e' : '#e2e8f0', background: input === r ? '#fff5f5' : '#fff', color: input === r ? '#c53030' : '#555', cursor: 'pointer', fontSize: 12 }}>
                    {r}
                  </button>
                ))}
              </div>
            )}
            <textarea value={input} onChange={e => setInput(e.target.value)}
              placeholder={modal === 'reject' ? 'Enter or select rejection reason…' : 'What information is needed from the applicant?'}
              rows={4} style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', marginBottom: '1rem', borderRadius: 4, border: '1px solid #ccc' }} />
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button onClick={handleModalAction} disabled={busy || !input}
                style={{ flex: 1, padding: '0.5rem', background: modal === 'reject' ? '#e53e3e' : '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
                {busy ? '…' : 'Confirm'}
              </button>
              <button onClick={() => { setModal(null); setInput(''); }}
                style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <Link to="/admin/store-applications" style={{ color: '#3182ce', textDecoration: 'none' }}>← Applications</Link>
        <div style={{ flex: 1 }}>
          <h1 style={{ margin: 0 }}>{app.storeName}</h1>
          <div style={{ fontSize: 12, color: '#888', marginTop: '0.2rem' }}>
            Submitted: {new Date(app.submittedAt).toLocaleDateString()}
            {app.businessType && <> · {app.businessType}</>}
          </div>
        </div>
        <span style={{ padding: '0.3rem 0.85rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[app.status] ?? '#555', fontWeight: 700, fontSize: 13 }}>
          {statusLabel}
        </span>
      </div>

      {error && <div style={{ background: '#fed7d7', padding: '0.75rem', borderRadius: 4, marginBottom: '1rem', color: '#c53030' }}>{error}</div>}

      {/* Rejection reason banner */}
      {app.rejectReason && (
        <div style={{ background: '#fff5f5', border: '1px solid #feb2b2', borderRadius: 6, padding: '0.85rem 1rem', marginBottom: '1rem', display: 'flex', gap: '0.75rem', alignItems: 'flex-start' }}>
          <span style={{ color: '#c53030', fontSize: 18, flexShrink: 0 }}>✕</span>
          <div>
            <div style={{ fontWeight: 600, color: '#c53030', marginBottom: '0.2rem' }}>Application Rejected</div>
            <div style={{ fontSize: 13, color: '#555' }}>{app.rejectReason}</div>
          </div>
        </div>
      )}
      {app.informNote && (
        <div style={{ background: '#ebf8ff', border: '1px solid #bee3f8', borderRadius: 6, padding: '0.85rem 1rem', marginBottom: '1rem' }}>
          <div style={{ fontWeight: 600, color: '#2b6cb0', marginBottom: '0.2rem' }}>Information Requested</div>
          <div style={{ fontSize: 13 }}>{app.informNote}</div>
        </div>
      )}

      {/* Main grid */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 340px', gap: '1.5rem' }}>
        {/* Left */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {/* Basic info */}
          <InfoCard title="Basic Information">
            <Row label="Store Name" value={app.storeName} />
            {app.taxCode && <Row label="Tax Code" value={app.taxCode} />}
            {app.businessType && <Row label="Business Type" value={app.businessType} />}
            {app.email && <Row label="Email" value={app.email} />}
            {app.phone && <Row label="Phone" value={app.phone} />}
            {app.address && <Row label="Address" value={app.address} />}
            {app.province && <Row label="Province" value={app.province} />}
          </InfoCard>

          {/* Owner */}
          <InfoCard title="Owner & Representative">
            <Row label="Full Name" value={app.applicantName} />
            <Row label="Email" value={app.applicantEmail} />
            {app.fullName && app.fullName !== app.applicantName && <Row label="Legal Name" value={app.fullName} />}
            {app.phone && <Row label="Phone" value={app.phone} />}
            {app.identityNumber && <Row label="Identity No." value={app.identityNumber} />}
            {app.identityIssuedDate && <Row label="Issued Date" value={app.identityIssuedDate} />}
            {app.identityIssuedPlace && <Row label="Issued Place" value={app.identityIssuedPlace} />}
          </InfoCard>

          {/* Banking */}
          {(app.bankName || app.bankAccountNumber) && (
            <InfoCard title="Banking & Financial Setup">
              {app.bankName && <Row label="Bank" value={app.bankName} />}
              {app.bankAccountNumber && <Row label="Account No." value={app.bankAccountNumber} />}
              {app.bankAccountName && <Row label="Account Name" value={app.bankAccountName} />}
              {app.commissionRate != null && <Row label="Commission Rate" value={`${app.commissionRate}%`} />}
            </InfoCard>
          )}

          {/* Metadata */}
          <InfoCard title="Application Metadata">
            <Row label="Application ID" value={app.id} />
            <Row label="Submitted" value={new Date(app.submittedAt).toLocaleString()} />
            {app.updatedAt && <Row label="Last Updated" value={new Date(app.updatedAt).toLocaleString()} />}
          </InfoCard>
        </div>

        {/* Right */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {/* Application history */}
          <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontWeight: 600, marginBottom: '1rem', color: '#555' }}>Application History</div>
            <div style={{ position: 'relative' }}>
              <div style={{ position: 'absolute', left: 9, top: 0, bottom: 0, width: 2, background: '#e2e8f0' }} />
              {[
                { status: 'Application Submitted', timestamp: app.submittedAt, note: 'Applicant submitted for review' },
                ...(app.history ?? []),
                { status: statusLabel, timestamp: app.updatedAt ?? app.submittedAt, note: null },
              ].filter((h, i, arr) => i === arr.length - 1 || h.status !== arr[arr.length - 1].status).map((h, i) => (
                <div key={i} style={{ display: 'flex', gap: '0.75rem', marginBottom: '0.85rem', position: 'relative' }}>
                  <div style={{ width: 20, height: 20, borderRadius: '50%', background: i === 0 ? '#e2e8f0' : STATUS_COLORS[app.status] ?? '#3182ce', border: '2px solid #fff', flexShrink: 0, zIndex: 1 }} />
                  <div>
                    <div style={{ fontWeight: 600, fontSize: 13 }}>{h.status}</div>
                    <div style={{ fontSize: 11, color: '#aaa' }}>{new Date(h.timestamp).toLocaleDateString()}</div>
                    {h.note && <div style={{ fontSize: 12, color: '#888', marginTop: '0.15rem' }}>{h.note}</div>}
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Documents */}
          <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
              <div style={{ fontWeight: 600, color: '#555' }}>Documents Preview</div>
              {(app.identityCardFrontImageUrl || app.identityCardBackImageUrl) ? (
                <span style={{ background: '#c6f6d5', color: '#276749', borderRadius: 12, padding: '0.1rem 0.5rem', fontSize: 11, fontWeight: 600 }}>Ready</span>
              ) : (
                <span style={{ background: '#fed7d7', color: '#c53030', borderRadius: 12, padding: '0.1rem 0.5rem', fontSize: 11, fontWeight: 600 }}>Missing</span>
              )}
            </div>
            {app.identityCardFrontImageUrl || app.identityCardBackImageUrl ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
                {[['Front', app.identityCardFrontImageUrl], ['Back', app.identityCardBackImageUrl]].map(([label, url]) => url && (
                  <div key={label as string}>
                    <div style={{ fontSize: 12, color: '#888', marginBottom: '0.25rem' }}>ID {label}</div>
                    <img src={url as string} alt={`ID ${label}`} onClick={() => setLightbox(url as string)}
                      style={{ width: '100%', height: 120, objectFit: 'cover', borderRadius: 6, border: '1px solid #e2e8f0', cursor: 'pointer' }} />
                    <button onClick={() => setLightbox(url as string)}
                      style={{ marginTop: '0.25rem', fontSize: 11, color: '#3182ce', background: 'none', border: 'none', cursor: 'pointer', padding: 0 }}>
                      Full Image
                    </button>
                  </div>
                ))}
              </div>
            ) : (
              <div style={{ background: '#fff5f5', borderRadius: 4, padding: '0.75rem', fontSize: 13, color: '#c53030' }}>
                No identity documents uploaded.
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Sticky bottom action bar (pending only) */}
      {isPending && (
        <div style={{ position: 'fixed', bottom: 0, left: 0, right: 0, background: '#fff', borderTop: '1px solid #e2e8f0', padding: '0.85rem 2rem', display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', zIndex: 100 }}>
          <button onClick={() => setModal('request-info')}
            style={{ padding: '0.5rem 1.1rem', background: '#ebf8ff', color: '#2b6cb0', border: '1px solid #bee3f8', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            Request Info
          </button>
          <button onClick={() => setModal('reject')}
            style={{ padding: '0.5rem 1.1rem', background: '#e53e3e', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            Reject
          </button>
          <button onClick={handleApprove} disabled={busy}
            style={{ padding: '0.5rem 1.5rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            {busy ? '…' : 'Approve Application'}
          </button>
        </div>
      )}
    </div>
  );
}

function InfoCard({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
      <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>{title}</div>
      {children}
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.35rem', fontSize: 13 }}>
      <span style={{ color: '#888' }}>{label}</span>
      <span style={{ fontWeight: 500, textAlign: 'right', maxWidth: '60%' }}>{value}</span>
    </div>
  );
}
