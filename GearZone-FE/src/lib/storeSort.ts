export interface StoreSortOption {
  label: string
  value: string
}

export const SORT_OPTIONS: StoreSortOption[] = [
  { label: 'Popular', value: 'popular' },
  { label: 'Newest', value: 'newest' },
  { label: 'Best Selling', value: 'best_selling' },
  { label: 'Price: Low → High', value: 'price_asc' },
  { label: 'Price: High → Low', value: 'price_desc' },
]
