/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  adminApi,
  storeStatus,
  type PagedResult,
  type StoreApplicationDto,
  type StoreApplicationStatsDto,
  type StoreStatus,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const PAGE_SIZE = 10

const emptyStats: StoreApplicationStatsDto = {
  approvedCount: 0,
  pendingCount: 0,
  rejectedCount: 0,
  totalCount: 0,
}

const statusOptions: Array<{ label: string; value: StoreStatus | '' }> = [
  { label: 'All Status', value: '' },
  { label: 'Draft', value: storeStatus.draft },
  { label: 'Pending', value: storeStatus.pending },
  { label: 'Approved', value: storeStatus.approved },
  { label: 'Rejected', value: storeStatus.rejected },
  { label: 'Locked', value: storeStatus.locked },
]

const shortcutOptions = [
  { label: 'All Time', value: '' },
  { label: 'Today', value: 'today' },
  { label: 'This Week', value: 'week' },
  { label: 'This Month', value: 'month' },
  { label: 'Custom Range', value: 'custom' },
]

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

function formatDate(value?: string | null) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value))
}

function formatTime(value?: string | null) {
  if (!value) return ''
  return new Intl.DateTimeFormat('en-US', { hour: '2-digit', minute: '2-digit' }).format(new Date(value))
}

function statusLabel(status: StoreStatus) {
  if (status === storeStatus.pending) return 'Pending'
  if (status === storeStatus.approved) return 'Approved'
  if (status === storeStatus.rejected) return 'Rejected'
  if (status === storeStatus.locked) return 'Locked'
  return 'Draft'
}

function statusBadgeClasses(status: StoreStatus) {
  if (status === storeStatus.pending) return 'bg-amber-50 text-amber-700 ring-amber-600/20'
  if (status === storeStatus.approved) return 'bg-green-50 text-green-700 ring-green-600/20'
  if (status === storeStatus.rejected) return 'bg-red-50 text-red-700 ring-red-600/10'
  return 'bg-slate-50 text-slate-700 ring-slate-600/20'
}

function statusDotClasses(status: StoreStatus) {
  if (status === storeStatus.pending) return 'bg-amber-500'
  if (status === storeStatus.approved) return 'bg-green-500'
  if (status === storeStatus.rejected) return 'bg-red-500'
  return 'bg-slate-500'
}

function initials(name: string) {
  return (name || 'ST').slice(0, 2).toUpperCase()
}

function StatCard({ icon, label, tone, value }: { icon: string; label: string; tone: string; value: number }) {
  return (
    <div className="flex items-center gap-4 rounded-xl border border-slate-100 bg-white p-4 shadow-sm">
      <div className={`flex size-12 items-center justify-center rounded-lg ${tone}`}>
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
          <td colSpan={8} className="px-6 py-4">
            <div className="h-10 rounded-lg bg-slate-100" />
          </td>
        </tr>
      ))}
    </>
  )
}

export default function AdminStoreApplicationsPage() {
  const navigate = useNavigate()
  const [applications, setApplications] = useState<PagedResult<StoreApplicationDto> | null>(null)
  const [stats, setStats] = useState<StoreApplicationStatsDto>(emptyStats)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [status, setStatus] = useState<StoreStatus | ''>('')
  const [dateShortcut, setDateShortcut] = useState('')
  const [customStart, setCustomStart] = useState('')
  const [customEnd, setCustomEnd] = useState('')
  const [pageNumber, setPageNumber] = useState(1)

  const totalPages = useMemo(() => {
    if (!applications) return 1
    return applications.totalPages || Math.max(1, Math.ceil(applications.totalCount / applications.pageSize))
  }, [applications])

  const rangeText = useMemo(() => {
    const total = applications?.totalCount ?? 0
    const page = applications?.pageNumber ?? pageNumber
    const pageSize = applications?.pageSize ?? PAGE_SIZE
    const start = total === 0 ? 0 : (page - 1) * pageSize + 1
    const end = Math.min(page * pageSize, total)
    return { end, start, total }
  }, [applications, pageNumber])

  const loadApplications = async (nextPage = pageNumber, nextShortcut = dateShortcut) => {
    setLoading(true)
    setError('')

    const range = shortcutRange(nextShortcut, customStart, customEnd)

    try {
      const [list, nextStats] = await Promise.all([
        adminApi.storeApplications.list({
          endDate: range.endDate,
          pageNumber: nextPage,
          pageSize: PAGE_SIZE,
          searchTerm: searchTerm.trim() || undefined,
          startDate: range.startDate,
          status,
        }),
        adminApi.storeApplications.stats(),
      ])

      setApplications(list)
      setStats(nextStats)
      setPageNumber(list.pageNumber)
    } catch (err) {
      setApplications(null)
      setError(err instanceof Error ? err.message : 'Unable to load store applications.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadApplications(1)
  }, [])

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadApplications(1)
  }

  const handleShortcutChange = (value: string) => {
    setDateShortcut(value)
    if (value !== 'custom') {
      setCustomStart('')
      setCustomEnd('')
      void loadApplications(1, value)
    }
  }

  const goToPage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > totalPages || loading) return
    void loadApplications(nextPage)
  }

  return (
    <AdminLayout activePage="Store Applications" breadcrumb={['Dashboard', 'Store Applications']} pageHeader="Store Applications">
      <div className="flex flex-col gap-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard icon="store" label="Total Applications" tone="bg-blue-50 text-blue-600" value={stats.totalCount} />
          <StatCard icon="pending_actions" label="Pending Review" tone="bg-amber-50 text-amber-600" value={stats.pendingCount} />
          <StatCard icon="check_circle" label="Approved" tone="bg-green-50 text-green-600" value={stats.approvedCount} />
          <StatCard icon="cancel" label="Rejected" tone="bg-red-50 text-red-600" value={stats.rejectedCount} />
        </div>

        <form
          onSubmit={handleSearch}
          className="flex flex-col items-start justify-between gap-4 rounded-xl border border-slate-100 bg-white p-4 shadow-sm lg:flex-row"
        >
          <div className="flex w-full flex-1 flex-col items-start gap-4 sm:flex-row lg:w-auto">
            <div className="relative w-full sm:max-w-xs">
              <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-slate-400">
                <span className="material-symbols-outlined text-[20px]">search</span>
              </span>
              <input
                value={searchTerm}
                onChange={(event) => setSearchTerm(event.target.value)}
                className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 py-2 pl-10 pr-4 text-sm text-slate-900 placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                placeholder="Search Company or Tax Code..."
                type="text"
              />
            </div>

            <select
              value={status}
              onChange={(event) => setStatus(event.target.value === '' ? '' : (Number(event.target.value) as StoreStatus))}
              className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 sm:w-40"
            >
              {statusOptions.map((option) => (
                <option key={option.label} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>

            <button
              type="submit"
              className="flex h-[42px] w-full items-center justify-center gap-2 rounded-lg border border-primary bg-primary px-4 py-2.5 text-sm font-medium text-white shadow-sm transition hover:bg-blue-700 sm:w-auto"
            >
              <span className="material-symbols-outlined text-[20px]">search</span>
              Search
            </button>

            <div className="w-full space-y-2 sm:w-80">
              <select
                value={dateShortcut}
                onChange={(event) => handleShortcutChange(event.target.value)}
                className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm text-slate-900 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
              >
                {shortcutOptions.map((option) => (
                  <option key={option.value || 'all'} value={option.value}>
                    {option.label}
                  </option>
                ))}
              </select>

              {dateShortcut === 'custom' && (
                <div className="flex items-center gap-2 rounded-lg border border-slate-200 bg-slate-50 px-3 py-2">
                  <span className="material-symbols-outlined text-[18px] text-slate-400">calendar_today</span>
                  <input
                    value={customStart}
                    onChange={(event) => setCustomStart(event.target.value)}
                    type="date"
                    className="min-w-0 flex-1 bg-transparent text-sm outline-none"
                  />
                  <span className="text-slate-300">to</span>
                  <input
                    value={customEnd}
                    onChange={(event) => setCustomEnd(event.target.value)}
                    type="date"
                    className="min-w-0 flex-1 bg-transparent text-sm outline-none"
                  />
                </div>
              )}
            </div>
          </div>

          <button
            type="button"
            title="Export CSV is a visual placeholder in the migrated page."
            className="flex h-[42px] w-full items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-5 py-2.5 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50 lg:w-auto"
          >
            <span className="material-symbols-outlined text-[20px]">download</span>
            <span>Export CSV</span>
          </button>
        </form>

        {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}

        <div className="flex flex-col overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full min-w-[980px] border-collapse text-left">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50/50">
                  <th className="py-4 pl-6 pr-3 text-xs font-semibold uppercase tracking-wider text-slate-500">Company Name</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Tax Code</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Business Type</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Rep Name</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Phone</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Submitted At</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Status</th>
                  <th className="py-4 pl-3 pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  <LoadingRows />
                ) : applications?.items.length ? (
                  applications.items.map((application) => (
                    <tr
                      key={application.id}
                      onClick={() => navigate(`/admin/store-applications/${application.id}`)}
                      className="group cursor-pointer transition-all hover:bg-slate-50"
                    >
                      <td className="py-4 pl-6 pr-3">
                        <div className="flex items-center gap-3">
                          <div className="flex size-9 items-center justify-center rounded-lg bg-blue-100 text-xs font-bold text-blue-600 shadow-sm">
                            {initials(application.storeName)}
                          </div>
                          <span className="text-sm font-semibold text-slate-900 transition-colors group-hover:text-primary">
                            {application.storeName}
                          </span>
                        </div>
                      </td>
                      <td className="px-3 py-4 font-mono text-xs uppercase tracking-tight text-slate-500">{application.taxCode}</td>
                      <td className="px-3 py-4 text-slate-600">{application.businessType}</td>
                      <td className="px-3 py-4 text-sm font-medium text-slate-900">{application.ownerName}</td>
                      <td className="px-3 py-4 text-slate-500">{application.phone}</td>
                      <td className="px-3 py-4">
                        <div className="flex flex-col">
                          <span className="text-slate-900">{formatDate(application.createdAt)}</span>
                          <span className="text-[10px] font-bold uppercase tracking-tighter text-slate-400">{formatTime(application.createdAt)}</span>
                        </div>
                      </td>
                      <td className="px-3 py-4">
                        <span
                          className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset ${statusBadgeClasses(
                            application.status,
                          )}`}
                        >
                          <span className={`size-1.5 rounded-full ${statusDotClasses(application.status)}`} />
                          {statusLabel(application.status)}
                        </span>
                      </td>
                      <td className="py-4 pl-3 pr-6 text-right">
                        <Link
                          to={`/admin/store-applications/${application.id}`}
                          onClick={(event) => event.stopPropagation()}
                          className="inline-flex p-1 text-slate-400 opacity-0 transition-all hover:text-primary group-hover:opacity-100"
                          title="View Details"
                        >
                          <span className="material-symbols-outlined text-[20px]">visibility</span>
                        </Link>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={8} className="px-6 py-8 text-center text-slate-500">
                      No applications found matching your criteria.
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
              <span className="font-medium text-slate-900">{rangeText.total}</span> applications
            </div>

            <nav aria-label="Pagination" className="flex items-center gap-1">
              <button
                type="button"
                disabled={pageNumber <= 1 || loading}
                onClick={() => goToPage(pageNumber - 1)}
                className="rounded-lg border border-slate-200 bg-white p-2 text-slate-400 transition-all hover:text-slate-600 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <span className="material-symbols-outlined text-[20px]">chevron_left</span>
              </button>

              <div className="flex items-center gap-1 px-1">
                <button
                  type="button"
                  className="size-9 rounded-lg border border-primary bg-primary text-sm font-bold text-white shadow-sm shadow-primary/20"
                >
                  {pageNumber}
                </button>
              </div>

              <button
                type="button"
                disabled={pageNumber >= totalPages || loading}
                onClick={() => goToPage(pageNumber + 1)}
                className="rounded-lg border border-slate-200 bg-white p-2 text-slate-400 transition-all hover:text-slate-600 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <span className="material-symbols-outlined text-[20px]">chevron_right</span>
              </button>
            </nav>
          </div>
        </div>
      </div>
    </AdminLayout>
  )
}
