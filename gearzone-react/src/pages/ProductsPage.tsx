import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { catalogApi } from '../api/catalog';
import ProductCard, { ProductCardData } from '../components/ProductCard';

interface FilterOption { name: string; slug: string; productCount?: number; }
interface Sidebar {
  brands?: FilterOption[];
  priceRange?: { min: number; max: number };
}
interface PagedResult {
  items: ProductCardData[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

const SORT_OPTIONS = [
  { value: 'newest', label: 'Newest' },
  { value: 'price_asc', label: 'Price: Low to High' },
  { value: 'price_desc', label: 'Price: High to Low' },
  { value: 'popular', label: 'Most Popular' },
  { value: 'rating', label: 'Top Rated' },
];

export default function ProductsPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [result, setResult] = useState<PagedResult | null>(null);
  const [sidebar, setSidebar] = useState<Sidebar | null>(null);
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<'grid' | 'list'>('grid');

  const page = Number(searchParams.get('page') ?? '1');
  const search = searchParams.get('search') ?? '';
  const categorySlug = searchParams.get('categorySlug') ?? undefined;
  const sort = searchParams.get('sort') ?? 'newest';
  const brandSlugs = searchParams.getAll('brand');
  const minPrice = searchParams.get('minPrice') ?? '';
  const maxPrice = searchParams.get('maxPrice') ?? '';

  const updateParam = (key: string, value: string) => {
    const next = new URLSearchParams(searchParams);
    if (value) next.set(key, value); else next.delete(key);
    next.set('page', '1');
    setSearchParams(next);
  };

  const toggleBrand = (slug: string) => {
    const next = new URLSearchParams(searchParams);
    const current = next.getAll('brand');
    next.delete('brand');
    if (current.includes(slug)) {
      current.filter(b => b !== slug).forEach(b => next.append('brand', b));
    } else {
      [...current, slug].forEach(b => next.append('brand', b));
    }
    next.set('page', '1');
    setSearchParams(next);
  };

  const resetFilters = () => {
    const next = new URLSearchParams();
    if (search) next.set('search', search);
    if (categorySlug) next.set('categorySlug', categorySlug);
    setSearchParams(next);
  };

  useEffect(() => {
    setLoading(true);
    const params = { page, search, categorySlug, pageSize: 20, sort, brand: brandSlugs.length ? brandSlugs : undefined, minPrice: minPrice ? Number(minPrice) : undefined, maxPrice: maxPrice ? Number(maxPrice) : undefined };
    Promise.all([
      catalogApi.browseProducts(params),
      catalogApi.filters(categorySlug),
    ]).then(([prod, fil]) => {
      setResult(prod as PagedResult);
      setSidebar(fil as Sidebar);
    }).finally(() => setLoading(false));
  }, [searchParams.toString()]);

  const totalPages = result ? Math.ceil(result.totalCount / result.pageSize) : 1;
  const pageTitle = search ? `Results for "${search}"` : categorySlug ? categorySlug.replace(/-/g, ' ') : 'All Products';

  return (
    <main className="flex-1 flex justify-center py-6 px-4 md:px-8">
      <div className="w-full max-w-[1440px] flex flex-col gap-6">
        {/* Breadcrumb + Title */}
        <div className="flex flex-col gap-2">
          <nav className="flex items-center gap-2 text-sm text-gray-500">
            <Link to="/" className="hover:text-primary transition-colors">Home</Link>
            <span className="material-symbols-outlined text-[16px]">chevron_right</span>
            {search ? (
              <>
                <Link to="/products" className="hover:text-primary transition-colors">All Products</Link>
                <span className="material-symbols-outlined text-[16px]">chevron_right</span>
                <span className="text-gray-900 font-medium">Search: {search}</span>
              </>
            ) : (
              <span className="text-gray-900 font-medium capitalize">{pageTitle}</span>
            )}
          </nav>
          <div className="flex flex-wrap items-baseline justify-between gap-4">
            <h1 className="text-3xl font-bold tracking-tight text-gray-900 capitalize">
              {pageTitle}
              {result && <span className="text-lg font-normal text-gray-500 ml-2">({result.totalCount} products)</span>}
            </h1>
            <div className="flex items-center gap-3">
              <select value={sort} onChange={e => updateParam('sort', e.target.value)}
                className="text-sm border border-gray-300 rounded-lg px-3 py-2 bg-white focus:outline-none focus:ring-1 focus:ring-primary">
                {SORT_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
              <div className="flex rounded-lg border border-gray-200 overflow-hidden">
                <button onClick={() => setViewMode('grid')}
                  className={`p-2 transition-colors ${viewMode === 'grid' ? 'bg-primary text-white' : 'bg-white text-gray-500 hover:bg-gray-50'}`}>
                  <span className="material-symbols-outlined text-[20px]">grid_view</span>
                </button>
                <button onClick={() => setViewMode('list')}
                  className={`p-2 transition-colors ${viewMode === 'list' ? 'bg-primary text-white' : 'bg-white text-gray-500 hover:bg-gray-50'}`}>
                  <span className="material-symbols-outlined text-[20px]">view_list</span>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div className="flex flex-col lg:flex-row gap-8">
          {/* Sidebar */}
          <aside className="w-full lg:w-[260px] flex-shrink-0">
            <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-5 sticky top-24">
              <div className="flex items-center justify-between mb-6">
                <h3 className="font-bold text-lg">Filter by</h3>
                <button onClick={resetFilters} className="text-xs font-medium text-gray-500 hover:text-primary transition-colors">Reset</button>
              </div>

              {/* Brand filter */}
              {sidebar?.brands && sidebar.brands.length > 0 && (
                <div className="mb-5">
                  <h4 className="font-semibold text-sm mb-3">Brand</h4>
                  <div className="space-y-2.5">
                    {sidebar.brands.slice(0, 12).map(b => (
                      <label key={b.slug} className="flex items-center gap-3 cursor-pointer group">
                        <input type="checkbox" checked={brandSlugs.includes(b.slug)} onChange={() => toggleBrand(b.slug)}
                          className="w-4 h-4 rounded border-gray-300 text-primary focus:ring-primary/20 cursor-pointer" />
                        <span className="text-sm text-gray-600 group-hover:text-gray-900 transition-colors flex-1">{b.name}</span>
                        {b.productCount != null && <span className="text-xs text-gray-400">{b.productCount}</span>}
                      </label>
                    ))}
                  </div>
                </div>
              )}

              {/* Price filter */}
              <div className="mb-5">
                <h4 className="font-semibold text-sm mb-3">Price Range</h4>
                <div className="flex gap-2">
                  <input type="number" placeholder="Min" value={minPrice}
                    onChange={e => updateParam('minPrice', e.target.value)}
                    className="w-1/2 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary" />
                  <input type="number" placeholder="Max" value={maxPrice}
                    onChange={e => updateParam('maxPrice', e.target.value)}
                    className="w-1/2 border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary" />
                </div>
              </div>

              {/* In-stock filter */}
              <label className="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" checked={searchParams.get('inStock') === 'true'} onChange={e => updateParam('inStock', e.target.checked ? 'true' : '')}
                  className="w-4 h-4 rounded border-gray-300 text-primary cursor-pointer" />
                <span className="text-sm text-gray-600">In Stock Only</span>
              </label>
            </div>
          </aside>

          {/* Product grid */}
          <div className="flex-1">
            {loading ? (
              <div className="flex items-center justify-center py-24">
                <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
              </div>
            ) : result?.items.length === 0 ? (
              <div className="bg-white rounded-xl border border-dashed border-gray-300 p-16 text-center">
                <span className="material-symbols-outlined text-5xl text-gray-300 mb-4 block">search_off</span>
                <p className="text-gray-500 font-medium">No products found.</p>
                <button onClick={resetFilters} className="mt-4 text-primary text-sm font-semibold hover:underline">Clear filters</button>
              </div>
            ) : (
              <>
                <div className={viewMode === 'grid'
                  ? 'grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-4 gap-4'
                  : 'flex flex-col gap-4'}>
                  {result?.items.map(p => <ProductCard key={p.slug} product={p} />)}
                </div>

                {/* Pagination */}
                {totalPages > 1 && (
                  <div className="mt-8 flex items-center justify-center gap-2 flex-wrap">
                    <button
                      onClick={() => updateParam('page', String(page - 1))}
                      disabled={page <= 1}
                      className="p-2 rounded-lg border border-gray-200 text-gray-500 hover:text-primary hover:border-primary disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
                      <span className="material-symbols-outlined text-[20px]">chevron_left</span>
                    </button>
                    {Array.from({ length: Math.min(totalPages, 10) }, (_, i) => i + 1).map(n => (
                      <button key={n} onClick={() => updateParam('page', String(n))}
                        className={`w-10 h-10 rounded-lg text-sm font-semibold transition-colors ${n === page ? 'bg-primary text-white' : 'border border-gray-200 text-gray-700 hover:border-primary hover:text-primary'}`}>
                        {n}
                      </button>
                    ))}
                    {totalPages > 10 && <span className="text-gray-400">…</span>}
                    <button
                      onClick={() => updateParam('page', String(page + 1))}
                      disabled={page >= totalPages}
                      className="p-2 rounded-lg border border-gray-200 text-gray-500 hover:text-primary hover:border-primary disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
                      <span className="material-symbols-outlined text-[20px]">chevron_right</span>
                    </button>
                  </div>
                )}
              </>
            )}
          </div>
        </div>
      </div>
    </main>
  );
}
