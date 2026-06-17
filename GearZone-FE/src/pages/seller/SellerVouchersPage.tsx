import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface Voucher {
  id: string
  code: string
  name: string
  description?: string
  discountType: string
  discountValue: number
  maxDiscount?: number | null
  minOrderAmount?: number | null
  usageLimit: number
  usedCount: number
  categoryId?: number | null
  categoryName?: string | null
  categoryIcon?: string | null
  startAt: string
  endAt: string
  status: string
}

interface VoucherSummary {
  totalVouchers?: number
  activeToday?: number
  redemptionRate?: number
  totalSavedAmount?: number
}

interface PagedVouchers {
  items?: Voucher[]
  totalCount?: number
  pageNumber?: number
  pageSize?: number
  totalPages?: number
}

interface CategoryItem {
  id: number
  name: string
}

interface VoucherListResult {
  vouchers?: PagedVouchers
  summary?: VoucherSummary
  categories?: CategoryItem[]
}

const STATUS_TABS = ['', 'Active', 'Upcoming', 'Expired', 'Disabled']
const SCOPES = ['Global', 'Category', 'Product']
const VOUCHER_TYPES = ['Order', 'Shipping']
const DISCOUNT_TYPES = ['Percent', 'Fixed']
const PAGE_SIZE = 10

function formatNumber(value?: number | null) {
  return new Intl.NumberFormat('en-US').format(value ?? 0)
}

function formatMoney(value?: number | null) {
  return `${formatNumber(value)} VND`
}

function formatDate(value?: string, includeYear = false) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: '2-digit',
    ...(includeYear ? { year: 'numeric' } : {}),
  }).format(date)
}

function statusBadgeClass(status: string) {
  switch (status) {
    case 'Active':
      return 'bg-emerald-100 text-emerald-700 px-2.5 py-1'
    case 'Upcoming':
      return 'bg-amber-100 text-amber-700 px-2.5 py-1'
    case 'Expired':
      return 'bg-slate-100 text-slate-500 px-2.5 py-1'
    case 'Disabled':
      return 'bg-red-100 text-red-700 px-2.5 py-1'
    case 'Finished':
      return 'bg-blue-100 text-blue-700 px-2.5 py-1'
    default:
      return 'bg-slate-100 text-slate-500 px-2.5 py-1'
  }
}

function accentClass(status: string) {
  switch (status) {
    case 'Active':
      return 'bg-primary'
    case 'Upcoming':
      return 'bg-amber-500'
    case 'Expired':
      return 'bg-slate-400'
    case 'Disabled':
      return 'bg-red-500'
    case 'Finished':
      return 'bg-blue-700'
    default:
      return 'bg-slate-500'
  }
}

function progressClass(status: string) {
  switch (status) {
    case 'Active':
      return 'bg-primary'
    case 'Upcoming':
      return 'bg-amber-400'
    case 'Disabled':
      return 'bg-red-400'
    default:
      return 'bg-slate-300'
  }
}

function getPageNumbers(page: number, totalPages: number) {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1)
  }

  const pages = new Set<number>([1, totalPages])
  for (let i = page - 1; i <= page + 1; i += 1) {
    if (i > 1 && i < totalPages) pages.add(i)
  }

  return Array.from(pages).sort((a, b) => a - b)
}

function parseSortOption(option: string) {
  if (!option) return { sortBy: 'createdAt', sortDirection: 'desc' }
  const [sortBy, sortDirection] = option.split('-')
  return {
    sortBy: sortBy || 'createdAt',
    sortDirection: sortDirection || 'desc',
  }
}

function dateParams(startDate: string, endDate: string) {
  return {
    StartDate: startDate || undefined,
    EndDate: endDate || undefined,
  }
}

export default function SellerVouchersPage() {
  const [vouchers, setVouchers] = useState<Voucher[]>([])
  const [summary, setSummary] = useState<VoucherSummary>({})
  const [categories, setCategories] = useState<CategoryItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage] = useState(1)
  const [searchInput, setSearchInput] = useState('')
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [scope, setScope] = useState('')
  const [voucherType, setVoucherType] = useState('')
  const [sortOption, setSortOption] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [discountType, setDiscountType] = useState('')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')
  const [advancedOpen, setAdvancedOpen] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [togglingId, setTogglingId] = useState<string | null>(null)

  const activeAdvancedCount =
    Number(Boolean(categoryId)) + Number(Boolean(discountType)) + Number(Boolean(startDate || endDate))

  const pageNumbers = useMemo(() => getPageNumbers(page, totalPages), [page, totalPages])

  const loadVouchers = () => {
    const sort = parseSortOption(sortOption)
    setLoading(true)
    setError(null)

    sellerApi.vouchers
      .list({
        Search: search || undefined,
        Status: status || undefined,
        Scope: scope || undefined,
        VoucherType: voucherType || undefined,
        DiscountType: discountType || undefined,
        CategoryId: categoryId ? Number(categoryId) : undefined,
        ...dateParams(startDate, endDate),
        SortBy: sort.sortBy,
        SortDirection: sort.sortDirection,
        PageNumber: page,
        PageSize: PAGE_SIZE,
      })
      .then((result) => {
        const data = result as VoucherListResult
        setVouchers(data.vouchers?.items ?? [])
        setSummary(data.summary ?? {})
        setCategories(data.categories ?? [])
        setTotalCount(data.vouchers?.totalCount ?? 0)
        setTotalPages(Math.max(1, data.vouchers?.totalPages ?? 1))
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Failed to load vouchers.')
      })
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    loadVouchers()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    search,
    status,
    scope,
    voucherType,
    sortOption,
    categoryId,
    discountType,
    startDate,
    endDate,
    page,
  ])

  const submitSearch = (event: FormEvent) => {
    event.preventDefault()
    setPage(1)
    setSearch(searchInput.trim())
  }

  const resetFilters = () => {
    setSearchInput('')
    setSearch('')
    setStatus('')
    setScope('')
    setVoucherType('')
    setSortOption('')
    setCategoryId('')
    setDiscountType('')
    setStartDate('')
    setEndDate('')
    setAdvancedOpen(false)
    setPage(1)
  }

  const toggleVoucherStatus = async (voucher: Voucher) => {
    const isDisabled = voucher.status === 'Disabled'
    const action = isDisabled ? 'enable' : 'disable'
    if (!window.confirm(`Are you sure you want to ${action} this voucher?`)) return

    setTogglingId(voucher.id)
    try {
      await sellerApi.vouchers.toggleStatus(voucher.id)
      loadVouchers()
    } finally {
      setTogglingId(null)
    }
  }

  const showingFrom = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1
  const showingTo = Math.min(page * PAGE_SIZE, totalCount)

  return (
    <SellerLayout pageHeader="Voucher Management" breadcrumb={['Marketing', 'Vouchers']}>
      <style>
        {`
          .voucher-row {
            transition: all 0.2s ease-in-out;
          }
          .voucher-row:hover {
            transform: translateX(4px);
            box-shadow: 0 4px 20px -5px rgba(0, 0, 0, 0.08);
          }
          .serrated-left {
            -webkit-mask-image: radial-gradient(circle at 2px 7px, transparent 4px, black 5px);
            -webkit-mask-size: 100% 14px;
            mask-image: radial-gradient(circle at 2px 7px, transparent 4px, black 5px);
            mask-size: 100% 14px;
          }
          .ticket-dash {
            background-image: linear-gradient(to bottom, #E2E8F0 60%, rgba(255,255,255,0) 0%);
            background-position: center;
            background-size: 1.5px 10px;
            background-repeat: repeat-y;
          }
        `}
      </style>

      <div className="flex flex-col gap-6">
        <div className="flex items-center justify-between rounded-2xl border border-slate-200 bg-white p-5 shadow-[0_1px_3px_0_rgb(15_23_42_/_0.08),0_1px_2px_-1px_rgb(15_23_42_/_0.08)]">
          <div>
            <h3 className="text-lg font-bold text-slate-800">Voucher Management</h3>
            <p className="text-sm text-slate-500">
              Monitor and create marketplace discount programs.
            </p>
          </div>
          <Link
            to="/store-owner/vouchers/create"
            className="seller-primary-button flex items-center gap-2 rounded-xl bg-primary px-6 py-3 font-semibold text-white shadow-sm shadow-primary/20 transition-all hover:bg-blue-700"
          >
            <span className="material-symbols-outlined">add</span>
            Create Voucher
          </Link>
        </div>

        <div className="flex flex-wrap gap-4">
          <div className="flex min-h-[108px] min-w-[220px] flex-1 items-center gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-[0_1px_3px_0_rgb(15_23_42_/_0.08),0_1px_2px_-1px_rgb(15_23_42_/_0.08)]">
            <div className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-blue-50 text-primary">
              <span className="material-symbols-outlined text-[20px]">confirmation_number</span>
            </div>
            <div className="flex min-w-0 flex-col">
              <p className="text-[10px] font-bold uppercase leading-none tracking-widest text-slate-400">
                Total Vouchers
              </p>
              <h3 className="mt-1.5 text-xl font-bold leading-tight text-slate-900">
                {formatNumber(summary.totalVouchers)}
              </h3>
              <div className="mt-2 flex items-center gap-1 text-[10px] font-bold text-green-600">
                <span className="material-symbols-outlined text-[12px]">trending_up</span>
                <span>+12% vs last month</span>
              </div>
            </div>
          </div>

          <div className="flex min-h-[108px] min-w-[220px] flex-1 items-center gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-[0_1px_3px_0_rgb(15_23_42_/_0.08),0_1px_2px_-1px_rgb(15_23_42_/_0.08)]">
            <div className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-emerald-50 text-emerald-600">
              <span className="material-symbols-outlined text-[20px]">bolt</span>
            </div>
            <div className="flex min-w-0 flex-col">
              <p className="text-[10px] font-bold uppercase leading-none tracking-widest text-slate-400">
                Active Today
              </p>
              <h3 className="mt-1.5 text-xl font-bold leading-tight text-slate-900">
                {formatNumber(summary.activeToday)}
              </h3>
              <div className="mt-2 flex items-center gap-1 text-[10px] font-bold text-slate-400">
                <span className="material-symbols-outlined text-[12px]">schedule</span>
                <span>Currently live</span>
              </div>
            </div>
          </div>

          <div className="flex min-h-[108px] min-w-[220px] flex-1 items-center gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-[0_1px_3px_0_rgb(15_23_42_/_0.08),0_1px_2px_-1px_rgb(15_23_42_/_0.08)]">
            <div className="mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-amber-50 text-amber-600">
              <span className="material-symbols-outlined text-[20px]">percent</span>
            </div>
            <div className="flex min-w-0 flex-col">
              <p className="text-[10px] font-bold uppercase leading-none tracking-widest text-slate-400">
                Redemption Rate
              </p>
              <h3 className="mt-1.5 text-xl font-bold leading-tight text-slate-900">
                {formatNumber(summary.redemptionRate)}%
              </h3>
              <div className="mt-2 flex items-center gap-1 text-[10px] font-bold text-green-600">
                <span className="material-symbols-outlined text-[12px]">arrow_upward</span>
                <span>+4.1% performance</span>
              </div>
            </div>
          </div>

          <div className="group relative flex min-h-[108px] min-w-[220px] flex-1 items-center gap-4 overflow-hidden rounded-xl border border-slate-800 bg-slate-900 p-5 shadow-[0_1px_3px_0_rgb(15_23_42_/_0.1),0_1px_2px_-1px_rgb(15_23_42_/_0.1)]">
            <div className="absolute right-0 top-0 p-4 opacity-10 transition-transform group-hover:scale-125">
              <span className="material-symbols-outlined text-[64px] text-white">savings</span>
            </div>
            <div className="z-10 mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full bg-white/10 text-white">
              <span className="material-symbols-outlined text-[20px]">payments</span>
            </div>
            <div className="z-10 flex min-w-0 flex-col">
              <p className="text-[10px] font-bold uppercase leading-none tracking-widest text-slate-400">
                Total Saved for Users
              </p>
              <h3 className="mt-1.5 text-xl font-bold leading-tight text-white">
                {formatMoney(summary.totalSavedAmount)}
              </h3>
              <div className="mt-2 flex items-center gap-1 text-[10px] font-bold text-blue-400">
                <span className="material-symbols-outlined text-[12px]">verified_user</span>
                <span>Platform impact</span>
              </div>
            </div>
          </div>
        </div>

        <form onSubmit={submitSearch}>
          <section className="space-y-4">
            <div className="flex overflow-x-auto border-b border-slate-200">
              {STATUS_TABS.map((tab) => {
                const active = status === tab
                return (
                  <button
                    key={tab || 'All'}
                    type="button"
                    onClick={() => {
                      setStatus(tab)
                      setPage(1)
                    }}
                    className={`border-b-2 px-6 py-3 text-sm font-semibold transition-all ${
                      active
                        ? 'border-primary text-primary'
                        : 'border-transparent text-slate-500 hover:bg-slate-50 hover:text-slate-700'
                    }`}
                  >
                    {tab || 'All'}
                  </button>
                )
              })}
            </div>

            <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
              <div className="p-4">
                <div className="flex flex-col gap-4 lg:flex-row">
                  <div className="relative flex-1">
                    <span className="absolute inset-y-0 left-3 flex items-center text-slate-400">
                      <span className="material-symbols-outlined text-[20px]">search</span>
                    </span>
                    <input
                      value={searchInput}
                      onChange={(event) => setSearchInput(event.target.value)}
                      className="h-11 w-full rounded-lg border border-slate-200 bg-slate-50 py-2 pl-10 pr-4 text-sm transition-all focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                      placeholder="Search code or name..."
                    />
                  </div>

                  <div className="w-full lg:w-40">
                    <select
                      value={scope}
                      onChange={(event) => {
                        setScope(event.target.value)
                        setPage(1)
                      }}
                      className="h-11 w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                    >
                      <option value="">All Scopes</option>
                      {SCOPES.map((item) => (
                        <option key={item} value={item}>
                          {item}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="w-full lg:w-44">
                    <select
                      value={voucherType}
                      onChange={(event) => {
                        setVoucherType(event.target.value)
                        setPage(1)
                      }}
                      className="h-11 w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                    >
                      <option value="">Voucher Type</option>
                      {VOUCHER_TYPES.map((item) => (
                        <option key={item} value={item}>
                          {item}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="w-full lg:w-48">
                    <select
                      value={sortOption}
                      onChange={(event) => {
                        setSortOption(event.target.value)
                        setPage(1)
                      }}
                      className="h-11 w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                    >
                      <option value="">Sort By</option>
                      <option value="createdAt-desc">Newest First</option>
                      <option value="createdAt-asc">Oldest First</option>
                      <option value="name-asc">Name (A-Z)</option>
                      <option value="name-desc">Name (Z-A)</option>
                      <option value="discount-desc">Highest Discount</option>
                      <option value="discount-asc">Lowest Discount</option>
                      <option value="usage-desc">Most Used</option>
                      <option value="expiry-asc">Expiring Soon</option>
                    </select>
                  </div>

                  <div className="flex items-center gap-2">
                    <button
                      type="submit"
                      className="seller-primary-button flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-all hover:bg-blue-700"
                    >
                      <span className="material-symbols-outlined text-[20px]">search</span>
                      <span>Search</span>
                    </button>

                    <button
                      type="button"
                      onClick={() => setAdvancedOpen((open) => !open)}
                      className={`flex items-center gap-2 rounded-lg border px-3.5 py-2.5 text-sm text-slate-600 shadow-sm transition-colors ${
                        advancedOpen || activeAdvancedCount > 0
                          ? 'border-slate-300 bg-slate-50 text-slate-900'
                          : 'border-slate-200 bg-white hover:bg-slate-50'
                      }`}
                    >
                      <span className="material-symbols-outlined text-[20px]">tune</span>
                      <span className="hidden sm:inline">Filters</span>
                      {activeAdvancedCount > 0 && (
                        <span className="flex size-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold text-white">
                          {activeAdvancedCount}
                        </span>
                      )}
                    </button>

                    <button
                      type="button"
                      onClick={resetFilters}
                      title="Reset Filters"
                      className="rounded-lg border border-slate-200 bg-white p-2.5 text-slate-400 shadow-sm transition-all hover:bg-slate-50 hover:text-slate-600"
                    >
                      <span className="material-symbols-outlined text-[20px]">restart_alt</span>
                    </button>
                  </div>
                </div>
              </div>

              {(advancedOpen || activeAdvancedCount > 0) && (
                <div className="border-t border-slate-100 bg-slate-50/40 p-4">
                  <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
                    <div className="space-y-1.5">
                      <label className="ml-1 flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-slate-400">
                        <span className="material-symbols-outlined text-[14px]">category</span>
                        Category
                      </label>
                      <select
                        value={categoryId}
                        onChange={(event) => {
                          setCategoryId(event.target.value)
                          setPage(1)
                        }}
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

                    <div className="space-y-1.5">
                      <label className="ml-1 flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-slate-400">
                        <span className="material-symbols-outlined text-[14px]">payments</span>
                        Discount Type
                      </label>
                      <select
                        value={discountType}
                        onChange={(event) => {
                          setDiscountType(event.target.value)
                          setPage(1)
                        }}
                        className="h-10 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                      >
                        <option value="">All Types</option>
                        {DISCOUNT_TYPES.map((item) => (
                          <option key={item} value={item}>
                            {item}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div className="space-y-1.5 lg:col-span-2">
                      <label className="ml-1 flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-slate-400">
                        <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                        Date Range
                      </label>
                      <div className="grid grid-cols-2 gap-2">
                        <input
                          value={startDate}
                          onChange={(event) => {
                            setStartDate(event.target.value)
                            setPage(1)
                          }}
                          type="date"
                          className="h-10 rounded-lg border border-slate-200 bg-white px-3 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                        />
                        <input
                          value={endDate}
                          onChange={(event) => {
                            setEndDate(event.target.value)
                            setPage(1)
                          }}
                          type="date"
                          className="h-10 rounded-lg border border-slate-200 bg-white px-3 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                        />
                      </div>
                    </div>
                  </div>

                  <div className="flex justify-end border-t border-slate-200/60 pt-4">
                    <button
                      type="button"
                      onClick={() => loadVouchers()}
                      className="rounded-lg bg-slate-800 px-6 py-2 text-sm font-bold text-white shadow-sm transition-all hover:bg-slate-900"
                    >
                      Apply Advanced Filters
                    </button>
                  </div>
                </div>
              )}
            </div>
          </section>
        </form>

        <section className="flex flex-col gap-4">
          {loading ? (
            Array.from({ length: 4 }).map((_, index) => (
              <div key={index} className="h-36 animate-pulse rounded-xl bg-white" />
            ))
          ) : error ? (
            <div className="rounded-xl border border-red-200 bg-red-50 p-12 text-center text-red-600">
              {error}
            </div>
          ) : vouchers.length === 0 ? (
            <div className="flex flex-col items-center justify-center rounded-xl border border-slate-200 bg-white p-12 text-center shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
              <div className="mb-4 flex size-16 items-center justify-center rounded-full bg-slate-50 text-slate-300">
                <span className="material-symbols-outlined text-[40px]">confirmation_number</span>
              </div>
              <h3 className="text-lg font-bold text-slate-800">No vouchers found</h3>
              <p className="mx-auto mb-6 mt-1 max-w-xs text-slate-500">
                Start by creating a new voucher for the marketplace.
              </p>
              <Link
                to="/store-owner/vouchers/create"
                className="seller-primary-button inline-flex items-center gap-2 rounded-lg bg-primary px-6 py-2.5 font-bold text-white shadow-md shadow-primary/20 transition-all hover:bg-blue-700"
              >
                <span className="material-symbols-outlined">add</span>
                Create First Voucher
              </Link>
            </div>
          ) : (
            vouchers.map((voucher) => {
              const progress =
                voucher.usageLimit > 0 ? (voucher.usedCount / voucher.usageLimit) * 100 : 0
              const strongAccent = ['Active', 'Upcoming', 'Disabled', 'Finished'].includes(
                voucher.status,
              )
              const icon = voucher.categoryIcon ?? 'confirmation_number'

              return (
                <div
                  key={voucher.id}
                  className="voucher-row flex h-36 overflow-hidden rounded-xl border-y border-r border-slate-100 bg-white shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]"
                >
                  <div
                    className={`serrated-left relative flex w-36 shrink-0 flex-col items-center justify-center rounded-l-xl p-4 ${accentClass(voucher.status)}`}
                  >
                    <div className="mb-2 flex size-12 items-center justify-center rounded-full bg-white shadow-sm">
                      <span
                        className={`material-symbols-outlined text-[28px] ${
                          voucher.status === 'Active' ? 'text-primary' : 'text-slate-400'
                        }`}
                      >
                        {icon}
                      </span>
                    </div>
                    <div className="text-center">
                      <p
                        className={`text-xl font-bold leading-none ${
                          strongAccent ? 'text-white' : 'text-slate-900'
                        }`}
                      >
                        {voucher.discountType === 'Percent'
                          ? `${formatNumber(voucher.discountValue)}% OFF`
                          : `${formatNumber(voucher.discountValue)} VND OFF`}
                      </p>
                      <p
                        className={`mt-1 text-[10px] font-black uppercase tracking-widest ${
                          strongAccent ? 'text-white/70' : 'text-slate-400'
                        }`}
                      >
                        {voucher.code}
                      </p>
                    </div>
                  </div>

                  <div className="ticket-dash h-full w-px shrink-0" />

                  <div className="flex min-w-0 flex-1 flex-col justify-between p-5">
                    <div className="flex items-start justify-between">
                      <div className="min-w-0">
                        <div className="mb-1.5 flex items-center gap-3">
                          <h3 className="truncate text-base font-bold leading-none text-slate-800">
                            {voucher.name}
                          </h3>
                          <span
                            className={`${statusBadgeClass(voucher.status)} shrink-0 rounded-full text-[10px] font-bold uppercase tracking-tight`}
                          >
                            {voucher.status}
                          </span>
                        </div>
                        <div className="flex flex-wrap gap-x-6 gap-y-1 text-xs text-slate-500">
                          <div className="flex items-center gap-1">
                            <span className="text-slate-400">Min. Spend:</span>
                            <span className="font-bold text-slate-700">
                              {formatMoney(voucher.minOrderAmount)}
                            </span>
                          </div>
                          <div className="flex items-center gap-1">
                            <span className="text-slate-400">Max Disc:</span>
                            <span className="font-bold text-slate-700">
                              {voucher.maxDiscount && voucher.maxDiscount > 0
                                ? formatMoney(voucher.maxDiscount)
                                : 'No Limit'}
                            </span>
                          </div>
                          <div className="flex items-center gap-1">
                            <span className="text-slate-400">Target:</span>
                            <span className="font-bold text-slate-700">
                              {voucher.categoryId ? voucher.categoryName : 'Global'}
                            </span>
                          </div>
                        </div>
                      </div>

                      <div className="flex items-center gap-1">
                        <Link
                          to={`/store-owner/vouchers/edit/${voucher.id}`}
                          className="rounded-lg p-2 text-slate-400 transition-all hover:bg-blue-50 hover:text-primary"
                          title="Edit"
                        >
                          <span className="material-symbols-outlined text-xl">edit</span>
                        </Link>
                        <Link
                          to={`/store-owner/vouchers/create?copyFromId=${voucher.id}`}
                          className="rounded-lg p-2 text-slate-400 transition-all hover:bg-slate-100 hover:text-slate-600"
                          title="Duplicate"
                        >
                          <span className="material-symbols-outlined text-xl">content_copy</span>
                        </Link>
                        <button
                          type="button"
                          disabled={togglingId === voucher.id}
                          onClick={() => void toggleVoucherStatus(voucher)}
                          className={`rounded-lg p-2 transition-all disabled:opacity-60 ${
                            voucher.status === 'Disabled'
                              ? 'text-emerald-500 hover:bg-emerald-50'
                              : 'text-red-400 hover:bg-red-50 hover:text-red-500'
                          }`}
                          title={voucher.status === 'Disabled' ? 'Enable Voucher' : 'Disable Voucher'}
                        >
                          <span className="material-symbols-outlined text-xl">
                            {voucher.status === 'Disabled' ? 'play_arrow' : 'block'}
                          </span>
                        </button>
                      </div>
                    </div>

                    <div className="mt-auto flex items-end justify-between gap-8">
                      <div className="max-w-md flex-1">
                        <div className="mb-1.5 flex justify-between text-[11px]">
                          <span className="font-medium text-slate-500">
                            Usage:{' '}
                            <span
                              className={`font-bold ${progress > 90 ? 'text-red-500' : 'text-slate-700'}`}
                            >
                              {voucher.usedCount}/{voucher.usageLimit}
                            </span>
                          </span>
                          <span className="font-bold text-slate-900">{progress.toFixed(0)}%</span>
                        </div>
                        <div className="h-2 w-full overflow-hidden rounded-full bg-slate-100 shadow-inner">
                          <div
                            className={`${progressClass(voucher.status)} h-full rounded-full shadow-sm transition-all duration-1000`}
                            style={{ width: `${Math.min(100, progress)}%` }}
                          />
                        </div>
                      </div>
                      <div className="mb-0.5 flex shrink-0 items-center gap-1.5 text-xs text-slate-400">
                        <span className="material-symbols-outlined text-sm">schedule</span>
                        <span>
                          {formatDate(voucher.startAt)} - {formatDate(voucher.endAt, true)}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              )
            })
          )}
        </section>

        <div className="flex flex-col items-center justify-between gap-4 rounded-xl border border-slate-200 bg-white px-6 py-4 shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)] sm:flex-row">
          <div className="text-[13px] font-medium text-slate-500">
            Showing <span className="font-bold text-slate-900">{showingFrom}</span> to{' '}
            <span className="font-bold text-slate-900">{showingTo}</span> of{' '}
            <span className="font-bold text-slate-900">{formatNumber(totalCount)}</span> vouchers
          </div>

          {totalPages > 1 && (
            <nav aria-label="Pagination" className="flex items-center gap-1.5">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => setPage((current) => Math.max(1, current - 1))}
                className="flex size-9 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-400 transition-all hover:bg-slate-50 hover:text-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <span className="material-symbols-outlined text-[20px]">chevron_left</span>
              </button>

              {pageNumbers.map((pageNumber, index) => {
                const previous = pageNumbers[index - 1]
                const showEllipsis = previous && pageNumber - previous > 1
                return (
                  <span key={pageNumber} className="flex items-center gap-1.5">
                    {showEllipsis && (
                      <span className="flex items-center justify-center px-2 text-sm font-bold text-slate-300">
                        ...
                      </span>
                    )}
                    <button
                      type="button"
                      onClick={() => setPage(pageNumber)}
                      className={`flex size-9 items-center justify-center rounded-lg border text-sm font-bold transition-all ${
                        page === pageNumber
                          ? 'border-primary bg-primary text-white shadow-sm shadow-primary/20'
                          : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
                      }`}
                    >
                      {pageNumber}
                    </button>
                  </span>
                )
              })}

              <button
                type="button"
                disabled={page >= totalPages}
                onClick={() => setPage((current) => Math.min(totalPages, current + 1))}
                className="flex size-9 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-400 transition-all hover:bg-slate-50 hover:text-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <span className="material-symbols-outlined text-[20px]">chevron_right</span>
              </button>
            </nav>
          )}
        </div>
      </div>
    </SellerLayout>
  )
}
