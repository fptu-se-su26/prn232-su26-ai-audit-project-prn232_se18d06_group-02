import CategoryFilterItem from '@/components/store/CategoryFilterItem'
import type { CatalogCategory } from '@/types/catalog'

interface StoreCategoryFilterProps {
  categories: CatalogCategory[]
  activeSlug?: string
  onSelect: (slug: string | null) => void
}

export default function StoreCategoryFilter({
  categories,
  activeSlug,
  onSelect,
}: StoreCategoryFilterProps) {
  return (
    <section>
      <h2 className="mb-2 text-sm font-bold uppercase tracking-wide text-gray-700">Categories</h2>
      <ul className="space-y-0.5">
        <li>
          <button
            type="button"
            onClick={() => onSelect(null)}
            aria-current={!activeSlug ? 'true' : undefined}
            className={`flex w-full items-center rounded-md px-2 py-1.5 text-left text-sm transition ${
              !activeSlug ? 'bg-orange-50 font-semibold text-secondary' : 'text-gray-600 hover:bg-gray-50'
            }`}
          >
            All Products
          </button>
        </li>
        {categories.map((category) => (
          <CategoryFilterItem
            key={category.id}
            category={category}
            activeSlug={activeSlug}
            onSelect={(slug) => onSelect(slug)}
          />
        ))}
      </ul>
    </section>
  )
}
