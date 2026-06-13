/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { adminApi, type AdminOrderDto, type AdminOrderStatsDto, type PagedResult } from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const PAGE_SIZE = 10

const emptyStats: AdminOrderStatsDto = {
  paidOrders: 0,
  totalOrders: 0,
  totalRevenue: 0,
  unpaidOrders: 0,
}

const numberFormatter = new Intl.NumberFormat('en-US')
const currencyFormatter = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 })

function formatNumber(value: number) {
  return numberFormatter.format(value ?? 0)
}

function formatCurrency(value: number) {
  return `${currencyFormatter.format(value ?? 0)} ₫`
}

function formatDate(value?: string | null) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value))
}

function formatDateTime(value?: string | null) {
  if (!value) return ''
  return new Intl.DateTimeFormat('vi-VN', {
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(value))
}

function isoDate(date: Date) {
  return date.toISOString().slice(0, 10)
}

function shortcutRange(shortcut: string, customStart: string, customEnd: string) {
  const today = new Date()
  const endDate = isoDate(today)

  if (shortcut === 'today') return { endDate, startDate: endDate }

  if (shortcut === 'week') {
    const start = new Date(today)
    start.setDate(today.getDate() - 7)
    return { endDate, startDate: isoDate(start) }
  }

  if (shortcut === 'month') {
    const start = new Date(today)
    start.setDate(today.getDate() - 30)
    return { endDate, startDate: isoDate(start) }
  }

  if (shortcut === 'custom') return { endDate: customEnd || undefined, startDate: customStart || undefined }

  return { endDate: undefined, startDate: undefined }
}

function StatCard({ icon, label, tone, value }: { icon: string; label: string; tone: string; value: string }) {
  return (
    <div className="flex items-center gap-4 rounded-xl border border-slate-100 bg-white p-4 shadow-sm">
      <div className={`flex size-12 items-center justify-center rounded-lg border ${tone}`}>
        <span className="material-symbols-outlined">{icon}</span>
      </div>
      <div>
        <p className="text-xs font-medium uppercase tracking-wider text-slate-500">{label}</p>
        <h3 className="text-2xl font-bold text-slate-900">{value}</h3>
      </div>
    </div>
  )
}

function LoadingRows() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, index) => (
        <tr key={index} className="animate-pulse">
          <td colSpan={7} className="px-6 py-4">
            <div className="h-12 rounded-lg bg-slate-100" />
          </td>
        </tr>
      ))}
    </>
  )
}

function SortIcon({ active, direction }: { active: boolean; direction: string }) {
  let icon = 'unfold_more'
  if (active && direction === 'asc') icon = 'arrow_upward'
  if (active && direction === 'desc') icon = 'arrow_downward'

  return (
    <span className={`material-symbols-outlined text-[16px] transition-all ${active ? 'text-primary' : 'text-slate-300 group-hover:text-slate-400'}`}>
      {icon}
    </span>
  )
}

function paginationPages(current: number, total: number) {
  let start = Math.max(1, current - 2)
  const end = Math.min(total, start + 4)
  if (end - start < 4 && start > 1) start = Math.max(1, end - 4)

  const pages: Array<number | 'ellipsis-start' | 'ellipsis-end'> = []
  if (start > 1) {
    pages.push(1)
    if (start > 2) pages.push('ellipsis-start')
  }
  for (let page = start; page <= end; page += 1) pages.push(page)
  if (end < total) {
    if (end < total - 1) pages.push('ellipsis-end')
    pages.push(total)
  }
  return pages
}

export default function AdminOrdersPage() {
  const navigate = useNavigate()
  const [orders, setOrders] = useState<PagedResult<AdminOrderDto> | null>(null)
  const [stats, setStats] = useState<AdminOrderStatsDto>(emptyStats)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [isPaid, setIsPaid] = useState<boolean | ''>('')
  const [dateShortcut, setDateShortcut] = useState('')
  const [customStart, setCustomStart] = useState('')
  const [customEnd, setCustomEnd] = useState('')
  const [minPrice, setMinPrice] = useState<number | ''>('')
  const [maxPrice, setMaxPrice] = useState<number | ''>('')
  const [sortBy, setSortBy] = useState('')
  const [sortDirection, setSortDirection] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const [showAdvanced, setShowAdvanced] = useState(false)

  const advancedActiveCount = (dateShortcut ? 1 : 0) + (minPrice !== '' || maxPrice !== '' ? 1 : 0)
  const pageCount = orders?.totalPages || Math.max(1, Math.ceil((orders?.totalCount ?? 0) / (orders?.pageSize ?? PAGE_SIZE)))

  const rangeText = useMemo(() => {
    const total = orders?.totalCount ?? 0
    const page = orders?.pageNumber ?? pageNumber
    const pageSize = orders?.pageSize ?? PAGE_SIZE
    const start = total === 0 ? 0 : (page - 1) * pageSize + 1
    const end = Math.min(page * pageSize, total)
    return { end, start, total }
  }, [orders, pageNumber])

  const loadOrders = async (
    nextPage = pageNumber,
    overrides?: {
      isPaid?: boolean | ''
      sortBy?: string
      sortDirection?: string
    },
  ) => {
    setLoading(true)
    setError('')

    const effectiveIsPaid = overrides?.isPaid ?? isPaid
    const effectiveSortBy = overrides?.sortBy ?? sortBy
    const effectiveSortDirection = overrides?.sortDirection ?? sortDirection
    const range = shortcutRange(dateShortcut, customStart, customEnd)

    try {
      const data = await adminApi.orders.list({
        endDate: range.endDate,
        isPaid: effectiveIsPaid,
        maxPrice,
        minPrice,
        pageNumber: nextPage,
        pageSize: PAGE_SIZE,
        searchTerm: searchTerm.trim() || undefined,
        sortBy: effectiveSortBy || undefined,
        sortDirection: effectiveSortDirection || undefined,
        startDate: range.startDate,
      })

      setOrders(data.orders)
      setStats(data.stats)
      setPageNumber(data.orders.pageNumber)
    } catch (err) {
      setOrders(null)
      setError(err instanceof Error ? err.message : 'Unable to load orders.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadOrders(1)
  }, [])

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadOrders(1)
  }

  const handlePaidChange = (value: string) => {
    const nextIsPaid = value === '' ? '' : value === 'true'
    setIsPaid(nextIsPaid)
    void loadOrders(1, { isPaid: nextIsPaid })
  }

  const handleReset = () => {
    setSearchTerm('')
    setIsPaid('')
    setDateShortcut('')
    setCustomStart('')
    setCustomEnd('')
    setMinPrice('')
    setMaxPrice('')
    setSortBy('')
    setSortDirection('')
    setShowAdvanced(false)
    setTimeout(() => void loadOrders(1, { isPaid: '', sortBy: '', sortDirection: '' }), 0)
  }

  const handleSort = (column: string) => {
    // eslint-disable-next-line no-useless-assignment
    let nextDirection = ''
    let nextSortBy = column

    if (sortBy !== column) nextDirection = 'desc'
    else if (sortDirection === 'desc') nextDirection = 'asc'
    else if (sortDirection === 'asc') {
      nextSortBy = ''
      nextDirection = ''
    } else nextDirection = 'desc'

    setSortBy(nextSortBy)
    setSortDirection(nextDirection)
    void loadOrders(1, { sortBy: nextSortBy, sortDirection: nextDirection })
  }

  const goToPage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > pageCount || loading) return
    void loadOrders(nextPage)
  }

  const tableRows = orders?.items ?? []
  const showAdvancedPanel = showAdvanced || advancedActiveCount > 0

  return (
    <AdminLayout activePage="Orders" breadcrumb={['Dashboard', 'Order Management']} pageHeader="Order Management">
      <div className="flex flex-col gap-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard icon="receipt_long" label="Total Orders" tone="border-blue-100/50 bg-blue-50 text-blue-600" value={formatNumber(stats.totalOrders)} />
          <StatCard icon="check_circle" label="Paid Orders" tone="border-green-100/50 bg-green-50 text-green-600" value={formatNumber(stats.paidOrders)} />
          <StatCard icon="pending_actions" label="Unpaid Orders" tone="border-red-100/50 bg-red-50 text-red-600" value={formatNumber(stats.unpaidOrders)} />
          <StatCard icon="payments" label="Total Revenue" tone="border-indigo-100/50 bg-indigo-50 text-indigo-600" value={formatCurrency(stats.totalRevenue)} />
        </div>

        <div className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <form id="filter-form" onSubmit={handleSearch} className="flex flex-col">
            <div className="flex flex-col items-start gap-3 p-4 lg:flex-row lg:items-center">
              <div className="relative w-full flex-1">
                <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-400">
                  <span className="material-symbols-outlined text-[20px]">search</span>
                </span>
                <input
                  value={searchTerm}
                  onChange={(event) => setSearchTerm(event.target.value)}
                  className="w-full rounded-lg border border-slate-200 bg-slate-50 py-2.5 pl-10 pr-4 text-sm text-slate-900 placeholder:text-slate-400 transition-colors focus:border-primary focus:bg-white focus:outline-none focus:ring-2 focus:ring-primary/20"
                  placeholder="Search by order code, customer name, receiver name..."
                  type="text"
                  autoComplete="off"
                />
              </div>

              <div className="w-full shrink-0 lg:w-44">
                <select
                  value={isPaid === '' ? '' : String(isPaid)}
                  onChange={(event) => handlePaidChange(event.target.value)}
                  className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-900 shadow-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">All Payment Statuses</option>
                  <option value="true">Paid</option>
                  <option value="false">Unpaid</option>
                </select>
              </div>

              <div className="flex w-full shrink-0 items-center gap-2 lg:w-auto">
                <button
                  type="submit"
                  className="flex flex-1 items-center justify-center gap-2 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-white shadow-sm shadow-blue-500/20 transition-all hover:bg-blue-700 lg:flex-none"
                >
                  <span className="material-symbols-outlined text-[18px]">search</span>
                  <span>Search</span>
                </button>

                <button
                  type="button"
                  onClick={() => setShowAdvanced((current) => !current)}
                  className={`flex items-center justify-center gap-1.5 whitespace-nowrap rounded-lg border px-3.5 py-2.5 text-sm text-slate-600 shadow-sm transition-colors hover:bg-slate-50 hover:border-slate-300 ${
                    showAdvancedPanel ? 'border-slate-300 bg-slate-50 text-slate-900' : 'border-slate-200 bg-white'
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
                  onClick={handleReset}
                  className="flex items-center justify-center rounded-lg border border-slate-200 bg-white p-2.5 text-slate-500 shadow-sm transition-colors hover:bg-slate-50 hover:text-slate-800"
                  title="Reset all filters"
                >
                  <span className="material-symbols-outlined text-[18px]">restart_alt</span>
                </button>

                <button
                  type="button"
                  title="Export is a visual placeholder in the migrated page."
                  className="flex items-center justify-center gap-1.5 whitespace-nowrap rounded-lg border border-slate-200 bg-white px-3.5 py-2.5 text-sm text-slate-600 shadow-sm transition-colors hover:bg-slate-50"
                >
                  <span className="material-symbols-outlined text-[18px]">file_download</span>
                  <span className="hidden font-medium sm:inline">Export</span>
                </button>
              </div>
            </div>

            {showAdvancedPanel && (
              <div className="flex flex-col gap-5 border-t border-slate-100 bg-slate-50/40 px-4 pb-5 pt-4">
                <div className="flex flex-wrap items-start gap-4 lg:flex-nowrap lg:gap-6">
                  <div className="w-full space-y-1 lg:w-72 lg:shrink-0">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">payments</span> Total Range (₫)
                    </label>
                    <div className="flex items-center gap-1.5">
                      <input
                        value={minPrice}
                        onChange={(event) => setMinPrice(event.target.value === '' ? '' : Number(event.target.value))}
                        type="number"
                        placeholder="Min"
                        className="h-[38px] min-w-0 w-full rounded-lg border border-slate-200 bg-white px-2.5 py-2 text-sm shadow-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20"
                      />
                      <span className="shrink-0 text-lg font-light text-slate-300">-</span>
                      <input
                        value={maxPrice}
                        onChange={(event) => setMaxPrice(event.target.value === '' ? '' : Number(event.target.value))}
                        type="number"
                        placeholder="Max"
                        className="h-[38px] min-w-0 w-full rounded-lg border border-slate-200 bg-white px-2.5 py-2 text-sm shadow-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20"
                      />
                    </div>
                  </div>

                  <div className="w-full space-y-1 lg:w-80 lg:shrink-0">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">calendar_today</span> Order Date
                    </label>
                    <div className="flex flex-col gap-2">
                      <select
                        value={dateShortcut}
                        onChange={(event) => {
                          const value = event.target.value
                          setDateShortcut(value)
                          if (value !== 'custom') {
                            setCustomStart('')
                            setCustomEnd('')
                          }
                        }}
                        className="h-[40px] w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                      >
                        <option value="">All Time</option>
                        <option value="today">Today</option>
                        <option value="week">This Week</option>
                        <option value="month">This Month</option>
                        <option value="custom">Custom Range</option>
                      </select>
                      {dateShortcut === 'custom' && (
                        <div className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2">
                          <span className="material-symbols-outlined text-[18px] text-slate-400">event</span>
                          <input value={customStart} onChange={(event) => setCustomStart(event.target.value)} type="date" className="min-w-0 flex-1 bg-transparent text-sm outline-none" />
                          <span className="text-slate-300">to</span>
                          <input value={customEnd} onChange={(event) => setCustomEnd(event.target.value)} type="date" className="min-w-0 flex-1 bg-transparent text-sm outline-none" />
                        </div>
                      )}
                    </div>
                  </div>

                  <div className="mt-2 w-full lg:ms-auto lg:mt-[22px] lg:w-auto">
                    <button
                      type="submit"
                      className="flex h-[40px] w-full items-center justify-center gap-2 rounded-lg bg-slate-800 px-6 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-900 sm:w-auto"
                    >
                      <span className="material-symbols-outlined text-[17px]">check</span>
                      Apply Filters
                    </button>
                  </div>
                </div>
              </div>
            )}
          </form>
        </div>

        {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}

        <div className="flex flex-col overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-left">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50/50">
                  <th className="px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button type="button" onClick={() => handleSort('orderCode')} className="group inline-flex w-full items-center gap-1 transition-colors hover:text-primary">
                      Order Code
                      <SortIcon active={sortBy === 'orderCode'} direction={sortDirection} />
                    </button>
                  </th>
                  <th className="hidden px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">Customer</th>
                  <th className="hidden px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">Receiver</th>
                  <th className="px-3 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button type="button" onClick={() => handleSort('grandTotal')} className="group inline-flex w-full items-center justify-end gap-1 transition-colors hover:text-primary">
                      Total
                      <SortIcon active={sortBy === 'grandTotal'} direction={sortDirection} />
                    </button>
                  </th>
                  <th className="px-3 py-4 text-center text-xs font-semibold uppercase tracking-wider text-slate-500">Payment Status</th>
                  <th className="py-4 pl-3 pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button type="button" onClick={() => handleSort('createdAt')} className="group inline-flex w-full items-center justify-end gap-1 transition-colors hover:text-primary">
                      Created
                      <SortIcon active={sortBy === 'createdAt'} direction={sortDirection} />
                    </button>
                  </th>
                  <th className="px-6 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  <LoadingRows />
                ) : tableRows.length ? (
                  tableRows.map((order) => (
                    <tr
                      key={order.id}
                      onClick={() => navigate(`/admin/orders/${order.id}`)}
                      className="group/row cursor-pointer transition-all hover:bg-slate-50"
                    >
                      <td className="px-6 py-4 align-middle font-mono text-sm font-semibold text-slate-900 transition-colors group-hover/row:text-primary">
                        #{order.orderCode}
                      </td>
                      <td className="hidden whitespace-nowrap px-3 py-4 align-middle md:table-cell">
                        <span className="text-sm text-slate-600">{order.customerName}</span>
                      </td>
                      <td className="hidden whitespace-nowrap px-3 py-4 align-middle md:table-cell">
                        <span className="text-sm font-medium text-slate-700">{order.receiverName}</span>
                        <div className="mt-0.5 text-xs text-slate-500">{order.receiverPhone}</div>
                      </td>
                      <td className="whitespace-nowrap px-3 py-4 text-right align-middle text-sm font-semibold text-slate-900">{formatCurrency(order.grandTotal)}</td>
                      <td className="whitespace-nowrap px-3 py-4 text-center align-middle">
                        {order.paidAt ? (
                          <>
                            <span className="inline-flex items-center gap-1.5 rounded-full bg-green-50 px-2.5 py-1.5 text-xs font-semibold text-green-700 ring-1 ring-inset ring-green-600/20">
                              <span className="size-1.5 rounded-full bg-green-600" />
                              Paid
                            </span>
                            <div className="mt-1 text-[10px] text-slate-500">{formatDateTime(order.paidAt)}</div>
                          </>
                        ) : (
                          <span className="inline-flex items-center gap-1.5 rounded-full bg-amber-50 px-2.5 py-1.5 text-xs font-semibold text-amber-700 ring-1 ring-inset ring-amber-600/20">
                            <span className="size-1.5 rounded-full bg-amber-500" />
                            Unpaid
                          </span>
                        )}
                      </td>
                      <td className="whitespace-nowrap px-3 py-4 text-right align-middle">
                        <p className="text-sm text-slate-500">{formatDate(order.createdAt)}</p>
                      </td>
                      <td className="whitespace-nowrap px-6 py-4 text-right align-middle">
                        <Link
                          to={`/admin/orders/${order.id}`}
                          onClick={(event) => event.stopPropagation()}
                          className="inline-flex items-center justify-center rounded-lg p-2 text-slate-400 transition-all hover:bg-primary/5 hover:text-primary"
                          title="View Details"
                        >
                          <span className="material-symbols-outlined text-[20px]">visibility</span>
                        </Link>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="py-12 text-center text-slate-500">
                      <div className="flex flex-col items-center justify-center gap-3">
                        <div className="flex size-16 items-center justify-center rounded-full bg-slate-50">
                          <span className="material-symbols-outlined text-4xl text-slate-300">receipt_long</span>
                        </div>
                        <p className="mt-2 text-base font-medium text-slate-900">No orders found</p>
                        <p className="text-sm">We couldn't find any orders matching your criteria.</p>
                        <button type="button" className="mt-2 text-sm font-medium text-primary hover:underline" onClick={handleReset}>
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
              Showing <span className="font-medium text-slate-900">{rangeText.start}</span> to{' '}
              <span className="font-medium text-slate-900">{rangeText.end}</span> of{' '}
              <span className="font-medium text-slate-900">{formatNumber(rangeText.total)}</span> orders
            </div>

            {pageCount > 1 && (
              <nav aria-label="Pagination" className="flex items-center gap-1">
                <button
                  type="button"
                  disabled={pageNumber <= 1 || loading}
                  onClick={() => goToPage(pageNumber - 1)}
                  className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:cursor-not-allowed disabled:border-slate-100 disabled:bg-slate-50 disabled:text-slate-300"
                >
                  <span className="material-symbols-outlined text-lg leading-none">chevron_left</span>
                </button>

                <div className="flex items-center gap-1 px-1">
                  {paginationPages(pageNumber, pageCount).map((page) =>
                    typeof page === 'number' ? (
                      <button
                        key={page}
                        type="button"
                        onClick={() => goToPage(page)}
                        className={`flex h-9 min-w-[36px] items-center justify-center rounded-lg text-sm font-medium transition-all ${
                          page === pageNumber
                            ? 'bg-primary text-white shadow-sm shadow-blue-500/20'
                            : 'border border-transparent text-slate-600 hover:border-slate-200 hover:bg-white hover:text-primary'
                        }`}
                      >
                        {page}
                      </button>
                    ) : (
                      <span key={page} className="px-1 text-slate-400">
                        ...
                      </span>
                    ),
                  )}
                </div>

                <button
                  type="button"
                  disabled={pageNumber >= pageCount || loading}
                  onClick={() => goToPage(pageNumber + 1)}
                  className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:cursor-not-allowed disabled:border-slate-100 disabled:bg-slate-50 disabled:text-slate-300"
                >
                  <span className="material-symbols-outlined text-lg leading-none">chevron_right</span>
                </button>
              </nav>
            )}
          </div>
        </div>
      </div>
    </AdminLayout>
  )
}
