import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { getCatalogCategories } from '@/api/catalog'
import StoreHeader from '@/components/store/StoreHeader'
import StoreSortTabs from '@/components/store/StoreSortTabs'
import StoreSidebar from '@/components/store/StoreSidebar'
import StoreProductGrid from '@/components/store/StoreProductGrid'
import StoreProductsEmptyState from '@/components/store/StoreProductsEmptyState'
import StoreNotFound from '@/components/store/StoreNotFound'
import Pagination from '@/components/ui/Pagination'
import LoadingOverlay from '@/components/ui/LoadingOverlay'
import ErrorState from '@/components/ui/ErrorState'
import { useStoreProfile } from '@/hooks/useStoreProfile'
import { useStoreFilters } from '@/hooks/useStoreFilters'
import { useStoreProducts } from '@/hooks/useStoreProducts'
import { useStoreFollow } from '@/hooks/useStoreFollow'
import type { CatalogCategory } from '@/types/catalog'

export default function StoreProfilePage() {
  const { slug } = useParams<{ slug: string }>()
  const { store, loading: profileLoading, error: profileError, notFound } = useStoreProfile(slug)
  const {
    filter,
    hasActiveFilters,
    setSortBy,
    setCategorySlug,
    setPriceRange,
    setPageNumber,
    clearFilters,
  } = useStoreFilters()
  const { page, loading: productsLoading, error: productsError } = useStoreProducts(slug, filter)
  const { isFollowing, followerCount, pending: followPending, toggle } = useStoreFollow(store)

  const [categories, setCategories] = useState<CatalogCategory[]>([])
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false)

  useEffect(() => {
    let active = true
    getCatalogCategories()
      .then((result) => {
        if (active) setCategories(result)
      })
      .catch(() => {
        if (active) setCategories([])
      })
    return () => {
      active = false
    }
  }, [])

  if (notFound) return <StoreNotFound />

  if (profileLoading && !store) {
    return (
      <div className="flex min-h-[60vh] items-center justify-center">
        <span className="material-symbols-outlined animate-spin text-[28px] text-secondary">
          progress_activity
        </span>
      </div>
    )
  }

  if (profileError || !store) {
    return <ErrorState message={profileError ?? 'Failed to load store.'} />
  }

  const products = page?.items ?? []
  const totalCount = page?.totalCount ?? 0
  const totalPages = page?.totalPages ?? 0

  const sidebar = (
    <StoreSidebar
      categories={categories}
      categorySlug={filter.categorySlug}
      minPrice={filter.minPrice}
      maxPrice={filter.maxPrice}
      hasActiveFilters={hasActiveFilters}
      onSelectCategory={setCategorySlug}
      onApplyPrice={setPriceRange}
      onClear={clearFilters}
    />
  )

  return (
    <div className="bg-gray-50">
      <StoreHeader
        store={store}
        isFollowing={isFollowing}
        followerCount={followerCount}
        followPending={followPending}
        onToggleFollow={toggle}
      />
      <StoreSortTabs sortBy={filter.sortBy} totalCount={totalCount} onChange={setSortBy} />

      <main className="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-6 lg:flex-row">
          <div className="hidden w-64 shrink-0 lg:block">
            <div className="sticky top-20">{sidebar}</div>
          </div>

          <div className="min-w-0 flex-1">
            <div className="mb-4 lg:hidden">
              <button
                type="button"
                onClick={() => setMobileFiltersOpen((open) => !open)}
                aria-expanded={mobileFiltersOpen}
                className="inline-flex items-center gap-1.5 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm font-semibold text-gray-700"
              >
                <span className="material-symbols-outlined text-[18px]">tune</span>
                Filters
              </button>
              {mobileFiltersOpen && <div className="mt-3">{sidebar}</div>}
            </div>

            {productsError ? (
              <ErrorState message={productsError} />
            ) : !productsLoading && products.length === 0 ? (
              <StoreProductsEmptyState onReset={clearFilters} />
            ) : (
              <LoadingOverlay loading={productsLoading}>
                <StoreProductGrid products={products} />
                {totalPages > 1 && (
                  <div className="mt-8">
                    <Pagination
                      pageNumber={filter.pageNumber}
                      totalPages={totalPages}
                      onChange={setPageNumber}
                    />
                  </div>
                )}
              </LoadingOverlay>
            )}
          </div>
        </div>
      </main>
    </div>
  )
}
