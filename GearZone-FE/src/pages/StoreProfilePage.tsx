import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { followStore, getStoreProducts, getStoreProfile } from '@/api/catalog'
import ProductCard from '@/components/ProductCard'
import type { CatalogProduct, StoreProfile } from '@/types/catalog'

export default function StoreProfilePage() {
  const { slug } = useParams<{ slug: string }>()
  const [store, setStore] = useState<StoreProfile | null>(null)
  const [products, setProducts] = useState<CatalogProduct[]>([])
  const [loadingStore, setLoadingStore] = useState(true)
  const [loadingProducts, setLoadingProducts] = useState(true)
  const [followPending, setFollowPending] = useState(false)

  useEffect(() => {
    if (!slug) return
    setLoadingStore(true)
    getStoreProfile(slug)
      .then(setStore)
      .catch(() => {})
      .finally(() => setLoadingStore(false))

    setLoadingProducts(true)
    getStoreProducts(slug)
      .then((res) => setProducts(res.items))
      .catch(() => {})
      .finally(() => setLoadingProducts(false))
  }, [slug])

  const handleFollow = async () => {
    if (!slug || followPending) return
    setFollowPending(true)
    try {
      const result = await followStore(slug)
      setStore((prev) =>
        prev
          ? {
              ...prev,
              isFollowing: result.isFollowing,
              followerCount: prev.followerCount != null
                ? prev.followerCount + (result.isFollowing ? 1 : -1)
                : prev.followerCount,
            }
          : prev,
      )
    } catch {
      // ignore
    } finally {
      setFollowPending(false)
    }
  }

  if (loadingStore) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center">
        <span className="material-symbols-outlined animate-spin text-[36px] text-slate-400">
          progress_activity
        </span>
      </div>
    )
  }

  if (!store) {
    return (
      <div className="flex min-h-[60vh] flex-col items-center justify-center gap-3 text-slate-400">
        <span className="material-symbols-outlined text-[48px]">store_mall_directory</span>
        <p className="text-sm">Store not found.</p>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-6 sm:px-6">
      {/* Banner */}
      <div className="relative overflow-hidden rounded-2xl">
        {store.bannerUrl ? (
          <img
            src={store.bannerUrl}
            alt={`${store.name} banner`}
            className="h-48 w-full object-cover"
          />
        ) : (
          <div className="h-48 w-full bg-gradient-to-r from-blue-600 to-blue-800" />
        )}

        {/* Store logo */}
        <div className="absolute bottom-0 left-6 translate-y-1/2">
          {store.logoUrl ? (
            <img
              src={store.logoUrl}
              alt={store.name}
              className="h-20 w-20 rounded-2xl border-4 border-white object-cover shadow-lg"
            />
          ) : (
            <div className="flex h-20 w-20 items-center justify-center rounded-2xl border-4 border-white bg-blue-600 text-2xl font-bold text-white shadow-lg">
              {store.name[0]?.toUpperCase() ?? 'S'}
            </div>
          )}
        </div>
      </div>

      {/* Store header */}
      <div className="mt-14 flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
        <div className="min-w-0">
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-bold text-slate-900">{store.name}</h1>
            {store.isVerified && (
              <span
                className="material-symbols-outlined text-[22px] text-blue-500"
                style={{ fontVariationSettings: "'FILL' 1" }}
              >
                verified
              </span>
            )}
          </div>
          {store.description && (
            <p className="mt-1 max-w-xl text-sm leading-6 text-slate-500">{store.description}</p>
          )}
        </div>

        <button
          type="button"
          onClick={handleFollow}
          disabled={followPending}
          className={[
            'flex shrink-0 items-center gap-2 rounded-xl border px-5 py-2.5 text-sm font-semibold transition',
            store.isFollowing
              ? 'border-slate-200 bg-slate-100 text-slate-700 hover:bg-slate-200'
              : 'border-blue-600 bg-blue-600 text-white hover:bg-blue-700',
            followPending ? 'opacity-60 cursor-not-allowed' : '',
          ].join(' ')}
        >
          <span className="material-symbols-outlined text-[18px]">
            {store.isFollowing ? 'person_remove' : 'person_add'}
          </span>
          {store.isFollowing ? 'Following' : 'Follow'}
        </button>
      </div>

      {/* Stats */}
      <div className="mt-5 flex flex-wrap gap-6 border-b border-slate-100 pb-5">
        {store.followerCount != null && (
          <div className="flex items-center gap-2 text-sm text-slate-600">
            <span className="material-symbols-outlined text-[18px] text-slate-400">group</span>
            <span>
              <strong className="font-semibold text-slate-900">
                {store.followerCount.toLocaleString('vi-VN')}
              </strong>{' '}
              Followers
            </span>
          </div>
        )}
        {store.productCount != null && (
          <div className="flex items-center gap-2 text-sm text-slate-600">
            <span className="material-symbols-outlined text-[18px] text-slate-400">
              inventory_2
            </span>
            <span>
              <strong className="font-semibold text-slate-900">
                {store.productCount.toLocaleString('vi-VN')}
              </strong>{' '}
              Products
            </span>
          </div>
        )}
        {store.totalSold != null && (
          <div className="flex items-center gap-2 text-sm text-slate-600">
            <span className="material-symbols-outlined text-[18px] text-slate-400">
              shopping_bag
            </span>
            <span>
              <strong className="font-semibold text-slate-900">
                {store.totalSold.toLocaleString('vi-VN')}
              </strong>{' '}
              Sold
            </span>
          </div>
        )}
        {store.rating != null && (
          <div className="flex items-center gap-2 text-sm text-slate-600">
            <span
              className="material-symbols-outlined text-[18px] text-amber-400"
              style={{ fontVariationSettings: "'FILL' 1" }}
            >
              star
            </span>
            <span>
              <strong className="font-semibold text-slate-900">{store.rating.toFixed(1)}</strong>
              {store.reviewCount != null && (
                <span className="ml-1 text-slate-400">({store.reviewCount.toLocaleString('vi-VN')} reviews)</span>
              )}
            </span>
          </div>
        )}
      </div>

      {/* Products */}
      <div className="mt-6">
        <h2 className="mb-4 text-lg font-bold text-slate-900">Products</h2>

        {loadingProducts ? (
          <div className="flex items-center justify-center py-16">
            <span className="material-symbols-outlined animate-spin text-[32px] text-slate-400">
              progress_activity
            </span>
          </div>
        ) : products.length === 0 ? (
          <div className="flex flex-col items-center justify-center gap-3 py-16 text-slate-400">
            <span className="material-symbols-outlined text-[48px]">inventory_2</span>
            <p className="text-sm">No products listed yet.</p>
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
            {products.map((product) => (
              <ProductCard
                key={product.id}
                product={{
                  slug: product.slug,
                  name: product.name,
                  basePrice: product.basePrice,
                  imageUrl: product.imageUrl,
                  brandName: product.brandName,
                  storeName: product.storeName,
                  storeLogoUrl: product.storeLogoUrl,
                  rating: product.rating,
                  reviewCount: product.reviewCount,
                  isInStock: product.isInStock,
                  defaultVariantId: product.defaultVariantId,
                  saleBadge: product.saleBadges?.[0],
                }}
              />
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
