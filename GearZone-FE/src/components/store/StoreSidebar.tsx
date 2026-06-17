import StoreCategoryFilter from '@/components/store/StoreCategoryFilter'
import StorePriceFilter from '@/components/store/StorePriceFilter'
import ClearFiltersLink from '@/components/store/ClearFiltersLink'
import type { CatalogCategory } from '@/types/catalog'

interface StoreSidebarProps {
  categories: CatalogCategory[]
  categorySlug?: string
  minPrice?: number
  maxPrice?: number
  hasActiveFilters: boolean
  onSelectCategory: (slug: string | null) => void
  onApplyPrice: (min: number | null, max: number | null) => void
  onClear: () => void
}

export default function StoreSidebar({
  categories,
  categorySlug,
  minPrice,
  maxPrice,
  hasActiveFilters,
  onSelectCategory,
  onApplyPrice,
  onClear,
}: StoreSidebarProps) {
  return (
    <aside className="flex flex-col gap-6 rounded-xl border border-gray-200 bg-white p-4">
      <StoreCategoryFilter
        categories={categories}
        activeSlug={categorySlug}
        onSelect={onSelectCategory}
      />
      <StorePriceFilter minPrice={minPrice} maxPrice={maxPrice} onApply={onApplyPrice} />
      {hasActiveFilters && (
        <div className="border-t border-gray-100 pt-3">
          <ClearFiltersLink onClear={onClear} />
        </div>
      )}
    </aside>
  )
}
