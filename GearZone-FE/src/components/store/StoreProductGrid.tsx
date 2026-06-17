import ProductCard, { type ProductCardData } from '@/components/ProductCard'
import type { CatalogProduct } from '@/types/catalog'

interface StoreProductGridProps {
  products: CatalogProduct[]
  onAddToCart?: (variantId: string) => void
}

function toCardData(product: CatalogProduct): ProductCardData {
  return {
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
  }
}

export default function StoreProductGrid({ products, onAddToCart }: StoreProductGridProps) {
  return (
    <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 xl:grid-cols-4">
      {products.map((product) => (
        <ProductCard key={product.id} product={toCardData(product)} onAddToCart={onAddToCart} />
      ))}
    </div>
  )
}
