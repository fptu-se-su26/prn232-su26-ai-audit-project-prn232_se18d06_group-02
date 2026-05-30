import { Link } from 'react-router-dom'

export interface ProductCardData {
  slug: string
  name: string
  basePrice: number
  imageUrl?: string
  brandName?: string
  storeName?: string
  storeLogoUrl?: string
  rating?: number
  reviewCount?: number
  isInStock?: boolean
  defaultVariantId?: string
  saleBadge?: string
}

interface Props {
  product: ProductCardData
  onAddToCart?: (variantId: string) => void
}

export default function ProductCard({ product: p, onAddToCart }: Props) {
  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm hover:shadow-md hover:-translate-y-1 transition-all duration-300 group flex flex-col overflow-hidden relative">
      {p.saleBadge && (
        <div className="absolute top-3 left-3 z-10 bg-red-600 text-white text-[10px] font-bold px-2 py-1 rounded">{p.saleBadge}</div>
      )}

      {/* Image */}
      <Link to={`/products/${p.slug}`} className="p-4 flex items-center justify-center h-48 bg-white">
        <img
          src={p.imageUrl ?? '/images/placeholder.png'}
          alt={p.name}
          className="w-full h-full object-contain mix-blend-multiply"
          loading="lazy"
        />
      </Link>

      {/* Content */}
      <div className="p-4 flex flex-col gap-2 flex-1 border-t border-gray-100">
        <div className="flex items-center gap-2 mb-1">
          {p.brandName && (
            <span className="text-[10px] font-bold uppercase tracking-wider text-gray-500 border border-gray-200 px-1.5 rounded">{p.brandName}</span>
          )}
          {p.reviewCount && p.reviewCount > 0 ? (
            <div className="flex items-center gap-0.5 text-amber-400">
              <span className="material-symbols-outlined text-[14px]" style={{ fontVariationSettings: "'FILL' 1" }}>star</span>
              <span className="text-xs font-medium text-gray-500 ml-0.5">{(p.rating ?? 0).toFixed(1)} ({p.reviewCount})</span>
            </div>
          ) : null}
        </div>

        <h3 className="font-bold text-sm text-gray-900 leading-snug line-clamp-2 min-h-[2.5em] group-hover:text-primary transition-colors">
          <Link to={`/products/${p.slug}`}>{p.name}</Link>
        </h3>

        <div className="mt-auto pt-2 flex flex-col gap-1">
          <span className="text-xl font-bold text-primary">{p.basePrice.toLocaleString('vi-VN')} ₫</span>

          {p.storeName && (
            <div className="flex items-center gap-2">
              {p.storeLogoUrl ? (
                <img src={p.storeLogoUrl} alt={p.storeName} className="w-4 h-4 rounded-full object-cover" />
              ) : (
                <div className="w-4 h-4 rounded-full bg-gray-200 flex-shrink-0" />
              )}
              <span className="text-xs text-gray-500 truncate">{p.storeName}</span>
            </div>
          )}

          <button
            onClick={() => p.defaultVariantId && onAddToCart?.(p.defaultVariantId)}
            disabled={p.isInStock === false}
            className="w-full mt-2 bg-orange-50 hover:bg-secondary text-secondary hover:text-white font-semibold py-2 rounded-lg text-sm transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span className="material-symbols-outlined text-[18px]">shopping_cart</span>
            {p.isInStock === false ? 'Out of Stock' : 'Add to Cart'}
          </button>
        </div>
      </div>
    </div>
  )
}
