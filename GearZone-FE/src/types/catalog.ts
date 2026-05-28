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
