import { describe, expect, it } from 'vitest'
import { toStoreProductParams } from '@/api/stores'

describe('toStoreProductParams', () => {
  it('always sets the default page size', () => {
    const params = toStoreProductParams({})
    expect(params.get('pageSize')).toBe('20')
  })

  it('omits the default sort and first page', () => {
    const params = toStoreProductParams({ sortBy: 'popular', pageNumber: 1 })
    expect(params.get('sortBy')).toBeNull()
    expect(params.get('pageNumber')).toBeNull()
  })

  it('emits non-default sort, category and page', () => {
    const params = toStoreProductParams({
      sortBy: 'price_asc',
      categorySlug: 'keyboards',
      pageNumber: 3,
    })
    expect(params.get('sortBy')).toBe('price_asc')
    expect(params.get('categorySlug')).toBe('keyboards')
    expect(params.get('pageNumber')).toBe('3')
  })

  it('ignores non-positive price bounds', () => {
    const params = toStoreProductParams({ minPrice: 0, maxPrice: -5 })
    expect(params.get('minPrice')).toBeNull()
    expect(params.get('maxPrice')).toBeNull()
  })

  it('emits positive price bounds', () => {
    const params = toStoreProductParams({ minPrice: 100, maxPrice: 999 })
    expect(params.get('minPrice')).toBe('100')
    expect(params.get('maxPrice')).toBe('999')
  })
})
