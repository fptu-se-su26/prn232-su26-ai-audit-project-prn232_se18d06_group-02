import { describe, expect, it } from 'vitest'
import { buildPageList, PAGINATION_ELLIPSIS } from '@/lib/pagination'

describe('buildPageList', () => {
  it('lists every page when total is small', () => {
    expect(buildPageList(1, 5)).toEqual([1, 2, 3, 4, 5])
  })

  it('adds a trailing ellipsis near the start', () => {
    expect(buildPageList(2, 10)).toEqual([1, 2, 3, PAGINATION_ELLIPSIS, 10])
  })

  it('adds a leading ellipsis near the end', () => {
    expect(buildPageList(9, 10)).toEqual([1, PAGINATION_ELLIPSIS, 8, 9, 10])
  })

  it('adds ellipses on both sides in the middle', () => {
    expect(buildPageList(5, 10)).toEqual([
      1,
      PAGINATION_ELLIPSIS,
      4,
      5,
      6,
      PAGINATION_ELLIPSIS,
      10,
    ])
  })
})
