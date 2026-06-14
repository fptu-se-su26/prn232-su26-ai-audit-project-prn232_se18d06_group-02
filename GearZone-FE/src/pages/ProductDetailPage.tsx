import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { getProductDetail } from '@/api/catalog'
import { useAuth } from '@/contexts/useAuth'
import { useChatContext } from '@/contexts/useChatContext'
import type {
  CatalogProduct,
  ProductAttributeSelection,
  ProductDetail,
  ProductDetailResponse,
  ProductReviewBreakdown,
  ProductReviewItem,
  ProductVariantDetail,
} from '@/types/catalog'

const GEARZONE_ORANGE = '#ff6b00'

function formatPrice(value: number) {
  return new Intl.NumberFormat('vi-VN').format(value)
}

function formatDate(value: string, includeTime = false) {
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    ...(includeTime ? { hour: '2-digit', minute: '2-digit' } : {}),
  }).format(new Date(value))
}

function formatCompactCount(value: number) {
  if (value > 999) return `${(value / 1000).toFixed(1).replace(/\.0$/, '')}k`
  return String(value)
}

function buildSelectionMap(product: ProductDetail, variant?: ProductVariantDetail | null) {
  const entries = product.attributeSelections.map((attribute) => {
    const selected = attribute.options.find((option) => variant?.selectedOptionIds.includes(option.optionId))
    return [attribute.attributeId, selected?.optionId ?? null] as const
  })

  return Object.fromEntries(entries) as Record<number, number | null>
}

function findVariantFromSelection(product: ProductDetail, selection: Record<number, number | null>) {
  const selectedOptionIds = Object.values(selection).filter((value): value is number => typeof value === 'number')
  if (selectedOptionIds.length !== product.attributeSelections.length) return null

  return (
    product.variants.find(
      (variant) =>
        variant.selectedOptionIds.length === selectedOptionIds.length &&
        selectedOptionIds.every((optionId) => variant.selectedOptionIds.includes(optionId)),
    ) ?? null
  )
}

function getJoinedText(value: string) {
  const joinedAt = new Date(value)
  const now = new Date()
  const days = Math.max(0, Math.floor((now.getTime() - joinedAt.getTime()) / (1000 * 60 * 60 * 24)))

  if (days >= 365) return `${Math.floor(days / 365)} years ago`
  if (days >= 30) return `${Math.floor(days / 30)} months ago`
  if (days >= 7) return `${Math.floor(days / 7)} weeks ago`
  if (days >= 1) return `${days} days ago`
  return 'recently'
}

function RatingStars({ rating, size = 16, accent = GEARZONE_ORANGE }: { rating: number; size?: number; accent?: string }) {
  return (
    <div className="flex items-center gap-0.5" aria-label={`${rating.toFixed(1)} out of 5 stars`}>
      {Array.from({ length: 5 }).map((_, index) => {
        const fill = Math.max(0, Math.min(1, rating - index))
        return (
          <span className="relative leading-none" key={index} style={{ fontSize: size }}>
            <span className="material-symbols-outlined text-slate-300">star</span>
            <span className="absolute inset-0 overflow-hidden" style={{ color: accent, width: `${fill * 100}%` }}>
              <span className="material-symbols-outlined">star</span>
            </span>
          </span>
        )
      })}
    </div>
  )
}

function RelatedProductCard({ product }: { product: CatalogProduct }) {
  const imageUrl = product.imageUrl || 'https://placehold.co/640x640/f8fafc/94a3b8?text=GearZone'

  return (
    <article className="group flex flex-col overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm transition-all duration-300 hover:-translate-y-1 hover:shadow-md">
      <Link className="relative block bg-white p-4" to={`/product/${product.slug}`}>
        <img alt={product.name} className="h-44 w-full object-contain mix-blend-multiply" src={imageUrl} />
      </Link>
      <div className="flex flex-1 flex-col gap-2 border-t border-slate-100 p-4">
        <div className="flex items-center gap-2">
          <span className="rounded border border-slate-200 px-1.5 py-0.5 text-[10px] font-bold uppercase tracking-wider text-slate-500">
            {product.brandName}
          </span>
          {product.reviewCount > 0 ? (
            <div className="flex items-center gap-1 text-xs text-slate-500">
              <span className="material-symbols-outlined text-[14px]" style={{ color: GEARZONE_ORANGE }}>star</span>
              <span>{product.rating.toFixed(1)} ({product.reviewCount})</span>
            </div>
          ) : null}
        </div>
        <Link className="line-clamp-2 min-h-[2.5em] text-sm font-bold leading-snug text-slate-900 transition-colors group-hover:text-[#ff6b00]" to={`/product/${product.slug}`}>
          {product.name}
        </Link>
        <div className="mt-auto flex items-end justify-between gap-3 pt-2">
          <div>
            <p className="text-lg font-bold text-[#ff6b00]">{formatPrice(product.basePrice)} VND</p>
            <p className="text-xs text-slate-500">{product.storeName}</p>
          </div>
          <span className={`rounded px-2 py-1 text-[11px] font-semibold ${product.isInStock ? 'bg-emerald-50 text-emerald-700' : 'bg-slate-100 text-slate-500'}`}>
            {product.isInStock ? 'In Stock' : 'Sold Out'}
          </span>
        </div>
      </div>
    </article>
  )
}

export default function ProductDetailPage() {
  const { slug } = useParams()
  const navigate = useNavigate()
  const { user, loading: authLoading } = useAuth()
  const { enabled: chatEnabled, openChatWithStore } = useChatContext()
  const [data, setData] = useState<ProductDetailResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [selectedVariantId, setSelectedVariantId] = useState<string | null>(null)
  const [selectedImageIndex, setSelectedImageIndex] = useState(0)
  const [quantity, setQuantity] = useState(1)
  const [cartLoading, setCartLoading] = useState(false)
  const [buyNowLoading, setBuyNowLoading] = useState(false)
  const [actionMessage, setActionMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)
  const [latestCartCount, setLatestCartCount] = useState<number | null>(null)

  useEffect(() => {
    if (!slug) return
    const productSlug = slug

    let cancelled = false

    async function loadProduct() {
      try {
        setLoading(true)
        setError(null)
        const result = await getProductDetail(productSlug)
        if (cancelled) return

        setData(result)
        setSelectedVariantId(result.product.variants[0]?.id ?? null)
        setSelectedImageIndex(0)
      } catch (loadError) {
        if (cancelled) return
        setError(loadError instanceof Error ? loadError.message : 'Could not load this product.')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void loadProduct()

    return () => {
      cancelled = true
    }
  }, [slug])

  const product = data?.product ?? null
  const selectedVariant = useMemo(
    () => product?.variants.find((variant) => variant.id === selectedVariantId) ?? product?.variants[0] ?? null,
    [product, selectedVariantId],
  )
  const selectedOptions = useMemo(() => (product ? buildSelectionMap(product, selectedVariant) : {}), [product, selectedVariant])
  const selectedImage = product?.imageUrls[selectedImageIndex] || product?.imageUrls[0] || 'https://placehold.co/960x960/f8fafc/94a3b8?text=GearZone'
  const stockQuantity = selectedVariant?.stockQuantity ?? 0
  const isInStock = stockQuantity > 0 || (product?.variants.length ?? 0) === 0
  const displayPrice = selectedVariant?.price ?? product?.basePrice ?? 0
  const displayOriginalPrice = Math.round(displayPrice * 1.15)
  const joinedText = product ? getJoinedText(product.storeCreatedAt) : ''

  function handleOptionSelect(attribute: ProductAttributeSelection, optionId: number) {
    if (!product) return

    const nextSelection = { ...selectedOptions, [attribute.attributeId]: optionId }
    const matchedVariant = findVariantFromSelection(product, nextSelection)
    setSelectedVariantId(matchedVariant?.id ?? null)
    setQuantity(1)
  }

  function handleVariantQuickSelect(variant: ProductVariantDetail) {
    setSelectedVariantId(variant.id)
    setQuantity(1)
  }

  async function submitCartAction(isBuyNow: boolean) {
    if (authLoading) return

    if (!user) {
      const returnUrl = `${window.location.pathname}${window.location.search}`
      navigate(`/login?returnUrl=${encodeURIComponent(returnUrl)}`)
      return
    }

    if (!selectedVariant && product?.variants.length) {
      setActionMessage({ type: 'error', text: 'Please select a valid product variant.' })
      return
    }

    if (stockQuantity > 0 && quantity > stockQuantity) {
      setActionMessage({ type: 'error', text: `Only ${stockQuantity} item(s) are available in stock.` })
      return
    }

    const variantId = selectedVariant?.id
    if (!variantId) {
      setActionMessage({ type: 'error', text: 'This product does not have an available variant to purchase.' })
      return
    }

    setActionMessage(null)
    if (isBuyNow) setBuyNowLoading(true)
    else setCartLoading(true)

    try {
      const response = await fetch('/api/cart/add', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify({
          variantId,
          quantity,
          isBuyNow,
        }),
      })

      const redirectedToLogin = response.redirected || (response.status === 200 && response.url.toLowerCase().includes('login'))
      if (response.status === 401 || redirectedToLogin) {
        const returnUrl = `${window.location.pathname}${window.location.search}`
        navigate(`/login?returnUrl=${encodeURIComponent(returnUrl)}`)
        return
      }

      const payload = await response.json().catch(() => null)
      if (!response.ok) {
        const message =
          (payload && typeof payload.error === 'string' && payload.error) ||
          'An error occurred while adding this product to your cart.'
        setActionMessage({ type: 'error', text: message })
        return
      }

      if (isBuyNow) {
        const cartItemId = payload?.cartItemId
        if (cartItemId) {
          window.location.href = `/Checkout?SelectedCartItemIds=${encodeURIComponent(cartItemId)}`
          return
        }

        window.location.href = '/cart'
        return
      }

      setQuantity(1)
      const nextCartCount = typeof payload?.cartCount === 'number' ? payload.cartCount : null
      setLatestCartCount(nextCartCount)
      if (nextCartCount !== null) {
        window.dispatchEvent(new CustomEvent('gearzone:cart-count-updated', { detail: { count: nextCartCount } }))
      }
      setActionMessage({ type: 'success', text: 'Product added to cart successfully!' })
    } catch {
      setActionMessage({ type: 'error', text: 'Server connection error.' })
    } finally {
      if (isBuyNow) setBuyNowLoading(false)
      else setCartLoading(false)
    }
  }

  function renderBreakdownRow(breakdown: ProductReviewBreakdown) {
    return (
      <button
        className="inline-flex min-h-[2.05rem] items-center justify-center gap-1.5 border border-[#ededed] bg-white px-4 py-2 text-sm font-medium text-[#555] transition hover:border-[#ff6b00] hover:text-[#ff6b00]"
        key={breakdown.rating}
        type="button"
      >
        <span>{breakdown.rating} Star</span>
        <span className="text-xs opacity-75">{breakdown.count}</span>
      </button>
    )
  }

  function renderReviewCard(review: ProductReviewItem) {
    return (
      <article className="border-b border-[#f5f5f5] py-6 last:border-b-0" key={review.id}>
        <div className="flex gap-4">
          {review.buyerAvatarUrl ? (
            <img alt={review.buyerDisplayName} className="h-10 w-10 rounded-full border border-slate-200 object-cover" src={review.buyerAvatarUrl} />
          ) : (
            <div className="flex h-10 w-10 items-center justify-center rounded-full border border-[#e8edf3] bg-[#f6f8fb] text-sm font-semibold text-[#5c6773]">
              {review.buyerDisplayName.slice(0, 1).toUpperCase()}
            </div>
          )}
          <div className="min-w-0 flex-1">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <p className="text-sm font-semibold text-slate-900">{review.buyerDisplayName}</p>
                  <span className="inline-flex items-center rounded-full bg-[#f6f6f6] px-2 py-0.5 text-[11px] font-bold uppercase tracking-[0.08em] text-[#8f8f8f]">
                    Verified
                  </span>
                </div>
                <div className="mt-2">
                  <RatingStars rating={review.rating} size={15} />
                </div>
                <p className="mt-2 text-xs text-[#767676]">
                  {formatDate(review.createdAt, true)}
                  {review.variantName ? ` | Variation: ${review.variantName}` : ''}
                </p>
              </div>
            </div>

            {review.comment ? (
              <p className="mt-3 whitespace-pre-line text-sm leading-6 text-slate-700">{review.comment}</p>
            ) : (
              <p className="mt-3 text-sm italic text-slate-400">Buyer left a star rating without a written comment.</p>
            )}

            {review.sellerReplyContent ? (
              <div className="mt-4 border-l-[3px] border-[#f4a58d] bg-[#fafafa] px-4 py-3">
                <p className="text-sm font-semibold text-slate-900">Seller response</p>
                <p className="mt-1.5 whitespace-pre-line text-sm leading-6 text-slate-600">{review.sellerReplyContent}</p>
                {review.sellerReplyAt ? <p className="mt-2 text-xs text-slate-400">{formatDate(review.sellerReplyAt, true)}</p> : null}
              </div>
            ) : null}
          </div>
        </div>
      </article>
    )
  }

  if (loading) {
    return (
      <section className="w-full max-w-[1280px] px-4 py-6 lg:px-8">
        <div className="mx-auto grid grid-cols-1 gap-8 lg:grid-cols-12 lg:gap-12">
          <div className="space-y-4 lg:col-span-5">
            <div className="aspect-[4/3] animate-pulse rounded-xl bg-white" />
            <div className="grid grid-cols-4 gap-3">
              {Array.from({ length: 4 }).map((_, index) => (
                <div className="aspect-square animate-pulse rounded-lg bg-white" key={index} />
              ))}
            </div>
          </div>
          <div className="space-y-4 lg:col-span-7">
            <div className="h-8 animate-pulse rounded bg-white" />
            <div className="h-6 w-2/3 animate-pulse rounded bg-white" />
            <div className="h-24 animate-pulse rounded-xl bg-white" />
            <div className="h-56 animate-pulse rounded-xl bg-white" />
          </div>
        </div>
      </section>
    )
  }

  if (error || !product) {
    return (
      <section className="mx-auto w-full max-w-[1280px] px-4 py-10 lg:px-8">
        <div className="rounded-xl border border-rose-200 bg-white px-8 py-14 text-center shadow-sm">
          <p className="text-xs font-black uppercase tracking-[0.24em] text-rose-500">Unavailable</p>
          <h1 className="mt-3 text-3xl font-bold text-slate-950">This product could not be loaded.</h1>
          <p className="mt-3 text-sm text-slate-500">{error ?? 'The requested product does not exist or is no longer public.'}</p>
          <Link className="mt-6 inline-flex rounded bg-[#ff6b00] px-5 py-3 text-sm font-bold text-white transition hover:bg-[#ea580c]" to="/products">
            Back to catalog
          </Link>
        </div>
      </section>
    )
  }

  return (
    <section className="mx-auto w-full max-w-[1280px] px-4 py-6 lg:px-8">
      <nav className="flex flex-wrap gap-2 pb-6 text-sm">
        <Link className="text-slate-500 transition hover:text-[#ff6b00]" to="/">Home</Link>
        <span className="text-slate-400">/</span>
        <Link className="text-slate-500 transition hover:text-[#ff6b00]" to={`/products/${product.categorySlug}`}>{product.categoryName}</Link>
        <span className="text-slate-400">/</span>
        <span className="font-medium text-slate-900">{product.name}</span>
      </nav>

      <div className="grid grid-cols-1 gap-8 lg:grid-cols-12 lg:gap-12">
        <div className="flex flex-col gap-4 lg:col-span-5">
          <div className="group relative aspect-[4/3] overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
            <img alt={`${product.name} main view`} className="h-full w-full object-contain p-4 transition-transform duration-500 group-hover:scale-105" src={selectedImage} />
            <div className="absolute left-4 top-4 rounded bg-[#ff6b00] px-2 py-1 text-xs font-bold text-white">-15% OFF</div>
          </div>

          {product.imageUrls.length > 1 ? (
            <div className="grid grid-cols-4 gap-3">
              {product.imageUrls.slice(0, 4).map((imageUrl, index) => (
                <button
                  className={`aspect-square overflow-hidden rounded-lg border bg-white p-1 transition-colors focus:outline-none ${
                    selectedImageIndex === index ? 'border-blue-700 ring-1 ring-blue-700' : 'border-slate-200 hover:border-blue-700'
                  }`}
                  key={`${imageUrl}-${index}`}
                  onClick={() => setSelectedImageIndex(index)}
                  type="button"
                >
                  <img alt={`Thumbnail ${index + 1}`} className="h-full w-full object-contain" src={imageUrl} />
                </button>
              ))}
            </div>
          ) : null}

          <button
            className="group/detail mt-4 flex items-center gap-3 rounded-2xl border border-slate-200 bg-white px-6 py-2.5 text-slate-600 shadow-sm transition-all hover:border-blue-700/50 hover:bg-blue-50/40"
            onClick={() => window.open(`/compare?categoryId=${product.categoryId}&ids=${product.id}`, '_self')}
            type="button"
          >
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-slate-100 transition-all duration-300 group-hover/detail:bg-blue-700 group-hover/detail:text-white">
              <span className="material-symbols-outlined text-lg">compare_arrows</span>
            </div>
            <span className="text-sm font-black uppercase tracking-widest transition-colors group-hover/detail:text-blue-700">Compare this product</span>
          </button>
        </div>

        <div className="flex flex-col gap-6 lg:col-span-7">
          <div className="flex flex-col gap-2">
            <h1 className="mb-1 mt-0 text-2xl font-medium leading-relaxed text-slate-900">{product.name}</h1>

            <div className="flex items-center gap-4 text-[14px]">
              {product.reviewCount > 0 ? (
                <>
                  <div className="flex cursor-pointer items-center gap-1.5">
                    <span className="border-b border-[#ff6b00] leading-tight text-[#ff6b00]">{product.rating.toFixed(1)}</span>
                    <div className="origin-left scale-90">
                      <RatingStars accent={GEARZONE_ORANGE} rating={product.rating} size={13} />
                    </div>
                  </div>
                  <div className="h-3.5 w-px bg-slate-300" />
                  <div className="flex items-center gap-1.5">
                    <span className="border-b border-slate-900 leading-tight text-slate-900">{product.reviewCount}</span>
                    <span className="text-[#757575]">Ratings</span>
                  </div>
                  <div className="h-3.5 w-px bg-slate-300" />
                </>
              ) : (
                <>
                  <div className="flex items-center gap-1 text-[#757575]">
                    <div className="origin-left scale-90">
                      <RatingStars accent={GEARZONE_ORANGE} rating={0} size={13} />
                    </div>
                    <span className="text-[13px]">No ratings yet</span>
                  </div>
                  <div className="h-3.5 w-px bg-slate-300" />
                </>
              )}

              <div className="flex items-center gap-1.5">
                <span className="text-slate-900">{formatCompactCount(product.soldCount)}</span>
                <span className="text-[#757575]">Sold</span>
              </div>

              <div className="flex-1" />

              <span className="text-[13px] text-[#757575]">Report</span>
            </div>

            <div className="mt-1 space-x-2 text-[13px] text-[#757575]">
              <span>
                Brand:{' '}
                <Link className="text-blue-700 hover:underline" to={`/products?brand=${product.brandSlug}`}>
                  {product.brandName}
                </Link>
              </span>
            </div>
          </div>

          <div className="rounded-xl bg-slate-50 p-5">
            <div className="flex flex-wrap items-end gap-3">
              <span className="text-4xl font-extrabold tracking-tight text-[#ff6b00]">{formatPrice(displayPrice)} VND</span>
              <span className="mb-1 text-lg text-slate-400 line-through">{formatPrice(displayOriginalPrice)} VND</span>
              <span className="mb-1 rounded bg-green-100 px-2 py-1 text-sm font-bold text-green-600">Save 15%</span>
            </div>
          </div>

          {product.attributeSelections.length > 0 ? (
            <div className="space-y-4">
              {product.attributeSelections.map((attribute) => (
                <div className="space-y-2" key={attribute.attributeId}>
                  <label className="text-sm font-medium text-slate-900">{attribute.name}</label>
                  <div className="flex flex-wrap gap-2">
                    {attribute.options.map((option) => {
                      const active = selectedOptions[attribute.attributeId] === option.optionId
                      return (
                        <button
                          className={`px-4 py-2 text-sm font-medium transition-all ${
                            active
                              ? 'border-2 border-blue-700 bg-blue-50 text-blue-700'
                              : 'border border-slate-200 bg-white text-slate-600 hover:border-slate-400'
                          } rounded-lg`}
                          key={option.optionId}
                          onClick={() => handleOptionSelect(attribute, option.optionId)}
                          type="button"
                        >
                          {option.value}
                        </button>
                      )
                    })}
                  </div>
                </div>
              ))}
            </div>
          ) : null}

          {product.variants.length > 1 ? (
            <div className="space-y-2">
              <label className="text-sm font-medium text-slate-900">Variant</label>
              <div className="flex flex-wrap gap-2">
                {product.variants.map((variant, index) => {
                  const active = selectedVariant?.id === variant.id
                  const label = variant.variantName?.trim() || `Variant #${index + 1}`
                  return (
                    <button
                      className={`rounded-lg px-4 py-2 text-sm font-medium transition-all ${
                        active
                          ? 'border-2 border-blue-700 bg-blue-50 text-blue-700'
                          : 'border border-slate-200 bg-white text-slate-600 hover:border-slate-400'
                      }`}
                      key={variant.id}
                      onClick={() => handleVariantQuickSelect(variant)}
                      type="button"
                    >
                      {label}
                    </button>
                  )
                })}
              </div>
            </div>
          ) : null}

          <div className="flex flex-col gap-5 pt-2">
            {isInStock ? (
              <div className="flex items-center gap-2 text-sm font-medium text-green-600">
                <span className="h-2 w-2 animate-pulse rounded-full bg-green-500" />
                <span>In Stock ({stockQuantity || 1} units available)</span>
              </div>
            ) : (
              <div className="flex items-center gap-2 text-sm font-medium text-red-600">
                <span className="h-2 w-2 rounded-full bg-red-500" />
                <span>Out of Stock</span>
              </div>
            )}

            <div className="flex flex-wrap items-stretch gap-4">
              <div className="flex h-12 items-center rounded-lg border border-slate-300 bg-white">
                <button
                  className="flex h-full w-10 items-center justify-center text-slate-500 hover:text-slate-900"
                  onClick={() => setQuantity((current) => Math.max(1, current - 1))}
                  type="button"
                >
                  <span className="material-symbols-outlined text-lg">remove</span>
                </button>
                <input className="h-full w-12 bg-transparent p-0 text-center font-medium text-slate-900 focus:outline-none" readOnly type="text" value={quantity} />
                <button
                  className="flex h-full w-10 items-center justify-center text-slate-500 hover:text-slate-900"
                  onClick={() => setQuantity((current) => Math.min(Math.max(stockQuantity, 1), current + 1))}
                  type="button"
                >
                  <span className="material-symbols-outlined text-lg">add</span>
                </button>
              </div>

              <button
                className={`flex h-12 min-w-[160px] flex-1 items-center justify-center gap-2 rounded-lg border-2 font-bold transition-colors ${
                  isInStock
                    ? 'border-blue-700 text-blue-700 hover:bg-blue-50'
                    : 'cursor-not-allowed border-slate-200 text-slate-400'
                }`}
                disabled={!isInStock || cartLoading || buyNowLoading}
                onClick={() => void submitCartAction(false)}
                type="button"
              >
                <span className={`material-symbols-outlined ${cartLoading ? 'animate-spin' : ''}`}>
                  {cartLoading ? 'refresh' : 'add_shopping_cart'}
                </span>
                {cartLoading ? 'Adding...' : 'Add to Cart'}
              </button>

              <button
                className={`flex h-12 min-w-[200px] flex-[1.5] items-center justify-center gap-2 rounded-lg font-bold text-white transition-colors ${
                  isInStock ? 'bg-[#ff6b00] hover:bg-[#ea580c]' : 'cursor-not-allowed bg-slate-300'
                }`}
                disabled={!isInStock || cartLoading || buyNowLoading}
                onClick={() => void submitCartAction(true)}
                type="button"
              >
                {buyNowLoading ? (
                  <>
                    <span className="material-symbols-outlined animate-spin">refresh</span>
                    Processing...
                  </>
                ) : (
                  'Buy Now'
                )}
              </button>
            </div>

            {product.attributeSelections.length > 0 && !selectedVariant ? (
              <div className="text-sm text-red-500">Please select a valid combination of options.</div>
            ) : null}

            {actionMessage ? (
              <div
                className={`rounded-lg border px-4 py-3 text-sm font-medium ${
                  actionMessage.type === 'success'
                    ? 'border-emerald-200 bg-emerald-50 text-emerald-700'
                    : 'border-rose-200 bg-rose-50 text-rose-700'
                }`}
              >
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <span>
                    {actionMessage.text}
                    {actionMessage.type === 'success' && latestCartCount !== null ? ` Cart now has ${latestCartCount} item(s).` : ''}
                  </span>
                  {actionMessage.type === 'success' ? (
                    <a className="font-bold underline underline-offset-2" href="/cart">
                      View cart
                    </a>
                  ) : null}
                </div>
              </div>
            ) : null}
          </div>

          <div className="mt-2 rounded-xl border border-blue-100 bg-blue-50 p-5">
            <ul className="space-y-3">
              <li className="flex items-start gap-3">
                <div className="flex h-6 min-w-6 items-center justify-center rounded-full bg-white text-blue-700 shadow-sm">
                  <span className="material-symbols-outlined text-sm">verified_user</span>
                </div>
                <div>
                  <p className="text-sm font-semibold text-slate-900">36 Months Genuine Warranty</p>
                  <p className="text-xs text-slate-500">Directly from GearZone service centers.</p>
                </div>
              </li>
              <li className="flex items-start gap-3">
                <div className="flex h-6 min-w-6 items-center justify-center rounded-full bg-white text-blue-700 shadow-sm">
                  <span className="material-symbols-outlined text-sm">local_shipping</span>
                </div>
                <div>
                  <p className="text-sm font-semibold text-slate-900">Free Nationwide Shipping</p>
                  <p className="text-xs text-slate-500">Estimated delivery: 2-4 business days.</p>
                </div>
              </li>
              <li className="flex items-start gap-3">
                <div className="flex h-6 min-w-6 items-center justify-center rounded-full bg-white text-blue-700 shadow-sm">
                  <span className="material-symbols-outlined text-sm">replay</span>
                </div>
                <div>
                  <p className="text-sm font-semibold text-slate-900">7-Day Return Policy</p>
                  <p className="text-xs text-slate-500">If there is a manufacturer defect.</p>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>

      <div className="mt-6 overflow-hidden border border-slate-200 bg-[#fafafa]">
        <div className="flex flex-col items-center gap-6 p-5 sm:flex-row sm:gap-10">
          <div className="flex min-w-[320px] items-center gap-4 pr-0 sm:border-r sm:border-slate-200 sm:pr-8">
            <a className="relative flex h-20 w-20 flex-shrink-0 items-center justify-center rounded-full border border-slate-200 bg-white shadow-sm" href={`/store/${product.storeSlug}`}>
              <span className="text-[28px] font-black tracking-wider text-slate-800">
                {(product.storeName || 'S').slice(0, 1).toUpperCase()}
              </span>
            </a>

            <div className="min-w-0">
              <a className="truncate text-[15px] font-medium text-slate-800 transition-colors hover:text-[#ff6b00]" href={`/store/${product.storeSlug}`}>
                {product.storeName}
              </a>
              <div className="mb-2 text-[13px] text-[#757575]">Active 6 minutes ago</div>
              <div className="flex items-center gap-2.5">
                <button
                  className="inline-flex items-center gap-1.5 rounded-sm border border-[#ff6b00] bg-[#fff1e8] px-3 py-[7px] text-[13px] text-[#ff6b00] transition-colors hover:bg-[#ffe2cf]"
                  type="button"
                  onClick={() => {
                    if (chatEnabled) void openChatWithStore(product.storeSlug)
                    else navigate('/login')
                  }}
                >
                  <span className="material-symbols-outlined text-[15px]">chat</span>
                  Chat Now
                </button>
                <a className="inline-flex items-center gap-1.5 rounded-sm border border-[#d5d5d5] bg-white px-3 py-[7px] text-[13px] text-[#555] shadow-sm transition-colors hover:bg-slate-50" href={`/store/${product.storeSlug}`}>
                  <span className="material-symbols-outlined text-[15px]">storefront</span>
                  View Shop
                </a>
              </div>
            </div>
          </div>

          <div className="grid w-full flex-1 grid-cols-1 gap-x-4 gap-y-4 text-[14px] sm:grid-cols-3">
            <div className="flex flex-col gap-4">
              <div className="flex items-center gap-4">
                <span className="w-[80px] text-[#757575]">Ratings</span>
                <span className="font-normal text-[#ff6b00]">{formatCompactCount(product.storeReviewCount)}</span>
              </div>
              <div className="flex items-center gap-4">
                <span className="w-[80px] text-[#757575]">Products</span>
                <span className="font-normal text-[#ff6b00]">{formatCompactCount(product.storeProductCount)}</span>
              </div>
            </div>

            <div className="flex flex-col gap-4">
              <div className="flex items-center gap-4">
                <span className="w-[110px] text-[#757575]">Response Rate</span>
                <span className="font-normal text-[#ff6b00]">91%</span>
              </div>
              <div className="flex items-center gap-4">
                <span className="w-[110px] text-[#757575]">Response Time</span>
                <span className="font-normal text-[#ff6b00]">within hours</span>
              </div>
            </div>

            <div className="flex flex-col gap-4">
              <div className="flex items-center gap-4">
                <span className="w-[70px] text-[#757575]">Joined</span>
                <span className="font-normal text-[#ff6b00]">{joinedText}</span>
              </div>
              <div className="flex items-center gap-4">
                <span className="w-[70px] text-[#757575]">Followers</span>
                <span className="font-normal text-[#ff6b00]">{formatCompactCount(product.storeFollowerCount)}</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="mt-16 space-y-10">
        <div className="sticky top-0 z-10 bg-slate-50 pt-4">
          <nav aria-label="Tabs" className="flex gap-8 overflow-x-auto border-b border-slate-200">
            {product.description?.trim() ? (
              <a className="whitespace-nowrap border-b-2 border-transparent px-1 py-4 text-sm font-medium text-slate-500 hover:border-slate-300 hover:text-slate-700" href="#product-description">
                Description
              </a>
            ) : null}
            {product.specifications.length > 0 ? (
              <a className="whitespace-nowrap border-b-2 border-transparent px-1 py-4 text-sm font-medium text-slate-500 hover:border-slate-300 hover:text-slate-700" href="#product-specifications">
                Specifications
              </a>
            ) : null}
            <a className="whitespace-nowrap border-b-2 border-transparent px-1 py-4 text-sm font-medium text-slate-500 hover:border-slate-300 hover:text-slate-700" href="#product-reviews">
              Reviews ({product.reviewCount})
            </a>
          </nav>
        </div>

        <div className="space-y-10">
          {product.specifications.length > 0 ? (
            <section className="scroll-mt-40" id="product-specifications">
              <h3 className="mb-6 text-xl font-bold text-slate-900">Technical Specifications</h3>
              <div className="overflow-hidden rounded-lg border border-slate-200">
                <table className="min-w-full divide-y divide-slate-200">
                  <tbody className="divide-y divide-slate-200 bg-white">
                    {product.specifications.map((specification) => (
                      <tr key={specification.name}>
                        <td className="w-1/3 whitespace-nowrap bg-slate-50/50 py-4 pl-4 pr-3 text-sm font-medium text-slate-500 sm:pl-6">
                          {specification.name}
                        </td>
                        <td className="break-words whitespace-normal px-3 py-4 font-mono text-sm text-slate-900">
                          {specification.value}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </section>
          ) : null}

          {product.description?.trim() ? (
            <section className="scroll-mt-40" id="product-description">
              <h3 className="mb-6 text-xl font-bold text-slate-900">Description</h3>
              <div className="max-w-none whitespace-pre-line text-sm leading-7 text-slate-600">
                {product.description}
              </div>
            </section>
          ) : null}

          <section className="scroll-mt-40 overflow-hidden border border-[#f0f0f0] bg-white" id="product-reviews">
            <div className="flex items-start justify-between gap-4 px-4 pb-0 pt-6 lg:px-8">
              <div>
                <h2 className="text-[1.15rem] font-bold text-[#222]">Product Ratings & Reviews</h2>
                <p className="mt-1 text-sm text-[#767676]">Ratings come from verified buyers only.</p>
              </div>
              {product.eligibleReview ? (
                <Link className="inline-flex min-h-[2.4rem] shrink-0 items-center justify-center gap-2 border border-[#ffd7bd] bg-white px-4 py-2 text-sm font-semibold text-[#ff6b00] transition hover:bg-[#fff1e8]" to="/customer">
                  {product.eligibleReview.hasExistingReview ? 'Edit yours' : 'Review now'}
                </Link>
              ) : null}
            </div>

            <div className="px-4 pb-4 pt-6 lg:px-8 lg:pb-6">
              <div className="rounded-sm border border-[#f6e7de] bg-[#fff8f5] p-6">
                {product.reviewSummary.totalReviews > 0 ? (
                  <div className="grid gap-6 lg:grid-cols-[14rem_1fr] lg:items-center">
                    <div className="text-center lg:text-left">
                      <p className="text-[1.9rem] font-bold leading-none text-[#ff6b00]">
                        <span className="text-[2rem]">{product.reviewSummary.averageRating.toFixed(1)}</span> out of 5
                      </p>
                      <div className="mt-3 flex justify-center lg:justify-start">
                        <RatingStars accent={GEARZONE_ORANGE} rating={product.reviewSummary.averageRating} size={21} />
                      </div>
                      <p className="mt-2 text-sm text-[#767676]">{product.reviewSummary.totalReviews} rating(s)</p>
                      <p className="mt-1 text-xs text-[#9aa4b2]">{product.reviewSummary.withCommentCount} with written comments</p>
                    </div>

                    <div className="flex flex-wrap gap-2.5">
                      {product.reviewSummary.breakdown
                        .slice()
                        .sort((left, right) => right.rating - left.rating)
                        .map(renderBreakdownRow)}
                    </div>
                  </div>
                ) : (
                  <div className="text-center">
                    <p className="text-[1.9rem] font-bold leading-none text-[#ff6b00]">
                      <span className="text-[2rem]">0.0</span> out of 5
                    </p>
                    <div className="mt-3 flex justify-center">
                      <RatingStars accent={GEARZONE_ORANGE} rating={0} size={21} />
                    </div>
                    <p className="mt-2 text-sm text-[#767676]">No verified ratings yet</p>
                    <p className="mt-1 text-xs text-[#9aa4b2]">Be the first buyer to review this product after delivery.</p>
                  </div>
                )}
              </div>

              {product.eligibleReview ? (
                <div className="mt-4 rounded border border-[#eef2f6] bg-[#f7f9fc] px-4 py-3.5">
                  <div className="flex items-start gap-3">
                    <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-white text-blue-700">
                      <span className="material-symbols-outlined text-[19px]">schedule</span>
                    </div>
                    <div>
                      <p className="text-base font-semibold text-slate-900">Your verified purchase can be reviewed.</p>
                      <p className="mt-1 text-sm text-slate-500">
                        Window closes {formatDate(product.eligibleReview.reviewDeadline, true)}.
                      </p>
                    </div>
                  </div>
                </div>
              ) : null}
            </div>

            <div className="px-4 lg:px-8">
              {data?.reviews.items.length ? (
                data.reviews.items.map(renderReviewCard)
              ) : (
                <div className="mb-6 border border-[#f1f1f1] bg-[#fcfcfc] px-5 py-8 text-center">
                  <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-white text-slate-300">
                    <span className="material-symbols-outlined text-3xl">forum</span>
                  </div>
                  <p className="mt-4 text-sm font-semibold text-slate-700">No reviews yet</p>
                  <p className="mt-2 text-xs text-slate-400">Be the first buyer to leave feedback for this product.</p>
                </div>
              )}
            </div>
          </section>

          {data?.relatedProducts.length ? (
            <section>
              <h3 className="mb-8 text-2xl font-bold text-slate-900">Related Products</h3>
              <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-4">
                {data.relatedProducts.map((relatedProduct) => (
                  <RelatedProductCard key={relatedProduct.id} product={relatedProduct} />
                ))}
              </div>
            </section>
          ) : null}
        </div>
      </div>
    </section>
  )
}
