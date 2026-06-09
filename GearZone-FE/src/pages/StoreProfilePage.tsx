import { useCallback, useEffect, useState } from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { followStore, getStoreProducts, getStoreProfile } from '@/api/catalog'
import { useAuth } from '@/contexts/useAuth'
import type { CatalogProduct, PagedResult, StoreProfile } from '@/types/catalog'

const SORT_OPTIONS = [
  { label: 'Popular', value: 'popular' },
  { label: 'Newest', value: 'newest' },
  { label: 'Best Selling', value: 'best-selling' },
  { label: 'Price: Low → High', value: 'price-asc' },
  { label: 'Price: High → Low', value: 'price-desc' },
]

function formatPrice(value: number) {
  return new Intl.NumberFormat('vi-VN').format(value)
}

function formatCompact(value: number) {
  if (value >= 1000) return `${(value / 1000).toFixed(1).replace(/\.0$/, '')}k`
  return String(value)
}

function ProductCard({ product }: { product: CatalogProduct }) {
  const image = product.imageUrl || 'https://placehold.co/400x400/f8fafc/94a3b8?text=GearZone'
  return (
    <article className="group flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md">
      <Link className="relative block overflow-hidden bg-slate-50 p-3" to={`/product/${product.slug}`}>
        <img alt={product.name} className="h-40 w-full object-contain mix-blend-multiply transition-transform duration-300 group-hover:scale-105" src={image} />
        {!product.isInStock && (
          <span className="absolute right-2 top-2 rounded bg-slate-900/70 px-1.5 py-0.5 text-[10px] font-bold text-white">
            Sold Out
          </span>
        )}
      </Link>
      <div className="flex flex-1 flex-col gap-1.5 p-3">
        <span className="text-[10px] font-bold uppercase tracking-wider text-slate-400">{product.brandName}</span>
        <Link className="line-clamp-2 min-h-[2.4em] text-sm font-semibold leading-snug text-slate-800 transition-colors group-hover:text-[#ff6b00]" to={`/product/${product.slug}`}>
          {product.name}
        </Link>
        {product.reviewCount > 0 && (
          <div className="flex items-center gap-1 text-xs text-slate-500">
            <span className="material-symbols-outlined text-[13px] text-[#ff6b00]">star</span>
            <span>{product.rating.toFixed(1)}</span>
            <span className="text-slate-300">·</span>
            <span>{formatCompact(product.reviewCount)} reviews</span>
          </div>
        )}
        <p className="mt-auto pt-1 text-base font-bold text-[#ff6b00]">{formatPrice(product.basePrice)} VND</p>
      </div>
    </article>
  )
}

export default function StoreProfilePage() {
  const { slug } = useParams<{ slug: string }>()
  const { user } = useAuth()
  const [searchParams, setSearchParams] = useSearchParams()

  const sortBy = searchParams.get('sort') ?? 'popular'
  const search = searchParams.get('search') ?? ''
  const pageNumber = Math.max(1, Number(searchParams.get('page') ?? 1))

  const [store, setStore] = useState<StoreProfile | null>(null)
  const [products, setProducts] = useState<PagedResult<CatalogProduct> | null>(null)
  const [storeLoading, setStoreLoading] = useState(true)
  const [productsLoading, setProductsLoading] = useState(true)
  const [storeError, setStoreError] = useState<string | null>(null)
  const [following, setFollowing] = useState(false)
  const [followLoading, setFollowLoading] = useState(false)

  useEffect(() => {
    if (!slug) return
    setStoreLoading(true)
    setStoreError(null)
    getStoreProfile(slug)
      .then((s) => {
        setStore(s)
        setFollowing(s.isFollowing ?? false)
      })
      .catch((err: unknown) => setStoreError(err instanceof Error ? err.message : 'Store not found.'))
      .finally(() => setStoreLoading(false))
  }, [slug])

  const loadProducts = useCallback(() => {
    if (!slug) return
    setProductsLoading(true)
    getStoreProducts(slug, { sortBy, search: search || undefined, pageNumber, pageSize: 20 })
      .then(setProducts)
      .catch(() => setProducts(null))
      .finally(() => setProductsLoading(false))
  }, [slug, sortBy, search, pageNumber])

  useEffect(() => { loadProducts() }, [loadProducts])

  const handleFollow = async () => {
    if (!slug || !user) return
    setFollowLoading(true)
    try {
      await followStore(slug)
      setFollowing((f) => !f)
      setStore((s) => s ? { ...s, followerCount: (s.followerCount ?? 0) + (following ? -1 : 1) } : s)
    } catch { /* ignore */ } finally { setFollowLoading(false) }
  }

  const setSort = (value: string) => setSearchParams((p) => { p.set('sort', value); p.delete('page'); return p })
  const setPage = (n: number) => setSearchParams((p) => { p.set('page', String(n)); return p })

  if (storeLoading) {
    return (
      <div className="mx-auto w-full max-w-[1280px] px-4 py-10 lg:px-8">
        <div className="mb-6 h-48 animate-pulse rounded-2xl bg-slate-200" />
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          {Array.from({ length: 10 }).map((_, i) => (
            <div className="h-64 animate-pulse rounded-xl bg-slate-200" key={i} />
          ))}
        </div>
      </div>
    )
  }

  if (storeError || !store) {
    return (
      <div className="mx-auto w-full max-w-[1280px] px-4 py-16 text-center lg:px-8">
        <span className="material-symbols-outlined mb-4 block text-6xl text-slate-300">store</span>
        <h1 className="text-2xl font-bold text-slate-900">Store not found</h1>
        <p className="mt-2 text-sm text-slate-500">{storeError ?? 'The requested store does not exist.'}</p>
        <Link className="mt-6 inline-flex rounded-lg bg-[#ff6b00] px-6 py-3 text-sm font-bold text-white hover:bg-[#ea580c]" to="/products">
          Browse Products
        </Link>
      </div>
    )
  }

  const totalPages = products?.totalPages ?? 1
  const totalCount = products?.totalCount ?? 0

  return (
    <div className="mx-auto w-full max-w-[1280px] px-4 py-6 lg:px-8">
      {/* Store header */}
      <div className="mb-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
        <div className="h-36 w-full bg-gradient-to-r from-slate-800 to-slate-900 md:h-48">
          {store.bannerUrl && (
            <img alt="Store banner" className="h-full w-full object-cover" src={store.bannerUrl} />
          )}
        </div>

        <div className="px-6 pb-6">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-end">
            <div className="-mt-10 flex h-20 w-20 shrink-0 items-center justify-center overflow-hidden rounded-2xl border-4 border-white bg-white shadow-md sm:-mt-12 sm:h-24 sm:w-24">
              {store.logoUrl
                ? <img alt={store.name} className="h-full w-full object-cover" src={store.logoUrl} />
                : <span className="text-3xl font-black text-[#ff6b00]">{store.name.slice(0, 1).toUpperCase()}</span>
              }
            </div>

            <div className="flex-1 min-w-0">
              <div className="flex flex-wrap items-center gap-2">
                <h1 className="text-2xl font-bold text-slate-900">{store.name}</h1>
                {store.isVerified && (
                  <span className="inline-flex items-center gap-1 rounded-full bg-blue-50 px-2.5 py-0.5 text-xs font-bold text-blue-700">
                    <span className="material-symbols-outlined text-[13px]">verified</span>
                    Verified
                  </span>
                )}
              </div>
              {store.description && (
                <p className="mt-1 max-w-2xl text-sm leading-relaxed text-slate-500">{store.description}</p>
              )}
            </div>

            <div className="flex shrink-0 items-center gap-3">
              <button
                className="inline-flex items-center gap-1.5 rounded-lg border border-[#ff6b00] bg-[#fff1e8] px-4 py-2 text-sm font-semibold text-[#ff6b00] transition hover:bg-[#ffe2cf]"
                onClick={() => window.dispatchEvent(new CustomEvent('gearzone:open-chat', { detail: { storeSlug: slug } }))}
                type="button"
              >
                <span className="material-symbols-outlined text-[16px]">chat</span>
                Chat Now
              </button>

              {user && (
                <button
                  className={`inline-flex items-center gap-1.5 rounded-lg border px-4 py-2 text-sm font-semibold transition ${
                    following
                      ? 'border-slate-300 bg-slate-100 text-slate-700 hover:bg-slate-200'
                      : 'border-blue-600 bg-blue-600 text-white hover:bg-blue-700'
                  }`}
                  disabled={followLoading}
                  onClick={handleFollow}
                  type="button"
                >
                  <span className="material-symbols-outlined text-[16px]">{following ? 'person_remove' : 'person_add'}</span>
                  {followLoading ? '…' : following ? 'Unfollow' : 'Follow'}
                </button>
              )}
            </div>
          </div>

          <div className="mt-5 flex flex-wrap gap-6 border-t border-slate-100 pt-4 text-sm">
            <div className="flex items-center gap-1.5 text-slate-600">
              <span className="material-symbols-outlined text-[17px] text-[#ff6b00]">group</span>
              <span>{formatCompact(store.followerCount ?? 0)} followers</span>
            </div>
            <div className="flex items-center gap-1.5 text-slate-600">
              <span className="material-symbols-outlined text-[17px] text-[#ff6b00]">inventory_2</span>
              <span>{formatCompact(store.productCount ?? totalCount)} products</span>
            </div>
            {store.totalSold != null && (
              <div className="flex items-center gap-1.5 text-slate-600">
                <span className="material-symbols-outlined text-[17px] text-[#ff6b00]">shopping_bag</span>
                <span>{formatCompact(store.totalSold)} sold</span>
              </div>
            )}
            {store.rating != null && store.reviewCount != null && store.reviewCount > 0 && (
              <div className="flex items-center gap-1.5 text-slate-600">
                <span className="material-symbols-outlined text-[17px] text-[#ff6b00]">star</span>
                <span>{store.rating.toFixed(1)} ({formatCompact(store.reviewCount)} ratings)</span>
              </div>
            )}
            {store.responseRate != null && (
              <div className="flex items-center gap-1.5 text-slate-600">
                <span className="material-symbols-outlined text-[17px] text-[#ff6b00]">reply</span>
                <span>Response rate {store.responseRate}%</span>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Sort tabs */}
      <div className="mb-4 flex flex-wrap items-center gap-2 border-b border-slate-200 pb-4">
        <span className="mr-2 text-sm font-semibold text-slate-600">Sort by:</span>
        {SORT_OPTIONS.map((opt) => (
          <button
            className={`rounded-full px-4 py-1.5 text-sm font-semibold transition ${
              sortBy === opt.value
                ? 'bg-[#ff6b00] text-white'
                : 'border border-slate-200 bg-white text-slate-600 hover:border-[#ff6b00] hover:text-[#ff6b00]'
            }`}
            key={opt.value}
            onClick={() => setSort(opt.value)}
            type="button"
          >
            {opt.label}
          </button>
        ))}
      </div>

      {/* Products grid */}
      {productsLoading ? (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
          {Array.from({ length: 10 }).map((_, i) => (
            <div className="h-64 animate-pulse rounded-xl bg-slate-200" key={i} />
          ))}
        </div>
      ) : products?.items.length ? (
        <>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5">
            {products.items.map((p) => <ProductCard key={p.slug} product={p} />)}
          </div>

          {totalPages > 1 && (
            <div className="mt-8 flex items-center justify-center gap-2">
              <button
                className="flex h-9 w-9 items-center justify-center rounded-lg border border-slate-200 text-slate-500 transition hover:border-[#ff6b00] hover:text-[#ff6b00] disabled:cursor-not-allowed disabled:opacity-40"
                disabled={pageNumber <= 1}
                onClick={() => setPage(pageNumber - 1)}
                type="button"
              >
                <span className="material-symbols-outlined text-lg">chevron_left</span>
              </button>

              {Array.from({ length: totalPages }, (_, i) => i + 1)
                .filter((p) => p === 1 || p === totalPages || Math.abs(p - pageNumber) <= 2)
                .reduce<(number | 'ellipsis')[]>((acc, p, idx, arr) => {
                  if (idx > 0 && (arr[idx - 1] as number) < p - 1) acc.push('ellipsis')
                  acc.push(p)
                  return acc
                }, [])
                .map((p, i) =>
                  p === 'ellipsis' ? (
                    <span className="px-1 text-slate-400" key={`ellipsis-${i}`}>…</span>
                  ) : (
                    <button
                      className={`flex h-9 w-9 items-center justify-center rounded-lg border text-sm font-semibold transition ${
                        p === pageNumber
                          ? 'border-[#ff6b00] bg-[#ff6b00] text-white'
                          : 'border-slate-200 text-slate-600 hover:border-[#ff6b00] hover:text-[#ff6b00]'
                      }`}
                      key={p}
                      onClick={() => setPage(p as number)}
                      type="button"
                    >
                      {p}
                    </button>
                  ),
                )}

              <button
                className="flex h-9 w-9 items-center justify-center rounded-lg border border-slate-200 text-slate-500 transition hover:border-[#ff6b00] hover:text-[#ff6b00] disabled:cursor-not-allowed disabled:opacity-40"
                disabled={pageNumber >= totalPages}
                onClick={() => setPage(pageNumber + 1)}
                type="button"
              >
                <span className="material-symbols-outlined text-lg">chevron_right</span>
              </button>
            </div>
          )}
        </>
      ) : (
        <div className="rounded-2xl border border-dashed border-slate-300 p-16 text-center text-slate-400">
          <span className="material-symbols-outlined mb-3 block text-5xl">inventory_2</span>
          <p className="font-medium">No products listed yet.</p>
        </div>
      )}
    </div>
  )
}
