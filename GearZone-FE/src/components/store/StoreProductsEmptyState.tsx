import EmptyState from '@/components/ui/EmptyState'

interface StoreProductsEmptyStateProps {
  onReset: () => void
}

export default function StoreProductsEmptyState({ onReset }: StoreProductsEmptyStateProps) {
  return (
    <EmptyState
      icon="inventory_2"
      title="No Products Found"
      description="This store hasn't listed any products matching your filters yet."
      action={
        <button
          type="button"
          onClick={onReset}
          className="rounded-lg bg-secondary px-4 py-2 text-sm font-semibold text-white transition hover:opacity-90"
        >
          View all products
        </button>
      }
    />
  )
}
