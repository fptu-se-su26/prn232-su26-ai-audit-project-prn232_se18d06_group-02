import apiClient, { unwrap } from '@/api/apiClient'
import type { CatalogProduct, PagedResult } from '@/types/catalog'
import type { FollowToggleResult, StoreProductFilter, StoreProfile } from '@/types/store'

const DEFAULT_PAGE_SIZE = 20

function toParams(filter: StoreProductFilter) {
  const params = new URLSearchParams()

  if (filter.categorySlug) params.set('categorySlug', filter.categorySlug)
  if (typeof filter.minPrice === 'number' && filter.minPrice > 0) {
    params.set('minPrice', String(filter.minPrice))
  }
  if (typeof filter.maxPrice === 'number' && filter.maxPrice > 0) {
    params.set('maxPrice', String(filter.maxPrice))
  }
  if (filter.sortBy && filter.sortBy !== 'popular') params.set('sortBy', filter.sortBy)
  if (filter.pageNumber && filter.pageNumber > 1) params.set('pageNumber', String(filter.pageNumber))
  params.set('pageSize', String(filter.pageSize ?? DEFAULT_PAGE_SIZE))

  return params
}

export async function getStoreProfile(slug: string) {
  const response = await apiClient.get(`/stores/${slug}`)
  return unwrap<StoreProfile>(response)
}

export async function toggleStoreFollow(slug: string) {
  const response = await apiClient.post(`/stores/${slug}/follow`)
  return unwrap<FollowToggleResult>(response)
}

export async function getStoreProducts(slug: string, filter: StoreProductFilter) {
  const response = await apiClient.get(`/stores/${slug}/products`, {
    params: toParams(filter),
  })
  return unwrap<PagedResult<CatalogProduct>>(response)
}

export { toParams as toStoreProductParams }
