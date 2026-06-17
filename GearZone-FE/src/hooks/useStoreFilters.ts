import { useCallback, useMemo } from 'react'
import { useSearchParams } from 'react-router-dom'
import type { StoreProductFilter } from '@/types/store'

export const DEFAULT_SORT = 'popular'

export interface StoreFiltersState extends StoreProductFilter {
  sortBy: string
  pageNumber: number
}

interface UseStoreFiltersResult {
  filter: StoreFiltersState
  hasActiveFilters: boolean
  setSortBy: (value: string) => void
  setCategorySlug: (value: string | null) => void
  setPriceRange: (min: number | null, max: number | null) => void
  setPageNumber: (value: number) => void
  clearFilters: () => void
}

function parsePositiveInt(value: string | null): number | undefined {
  if (!value) return undefined
  const parsed = Number.parseInt(value, 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : undefined
}

export function useStoreFilters(): UseStoreFiltersResult {
  const [searchParams, setSearchParams] = useSearchParams()

  const filter = useMemo<StoreFiltersState>(() => {
    return {
      sortBy: searchParams.get('sortBy') || DEFAULT_SORT,
      categorySlug: searchParams.get('categorySlug') || undefined,
      minPrice: parsePositiveInt(searchParams.get('minPrice')),
      maxPrice: parsePositiveInt(searchParams.get('maxPrice')),
      pageNumber: parsePositiveInt(searchParams.get('pageNumber')) ?? 1,
    }
  }, [searchParams])

  const hasActiveFilters = Boolean(
    filter.categorySlug || filter.minPrice !== undefined || filter.maxPrice !== undefined,
  )

  const update = useCallback(
    (mutate: (params: URLSearchParams) => void, resetPage = true) => {
      setSearchParams(
        (prev) => {
          const next = new URLSearchParams(prev)
          mutate(next)
          if (resetPage) next.delete('pageNumber')
          return next
        },
        { replace: false },
      )
    },
    [setSearchParams],
  )

  const setSortBy = useCallback(
    (value: string) =>
      update((params) => {
        if (value && value !== DEFAULT_SORT) params.set('sortBy', value)
        else params.delete('sortBy')
      }),
    [update],
  )

  const setCategorySlug = useCallback(
    (value: string | null) =>
      update((params) => {
        if (value) params.set('categorySlug', value)
        else params.delete('categorySlug')
      }),
    [update],
  )

  const setPriceRange = useCallback(
    (min: number | null, max: number | null) =>
      update((params) => {
        if (typeof min === 'number' && min > 0) params.set('minPrice', String(min))
        else params.delete('minPrice')
        if (typeof max === 'number' && max > 0) params.set('maxPrice', String(max))
        else params.delete('maxPrice')
      }),
    [update],
  )

  const setPageNumber = useCallback(
    (value: number) =>
      update((params) => {
        if (value > 1) params.set('pageNumber', String(value))
        else params.delete('pageNumber')
      }, false),
    [update],
  )

  const clearFilters = useCallback(
    () =>
      update((params) => {
        params.delete('categorySlug')
        params.delete('minPrice')
        params.delete('maxPrice')
      }),
    [update],
  )

  return {
    filter,
    hasActiveFilters,
    setSortBy,
    setCategorySlug,
    setPriceRange,
    setPageNumber,
    clearFilters,
  }
}
