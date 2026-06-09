import { useEffect, useState, useCallback } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { catalogApi } from '../api/catalog';
import { useAuth } from '../contexts/AuthContext';
import ProductCard, { ProductCardData } from '../components/ProductCard';

interface StoreProfile {
  slug: string;
  name: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  province?: string;
  followerCount?: number;
  productCount?: number;
  totalSold?: number;
  rating?: number;
  reviewCount?: number;
  isFollowing?: boolean;
  isVerified?: boolean;
  createdAt?: string;
}

interface Category { name: string; slug: string; subCategories?: { name: string; slug: string }[]; }

interface PagedProducts {
  items: ProductCardData[];
  totalCount: number;
  page?: number;
  pageSize?: number;
}

const SORT_TABS = [
  { key: 'popular', label: 'Popular', icon: 'trending_up' },
  { key: 'newest', label: 'Newest', icon: 'schedule' },
  { key: 'best_selling', label: 'Best Selling', icon: 'local_fire_department' },
  { key: 'price_asc', label: 'Price ↑', icon: '' },
  { key: 'price_desc', label: 'Price ↓', icon: '' },
];

const PAGE_SIZE = 20;

function storeAge(createdAt?: string): string {
  if (!createdAt) return '—';
  const diff = Date.now() - new Date(createdAt).getTime();
  const days = Math.floor(diff / 86400000);
  if (days >= 365) return `${Math.floor(days / 365)} yr${Math.floor(days / 365) > 1 ? 's' : ''}`;
  if (days >= 30) return `${Math.floor(days / 30)} mo`;
  return `${days} days`;
}

export default function StoreProfilePage() {
  const { slug } = useParams<{ slug: string }>();
  const { user } = useAuth();
  const [searchParams, setSearchParams] = useSearchParams();

  const [store, setStore] = useState<StoreProfile | null>(null);
  const [products, setProducts] = useState<ProductCardData[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loadingStore, setLoadingStore] = useState(true);
  const [loadingProducts, setLoadingProducts] = useState(false);
  const [following, setFollowing] = useState(false);
  const [followLoading, setFollowLoading] = useState(false);
  const [minPriceInput, setMinPriceInput] = useState(searchParams.get('minPrice') ?? '');
  const [maxPriceInput, setMaxPriceInput] = useState(searchParams.get('maxPrice') ?? '');

  const sortBy = searchParams.get('sort') ?? 'popular';
  const categorySlug = searchParams.get('categorySlug') ?? '';
  const minPrice = searchParams.get('minPrice') ? Number(searchParams.get('minPrice')) : undefined;
  const maxPrice = searchParams.get('maxPrice') ? Number(searchParams.get('maxPrice')) : undefined;
  const page = Number(searchParams.get('page') ?? '1');
  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  useEffect(() => {
    if (!slug) return;
    setLoadingStore(true);
    catalogApi.storeProfile(slug).then(s => {
      const storeData = s as StoreProfile;
      setStore(storeData);
      setFollowing(storeData.isFollowing ?? false);
    }).finally(() => setLoadingStore(false));

    catalogApi.categories()
      .then(d => setCategories((d as Category[]) ?? []))
      .catch(() => {});
  }, [slug]);

  const fetchProducts = useCallback(() => {
    if (!slug) return;
    setLoadingProducts(true);
    catalogApi.storeProducts(slug, {
      sort: sortBy,
      categorySlug: categorySlug || undefined,
      minPrice,
      maxPrice,
      page,
      pageSize: PAGE_SIZE,
    }).then(res => {
      const paged = res as PagedProducts;
      if (paged.items) {
        setProducts(paged.items);
        setTotalCount(paged.totalCount ?? paged.items.length);
      } else {
        setProducts((res as ProductCardData[]) ?? []);
        setTotalCount(((res as ProductCardData[]) ?? []).length);
      }
    }).catch(() => {}).finally(() => setLoadingProducts(false));
  }, [slug, sortBy, categorySlug, minPrice, maxPrice, page]);

  useEffect(() => { fetchProducts(); }, [fetchProducts]);

  const setParam = (key: string, value: string | undefined) => {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(key, value); else next.delete(key);
    if (key !== 'page') next.delete('page');
    setSearchParams(next, { replace: true });
  };

  const applyPriceFilter = () => {
    const next = new URLSearchParams(searchParams);
    if (minPriceInput) next.set('minPrice', minPriceInput); else next.delete('minPrice');
    if (maxPriceInput) next.set('maxPrice', maxPriceInput); else next.delete('maxPrice');
    next.delete('page');
    setSearchParams(next, { replace: true });
  };

  const clearFilters = () => {
    setMinPriceInput('');
    setMaxPriceInput('');
    setSearchParams({ sort: sortBy }, { replace: true });
  };

  const handleFollow = async () => {
    if (!slug) return;
    setFollowLoading(true);
    try {
      await catalogApi.followStore(slug);
      setFollowing(f => !f);
    } catch { } finally { setFollowLoading(false); }
  };

  const handleChatClick = () => {
    window.dispatchEvent(new CustomEvent('gearzone:open-chat', { detail: { storeSlug: slug } }));
  };

  if (loadingStore) return (
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

  const hasActiveFilters = !!categorySlug || !!minPrice || !!maxPrice;

  return (
    <div>
      {/* Store Header Banner */}
      <section className="text-white" style={{ background: 'linear-gradient(to right, #0f1729, #1a2744)' }}>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
          <div className="flex flex-col md:flex-row items-start md:items-center gap-6">
            {/* Logo */}
            <div className="w-20 h-20 md:w-24 md:h-24 rounded-full border-4 border-white/20 overflow-hidden bg-white flex-shrink-0 shadow-lg">
              {store.logoUrl
                ? <img src={store.logoUrl} alt={store.name} className="w-full h-full object-cover" />
                : <div className="w-full h-full flex items-center justify-center bg-primary/10">
                  <span className="material-symbols-outlined text-4xl text-primary">storefront</span>
                </div>
              }
            </div>

            {/* Info */}
            <div className="flex-1 min-w-0">
              <div className="flex flex-wrap items-center gap-3 mb-2">
                <h1 className="text-2xl md:text-3xl font-bold">{store.name}</h1>
                {store.isVerified && (
                  <span className="bg-green-500/20 text-green-400 text-xs font-semibold px-2.5 py-1 rounded-full flex items-center gap-1">
                    <span className="w-2 h-2 bg-green-400 rounded-full" />
                    Verified
                  </span>
                )}
              </div>
              {store.province && (
                <p className="text-gray-300 text-sm flex items-center gap-1 mb-2">
                  <span className="material-symbols-outlined text-[16px]">location_on</span>
                  {store.province}
                </p>
              )}
              {store.description && (
                <p className="text-gray-400 text-sm max-w-2xl line-clamp-2">{store.description}</p>
              )}
            </div>

            {/* Actions */}
            <div className="flex items-center gap-3 flex-shrink-0">
              <button onClick={handleFollow} disabled={followLoading}
                className={`flex items-center gap-2 px-5 py-2.5 rounded-lg text-sm font-semibold transition-all shadow-sm ${following
                  ? 'bg-white/10 border border-white/20 text-white hover:bg-red-500/20 hover:border-red-400 hover:text-red-300'
                  : 'bg-secondary hover:bg-orange-600 text-white'}`}>
                <span className="material-symbols-outlined text-[18px]"
                  style={{ fontVariationSettings: `'FILL' ${following ? 1 : 0}` }}>
                  {following ? 'favorite' : 'add'}
                </span>
                {followLoading ? '…' : following ? 'Following' : 'Follow'}
                <span className="bg-white/10 px-2 py-0.5 rounded-full text-xs">{store.followerCount ?? 0}</span>
              </button>
              {user && (
                <button onClick={handleChatClick}
                  className="flex items-center gap-2 px-5 py-2.5 bg-white/10 hover:bg-white/20 text-white rounded-lg text-sm font-semibold transition-colors border border-white/20">
                  <span className="material-symbols-outlined text-[18px]">chat</span>
                  Chat
                </button>
              )}
            </div>
          </div>

          {/* Stats */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mt-6 pt-6 border-t border-white/10">
            <div className="text-center md:text-left">
              <p className="text-2xl font-bold text-white">{(store.productCount ?? products.length).toLocaleString()}</p>
              <p className="text-xs text-gray-400 uppercase tracking-wider mt-1">Products</p>
            </div>
            <div className="text-center md:text-left">
              <p className="text-2xl font-bold text-white">{(store.totalSold ?? 0).toLocaleString()}</p>
              <p className="text-xs text-gray-400 uppercase tracking-wider mt-1">Total Sold</p>
            </div>
            <div className="text-center md:text-left">
              {(store.reviewCount ?? 0) > 0 ? (
                <>
                  <div className="flex items-center justify-center md:justify-start gap-1">
                    <span className="material-symbols-outlined text-amber-400 text-xl" style={{ fontVariationSettings: "'FILL' 1" }}>star</span>
                    <p className="text-2xl font-bold text-white">{(store.rating ?? 0).toFixed(1)}</p>
                    <span className="text-gray-400 text-sm">/5</span>
                  </div>
                  <p className="text-xs text-gray-400 uppercase tracking-wider mt-1">{(store.reviewCount ?? 0).toLocaleString()} Reviews</p>
                </>
              ) : (
                <>
                  <div className="flex items-center justify-center md:justify-start gap-1">
                    <span className="material-symbols-outlined text-gray-500 text-xl">star</span>
                    <p className="text-lg font-bold text-gray-400">—</p>
                  </div>
                  <p className="text-xs text-gray-400 uppercase tracking-wider mt-1">No Reviews Yet</p>
                </>
              )}
            </div>
            <div className="text-center md:text-left">
              <p className="text-2xl font-bold text-white">{storeAge(store.createdAt)}</p>
              <p className="text-xs text-gray-400 uppercase tracking-wider mt-1">Joined</p>
            </div>
          </div>
        </div>
      </section>

      {/* Sort Tabs */}
      <section className="bg-white border-b border-gray-200 sticky top-[80px] z-30">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-1 h-12 overflow-x-auto" style={{ scrollbarWidth: 'none' }}>
            {SORT_TABS.map(tab => (
              <button key={tab.key} onClick={() => setParam('sort', tab.key)}
                className={`px-4 py-2 text-sm font-medium rounded-lg whitespace-nowrap transition-colors flex items-center gap-1 ${sortBy === tab.key ? 'bg-primary text-white' : 'text-gray-600 hover:bg-gray-100'}`}>
                {tab.icon && <span className="material-symbols-outlined text-[16px]">{tab.icon}</span>}
                {tab.label}
              </button>
            ))}
            <div className="ml-auto text-sm text-gray-500 whitespace-nowrap flex-shrink-0">
              <span className="font-semibold text-gray-700">{totalCount.toLocaleString()}</span> products
            </div>
          </div>
        </div>
      </section>

      {/* Main: sidebar + grid */}
      <section className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        <div className="flex gap-6">
          {/* Sidebar */}
          <aside className="hidden lg:block w-60 flex-shrink-0">
            <div className="sticky top-[140px] space-y-4">
              {/* Categories */}
              {categories.length > 0 && (
                <div className="bg-white rounded-xl border border-gray-200 p-4">
                  <h3 className="font-bold text-sm text-gray-900 mb-3 flex items-center gap-2">
                    <span className="material-symbols-outlined text-[18px] text-primary">category</span>
                    Categories
                  </h3>
                  <ul className="space-y-1">
                    <li>
                      <button onClick={() => setParam('categorySlug', undefined)}
                        className={`block w-full text-left px-3 py-2 text-sm rounded-lg transition-colors ${!categorySlug ? 'bg-primary/10 text-primary font-semibold' : 'text-gray-600 hover:bg-gray-50'}`}>
                        All Products
                      </button>
                    </li>
                    {categories.map(cat => (
                      <li key={cat.slug}>
                        <button onClick={() => setParam('categorySlug', cat.slug)}
                          className={`block w-full text-left px-3 py-2 text-sm rounded-lg transition-colors ${categorySlug === cat.slug ? 'bg-primary/10 text-primary font-semibold' : 'text-gray-600 hover:bg-gray-50'}`}>
                          {cat.name}
                        </button>
                        {cat.subCategories && cat.subCategories.length > 0 && (
                          <ul className="ml-4 mt-1 space-y-0.5">
                            {cat.subCategories.map(sub => (
                              <li key={sub.slug}>
                                <button onClick={() => setParam('categorySlug', sub.slug)}
                                  className={`block w-full text-left px-3 py-1.5 text-xs rounded-lg transition-colors ${categorySlug === sub.slug ? 'bg-primary/10 text-primary font-semibold' : 'text-gray-500 hover:bg-gray-50'}`}>
                                  {sub.name}
                                </button>
                              </li>
                            ))}
                          </ul>
                        )}
                      </li>
                    ))}
                  </ul>
                </div>
              )}

              {/* Price Range */}
              <div className="bg-white rounded-xl border border-gray-200 p-4">
                <h3 className="font-bold text-sm text-gray-900 mb-3 flex items-center gap-2">
                  <span className="material-symbols-outlined text-[18px] text-primary">payments</span>
                  Price Range
                </h3>
                <div className="flex items-center gap-2 mb-3">
                  <input type="number" placeholder="From" value={minPriceInput}
                    onChange={e => setMinPriceInput(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary" />
                  <span className="text-gray-400 flex-shrink-0">—</span>
                  <input type="number" placeholder="To" value={maxPriceInput}
                    onChange={e => setMaxPriceInput(e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-1 focus:ring-primary focus:border-primary" />
                </div>
                <button onClick={applyPriceFilter}
                  className="w-full bg-primary/10 hover:bg-primary text-primary hover:text-white font-semibold py-2 rounded-lg text-sm transition-all">
                  Apply
                </button>
              </div>

              {hasActiveFilters && (
                <button onClick={clearFilters}
                  className="flex items-center justify-center gap-2 text-sm text-red-500 hover:text-red-600 font-medium py-2 w-full">
                  <span className="material-symbols-outlined text-[16px]">filter_alt_off</span>
                  Clear All Filters
                </button>
              )}
            </div>
          </aside>

          {/* Product Grid */}
          <div className="flex-1 min-w-0">
            {loadingProducts ? (
              <div className="flex items-center justify-center py-20">
                <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
              </div>
            ) : products.length > 0 ? (
              <>
                <div className="grid grid-cols-2 sm:grid-cols-3 xl:grid-cols-4 gap-4">
                  {products.map(p => <ProductCard key={p.slug} product={p} />)}
                </div>

                {/* Pagination */}
                {totalPages > 1 && (
                  <nav className="flex items-center justify-center gap-2 mt-8">
                    {page > 1 && (
                      <button onClick={() => setParam('page', String(page - 1))}
                        className="flex items-center justify-center w-10 h-10 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-100 transition-colors">
                        <span className="material-symbols-outlined text-[20px]">chevron_left</span>
                      </button>
                    )}
                    {Array.from({ length: totalPages }, (_, i) => i + 1)
                      .filter(n => n <= 3 || n >= totalPages - 1 || Math.abs(n - page) <= 1)
                      .reduce<(number | '...')[]>((acc, n, i, arr) => {
                        if (i > 0 && typeof arr[i - 1] === 'number' && (n as number) - (arr[i - 1] as number) > 1) acc.push('...');
                        acc.push(n);
                        return acc;
                      }, [])
                      .map((n, i) => n === '...'
                        ? <span key={`dot-${i}`} className="text-gray-400 px-1">…</span>
                        : (
                          <button key={n} onClick={() => setParam('page', String(n))}
                            className={`flex items-center justify-center w-10 h-10 rounded-lg text-sm font-medium transition-colors ${n === page ? 'bg-primary text-white shadow-sm' : 'border border-gray-300 text-gray-600 hover:bg-gray-100'}`}>
                            {n}
                          </button>
                        )
                      )}
                    {page < totalPages && (
                      <button onClick={() => setParam('page', String(page + 1))}
                        className="flex items-center justify-center w-10 h-10 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-100 transition-colors">
                        <span className="material-symbols-outlined text-[20px]">chevron_right</span>
                      </button>
                    )}
                  </nav>
                )}
              </>
            ) : (
              <div className="flex flex-col items-center justify-center py-20 text-center">
                <div className="w-24 h-24 bg-gray-100 rounded-full flex items-center justify-center mb-4">
                  <span className="material-symbols-outlined text-5xl text-gray-300">inventory_2</span>
                </div>
                <h3 className="text-lg font-semibold text-gray-700 mb-2">No Products Found</h3>
                <p className="text-sm text-gray-500 mb-4">No products match your current filters.</p>
                {hasActiveFilters && (
                  <button onClick={clearFilters}
                    className="text-primary hover:text-primary/80 text-sm font-medium flex items-center gap-1">
                    <span className="material-symbols-outlined text-[16px]">arrow_back</span>
                    View all products
                  </button>
                )}
              </div>
            )}
          </div>
        </div>
      </section>
    </div>
  );
}
