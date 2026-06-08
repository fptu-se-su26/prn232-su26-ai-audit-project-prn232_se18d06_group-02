import { useEffect, useState } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { catalogApi } from '../api/catalog';
import { cartApi } from '../api/cart';
import { useAuth } from '../contexts/AuthContext';
import ProductCard, { ProductCardData } from '../components/ProductCard';

interface ProductDetail {
  slug: string;
  name: string;
  basePrice: number;
  originalPrice?: number;
  imageUrl?: string;
  imageUrls?: string[];
  brandName: string;
  storeName: string;
  storeSlug: string;
  storeLogoUrl?: string;
  description?: string;
  specifications?: Record<string, string>;
  attributes?: Array<{ name: string; values: string[] }>;
  reviews?: Array<{ id: string; rating: number; comment: string; reviewerName: string; createdAt: string }>;
  relatedProducts?: ProductCardData[];
  averageRating?: number;
  reviewCount?: number;
  isInStock?: boolean;
  defaultVariantId?: string;
  saleBadges?: string[];
}

function StarRating({ value, max = 5, size = 20 }: { value: number; max?: number; size?: number }) {
  return (
    <div className="flex items-center gap-0.5">
      {Array.from({ length: max }, (_, i) => (
        <span key={i} className={`material-symbols-outlined text-amber-400`}
          style={{ fontSize: size, fontVariationSettings: i < Math.round(value) ? "'FILL' 1" : "'FILL' 0" }}>
          star
        </span>
      ))}
    </div>
  );
}

export default function ProductDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const { user } = useAuth();
  const navigate = useNavigate();
  const [product, setProduct] = useState<ProductDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [addingToCart, setAddingToCart] = useState(false);
  const [cartMsg, setCartMsg] = useState('');
  const [activeImage, setActiveImage] = useState(0);
  const [activeTab, setActiveTab] = useState<'desc' | 'specs' | 'reviews'>('desc');

  useEffect(() => {
    if (!slug) return;
    setLoading(true);
    setActiveImage(0);
    catalogApi.getProduct(slug)
      .then(d => setProduct(d as ProductDetail))
      .finally(() => setLoading(false));
  }, [slug]);

  const handleAddToCart = async () => {
    if (!user) { navigate('/login'); return; }
    if (!product) return;
    setAddingToCart(true);
    try {
      await cartApi.add(product.slug, quantity);
      setCartMsg('Added to cart!');
      setTimeout(() => setCartMsg(''), 3000);
    } catch (e: unknown) {
      setCartMsg(e instanceof Error ? e.message : 'Failed to add to cart.');
    } finally { setAddingToCart(false); }
  };

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  );
  if (!product) return (
    <div className="flex flex-col items-center justify-center py-24 text-gray-500">
      <span className="material-symbols-outlined text-6xl text-gray-300 mb-4">search_off</span>
      <p className="text-lg font-semibold">Product not found.</p>
      <Link to="/products" className="mt-4 text-primary font-semibold hover:underline">Back to products</Link>
    </div>
  );

  const images = product.imageUrls?.length ? product.imageUrls : product.imageUrl ? [product.imageUrl] : [];

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-2 text-sm text-gray-500 mb-6">
        <Link to="/" className="hover:text-primary transition-colors">Home</Link>
        <span className="material-symbols-outlined text-[16px]">chevron_right</span>
        <Link to="/products" className="hover:text-primary transition-colors">Products</Link>
        <span className="material-symbols-outlined text-[16px]">chevron_right</span>
        <span className="text-gray-900 font-medium truncate max-w-[200px]">{product.name}</span>
      </nav>

      {/* Main product area */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-10 mb-12">
        {/* Image gallery */}
        <div className="flex flex-col gap-3">
          <div className="relative bg-white rounded-2xl border border-gray-200 overflow-hidden aspect-square flex items-center justify-center p-8">
            {product.saleBadges?.[0] && (
              <span className="absolute top-4 left-4 bg-red-600 text-white text-xs font-bold px-2.5 py-1 rounded-md z-10">
                {product.saleBadges[0]}
              </span>
            )}
            {images[activeImage] ? (
              <img src={images[activeImage]} alt={product.name}
                className="max-w-full max-h-full object-contain mix-blend-multiply" />
            ) : (
              <span className="material-symbols-outlined text-8xl text-gray-200">image</span>
            )}
          </div>
          {images.length > 1 && (
            <div className="flex gap-2 overflow-x-auto pb-1">
              {images.map((url, i) => (
                <button key={i} onClick={() => setActiveImage(i)}
                  className={`flex-shrink-0 w-16 h-16 rounded-lg border-2 overflow-hidden bg-white p-1 transition-colors ${i === activeImage ? 'border-primary' : 'border-gray-200 hover:border-gray-300'}`}>
                  <img src={url} alt="" className="w-full h-full object-contain mix-blend-multiply" />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Product info */}
        <div className="flex flex-col">
          {product.brandName && (
            <span className="text-xs font-bold uppercase tracking-wider text-gray-500 border border-gray-200 px-2 py-0.5 rounded w-fit mb-3">{product.brandName}</span>
          )}
          <h1 className="text-2xl md:text-3xl font-bold text-gray-900 leading-snug mb-3">{product.name}</h1>

          {product.averageRating !== undefined && product.reviewCount !== undefined && (
            <div className="flex items-center gap-3 mb-4">
              <StarRating value={product.averageRating} size={18} />
              <span className="text-sm font-semibold text-gray-600">{product.averageRating.toFixed(1)}</span>
              <span className="text-sm text-gray-400">({product.reviewCount} reviews)</span>
            </div>
          )}

          <div className="flex items-baseline gap-3 mb-2">
            <span className="text-3xl font-extrabold text-primary">{product.basePrice.toLocaleString('vi-VN')} ₫</span>
            {product.originalPrice && (
              <span className="text-lg text-gray-400 line-through">{product.originalPrice.toLocaleString('vi-VN')} ₫</span>
            )}
          </div>

          {/* Store */}
          <Link to={`/store/${product.storeSlug}`} className="flex items-center gap-2 mb-6 mt-2 w-fit">
            {product.storeLogoUrl
              ? <img src={product.storeLogoUrl} alt={product.storeName} className="w-6 h-6 rounded-full object-cover" />
              : <div className="w-6 h-6 rounded-full bg-blue-100 flex items-center justify-center text-primary font-bold text-xs">{product.storeName[0]}</div>
            }
            <span className="text-sm font-medium text-gray-600 hover:text-primary transition-colors">{product.storeName}</span>
          </Link>

          {/* Quantity + Add to cart */}
          <div className="flex items-center gap-3 mb-4">
            <div className="flex items-center border border-gray-300 rounded-xl overflow-hidden">
              <button onClick={() => setQuantity(q => Math.max(1, q - 1))} className="px-4 py-3 hover:bg-gray-50 text-gray-600 font-bold text-lg transition-colors">−</button>
              <span className="px-4 py-3 font-semibold min-w-[3rem] text-center">{quantity}</span>
              <button onClick={() => setQuantity(q => q + 1)} className="px-4 py-3 hover:bg-gray-50 text-gray-600 font-bold text-lg transition-colors">+</button>
            </div>
            <button onClick={handleAddToCart} disabled={addingToCart || product.isInStock === false}
              className="flex-1 flex items-center justify-center gap-2 bg-secondary hover:bg-orange-600 disabled:bg-gray-300 text-white font-bold py-3 px-6 rounded-xl transition-all shadow-[0_8px_20px_-6px_rgba(255,107,0,0.5)] disabled:shadow-none">
              <span className="material-symbols-outlined text-[20px]">shopping_cart</span>
              {addingToCart ? 'Adding…' : product.isInStock === false ? 'Out of Stock' : 'Add to Cart'}
            </button>
          </div>

          {cartMsg && (
            <div className={`flex items-center gap-2 px-4 py-3 rounded-xl text-sm font-medium ${cartMsg.includes('Failed') ? 'bg-red-50 text-red-600' : 'bg-green-50 text-green-700'}`}>
              <span className="material-symbols-outlined text-[18px]">{cartMsg.includes('Failed') ? 'error' : 'check_circle'}</span>
              {cartMsg}
            </div>
          )}

          {/* Trust badges */}
          <div className="flex flex-wrap gap-4 mt-6 pt-6 border-t border-gray-100 text-sm text-gray-500">
            {[['local_shipping', 'Free Shipping over 500K'], ['verified', 'Verified Seller'], ['replay', 'Easy Returns']].map(([icon, label]) => (
              <div key={label} className="flex items-center gap-2">
                <span className="material-symbols-outlined text-[18px] text-primary">{icon}</span>
                {label}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white rounded-2xl border border-gray-200 shadow-sm overflow-hidden mb-10">
        <div className="flex border-b border-gray-200">
          {([['desc', 'Description'], ['specs', 'Specifications'], ['reviews', `Reviews (${product.reviewCount ?? 0})`]] as const).map(([key, label]) => (
            <button key={key} onClick={() => setActiveTab(key)}
              className={`px-6 py-4 text-sm font-semibold transition-colors border-b-2 -mb-px ${activeTab === key ? 'border-primary text-primary' : 'border-transparent text-gray-500 hover:text-gray-900'}`}>
              {label}
            </button>
          ))}
        </div>
        <div className="p-6">
          {activeTab === 'desc' && (
            <p className="text-gray-600 leading-relaxed text-sm">
              {product.description ?? 'No description available.'}
            </p>
          )}
          {activeTab === 'specs' && (
            product.specifications && Object.keys(product.specifications).length > 0 ? (
              <table className="w-full text-sm">
                <tbody>
                  {Object.entries(product.specifications).map(([k, v], i) => (
                    <tr key={k} className={i % 2 === 0 ? 'bg-gray-50' : 'bg-white'}>
                      <td className="py-3 px-4 font-semibold text-gray-700 w-2/5">{k}</td>
                      <td className="py-3 px-4 text-gray-600">{v}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : <p className="text-gray-400 text-sm">No specifications available.</p>
          )}
          {activeTab === 'reviews' && (
            product.reviews && product.reviews.length > 0 ? (
              <div className="space-y-5">
                {product.reviews.map(r => (
                  <div key={r.id} className="flex gap-4 pb-5 border-b border-gray-100 last:border-0">
                    <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center text-primary font-bold flex-shrink-0">
                      {(r.reviewerName || '?')[0].toUpperCase()}
                    </div>
                    <div className="flex-1">
                      <div className="flex items-center justify-between mb-1">
                        <span className="font-semibold text-sm text-gray-900">{r.reviewerName}</span>
                        <span className="text-xs text-gray-400">{new Date(r.createdAt).toLocaleDateString('vi-VN')}</span>
                      </div>
                      <StarRating value={r.rating} size={14} />
                      <p className="mt-2 text-sm text-gray-600 leading-relaxed">{r.comment}</p>
                    </div>
                  </div>
                ))}
              </div>
            ) : <p className="text-gray-400 text-sm">No reviews yet. Be the first to review!</p>
          )}
        </div>
      </div>

      {/* Related products */}
      {product.relatedProducts && product.relatedProducts.length > 0 && (
        <section>
          <h2 className="text-xl font-bold text-gray-900 mb-5">Related Products</h2>
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
            {product.relatedProducts.map(p => <ProductCard key={p.slug} product={p} />)}
          </div>
        </section>
      )}
    </div>
  );
}
