/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  adminApi,
  type AdminBrandDto,
  type AdminCategoryDto,
  type AdminProductDto,
  type AdminProductMetadataDto,
  type AdminProductStatsDto,
  type PagedResult,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const PAGE_SIZE = 10

const emptyStats: AdminProductStatsDto = {
  activeProducts: 0,
  outOfStock: 0,
  pendingApproval: 0,
  totalProducts: 0,
}

interface ProductFilters {
  brandId: number | ''
  categoryId: number | ''
  customEnd: string
  customStart: string
  dateShortcut: string
  maxPrice: number | ''
  minPrice: number | ''
  outOfStock: boolean
  searchTerm: string
  sortBy: string
  sortDirection: string
  status: string
  storeId: string
}

type ProductActionType = 'approve' | 'reject' | 'suspend' | 'delete' | 'bulk-approve' | 'bulk-reject' | 'bulk-inactive'

interface PendingAction {
  icon: string
  product?: AdminProductDto
  reasonRequired: boolean
  tone: 'green' | 'red' | 'amber'
  type: ProductActionType
}

const defaultFilters: ProductFilters = {
  brandId: '',
  categoryId: '',
  customEnd: '',
  customStart: '',
  dateShortcut: '',
  maxPrice: '',
  minPrice: '',
  outOfStock: false,
  searchTerm: '',
  sortBy: '',
  sortDirection: '',
  status: '',
  storeId: '',
}

const numberFormatter = new Intl.NumberFormat('en-US')
const currencyFormatter = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 })

function formatNumber(value: number) {
  return numberFormatter.format(value ?? 0)
}

function formatCurrency(value: number) {
  return `${currencyFormatter.format(value ?? 0)} VND`
}

function formatDate(value?: string | null) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value))
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

function actionCopy(action: ProductActionType) {
  if (action === 'approve' || action === 'bulk-approve') return { action: 'Approve', title: 'Approve Product?' }
  if (action === 'reject' || action === 'bulk-reject') return { action: 'Reject', title: 'Reject Product?' }
  if (action === 'suspend' || action === 'bulk-inactive') return { action: 'Suspend', title: 'Suspend Product?' }
  return { action: 'Delete', title: 'Delete Product?' }
}

function statusConfig(status: string) {
  const normalized = status.toLowerCase()
  if (normalized === 'active' || normalized === 'approved') {
    return {
      icon: 'check_circle',
      label: normalized === 'approved' ? 'Approved' : 'Active',
      tone: 'border-green-200 bg-green-50 text-green-700',
    }
  }
  if (normalized === 'pending') return { icon: 'pending_actions', label: 'Pending', tone: 'border-amber-200 bg-amber-50 text-amber-700' }
  if (normalized === 'inactive' || normalized === 'suspended') return { icon: 'block', label: 'Inactive', tone: 'border-slate-200 bg-slate-100 text-slate-700' }
  if (normalized === 'outofstock' || normalized === 'out of stock') return { icon: 'production_quantity_limits', label: 'Out of Stock', tone: 'border-red-200 bg-red-50 text-red-700' }
  if (normalized === 'rejected') return { icon: 'cancel', label: 'Rejected', tone: 'border-red-200 bg-red-50 text-red-700' }
  return { icon: 'inventory_2', label: status || 'Draft', tone: 'border-slate-200 bg-slate-50 text-slate-600' }
}

function flattenCategories(categories: AdminCategoryDto[], depth = 0): Array<{ depth: number; item: AdminCategoryDto }> {
  return categories.flatMap((category) => [
    { depth, item: category },
    ...flattenCategories(category.children ?? [], depth + 1),
  ])
}

function buildCategoryOptions(categories: AdminCategoryDto[]) {
  const hasTree = categories.some((category) => (category.children ?? []).length > 0)
  if (hasTree) return flattenCategories(categories)

  const byParent = new Map<number | null, AdminCategoryDto[]>()
  categories.forEach((category) => {
    const parentId = category.parentId ?? null
    byParent.set(parentId, [...(byParent.get(parentId) ?? []), category])
  })

  const walk = (parentId: number | null, depth: number): Array<{ depth: number; item: AdminCategoryDto }> =>
    (byParent.get(parentId) ?? []).flatMap((category) => [
      { depth, item: category },
      ...walk(category.id, depth + 1),
    ])

  return walk(null, 0)
}

function paginationPages(current: number, total: number) {
  let start = Math.max(1, current - 2)
  let end = Math.min(total, start + 4)
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

function StatCard({
  clickable,
  icon,
  label,
  onClick,
  tone,
  value,
}: {
  clickable?: boolean
  icon: string
  label: string
  onClick?: () => void
  tone: string
  value: string
}) {
  const content = (
    <>
      <div className={`flex size-12 items-center justify-center rounded-lg border ${tone}`}>
        <span className="material-symbols-outlined">{icon}</span>
      </div>
      <div>
        <p className="text-xs font-medium uppercase tracking-wider text-slate-500">{label}</p>
        <h3 className="text-2xl font-bold text-slate-900">{value}</h3>
      </div>
    </>
  )

  if (clickable) {
    return (
      <button
        type="button"
        onClick={onClick}
        className="flex items-center gap-4 rounded-xl border border-slate-100 bg-white p-4 text-left shadow-sm transition-colors hover:border-amber-300"
      >
        {content}
      </button>
    )
  }

  return <div className="flex items-center gap-4 rounded-xl border border-slate-100 bg-white p-4 shadow-sm">{content}</div>
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

function StatusBadge({ status }: { status: string }) {
  const config = statusConfig(status)
  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-xs font-bold ${config.tone}`}>
      <span className="material-symbols-outlined text-[14px]">{config.icon}</span>
      {config.label}
    </span>
  )
}

function LoadingRows() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, index) => (
        <tr key={index} className="animate-pulse">
          <td colSpan={9} className="px-6 py-4">
            <div className="h-14 rounded-lg bg-slate-100" />
          </td>
        </tr>
      ))}
    </>
  )
}

function ModalShell({ children, onClose }: { children: ReactNode; onClose: () => void }) {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4 backdrop-blur-sm" role="dialog" aria-modal="true">
      <button type="button" className="absolute inset-0 cursor-default" aria-label="Close modal" onClick={onClose} />
      <div className="relative w-full max-w-md rounded-2xl border border-slate-100 bg-white p-6 shadow-2xl">{children}</div>
    </div>
  )
}

function ActionModal({
  busy,
  onClose,
  onConfirm,
  pendingAction,
}: {
  busy: boolean
  onClose: () => void
  onConfirm: (reason: string) => void
  pendingAction: PendingAction
}) {
  const [reason, setReason] = useState('')
  const copy = actionCopy(pendingAction.type)
  const isBulk = pendingAction.type.startsWith('bulk')
  const color =
    pendingAction.tone === 'green'
      ? 'bg-emerald-600 hover:bg-emerald-700'
      : pendingAction.tone === 'amber'
        ? 'bg-amber-600 hover:bg-amber-700'
        : 'bg-red-600 hover:bg-red-700'
  const iconTone =
    pendingAction.tone === 'green'
      ? 'bg-emerald-100 text-emerald-600'
      : pendingAction.tone === 'amber'
        ? 'bg-amber-100 text-amber-600'
        : 'bg-red-100 text-red-600'

  return (
    <ModalShell onClose={onClose}>
      <div className="flex items-start gap-4">
        <div className={`flex size-12 shrink-0 items-center justify-center rounded-xl ${iconTone}`}>
          <span className="material-symbols-outlined">{pendingAction.icon}</span>
        </div>
        <div className="min-w-0 flex-1">
          <h3 className="text-lg font-bold text-slate-900">{isBulk ? copy.title.replace('Product', 'Selected Products') : copy.title}</h3>
          <p className="mt-1 text-sm text-slate-500">
            {pendingAction.product ? pendingAction.product.name : 'This action will be applied to the selected products.'}
          </p>
        </div>
      </div>

      {pendingAction.reasonRequired && (
        <textarea
          value={reason}
          onChange={(event) => setReason(event.target.value)}
          className="mt-5 min-h-28 w-full rounded-xl border border-slate-200 bg-slate-50 p-3 text-sm text-slate-900 outline-none transition focus:border-primary focus:bg-white focus:ring-2 focus:ring-primary/20"
          placeholder="Enter reason..."
        />
      )}

      <div className="mt-6 flex justify-end gap-3">
        <button type="button" disabled={busy} onClick={onClose} className="rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-bold text-slate-700 hover:bg-slate-50 disabled:opacity-60">
          Cancel
        </button>
        <button
          type="button"
          disabled={busy || (pendingAction.reasonRequired && !reason.trim())}
          onClick={() => onConfirm(reason.trim())}
          className={`rounded-lg px-4 py-2 text-sm font-bold text-white shadow-sm transition disabled:cursor-not-allowed disabled:opacity-60 ${color}`}
        >
          {busy ? 'Processing...' : copy.action}
        </button>
      </div>
    </ModalShell>
  )
}

export default function AdminProductsPage() {
  const navigate = useNavigate()
  const [products, setProducts] = useState<PagedResult<AdminProductDto> | null>(null)
  const [stats, setStats] = useState<AdminProductStatsDto>(emptyStats)
  const [metadata, setMetadata] = useState<AdminProductMetadataDto>({ brands: [], categories: [], stores: [] })
  const [filters, setFilters] = useState<ProductFilters>(defaultFilters)
  const [loading, setLoading] = useState(true)
  const [metadataLoading, setMetadataLoading] = useState(true)
  const [error, setError] = useState('')
  const [pageNumber, setPageNumber] = useState(1)
  const [selectedIds, setSelectedIds] = useState<Set<string>>(new Set())
  const [showAdvanced, setShowAdvanced] = useState(false)
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null)
  const [actionBusy, setActionBusy] = useState(false)

  const pageCount = products?.totalPages || Math.max(1, Math.ceil((products?.totalCount ?? 0) / (products?.pageSize ?? PAGE_SIZE)))
  const tableRows = products?.items ?? []
  const categoryOptions = useMemo(() => buildCategoryOptions(metadata.categories), [metadata.categories])
  const brandOptions = useMemo(() => metadata.brands.filter((brand: AdminBrandDto) => brand.isApproved), [metadata.brands])

  const selectedProducts = useMemo(() => tableRows.filter((product) => selectedIds.has(product.id)), [selectedIds, tableRows])
  const allRowsSelected = tableRows.length > 0 && tableRows.every((product) => selectedIds.has(product.id))
  const allSelectedPending = selectedProducts.length > 0 && selectedProducts.every((product) => product.status === 'Pending')
  const allSelectedActive = selectedProducts.length > 0 && selectedProducts.every((product) => product.status === 'Active')

  const advancedActiveCount =
    (filters.categoryId !== '' ? 1 : 0) +
    (filters.storeId ? 1 : 0) +
    (filters.minPrice !== '' || filters.maxPrice !== '' ? 1 : 0) +
    (filters.dateShortcut ? 1 : 0) +
    (filters.outOfStock ? 1 : 0)

  const showAdvancedPanel = showAdvanced || advancedActiveCount > 0

  const rangeText = useMemo(() => {
    const total = products?.totalCount ?? 0
    const page = products?.pageNumber ?? pageNumber
    const pageSize = products?.pageSize ?? PAGE_SIZE
    const start = total === 0 ? 0 : (page - 1) * pageSize + 1
    const end = Math.min(page * pageSize, total)
    return { end, start, total }
  }, [products, pageNumber])

  const loadMetadata = async () => {
    setMetadataLoading(true)
    try {
      setMetadata(await adminApi.products.metadata())
    } catch {
      setMetadata({ brands: [], categories: [], stores: [] })
    } finally {
      setMetadataLoading(false)
    }
  }

  const loadProducts = async (nextPage = pageNumber, overrides?: Partial<ProductFilters>) => {
    setLoading(true)
    setError('')

    const effective = { ...filters, ...overrides }
    const range = shortcutRange(effective.dateShortcut, effective.customStart, effective.customEnd)

    try {
      const data = await adminApi.products.list({
        brandId: effective.brandId,
        categoryId: effective.categoryId,
        endDate: range.endDate,
        maxPrice: effective.maxPrice,
        minPrice: effective.minPrice,
        outOfStock: effective.outOfStock || undefined,
        pageNumber: nextPage,
        pageSize: PAGE_SIZE,
        searchTerm: effective.searchTerm.trim() || undefined,
        sortBy: effective.sortBy || undefined,
        sortDirection: effective.sortDirection || undefined,
        startDate: range.startDate,
        status: effective.status || undefined,
        storeId: effective.storeId || undefined,
      })

      setProducts(data.products)
      setStats(data.stats)
      setPageNumber(data.products.pageNumber)
      setSelectedIds(new Set())
    } catch (err) {
      setProducts(null)
      setError(err instanceof Error ? err.message : 'Unable to load products.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadMetadata()
    void loadProducts(1)
  }, [])

  const updateFilters = (patch: Partial<ProductFilters>) => {
    setFilters((current) => ({ ...current, ...patch }))
  }

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadProducts(1)
  }

  const handleStatusChange = (value: string) => {
    updateFilters({ status: value })
    void loadProducts(1, { status: value })
  }

  const handleBrandChange = (value: string) => {
    const nextBrandId = value === '' ? '' : Number(value)
    updateFilters({ brandId: nextBrandId })
    void loadProducts(1, { brandId: nextBrandId })
  }

  const handleReset = () => {
    setFilters(defaultFilters)
    setShowAdvanced(false)
    void loadProducts(1, defaultFilters)
  }

  const handleSort = (column: string) => {
    let nextDirection = ''
    let nextSortBy = column

    if (filters.sortBy !== column) nextDirection = 'desc'
    else if (filters.sortDirection === 'desc') nextDirection = 'asc'
    else if (filters.sortDirection === 'asc') {
      nextSortBy = ''
      nextDirection = ''
    } else nextDirection = 'desc'

    updateFilters({ sortBy: nextSortBy, sortDirection: nextDirection })
    void loadProducts(1, { sortBy: nextSortBy, sortDirection: nextDirection })
  }

  const goToPage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > pageCount || loading) return
    void loadProducts(nextPage)
  }

  const toggleSelectAll = () => {
    if (allRowsSelected) {
      setSelectedIds(new Set())
      return
    }
    setSelectedIds(new Set(tableRows.map((product) => product.id)))
  }

  const toggleProduct = (id: string) => {
    setSelectedIds((current) => {
      const next = new Set(current)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  const openAction = (type: ProductActionType, product?: AdminProductDto) => {
    const reasonRequired = type === 'reject' || type === 'suspend' || type === 'delete' || type === 'bulk-reject' || type === 'bulk-inactive'
    const tone = type === 'approve' || type === 'bulk-approve' ? 'green' : type === 'suspend' || type === 'bulk-inactive' ? 'amber' : 'red'
    const icon = type === 'approve' || type === 'bulk-approve' ? 'check_circle' : type === 'suspend' || type === 'bulk-inactive' ? 'block' : type === 'delete' ? 'delete' : 'cancel'
    setPendingAction({ icon, product, reasonRequired, tone, type })
  }

  const performAction = async (reason: string) => {
    if (!pendingAction) return
    setActionBusy(true)
    try {
      if (pendingAction.type === 'approve' && pendingAction.product) await adminApi.products.approve(pendingAction.product.id)
      if (pendingAction.type === 'reject' && pendingAction.product) await adminApi.products.reject(pendingAction.product.id, { reason })
      if (pendingAction.type === 'suspend' && pendingAction.product) await adminApi.products.suspend(pendingAction.product.id, { reason })
      if (pendingAction.type === 'delete' && pendingAction.product) await adminApi.products.delete(pendingAction.product.id, { reason })

      if (pendingAction.type.startsWith('bulk')) {
        const action = pendingAction.type === 'bulk-approve' ? 'Approve' : pendingAction.type === 'bulk-reject' ? 'Reject' : 'inactive'
        await adminApi.products.bulkUpdateStatus({ action, productIds: [...selectedIds], reason })
      }

      setPendingAction(null)
      await loadProducts(pageNumber)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Action failed.')
    } finally {
      setActionBusy(false)
    }
  }

  return (
    <AdminLayout activePage="Product" breadcrumb={['Dashboard', 'Product Management']} pageHeader="Product Management">
      <div className="flex flex-col gap-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard icon="inventory_2" label="Total Products" tone="border-blue-100/50 bg-blue-50 text-blue-600" value={formatNumber(stats.totalProducts)} />
          <StatCard icon="check_circle" label="Active Products" tone="border-green-100/50 bg-green-50 text-green-600" value={formatNumber(stats.activeProducts)} />
          <StatCard
            clickable
            icon="pending_actions"
            label="Pending Approval"
            onClick={() => {
              updateFilters({ status: 'Pending' })
              void loadProducts(1, { status: 'Pending' })
            }}
            tone="border-amber-100/50 bg-amber-50 text-amber-600"
            value={formatNumber(stats.pendingApproval)}
          />
          <StatCard icon="production_quantity_limits" label="Out of Stock" tone="border-red-100/50 bg-red-50 text-red-600" value={formatNumber(stats.outOfStock)} />
        </div>

        <div className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <form onSubmit={handleSearch} className="flex flex-col">
            <div className="flex flex-col items-start gap-3 p-4 lg:flex-row lg:items-center">
              <div className="relative w-full flex-1">
                <span className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-400">
                  <span className="material-symbols-outlined text-[20px]">search</span>
                </span>
                <input
                  value={filters.searchTerm}
                  onChange={(event) => updateFilters({ searchTerm: event.target.value })}
                  className="w-full rounded-lg border border-slate-200 bg-slate-50 py-2.5 pl-10 pr-4 text-sm text-slate-900 placeholder:text-slate-400 transition-colors focus:border-primary focus:bg-white focus:outline-none focus:ring-2 focus:ring-primary/20"
                  placeholder="Search by name, SKU or store..."
                  type="text"
                  autoComplete="off"
                />
              </div>

              <div className="w-full shrink-0 lg:w-40">
                <select
                  value={filters.status}
                  onChange={(event) => handleStatusChange(event.target.value)}
                  className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-900 shadow-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                >
                  <option value="">All Statuses</option>
                  <option value="Active">Active</option>
                  <option value="Pending">Pending</option>
                  <option value="Inactive">Inactive</option>
                  <option value="Rejected">Rejected</option>
                </select>
              </div>

              <div className="w-full shrink-0 lg:w-44">
                <select
                  value={filters.brandId}
                  onChange={(event) => handleBrandChange(event.target.value)}
                  disabled={metadataLoading}
                  className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-900 shadow-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 disabled:opacity-60"
                >
                  <option value="">All Brands</option>
                  {brandOptions.map((brand) => (
                    <option key={brand.id} value={brand.id}>
                      {brand.name}
                    </option>
                  ))}
                </select>
              </div>

              <div className="flex w-full shrink-0 items-center gap-2 lg:w-auto">
                <button type="submit" className="flex flex-1 items-center justify-center gap-2 rounded-lg bg-primary px-4 py-2.5 text-sm font-medium text-white shadow-sm shadow-blue-500/20 transition-all hover:bg-blue-700 lg:flex-none">
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

                <button type="button" onClick={handleReset} className="flex items-center justify-center rounded-lg border border-slate-200 bg-white p-2.5 text-slate-500 shadow-sm transition-colors hover:bg-slate-50 hover:text-slate-800" title="Reset all filters">
                  <span className="material-symbols-outlined text-[18px]">restart_alt</span>
                </button>

                <button type="button" title="Export is a visual placeholder in the migrated page." className="flex items-center justify-center gap-1.5 whitespace-nowrap rounded-lg border border-slate-200 bg-white px-3.5 py-2.5 text-sm text-slate-600 shadow-sm transition-colors hover:bg-slate-50">
                  <span className="material-symbols-outlined text-[18px]">file_download</span>
                  <span className="hidden font-medium sm:inline">Export</span>
                </button>
              </div>
            </div>

            {showAdvancedPanel && (
              <div className="flex flex-col gap-5 border-t border-slate-100 bg-slate-50/40 px-4 pb-5 pt-4">
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">
                  <div className="space-y-1">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">category</span> Category
                    </label>
                    <select
                      value={filters.categoryId}
                      onChange={(event) => updateFilters({ categoryId: event.target.value === '' ? '' : Number(event.target.value) })}
                      className="h-[40px] w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                    >
                      <option value="">All Categories</option>
                      {categoryOptions.map(({ depth, item }) => (
                        <option key={item.id} value={item.id}>
                          {`${'-- '.repeat(depth)}${item.name}`}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="space-y-1">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">storefront</span> Store
                    </label>
                    <select
                      value={filters.storeId}
                      onChange={(event) => updateFilters({ storeId: event.target.value })}
                      className="h-[40px] w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm shadow-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                    >
                      <option value="">All Stores</option>
                      {metadata.stores.map((store) => (
                        <option key={store.id} value={store.id}>
                          {store.storeName}
                        </option>
                      ))}
                    </select>
                  </div>

                  <div className="space-y-1">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">payments</span> Price Range (VND)
                    </label>
                    <div className="flex items-center gap-1.5">
                      <input value={filters.minPrice} onChange={(event) => updateFilters({ minPrice: event.target.value === '' ? '' : Number(event.target.value) })} type="number" placeholder="Min" className="h-[40px] min-w-0 w-full rounded-lg border border-slate-200 bg-white px-2.5 py-2 text-sm shadow-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                      <span className="shrink-0 text-lg font-light text-slate-300">-</span>
                      <input value={filters.maxPrice} onChange={(event) => updateFilters({ maxPrice: event.target.value === '' ? '' : Number(event.target.value) })} type="number" placeholder="Max" className="h-[40px] min-w-0 w-full rounded-lg border border-slate-200 bg-white px-2.5 py-2 text-sm shadow-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                    </div>
                  </div>

                  <div className="space-y-1">
                    <label className="flex items-center gap-1 text-[11px] font-semibold uppercase tracking-wider text-slate-500">
                      <span className="material-symbols-outlined text-[14px]">calendar_today</span> Created Date
                    </label>
                    <div className="flex flex-col gap-2">
                      <select
                        value={filters.dateShortcut}
                        onChange={(event) => {
                          const value = event.target.value
                          updateFilters({ customEnd: value === 'custom' ? filters.customEnd : '', customStart: value === 'custom' ? filters.customStart : '', dateShortcut: value })
                        }}
                        className="h-[40px] w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                      >
                        <option value="">All Time</option>
                        <option value="today">Today</option>
                        <option value="week">This Week</option>
                        <option value="month">This Month</option>
                        <option value="custom">Custom Range</option>
                      </select>
                      {filters.dateShortcut === 'custom' && (
                        <div className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-2">
                          <span className="material-symbols-outlined text-[18px] text-slate-400">event</span>
                          <input value={filters.customStart} onChange={(event) => updateFilters({ customStart: event.target.value })} type="date" className="min-w-0 flex-1 bg-transparent text-sm outline-none" />
                          <span className="text-slate-300">to</span>
                          <input value={filters.customEnd} onChange={(event) => updateFilters({ customEnd: event.target.value })} type="date" className="min-w-0 flex-1 bg-transparent text-sm outline-none" />
                        </div>
                      )}
                    </div>
                  </div>
                </div>

                <div className="flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-center">
                  <label className="flex cursor-pointer select-none items-center gap-2.5">
                    <span className="relative inline-flex">
                      <input checked={filters.outOfStock} onChange={(event) => updateFilters({ outOfStock: event.target.checked })} type="checkbox" className="peer sr-only" />
                      <span className="h-5 w-9 rounded-full bg-slate-200 shadow-inner transition peer-checked:bg-red-500" />
                      <span className="absolute left-0.5 top-0.5 size-4 rounded-full border border-slate-300 bg-white transition peer-checked:translate-x-full" />
                    </span>
                    <span className="text-sm font-medium text-slate-700">Out of Stock only</span>
                  </label>

                  <button type="submit" className="flex w-full items-center justify-center gap-2 rounded-lg bg-slate-800 px-6 py-2 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-900 sm:w-auto">
                    <span className="material-symbols-outlined text-[17px]">check</span>
                    Apply Filters
                  </button>
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
                  <th className="w-[50px] py-4 pl-6 pr-3 text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <input checked={allRowsSelected} onChange={toggleSelectAll} className="size-4 rounded border-slate-300 text-primary focus:ring-primary" type="checkbox" />
                  </th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Product Info</th>
                  <th className="hidden px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">Category</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Store</th>
                  <th className="px-3 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button type="button" onClick={() => handleSort('price')} className="group inline-flex w-full items-center justify-end gap-1 transition-colors hover:text-primary">
                      Price
                      <SortIcon active={filters.sortBy === 'price'} direction={filters.sortDirection} />
                    </button>
                  </th>
                  <th className="px-3 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button type="button" onClick={() => handleSort('stock')} className="group inline-flex w-full items-center justify-end gap-1 transition-colors hover:text-primary">
                      Stock
                      <SortIcon active={filters.sortBy === 'stock'} direction={filters.sortDirection} />
                    </button>
                  </th>
                  <th className="px-3 py-4 text-center text-xs font-semibold uppercase tracking-wider text-slate-500">Status</th>
                  <th className="px-3 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <button type="button" onClick={() => handleSort('createdAt')} className="group inline-flex w-full items-center justify-end gap-1 transition-colors hover:text-primary">
                      Created
                      <SortIcon active={filters.sortBy === 'createdAt'} direction={filters.sortDirection} />
                    </button>
                  </th>
                  <th className="py-4 pl-3 pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  <LoadingRows />
                ) : tableRows.length ? (
                  tableRows.map((product) => (
                    <tr key={product.id} className="group/row transition-all hover:bg-slate-50">
                      <td className="py-4 pl-6 pr-3 align-middle">
                        <input
                          checked={selectedIds.has(product.id)}
                          onChange={() => toggleProduct(product.id)}
                          className="size-4 rounded border-slate-300 text-primary focus:ring-primary"
                          type="checkbox"
                        />
                      </td>
                      <td className="px-3 py-4 align-middle">
                        <button type="button" onClick={() => navigate(`/admin/products/${product.id}`)} className="flex min-w-[260px] items-center gap-3 text-left">
                          {product.thumbnailUrl ? (
                            <img src={product.thumbnailUrl} alt={product.name} className="size-12 rounded-lg border border-slate-100 bg-slate-100 object-cover shadow-sm" />
                          ) : (
                            <div className="flex size-12 items-center justify-center rounded-lg border border-slate-100 bg-slate-50 shadow-sm">
                              <span className="material-symbols-outlined text-slate-300">image</span>
                            </div>
                          )}
                          <div className="min-w-0">
                            <p className="line-clamp-1 text-sm font-bold text-slate-900 transition-colors group-hover/row:text-primary">{product.name}</p>
                            <p className="mt-1 font-mono text-xs text-slate-400">{product.sku}</p>
                          </div>
                        </button>
                      </td>
                      <td className="hidden whitespace-nowrap px-3 py-4 align-middle text-sm text-slate-600 md:table-cell">{product.category}</td>
                      <td className="max-w-[180px] truncate whitespace-nowrap px-3 py-4 align-middle text-sm font-medium text-slate-700">{product.storeName}</td>
                      <td className="whitespace-nowrap px-3 py-4 text-right align-middle text-sm font-bold text-primary">{formatCurrency(product.price)}</td>
                      <td className="whitespace-nowrap px-3 py-4 text-right align-middle">
                        <span className={`inline-flex rounded-md px-2 py-0.5 text-sm font-bold ${product.stock > 0 ? 'bg-slate-100 text-slate-900' : 'bg-red-50 text-red-600'}`}>
                          {product.stock}
                        </span>
                      </td>
                      <td className="whitespace-nowrap px-3 py-4 text-center align-middle">
                        <StatusBadge status={product.status} />
                      </td>
                      <td className="whitespace-nowrap px-3 py-4 text-right align-middle text-sm text-slate-500">{formatDate(product.createdAt)}</td>
                      <td className="whitespace-nowrap py-4 pl-3 pr-6 text-right align-middle">
                        <div className="flex items-center justify-end gap-1">
                          <Link to={`/admin/products/${product.id}`} className="rounded-lg p-2 text-slate-400 transition-all hover:bg-primary/5 hover:text-primary" title="View Details">
                            <span className="material-symbols-outlined text-[20px]">visibility</span>
                          </Link>
                          {product.status === 'Pending' && (
                            <>
                              <button type="button" onClick={() => openAction('approve', product)} className="rounded-lg p-2 text-slate-400 transition-all hover:bg-emerald-50 hover:text-emerald-600" title="Approve">
                                <span className="material-symbols-outlined text-[20px]">check_circle</span>
                              </button>
                              <button type="button" onClick={() => openAction('reject', product)} className="rounded-lg p-2 text-slate-400 transition-all hover:bg-red-50 hover:text-red-600" title="Reject">
                                <span className="material-symbols-outlined text-[20px]">cancel</span>
                              </button>
                            </>
                          )}
                          {product.status === 'Active' && (
                            <button type="button" onClick={() => openAction('suspend', product)} className="rounded-lg p-2 text-slate-400 transition-all hover:bg-amber-50 hover:text-amber-600" title="Set Inactive">
                              <span className="material-symbols-outlined text-[20px]">block</span>
                            </button>
                          )}
                          <button type="button" onClick={() => openAction('delete', product)} className="rounded-lg p-2 text-slate-400 transition-all hover:bg-red-50 hover:text-red-600" title="Delete">
                            <span className="material-symbols-outlined text-[20px]">delete</span>
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={9} className="py-12 text-center text-slate-500">
                      <div className="flex flex-col items-center justify-center gap-3">
                        <div className="flex size-16 items-center justify-center rounded-full bg-slate-50">
                          <span className="material-symbols-outlined text-4xl text-slate-300">inventory_2</span>
                        </div>
                        <p className="mt-2 text-base font-medium text-slate-900">No products found</p>
                        <p className="text-sm">We could not find any products matching your criteria.</p>
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
              <span className="font-medium text-slate-900">{formatNumber(rangeText.total)}</span> products
            </div>

            {pageCount > 1 && (
              <nav aria-label="Pagination" className="flex items-center gap-1">
                <button type="button" disabled={pageNumber <= 1 || loading} onClick={() => goToPage(pageNumber - 1)} className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:cursor-not-allowed disabled:border-slate-100 disabled:bg-slate-50 disabled:text-slate-300">
                  <span className="material-symbols-outlined text-lg leading-none">chevron_left</span>
                </button>
                <div className="flex items-center gap-1 px-1">
                  {paginationPages(pageNumber, pageCount).map((page) =>
                    typeof page === 'number' ? (
                      <button key={page} type="button" onClick={() => goToPage(page)} className={`flex h-9 min-w-[36px] items-center justify-center rounded-lg text-sm font-medium transition-all ${page === pageNumber ? 'bg-primary text-white shadow-sm shadow-blue-500/20' : 'border border-transparent text-slate-600 hover:border-slate-200 hover:bg-white hover:text-primary'}`}>
                        {page}
                      </button>
                    ) : (
                      <span key={page} className="px-1 text-slate-400">
                        ...
                      </span>
                    ),
                  )}
                </div>
                <button type="button" disabled={pageNumber >= pageCount || loading} onClick={() => goToPage(pageNumber + 1)} className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:cursor-not-allowed disabled:border-slate-100 disabled:bg-slate-50 disabled:text-slate-300">
                  <span className="material-symbols-outlined text-lg leading-none">chevron_right</span>
                </button>
              </nav>
            )}
          </div>
        </div>

        {selectedIds.size > 0 && (
          <div className="fixed bottom-6 left-1/2 z-40 w-[calc(100%-2rem)] max-w-3xl -translate-x-1/2 rounded-2xl border border-slate-200 bg-white px-4 py-3 shadow-2xl">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
              <div className="flex items-center gap-3">
                <div className="flex size-9 items-center justify-center rounded-lg bg-primary text-white">
                  <span className="material-symbols-outlined text-[18px]">done_all</span>
                </div>
                <div>
                  <p className="text-sm font-bold text-slate-900">{selectedIds.size} selected</p>
                  <p className="text-xs text-slate-400">Bulk actions follow the current product status.</p>
                </div>
              </div>
              <div className="flex flex-wrap items-center justify-end gap-2">
                {allSelectedPending && (
                  <>
                    <button type="button" onClick={() => openAction('bulk-approve')} className="flex items-center gap-1.5 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-1.5 text-sm font-medium text-emerald-700 transition-colors hover:bg-emerald-100">
                      <span className="material-symbols-outlined text-[18px]">check_circle</span>
                      Approve
                    </button>
                    <button type="button" onClick={() => openAction('bulk-reject')} className="flex items-center gap-1.5 rounded-lg border border-red-200 bg-red-50 px-3 py-1.5 text-sm font-medium text-red-700 transition-colors hover:bg-red-100">
                      <span className="material-symbols-outlined text-[18px]">cancel</span>
                      Reject
                    </button>
                  </>
                )}
                {allSelectedActive && (
                  <button type="button" onClick={() => openAction('bulk-inactive')} className="flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-sm font-medium text-slate-700 shadow-sm transition-colors hover:bg-slate-50">
                    <span className="material-symbols-outlined text-[18px]">block</span>
                    Set Inactive
                  </button>
                )}
                {!allSelectedPending && !allSelectedActive && <span className="px-3 text-xs font-semibold text-slate-400">Multiple statuses</span>}
                <button type="button" onClick={() => setSelectedIds(new Set())} className="rounded-lg p-2 text-slate-400 transition hover:bg-slate-50 hover:text-slate-700">
                  <span className="material-symbols-outlined text-[18px]">close</span>
                </button>
              </div>
            </div>
          </div>
        )}

        {pendingAction && <ActionModal busy={actionBusy} onClose={() => setPendingAction(null)} onConfirm={(reason) => void performAction(reason)} pendingAction={pendingAction} />}
      </div>
    </AdminLayout>
  )
}
