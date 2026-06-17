/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { ChangeEvent, FormEvent, ReactNode } from 'react'
import {
  adminApi,
  type AdminBrandDto,
  type AdminBrandFormRequest,
  type AdminBrandStatsDto,
  type PagedResult,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const PAGE_SIZE = 10

type StatusFilter = boolean | ''
type ModalMode = 'create' | 'edit' | null

interface BrandFormState {
  id?: number
  name: string
  slug: string
  logoUrl: string
  logoFile: File | null
  logoPreview: string
  logoSource: string
  isApproved: boolean
}

const emptyStats: AdminBrandStatsDto = {
  approvedBrands: 0,
  pendingBrands: 0,
  totalBrands: 0,
}

const emptyForm: BrandFormState = {
  isApproved: true,
  logoFile: null,
  logoPreview: '',
  logoSource: '',
  logoUrl: '',
  name: '',
  slug: '',
}

function toSlug(value: string) {
  return value
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/[\u0111\u0110]/g, 'd')
    .replace(/[^a-z0-9\s-]/g, '')
    .replace(/\s+/g, '-')
    .replace(/-+/g, '-')
    .replace(/^-|-$/g, '')
}

function formFromBrand(brand: AdminBrandDto): BrandFormState {
  return {
    id: brand.id,
    isApproved: brand.isApproved,
    logoFile: null,
    logoPreview: brand.logoUrl ?? '',
    logoSource: brand.logoUrl ? 'Current logo' : '',
    logoUrl: brand.logoUrl ?? '',
    name: brand.name,
    slug: brand.slug,
  }
}

function toRequest(form: BrandFormState): AdminBrandFormRequest {
  return {
    id: form.id,
    isApproved: form.isApproved,
    logoFile: form.logoFile,
    logoUrl: form.logoFile ? '' : form.logoUrl.trim(),
    name: form.name.trim(),
    slug: form.slug.trim(),
  }
}

function rangeText(brands: PagedResult<AdminBrandDto> | null, pageNumber: number) {
  const total = brands?.totalCount ?? 0
  const page = brands?.pageNumber ?? pageNumber
  const pageSize = brands?.pageSize ?? PAGE_SIZE
  const start = total === 0 ? 0 : (page - 1) * pageSize + 1
  const end = Math.min(page * pageSize, total)
  return { end, start, total }
}

function pageCount(brands: PagedResult<AdminBrandDto> | null) {
  if (!brands) return 1
  return brands.totalPages || Math.max(1, Math.ceil(brands.totalCount / brands.pageSize))
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

function StatusBadge({ isApproved }: { isApproved: boolean }) {
  if (isApproved) {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-green-50 px-2.5 py-1 text-xs font-medium text-green-700 ring-1 ring-inset ring-green-600/20">
        <span className="size-1.5 rounded-full bg-green-600" />
        Approved
      </span>
    )
  }

  return (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-amber-50 px-2.5 py-1 text-xs font-medium text-amber-700 ring-1 ring-inset ring-amber-600/20">
      <span className="size-1.5 rounded-full bg-amber-500" />
      Pending
    </span>
  )
}

function ModalShell({
  children,
  maxWidth = 'max-w-lg',
  onClose,
}: {
  children: ReactNode
  maxWidth?: string
  onClose: () => void
}) {
  return (
    <div className="fixed inset-0 z-[60]">
      <button type="button" aria-label="Close modal" className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" onClick={onClose} />
      <div className="absolute inset-0 flex items-center justify-center p-4">
        <div className={`relative flex max-h-[92vh] w-full ${maxWidth} flex-col overflow-hidden rounded-2xl bg-white shadow-2xl`}>{children}</div>
      </div>
    </div>
  )
}

function LoadingRows() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, index) => (
        <tr key={index} className="animate-pulse">
          <td colSpan={6} className="px-6 py-4">
            <div className="h-12 rounded-lg bg-slate-100" />
          </td>
        </tr>
      ))}
    </>
  )
}

function LogoBox({
  mode,
  onClear,
  onFileChange,
  onUrlChange,
  preview,
  source,
  urlValue,
}: {
  mode: 'create' | 'edit'
  onClear: () => void
  onFileChange: (file: File) => void
  onUrlChange: (value: string) => void
  preview: string
  source: string
  urlValue: string
}) {
  const inputId = `${mode}-brand-logo-file`

  const handleFile = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (file) onFileChange(file)
  }

  return (
    <div className="space-y-1.5">
      <label className="block text-sm font-semibold text-slate-700">Brand Logo</label>
      <input id={inputId} type="file" accept="image/*" className="hidden" onChange={handleFile} />
      <button
        type="button"
        onClick={() => document.getElementById(inputId)?.click()}
        className={`group relative flex h-40 w-full cursor-pointer items-center justify-center overflow-hidden rounded-2xl border-2 border-dashed transition-all ${
          preview ? 'border-primary/40 bg-primary/5' : 'border-slate-200 bg-slate-50 hover:border-primary/50 hover:bg-primary/5'
        }`}
      >
        {preview ? (
          <>
            <img src={preview} alt="Logo preview" className="absolute inset-0 size-full object-contain p-3" />
            <span className="absolute bottom-2 left-2 rounded-full bg-slate-800/60 px-2 py-0.5 text-[10px] font-bold text-white backdrop-blur-sm">
              {source}
            </span>
          </>
        ) : (
          <div className="flex select-none flex-col items-center gap-2 text-slate-400">
            <div className="flex size-12 items-center justify-center rounded-xl bg-slate-200 transition-colors group-hover:bg-primary/10">
              <span className="material-symbols-outlined text-[28px] text-slate-400 transition-colors group-hover:text-primary">
                add_photo_alternate
              </span>
            </div>
            <div className="text-center">
              <p className="text-sm font-semibold text-slate-600">Click to upload image</p>
              <p className="mt-0.5 text-xs text-slate-400">PNG, JPG, WEBP up to 5MB</p>
            </div>
          </div>
        )}
      </button>

      {preview && (
        <button
          type="button"
          onClick={onClear}
          className="-mt-10 ml-auto mr-2 flex size-7 items-center justify-center rounded-full bg-red-500 text-white shadow-md transition-all hover:bg-red-600"
          title="Clear logo"
        >
          <span className="material-symbols-outlined text-[16px]">close</span>
        </button>
      )}

      <div className="relative">
        <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-slate-300">
          <span className="material-symbols-outlined text-[18px]">link</span>
        </span>
        <input
          value={urlValue}
          onChange={(event) => onUrlChange(event.target.value)}
          className="w-full rounded-xl border border-slate-200 bg-slate-50 py-2.5 pl-9 pr-4 text-sm transition-all focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
          placeholder="Or paste image URL: https://..."
          type="url"
        />
      </div>
    </div>
  )
}

function BrandFormModal({
  busy,
  form,
  mode,
  onChange,
  onClose,
  onSubmit,
}: {
  busy: boolean
  form: BrandFormState
  mode: 'create' | 'edit'
  onChange: (patch: Partial<BrandFormState>) => void
  onClose: () => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
}) {
  const title = mode === 'create' ? 'Add New Brand' : 'Edit Brand'

  const handleFileChange = (file: File) => {
    const reader = new FileReader()
    reader.onload = () => {
      onChange({
        logoFile: file,
        logoPreview: String(reader.result ?? ''),
        logoSource: 'Local file',
        logoUrl: '',
      })
    }
    reader.readAsDataURL(file)
  }

  const handleUrlChange = (value: string) => {
    onChange({
      logoFile: null,
      logoPreview: value.trim(),
      logoSource: value.trim() ? 'URL' : '',
      logoUrl: value,
    })
  }

  return (
    <ModalShell onClose={onClose}>
      <form onSubmit={onSubmit} className="flex flex-1 flex-col overflow-hidden">
        <div className="flex items-center justify-between border-b border-slate-100 bg-slate-50/50 px-6 py-4">
          <h3 className="text-lg font-bold text-slate-900">{title}</h3>
          <button type="button" onClick={onClose} className="text-slate-400 transition-colors hover:text-slate-600">
            <span className="material-symbols-outlined">close</span>
          </button>
        </div>

        <div className="flex-1 space-y-5 overflow-y-auto p-6">
          <label className="block space-y-1.5">
            <span className="block text-sm font-semibold text-slate-700">
              Brand Name <span className="text-red-500">*</span>
            </span>
            <input
              required
              value={form.name}
              onChange={(event) => {
                const name = event.target.value
                onChange({ name, slug: toSlug(name) })
              }}
              className="w-full rounded-xl border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm transition-all focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
              placeholder="e.g. ASUS, Samsung, Apple"
              type="text"
            />
          </label>

          <label className="block space-y-1.5">
            <span className="block text-sm font-semibold text-slate-700">
              Slug <span className="text-red-500">*</span>
            </span>
            <div className="relative">
              <input
                required
                value={form.slug}
                onChange={(event) => onChange({ slug: event.target.value })}
                className="w-full rounded-xl border border-slate-200 bg-slate-50 py-2.5 pl-3 pr-24 font-mono text-sm transition-all focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary"
                placeholder="auto-generated-from-name"
                type="text"
              />
              <button
                type="button"
                onClick={() => onChange({ slug: toSlug(form.name) })}
                className="absolute right-2 top-1/2 -translate-y-1/2 rounded-lg px-2 py-1 text-xs font-semibold text-primary transition-colors hover:bg-primary/5 hover:text-blue-700"
              >
                Generate
              </button>
            </div>
            <p className="text-xs text-slate-400">Used for SEO-friendly URLs. Auto-generated from name.</p>
          </label>

          <LogoBox
            mode={mode}
            preview={form.logoPreview}
            source={form.logoSource}
            urlValue={form.logoUrl}
            onFileChange={handleFileChange}
            onUrlChange={handleUrlChange}
            onClear={() => onChange({ logoFile: null, logoPreview: '', logoSource: '', logoUrl: '' })}
          />

          <label className="flex items-center gap-3">
            <span className="relative inline-flex cursor-pointer items-center">
              <input
                checked={form.isApproved}
                onChange={(event) => onChange({ isApproved: event.target.checked })}
                type="checkbox"
                className="peer sr-only"
              />
              <span className="h-5 w-9 rounded-full bg-slate-200 transition peer-checked:bg-green-500" />
              <span className="absolute left-0.5 top-0.5 size-4 rounded-full border border-slate-300 bg-white transition peer-checked:translate-x-full peer-checked:border-white" />
            </span>
            <span className="text-sm font-medium text-slate-700">Approved explicitly?</span>
          </label>
        </div>

        <div className="flex items-center justify-end gap-3 border-t border-slate-100 px-6 py-4">
          <button
            type="button"
            disabled={busy}
            onClick={onClose}
            className="rounded-xl border border-slate-200 bg-white px-5 py-2.5 text-sm font-semibold text-slate-700 transition-colors hover:bg-slate-50 disabled:opacity-60"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={busy}
            className="flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-bold text-white shadow-md shadow-blue-500/20 transition-all hover:bg-blue-700 disabled:opacity-60"
          >
            <span className="material-symbols-outlined text-[18px]">save</span>
            {busy ? 'Saving...' : mode === 'create' ? 'Save' : 'Save Changes'}
          </button>
        </div>
      </form>
    </ModalShell>
  )
}

export default function AdminBrandsPage() {
  const [brands, setBrands] = useState<PagedResult<AdminBrandDto> | null>(null)
  const [stats, setStats] = useState<AdminBrandStatsDto>(emptyStats)
  const [searchTerm, setSearchTerm] = useState('')
  const [status, setStatus] = useState<StatusFilter>('')
  const [pageNumber, setPageNumber] = useState(1)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [modal, setModal] = useState<ModalMode>(null)
  const [form, setForm] = useState<BrandFormState>(emptyForm)
  const [pendingDelete, setPendingDelete] = useState<AdminBrandDto | null>(null)

  const rows = brands?.items ?? []
  const totalPages = pageCount(brands)
  const pageNumbers = Array.from({ length: totalPages }, (_, index) => index + 1)
  const currentRange = useMemo(() => rangeText(brands, pageNumber), [brands, pageNumber])

  const loadBrands = async (nextPage = pageNumber, overrides?: { searchTerm?: string; status?: StatusFilter }) => {
    setLoading(true)
    setError('')

    const effectiveSearch = overrides?.searchTerm ?? searchTerm
    const effectiveStatus = overrides?.status ?? status

    try {
      const data = await adminApi.brands.list({
        isApproved: effectiveStatus,
        pageNumber: nextPage,
        pageSize: PAGE_SIZE,
        searchTerm: effectiveSearch.trim() || undefined,
      })
      setBrands(data.brands)
      setStats(data.stats)
      setPageNumber(data.brands.pageNumber)
    } catch (err) {
      setBrands(null)
      setError(err instanceof Error ? err.message : 'Unable to load brands.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadBrands(1)
  }, [])

  const closeModal = () => {
    if (saving) return
    setModal(null)
    setForm(emptyForm)
  }

  const openCreate = () => {
    setForm(emptyForm)
    setModal('create')
  }

  const openEdit = (brand: AdminBrandDto) => {
    setForm(formFromBrand(brand))
    setModal('edit')
  }

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadBrands(1)
  }

  const handleReset = () => {
    setSearchTerm('')
    setStatus('')
    void loadBrands(1, { searchTerm: '', status: '' })
  }

  const goToPage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > totalPages || loading) return
    void loadBrands(nextPage)
  }

  const runAction = async (action: () => Promise<string>, messageFallback: string) => {
    setSaving(true)
    setError('')
    setSuccess('')

    try {
      const message = await action()
      setSuccess(message || messageFallback)
      closeModal()
      setPendingDelete(null)
      await loadBrands(pageNumber)
    } catch (err) {
      setError(err instanceof Error ? err.message : messageFallback)
    } finally {
      setSaving(false)
    }
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    if (modal === 'create') {
      void runAction(() => adminApi.brands.create(toRequest(form)), 'Failed to create brand.')
      return
    }
    const brandId = form.id
    if (modal === 'edit' && brandId !== undefined) {
      void runAction(() => adminApi.brands.update({ ...toRequest(form), id: brandId }), 'Failed to update brand.')
    }
  }

  const approveBrand = (brand: AdminBrandDto) => {
    void runAction(() => adminApi.brands.approve(brand.id), 'Failed to approve brand.')
  }

  const deleteBrand = () => {
    if (!pendingDelete) return
    void runAction(() => adminApi.brands.delete(pendingDelete.id), 'Failed to delete brand.')
  }

  return (
    <AdminLayout activePage="Brands" breadcrumb={['Dashboard', 'Brand Management']} pageHeader="Brand Management">
      <div className="flex flex-col gap-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <StatCard icon="branding_watermark" label="Total Brands" tone="bg-blue-50 text-blue-600" value={stats.totalBrands} />
          <StatCard icon="verified" label="Approved" tone="bg-green-50 text-green-600" value={stats.approvedBrands} />
          <StatCard icon="pending_actions" label="Pending" tone="bg-amber-50 text-amber-600" value={stats.pendingBrands} />
        </div>

        <div className="flex flex-col items-start justify-between gap-4 rounded-xl border border-slate-100 bg-white p-4 shadow-sm lg:flex-row lg:items-end">
          <form onSubmit={handleSearch} className="flex w-full flex-1 flex-col gap-4 sm:flex-row lg:w-auto">
            <div className="relative w-full sm:max-w-xs">
              <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-slate-400">
                <span className="material-symbols-outlined text-[20px]">search</span>
              </span>
              <input
                value={searchTerm}
                onChange={(event) => setSearchTerm(event.target.value)}
                className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 py-2.5 pl-10 pr-4 text-sm text-slate-900 placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                placeholder="Search brands..."
                type="text"
              />
            </div>

            <select
              value={status === '' ? '' : String(status)}
              onChange={(event) => {
                const nextStatus = event.target.value === '' ? '' : event.target.value === 'true'
                setStatus(nextStatus)
                void loadBrands(1, { status: nextStatus })
              }}
              className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-900 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 sm:w-40"
            >
              <option value="">All Statuses</option>
              <option value="true">Approved</option>
              <option value="false">Pending</option>
            </select>

            <button type="submit" className="hidden">
              Search
            </button>

            <button
              type="button"
              onClick={handleReset}
              className="flex h-[42px] items-center justify-center rounded-lg border border-slate-200 px-4 text-slate-600 transition-colors hover:bg-slate-50 hover:text-slate-900"
              title="Reset filters"
            >
              <span className="material-symbols-outlined text-[20px]">restart_alt</span>
            </button>
          </form>

          <button
            type="button"
            onClick={openCreate}
            className="flex h-[42px] w-full items-center justify-center gap-2 rounded-lg bg-primary px-5 py-2.5 text-sm font-medium text-white shadow-sm shadow-blue-500/30 transition hover:bg-blue-700 lg:w-auto"
          >
            <span className="material-symbols-outlined text-[20px]">add</span>
            <span>Add Brand</span>
          </button>
        </div>

        {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}
        {success && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">{success}</div>}

        <div className="flex flex-col overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-left">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50/50">
                  <th className="w-[50px] py-4 pl-6 pr-3 text-xs font-semibold uppercase tracking-wider text-slate-500">ID</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Brand</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Slug</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Status</th>
                  <th className="hidden px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">Products</th>
                  <th className="py-4 pl-3 pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  <LoadingRows />
                ) : rows.length ? (
                  rows.map((brand) => (
                    <tr key={brand.id} onClick={() => openEdit(brand)} className="group cursor-pointer transition-all hover:bg-slate-50">
                      <td className="py-4 pl-6 pr-3 text-sm font-medium text-slate-500">#{brand.id}</td>
                      <td className="px-3 py-4">
                        <div className="flex items-center gap-3">
                          <div className="relative shrink-0">
                            {brand.logoUrl ? (
                              <img src={brand.logoUrl} alt={brand.name} className="size-10 rounded-lg border border-slate-100 bg-white object-contain p-1 shadow-sm" />
                            ) : (
                              <div className="flex size-10 items-center justify-center rounded-lg border border-slate-200 bg-slate-100 text-slate-400 shadow-sm">
                                <span className="material-symbols-outlined">image_not_supported</span>
                              </div>
                            )}
                          </div>
                          <span className="text-sm font-semibold text-slate-900">{brand.name}</span>
                        </div>
                      </td>
                      <td className="px-3 py-4">
                        <span className="text-sm text-slate-500">{brand.slug}</span>
                      </td>
                      <td className="px-3 py-4">
                        <StatusBadge isApproved={brand.isApproved} />
                      </td>
                      <td className="hidden px-3 py-4 md:table-cell">
                        <div className="flex items-center gap-1.5">
                          <span className="material-symbols-outlined text-[16px] text-slate-400">inventory_2</span>
                          <span className="text-sm font-medium text-slate-700">{brand.productCount}</span>
                        </div>
                      </td>
                      <td className="py-4 pl-3 pr-6 text-right">
                        <div onClick={(event) => event.stopPropagation()} className="flex items-center justify-end gap-2 opacity-0 transition-opacity group-hover:opacity-100">
                          {!brand.isApproved && (
                            <button
                              type="button"
                              disabled={saving}
                              onClick={() => approveBrand(brand)}
                              className="p-1 text-slate-400 transition-colors hover:text-green-600 disabled:opacity-60"
                              title="Approve Brand"
                            >
                              <span className="material-symbols-outlined text-[20px]">check_circle</span>
                            </button>
                          )}
                          <button
                            type="button"
                            onClick={() => openEdit(brand)}
                            className="p-1 text-slate-400 transition-colors hover:text-amber-500"
                            title="Edit Brand"
                          >
                            <span className="material-symbols-outlined text-[20px]">edit</span>
                          </button>
                          <button
                            type="button"
                            onClick={() => setPendingDelete(brand)}
                            className="p-1 text-slate-400 transition-colors hover:text-red-600"
                            title="Delete Brand"
                          >
                            <span className="material-symbols-outlined text-[20px]">delete</span>
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="py-8 text-center text-slate-500">
                      <div className="flex flex-col items-center justify-center gap-3">
                        <span className="material-symbols-outlined text-4xl text-slate-300">search_off</span>
                        <p className="text-base font-medium">No brands found</p>
                        <p className="text-sm">Try adjusting your filters or search term.</p>
                      </div>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <div className="flex flex-col items-center justify-between gap-4 border-t border-slate-100 bg-slate-50/30 px-6 py-4 sm:flex-row">
            <div className="text-sm text-slate-500">
              Showing <span className="font-medium text-slate-900">{currentRange.start}</span> to{' '}
              <span className="font-medium text-slate-900">{currentRange.end}</span> of{' '}
              <span className="font-medium text-slate-900">{currentRange.total}</span> brands
            </div>

            {totalPages > 1 && (
              <nav aria-label="Pagination" className="flex items-center gap-1">
                <button
                  type="button"
                  disabled={pageNumber <= 1 || loading}
                  onClick={() => goToPage(pageNumber - 1)}
                  className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:pointer-events-none disabled:opacity-50"
                  title="Previous"
                >
                  <span className="material-symbols-outlined text-lg leading-none">chevron_left</span>
                </button>
                <div className="flex items-center gap-1 px-1">
                  {pageNumbers.map((page) => (
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
                  ))}
                </div>
                <button
                  type="button"
                  disabled={pageNumber >= totalPages || loading}
                  onClick={() => goToPage(pageNumber + 1)}
                  className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:pointer-events-none disabled:opacity-50"
                  title="Next"
                >
                  <span className="material-symbols-outlined text-lg leading-none">chevron_right</span>
                </button>
              </nav>
            )}
          </div>
        </div>
      </div>

      {modal && (
        <BrandFormModal
          busy={saving}
          form={form}
          mode={modal}
          onChange={(patch) => setForm((current) => ({ ...current, ...patch }))}
          onClose={closeModal}
          onSubmit={handleSubmit}
        />
      )}

      {pendingDelete && (
        <ModalShell maxWidth="max-w-md" onClose={() => setPendingDelete(null)}>
          <div className="p-6">
            <div className="flex gap-4">
              <div className="mt-1 flex size-10 shrink-0 items-center justify-center rounded-full bg-red-100 text-red-600">
                <span className="material-symbols-outlined text-2xl">warning</span>
              </div>
              <div>
                <h3 className="text-xl font-bold text-slate-900">Delete Brand</h3>
                <p className="mt-1 text-sm text-slate-500">
                  Are you sure you want to delete <span className="font-bold text-slate-900">{pendingDelete.name}</span>?
                </p>
              </div>
            </div>
            <div className="mt-6 flex justify-end gap-3 border-t border-slate-100 pt-5">
              <button type="button" disabled={saving} onClick={() => setPendingDelete(null)} className="rounded-xl border border-slate-200 px-5 py-2 text-sm font-bold text-slate-600 hover:bg-slate-50 disabled:opacity-60">
                Cancel
              </button>
              <button type="button" disabled={saving} onClick={deleteBrand} className="rounded-xl bg-red-600 px-6 py-2 text-sm font-bold text-white hover:bg-red-700 disabled:opacity-60">
                {saving ? 'Deleting...' : 'Confirm Delete'}
              </button>
            </div>
          </div>
        </ModalShell>
      )}
    </AdminLayout>
  )
}
