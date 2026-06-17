import type { ProductBrowseFilter } from '@/types/catalog'

export interface StoreReviewSummary {
  averageRating: number
  totalReviews: number
}

export interface StoreProfile {
  id: string
  storeName: string
  slug: string
  description?: string | null
  logoUrl?: string | null
  province: string
  productCount: number
  totalSold: number
  rating: number
  reviewCount: number
  followerCount: number
  isFollowing: boolean
  createdAt: string
  reviewSummary: StoreReviewSummary
}

export interface FollowToggleResult {
  isFollowing: boolean
  followerCount: number
}

export type StoreProductFilter = Pick<
  ProductBrowseFilter,
  'sortBy' | 'categorySlug' | 'minPrice' | 'maxPrice' | 'pageNumber' | 'pageSize'
>
