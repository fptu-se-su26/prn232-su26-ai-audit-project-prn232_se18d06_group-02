import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { catalogApi } from '../api/catalog';
import { useAuth } from '../contexts/AuthContext';
import ProductCard, { ProductCardData } from '../components/ProductCard';

interface StoreProfile {
  slug: string;
  name: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  followerCount?: number;
  productCount?: number;
  totalSold?: number;
  rating?: number;
  reviewCount?: number;
  isFollowing?: boolean;
  isVerified?: boolean;
}

export default function StoreProfilePage() {
  const { slug } = useParams<{ slug: string }>();
  const { user } = useAuth();
  const [store, setStore] = useState<StoreProfile | null>(null);
  const [products, setProducts] = useState<ProductCardData[]>([]);
  const [loading, setLoading] = useState(true);
  const [following, setFollowing] = useState(false);
  const [followLoading, setFollowLoading] = useState(false);

  useEffect(() => {
    if (!slug) return;
    setLoading(true);
    Promise.all([catalogApi.storeProfile(slug), catalogApi.storeProducts(slug)]).then(([s, p]) => {
      const storeData = s as StoreProfile;
      setStore(storeData);
      setFollowing(storeData.isFollowing ?? false);
      setProducts((p as { items?: ProductCardData[] }).items ?? (p as ProductCardData[]) ?? []);
    }).finally(() => setLoading(false));
  }, [slug]);

  const handleFollow = async () => {
    if (!slug) return;
    setFollowLoading(true);
    try {
      await catalogApi.followStore(slug);
      setFollowing(f => !f);
    } catch { } finally { setFollowLoading(false); }
  };

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  );
  if (!store) return (
    <div className="text-center py-24 text-gray-500">
      <span className="material-symbols-outlined text-6xl text-gray-200 mb-4 block">store</span>
      <p className="font-medium">Store not found.</p>
    </div>
  );

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Store header */}
      <div className="bg-white rounded-2xl border border-gray-200 overflow-hidden mb-8">
        {/* Banner */}
        <div className="h-40 md:h-56 w-full overflow-hidden bg-gradient-to-r from-blue-600 to-blue-800">
          {store.bannerUrl && <img src={store.bannerUrl} alt="Store banner" className="w-full h-full object-cover" />}
        </div>

        {/* Store info row */}
        <div className="px-6 pb-6">
          <div className="flex flex-col sm:flex-row sm:items-end gap-4 -mt-10 sm:-mt-12">
            {/* Logo */}
            <div className="w-20 h-20 sm:w-24 sm:h-24 rounded-2xl border-4 border-white bg-white shadow-md overflow-hidden flex items-center justify-center flex-shrink-0">
              {store.logoUrl
                ? <img src={store.logoUrl} alt={store.name} className="w-full h-full object-cover" />
                : <span className="text-3xl font-black text-primary">{store.name[0]}</span>
              }
            </div>

            <div className="flex-1">
              <div className="flex flex-wrap items-center gap-2 mb-1">
                <h1 className="text-2xl font-bold text-gray-900">{store.name}</h1>
                {store.isVerified && (
                  <span className="inline-flex items-center gap-1 bg-blue-50 text-primary text-xs font-bold px-2.5 py-1 rounded-full">
                    <span className="material-symbols-outlined text-[14px]">verified</span>
                    Verified
                  </span>
                )}
              </div>
              {store.description && <p className="text-sm text-gray-500 leading-relaxed max-w-2xl">{store.description}</p>}
            </div>

            {user && (
              <button onClick={handleFollow} disabled={followLoading}
                className={`flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold text-sm transition-all ${following ? 'bg-gray-100 text-gray-700 hover:bg-gray-200' : 'bg-primary text-white hover:bg-blue-700 shadow-[0_8px_20px_-6px_rgba(26,87,219,0.4)]'}`}>
                <span className="material-symbols-outlined text-[18px]">{following ? 'person_remove' : 'person_add'}</span>
                {followLoading ? '…' : following ? 'Unfollow' : 'Follow'}
              </button>
            )}
          </div>

          {/* Stats */}
          <div className="flex flex-wrap gap-6 mt-5 pt-5 border-t border-gray-100 text-sm">
            {[
              ['group', `${(store.followerCount ?? 0).toLocaleString()} followers`],
              ['inventory_2', `${(store.productCount ?? products.length).toLocaleString()} products`],
              ...(store.totalSold != null ? [['shopping_bag', `${store.totalSold.toLocaleString()} sold`]] : []),
            ].map(([icon, label]) => (
              <div key={label} className="flex items-center gap-2 text-gray-600">
                <span className="material-symbols-outlined text-[18px] text-primary">{icon}</span>
                {label}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Products */}
      <h2 className="text-xl font-bold text-gray-900 mb-5">
        Products <span className="text-base text-gray-400 font-normal">({products.length})</span>
      </h2>
      {products.length > 0 ? (
        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
          {products.map(p => <ProductCard key={p.slug} product={p} />)}
        </div>
      ) : (
        <div className="bg-white rounded-2xl border border-dashed border-gray-300 p-16 text-center text-gray-400">
          <span className="material-symbols-outlined text-5xl mb-3 block">inventory_2</span>
          <p>No products listed yet.</p>
        </div>
      )}
    </div>
  );
}
