export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CatalogProduct {
  id: string
  name: string
  slug: string
  categoryId: number
  brandName: string
  basePrice: number
  originalPrice?: number | null
  imageUrl: string
  rating: number
  reviewCount: number
  storeName: string
  storeSlug: string
  storeLogoUrl: string
  saleBadges: string[]
  highlightTags: string[]
  isInStock: boolean
  defaultVariantId: string
}

export interface BrandFilter {
  name: string
  slug: string
  productCount: number
}

export interface AttributeValueFilter {
  value: string
  productCount: number
}

export interface CategoryAttributeFilter {
  name: string
  filterType: string
  values: AttributeValueFilter[]
}

export interface CatalogFilterSidebar {
  brands: BrandFilter[]
  attributes: CategoryAttributeFilter[]
}

export interface CatalogCategory {
  id: number
  name: string
  slug: string
  subCategories: CatalogCategory[]
}

export interface ProductBrowseFilter {
  search?: string
  categorySlug?: string
  brandSlugs?: string[]
  minPrice?: number
  maxPrice?: number
  inStockOnly?: boolean
  attributes?: Record<string, string[]>
  sortBy?: string
  viewMode?: 'grid' | 'list'
  pageNumber?: number
  pageSize?: number
}

export interface ProductSuggestion {
  name: string
  slug: string
  imageUrl: string
  price: number
  brandName: string
}

export interface CompareProduct {
  id: string
  name: string
  image: string
  categoryId: string
}

export interface ProductAttributeOption {
  optionId: number
  value: string
}

export interface ProductAttributeSelection {
  attributeId: number
  name: string
  options: ProductAttributeOption[]
}

export interface ProductVariantDetail {
  id: string
  sku: string
  variantName: string
  price: number
  stockQuantity: number
  selectedOptionIds: number[]
}

export interface ProductSpecification {
  name: string
  value: string
}

export interface ProductReviewBreakdown {
  rating: number
  count: number
  percentage: number
}

export interface ProductReviewSummary {
  averageRating: number
  totalReviews: number
  withCommentCount: number
  breakdown: ProductReviewBreakdown[]
}

export interface ProductReviewItem {
  id: string
  orderItemId: string
  buyerDisplayName: string
  buyerAvatarUrl?: string | null
  rating: number
  comment?: string | null
  variantName: string
  createdAt: string
  updatedAt?: string | null
  sellerReplyContent?: string | null
  sellerReplyAt?: string | null
  sellerReplyUpdatedAt?: string | null
}

export interface EligibleReviewItem {
  orderItemId: string
  productId: string
  storeId: string
  productName: string
  productSlug: string
  productImageUrl?: string | null
  variantName: string
  storeName: string
  orderCode: number
  deliveredAt: string
  reviewDeadline: string
  hasExistingReview: boolean
  reviewId?: string | null
  existingRating?: number | null
  existingComment?: string | null
}

export interface ProductDetail {
  id: string
  categoryId: number
  name: string
  slug: string
  description: string
  basePrice: number
  soldCount: number
  rating: number
  reviewCount: number
  brandName: string
  brandSlug: string
  categoryName: string
  categorySlug: string
  storeId: string
  storeName: string
  storeSlug: string
  storeReviewCount: number
  storeProductCount: number
  storeCreatedAt: string
  storeFollowerCount: number
  imageUrls: string[]
  attributeSelections: ProductAttributeSelection[]
  variants: ProductVariantDetail[]
  specifications: ProductSpecification[]
  reviewSummary: ProductReviewSummary
  eligibleReview?: EligibleReviewItem | null
}

export interface ProductDetailResponse {
  product: ProductDetail
  reviews: PagedResult<ProductReviewItem>
  relatedProducts: CatalogProduct[]
}
