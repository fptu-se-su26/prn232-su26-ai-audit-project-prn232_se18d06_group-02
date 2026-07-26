import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface SellerOrder {
  subOrderId?: string
  id?: string
  orderCode?: number | string
  buyerDisplayName?: string
  buyerName?: string
  buyerAvatarUrl?: string
  createdAt: string
  status: string
  subtotal?: number
  totalPrice?: number
  itemCount?: number
}

interface OrderStats {
  total?: number
  paid?: number
  unpaid?: number
  revenue?: number
}

interface PagedOrders {
  items?: SellerOrder[]
  totalCount?: number
  pageNumber?: number
  pageSize?: number
  totalPages?: number
}

interface OrderListResult {
  orders?: PagedOrders
  stats?: OrderStats
}

const STATUS_TABS = [
  { label: 'All', value: '' },
  { label: 'Pending', value: 'Pending' },
  { label: 'Processing', value: 'Processing' },
  { label: 'Delivered', value: 'Delivered' },
  { label: 'Completed', value: 'Completed' },
  { label: 'Cancelled', value: 'Cancelled' },
]

const PAGE_SIZE = 10

function formatNumber(value?: number) {
  return new Intl.NumberFormat('en-US').format(value ?? 0)
}

function formatPrice(value?: number) {
  return `${formatNumber(value)} ₫`
}

function formatDate(value?: string) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return new Intl.DateTimeFormat('en-US', {
    month: 'short',
    day: '2-digit',
    year: 'numeric',
  }).format(date)
}

function orderId(order: SellerOrder) {
  return order.subOrderId ?? order.id ?? ''
}

function orderSubtotal(order: SellerOrder) {
  return order.subtotal ?? order.totalPrice ?? 0
}

function buyerName(order: SellerOrder) {
  return order.buyerDisplayName ?? order.buyerName ?? 'Buyer'
}

function statusClass(status: string) {
  switch (status) {
    case 'Pending':
      return 'bg-amber-50 text-amber-700 ring-amber-600/20'
    case 'Paid':
    case 'Processing':
    case 'Approved':
      return 'bg-blue-50 text-blue-700 ring-blue-600/20'
    case 'Delivered':
    case 'Completed':
      return 'bg-green-50 text-green-700 ring-green-600/20'
    case 'Cancelled':
    case 'Rejected':
      return 'bg-red-50 text-red-700 ring-red-600/20'
    default:
      return 'bg-slate-50 text-slate-700 ring-slate-600/20'
  }
}

function statusDotClass(status: string) {
  switch (status) {
    case 'Pending':
      return 'bg-amber-500'
    case 'Paid':
    case 'Processing':
    case 'Approved':
      return 'bg-blue-600'
    case 'Delivered':
    case 'Completed':
      return 'bg-green-600'
    case 'Cancelled':
    case 'Rejected':
      return 'bg-red-600'
    default:
      return 'bg-slate-600'
  }
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

function dateRangeForShortcut(shortcut: string, customStart: string, customEnd: string) {
  const today = new Date()
  const toIso = (date: Date) => date.toISOString().slice(0, 10)

  if (shortcut === 'today') {
    const value = toIso(today)
    return { startDate: value, endDate: value }
  }

  if (shortcut === 'week') {
    const start = new Date(today)
    start.setDate(today.getDate() - 7)
    return { startDate: toIso(start), endDate: toIso(today) }
  }

  if (shortcut === 'month') {
    const start = new Date(today)
    start.setDate(today.getDate() - 30)
    return { startDate: toIso(start), endDate: toIso(today) }
  }

  if (shortcut === 'custom') {
    return {
      startDate: customStart || undefined,
      endDate: customEnd || undefined,
    }
  }

  return { startDate: undefined, endDate: undefined }
}

export default function SellerOrdersPage() {
  const [orders, setOrders] = useState<SellerOrder[]>([])
  const [stats, setStats] = useState<OrderStats>({})
  const [totalCount, setTotalCount] = useState(0)
  const [totalPages, setTotalPages] = useState(1)
  const [page, setPage] = useState(1)
  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [status, setStatus] = useState('')
  const [sortBy, setSortBy] = useState('createdAt')
  const [sortDirection, setSortDirection] = useState('desc')
  const [minSubtotal, setMinSubtotal] = useState('')
  const [maxSubtotal, setMaxSubtotal] = useState('')
  const [dateRangeShortcut, setDateRangeShortcut] = useState('')
  const [customStartDate, setCustomStartDate] = useState('')
  const [customEndDate, setCustomEndDate] = useState('')
  const [advancedOpen, setAdvancedOpen] = useState(false)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [openMenuId, setOpenMenuId] = useState<string | null>(null)
  const [processingId, setProcessingId] = useState<string | null>(null)
  const menuRef = useRef<HTMLDivElement | null>(null)

  const advancedActiveCount =
    Number(Boolean(dateRangeShortcut)) + Number(Boolean(minSubtotal || maxSubtotal))

  const loadOrders = () => {
    const { startDate, endDate } = dateRangeForShortcut(
      dateRangeShortcut,
      customStartDate,
      customEndDate,
    )

    setLoading(true)
    setError(null)
    sellerApi.orders
      .list({
        SearchTerm: searchTerm || undefined,
        Status: status || undefined,
        MinSubtotal: minSubtotal ? Number(minSubtotal) : undefined,
        MaxSubtotal: maxSubtotal ? Number(maxSubtotal) : undefined,
        StartDate: startDate,
        EndDate: endDate,
        SortBy: sortBy,
        SortDirection: sortDirection,
        PageNumber: page,
        PageSize: PAGE_SIZE,
      })
      .then((result) => {
        const data = result as OrderListResult
        setOrders(data.orders?.items ?? [])
        setStats(data.stats ?? {})
        setTotalCount(data.orders?.totalCount ?? 0)
        setTotalPages(Math.max(1, data.orders?.totalPages ?? 1))
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Failed to load orders.')
      })
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    loadOrders()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    searchTerm,
    status,
    sortBy,
    sortDirection,
    page,
    dateRangeShortcut,
    customStartDate,
    customEndDate,
    minSubtotal,
    maxSubtotal,
  ])

  useEffect(() => {
    const handleClick = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setOpenMenuId(null)
      }
    }
    document.addEventListener('mousedown', handleClick)
    return () => document.removeEventListener('mousedown', handleClick)
  }, [])

  const pageNumbers = useMemo(() => getPageNumbers(page, totalPages), [page, totalPages])
  const showingFrom = totalCount === 0 ? 0 : (page - 1) * PAGE_SIZE + 1
  const showingTo = Math.min(page * PAGE_SIZE, totalCount)

  const submitSearch = (event: FormEvent) => {
    event.preventDefault()
    setPage(1)
    setSearchTerm(searchInput.trim())
  }

  const resetFilters = () => {
    setSearchInput('')
    setSearchTerm('')
    setStatus('')
    setSortBy('createdAt')
    setSortDirection('desc')
    setMinSubtotal('')
    setMaxSubtotal('')
    setDateRangeShortcut('')
    setCustomStartDate('')
    setCustomEndDate('')
    setAdvancedOpen(false)
    setPage(1)
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

  const runAction = async (
    id: string,
    action: 'approve' | 'reject' | 'markProcessing' | 'markDelivered',
  ) => {
    const messageMap = {
      approve: 'Are you sure you want to approve this order?',
      reject: 'Please confirm rejecting this order.',
      markProcessing: 'Move this order to processing?',
      markDelivered: 'Confirm delivery for this order?',
    }

    if (!window.confirm(messageMap[action])) return

    setProcessingId(id)
    try {
      const actionMap = {
        approve: () => sellerApi.orders.approve(id),
        reject: () => sellerApi.orders.reject(id, 'Rejected by seller'),
        markProcessing: () => sellerApi.orders.markProcessing(id),
        markDelivered: () => sellerApi.orders.markDelivered(id),
      }

      await actionMap[action]()
      setOpenMenuId(null)
      loadOrders()
    } finally {
      setProcessingId(null)
    }
  }

  return (
    <SellerLayout pageHeader="Order Management" breadcrumb={['Orders']}>
      <div className="flex flex-col gap-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
            <div className="flex size-12 items-center justify-center rounded-lg border border-blue-100/50 bg-blue-50 text-blue-600">
              <span className="material-symbols-outlined">receipt_long</span>
            </div>
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Total Orders
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
                Paid Orders
              </p>
              <h3 className="text-2xl font-bold text-slate-900">{formatNumber(stats.paid)}</h3>
            </div>
          </div>

          <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
            <div className="flex size-12 items-center justify-center rounded-lg border border-red-100/50 bg-red-50 text-red-600">
              <span className="material-symbols-outlined">pending_actions</span>
            </div>
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Unpaid Orders
              </p>
              <h3 className="text-2xl font-bold text-slate-900">{formatNumber(stats.unpaid)}</h3>
            </div>
          </div>

          <div className="flex items-center gap-4 rounded-xl border border-slate-200 bg-white p-4 shadow-[0_1px_3px_0_rgb(0_0_0_/_0.1),0_1px_2px_-1px_rgb(0_0_0_/_0.1)]">
            <div className="flex size-12 items-center justify-center rounded-lg border border-indigo-100/50 bg-indigo-50 text-indigo-600">
              <span className="material-symbols-outlined">payments</span>
            </div>
            <div>
              <p className="text-xs font-medium uppercase tracking-wider text-slate-500">
                Total Revenue
              </p>
              <h3 className="text-2xl font-bold text-slate-900">{formatPrice(stats.revenue)}</h3>
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
                  onClick={() => {
                    setStatus(tab.value)
                    setPage(1)
                  }}
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
                  placeholder="Search by order code, buyer name..."
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

                <button
                  type="button"
                  className="flex items-center justify-center gap-1.5 whitespace-nowrap rounded-lg border border-slate-200 bg-white px-3.5 py-2.5 text-sm text-slate-600 shadow-sm transition-colors hover:bg-slate-50"
                >
                  <span className="material-symbols-outlined text-[18px]">file_download</span>
                  <span className="hidden font-medium sm:inline">Export</span>
                </button>
              </div>
            </div>

            {(advancedOpen || advancedActiveCount > 0) && (
              <div className="flex flex-col gap-5 border-t border-slate-100 bg-slate-50/40 px-4 pb-5 pt-4">
                <div className="flex flex-wrap items-start gap-4 lg:flex-nowrap lg:gap-6">
                  <div className="w-full space-y-1 lg:w-72 lg:shrink-0">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">payments</span>
                      Subtotal Range (₫)
                    </label>
                    <div className="flex items-center gap-1.5">
                      <input
                        value={minSubtotal}
                        onChange={(event) => {
                          setMinSubtotal(event.target.value)
                          setPage(1)
                        }}
                        type="number"
                        placeholder="Min"
                        className="h-[38px] min-w-0 w-full rounded-lg border border-slate-200 bg-white px-2.5 py-2 text-sm shadow-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                      />
                      <span className="shrink-0 text-lg font-light text-slate-300">-</span>
                      <input
                        value={maxSubtotal}
                        onChange={(event) => {
                          setMaxSubtotal(event.target.value)
                          setPage(1)
                        }}
                        type="number"
                        placeholder="Max"
                        className="h-[38px] min-w-0 w-full rounded-lg border border-slate-200 bg-white px-2.5 py-2 text-sm shadow-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                      />
                    </div>
                  </div>

                  <div className="w-full space-y-1 lg:w-80 lg:shrink-0">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                      Order Date
                    </label>
                    <div className="flex flex-col gap-2">
                      <select
                        value={dateRangeShortcut}
                        onChange={(event) => {
                          setDateRangeShortcut(event.target.value)
                          if (event.target.value !== 'custom') {
                            setCustomStartDate('')
                            setCustomEndDate('')
                          }
                          setPage(1)
                        }}
                        className="h-10 w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                      >
                        <option value="">All Time</option>
                        <option value="today">Today</option>
                        <option value="week">This Week</option>
                        <option value="month">This Month</option>
                        <option value="custom">Custom Range</option>
                      </select>
                      {dateRangeShortcut === 'custom' && (
                        <div className="grid grid-cols-2 gap-2">
                          <input
                            value={customStartDate}
                            onChange={(event) => {
                              setCustomStartDate(event.target.value)
                              setPage(1)
                            }}
                            type="date"
                            className="h-10 rounded-lg border border-slate-200 bg-white px-3 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                          />
                          <input
                            value={customEndDate}
                            onChange={(event) => {
                              setCustomEndDate(event.target.value)
                              setPage(1)
                            }}
                            type="date"
                            className="h-10 rounded-lg border border-slate-200 bg-white px-3 text-sm focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                          />
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="mt-2 w-full lg:ms-auto lg:mt-[22px] lg:w-auto">
                    <button
                      type="button"
                      onClick={() => loadOrders()}
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
                      onClick={() => handleSort('orderCode')}
                      className="group inline-flex w-full items-center gap-1 transition-colors hover:text-primary"
                    >
                      Order Code
                      <span
                        className={`material-symbols-outlined text-[16px] transition-all ${sortIconClass(sortBy, 'orderCode')}`}
                      >
                        {sortIcon(sortBy, sortDirection, 'orderCode')}
                      </span>
                    </button>
                  </th>
                  <th className="hidden px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">
                    Buyer
                  </th>
                  <th className="px-3 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button
                      type="button"
                      onClick={() => handleSort('subtotal')}
                      className="group inline-flex w-full items-center justify-end gap-1 transition-colors hover:text-primary"
                    >
                      Subtotal
                      <span
                        className={`material-symbols-outlined text-[16px] transition-all ${sortIconClass(sortBy, 'subtotal')}`}
                      >
                        {sortIcon(sortBy, sortDirection, 'subtotal')}
                      </span>
                    </button>
                  </th>
                  <th className="px-3 py-4 text-center text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Status
                  </th>
                  <th className="py-4 pl-3 pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button
                      type="button"
                      onClick={() => handleSort('createdAt')}
                      className="group inline-flex w-full items-center justify-end gap-1 transition-colors hover:text-primary"
                    >
                      Created
                      <span
                        className={`material-symbols-outlined text-[16px] transition-all ${sortIconClass(sortBy, 'createdAt')}`}
                      >
                        {sortIcon(sortBy, sortDirection, 'createdAt')}
                      </span>
                    </button>
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
                        <div className="h-10 animate-pulse rounded-lg bg-slate-100" />
                      </td>
                    </tr>
                  ))
                ) : error ? (
                  <tr>
                    <td colSpan={6} className="px-6 py-12 text-center text-red-600">
                      {error}
                    </td>
                  </tr>
                ) : orders.length > 0 ? (
                  orders.map((order) => {
                    const id = orderId(order)
                    const name = buyerName(order)
                    const canApproveReject = order.status === 'Pending'
                    const canProcess = order.status === 'Paid' || order.status === 'Approved'
                    const canDeliver = order.status === 'Processing'

                    return (
                      <tr
                        key={id || order.orderCode}
                        className="group/row cursor-pointer transition-all hover:bg-slate-50"
                        onClick={() => {
                          if (id) window.location.href = `/store-owner/orders/${id}`
                        }}
                      >
                        <td className="px-6 py-4 font-mono text-sm font-semibold text-slate-900 transition-colors group-hover/row:text-primary">
                          #{order.orderCode ?? id.slice(0, 8).toUpperCase()}
                        </td>
                        <td className="hidden whitespace-nowrap px-3 py-4 align-middle md:table-cell">
                          <div className="flex items-center gap-3">
                            {order.buyerAvatarUrl ? (
                              <img
                                src={order.buyerAvatarUrl}
                                className="size-8 rounded-full object-cover"
                                alt=""
                              />
                            ) : (
                              <div className="flex size-8 items-center justify-center rounded-full bg-slate-100 text-xs font-bold text-slate-500">
                                {name ? name.slice(0, 1).toUpperCase() : '?'}
                              </div>
                            )}
                            <span className="text-sm text-slate-600">{name}</span>
                          </div>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-right text-sm font-semibold text-slate-900">
                          {formatPrice(orderSubtotal(order))}
                          <div className="text-[10px] font-normal text-slate-500">
                            {formatNumber(order.itemCount)} item(s)
                          </div>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-center">
                          <span
                            className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1.5 text-xs font-semibold ring-1 ring-inset ${statusClass(order.status)}`}
                          >
                            <span className={`size-1.5 rounded-full ${statusDotClass(order.status)}`} />
                            {order.status}
                          </span>
                        </td>
                        <td className="whitespace-nowrap px-3 py-4 text-right">
                          <p className="text-sm text-slate-500">{formatDate(order.createdAt)}</p>
                        </td>
                        <td className="whitespace-nowrap px-6 py-4 text-right">
                          <div className="relative inline-block text-left" ref={openMenuId === id ? menuRef : null}>
                            <button
                              type="button"
                              title="Actions"
                              onClick={(event) => {
                                event.stopPropagation()
                                setOpenMenuId((current) => (current === id ? null : id))
                              }}
                              className={`rounded-lg p-2 transition-all ${
                                openMenuId === id
                                  ? 'bg-slate-100 text-slate-600'
                                  : 'text-slate-400 hover:bg-slate-100 hover:text-slate-600'
                              }`}
                            >
                              <span className="material-symbols-outlined text-[20px]">more_vert</span>
                            </button>

                            {openMenuId === id && (
                              <div
                                className="absolute right-0 z-50 mt-2 w-48 overflow-hidden rounded-xl bg-white shadow-lg ring-1 ring-black/5"
                                onClick={(event) => event.stopPropagation()}
                              >
                                <div className="divide-y divide-slate-100">
                                  <div className="py-1">
                                    <Link
                                      to={`/store-owner/orders/${id}`}
                                      className="flex items-center gap-2 px-4 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-50"
                                    >
                                      <span className="material-symbols-outlined text-[18px] text-slate-400">
                                        visibility
                                      </span>
                                      View Details
                                    </Link>
                                  </div>

                                  {canApproveReject && (
                                    <div className="py-1">
                                      <button
                                        type="button"
                                        disabled={processingId === id}
                                        onClick={() => void runAction(id, 'approve')}
                                        className="flex w-full items-center gap-2 px-4 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-50 disabled:opacity-60"
                                      >
                                        <span className="material-symbols-outlined text-[18px] text-emerald-600">
                                          verified
                                        </span>
                                        Approve Order
                                      </button>
                                      <button
                                        type="button"
                                        disabled={processingId === id}
                                        onClick={() => void runAction(id, 'reject')}
                                        className="flex w-full items-center gap-2 px-4 py-2 text-sm text-red-600 transition-colors hover:bg-slate-50 disabled:opacity-60"
                                      >
                                        <span className="material-symbols-outlined text-[18px]">block</span>
                                        Reject Order
                                      </button>
                                    </div>
                                  )}

                                  {canProcess && (
                                    <div className="py-1">
                                      <button
                                        type="button"
                                        disabled={processingId === id}
                                        onClick={() => void runAction(id, 'markProcessing')}
                                        className="flex w-full items-center gap-2 px-4 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-50 disabled:opacity-60"
                                      >
                                        <span className="material-symbols-outlined text-[18px] text-blue-600">
                                          package_2
                                        </span>
                                        Start Processing
                                      </button>
                                    </div>
                                  )}

                                  {canDeliver && (
                                    <div className="py-1">
                                      <button
                                        type="button"
                                        disabled={processingId === id}
                                        onClick={() => void runAction(id, 'markDelivered')}
                                        className="flex w-full items-center gap-2 px-4 py-2 text-sm text-slate-700 transition-colors hover:bg-slate-50 disabled:opacity-60"
                                      >
                                        <span className="material-symbols-outlined text-[18px] text-emerald-600">
                                          local_shipping
                                        </span>
                                        Mark Delivered
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
                            receipt_long
                          </span>
                        </div>
                        <p className="mt-2 text-base font-medium text-slate-900">No orders found</p>
                        <p className="text-sm">We couldn't find any orders matching your criteria.</p>
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
              <span className="font-medium text-slate-900">{formatNumber(totalCount)}</span> orders
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
