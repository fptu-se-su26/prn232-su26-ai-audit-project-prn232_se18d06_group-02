import { buildPageList, PAGINATION_ELLIPSIS } from '@/lib/pagination'

interface PaginationProps {
  pageNumber: number
  totalPages: number
  onChange: (page: number) => void
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
        page === PAGINATION_ELLIPSIS ? (
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
