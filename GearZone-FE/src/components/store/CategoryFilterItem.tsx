import type { CatalogCategory } from '@/types/catalog'

interface CategoryFilterItemProps {
  category: CatalogCategory
  activeSlug?: string
  onSelect: (slug: string) => void
  depth?: number
}

export default function CategoryFilterItem({
  category,
  activeSlug,
  onSelect,
  depth = 0,
}: CategoryFilterItemProps) {
  const active = category.slug === activeSlug

  return (
    <li>
      <button
        type="button"
        onClick={() => onSelect(category.slug)}
        aria-current={active ? 'true' : undefined}
        style={{ paddingLeft: `${0.5 + depth * 0.75}rem` }}
        className={`flex w-full items-center rounded-md py-1.5 pr-2 text-left text-sm transition ${
          active ? 'bg-orange-50 font-semibold text-secondary' : 'text-gray-600 hover:bg-gray-50'
        }`}
      >
        {category.name}
      </button>
      {category.subCategories?.length > 0 && (
        <ul className="mt-0.5 space-y-0.5">
          {category.subCategories.map((sub) => (
            <CategoryFilterItem
              key={sub.id}
              category={sub}
              activeSlug={activeSlug}
              onSelect={onSelect}
              depth={depth + 1}
            />
          ))}
        </ul>
      )}
    </li>
  )
}
