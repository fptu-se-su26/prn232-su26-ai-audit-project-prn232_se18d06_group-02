import { useEffect, useState } from 'react'
import { getStoreProducts } from '@/api/stores'
import type { CatalogProduct, PagedResult } from '@/types/catalog'
import type { StoreProductFilter } from '@/types/store'

interface UseStoreProductsResult {
  page: PagedResult<CatalogProduct> | null
  loading: boolean
  error: string | null
}

export function useStoreProducts(
  slug: string | undefined,
  filter: StoreProductFilter,
): UseStoreProductsResult {
  const [page, setPage] = useState<PagedResult<CatalogProduct> | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const { sortBy, categorySlug, minPrice, maxPrice, pageNumber, pageSize } = filter

  useEffect(() => {
    if (!slug) return
    let active = true
    setLoading(true)
    setError(null)

    getStoreProducts(slug, { sortBy, categorySlug, minPrice, maxPrice, pageNumber, pageSize })
      .then((result) => {
        if (active) setPage(result)
      })
      .catch((err: unknown) => {
        if (!active) return
        setError(err instanceof Error ? err.message : 'Failed to load products.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })

    return () => {
      active = false
    }
  }, [slug, sortBy, categorySlug, minPrice, maxPrice, pageNumber, pageSize])

  return { page, loading, error }
}
