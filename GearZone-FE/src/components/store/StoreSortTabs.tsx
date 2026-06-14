import { formatCount } from '@/lib/format'

export const SORT_OPTIONS: Array<{ label: string; value: string }> = [
  { label: 'Popular', value: 'popular' },
  { label: 'Newest', value: 'newest' },
  { label: 'Best Selling', value: 'best_selling' },
  { label: 'Price: Low → High', value: 'price_asc' },
  { label: 'Price: High → Low', value: 'price_desc' },
]

interface StoreSortTabsProps {
  sortBy: string
  totalCount: number
  onChange: (value: string) => void
}

export default function StoreSortTabs({ sortBy, totalCount, onChange }: StoreSortTabsProps) {
  return (
    <div className="sticky top-0 z-20 border-b border-gray-200 bg-white/95 backdrop-blur">
      <div className="mx-auto flex max-w-7xl flex-col gap-2 px-4 py-3 sm:px-6 md:flex-row md:items-center md:justify-between lg:px-8">
        <div className="-mx-1 flex items-center gap-1 overflow-x-auto">
          {SORT_OPTIONS.map((option) => {
            const active = option.value === sortBy
            return (
              <button
                key={option.value}
                type="button"
                onClick={() => onChange(option.value)}
                aria-pressed={active}
                className={`shrink-0 whitespace-nowrap rounded-full px-3 py-1.5 text-sm font-medium transition ${
                  active ? 'bg-primary text-white' : 'text-gray-600 hover:bg-gray-100'
                }`}
              >
                {option.label}
              </button>
            )
          })}
        </div>
        <span className="shrink-0 text-sm text-gray-500">{formatCount(totalCount)} products</span>
      </div>
    </div>
  )
}
