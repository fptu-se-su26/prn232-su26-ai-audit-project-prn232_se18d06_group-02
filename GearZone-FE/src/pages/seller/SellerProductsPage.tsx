import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface Product {
  id: string
  name: string
  slug?: string
  categoryId?: number
  categoryName?: string
  brandId?: number
  brandName?: string
  basePrice: number
  totalStock?: number
  stockQuantity?: number
  status: string
  primaryImageUrl?: string
  imageUrl?: string
  createdAt?: string
}

interface ProductStats {
  total?: number
  active?: number
  outOfStock?: number
  draft?: number
  pending?: number
}

interface ProductListResult {
  stats?: ProductStats
  totalCount?: number
  items?: Product[]
  page?: number
  pageSize?: number
  totalPages?: number
}

interface MetadataItem {
  id: number
  name: string
}

interface MetadataResult {
  categories?: MetadataItem[]
  brands?: MetadataItem[]
}

const STATUS_TABS = [
  { label: 'All Products', value: '' },
  { label: 'Active', value: 'Active' },
  { label: 'Draft', value: 'Draft' },
  { label: 'Pending', value: 'Pending' },
  { label: 'Inactive', value: 'Inactive' },
  { label: 'Rejected', value: 'Rejected' },
]

const PAGE_SIZE = 10

function formatNumber(value?: number) {
  return new Intl.NumberFormat('en-US').format(value ?? 0)
}

function formatPrice(value?: number) {
  return `${formatNumber(value)} ₫`
}

function productStock(product: Product) {
  return product.totalStock ?? product.stockQuantity ?? 0
}

function statusClass(status: string) {
  switch (status) {
    case 'Active':
      return 'bg-green-50 text-green-700 ring-green-600/20'
    case 'Inactive':
      return 'bg-slate-100 text-slate-600 ring-slate-600/10'
    case 'Draft':
      return 'bg-blue-50 text-blue-700 ring-blue-600/20'
    case 'Pending':
      return 'bg-amber-50 text-amber-700 ring-amber-600/20'
    case 'Rejected':
      return 'bg-red-50 text-red-700 ring-red-600/20'
    default:
      return 'bg-slate-50 text-slate-700 ring-slate-600/20'
  }
}

function statusDotClass(status: string) {
  switch (status) {
    case 'Active':
      return 'bg-green-600'
    case 'Inactive':
      return 'bg-slate-400'
    case 'Draft':
      return 'bg-blue-600'
    case 'Pending':
      return 'bg-amber-500'
    case 'Rejected':
      return 'bg-red-600'
    default:
      return 'bg-slate-600'
  }
}

function stockClass(stock: number) {
  if (stock === 0) return 'text-red-600 font-bold'
  if (stock < 10) return 'text-amber-600 font-bold'
  return 'text-slate-600 font-medium'
}

function sortIcon(sortBy: string, sortDirection: string, column: string) {
  if (sortBy !== column) return 'unfold_more'
  if (sortDirection === 'asc') return 'arrow_upward'
  if (sortDirection === 'desc') return 'arrow_downward'
  return 'unfold_more'
}

function sortIconClass(sortBy: string, column: string) {
  return sortBy === column ? 'text-primary' : 'text-slate-300 group-hover:text-slate-400'
}

function getPageNumbers(page: number, totalPages: number) {
  let startPage = Math.max(1, page - 2)
  let endPage = Math.min(totalPages, startPage + 4)

  if (endPage - startPage < 4 && startPage > 1) {
    startPage = Math.max(1, endPage - 4)
  }

  return Array.from({ length: endPage - startPage + 1 }, (_, index) => startPage + index)
}

export default function SellerProductsPage() {
  const [products, setProducts] = useState<Product[]>([])
  const [stats, setStats] = useState<ProductStats>({})
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [categories, setCategories] = useState<MetadataItem[]>([])
  const [brands, setBrands] = useState<MetadataItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [status, setStatus] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [brandId, setBrandId] = useState('')
  const [sortBy, setSortBy] = useState('createdAt')
  const [sortDirection, setSortDirection] = useState('desc')
  const [page, setPage] = useState(1)
  const [advancedOpen, setAdvancedOpen] = useState(false)
  const [openMenuId, setOpenMenuId] = useState<string | null>(null)
  const [togglingId, setTogglingId] = useState<string | null>(null)
  const menuRef = useRef<HTMLDivElement | null>(null)

  const advancedActiveCount = Number(Boolean(categoryId)) + Number(Boolean(brandId))

  const loadProducts = () => {
    setLoading(true)
    setError(null)
    sellerApi.products
      .list({
        searchTerm: searchTerm || undefined,
        status: status || undefined,
        categoryId: categoryId ? Number(categoryId) : undefined,
        brandId: brandId ? Number(brandId) : undefined,
        sortBy,
        sortDir: sortDirection,
        page,
        pageSize: PAGE_SIZE,
      })
      .then((result) => {
        const data = result as ProductListResult
        setProducts(data.items ?? [])
        setStats(data.stats ?? {})
        setTotalCount(data.totalCount ?? 0)
        setTotalPages(Math.max(1, data.totalPages ?? 1))
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Failed to load products.')
      })
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    sellerApi.products
      .metadata()
      .then((result) => {
        const data = result as MetadataResult
        setCategories(data.categories ?? [])
        setBrands(data.brands ?? [])
      })
      .catch(() => {
        setCategories([])
        setBrands([])
      })
  }, [])

  useEffect(() => {
    loadProducts()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchTerm, status, categoryId, brandId, sortBy, sortDirection, page])

  useEffect(() => {
    const handleClick = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setOpenMenuId(null)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  const showingFrom = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1
  const showingTo = Math.min(page * PAGE_SIZE, totalCount)

  const pageNumbers = useMemo(() => getPageNumbers(page, totalPages), [page, totalPages])

  const submitSearch = (event: FormEvent) => {
    event.preventDefault()
    setPage(1)
    setSearchTerm(searchInput.trim())
  }

  const setStatusAndSubmit = (nextStatus: string) => {
    setStatus(nextStatus)
    setPage(1)
  }

  const resetFilters = () => {
    setSearchInput('')
    setSearchTerm('')
    setStatus('')
    setCategoryId('')
    setBrandId('')
    setSortBy('createdAt')
    setSortDirection('desc')
    setPage(1)
    setAdvancedOpen(false)
  }

  const handleSort = (column: string) => {
    let nextSortBy = column
    let nextDirection = 'desc'

    if (sortBy === column) {
      if (sortDirection === 'desc') {
        nextDirection = 'asc'
      } else if (sortDirection === 'asc') {
        nextSortBy = ''
        nextDirection = ''
      }
    }

    setSortBy(nextSortBy || 'createdAt')
    setSortDirection(nextDirection || 'desc')
    setPage(1)
  }

  const applyAdvancedFilters = () => {
    setPage(1)
    loadProducts()
  }

  const toggleProductStatus = async (product: Product) => {
    const isActiveLike = product.status === 'Active' || product.status === 'Approved'
    const action = isActiveLike ? 'deactivate' : 'activate'
    if (!window.confirm(`Are you sure you want to ${action} this product?`)) return

    setTogglingId(product.id)
    try {
      await sellerApi.products.toggleStatus(product.id)
      setOpenMenuId(null)
      loadProducts()
    } finally {
      setTogglingId(null)
    }
  }

  return (
    <SellerLayout pageHeader="Product Management" breadcrumb={['Products']}>
      <div className="flex flex-col gap-6">
        <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-center">
          <div>
            <h1 className="text-2xl font-bold text-slate-900">Products</h1>
            <p className="text-sm font-medium text-slate-500">
              Manage your store's product catalog and inventory.
            </p>
          </div>
          <Link
            to="/store-owner/products/create"
            className="seller-primary-button inline-flex items-center justify-center gap-2 rounded-xl bg-primary px-5 py-2.5 font-semibold text-white shadow-sm shadow-blue-500/25 transition-all hover:bg-blue-700"
          >
            <span className="material-symbols-outlined text-[20px]">add_circle</span>
            <span>Add New Product</span>
          </Link>
        </div>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
            <div className="flex size-12 items-center justify-center rounded-lg border border-blue-100/50 bg-blue-50 text-blue-600">
              <span className="material-symbols-outlined">inventory_2</span>
            </div>
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Total Products
              </p>
              <h3 className="text-2xl font-bold text-slate-900">{formatNumber(stats.total)}</h3>
            </div>
          </div>

          <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
            <div className="flex size-12 items-center justify-center rounded-lg border border-green-100/50 bg-green-50 text-green-600">
              <span className="material-symbols-outlined">check_circle</span>
            </div>
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Active Products
              </p>
              <h3 className="text-2xl font-bold text-slate-900">{formatNumber(stats.active)}</h3>
            </div>
          </div>

          <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
            <div className="flex size-12 items-center justify-center rounded-lg border border-red-100/50 bg-red-50 text-red-600">
              <span className="material-symbols-outlined">running_with_errors</span>
            </div>
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Out of Stock
              </p>
              <h3 className="text-2xl font-bold text-slate-900">
                {formatNumber(stats.outOfStock)}
              </h3>
            </div>
          </div>

          <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
            <div className="flex size-12 items-center justify-center rounded-lg border border-amber-100/50 bg-amber-50 text-amber-600">
              <span className="material-symbols-outlined">pending</span>
            </div>
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Pending/Draft
              </p>
              <h3 className="text-2xl font-bold text-slate-900">
                {formatNumber((stats.pending ?? 0) + (stats.draft ?? 0))}
              </h3>
            </div>
          </div>
        </div>

        <form className="flex flex-col gap-4" onSubmit={submitSearch}>
          <div className="no-scrollbar flex overflow-x-auto border-b border-slate-200">
            {STATUS_TABS.map((tab) => {
              const active = status === tab.value
              return (
                <button
                  key={tab.label}
                  type="button"
                  onClick={() => setStatusAndSubmit(tab.value)}
                  className={
                    active
                      ? 'whitespace-nowrap border-b-2 border-primary px-6 py-3 text-sm font-bold text-primary'
                      : 'whitespace-nowrap border-b-2 border-transparent px-6 py-3 text-sm font-semibold text-slate-500 transition-all hover:bg-slate-50 hover:text-slate-700'
                  }
                >
                  {tab.label}
                </button>
              )
            })}
          </div>

          <div className="flex flex-col overflow-hidden rounded-b-xl border border-slate-200 bg-white shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
            <div className="flex flex-col items-start gap-3 p-4 lg:flex-row lg:items-center">
              <div className="relative w-full flex-1">
                <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-400">
                  <span className="material-symbols-outlined text-[20px]">search</span>
                </span>
                <input
                  value={searchInput}
                  onChange={(event) => setSearchInput(event.target.value)}
                  className="w-full rounded-lg border border-slate-200 bg-slate-50 py-2.5 pl-10 pr-4 text-sm text-slate-900 transition-colors placeholder:text-slate-400 focus:border-primary focus:bg-white focus:outline-none focus:ring-1 focus:ring-primary"
                  placeholder="Search by name, category, brand..."
                  autoComplete="off"
                />
              </div>

              <div className="flex w-full shrink-0 items-center gap-2 lg:w-auto">
                <button
                  type="submit"
                  className="seller-primary-button flex flex-1 items-center justify-center gap-2 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-white shadow-sm shadow-blue-500/20 transition-all hover:bg-blue-700 lg:flex-none"
                >
                  <span className="material-symbols-outlined text-[18px]">search</span>
                  <span>Search</span>
                </button>

                <button
                  type="button"
                  onClick={() => setAdvancedOpen((open) => !open)}
                  className={`flex items-center justify-center gap-1.5 rounded-lg border px-3.5 py-2.5 text-sm shadow-sm transition-colors ${
                    advancedOpen || advancedActiveCount > 0
                      ? 'border-slate-300 bg-slate-50 text-slate-900'
                      : 'border-slate-200 bg-white text-slate-600 hover:border-slate-300 hover:bg-slate-50'
                  }`}
                >
                  <span className="material-symbols-outlined text-[18px]">tune</span>
                  <span className="hidden font-medium sm:inline">Filters</span>
                  {advancedActiveCount > 0 && (
                    <span className="inline-flex size-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold text-white">
                      {advancedActiveCount}
                    </span>
                  )}
                </button>

                <button
                  type="button"
                  onClick={resetFilters}
                  title="Reset all filters"
                  className="flex items-center justify-center rounded-lg border border-slate-200 bg-white p-2.5 text-slate-500 shadow-sm transition-colors hover:bg-slate-50 hover:text-slate-800"
                >
                  <span className="material-symbols-outlined text-[18px]">restart_alt</span>
                </button>
              </div>
            </div>

            {(advancedOpen || advancedActiveCount > 0) && (
              <div className="flex flex-col gap-5 border-t border-slate-100 bg-slate-50/40 px-4 pb-5 pt-4">
                <div className="flex flex-wrap items-start gap-4 lg:flex-nowrap lg:gap-6">
                  <div className="w-full space-y-1 lg:w-64 lg:shrink-0">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">category</span>
                      Category
                    </label>
                    <select
                      value={categoryId}
                      onChange={(event) => setCategoryId(event.target.value)}
                      className="h-10 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                    >
                      <option value="">All Categories</option>
                      {categories.map((category) => (
                        <option key={category.id} value={category.id}>
                          {category.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="w-full space-y-1 lg:w-64 lg:shrink-0">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">brand_family</span>
                      Brand
                    </label>
                    <select
                      value={brandId}
                      onChange={(event) => setBrandId(event.target.value)}
                      className="h-10 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                    >
                      <option value="">All Brands</option>
                      {brands.map((brand) => (
                        <option key={brand.id} value={brand.id}>
                          {brand.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="mt-2 w-full lg:ms-auto lg:mt-[22px] lg:w-auto">
                    <button
                      type="button"
                      onClick={applyAdvancedFilters}
                      className="flex h-10 w-full items-center justify-center gap-2 rounded-lg bg-slate-800 px-6 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-900 sm:w-auto"
                    >
                      <span className="material-symbols-outlined text-[17px]">check</span>
                      Apply Filters
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        </form>

        <div className="flex flex-col overflow-hidden rounded-xl border border-slate-200 bg-white shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-left">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50/50">
                  <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button
                      type="button"
                      onClick={() => handleSort('name')}
                      className="group inline-flex w-full items-center gap-1 transition-colors hover:text-primary"
                    >
                      Product
                      <span
                        className={`material-symbols-outlined text-[16px] transition-all ${sortIconClass(sortBy, 'name')}`}
                      >
                        {sortIcon(sortBy, sortDirection, 'name')}
                      </span>
                    </button>
                  </th>
                  <th className="hidden px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">
                    Category / Brand
                  </th>
                  <th className="px-3 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button
                      type="button"
                      onClick={() => handleSort('price')}
                      className="group inline-flex w-full items-center justify-end gap-1 transition-colors hover:text-primary"
                    >
                      Price
                      <span
                        className={`material-symbols-outlined text-[16px] transition-all ${sortIconClass(sortBy, 'price')}`}
                      >
                        {sortIcon(sortBy, sortDirection, 'price')}
                      </span>
                    </button>
                  </th>
                  <th className="px-3 py-4 text-center text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button
                      type="button"
                      onClick={() => handleSort('stock')}
                      className="group inline-flex w-full items-center justify-center gap-1 transition-colors hover:text-primary"
                    >
                      Stock
                      <span
                        className={`material-symbols-outlined text-[16px] transition-all ${sortIconClass(sortBy, 'stock')}`}
                      >
                        {sortIcon(sortBy, sortDirection, 'stock')}
                      </span>
                    </button>
                  </th>
                  <th className="px-3 py-4 text-center text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Status
                  </th>
                  <th className="px-6 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  Array.from({ length: 6 }).map((_, index) => (
                    <tr key={index}>
                      <td colSpan={6} className="px-6 py-4">
                        <div className="h-12 animate-pulse rounded-lg bg-slate-100" />
                      </td>
                    </tr>
                  ))
                ) : error ? (
                  <tr>
                    <td colSpan={6} className="px-6 py-12 text-center text-red-600">
                      {error}
                    </td>
                  </tr>
                ) : products.length > 0 ? (
                  products.map((product) => {
                    const stock = productStock(product)
                    const canToggle =
                      product.status === 'Active' ||
                      product.status === 'Approved' ||
                      product.status === 'Inactive'
                    const isActiveLike = product.status === 'Active' || product.status === 'Approved'

                    return (
                      <tr
                        key={product.id}
                        className="group/row cursor-pointer transition-all hover:bg-slate-50"
                        onClick={() => {
                          window.location.href = `/store-owner/products/details/${product.id}`
                        }}
                      >
                        <td className="px-6 py-4 align-middle">
                          <div className="flex items-center gap-3">
                            <div className="size-12 shrink-0 overflow-hidden rounded-lg border border-slate-100 bg-slate-50">
                              {product.primaryImageUrl || product.imageUrl ? (
                                <img
                                  src={product.primaryImageUrl ?? product.imageUrl}
                                  className="size-full object-cover"
                                  alt=""
                                />
                              ) : (
                                <div className="flex size-full items-center justify-center text-slate-300">
                                  <span className="material-symbols-outlined text-[24px]">image</span>
                                </div>
                              )}
                            </div>
                            <div className="min-w-0">
                              <p className="truncate text-sm font-bold text-slate-900 transition-colors group-hover/row:text-primary">
                                {product.name}
                              </p>
                              <p className="font-mono text-[11px] text-slate-400">
                                ID: {product.id.slice(0, 8)}
                              </p>
                            </div>
                          </div>
                        </td>
                        <td className="hidden whitespace-nowrap px-3 py-4 align-middle md:table-cell">
                          <p className="text-sm font-semibold text-slate-700">
                            {product.categoryName}
                          </p>
                          <p className="text-xs text-slate-500">{product.brandName}</p>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-right text-sm font-bold text-slate-900">
                          {formatPrice(product.basePrice)}
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-center">
                          <span className={`text-sm ${stockClass(stock)}`}>
                            {formatNumber(stock)}
                          </span>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-center">
                          <span
                            className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1.5 text-xs font-semibold ring-1 ring-inset ${statusClass(product.status)}`}
                          >
                            <span
                              className={`size-1.5 rounded-full ${statusDotClass(product.status)}`}
                            />
                            {product.status}
                          </span>
                        </td>
                        <td className="whitespace-nowrap px-6 py-4 text-right align-middle">
                          <div className="relative inline-block text-left" ref={openMenuId === product.id ? menuRef : null}>
                            <button
                              type="button"
                              title="Actions"
                              onClick={(event) => {
                                event.stopPropagation()
                                setOpenMenuId((current) => (current === product.id ? null : product.id))
                              }}
                              className={`rounded-lg p-2 transition-all ${
                                openMenuId === product.id
                                  ? 'bg-slate-100 text-slate-600'
                                  : 'text-slate-400 hover:bg-slate-100 hover:text-slate-600'
                              }`}
                            >
                              <span className="material-symbols-outlined text-[20px]">more_vert</span>
                            </button>
                            {openMenuId === product.id && (
                              <div
                                className="absolute right-0 z-50 mt-2 w-48 overflow-hidden rounded-xl bg-white shadow-lg ring-1 ring-black/5"
                                onClick={(event) => event.stopPropagation()}
                              >
                                <div className="divide-y divide-slate-100">
                                  <div className="py-1">
                                    <Link
                                      to={`/store-owner/products/details/${product.id}`}
                                      className="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-50"
                                    >
                                      <span className="material-symbols-outlined text-[18px] text-slate-400">
                                        visibility
                                      </span>
                                      View Details
                                    </Link>
                                    <Link
                                      to={`/store-owner/products/edit/${product.id}`}
                                      className="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-50"
                                    >
                                      <span className="material-symbols-outlined text-[18px] text-slate-400">
                                        edit
                                      </span>
                                      Edit Product
                                    </Link>
                                    <Link
                                      to={`/store-owner/products/create?copyFromId=${product.id}`}
                                      className="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-50"
                                    >
                                      <span className="material-symbols-outlined text-[18px] text-slate-400">
                                        content_copy
                                      </span>
                                      Copy Product
                                    </Link>
                                    {isActiveLike ? (
                                      <a
                                        href={`/product/${product.slug ?? product.id}`}
                                        target="_blank"
                                        rel="noreferrer"
                                        className="flex items-center gap-2 px-4 py-2 text-sm text-emerald-600 transition-colors hover:bg-emerald-50"
                                      >
                                        <span className="material-symbols-outlined text-[18px]">
                                          open_in_new
                                        </span>
                                        View Live
                                      </a>
                                    ) : (
                                      <Link
                                        to={`/product/${product.slug ?? product.id}`}
                                        className="flex items-center gap-2 px-4 py-2 text-sm text-blue-600 transition-colors hover:bg-blue-50"
                                      >
                                        <span className="material-symbols-outlined text-[18px]">
                                          visibility
                                        </span>
                                        Live Preview
                                      </Link>
                                    )}
                                  </div>

                                  {canToggle && (
                                    <div className="py-1">
                                      <button
                                        type="button"
                                        disabled={togglingId === product.id}
                                        onClick={() => void toggleProductStatus(product)}
                                        className={`flex w-full items-center gap-2 px-4 py-2 text-sm transition-colors hover:bg-slate-50 disabled:opacity-60 ${
                                          isActiveLike ? 'text-red-600' : 'text-green-600'
                                        }`}
                                      >
                                        <span className="material-symbols-outlined text-[18px]">
                                          {isActiveLike ? 'toggle_off' : 'toggle_on'}
                                        </span>
                                        {isActiveLike ? 'Deactivate' : 'Activate'} Product
                                      </button>
                                    </div>
                                  )}
                                </div>
                              </div>
                            )}
                          </div>
                        </td>
                      </tr>
                    )
                  })
                ) : (
                  <tr>
                    <td colSpan={6} className="px-6 py-12 text-center text-slate-500">
                      <div className="flex flex-col items-center justify-center space-y-3">
                        <div className="flex size-16 items-center justify-center rounded-full bg-slate-50">
                          <span className="material-symbols-outlined text-4xl text-slate-300">
                            inventory_2
                          </span>
                        </div>
                        <p className="mt-2 text-base font-medium text-slate-900">No products found</p>
                        <p className="text-sm">We couldn't find any products matching your criteria.</p>
                        <button
                          type="button"
                          className="mt-2 text-sm font-medium text-primary hover:underline"
                          onClick={resetFilters}
                        >
                          Clear all filters
                        </button>
                      </div>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <div className="flex flex-col items-center justify-between gap-4 border-t border-slate-100 bg-slate-50/30 px-6 py-4 sm:flex-row">
            <div className="text-sm text-slate-500">
              Showing <span className="font-medium text-slate-900">{showingFrom}</span> to{' '}
              <span className="font-medium text-slate-900">{showingTo}</span> of{' '}
              <span className="font-medium text-slate-900">{formatNumber(totalCount)}</span> products
            </div>

            {totalPages > 1 && (
              <nav aria-label="Pagination" className="flex items-center gap-1">
                <button
                  type="button"
                  disabled={page <= 1}
                  onClick={() => setPage((current) => Math.max(1, current - 1))}
                  className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:cursor-not-allowed disabled:border-slate-100 disabled:bg-slate-50 disabled:text-slate-300"
                >
                  <span className="material-symbols-outlined text-lg leading-none">chevron_left</span>
                </button>

                <div className="flex items-center gap-1 px-1">
                  {pageNumbers[0] > 1 && (
                    <>
                      <button
                        type="button"
                        onClick={() => setPage(1)}
                        className="flex h-9 min-w-9 items-center justify-center rounded-lg border border-transparent text-sm font-medium text-slate-600 transition-all hover:border-slate-200 hover:bg-white hover:text-primary"
                      >
                        1
                      </button>
                      {pageNumbers[0] > 2 && <span className="px-1 text-slate-400">...</span>}
                    </>
                  )}

                  {pageNumbers.map((pageNumber) => (
                    <button
                      key={pageNumber}
                      type="button"
                      onClick={() => setPage(pageNumber)}
                      className={`flex h-9 min-w-9 items-center justify-center rounded-lg text-sm font-medium transition-all ${
                        pageNumber === page
                          ? 'bg-primary text-white shadow-sm shadow-blue-500/20'
                          : 'border border-transparent text-slate-600 hover:border-slate-200 hover:bg-white hover:text-primary'
                      }`}
                    >
                      {pageNumber}
                    </button>
                  ))}

                  {pageNumbers[pageNumbers.length - 1] < totalPages && (
                    <>
                      {pageNumbers[pageNumbers.length - 1] < totalPages - 1 && (
                        <span className="px-1 text-slate-400">...</span>
                      )}
                      <button
                        type="button"
                        onClick={() => setPage(totalPages)}
                        className="flex h-9 min-w-9 items-center justify-center rounded-lg border border-transparent text-sm font-medium text-slate-600 transition-all hover:border-slate-200 hover:bg-white hover:text-primary"
                      >
                        {totalPages}
                      </button>
                    </>
                  )}
                </div>

                <button
                  type="button"
                  disabled={page >= totalPages}
                  onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
                  className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:cursor-not-allowed disabled:border-slate-100 disabled:bg-slate-50 disabled:text-slate-300"
                >
                  <span className="material-symbols-outlined text-lg leading-none">chevron_right</span>
                </button>
              </nav>
            )}
          </div>
        </div>
      </div>
    </SellerLayout>
  )
}
