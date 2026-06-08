import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface ReviewItem {
  reviewId: string;
  productName: string;
  productImageUrl?: string;
  variantName?: string;
  buyerName: string;
  rating: number;
  comment?: string;
  createdAt: string;
  editedAt?: string;
  sellerReply?: string;
  repliedAt?: string;
}

type FilterType = 'all' | 'awaiting' | 'replied';

function Stars({ rating }: { rating: number }) {
  return (
    <span style={{ color: '#ed8936', fontSize: 15 }}>
      {'★'.repeat(rating)}{'☆'.repeat(5 - rating)}
      <span style={{ marginLeft: '0.4rem', color: '#555', fontWeight: 600, fontSize: 13 }}>{rating}/5</span>
    </span>
  );
}

export default function SellerReviewsPage() {
  const [reviews, setReviews] = useState<ReviewItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [filter, setFilter] = useState<FilterType>('all');
  const [replyId, setReplyId] = useState<string | null>(null);
  const [replyText, setReplyText] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const fetchReviews = (p = page) => {
    setLoading(true);
    sellerApi.storeReviews(p)
      .then(d => {
        const res = d as { items?: ReviewItem[]; totalPages?: number };
        setReviews(res.items ?? (d as ReviewItem[]) ?? []);
        setTotalPages(res.totalPages ?? 1);
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchReviews(page); }, [page]);

  const handleReply = async (reviewId: string) => {
    if (!replyText.trim()) return;
    setSaving(true); setError('');
    try {
      await sellerApi.replyToReview(reviewId, replyText.trim());
      setReplyId(null); setReplyText('');
      fetchReviews(page);
    } catch (e) { setError(e instanceof Error ? e.message : 'Failed to post reply'); }
    finally { setSaving(false); }
  };

  const openReply = (r: ReviewItem) => {
    setReplyId(r.reviewId);
    setReplyText(r.sellerReply ?? '');
  };

  const filtered = reviews.filter(r => {
    if (filter === 'awaiting') return !r.sellerReply;
    if (filter === 'replied') return !!r.sellerReply;
    return true;
  });

  const FILTERS: { key: FilterType; label: string }[] = [
    { key: 'all', label: 'All' },
    { key: 'awaiting', label: 'Awaiting Reply' },
    { key: 'replied', label: 'Replied' },
  ];

  if (loading && page === 1) return <div style={{ padding: '2rem' }}>Loading…</div>;

  return (
    <div style={{ padding: '2rem' }}>
      <h1 style={{ marginBottom: '1.25rem' }}>Customer Reviews</h1>

      {/* Filter chips */}
      <div style={{ display: 'flex', gap: '0.5rem', marginBottom: '1.5rem' }}>
        {FILTERS.map(f => (
          <button key={f.key} onClick={() => setFilter(f.key)}
            style={{
              padding: '0.35rem 1rem', borderRadius: 20, border: '1px solid',
              borderColor: filter === f.key ? '#3182ce' : '#e2e8f0',
              background: filter === f.key ? '#ebf8ff' : '#fff',
              color: filter === f.key ? '#3182ce' : '#555',
              fontWeight: filter === f.key ? 600 : 400,
              cursor: 'pointer', fontSize: 13,
            }}>
            {f.label}
            {f.key === 'awaiting' && (
              <span style={{ marginLeft: '0.35rem', background: '#ed8936', color: '#fff', borderRadius: 10, fontSize: 11, padding: '1px 5px' }}>
                {reviews.filter(r => !r.sellerReply).length}
              </span>
            )}
          </button>
        ))}
      </div>

      {error && <div style={{ background: '#fed7d7', padding: '0.75rem', borderRadius: 4, marginBottom: '1rem', color: '#c53030' }}>{error}</div>}

      {filtered.length === 0 && !loading && (
        <div style={{ textAlign: 'center', padding: '3rem', color: '#888' }}>No reviews found.</div>
      )}

      <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        {filtered.map(r => (
          <div key={r.reviewId} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1.25rem' }}>
            <div style={{ display: 'flex', gap: '1rem', alignItems: 'flex-start', marginBottom: '0.75rem' }}>
              {r.productImageUrl
                ? <img src={r.productImageUrl} alt="" style={{ width: 64, height: 64, objectFit: 'cover', borderRadius: 6, flexShrink: 0, border: '1px solid #eee' }} />
                : <div style={{ width: 64, height: 64, borderRadius: 6, background: '#f0f0f0', flexShrink: 0 }} />
              }
              <div style={{ flex: 1 }}>
                <div style={{ fontWeight: 600, marginBottom: '0.15rem' }}>{r.productName}</div>
                {r.variantName && <div style={{ fontSize: 12, color: '#888', marginBottom: '0.15rem' }}>{r.variantName}</div>}
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.15rem', flexWrap: 'wrap' }}>
                  <Stars rating={r.rating} />
                  <span style={{ fontSize: 12, color: '#888' }}>{r.buyerName}</span>
                  <span style={{ fontSize: 12, color: '#aaa' }}>{new Date(r.createdAt).toLocaleDateString()}</span>
                </div>
              </div>
              {r.sellerReply && (
                <span style={{ background: '#c6f6d5', color: '#276749', borderRadius: 12, fontSize: 11, padding: '2px 8px', fontWeight: 600, flexShrink: 0 }}>Replied</span>
              )}
            </div>

            {r.comment ? (
              <div style={{ fontSize: 14, color: '#444', marginBottom: '0.75rem', lineHeight: 1.6, background: '#f9f9f9', padding: '0.75rem', borderRadius: 4 }}>
                "{r.comment}"
              </div>
            ) : (
              <div style={{ fontSize: 13, color: '#aaa', fontStyle: 'italic', marginBottom: '0.75rem' }}>No written review.</div>
            )}

            {/* Existing reply display */}
            {r.sellerReply && replyId !== r.reviewId && (
              <div style={{ background: '#ebf8ff', borderRadius: 4, padding: '0.75rem', fontSize: 13, marginBottom: '0.5rem' }}>
                <div style={{ fontWeight: 600, color: '#2b6cb0', marginBottom: '0.2rem' }}>Your reply</div>
                <div style={{ color: '#444' }}>{r.sellerReply}</div>
                {r.repliedAt && <div style={{ color: '#aaa', fontSize: 11, marginTop: '0.25rem' }}>{new Date(r.repliedAt).toLocaleDateString()}</div>}
              </div>
            )}

            {/* Inline reply textarea */}
            {replyId === r.reviewId ? (
              <div>
                <textarea
                  value={replyText}
                  onChange={e => setReplyText(e.target.value.slice(0, 2000))}
                  placeholder="Write your reply…"
                  rows={3}
                  style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', borderRadius: 4, border: '1px solid #bee3f8', marginBottom: '0.25rem', fontSize: 13, resize: 'vertical' }}
                />
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
                  <span style={{ fontSize: 11, color: replyText.length > 1900 ? '#e53e3e' : '#aaa' }}>{replyText.length}/2000</span>
                </div>
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                  <button onClick={() => handleReply(r.reviewId)} disabled={saving || !replyText.trim()}
                    style={{ padding: '0.4rem 1rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13 }}>
                    {saving ? 'Posting…' : r.sellerReply ? 'Update Reply' : 'Send Reply'}
                  </button>
                  <button onClick={() => { setReplyId(null); setReplyText(''); }}
                    style={{ padding: '0.4rem 0.75rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13 }}>
                    Cancel
                  </button>
                </div>
              </div>
            ) : (
              <button onClick={() => openReply(r)}
                style={{ padding: '0.35rem 0.75rem', background: '#ebf8ff', border: '1px solid #bee3f8', borderRadius: 4, cursor: 'pointer', fontSize: 13, color: '#3182ce' }}>
                {r.sellerReply ? 'Edit reply' : 'Reply'}
              </button>
            )}
          </div>
        ))}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem', marginTop: '1.5rem' }}>
          <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1}
            style={{ padding: '0.4rem 0.8rem', border: '1px solid #e2e8f0', borderRadius: 4, cursor: 'pointer', background: '#fff' }}>
            ← Prev
          </button>
          <span style={{ padding: '0.4rem 0.8rem', fontSize: 13, color: '#555' }}>Page {page} / {totalPages}</span>
          <button onClick={() => setPage(p => Math.min(totalPages, p + 1))} disabled={page === totalPages}
            style={{ padding: '0.4rem 0.8rem', border: '1px solid #e2e8f0', borderRadius: 4, cursor: 'pointer', background: '#fff' }}>
            Next →
          </button>
        </div>
      )}
    </div>
  );
}
