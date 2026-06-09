/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { CSSProperties, FormEvent, ReactNode } from 'react'
import { Link, useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import {
  adminApi,
  discountType,
  voucherScope,
  voucherStatus,
  voucherType,
  type AdminCategoryDto,
  type AdminVoucherDto,
  type AdminVoucherRequest,
  type AdminVoucherSummaryDto,
  type DiscountType,
  type PagedResult,
  type VoucherScope,
  type VoucherStatus,
  type VoucherType,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const PAGE_SIZE = 10

type VoucherMode = 'list' | 'create' | 'edit'
type StatusFilter = VoucherStatus | ''
type ScopeFilter = VoucherScope | ''
type TypeFilter = VoucherType | ''
type DiscountFilter = DiscountType | ''

interface VoucherFilters {
  categoryId: number | ''
  discountType: DiscountFilter
  endDate: string
  scope: ScopeFilter
  search: string
  sortOption: string
  startDate: string
  status: StatusFilter
  voucherType: TypeFilter
}

interface VoucherFormState {
  categoryId: number | ''
  code: string
  description: string
  discountType: 'Percent' | 'Fixed'
  discountValue: number
  endAt: string
  isVisible: boolean
  maxDiscount: number | ''
  maxUsagePerUser: number
  minOrderAmount: number
  name: string
  startAt: string
  type: 'Order' | 'Shipping'
  usageLimit: number
}

const defaultFilters: VoucherFilters = {
  categoryId: '',
  discountType: '',
  endDate: '',
  scope: '',
  search: '',
  sortOption: '',
  startDate: '',
  status: '',
  voucherType: '',
}

const emptySummary: AdminVoucherSummaryDto = {
  activeToday: 0,
  redemptionRate: 0,
  totalSavedAmount: 0,
  totalVouchers: 0,
}

function toDateTimeLocal(date: Date) {
  const pad = (value: number) => String(value).padStart(2, '0')
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`
}

function defaultForm(): VoucherFormState {
  const now = new Date()
  const end = new Date(now)
  end.setDate(now.getDate() + 30)

  return {
    categoryId: '',
    code: '',
    description: '',
    discountType: 'Percent',
    discountValue: 10,
    endAt: toDateTimeLocal(end),
    isVisible: true,
    maxDiscount: '',
    maxUsagePerUser: 1,
    minOrderAmount: 50000,
    name: '',
    startAt: toDateTimeLocal(now),
    type: 'Order',
    usageLimit: 1000,
  }
}

function formFromVoucher(voucher: AdminVoucherDto): VoucherFormState {
  return {
    categoryId: voucher.categoryId ?? '',
    code: voucher.code,
    description: voucher.description ?? '',
    discountType: voucher.discountType === discountType.percent ? 'Percent' : 'Fixed',
    discountValue: voucher.discountValue,
    endAt: toDateTimeLocal(new Date(voucher.endAt)),
    isVisible: voucher.isActive,
    maxDiscount: voucher.maxDiscount ?? '',
    maxUsagePerUser: voucher.maxUsagePerUser,
    minOrderAmount: voucher.minOrderAmount ?? 0,
    name: voucher.name,
    startAt: toDateTimeLocal(new Date(voucher.startAt)),
    type: voucher.type === voucherType.shippingDiscount ? 'Shipping' : 'Order',
    usageLimit: voucher.usageLimit,
  }
}

function copyFormFromVoucher(voucher: AdminVoucherDto): VoucherFormState {
  const form = formFromVoucher(voucher)
  const defaults = defaultForm()
  return {
    ...form,
    code: '',
    endAt: defaults.endAt,
    isVisible: true,
    name: `${voucher.name} (Copy)`,
    startAt: defaults.startAt,
  }
}

function toRequest(form: VoucherFormState): AdminVoucherRequest {
  return {
    categoryId: form.type === 'Shipping' || form.categoryId === '' ? null : form.categoryId,
    code: form.code.trim().toUpperCase(),
    description: form.description.trim() || null,
    discountType: form.discountType,
    discountValue: Number(form.discountValue),
    endAt: form.endAt,
    isVisible: form.isVisible,
    maxDiscount: form.discountType === 'Fixed' || form.maxDiscount === '' ? null : Number(form.maxDiscount),
    maxUsagePerUser: Number(form.maxUsagePerUser),
    minOrderAmount: Number(form.minOrderAmount),
    name: form.name.trim(),
    startAt: form.startAt,
    type: form.type,
    usageLimit: Number(form.usageLimit),
  }
}

function parseSort(sortOption: string) {
  if (!sortOption) return { sortBy: undefined, sortDirection: undefined }
  const [sortBy, sortDirection] = sortOption.split('-')
  return { sortBy, sortDirection }
}

function formatVnd(value?: number | null) {
  return `${Math.round(value ?? 0).toLocaleString('vi-VN')} VND`
}

function formatNumber(value: number) {
  return Math.round(value ?? 0).toLocaleString('vi-VN')
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat('en-US', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value))
}

function shortDate(value: string) {
  return new Intl.DateTimeFormat('en-US', { day: '2-digit', month: 'short' }).format(new Date(value))
}

function statusLabel(status: VoucherStatus) {
  if (status === voucherStatus.upcoming) return 'Upcoming'
  if (status === voucherStatus.active) return 'Active'
  if (status === voucherStatus.expired) return 'Expired'
  if (status === voucherStatus.disabled) return 'Disabled'
  return 'Finished'
}

function statusTone(status: VoucherStatus) {
  if (status === voucherStatus.active) return { accent: 'bg-primary', badge: 'bg-emerald-100 text-emerald-700', bar: 'bg-primary', icon: 'text-primary' }
  if (status === voucherStatus.upcoming) return { accent: 'bg-amber-500', badge: 'bg-amber-100 text-amber-700', bar: 'bg-amber-400', icon: 'text-slate-400' }
  if (status === voucherStatus.disabled) return { accent: 'bg-red-500', badge: 'bg-red-100 text-red-700', bar: 'bg-red-400', icon: 'text-slate-400' }
  if (status === voucherStatus.finished) return { accent: 'bg-blue-700', badge: 'bg-blue-100 text-blue-700', bar: 'bg-blue-500', icon: 'text-slate-400' }
  return { accent: 'bg-slate-400', badge: 'bg-slate-100 text-slate-500', bar: 'bg-slate-300', icon: 'text-slate-400' }
}

function activeAdvancedCount(filters: VoucherFilters) {
  return (filters.categoryId !== '' ? 1 : 0) + (filters.discountType !== '' ? 1 : 0) + (filters.startDate || filters.endDate ? 1 : 0)
}

function paginationPages(current: number, total: number) {
  const pages: Array<number | 'ellipsis-start' | 'ellipsis-end'> = []
  if (total <= 7) return Array.from({ length: total }, (_, index) => index + 1)

  for (let page = 1; page <= total; page += 1) {
    if (page === 1 || page === total || (page >= current - 1 && page <= current + 1)) pages.push(page)
    else if (page === 2) pages.push('ellipsis-start')
    else if (page === total - 1) pages.push('ellipsis-end')
  }
  return pages
}

function categoryIcon(categoryName: string, type: 'Order' | 'Shipping', hasCategory: boolean) {
  if (type === 'Shipping') return 'local_shipping'
  const name = categoryName.toLowerCase()
  if (name.includes('keyboard')) return 'keyboard'
  if (name.includes('mouse') || name.includes('mice')) return 'mouse'
  if (name.includes('headset') || name.includes('headphone')) return 'headset'
  if (name.includes('monitor')) return 'monitor'
  if (name.includes('pc-components') || name.includes('cpu') || name.includes('gpu')) return 'memory'
  if (name.includes('furniture')) return 'chair'
  if (name.includes('console') || name.includes('controller')) return 'videogame_asset'
  return hasCategory ? 'category' : 'confirmation_number'
}

function serratedStyle(): CSSProperties {
  return {
    WebkitMaskImage: 'radial-gradient(circle at 2px 7px, transparent 4px, black 5px)',
    WebkitMaskSize: '100% 14px',
    maskImage: 'radial-gradient(circle at 2px 7px, transparent 4px, black 5px)',
    maskSize: '100% 14px',
  }
}

function ticketDashStyle(): CSSProperties {
  return {
    backgroundImage: 'linear-gradient(to bottom, #E2E8F0 60%, rgba(255,255,255,0) 0%)',
    backgroundPosition: 'center',
    backgroundRepeat: 'repeat-y',
    backgroundSize: '1.5px 10px',
  }
}

function KpiCard({
  dark,
  helper,
  icon,
  label,
  tone,
  value,
}: {
  dark?: boolean
  helper: string
  icon: string
  label: string
  tone: string
  value: string | number
}) {
  return (
    <div
      className={`relative flex min-w-[220px] flex-1 items-start gap-4 overflow-hidden rounded-[10px] border p-4 shadow-sm lg:p-5 ${
        dark ? 'border-slate-800 bg-slate-900' : 'border-slate-100 bg-white'
      }`}
    >
      {dark && (
        <div className="absolute right-0 top-0 p-4 opacity-10 transition-transform group-hover:scale-125">
          <span className="material-symbols-outlined text-[64px] text-white">savings</span>
        </div>
      )}
      <div className={`z-10 mt-0.5 flex size-9 shrink-0 items-center justify-center rounded-full ${tone}`}>
        <span className="material-symbols-outlined text-[20px]">{icon}</span>
      </div>
      <div className="z-10 min-w-0">
        <p className={`text-[10px] font-bold uppercase leading-none tracking-widest ${dark ? 'text-slate-400' : 'text-slate-400'}`}>{label}</p>
        <h3 className={`mt-1.5 text-xl font-bold leading-tight ${dark ? 'text-white' : 'text-slate-900'}`}>{value}</h3>
        <div className={`mt-2 flex items-center gap-1 text-[10px] font-bold ${dark ? 'text-blue-400' : 'text-green-600'}`}>
          <span className="material-symbols-outlined text-[12px]">{dark ? 'verified_user' : 'trending_up'}</span>
          <span>{helper}</span>
        </div>
      </div>
    </div>
  )
}

function VoucherTicket({
  onDuplicate,
  onToggle,
  voucher,
}: {
  onDuplicate: (voucher: AdminVoucherDto) => void
  onToggle: (voucher: AdminVoucherDto) => void
  voucher: AdminVoucherDto
}) {
  const tone = statusTone(voucher.status)
  const strongText =
    voucher.status === voucherStatus.active ||
    voucher.status === voucherStatus.upcoming ||
    voucher.status === voucherStatus.disabled ||
    voucher.status === voucherStatus.finished
  const usagePercent = voucher.usageLimit > 0 ? Math.min(100, (voucher.usedCount / voucher.usageLimit) * 100) : 0
  const discount = voucher.discountType === discountType.percent ? `${voucher.discountValue.toFixed(0)}% OFF` : `${formatNumber(voucher.discountValue)} VND OFF`
  const target = voucher.categoryId ? voucher.categoryName || 'Category' : 'Global'

  return (
    <div className="flex h-36 overflow-hidden rounded-xl border-y border-r border-slate-100 bg-white shadow-sm transition-all hover:translate-x-1 hover:shadow-lg">
      <div className={`flex w-36 shrink-0 flex-col items-center justify-center rounded-l-xl p-4 ${tone.accent}`} style={serratedStyle()}>
        <div className="mb-2 flex size-12 items-center justify-center rounded-full bg-white shadow-sm">
          <span className={`material-symbols-outlined text-[28px] ${tone.icon}`}>{voucher.categoryIcon || 'confirmation_number'}</span>
        </div>
        <div className="text-center">
          <p className={`text-xl font-bold leading-none ${strongText ? 'text-white' : 'text-slate-900'}`}>{discount}</p>
          <p className={`mt-1 text-[10px] font-black uppercase tracking-widest ${strongText ? 'text-white/70' : 'text-slate-400'}`}>{voucher.code}</p>
        </div>
      </div>

      <div className="h-full w-px shrink-0" style={ticketDashStyle()} />

      <div className="flex min-w-0 flex-1 flex-col justify-between p-5">
        <div className="flex items-start justify-between gap-4">
          <div className="min-w-0">
            <div className="mb-1.5 flex items-center gap-3">
              <h3 className="truncate text-base font-bold leading-none text-slate-800">{voucher.name}</h3>
              <span className={`shrink-0 rounded-full px-2.5 py-1 text-[10px] font-bold uppercase tracking-tight ${tone.badge}`}>{statusLabel(voucher.status)}</span>
            </div>
            <div className="flex flex-wrap gap-x-6 gap-y-1 text-xs text-slate-500">
              <div>
                <span className="text-slate-400">Min. Spend: </span>
                <span className="font-bold text-slate-700">{formatVnd(voucher.minOrderAmount)}</span>
              </div>
              <div>
                <span className="text-slate-400">Max Disc: </span>
                <span className="font-bold text-slate-700">{voucher.maxDiscount && voucher.maxDiscount > 0 ? formatVnd(voucher.maxDiscount) : 'No Limit'}</span>
              </div>
              <div>
                <span className="text-slate-400">Target: </span>
                <span className="font-bold text-slate-700">{target}</span>
              </div>
            </div>
          </div>
          <div className="flex items-center gap-1">
            <Link to={`/admin/vouchers/edit/${voucher.id}`} className="rounded-lg p-2 text-slate-400 transition-all hover:bg-blue-50 hover:text-primary" title="Edit">
              <span className="material-symbols-outlined text-xl">edit</span>
            </Link>
            <button type="button" onClick={() => onDuplicate(voucher)} className="rounded-lg p-2 text-slate-400 transition-all hover:bg-slate-100 hover:text-slate-600" title="Duplicate">
              <span className="material-symbols-outlined text-xl">content_copy</span>
            </button>
            <button
              type="button"
              onClick={() => onToggle(voucher)}
              className={`rounded-lg p-2 transition-all ${
                voucher.status === voucherStatus.disabled ? 'text-emerald-500 hover:bg-emerald-50' : 'text-red-400 hover:bg-red-50 hover:text-red-500'
              }`}
              title={voucher.status === voucherStatus.disabled ? 'Enable Voucher' : 'Disable Voucher'}
            >
              <span className="material-symbols-outlined text-xl">{voucher.status === voucherStatus.disabled ? 'play_arrow' : 'block'}</span>
            </button>
          </div>
        </div>

        <div className="mt-auto flex items-end justify-between gap-8">
          <div className="max-w-md flex-1">
            <div className="mb-1.5 flex justify-between text-[11px]">
              <span className="font-medium text-slate-500">
                Usage: <span className={`font-bold ${usagePercent > 90 ? 'text-red-500' : 'text-slate-700'}`}>{voucher.usedCount}/{voucher.usageLimit}</span>
              </span>
              <span className="font-bold text-slate-900">{usagePercent.toFixed(0)}%</span>
            </div>
            <div className="h-2 w-full overflow-hidden rounded-full bg-slate-100 shadow-inner">
              <div className={`h-full rounded-full shadow-sm transition-all duration-1000 ${tone.bar}`} style={{ width: `${usagePercent}%` }} />
            </div>
          </div>
          <div className="mb-0.5 flex shrink-0 items-center gap-1.5 text-xs text-slate-400">
            <span className="material-symbols-outlined text-sm">schedule</span>
            <span>{shortDate(voucher.startAt)} - {formatDate(voucher.endAt)}</span>
          </div>
        </div>
      </div>
    </div>
  )
}

function VoucherPreview({ categories, form }: { categories: AdminCategoryDto[]; form: VoucherFormState }) {
  const selectedCategory = categories.find((category) => category.id === form.categoryId)
  const hasCategory = form.categoryId !== ''
  const icon = categoryIcon(selectedCategory?.name ?? '', form.type, hasCategory)
  const value = form.discountType === 'Percent' ? `${Number(form.discountValue || 0)}% OFF` : `${formatNumber(Number(form.discountValue || 0))} VND OFF`
  const usagePercent = ((form.usageLimit - 10) / (5000 - 10)) * 100

  return (
    <div className="relative overflow-hidden rounded-xl bg-slate-900 p-6 shadow-sm">
      <div className="absolute -right-12 -top-12 size-40 rounded-full bg-white/5 blur-3xl" />
      <h3 className="relative z-10 mb-4 text-[10px] font-bold uppercase tracking-[0.2em] text-slate-400">Real-time Preview</h3>
      <div className="relative z-10 flex h-36 overflow-hidden rounded-xl border-y border-r border-slate-50 bg-white shadow-xl transition-transform hover:scale-[1.02]">
        <div className="flex w-32 shrink-0 flex-col items-center justify-center rounded-l-xl bg-primary p-4" style={serratedStyle()}>
          <div className="mb-2 flex size-10 items-center justify-center rounded-full bg-white shadow-sm">
            <span className="material-symbols-outlined text-[24px] text-primary">{icon}</span>
          </div>
          <div className="text-center">
            <p className="text-lg font-bold leading-none text-white">{value}</p>
            <p className="mt-1 text-[9px] font-black uppercase tracking-widest text-white/70">{form.code || 'VCODE'}</p>
          </div>
        </div>
        <div className="h-full w-px shrink-0" style={ticketDashStyle()} />
        <div className="flex min-w-0 flex-1 flex-col justify-between p-4">
          <div>
            <div className="mb-1 flex items-center gap-2">
              <h3 className="truncate text-sm font-bold text-slate-800">{form.name || 'My Campaign'}</h3>
              <span className="shrink-0 rounded-full bg-emerald-100 px-1.5 py-0.5 text-[8px] font-bold uppercase text-emerald-700">Active</span>
            </div>
            <div className="space-y-1 text-[10px] text-slate-500">
              <div className="flex justify-between">
                <span>Min. Spend:</span>
                <span className="font-bold text-slate-700">{formatVnd(form.minOrderAmount)}</span>
              </div>
              {form.type === 'Order' && (
                <div className="flex justify-between">
                  <span>Category:</span>
                  <span className="font-bold text-slate-700">{selectedCategory?.name ?? 'All'}</span>
                </div>
              )}
            </div>
          </div>
          <div className="mt-auto">
            <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-100">
              <div className="h-full rounded-full bg-primary shadow-sm" style={{ width: `${Math.max(2, Math.min(100, usagePercent))}%` }} />
            </div>
            <div className="mt-1 flex justify-between text-[8px] font-bold uppercase tracking-tighter text-slate-400">
              <span>0 Usage / {form.usageLimit}</span>
              <span>Valid 30 Days</span>
            </div>
          </div>
        </div>
      </div>
      <p className="mt-4 text-center text-[10px] italic text-slate-500">This is how customers will see your voucher.</p>
    </div>
  )
}

function VoucherFormPage({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { id } = useParams()
  const [categories, setCategories] = useState<AdminCategoryDto[]>([])
  const [form, setForm] = useState<VoucherFormState>(defaultForm)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  const discount = Number(form.discountValue || 0)
  const samplePurchase = Math.max(Number(form.minOrderAmount || 0), form.discountType === 'Percent' ? 100000 : discount + 10000)
  const sampleDiscount = form.discountType === 'Percent' ? (samplePurchase * discount) / 100 : discount
  const sampleFinal = Math.max(0, samplePurchase - sampleDiscount)
  const rangePercent = ((form.usageLimit - 10) / (5000 - 10)) * 100

  useEffect(() => {
    const load = async () => {
      setLoading(true)
      setError('')
      try {
        const metadataPromise = adminApi.vouchers.list({ pageNumber: 1, pageSize: 1 })
        const copyFromId = searchParams.get('copyFromId')

        if (mode === 'edit' && id) {
          const [metadata, voucher] = await Promise.all([metadataPromise, adminApi.vouchers.get(id)])
          setCategories(metadata.categories)
          setForm(formFromVoucher(voucher))
        } else if (copyFromId) {
          const [metadata, voucher] = await Promise.all([metadataPromise, adminApi.vouchers.get(copyFromId)])
          setCategories(metadata.categories)
          setForm(copyFormFromVoucher(voucher))
        } else {
          const metadata = await metadataPromise
          setCategories(metadata.categories)
          setForm(defaultForm())
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unable to load voucher data.')
      } finally {
        setLoading(false)
      }
    }

    void load()
  }, [mode, id, searchParams])

  const patchForm = (patch: Partial<VoucherFormState>) => {
    setForm((current) => ({ ...current, ...patch }))
  }

  const generateCode = () => {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'
    let result = ''
    for (let index = 0; index < 8; index += 1) result += chars.charAt(Math.floor(Math.random() * chars.length))
    patchForm({ code: result })
  }

  const validate = () => {
    if (form.discountType === 'Percent' && Number(form.discountValue) > 100) return 'Discount percentage must be less than or equal to 100%.'
    if (form.discountType === 'Fixed' && Number(form.minOrderAmount) <= Number(form.discountValue)) return 'Minimum spend must be greater than discount amount.'
    if (new Date(form.endAt) <= new Date(form.startAt)) return 'Expiration date must be later than launch date.'
    return ''
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const validation = validate()
    if (validation) {
      setError(validation)
      return
    }

    setSaving(true)
    setError('')

    try {
      if (mode === 'edit' && id) await adminApi.vouchers.update(id, toRequest(form))
      else await adminApi.vouchers.create(toRequest(form))
      navigate('/admin/vouchers', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to save voucher.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="flex min-h-[420px] items-center justify-center rounded-xl border border-slate-100 bg-white text-slate-500 shadow-sm">
        <span className="material-symbols-outlined mr-2 animate-spin text-[20px]">progress_activity</span>
        Loading voucher...
      </div>
    )
  }

  return (
    <form onSubmit={handleSubmit}>
      <div className="grid grid-cols-1 gap-8 lg:grid-cols-12">
        <div className="space-y-6 lg:col-span-8">
          {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}

          <section className="rounded-xl border border-slate-100 bg-white p-6 shadow-sm">
            <div className="mb-6 flex items-center gap-3">
              <div className="h-6 w-1.5 rounded-full bg-primary" />
              <h2 className="text-lg font-bold text-slate-800">Basic Information</h2>
            </div>

            <div className="grid grid-cols-1 gap-5 md:grid-cols-2">
              <label className="block md:col-span-2">
                <span className="mb-1.5 block text-sm font-bold text-slate-700">Voucher Name</span>
                <input value={form.name} onChange={(event) => patchForm({ name: event.target.value })} required className="h-[42px] w-full rounded-lg border border-slate-200 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" placeholder="e.g. Summer Mega Sale 2024" />
              </label>

              <label className="block">
                <span className="mb-1.5 block text-sm font-bold text-slate-700">Voucher Code</span>
                <div className="relative">
                  <input value={form.code} onChange={(event) => patchForm({ code: event.target.value.toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 12) })} required className="h-[42px] w-full rounded-lg border border-slate-200 px-3 pr-20 font-mono text-sm uppercase outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" placeholder="SUMMER50" />
                  <button type="button" onClick={generateCode} className="absolute right-2 top-1.5 rounded border border-primary/20 px-2 py-1 text-[10px] font-bold text-primary transition-all hover:bg-blue-50">
                    AUTO
                  </button>
                </div>
              </label>

              <div className="md:col-span-2">
                <span className="mb-3 block text-sm font-bold text-slate-700">Voucher Type</span>
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  {[
                    { icon: 'confirmation_number', label: 'Order Discount', sub: 'Fixed or percentage discount on order total', value: 'Order' as const },
                    { icon: 'local_shipping', label: 'Shipping Discount', sub: 'Discount on shipping fees', value: 'Shipping' as const },
                  ].map((item) => (
                    <button
                      key={item.value}
                      type="button"
                      onClick={() => patchForm({ type: item.value, categoryId: item.value === 'Shipping' ? '' : form.categoryId })}
                      className={`flex items-center gap-3 rounded-xl border-2 p-3 text-left transition-all hover:bg-slate-50 ${
                        form.type === item.value ? 'border-primary bg-blue-50' : 'border-slate-100'
                      }`}
                    >
                      <span className="flex size-10 items-center justify-center rounded-lg bg-white text-slate-400">
                        <span className="material-symbols-outlined text-xl">{item.icon}</span>
                      </span>
                      <span>
                        <span className="block text-xs font-bold text-slate-700">{item.label}</span>
                        <span className="text-[10px] text-slate-400">{item.sub}</span>
                      </span>
                    </button>
                  ))}
                </div>
              </div>

              {form.type === 'Order' && (
                <label className="block">
                  <span className="mb-1.5 block text-sm font-bold text-slate-700">Category Restriction</span>
                  <select value={form.categoryId} onChange={(event) => patchForm({ categoryId: event.target.value === '' ? '' : Number(event.target.value) })} className="h-[42px] w-full rounded-lg border border-slate-200 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20">
                    <option value="">All Categories</option>
                    {categories.map((category) => (
                      <option key={category.id} value={category.id}>
                        {category.name}
                      </option>
                    ))}
                  </select>
                </label>
              )}

              <label className="block md:col-span-2">
                <span className="mb-1.5 block text-sm font-bold text-slate-700">Description (Optional)</span>
                <textarea value={form.description} onChange={(event) => patchForm({ description: event.target.value })} rows={3} className="w-full rounded-lg border border-slate-200 p-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" placeholder="Campaign details, terms and conditions..." />
              </label>
            </div>
          </section>

          <section className="rounded-xl border border-slate-100 bg-white p-6 shadow-sm">
            <div className="mb-6 flex items-center gap-3">
              <div className="h-6 w-1.5 rounded-full bg-primary" />
              <h2 className="text-lg font-bold text-slate-800">Configuration & Discount</h2>
            </div>

            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <div className="space-y-3">
                <span className="block text-sm font-bold text-slate-700">Discount Logic</span>
                <div className="flex h-[42px] rounded-xl bg-slate-100 p-1">
                  {(['Percent', 'Fixed'] as const).map((type) => (
                    <button
                      key={type}
                      type="button"
                      onClick={() => patchForm({ discountType: type, discountValue: type === 'Percent' ? 10 : 10000, maxDiscount: type === 'Fixed' ? '' : form.maxDiscount })}
                      className={`flex-1 rounded-lg text-xs font-bold transition-all ${form.discountType === type ? 'bg-white text-primary shadow-sm' : 'text-slate-500 hover:text-slate-700'}`}
                    >
                      {type === 'Percent' ? 'Percentage (%)' : 'Fixed Amount (VND)'}
                    </button>
                  ))}
                </div>
              </div>

              <div />

              <div className="space-y-4">
                <label className="block">
                  <span className="mb-1.5 block text-sm font-bold text-slate-700">{form.discountType === 'Percent' ? 'Discount Percentage (%)' : 'Discount Amount (VND)'}</span>
                  <div className="relative">
                    <input value={form.discountValue} onChange={(event) => patchForm({ discountValue: Number(event.target.value) })} required type="number" min="0.01" className="h-[42px] w-full rounded-lg border border-slate-200 px-3 pr-12 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                    <span className="absolute right-4 top-2.5 text-sm font-bold text-slate-400">{form.discountType === 'Percent' ? '%' : 'VND'}</span>
                  </div>
                </label>
                {form.discountType === 'Percent' && (
                  <label className="block">
                    <span className="mb-1.5 block text-sm font-bold text-slate-700">Maximum Discount Cap (VND)</span>
                    <input value={form.maxDiscount} onChange={(event) => patchForm({ maxDiscount: event.target.value === '' ? '' : Number(event.target.value) })} type="number" className="h-[42px] w-full rounded-lg border border-slate-200 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" placeholder="Optional" />
                  </label>
                )}
              </div>

              <div className="space-y-4">
                <label className="block">
                  <span className="mb-1.5 block text-sm font-bold text-slate-700">Minimum Spend (VND)</span>
                  <input value={form.minOrderAmount} onChange={(event) => patchForm({ minOrderAmount: Number(event.target.value) })} required min="0" type="number" className="h-[42px] w-full rounded-lg border border-slate-200 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                </label>
                <div className="rounded-xl border border-emerald-100 bg-emerald-50 p-4">
                  <p className="mb-2 text-[10px] font-bold uppercase tracking-widest text-emerald-600">Example Calculation</p>
                  <div className="space-y-1">
                    <div className="flex justify-between text-xs text-emerald-800">
                      <span>Sample Purchase:</span>
                      <span className="font-bold">{formatVnd(samplePurchase)}</span>
                    </div>
                    <div className="flex justify-between text-xs text-emerald-800">
                      <span>Discount Apply:</span>
                      <span className="font-bold">-{form.discountType === 'Percent' ? `${discount}%` : formatVnd(discount)}</span>
                    </div>
                    <div className="my-2 h-px bg-emerald-200/50" />
                    <div className="flex justify-between text-sm font-black text-emerald-900">
                      <span>Customer Pays:</span>
                      <span>{formatVnd(sampleFinal)}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <section className="rounded-xl border border-slate-100 bg-white p-6 shadow-sm">
            <div className="mb-6 flex items-center gap-3">
              <div className="h-6 w-1.5 rounded-full bg-primary" />
              <h2 className="text-lg font-bold text-slate-800">Usage & Lifecycle</h2>
            </div>

            <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
              <div className="space-y-5">
                <div className="mb-1 grid grid-cols-2 gap-4 text-xs font-bold uppercase tracking-widest text-slate-400">
                  <span>Total Limit</span>
                  <span className="text-right text-slate-900">{form.usageLimit} claims</span>
                </div>
                <input
                  value={form.usageLimit}
                  onChange={(event) => patchForm({ usageLimit: Number(event.target.value) })}
                  type="range"
                  min="10"
                  max="5000"
                  step="10"
                  className="h-2 w-full cursor-pointer appearance-none rounded-lg bg-slate-100 accent-primary"
                  style={{ backgroundImage: 'linear-gradient(#1A56DB, #1A56DB)', backgroundRepeat: 'no-repeat', backgroundSize: `${rangePercent}% 100%` }}
                />
                <div className="space-y-4 rounded-xl border border-slate-200 bg-slate-50 p-4">
                  <label className="block">
                    <span className="mb-1.5 block text-xs font-bold uppercase tracking-wider text-slate-700">Max Usage Per User</span>
                    <input value={form.maxUsagePerUser} onChange={(event) => patchForm({ maxUsagePerUser: Number(event.target.value) })} required min="1" max="1000" type="number" className="h-[38px] w-full rounded-lg border border-slate-200 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                  </label>
                  <p className="text-[11px] italic leading-relaxed text-slate-500">* The voucher will automatically expire when the usage limit is reached or the campaign end date passes.</p>
                </div>
              </div>

              <div className="space-y-4">
                <label className="block">
                  <span className="mb-1.5 block text-sm font-bold text-slate-700">Launch Date</span>
                  <input value={form.startAt} onChange={(event) => patchForm({ startAt: event.target.value })} required type="datetime-local" className="h-[42px] w-full rounded-lg border border-slate-200 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                </label>
                <label className="block">
                  <span className="mb-1.5 block text-sm font-bold text-slate-700">Expiration Date</span>
                  <input value={form.endAt} onChange={(event) => patchForm({ endAt: event.target.value })} required type="datetime-local" className="h-[42px] w-full rounded-lg border border-slate-200 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                </label>
              </div>
            </div>
          </section>
        </div>

        <div className="space-y-6 lg:col-span-4">
          <div className="sticky top-6 space-y-6">
            <VoucherPreview categories={categories} form={form} />

            <div className="rounded-xl border border-slate-100 bg-white p-5 shadow-sm">
              <h3 className="mb-4 text-sm font-bold text-slate-800">Publish Information</h3>
              <div className="space-y-4">
                <div className="flex items-center justify-between rounded-lg bg-slate-50 p-3">
                  <div>
                    <p className="text-xs font-bold text-slate-700">Scope</p>
                    <p className="text-[10px] text-slate-500">Global Platform Voucher</p>
                  </div>
                  <span className="material-symbols-outlined text-primary">public</span>
                </div>
                <label className="flex items-center justify-between px-1 py-2">
                  <span>
                    <span className="block text-xs font-bold text-slate-700">Display in Voucher Center</span>
                    <span className="text-[10px] text-slate-500">Visible to all users</span>
                  </span>
                  <span className="relative inline-flex cursor-pointer">
                    <input checked={form.isVisible} onChange={(event) => patchForm({ isVisible: event.target.checked })} type="checkbox" className="peer sr-only" />
                    <span className="h-5 w-10 rounded-full bg-slate-200 transition peer-checked:bg-primary" />
                    <span className="absolute left-0.5 top-0.5 size-4 rounded-full border border-slate-300 bg-white transition peer-checked:translate-x-full peer-checked:border-white" />
                  </span>
                </label>
              </div>

              <div className="mt-6 space-y-3">
                <button type="submit" disabled={saving} className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary py-3 text-sm font-bold text-white shadow-lg shadow-primary/20 transition-all hover:bg-blue-700 disabled:opacity-60">
                  <span className="material-symbols-outlined text-[20px]">{mode === 'edit' ? 'save' : 'send'}</span>
                  {saving ? 'Saving...' : mode === 'edit' ? 'Update Voucher' : 'Launch Voucher'}
                </button>
                <Link to="/admin/vouchers" className="flex w-full items-center justify-center rounded-xl border border-slate-200 bg-white py-3 text-sm font-bold text-slate-600 transition-all hover:bg-slate-50">
                  {mode === 'edit' ? 'Cancel' : 'Save as Draft'}
                </Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </form>
  )
}

function ConfirmModal({
  busy,
  children,
  confirmLabel,
  icon,
  onClose,
  onConfirm,
  title,
}: {
  busy: boolean
  children: ReactNode
  confirmLabel: string
  icon: string
  onClose: () => void
  onConfirm: () => void
  title: string
}) {
  return (
    <div className="fixed inset-0 z-[60]">
      <button type="button" aria-label="Close modal" className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" onClick={onClose} />
      <div className="absolute inset-0 flex items-center justify-center p-4">
        <div className="relative w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl">
          <div className="flex gap-4">
            <div className="mt-1 flex size-10 shrink-0 items-center justify-center rounded-full bg-amber-100 text-amber-600">
              <span className="material-symbols-outlined text-2xl">{icon}</span>
            </div>
            <div>
              <h3 className="text-xl font-bold text-slate-900">{title}</h3>
              <div className="mt-1 text-sm text-slate-500">{children}</div>
            </div>
          </div>
          <div className="mt-6 flex justify-end gap-3 border-t border-slate-100 pt-5">
            <button type="button" disabled={busy} onClick={onClose} className="rounded-xl border border-slate-200 px-5 py-2 text-sm font-bold text-slate-600 hover:bg-slate-50 disabled:opacity-60">
              Cancel
            </button>
            <button type="button" disabled={busy} onClick={onConfirm} className="rounded-xl bg-primary px-6 py-2 text-sm font-bold text-white hover:bg-blue-700 disabled:opacity-60">
              {busy ? 'Processing...' : confirmLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

function VoucherListPage() {
  const navigate = useNavigate()
  const [vouchers, setVouchers] = useState<PagedResult<AdminVoucherDto> | null>(null)
  const [summary, setSummary] = useState<AdminVoucherSummaryDto>(emptySummary)
  const [categories, setCategories] = useState<AdminCategoryDto[]>([])
  const [filters, setFilters] = useState<VoucherFilters>(defaultFilters)
  const [pageNumber, setPageNumber] = useState(1)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [showAdvanced, setShowAdvanced] = useState(false)
  const [pendingToggle, setPendingToggle] = useState<AdminVoucherDto | null>(null)
  const [busy, setBusy] = useState(false)

  const rows = vouchers?.items ?? []
  const totalPages = vouchers?.totalPages || Math.max(1, Math.ceil((vouchers?.totalCount ?? 0) / (vouchers?.pageSize ?? PAGE_SIZE)))
  const activeCount = activeAdvancedCount(filters)
  const showAdvancedPanel = showAdvanced || activeCount > 0
  const pages = paginationPages(pageNumber, totalPages)

  const currentRange = useMemo(() => {
    const total = vouchers?.totalCount ?? 0
    const page = vouchers?.pageNumber ?? pageNumber
    const pageSize = vouchers?.pageSize ?? PAGE_SIZE
    const start = total === 0 ? 0 : (page - 1) * pageSize + 1
    const end = Math.min(page * pageSize, total)
    return { end, start, total }
  }, [vouchers, pageNumber])

  const loadVouchers = async (nextPage = pageNumber, overrides?: Partial<VoucherFilters>) => {
    setLoading(true)
    setError('')

    const effective = { ...filters, ...overrides }
    const sort = parseSort(effective.sortOption)

    try {
      const data = await adminApi.vouchers.list({
        categoryId: effective.categoryId,
        discountType: effective.discountType,
        endDate: effective.endDate || undefined,
        pageNumber: nextPage,
        pageSize: PAGE_SIZE,
        scope: effective.scope,
        search: effective.search.trim() || undefined,
        sortBy: sort.sortBy,
        sortDirection: sort.sortDirection,
        startDate: effective.startDate || undefined,
        status: effective.status,
        voucherType: effective.voucherType,
      })
      setVouchers(data.vouchers)
      setSummary(data.summary)
      setCategories(data.categories)
      setPageNumber(data.vouchers.pageNumber)
    } catch (err) {
      setVouchers(null)
      setError(err instanceof Error ? err.message : 'Unable to load vouchers.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadVouchers(1)
  }, [])

  const updateFilters = (patch: Partial<VoucherFilters>) => setFilters((current) => ({ ...current, ...patch }))

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadVouchers(1)
  }

  const filterByStatus = (status: StatusFilter) => {
    updateFilters({ status })
    void loadVouchers(1, { status })
  }

  const resetFilters = () => {
    setFilters(defaultFilters)
    setShowAdvanced(false)
    void loadVouchers(1, defaultFilters)
  }

  const goToPage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > totalPages || loading) return
    void loadVouchers(nextPage)
  }

  const duplicateVoucher = (voucher: AdminVoucherDto) => {
    navigate(`/admin/vouchers/create?copyFromId=${voucher.id}`)
  }

  const confirmToggle = async () => {
    if (!pendingToggle) return
    setBusy(true)
    setError('')
    setSuccess('')

    try {
      const message = await adminApi.vouchers.toggleStatus(pendingToggle.id)
      setSuccess(message || 'Voucher status updated.')
      setPendingToggle(null)
      await loadVouchers(pageNumber)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update voucher status.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between rounded-xl border border-slate-100 bg-white p-4 shadow-sm lg:p-5">
        <div>
          <h3 className="text-lg font-bold text-slate-800">Voucher Management</h3>
          <p className="text-sm text-slate-500">Monitor and create marketplace discount programs.</p>
        </div>
        <Link to="/admin/vouchers/create" className="flex items-center gap-2 rounded-lg bg-primary px-5 py-2.5 font-semibold text-white shadow-sm shadow-primary/20 transition-all hover:bg-blue-700">
          <span className="material-symbols-outlined">add</span>
          Create Voucher
        </Link>
      </div>

      <div className="flex flex-wrap gap-4">
        <KpiCard helper="+12% vs last month" icon="confirmation_number" label="Total Vouchers" tone="bg-blue-50 text-primary" value={summary.totalVouchers} />
        <KpiCard helper="Currently live" icon="bolt" label="Active Today" tone="bg-emerald-50 text-emerald-600" value={summary.activeToday} />
        <KpiCard helper="+4.1% performance" icon="percent" label="Redemption Rate" tone="bg-amber-50 text-amber-600" value={`${summary.redemptionRate}%`} />
        <KpiCard dark helper="Platform impact" icon="payments" label="Total Saved for Users" tone="bg-white/10 text-white" value={formatVnd(summary.totalSavedAmount)} />
      </div>

      <form onSubmit={handleSearch}>
        <section className="space-y-4">
          <div className="flex border-b border-slate-200">
            <button type="button" onClick={() => filterByStatus('')} className={`border-b-2 px-6 py-3 text-sm font-semibold transition-all ${filters.status === '' ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:bg-slate-50 hover:text-slate-700'}`}>
              All
            </button>
            {([voucherStatus.upcoming, voucherStatus.active, voucherStatus.expired, voucherStatus.disabled] as VoucherStatus[]).map((status) => (
              <button key={status} type="button" onClick={() => filterByStatus(status)} className={`border-b-2 px-6 py-3 text-sm font-semibold transition-all ${filters.status === status ? 'border-primary text-primary' : 'border-transparent text-slate-500 hover:bg-slate-50 hover:text-slate-700'}`}>
                {statusLabel(status)}
              </button>
            ))}
          </div>

          <div className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
            <div className="p-4">
              <div className="flex flex-col gap-4 lg:flex-row">
                <div className="relative flex-1">
                  <span className="absolute inset-y-0 left-3 flex items-center text-slate-400">
                    <span className="material-symbols-outlined text-[20px]">search</span>
                  </span>
                  <input value={filters.search} onChange={(event) => updateFilters({ search: event.target.value })} className="h-[44px] w-full rounded-lg border border-slate-200 bg-slate-50 py-2 pl-10 pr-4 text-sm outline-none transition-all focus:border-primary focus:ring-2 focus:ring-primary/20" placeholder="Search code or name..." />
                </div>

                <select value={filters.scope} onChange={(event) => { const scope = event.target.value === '' ? '' : Number(event.target.value) as VoucherScope; updateFilters({ scope }); void loadVouchers(1, { scope }) }} className="h-[44px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 lg:w-40">
                  <option value="">All Scopes</option>
                  <option value={voucherScope.platform}>Platform</option>
                  <option value={voucherScope.seller}>Seller</option>
                </select>

                <select value={filters.voucherType} onChange={(event) => { const nextType = event.target.value === '' ? '' : Number(event.target.value) as VoucherType; updateFilters({ voucherType: nextType }); void loadVouchers(1, { voucherType: nextType }) }} className="h-[44px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 lg:w-44">
                  <option value="">Voucher Type</option>
                  <option value={voucherType.orderDiscount}>OrderDiscount</option>
                  <option value={voucherType.shippingDiscount}>ShippingDiscount</option>
                </select>

                <select value={filters.sortOption} onChange={(event) => { const sortOption = event.target.value; updateFilters({ sortOption }); void loadVouchers(1, { sortOption }) }} className="h-[44px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20 lg:w-48">
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

                <div className="flex items-center gap-2">
                  <button type="submit" className="flex items-center gap-1.5 rounded-lg bg-primary px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition-all hover:bg-blue-700">
                    <span className="material-symbols-outlined text-[20px]">search</span>
                    <span>Search</span>
                  </button>
                  <button type="button" onClick={() => setShowAdvanced((value) => !value)} className={`flex items-center gap-2 rounded-lg border px-3.5 py-2.5 text-sm text-slate-600 shadow-sm transition-colors hover:bg-slate-50 ${showAdvancedPanel ? 'border-slate-300 bg-slate-50 text-slate-900' : 'border-slate-200 bg-white'}`}>
                    <span className="material-symbols-outlined text-[20px]">tune</span>
                    <span className="hidden sm:inline">Filters</span>
                    {activeCount > 0 && <span className="flex size-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold text-white">{activeCount}</span>}
                  </button>
                  <button type="button" onClick={resetFilters} className="rounded-lg border border-slate-200 bg-white p-2.5 text-slate-400 shadow-sm transition-all hover:bg-slate-50 hover:text-slate-600" title="Reset Filters">
                    <span className="material-symbols-outlined text-[20px]">restart_alt</span>
                  </button>
                </div>
              </div>
            </div>

            {showAdvancedPanel && (
              <div className="border-t border-slate-100 bg-slate-50/40 p-4">
                <div className="mb-4 grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
                  <label className="space-y-1.5">
                    <span className="ml-1 flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-slate-400">
                      <span className="material-symbols-outlined text-[14px]">category</span>
                      Category
                    </span>
                    <select value={filters.categoryId} onChange={(event) => updateFilters({ categoryId: event.target.value === '' ? '' : Number(event.target.value) })} className="h-[40px] w-full rounded-lg border border-slate-200 bg-white px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20">
                      <option value="">All Categories</option>
                      {categories.map((category) => (
                        <option key={category.id} value={category.id}>{category.name}</option>
                      ))}
                    </select>
                  </label>
                  <label className="space-y-1.5">
                    <span className="ml-1 flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-slate-400">
                      <span className="material-symbols-outlined text-[14px]">payments</span>
                      Discount Type
                    </span>
                    <select value={filters.discountType} onChange={(event) => updateFilters({ discountType: event.target.value === '' ? '' : Number(event.target.value) as DiscountType })} className="h-[40px] w-full rounded-lg border border-slate-200 bg-white px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20">
                      <option value="">All Types</option>
                      <option value={discountType.percent}>Percent</option>
                      <option value={discountType.fixedAmount}>FixedAmount</option>
                    </select>
                  </label>
                  <label className="space-y-1.5">
                    <span className="ml-1 flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-slate-400">
                      <span className="material-symbols-outlined text-[14px]">calendar_today</span>
                      Start Date
                    </span>
                    <input value={filters.startDate} onChange={(event) => updateFilters({ startDate: event.target.value })} type="date" className="h-[40px] w-full rounded-lg border border-slate-200 bg-white px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                  </label>
                  <label className="space-y-1.5">
                    <span className="ml-1 flex items-center gap-1.5 text-[11px] font-bold uppercase tracking-widest text-slate-400">
                      <span className="material-symbols-outlined text-[14px]">event</span>
                      End Date
                    </span>
                    <input value={filters.endDate} onChange={(event) => updateFilters({ endDate: event.target.value })} type="date" className="h-[40px] w-full rounded-lg border border-slate-200 bg-white px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20" />
                  </label>
                </div>
                <div className="flex justify-end border-t border-slate-200/60 pt-4">
                  <button type="submit" className="rounded-lg bg-slate-800 px-6 py-2 text-sm font-bold text-white shadow-sm transition-all hover:bg-slate-900">
                    Apply Advanced Filters
                  </button>
                </div>
              </div>
            )}
          </div>
        </section>
      </form>

      {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}
      {success && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">{success}</div>}

      <section className="flex flex-col gap-4">
        {loading ? (
          Array.from({ length: 4 }).map((_, index) => <div key={index} className="h-36 animate-pulse rounded-xl bg-slate-100" />)
        ) : rows.length ? (
          rows.map((voucher) => <VoucherTicket key={voucher.id} voucher={voucher} onDuplicate={duplicateVoucher} onToggle={setPendingToggle} />)
        ) : (
          <div className="flex flex-col items-center justify-center rounded-xl border border-slate-100 bg-white p-12 text-center shadow-sm">
            <div className="mb-4 flex size-16 items-center justify-center rounded-full bg-slate-50 text-slate-300">
              <span className="material-symbols-outlined text-[40px]">confirmation_number</span>
            </div>
            <h3 className="text-lg font-bold text-slate-800">No vouchers found</h3>
            <p className="mx-auto mb-6 mt-1 max-w-xs text-slate-500">Start by creating a new voucher for the marketplace.</p>
            <Link to="/admin/vouchers/create" className="inline-flex items-center gap-2 rounded-lg bg-primary px-6 py-2.5 font-bold text-white shadow-md shadow-primary/20 transition-all hover:bg-blue-700">
              <span className="material-symbols-outlined">add</span>
              Create First Voucher
            </Link>
          </div>
        )}
      </section>

      <div className="flex flex-col items-center justify-between gap-4 rounded-xl border border-slate-100 bg-white px-6 py-4 shadow-sm sm:flex-row">
        <div className="text-[13px] font-medium text-slate-500">
          Showing <span className="font-bold text-slate-900">{currentRange.start}</span> to <span className="font-bold text-slate-900">{currentRange.end}</span> of{' '}
          <span className="font-bold text-slate-900">{currentRange.total}</span> vouchers
        </div>
        {totalPages > 1 && (
          <nav aria-label="Pagination" className="flex items-center gap-1.5">
            <button type="button" disabled={pageNumber <= 1 || loading} onClick={() => goToPage(pageNumber - 1)} className="flex size-9 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-400 transition-all hover:bg-slate-50 hover:text-slate-700 disabled:cursor-not-allowed disabled:opacity-50">
              <span className="material-symbols-outlined text-[20px]">chevron_left</span>
            </button>
            {pages.map((page) =>
              typeof page === 'number' ? (
                <button key={page} type="button" onClick={() => goToPage(page)} className={`flex size-9 items-center justify-center rounded-lg border text-sm font-bold transition-all ${pageNumber === page ? 'border-primary bg-primary text-white shadow-sm shadow-primary/20' : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50'}`}>
                  {page}
                </button>
              ) : (
                <span key={page} className="flex items-center justify-center px-2 text-sm font-bold text-slate-300">...</span>
              ),
            )}
            <button type="button" disabled={pageNumber >= totalPages || loading} onClick={() => goToPage(pageNumber + 1)} className="flex size-9 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-400 transition-all hover:bg-slate-50 hover:text-slate-700 disabled:cursor-not-allowed disabled:opacity-50">
              <span className="material-symbols-outlined text-[20px]">chevron_right</span>
            </button>
          </nav>
        )}
      </div>

      {pendingToggle && (
        <ConfirmModal busy={busy} confirmLabel="Change Status" icon="published_with_changes" onClose={() => setPendingToggle(null)} onConfirm={() => void confirmToggle()} title="Change Voucher Status">
          Are you sure you want to change <span className="font-bold text-slate-900">{pendingToggle.code}</span>'s status?
        </ConfirmModal>
      )}
    </div>
  )
}

export default function AdminVouchersPage() {
  const location = useLocation()
  const mode: VoucherMode = location.pathname.includes('/create') ? 'create' : location.pathname.includes('/edit/') ? 'edit' : 'list'
  const title = mode === 'create' ? 'Create Voucher' : mode === 'edit' ? 'Edit Voucher' : 'Voucher Management'
  const breadcrumb = mode === 'list' ? ['Dashboard', 'Marketing', 'Vouchers'] : ['Dashboard', 'Marketing', 'Vouchers', mode === 'edit' ? 'Edit' : 'Create']

  return (
    <AdminLayout activePage="Vouchers" breadcrumb={breadcrumb} pageHeader={title}>
      {mode === 'list' ? <VoucherListPage /> : <VoucherFormPage mode={mode} />}
    </AdminLayout>
  )
}
