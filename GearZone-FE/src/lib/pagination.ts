export const PAGINATION_ELLIPSIS = 'ellipsis'

export type PageListEntry = number | typeof PAGINATION_ELLIPSIS

/** Builds a condensed page list: first, last, current ±1, with ellipsis for gaps. */
export function buildPageList(current: number, total: number): PageListEntry[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, index) => index + 1)
  }

  const pages = new Set<number>([1, total, current, current - 1, current + 1])
  const sorted = [...pages].filter((page) => page >= 1 && page <= total).sort((a, b) => a - b)

  const result: PageListEntry[] = []
  let previous = 0
  for (const page of sorted) {
    if (page - previous > 1) result.push(PAGINATION_ELLIPSIS)
    result.push(page)
    previous = page
  }
  return result
}
