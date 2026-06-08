import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { adminApi } from '../../api/admin';

interface ProductVariant {
  sku: string;
  variantName: string;
  attributes?: Record<string, string>;
  price: number;
  stock: number;
}

interface ProductSpec {
  attributeName: string;
  values: string[];
}

interface ProductDetail {
  id: string;
  name: string;
  slug?: string;
  sku?: string;
  status: string;
  storeName: string;
  storeId?: string;
  brandName: string;
  categoryName: string;
  basePrice: number;
  stockQuantity: number;
  platformFeePercent?: number;
  description?: string;
  imageUrls: string[];
  variants?: ProductVariant[];
  specifications?: ProductSpec[];
  createdAt: string;
  updatedAt?: string;
  rejectedReason?: string;
}

const STATUS_COLORS: Record<string, string> = {
  Active: '#38a169', Approved: '#38a169',
  Pending: '#ed8936', Draft: '#718096',
  Inactive: '#718096', Rejected: '#e53e3e', Archived: '#718096',
};

export default function AdminProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [modal, setModal] = useState<'reject' | 'suspend' | 'delete' | null>(null);
  const [reason, setReason] = useState('');
  const [error, setError] = useState('');
  const [activeImg, setActiveImg] = useState(0);

  useEffect(() => {
    if (!id) return;
    adminApi.products.get(id)
      .then(d => setProduct(d as ProductDetail))
      .catch(() => navigate('/admin/products'))
      .finally(() => setLoading(false));
  }, [id, navigate]);

  const reload = () => {
    if (!id) return;
    adminApi.products.get(id).then(d => setProduct(d as ProductDetail));
  };

  const handleApprove = async () => {
    if (!id) return;
    setBusy(true);
    try { await adminApi.products.approve(id); reload(); }
    catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(false); }
  };

  const handleModalAction = async () => {
    if (!id) return;
    setBusy(true);
    try {
      if (modal === 'reject') await adminApi.products.reject(id, reason);
      else if (modal === 'suspend') await adminApi.products.suspend(id, reason);
      else if (modal === 'delete') { await adminApi.products.delete(id, reason); navigate('/admin/products'); return; }
      setModal(null); setReason(''); reload();
    } catch (e) { setError(e instanceof Error ? e.message : 'Failed'); }
    finally { setBusy(false); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!product) return null;

  const images = product.imageUrls ?? [];
  const isPending = product.status === 'Pending' || product.status === 'Draft';

  return (
    <div style={{ padding: '2rem', maxWidth: 960, margin: '0 auto', paddingBottom: '5rem' }}>
      {/* Modal */}
      {modal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: '#fff', padding: '2rem', borderRadius: 8, maxWidth: 420, width: '90%' }}>
            <h3 style={{ marginTop: 0, textTransform: 'capitalize' }}>{modal} Product</h3>
            {modal !== 'suspend' && <p style={{ color: '#555', fontSize: 13, margin: '0 0 0.75rem' }}>
              {modal === 'reject' ? 'Reason will be shown to the seller.' : 'This action cannot be undone.'}
            </p>}
            <textarea value={reason} onChange={e => setReason(e.target.value)}
              placeholder={modal === 'suspend' ? 'Reason (optional)…' : 'Reason (required)…'} rows={3}
              style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box', marginBottom: '1rem', borderRadius: 4, border: '1px solid #ccc' }} />
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button onClick={handleModalAction} disabled={busy || (modal !== 'suspend' && !reason)}
                style={{ flex: 1, padding: '0.5rem', background: modal === 'delete' ? '#e53e3e' : modal === 'reject' ? '#e53e3e' : '#ed8936', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                {busy ? '…' : `Confirm ${modal}`}
              </button>
              <button onClick={() => { setModal(null); setReason(''); }}
                style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                Cancel
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Header */}
      <div style={{ display: 'flex', alignItems: 'flex-start', gap: '1rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
        <Link to="/admin/products" style={{ color: '#3182ce', textDecoration: 'none', marginTop: 4 }}>← Products</Link>
        <div style={{ flex: 1 }}>
          <div style={{ fontSize: 12, color: '#888', marginBottom: '0.2rem' }}>{product.categoryName}</div>
          <h1 style={{ margin: '0 0 0.25rem' }}>{product.name}</h1>
          <div style={{ fontSize: 13, color: '#888' }}>
            Brand: <strong>{product.brandName}</strong>
            {product.sku && <> · SKU: <span style={{ fontFamily: 'monospace' }}>{product.sku}</span></>}
            {' · '}Store: <strong>{product.storeName}</strong>
          </div>
        </div>
        <span style={{ padding: '0.3rem 0.85rem', borderRadius: 12, background: '#f0f4ff', color: STATUS_COLORS[product.status] ?? '#555', fontWeight: 700, fontSize: 13 }}>
          {product.status}
        </span>
      </div>

      {error && <div style={{ background: '#fed7d7', padding: '0.75rem', borderRadius: 4, marginBottom: '1rem', color: '#c53030' }}>{error}</div>}
      {product.rejectedReason && (
        <div style={{ background: '#fff5f5', border: '1px solid #feb2b2', borderRadius: 4, padding: '0.75rem', marginBottom: '1rem', fontSize: 13 }}>
          <strong style={{ color: '#c53030' }}>Rejection Reason:</strong> {product.rejectedReason}
        </div>
      )}

      {/* Main content grid */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 320px', gap: '1.5rem' }}>
        {/* Left column */}
        <div>
          {/* Image gallery */}
          {images.length > 0 && (
            <div style={{ marginBottom: '1.5rem' }}>
              <img src={images[activeImg]} alt="" style={{ width: '100%', maxHeight: 380, objectFit: 'contain', borderRadius: 8, border: '1px solid #e2e8f0', background: '#fafafa' }} />
              {images.length > 1 && (
                <div style={{ display: 'flex', gap: '0.5rem', marginTop: '0.5rem', flexWrap: 'wrap' }}>
                  {images.map((url, i) => (
                    <img key={i} src={url} alt="" onClick={() => setActiveImg(i)}
                      style={{ width: 64, height: 64, objectFit: 'cover', borderRadius: 6, cursor: 'pointer',
                        border: activeImg === i ? '2px solid #3182ce' : '2px solid #e2e8f0' }} />
                  ))}
                </div>
              )}
            </div>
          )}

          {/* Specifications */}
          {(product.specifications ?? []).length > 0 && (
            <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem', marginBottom: '1rem' }}>
              <div style={{ fontWeight: 600, marginBottom: '0.75rem' }}>Technical Specifications</div>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <tbody>
                  {(product.specifications ?? []).map((spec, i) => (
                    <tr key={i} style={{ borderBottom: '1px solid #f5f5f5' }}>
                      <td style={{ padding: '0.5rem 0', color: '#888', width: '35%' }}>{spec.attributeName}</td>
                      <td style={{ padding: '0.5rem 0' }}>
                        <div style={{ display: 'flex', gap: '0.3rem', flexWrap: 'wrap' }}>
                          {spec.values.map(v => (
                            <span key={v} style={{ background: '#f0f0f0', borderRadius: 4, padding: '0.1rem 0.4rem', fontSize: 12 }}>{v}</span>
                          ))}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Variants */}
          {(product.variants ?? []).length > 0 && (
            <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem', marginBottom: '1rem' }}>
              <div style={{ fontWeight: 600, marginBottom: '0.75rem' }}>Product Variants</div>
              <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
                <thead>
                  <tr style={{ background: '#f9f9f9', color: '#888' }}>
                    <th style={{ padding: '0.5rem', textAlign: 'left', fontWeight: 500 }}>SKU</th>
                    <th style={{ padding: '0.5rem', textAlign: 'left', fontWeight: 500 }}>Variant</th>
                    <th style={{ padding: '0.5rem', textAlign: 'right', fontWeight: 500 }}>Price</th>
                    <th style={{ padding: '0.5rem', textAlign: 'right', fontWeight: 500 }}>Stock</th>
                  </tr>
                </thead>
                <tbody>
                  {(product.variants ?? []).map((v, i) => (
                    <tr key={i} style={{ borderBottom: '1px solid #f5f5f5' }}>
                      <td style={{ padding: '0.5rem', fontFamily: 'monospace', fontSize: 12 }}>{v.sku}</td>
                      <td style={{ padding: '0.5rem' }}>{v.variantName}</td>
                      <td style={{ padding: '0.5rem', textAlign: 'right' }}>{v.price?.toLocaleString()} ₫</td>
                      <td style={{ padding: '0.5rem', textAlign: 'right' }}>
                        <span style={{ color: v.stock === 0 ? '#e53e3e' : v.stock < 10 ? '#ed8936' : '#38a169', fontWeight: 600 }}>{v.stock}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Description */}
          {product.description && (
            <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
              <div style={{ fontWeight: 600, marginBottom: '0.5rem' }}>Description</div>
              <div style={{ fontSize: 13, color: '#555', lineHeight: 1.7, maxHeight: 300, overflowY: 'auto' }}>{product.description}</div>
            </div>
          )}
        </div>

        {/* Right column */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {/* Commercial insights */}
          <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Commercial Insights</div>
            {([
              ['Base Price', `${product.basePrice?.toLocaleString()} ₫`],
              ['Total Stock', String(product.stockQuantity ?? 0)],
              ...(product.platformFeePercent != null ? [['Platform Fee', `${product.platformFeePercent}%`]] : []),
            ] as [string, string][]).map(([k, v]) => (
              <div key={k} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.4rem', fontSize: 13 }}>
                <span style={{ color: '#888' }}>{k}</span>
                <span style={{ fontWeight: 600 }}>{v}</span>
              </div>
            ))}
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', marginTop: '0.5rem', paddingTop: '0.5rem', borderTop: '1px solid #f0f0f0' }}>
              <span style={{ width: 8, height: 8, borderRadius: '50%', background: product.stockQuantity > 0 ? '#38a169' : '#e53e3e', display: 'inline-block' }} />
              <span style={{ fontSize: 12, color: product.stockQuantity > 0 ? '#38a169' : '#e53e3e', fontWeight: 600 }}>
                {product.stockQuantity > 0 ? 'In Stock' : 'Out of Stock'}
              </span>
            </div>
          </div>

          {/* Product meta */}
          <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Details</div>
            {([
              ['Created', new Date(product.createdAt).toLocaleDateString()],
              ...(product.updatedAt ? [['Updated', new Date(product.updatedAt).toLocaleDateString()]] : []),
            ] as [string, string][]).map(([k, v]) => (
              <div key={k} style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.35rem', fontSize: 13 }}>
                <span style={{ color: '#888' }}>{k}</span>
                <span>{v}</span>
              </div>
            ))}
          </div>

          {/* Vendor info */}
          <div style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontWeight: 600, marginBottom: '0.75rem', color: '#555' }}>Vendor</div>
            <div style={{ fontWeight: 600, marginBottom: '0.25rem' }}>{product.storeName}</div>
            {product.storeId && (
              <Link to={`/admin/stores/${product.storeId}`} style={{ fontSize: 13, color: '#3182ce', textDecoration: 'none' }}>
                View Store Profile →
              </Link>
            )}
          </div>
        </div>
      </div>

      {/* Sticky bottom action bar */}
      <div style={{ position: 'fixed', bottom: 0, left: 0, right: 0, background: '#fff', borderTop: '1px solid #e2e8f0', padding: '0.75rem 2rem', display: 'flex', justifyContent: 'flex-end', gap: '0.75rem', zIndex: 100 }}>
        <button onClick={() => setModal('delete')}
          style={{ padding: '0.5rem 1rem', background: '#fff', color: '#e53e3e', border: '1px solid #e53e3e', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          Delete
        </button>
        {product.status === 'Active' && (
          <button onClick={() => setModal('suspend')}
            style={{ padding: '0.5rem 1rem', background: '#ed8936', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            Suspend
          </button>
        )}
        {product.status !== 'Rejected' && (
          <button onClick={() => setModal('reject')}
            style={{ padding: '0.5rem 1rem', background: '#e53e3e', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            Reject
          </button>
        )}
        {isPending && (
          <button onClick={handleApprove} disabled={busy}
            style={{ padding: '0.5rem 1.25rem', background: '#38a169', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
            {busy ? '…' : 'Approve'}
          </button>
        )}
        {!isPending && product.status !== 'Active' && (
          <button disabled style={{ padding: '0.5rem 1.25rem', background: '#c6f6d5', color: '#276749', border: 'none', borderRadius: 4, fontWeight: 600, cursor: 'default', opacity: 0.7 }}>
            Already Approved
          </button>
        )}
      </div>
    </div>
  );
}
