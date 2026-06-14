interface PaginationProps {
  pageNumber: number
  totalPages: number
  onChange: (page: number) => void
}

const ELLIPSIS = 'ellipsis'

/** Builds a condensed page list: first, last, current ±1, with ellipsis for gaps. */
export function buildPageList(current: number, total: number): Array<number | typeof ELLIPSIS> {
  if (total <= 7) {
    return Array.from({ length: total }, (_, index) => index + 1)
  }

  const pages = new Set<number>([1, total, current, current - 1, current + 1])
  const sorted = [...pages].filter((page) => page >= 1 && page <= total).sort((a, b) => a - b)

  const result: Array<number | typeof ELLIPSIS> = []
  let previous = 0
  for (const page of sorted) {
    if (page - previous > 1) result.push(ELLIPSIS)
    result.push(page)
    previous = page
  }
  return result
}

export default function Pagination({ pageNumber, totalPages, onChange }: PaginationProps) {
  if (totalPages <= 1) return null

  const pages = buildPageList(pageNumber, totalPages)
  const isFirst = pageNumber <= 1
  const isLast = pageNumber >= totalPages

  const baseBtn =
    'inline-flex h-9 min-w-9 items-center justify-center rounded-md px-2 text-sm font-medium transition'

  return (
    <nav className="flex items-center justify-center gap-1" aria-label="Pagination">
      <button
        type="button"
        onClick={() => onChange(pageNumber - 1)}
        disabled={isFirst}
        aria-label="Previous page"
        className={`${baseBtn} text-gray-600 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-40`}
      >
        <span className="material-symbols-outlined text-[18px]">chevron_left</span>
      </button>

      {pages.map((page, index) =>
        page === ELLIPSIS ? (
          <span key={`ellipsis-${index}`} className="px-1 text-gray-400">
            …
          </span>
        ) : (
          <button
            key={page}
            type="button"
            onClick={() => onChange(page)}
            aria-current={page === pageNumber ? 'page' : undefined}
            className={`${baseBtn} ${
              page === pageNumber ? 'bg-primary text-white' : 'text-gray-600 hover:bg-gray-100'
            }`}
          >
            {page}
          </button>
        ),
      )}

      <button
        type="button"
        onClick={() => onChange(pageNumber + 1)}
        disabled={isLast}
        aria-label="Next page"
        className={`${baseBtn} text-gray-600 hover:bg-gray-100 disabled:cursor-not-allowed disabled:opacity-40`}
      >
        <span className="material-symbols-outlined text-[18px]">chevron_right</span>
      </button>
    </nav>
  )
}
