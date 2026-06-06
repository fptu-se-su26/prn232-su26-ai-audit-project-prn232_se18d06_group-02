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
const MAX_REASON_LENGTH = 500

type StatusModalAction = 'approve' | 'reject' | 'lock' | 'suspend'

const emptyStats: StoreApplicationStatsDto = {
  approvedCount: 0,
  pendingCount: 0,
  rejectedCount: 0,
  totalCount: 0,
}

const statusOptions: Array<{ label: string; value: StoreStatus | '' }> = [
  { label: 'All Status', value: '' },
  { label: 'Approved', value: storeStatus.approved },
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
  if (status === storeStatus.rejected || status === storeStatus.locked) return 'bg-red-50 text-red-700 ring-red-600/10'
  return 'bg-slate-50 text-slate-700 ring-slate-600/20'
}

function statusDotClasses(status: StoreStatus) {
  if (status === storeStatus.pending) return 'bg-amber-500'
  if (status === storeStatus.approved) return 'bg-green-500'
  if (status === storeStatus.rejected || status === storeStatus.locked) return 'bg-red-500'
  return 'bg-slate-500'
}

function initials(name?: string | null) {
  return (name || 'ST').slice(0, 2).toUpperCase()
}

function actionCopy(action: StatusModalAction) {
  if (action === 'approve') {
    return {
      actionText: 'approve',
      buttonText: 'Approve',
      icon: 'check_circle',
      nextStatus: storeStatus.approved,
      title: 'Approve Store',
      tone: 'green',
    }
  }

  if (action === 'reject') {
    return {
      actionText: 'reject',
      buttonText: 'Confirm Rejection',
      icon: 'warning',
      nextStatus: storeStatus.rejected,
      title: 'Reject Registration',
      tone: 'red',
    }
  }

  if (action === 'suspend') {
    return {
      actionText: 'suspend',
      buttonText: 'Confirm Suspend',
      icon: 'warning',
      nextStatus: storeStatus.locked,
      title: 'Suspend Store',
      tone: 'red',
    }
  }

  return {
    actionText: 'lock',
    buttonText: 'Confirm Lock',
    icon: 'warning',
    nextStatus: storeStatus.locked,
    title: 'Lock Store',
    tone: 'red',
  }
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
          <td colSpan={7} className="px-6 py-4">
            <div className="h-10 rounded-lg bg-slate-100" />
          </td>
        </tr>
      ))}
    </>
  )
}

export default function AdminStoresPage() {
  const navigate = useNavigate()
  const [stores, setStores] = useState<PagedResult<StoreApplicationDto> | null>(null)
  const [stats, setStats] = useState<StoreApplicationStatsDto>(emptyStats)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(true)
  const [actionLoading, setActionLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [status, setStatus] = useState<StoreStatus | ''>('')
  const [dateShortcut, setDateShortcut] = useState('')
  const [customStart, setCustomStart] = useState('')
  const [customEnd, setCustomEnd] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const [modalStore, setModalStore] = useState<StoreApplicationDto | null>(null)
  const [modalAction, setModalAction] = useState<StatusModalAction | null>(null)
  const [reason, setReason] = useState('')
  const [validationError, setValidationError] = useState('')

  const totalStores = stats.totalCount - stats.pendingCount - stats.rejectedCount
  const lockedStores = stats.totalCount - stats.pendingCount - stats.approvedCount - stats.rejectedCount

  const totalPages = useMemo(() => {
    if (!stores) return 1
    return stores.totalPages || Math.max(1, Math.ceil(stores.totalCount / stores.pageSize))
  }, [stores])

  const rangeText = useMemo(() => {
    const total = stores?.totalCount ?? 0
    const page = stores?.pageNumber ?? pageNumber
    const pageSize = stores?.pageSize ?? PAGE_SIZE
    const start = total === 0 ? 0 : (page - 1) * pageSize + 1
    const end = Math.min(page * pageSize, total)
    return { end, start, total }
  }, [stores, pageNumber])

  const loadStores = async (nextPage = pageNumber, nextShortcut = dateShortcut) => {
    setLoading(true)
    setError('')

    const range = shortcutRange(nextShortcut, customStart, customEnd)

    try {
      const [list, nextStats] = await Promise.all([
        adminApi.stores.list({
          endDate: range.endDate,
          pageNumber: nextPage,
          pageSize: PAGE_SIZE,
          searchTerm: searchTerm.trim() || undefined,
          startDate: range.startDate,
          status,
        }),
        adminApi.stores.stats(),
      ])

      setStores(list)
      setStats(nextStats)
      setPageNumber(list.pageNumber)
    } catch (err) {
      setStores(null)
      setError(err instanceof Error ? err.message : 'Unable to load stores.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadStores(1)
  }, [])

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadStores(1)
  }

  const handleShortcutChange = (value: string) => {
    setDateShortcut(value)
    if (value !== 'custom') {
      setCustomStart('')
      setCustomEnd('')
      void loadStores(1, value)
    }
  }

  const goToPage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > totalPages || loading) return
    void loadStores(nextPage)
  }

  const openStatusModal = (store: StoreApplicationDto, action: StatusModalAction) => {
    setModalStore(store)
    setModalAction(action)
    setReason('')
    setValidationError('')
  }

  const closeModal = () => {
    if (actionLoading) return
    setModalStore(null)
    setModalAction(null)
    setReason('')
    setValidationError('')
  }

  const changeStatus = async (store: StoreApplicationDto, nextStatus: StoreStatus, nextReason?: string) => {
    setActionLoading(true)
    setError('')
    setSuccess('')

    try {
      const message = await adminApi.stores.changeStatus(store.id, { reason: nextReason, status: nextStatus })
      setSuccess(message)
      closeModal()
      await loadStores(pageNumber)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to update store status.')
    } finally {
      setActionLoading(false)
    }
  }

  const handleActivate = (store: StoreApplicationDto) => {
    void changeStatus(store, storeStatus.approved)
  }

  const handleModalSubmit = () => {
    if (!modalStore || !modalAction) return

    const copy = actionCopy(modalAction)
    const trimmedReason = reason.trim()
    const requiresReason = copy.nextStatus === storeStatus.locked || copy.nextStatus === storeStatus.rejected

    if (requiresReason && !trimmedReason) {
      setValidationError('Reason is required for this status change.')
      return
    }

    if (trimmedReason.length > MAX_REASON_LENGTH) {
      setValidationError(`Reason cannot exceed ${MAX_REASON_LENGTH} characters.`)
      return
    }

    void changeStatus(modalStore, copy.nextStatus, trimmedReason || undefined)
  }

  const modalCopy = modalAction ? actionCopy(modalAction) : null

  return (
    <AdminLayout activePage="Stores" breadcrumb={['Dashboard', 'Store Management']} pageHeader="Store Management">
      <div className="flex flex-col gap-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard icon="store" label="Total Stores" tone="bg-blue-50 text-blue-600" value={totalStores} />
          <StatCard icon="check_circle" label="Active Stores" tone="bg-emerald-50 text-emerald-600" value={stats.approvedCount} />
          <StatCard icon="block" label="Suspended/Locked" tone="bg-red-50 text-red-600" value={lockedStores} />
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
                placeholder="Search Store or Tax Code..."
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
        {success && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">{success}</div>}

        <div className="flex flex-col overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <div className="w-full overflow-x-auto">
            <table className="w-full min-w-max border-collapse text-left">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50/50">
                  <th className="whitespace-nowrap py-4 pl-6 pr-3 text-xs font-semibold uppercase tracking-wider text-slate-500">Store Name</th>
                  <th className="whitespace-nowrap px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Owner</th>
                  <th className="whitespace-nowrap px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Email</th>
                  <th className="whitespace-nowrap px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Phone</th>
                  <th className="whitespace-nowrap px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Status</th>
                  <th className="whitespace-nowrap px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Created Date</th>
                  <th className="whitespace-nowrap py-4 pl-3 pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  <LoadingRows />
                ) : stores?.items.length ? (
                  stores.items.map((store) => (
                    <tr
                      key={store.id}
                      onClick={() => navigate(`/admin/stores/${store.id}`)}
                      className="group cursor-pointer transition-all hover:bg-slate-50"
                    >
                      <td className="whitespace-nowrap py-4 pl-6 pr-3">
                        <div className="flex items-center gap-3">
                          <div className="flex size-9 shrink-0 items-center justify-center rounded-lg bg-blue-100 text-xs font-bold text-blue-600 shadow-sm">
                            {initials(store.storeName)}
                          </div>
                          <span className="text-sm font-semibold text-slate-900 transition-colors group-hover:text-primary">
                            {store.storeName || 'N/A'}
                          </span>
                        </div>
                      </td>
                      <td className="whitespace-nowrap px-3 py-4 text-slate-600">
                        <span className="text-sm font-medium text-slate-900">{store.ownerName}</span>
                      </td>
                      <td className="whitespace-nowrap px-3 py-4 text-sm text-slate-500">{store.email}</td>
                      <td className="whitespace-nowrap px-3 py-4 text-sm text-slate-500">{store.phone}</td>
                      <td className="whitespace-nowrap px-3 py-4">
                        <span
                          className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset ${statusBadgeClasses(
                            store.status,
                          )}`}
                        >
                          <span className={`size-1.5 rounded-full ${statusDotClasses(store.status)}`} />
                          {statusLabel(store.status)}
                        </span>
                      </td>
                      <td className="whitespace-nowrap px-3 py-4">
                        <div className="flex flex-col">
                          <span className="text-sm text-slate-900">{formatDate(store.createdAt)}</span>
                          <span className="text-[10px] font-bold uppercase tracking-tighter text-slate-400">{formatTime(store.createdAt)}</span>
                        </div>
                      </td>
                      <td className="whitespace-nowrap py-4 pl-3 pr-6 text-right">
                        <div
                          onClick={(event) => event.stopPropagation()}
                          className="flex items-center justify-end gap-2 opacity-0 transition-opacity group-hover:opacity-100"
                        >
                          <Link to={`/admin/stores/${store.id}`} className="p-1 text-slate-400 transition-colors hover:text-primary" title="View Details">
                            <span className="material-symbols-outlined text-[20px]">visibility</span>
                          </Link>
                          {store.status === storeStatus.approved && (
                            <>
                              <button
                                type="button"
                                onClick={() => openStatusModal(store, 'suspend')}
                                className="p-1 text-slate-400 transition-colors hover:text-amber-500"
                                title="Suspend"
                              >
                                <span className="material-symbols-outlined text-[20px]">block</span>
                              </button>
                              <button
                                type="button"
                                onClick={() => openStatusModal(store, 'lock')}
                                className="p-1 text-slate-400 transition-colors hover:text-red-500"
                                title="Lock"
                              >
                                <span className="material-symbols-outlined text-[20px]">lock</span>
                              </button>
                            </>
                          )}
                          {store.status === storeStatus.locked && (
                            <button
                              type="button"
                              disabled={actionLoading}
                              onClick={() => handleActivate(store)}
                              className="p-1 text-slate-400 transition-colors hover:text-green-500 disabled:opacity-60"
                              title="Activate"
                            >
                              <span className="material-symbols-outlined text-[20px]">lock_open</span>
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="px-6 py-8 text-center text-slate-500">
                      No stores found matching your criteria.
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
              <span className="font-medium text-slate-900">{rangeText.total}</span> stores
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

      {modalStore && modalCopy && (
        <div className="fixed inset-0 z-[60] flex items-center justify-center bg-slate-900/50 p-4 backdrop-blur-sm">
          <button type="button" aria-label="Close modal" className="absolute inset-0 cursor-default" onClick={closeModal} />
          <div className="relative mx-auto w-full max-w-lg rounded-2xl border border-slate-100 bg-white p-6 shadow-xl">
            <div className={modalAction === 'approve' ? 'flex flex-col items-center justify-center gap-3 text-center' : 'flex gap-4'}>
              <div
                className={`${
                  modalAction === 'approve' ? 'mx-auto mb-2 size-16 ring-8 ring-green-50/50' : 'mt-1 size-10'
                } flex shrink-0 items-center justify-center rounded-full ${
                  modalCopy.tone === 'green' ? 'bg-green-50 text-green-600' : 'bg-red-100 text-red-600'
                }`}
              >
                <span className="material-symbols-outlined text-3xl">{modalCopy.icon}</span>
              </div>
              <div className={modalAction === 'approve' ? '' : 'w-full'}>
                <h3 className="text-xl font-bold text-slate-900">{modalCopy.title}</h3>
                <p className="mt-1 text-sm text-slate-500">
                  You are about to <span>{modalCopy.actionText}</span> the store{' '}
                  <span className="font-bold text-slate-900">{modalStore.storeName}</span>.
                </p>

                {modalAction !== 'approve' && (
                  <div className="mt-6 w-full">
                    <label htmlFor="statusReason" className="mb-2 block text-sm font-bold text-slate-700">
                      Reason
                    </label>
                    <textarea
                      id="statusReason"
                      rows={4}
                      maxLength={MAX_REASON_LENGTH}
                      value={reason}
                      onChange={(event) => {
                        setReason(event.target.value)
                        setValidationError('')
                      }}
                      className="w-full resize-none rounded-xl border border-slate-200 px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                      placeholder="Please provide reason..."
                      required
                    />
                    <div className="mt-1 flex justify-between text-xs">
                      <span className="font-medium text-red-600">{validationError}</span>
                      <span className="text-slate-400">{reason.length}/{MAX_REASON_LENGTH}</span>
                    </div>
                  </div>
                )}
              </div>
            </div>

            <div className="mt-6 flex justify-end gap-3 border-t border-slate-100 pt-5">
              <button
                type="button"
                onClick={closeModal}
                className="rounded-xl border border-slate-200 bg-white px-5 py-2 text-sm font-bold text-slate-600 transition-colors hover:bg-slate-50"
              >
                Cancel
              </button>
              <button
                type="button"
                disabled={actionLoading}
                onClick={handleModalSubmit}
                className={`rounded-xl px-6 py-2 text-sm font-bold text-white transition-colors disabled:opacity-60 ${
                  modalCopy.tone === 'green' ? 'bg-primary hover:bg-blue-700' : 'bg-red-600 hover:bg-red-700'
                }`}
              >
                {actionLoading ? 'Processing...' : modalCopy.buttonText}
              </button>
            </div>
          </div>
        </div>
      )}
    </AdminLayout>
  )
}
