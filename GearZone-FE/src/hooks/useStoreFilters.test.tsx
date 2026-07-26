import type { ReactNode } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { act, renderHook } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { useStoreFilters } from '@/hooks/useStoreFilters'

function wrapper({ children }: { children: ReactNode }) {
  return <MemoryRouter initialEntries={['/store/acme']}>{children}</MemoryRouter>
}

describe('useStoreFilters', () => {
  it('defaults sortBy to popular and page to 1', () => {
    const { result } = renderHook(() => useStoreFilters(), { wrapper })
    expect(result.current.filter.sortBy).toBe('popular')
    expect(result.current.filter.pageNumber).toBe(1)
    expect(result.current.hasActiveFilters).toBe(false)
  })

  it('round-trips sort, category and price through the URL', () => {
    const { result } = renderHook(() => useStoreFilters(), { wrapper })

    act(() => result.current.setSortBy('newest'))
    expect(result.current.filter.sortBy).toBe('newest')

    act(() => result.current.setCategorySlug('keyboards'))
    expect(result.current.filter.categorySlug).toBe('keyboards')

    act(() => result.current.setPriceRange(100, 500))
    expect(result.current.filter.minPrice).toBe(100)
    expect(result.current.filter.maxPrice).toBe(500)
    expect(result.current.hasActiveFilters).toBe(true)
  })

  it('resets page to 1 when a filter changes', () => {
    const { result } = renderHook(() => useStoreFilters(), { wrapper })

    act(() => result.current.setPageNumber(4))
    expect(result.current.filter.pageNumber).toBe(4)

    act(() => result.current.setCategorySlug('mice'))
    expect(result.current.filter.pageNumber).toBe(1)
  })

  it('clears all filters', () => {
    const { result } = renderHook(() => useStoreFilters(), { wrapper })

    act(() => result.current.setCategorySlug('mice'))
    act(() => result.current.setPriceRange(100, 500))
    act(() => result.current.clearFilters())

    expect(result.current.filter.categorySlug).toBeUndefined()
    expect(result.current.filter.minPrice).toBeUndefined()
    expect(result.current.filter.maxPrice).toBeUndefined()
    expect(result.current.hasActiveFilters).toBe(false)
  })
})
