import apiClient, { unwrap } from '@/api/apiClient'
import type {
  CatalogCategory,
  CatalogFilterSidebar,
  CatalogProduct,
  PagedResult,
  ProductBrowseFilter,
  ProductSuggestion,
} from '@/types/catalog'

function appendArray(params: URLSearchParams, key: string, values?: string[]) {
  values?.forEach((value) => {
    if (value) params.append(key, value)
  })
}

function appendAttributes(params: URLSearchParams, attributes?: Record<string, string[]>) {
  Object.entries(attributes ?? {}).forEach(([key, values]) => {
    values.forEach((value) => {
      if (value) params.append(key, value)
    })
  })
}

function toParams(filter: ProductBrowseFilter) {
  const params = new URLSearchParams()

  if (filter.search) params.set('search', filter.search)
  if (filter.categorySlug) params.set('categorySlug', filter.categorySlug)
  appendArray(params, 'brand', filter.brandSlugs)
  if (typeof filter.minPrice === 'number' && filter.minPrice > 0) params.set('minPrice', String(filter.minPrice))
  if (typeof filter.maxPrice === 'number' && filter.maxPrice > 0) params.set('maxPrice', String(filter.maxPrice))
  if (filter.inStockOnly) params.set('inStockOnly', 'true')
  if (filter.sortBy && filter.sortBy !== 'popular') params.set('sortBy', filter.sortBy)
  if (filter.viewMode && filter.viewMode !== 'grid') params.set('viewMode', filter.viewMode)
  if (filter.pageNumber && filter.pageNumber > 1) params.set('pageNumber', String(filter.pageNumber))
  if (filter.pageSize) params.set('pageSize', String(filter.pageSize))

  appendAttributes(params, filter.attributes)

  return params
}

export async function getCatalogCategories() {
  const response = await apiClient.get('/catalog/categories')
  return unwrap<CatalogCategory[]>(response)
}

export async function getCatalogFilters(slug?: string) {
  const response = await apiClient.get('/catalog/filters', {
    params: slug ? { slug } : undefined,
  })
  return unwrap<CatalogFilterSidebar>(response)
}

export async function getProducts(filter: ProductBrowseFilter) {
  const response = await apiClient.get('/products', {
    params: toParams(filter),
  })
  return unwrap<PagedResult<CatalogProduct>>(response)
}

export async function getProductSuggestions(query: string) {
  const response = await apiClient.get('/products/suggestions', {
    params: { query },
  })
  return unwrap<ProductSuggestion[]>(response)
}
