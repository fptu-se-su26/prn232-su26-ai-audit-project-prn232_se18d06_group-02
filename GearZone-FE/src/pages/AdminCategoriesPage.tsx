/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { DragEvent, FormEvent, ReactNode } from 'react'
import { Link, useLocation, useNavigate, useParams } from 'react-router-dom'
import {
  adminApi,
  type AdminCategoryAttributeDto,
  type AdminCategoryAttributeOptionDto,
  type AdminCategoryDto,
  type CreateAdminCategoryRequest,
  type EditAdminCategoryRequest,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

type CategoryMode = 'list' | 'create' | 'edit'
type StatusFilter = boolean | ''

interface CategoryFormState {
  id?: number
  name: string
  slug: string
  parentId: number | ''
  isActive: boolean
}

const emptyForm: CategoryFormState = {
  isActive: true,
  name: '',
  parentId: '',
  slug: '',
}

const emptyAttribute = (): AdminCategoryAttributeDto => ({
  categoryId: 0,
  displayOrder: 0,
  filterType: 'Checkbox',
  id: 0,
  isFilterable: true,
  name: '',
  options: [],
})

const numberFormatter = new Intl.NumberFormat('en-US')
const currencyFormatter = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 })

function formatNumber(value: number) {
  return numberFormatter.format(value ?? 0)
}

function formatCurrency(value: number) {
  return currencyFormatter.format(value ?? 0)
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

function flattenCategories(categories: AdminCategoryDto[], depth = 0): Array<{ depth: number; item: AdminCategoryDto }> {
  return categories.flatMap((category) => [
    { depth, item: category },
    ...flattenCategories(category.children ?? [], depth + 1),
  ])
}

function categoryOptions(categories: AdminCategoryDto[]) {
  return flattenCategories(categories).sort((a, b) => a.item.name.localeCompare(b.item.name))
}

function formFromCategory(category: AdminCategoryDto): CategoryFormState {
  return {
    id: category.id,
    isActive: category.isActive,
    name: category.name,
    parentId: category.parentId ?? '',
    slug: category.slug,
  }
}

function StatusBadge({ category }: { category: AdminCategoryDto }) {
  if (category.isDeleted) {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-red-50 px-2.5 py-1 text-xs font-medium text-red-700 ring-1 ring-inset ring-red-600/20">
        <span className="size-1.5 rounded-full bg-red-600" />
        Deleted
      </span>
    )
  }

  if (category.isActive) {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-green-50 px-2.5 py-1 text-xs font-medium text-green-700 ring-1 ring-inset ring-green-600/20">
        <span className="size-1.5 rounded-full bg-green-600" />
        Active
      </span>
    )
  }

  return (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-slate-50 px-2.5 py-1 text-xs font-medium text-slate-600 ring-1 ring-inset ring-slate-500/20">
      <span className="size-1.5 rounded-full bg-slate-400" />
      Inactive
    </span>
  )
}

function ModalShell({ children, onClose }: { children: ReactNode; onClose: () => void }) {
  return (
    <div className="fixed inset-0 z-[60]">
      <button type="button" aria-label="Close modal" className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" onClick={onClose} />
      <div className="absolute inset-0 flex items-center justify-center p-4">
        <div className="relative flex w-full max-w-md flex-col overflow-hidden rounded-3xl bg-white shadow-2xl">{children}</div>
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

function CategoryList() {
  const [categories, setCategories] = useState<AdminCategoryDto[]>([])
  const [expanded, setExpanded] = useState<Set<number>>(new Set())
  const [searchTerm, setSearchTerm] = useState('')
  const [status, setStatus] = useState<StatusFilter>('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [pendingDelete, setPendingDelete] = useState<AdminCategoryDto | null>(null)
  const [deleting, setDeleting] = useState(false)

  const roots = categories
  const rootCount = roots.length
  const subCount = roots.reduce((sum, category) => sum + (category.children?.length ?? 0), 0)
  const totalCount = rootCount + subCount

  const loadCategories = async (overrides?: { searchTerm?: string; status?: StatusFilter }) => {
    setLoading(true)
    setError('')
    const effectiveSearch = overrides?.searchTerm ?? searchTerm
    const effectiveStatus = overrides?.status ?? status

    try {
      const data = await adminApi.categories.list({
        isActive: effectiveStatus,
        searchTerm: effectiveSearch.trim() || undefined,
      })
      setCategories(data)
      setExpanded(new Set())
    } catch (err) {
      setCategories([])
      setError(err instanceof Error ? err.message : 'Unable to load categories.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadCategories()
  }, [])

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadCategories()
  }

  const handleReset = () => {
    setSearchTerm('')
    setStatus('')
    void loadCategories({ searchTerm: '', status: '' })
  }

  const toggleExpanded = (id: number) => {
    setExpanded((current) => {
      const next = new Set(current)
      if (next.has(id)) next.delete(id)
      else next.add(id)
      return next
    })
  }

  const confirmDelete = async () => {
    if (!pendingDelete) return
    setDeleting(true)
    setError('')
    setSuccess('')

    try {
      const message = await adminApi.categories.delete(pendingDelete.id)
      setSuccess(message)
      setPendingDelete(null)
      await loadCategories()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete category.')
    } finally {
      setDeleting(false)
    }
  }

  const renderCategoryRow = (category: AdminCategoryDto, isChild = false, parentId?: number) => {
    const hasChildren = !isChild && (category.children?.length ?? 0) > 0
    const isExpanded = expanded.has(category.id)

    return (
      <tr
        key={`${isChild ? 'child' : 'root'}-${category.id}`}
        className={`${isChild ? 'bg-slate-50/40 hover:bg-indigo-50/30' : 'bg-white hover:bg-slate-50'} group transition-all ${hasChildren ? 'cursor-pointer' : ''}`}
        data-parent-id={parentId}
        onClick={() => {
          if (hasChildren) toggleExpanded(category.id)
        }}
      >
        <td className={`${isChild ? 'py-3' : 'py-3.5'} px-6`}>
          <div className={`flex items-center gap-2.5 ${isChild ? 'pl-7' : ''}`}>
            {hasChildren ? (
              <span className={`shrink-0 text-slate-400 transition-transform duration-200 ${isExpanded ? 'rotate-90' : ''}`}>
                <span className="material-symbols-outlined text-[18px]">chevron_right</span>
              </span>
            ) : (
              <span className="w-[18px] shrink-0 text-slate-300">
                {isChild && <span className="material-symbols-outlined text-[16px]">chevron_right</span>}
              </span>
            )}
            <span className={`material-symbols-outlined shrink-0 ${isChild ? 'text-[18px] text-indigo-300' : 'text-[20px] text-slate-400'}`}>
              {isChild || isExpanded ? 'folder_open' : 'folder'}
            </span>
            <span className={`${isChild ? 'font-medium text-slate-700 group-hover:text-indigo-700' : 'font-semibold text-slate-900 group-hover:text-primary'} text-sm transition-colors`}>
              {category.name}
            </span>
          </div>
        </td>
        <td className={`${isChild ? 'py-3' : 'py-3.5'} px-3`}>
          <span className={`rounded-md bg-slate-50 px-2 py-1 font-mono text-xs ${isChild ? 'text-slate-400' : 'text-slate-500'}`}>{category.slug}</span>
        </td>
        <td className={`${isChild ? 'py-3' : 'py-3.5'} px-3`}>
          <span
            className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-medium ring-1 ring-inset ${
              isChild ? 'bg-indigo-50 text-indigo-600 ring-indigo-600/10' : 'bg-blue-50 text-blue-700 ring-blue-700/10'
            }`}
          >
            {isChild ? 'Sub 1' : 'Root'}
          </span>
        </td>
        <td className={`${isChild ? 'py-3' : 'py-3.5'} px-3 text-right`}>
          <span className={`${isChild ? 'text-slate-600' : 'font-medium text-slate-700'} text-sm`}>{formatNumber(category.productCount)}</span>
        </td>
        <td className={`${isChild ? 'py-3' : 'py-3.5'} hidden px-3 text-right lg:table-cell`}>
          <span className={`${isChild ? 'text-slate-600' : 'font-medium text-slate-700'} text-sm`}>{formatCurrency(category.revenue)}</span>
        </td>
        <td className={`${isChild ? 'py-3' : 'py-3.5'} px-3`}>
          <StatusBadge category={category} />
        </td>
        <td className={`${isChild ? 'py-3' : 'py-3.5'} py-3.5 pl-3 pr-6 text-right`}>
          <div className="flex items-center justify-end gap-1 opacity-0 transition-opacity group-hover:opacity-100">
            <Link
              to={`/admin/categories/${category.id}/edit`}
              onClick={(event) => event.stopPropagation()}
              className="rounded-lg p-1.5 text-slate-400 transition-colors hover:bg-amber-50 hover:text-amber-500"
              title="Edit Category"
            >
              <span className="material-symbols-outlined text-[20px]">edit</span>
            </Link>
            <button
              type="button"
              onClick={(event) => {
                event.stopPropagation()
                setPendingDelete(category)
              }}
              className="rounded-lg p-1.5 text-slate-400 transition-colors hover:bg-red-50 hover:text-red-600"
              title="Delete Category"
            >
              <span className="material-symbols-outlined text-[20px]">delete</span>
            </button>
          </div>
        </td>
      </tr>
    )
  }

  return (
    <div className="flex flex-col gap-6">
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
              placeholder="Search by name or slug..."
              type="text"
            />
          </div>

          <select
            value={status === '' ? '' : String(status)}
            onChange={(event) => {
              const nextStatus = event.target.value === '' ? '' : event.target.value === 'true'
              setStatus(nextStatus)
              void loadCategories({ status: nextStatus })
            }}
            className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-900 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 sm:w-40"
          >
            <option value="">All Status</option>
            <option value="true">Active</option>
            <option value="false">Inactive</option>
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

        <Link
          to="/admin/categories/create"
          className="flex h-[42px] w-full items-center justify-center gap-2 rounded-lg bg-primary px-5 py-2.5 text-sm font-medium text-white shadow-sm shadow-blue-500/30 transition hover:bg-blue-700 lg:w-auto"
        >
          <span className="material-symbols-outlined text-[20px]">add</span>
          <span>Add New Category</span>
        </Link>
      </div>

      {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}
      {success && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">{success}</div>}

      <div className="flex flex-col overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse text-left">
            <thead>
              <tr className="border-b border-slate-100 bg-slate-50/50">
                <th className="w-2/5 px-6 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Name</th>
                <th className="w-1/5 px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Slug</th>
                <th className="w-[90px] px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Level</th>
                <th className="w-[90px] px-3 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Products</th>
                <th className="hidden w-[110px] px-3 py-4 text-right text-xs font-semibold uppercase tracking-wider text-slate-500 lg:table-cell">Revenue</th>
                <th className="w-[110px] px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Status</th>
                <th className="py-4 pl-3 pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {loading ? (
                <LoadingRows />
              ) : roots.length ? (
                roots.flatMap((root) => [
                  renderCategoryRow(root),
                  ...(expanded.has(root.id) ? (root.children ?? []).map((child) => renderCategoryRow(child, true, root.id)) : []),
                ])
              ) : (
                <tr>
                  <td colSpan={7} className="py-12 text-center text-slate-500">
                    <div className="flex flex-col items-center justify-center gap-3">
                      <span className="material-symbols-outlined text-4xl text-slate-300">search_off</span>
                      <p className="text-base font-medium">No categories found</p>
                      <p className="text-sm">Try adjusting your filters or search term.</p>
                    </div>
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        <div className="flex items-center justify-between gap-4 border-t border-slate-100 bg-slate-50/30 px-6 py-4">
          <div className="text-sm text-slate-500">
            Total: <span className="font-medium text-slate-900">{totalCount}</span> categories (
            <span className="font-medium text-blue-600">{rootCount}</span> root,{' '}
            <span className="font-medium text-indigo-600">{subCount}</span> sub)
          </div>
        </div>
      </div>

      {pendingDelete && (
        <ModalShell onClose={() => setPendingDelete(null)}>
          <div className="space-y-4 p-8 text-center">
            <div className="mx-auto flex size-16 items-center justify-center rounded-full bg-red-100">
              <span className="material-symbols-outlined text-3xl text-red-600">delete_forever</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">Delete Category</h3>
            <p className="text-sm text-slate-500">
              Are you sure you want to soft-delete <strong className="text-slate-800">"{pendingDelete.name}"</strong>?
            </p>
            <p className="text-xs text-slate-400">Products in this category will not be deleted, but the category will be hidden from the storefront.</p>
          </div>
          <div className="flex justify-end gap-3 rounded-b-3xl bg-slate-50/50 px-6 py-4">
            <button
              type="button"
              disabled={deleting}
              onClick={() => setPendingDelete(null)}
              className="rounded-xl border border-slate-200 bg-white px-5 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition-all hover:bg-slate-50 disabled:opacity-60"
            >
              Cancel
            </button>
            <button
              type="button"
              disabled={deleting}
              onClick={() => void confirmDelete()}
              className="rounded-xl bg-red-600 px-6 py-2.5 text-sm font-bold text-white shadow-md shadow-red-600/20 transition-all hover:bg-red-700 disabled:opacity-60"
            >
              {deleting ? 'Deleting...' : 'Yes, Delete'}
            </button>
          </div>
        </ModalShell>
      )}
    </div>
  )
}

function AttributeEditor({
  attributes,
  onChange,
}: {
  attributes: AdminCategoryAttributeDto[]
  onChange: (attributes: AdminCategoryAttributeDto[]) => void
}) {
  const [openIndexes, setOpenIndexes] = useState<Set<number>>(new Set())
  const [draggingAttr, setDraggingAttr] = useState<number | null>(null)
  const [draggingOption, setDraggingOption] = useState<{ attrIndex: number; optionIndex: number } | null>(null)

  const patchAttribute = (index: number, patch: Partial<AdminCategoryAttributeDto>) => {
    onChange(attributes.map((attribute, attrIndex) => (attrIndex === index ? { ...attribute, ...patch } : attribute)))
  }

  const addAttribute = () => {
    const nextIndex = attributes.length
    onChange([...attributes, emptyAttribute()])
    setOpenIndexes((current) => new Set(current).add(nextIndex))
  }

  const removeAttribute = (index: number) => {
    onChange(attributes.filter((_, attrIndex) => attrIndex !== index))
    setOpenIndexes((current) => {
      const next = new Set<number>()
      current.forEach((item) => {
        if (item < index) next.add(item)
        if (item > index) next.add(item - 1)
      })
      return next
    })
  }

  const toggleOpen = (index: number, forceOpen?: boolean) => {
    setOpenIndexes((current) => {
      const next = new Set(current)
      const shouldOpen = forceOpen ?? !next.has(index)
      if (shouldOpen) next.add(index)
      else next.delete(index)
      return next
    })
  }

  const addOption = (attrIndex: number, value: string) => {
    const trimmed = value.trim()
    if (!trimmed) return

    const attribute = attributes[attrIndex]
    const option: AdminCategoryAttributeOptionDto = {
      displayOrder: attribute.options.length,
      id: 0,
      value: trimmed,
    }

    patchAttribute(attrIndex, { options: [...attribute.options, option] })
  }

  const removeOption = (attrIndex: number, optionIndex: number) => {
    const attribute = attributes[attrIndex]
    patchAttribute(attrIndex, { options: attribute.options.filter((_, index) => index !== optionIndex) })
  }

  const moveAttribute = (fromIndex: number, toIndex: number) => {
    if (fromIndex === toIndex) return
    const next = [...attributes]
    const [item] = next.splice(fromIndex, 1)
    next.splice(toIndex, 0, item)
    onChange(next)
  }

  const moveOption = (attrIndex: number, fromIndex: number, toIndex: number) => {
    if (fromIndex === toIndex) return
    const attribute = attributes[attrIndex]
    const options = [...attribute.options]
    const [item] = options.splice(fromIndex, 1)
    options.splice(toIndex, 0, item)
    patchAttribute(attrIndex, { options })
  }

  const handleAttrDragOver = (event: DragEvent<HTMLDivElement>, targetIndex: number) => {
    event.preventDefault()
    if (draggingAttr === null || draggingAttr === targetIndex) return
    moveAttribute(draggingAttr, targetIndex)
    setDraggingAttr(targetIndex)
  }

  const handleOptionDragOver = (event: DragEvent<HTMLDivElement>, attrIndex: number, targetIndex: number) => {
    event.preventDefault()
    if (!draggingOption || draggingOption.attrIndex !== attrIndex || draggingOption.optionIndex === targetIndex) return
    moveOption(attrIndex, draggingOption.optionIndex, targetIndex)
    setDraggingOption({ attrIndex, optionIndex: targetIndex })
  }

  return (
    <div className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
      <div className="h-1 bg-gradient-to-r from-primary/60 to-primary" />
      <div className="flex items-center justify-between border-b border-slate-100 px-6 py-4">
        <div className="flex items-center gap-3">
          <div className="rounded-lg bg-primary/10 p-2">
            <span className="material-symbols-outlined text-[20px] text-primary">tune</span>
          </div>
          <div>
            <h2 className="text-base font-semibold text-slate-800">Category Attributes</h2>
            <p className="mt-0.5 text-xs text-slate-400">Define filters and specs for products in this category.</p>
          </div>
        </div>
        <button
          type="button"
          onClick={addAttribute}
          className="flex items-center gap-1.5 rounded-xl bg-primary px-4 py-2 text-sm font-semibold text-white shadow-sm shadow-primary/30 transition-all hover:bg-blue-700"
        >
          <span className="material-symbols-outlined text-[18px]">add</span>
          Add Attribute
        </button>
      </div>

      <div className="divide-y divide-slate-100">
        {attributes.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-14 text-slate-400">
            <div className="mb-4 flex size-16 items-center justify-center rounded-full bg-primary/5">
              <span className="material-symbols-outlined text-3xl text-primary/40">tune</span>
            </div>
            <p className="text-sm font-semibold text-slate-600">No attributes yet</p>
            <p className="mt-1 text-xs text-slate-400">Click "Add Attribute" to define category-specific filters.</p>
          </div>
        ) : (
          attributes.map((attribute, attrIndex) => {
            const isOpen = openIndexes.has(attrIndex)
            const optionCount = attribute.options.length

            return (
              <AttributeRow
                key={`${attribute.id}-${attrIndex}`}
                attribute={attribute}
                attrIndex={attrIndex}
                isOpen={isOpen}
                onAddOption={addOption}
                onAttrDragOver={handleAttrDragOver}
                onDragEnd={() => setDraggingAttr(null)}
                onDragStart={() => setDraggingAttr(attrIndex)}
                onOptionDragEnd={() => setDraggingOption(null)}
                onOptionDragOver={handleOptionDragOver}
                onOptionDragStart={(optionIndex) => setDraggingOption({ attrIndex, optionIndex })}
                onPatch={patchAttribute}
                onRemove={removeAttribute}
                onRemoveOption={removeOption}
                onToggle={toggleOpen}
                optionCount={optionCount}
              />
            )
          })
        )}
      </div>
    </div>
  )
}

function AttributeRow({
  attribute,
  attrIndex,
  isOpen,
  onAddOption,
  onAttrDragOver,
  onDragEnd,
  onDragStart,
  onOptionDragEnd,
  onOptionDragOver,
  onOptionDragStart,
  onPatch,
  onRemove,
  onRemoveOption,
  onToggle,
  optionCount,
}: {
  attribute: AdminCategoryAttributeDto
  attrIndex: number
  isOpen: boolean
  onAddOption: (attrIndex: number, value: string) => void
  onAttrDragOver: (event: DragEvent<HTMLDivElement>, targetIndex: number) => void
  onDragEnd: () => void
  onDragStart: () => void
  onOptionDragEnd: () => void
  onOptionDragOver: (event: DragEvent<HTMLDivElement>, attrIndex: number, targetIndex: number) => void
  onOptionDragStart: (optionIndex: number) => void
  onPatch: (index: number, patch: Partial<AdminCategoryAttributeDto>) => void
  onRemove: (index: number) => void
  onRemoveOption: (attrIndex: number, optionIndex: number) => void
  onToggle: (index: number, forceOpen?: boolean) => void
  optionCount: number
}) {
  const [newOption, setNewOption] = useState('')

  const submitOption = () => {
    onAddOption(attrIndex, newOption)
    setNewOption('')
  }

  return (
    <div draggable onDragStart={onDragStart} onDragEnd={onDragEnd} onDragOver={(event) => onAttrDragOver(event, attrIndex)}>
      <div className="group/row flex cursor-pointer items-center gap-3 px-5 py-3.5 transition-colors hover:bg-slate-50/80" onClick={() => onToggle(attrIndex)}>
        <span className="drag-handle shrink-0 cursor-grab text-slate-300 group-hover/row:text-slate-400 active:cursor-grabbing">
          <span className="material-symbols-outlined text-[20px]">drag_indicator</span>
        </span>
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-sm font-bold text-slate-800">{attribute.name || 'New Attribute'}</span>
            <span className="rounded-full bg-primary/10 px-2 py-0.5 text-[11px] font-semibold text-primary/80">{attribute.filterType}</span>
            {optionCount > 0 && (
              <span className="rounded-full bg-slate-100 px-2 py-0.5 text-[11px] font-medium text-slate-600">
                {optionCount} option{optionCount > 1 ? 's' : ''}
              </span>
            )}
          </div>
        </div>
        <div className="flex shrink-0 items-center gap-1">
          <span
            className={`inline-flex items-center gap-1 rounded-full border px-2.5 py-1 text-xs font-medium ${
              attribute.isFilterable ? 'border-green-100 bg-green-50 text-green-700' : 'border-slate-200 bg-slate-100 text-slate-500'
            }`}
          >
            <span className="material-symbols-outlined text-[12px]">{attribute.isFilterable ? 'check_circle' : 'remove_circle'}</span>
            {attribute.isFilterable ? 'Filterable' : 'No Filter'}
          </span>
          <button
            type="button"
            onClick={(event) => {
              event.stopPropagation()
              onToggle(attrIndex)
            }}
            className="ml-1 rounded-lg p-1.5 text-slate-400 transition-colors hover:bg-primary/5 hover:text-primary"
          >
            <span className={`material-symbols-outlined text-[20px] transition-transform ${isOpen ? 'rotate-180' : ''}`}>expand_more</span>
          </button>
          <button
            type="button"
            onClick={(event) => {
              event.stopPropagation()
              onRemove(attrIndex)
            }}
            className="rounded-lg p-1.5 text-slate-400 transition-colors hover:bg-red-50 hover:text-red-500"
          >
            <span className="material-symbols-outlined text-[20px]">delete</span>
          </button>
        </div>
      </div>

      {isOpen && (
        <div className="space-y-4 border-t border-primary/10 bg-gradient-to-b from-slate-50/80 to-white px-5 py-5">
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <label className="space-y-1.5">
              <span className="block text-xs font-bold uppercase tracking-wider text-slate-500">
                Attribute Name <span className="text-red-400">*</span>
              </span>
              <input
                value={attribute.name}
                onChange={(event) => onPatch(attrIndex, { name: event.target.value })}
                className="w-full rounded-xl border border-slate-200 bg-white px-3.5 py-2.5 text-sm outline-none transition-all placeholder:text-slate-300 focus:border-primary focus:ring-2 focus:ring-primary/15"
                placeholder="e.g. GPU Architecture"
                type="text"
              />
            </label>
            <label className="space-y-1.5">
              <span className="block text-xs font-bold uppercase tracking-wider text-slate-500">Input Type</span>
              <div className="relative">
                <select
                  value={attribute.filterType}
                  onChange={(event) => onPatch(attrIndex, { filterType: event.target.value })}
                  className="w-full cursor-pointer appearance-none rounded-xl border border-slate-200 bg-white py-2.5 pl-3.5 pr-8 text-sm outline-none transition-all focus:border-primary focus:ring-2 focus:ring-primary/15"
                >
                  <option value="Checkbox">Checkbox</option>
                  <option value="Radio">Radio</option>
                  <option value="Select">Select</option>
                  <option value="Text">Text</option>
                  <option value="NumberRange">Number Range</option>
                </select>
                <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-slate-400">
                  <span className="material-symbols-outlined text-[18px]">expand_more</span>
                </div>
              </div>
            </label>
          </div>

          <label className="inline-flex cursor-pointer items-center gap-3 rounded-xl border border-slate-200 bg-white px-4 py-3 transition-colors hover:border-primary/30">
            <span className="relative inline-flex">
              <input
                checked={attribute.isFilterable}
                onChange={(event) => onPatch(attrIndex, { isFilterable: event.target.checked })}
                type="checkbox"
                className="peer sr-only"
              />
              <span className="h-5 w-10 rounded-full bg-slate-200 transition peer-checked:bg-primary" />
              <span className="absolute left-0.5 top-0.5 size-4 rounded-full bg-white shadow-sm transition peer-checked:translate-x-full" />
            </span>
            <span className="text-sm font-semibold text-slate-700">Use as storefront filter</span>
            <span className="ml-auto text-xs text-slate-400">Enables product filtering on this attribute</span>
          </label>

          <div className="space-y-3">
            <div className="flex items-center justify-between">
              <label className="flex items-center gap-1.5 text-xs font-bold uppercase tracking-wider text-slate-500">
                <span className="material-symbols-outlined text-[14px]">list</span>
                Options <span className="font-extrabold text-primary">{optionCount || ''}</span>
              </label>
              <span className="flex items-center gap-1 text-xs text-slate-400">
                <span className="material-symbols-outlined text-[13px]">drag_indicator</span> Drag to reorder
              </span>
            </div>
            <div className="flex min-h-[40px] flex-wrap gap-2 rounded-xl border border-dashed border-slate-200 bg-white p-2">
              {attribute.options.map((option, optionIndex) => (
                <div
                  key={`${option.value}-${optionIndex}`}
                  draggable
                  onDragStart={() => onOptionDragStart(optionIndex)}
                  onDragEnd={onOptionDragEnd}
                  onDragOver={(event) => onOptionDragOver(event, attrIndex, optionIndex)}
                  className="inline-flex cursor-grab select-none items-center gap-1.5 rounded-full border border-primary/20 bg-primary/10 py-1 pl-2.5 pr-1 text-xs font-semibold text-primary shadow-sm active:cursor-grabbing"
                >
                  <span className="material-symbols-outlined text-[11px] text-primary/40">drag_indicator</span>
                  <span>{option.value}</span>
                  <button
                    type="button"
                    onClick={() => onRemoveOption(attrIndex, optionIndex)}
                    className="ml-0.5 rounded-full p-0.5 text-primary/40 transition-colors hover:bg-red-50 hover:text-red-500"
                  >
                    <span className="material-symbols-outlined text-[14px]">close</span>
                  </button>
                </div>
              ))}
            </div>
            <div className="flex gap-2">
              <input
                value={newOption}
                onChange={(event) => setNewOption(event.target.value)}
                onKeyDown={(event) => {
                  if (event.key === 'Enter') {
                    event.preventDefault()
                    submitOption()
                  }
                }}
                className="min-w-0 flex-1 rounded-xl border border-slate-200 bg-white px-3.5 py-2.5 text-sm outline-none transition-all placeholder:text-slate-300 focus:border-primary focus:ring-2 focus:ring-primary/15"
                placeholder="Type option value and press Enter..."
                type="text"
              />
              <button
                type="button"
                onClick={submitOption}
                className="flex items-center gap-1 rounded-xl bg-primary px-4 py-2.5 text-sm font-bold text-white shadow-sm shadow-primary/20 transition-all hover:bg-blue-700"
              >
                <span className="material-symbols-outlined text-[16px]">add</span>
                Add
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

function CategoryForm({ mode }: { mode: 'create' | 'edit' }) {
  const navigate = useNavigate()
  const { id } = useParams()
  const categoryId = Number(id)
  const [form, setForm] = useState<CategoryFormState>(emptyForm)
  const [attributes, setAttributes] = useState<AdminCategoryAttributeDto[]>([])
  const [categories, setCategories] = useState<AdminCategoryDto[]>([])
  const [loading, setLoading] = useState(mode === 'edit')
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  const allOptions = useMemo(() => categoryOptions(categories).filter(({ item }) => item.id !== form.id), [categories, form.id])
  const selectedParent = allOptions.find(({ item }) => item.id === form.parentId)?.item

  useEffect(() => {
    const loadForm = async () => {
      setLoading(true)
      setError('')

      try {
        const listPromise = adminApi.categories.list()

        if (mode === 'edit') {
          const [list, category, attrs] = await Promise.all([
            listPromise,
            adminApi.categories.get(categoryId),
            adminApi.categories.attributes(categoryId),
          ])
          setCategories(list)
          setForm(formFromCategory(category))
          setAttributes(attrs)
        } else {
          const list = await listPromise
          setCategories(list)
          setForm(emptyForm)
          setAttributes([])
        }
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unable to load category data.')
      } finally {
        setLoading(false)
      }
    }

    void loadForm()
  }, [mode, categoryId])

  const patchForm = (patch: Partial<CategoryFormState>) => {
    setForm((current) => ({ ...current, ...patch }))
  }

  const normalizeAttributes = (categoryIdForSave: number) =>
    attributes
      .map((attribute, index) => ({
        ...attribute,
        categoryId: categoryIdForSave,
        displayOrder: index,
        id: 0,
        name: attribute.name.trim(),
        options: attribute.options
          .filter((option) => option.value.trim())
          .map((option, optionIndex) => ({
            ...option,
            displayOrder: optionIndex,
            id: 0,
            value: option.value.trim(),
          })),
      }))
      .filter((attribute) => attribute.name)

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSaving(true)
    setError('')

    try {
      let savedId = form.id ?? 0

      if (mode === 'create') {
        const request: CreateAdminCategoryRequest = {
          isActive: form.isActive,
          isDeleted: false,
          name: form.name.trim(),
          parentId: form.parentId === '' ? null : form.parentId,
          slug: form.slug.trim(),
        }
        const created = await adminApi.categories.create(request)
        savedId = created.id
      } else {
        const request: EditAdminCategoryRequest = {
          id: savedId,
          isActive: form.isActive,
          name: form.name.trim(),
          parentId: form.parentId === '' ? null : form.parentId,
          slug: form.slug.trim(),
        }
        await adminApi.categories.update(request)
      }

      await adminApi.categories.saveAttributes(savedId, {
        attributes: normalizeAttributes(savedId),
        categoryId: savedId,
      })

      navigate('/admin/categories', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to save category.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="flex min-h-[420px] items-center justify-center rounded-xl border border-slate-100 bg-white text-slate-500 shadow-sm">
        <span className="material-symbols-outlined mr-2 animate-spin text-[20px]">progress_activity</span>
        Loading category...
      </div>
    )
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col items-start gap-6 lg:flex-row">
      <div className="flex w-full flex-1 flex-col gap-6">
        <div className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <div className={`h-1 bg-gradient-to-r ${mode === 'edit' ? 'from-amber-400 to-amber-500' : 'from-primary to-blue-700'}`} />
          <div className="flex items-center gap-3 border-b border-slate-100 px-6 py-4">
            <div className={`rounded-lg p-2 ${mode === 'edit' ? 'bg-amber-50' : 'bg-primary/10'}`}>
              <span className={`material-symbols-outlined text-[20px] ${mode === 'edit' ? 'text-amber-500' : 'text-primary'}`}>
                {mode === 'edit' ? 'edit' : 'category'}
              </span>
            </div>
            <div>
              <h2 className="text-base font-semibold text-slate-800">{mode === 'edit' ? 'Edit Category Information' : 'Basic Information'}</h2>
              <p className="mt-0.5 text-xs text-slate-400">{mode === 'edit' ? `ID: #${form.id}` : 'Fill in the core details for the new category.'}</p>
            </div>
          </div>

          <div className="space-y-5 p-6">
            {error && <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-600">{error}</div>}

            <label className="block space-y-1.5">
              <span className="block text-sm font-semibold text-slate-700">
                Category Name <span className="text-red-500">*</span>
              </span>
              <div className="group relative">
                <span className="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-400 transition-colors group-focus-within:text-primary">
                  <span className="material-symbols-outlined text-xl">category</span>
                </span>
                <input
                  required
                  value={form.name}
                  onChange={(event) => {
                    const name = event.target.value
                    patchForm({ name, slug: toSlug(name) })
                  }}
                  className="w-full rounded-xl border border-slate-200 bg-slate-50/50 py-2.5 pl-11 pr-4 text-sm outline-none transition-all focus:border-primary focus:ring-4 focus:ring-primary/10"
                  placeholder="e.g. Graphics Card (GPU)"
                  type="text"
                />
              </div>
              {mode === 'create' && <p className="text-xs text-slate-400">This is how the category will appear on the storefront.</p>}
            </label>

            <label className="block space-y-1.5">
              <span className="flex items-center justify-between">
                <span className="block text-sm font-semibold text-slate-700">
                  Slug <span className="text-red-500">*</span>
                </span>
                <button
                  type="button"
                  onClick={() => patchForm({ slug: toSlug(form.name) })}
                  className="flex items-center gap-1 text-xs font-medium text-primary transition-colors hover:text-blue-700"
                >
                  <span className="material-symbols-outlined text-[14px]">auto_fix</span>
                  Auto-generate
                </button>
              </span>
              <div className="group relative">
                <span className="absolute inset-y-0 left-0 flex items-center pl-3.5 text-slate-400 transition-colors group-focus-within:text-primary">
                  <span className="material-symbols-outlined text-xl">link</span>
                </span>
                <input
                  required
                  value={form.slug}
                  onChange={(event) => patchForm({ slug: event.target.value })}
                  className="w-full rounded-xl border border-slate-200 bg-slate-50/50 py-2.5 pl-11 pr-4 font-mono text-sm outline-none transition-all focus:border-primary focus:ring-4 focus:ring-primary/10"
                  placeholder="e.g. card-do-hoa-gpu"
                  type="text"
                />
              </div>
              <div className="flex items-center gap-2 rounded-lg border border-slate-100 bg-slate-50 p-2.5">
                <span className="material-symbols-outlined text-[16px] text-slate-400">link</span>
                <span className="font-mono text-xs text-slate-500">
                  gearzone.vn/category/<span className="font-semibold text-slate-700">{form.slug || '...'}</span>
                </span>
              </div>
              {mode === 'edit' && (
                <div className="flex items-start gap-2 rounded-lg border border-amber-100 bg-amber-50 p-3">
                  <span className="material-symbols-outlined mt-0.5 shrink-0 text-[16px] text-amber-500">warning</span>
                  <p className="text-xs text-amber-700">Changing the slug may break existing links to this category page.</p>
                </div>
              )}
            </label>

            <label className="block space-y-1.5">
              <span className="block text-sm font-semibold text-slate-700">Parent Category</span>
              <div className="group relative">
                <span className="absolute inset-y-0 left-0 z-10 flex items-center pl-3.5 text-slate-400 transition-colors group-focus-within:text-primary">
                  <span className="material-symbols-outlined text-xl">account_tree</span>
                </span>
                <select
                  value={form.parentId}
                  onChange={(event) => patchForm({ parentId: event.target.value === '' ? '' : Number(event.target.value) })}
                  className="w-full cursor-pointer appearance-none rounded-xl border border-slate-200 bg-slate-50/50 py-2.5 pl-11 pr-10 text-sm outline-none transition-all focus:border-primary focus:ring-4 focus:ring-primary/10"
                >
                  <option value="">None (Root Category)</option>
                  {allOptions.map(({ depth, item }) => (
                    <option key={item.id} value={item.id}>
                      {'- '.repeat(depth)}
                      {item.name}
                    </option>
                  ))}
                </select>
                <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-3 text-slate-400">
                  <span className="material-symbols-outlined text-[20px]">expand_more</span>
                </div>
              </div>
            </label>
          </div>
        </div>

        <AttributeEditor attributes={attributes} onChange={setAttributes} />
      </div>

      <div className="flex w-full flex-col gap-6 lg:sticky lg:top-0 lg:w-80">
        <div className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <div className="h-1 bg-gradient-to-r from-green-400 to-emerald-500" />
          <div className="p-5">
            <h2 className="mb-4 flex items-center gap-2 text-sm font-bold uppercase tracking-wider text-slate-700">
              <span className="material-symbols-outlined text-[16px] text-slate-400">visibility</span>
              Status &amp; Visibility
            </h2>
            <div className="space-y-2.5">
              <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-slate-200 p-3.5 transition-all has-[:checked]:border-green-300 has-[:checked]:bg-green-50/70 hover:border-slate-300">
                <input
                  checked={form.isActive}
                  onChange={() => patchForm({ isActive: true })}
                  type="radio"
                  className="size-4 cursor-pointer border-slate-300 text-green-500 focus:ring-green-400"
                />
                <span className="flex items-center gap-2.5">
                  <span className="size-2 rounded-full bg-green-500 ring-2 ring-green-500/30" />
                  <span className="text-sm font-semibold text-slate-800">Active</span>
                </span>
                <span className="ml-auto rounded-full border border-green-100 bg-green-50 px-2 py-0.5 text-xs font-medium text-green-600">Visible</span>
              </label>
              <label className="flex cursor-pointer items-center gap-3 rounded-xl border border-slate-200 p-3.5 transition-all has-[:checked]:border-slate-300 has-[:checked]:bg-slate-50 hover:border-slate-300">
                <input
                  checked={!form.isActive}
                  onChange={() => patchForm({ isActive: false })}
                  type="radio"
                  className="size-4 cursor-pointer border-slate-300 text-slate-400 focus:ring-slate-300"
                />
                <span className="flex items-center gap-2.5">
                  <span className="size-2 rounded-full bg-slate-400 ring-2 ring-slate-400/30" />
                  <span className="text-sm font-semibold text-slate-800">Inactive</span>
                </span>
                <span className="ml-auto rounded-full border border-slate-200 bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-500">Hidden</span>
              </label>
            </div>

            <div className="mt-4 border-t border-slate-100 pt-4">
              <p className="mb-2 text-[10px] font-bold uppercase tracking-widest text-slate-400">{mode === 'edit' ? 'Category Meta' : 'Category Path'}</p>
              {mode === 'edit' ? (
                <div className="flex items-center justify-between rounded-lg border border-slate-100 bg-slate-50 px-3 py-2">
                  <span className="flex items-center gap-1.5 text-xs text-slate-500">
                    <span className="material-symbols-outlined text-[14px] text-slate-400">tag</span> ID
                  </span>
                  <span className="font-mono text-sm font-bold text-primary">#{form.id}</span>
                </div>
              ) : (
                <div className="flex flex-wrap items-center gap-1 rounded-lg border border-slate-100 bg-slate-50 px-3 py-2">
                  <span className="material-symbols-outlined text-[14px] text-slate-400">home</span>
                  <span className="material-symbols-outlined text-[12px] text-slate-300">chevron_right</span>
                  {selectedParent && (
                    <>
                      <span className="text-xs font-medium text-slate-600">{selectedParent.name}</span>
                      <span className="material-symbols-outlined text-[12px] text-slate-300">chevron_right</span>
                    </>
                  )}
                  <span className="text-xs font-bold text-primary">{form.name || '[New Category]'}</span>
                </div>
              )}
            </div>
          </div>
        </div>

        {mode === 'create' && (
          <div className="rounded-xl border border-primary/15 bg-gradient-to-br from-primary/5 to-primary/10 p-5">
            <h3 className="mb-3 flex items-center gap-2 text-sm font-bold text-primary">
              <span className="material-symbols-outlined text-[18px]">lightbulb</span>
              Quick Tips
            </h3>
            <ul className="space-y-3 text-sm text-slate-600">
              <li className="flex items-start gap-2.5">
                <span className="mt-0.5 flex size-5 shrink-0 items-center justify-center rounded-full bg-primary/15">
                  <span className="material-symbols-outlined text-[12px] text-primary">tune</span>
                </span>
                <span>Attributes define product filters like "Color", "RAM", or "GPU".</span>
              </li>
              <li className="flex items-start gap-2.5">
                <span className="mt-0.5 flex size-5 shrink-0 items-center justify-center rounded-full bg-primary/15">
                  <span className="material-symbols-outlined text-[12px] text-primary">link_off</span>
                </span>
                <span>Changing the slug later may break existing product links.</span>
              </li>
            </ul>
          </div>
        )}

        <div className="flex flex-col gap-2.5">
          <button
            type="submit"
            disabled={saving}
            className="flex w-full items-center justify-center gap-2 rounded-xl bg-primary px-5 py-3.5 font-bold text-white shadow-lg shadow-primary/30 transition-all hover:-translate-y-0.5 hover:bg-blue-700 hover:shadow-xl active:translate-y-0 disabled:translate-y-0 disabled:opacity-60"
          >
            <span className="material-symbols-outlined text-[20px]">save</span>
            {saving ? 'Saving...' : mode === 'edit' ? 'Save Changes' : 'Create Category'}
          </button>
          <Link
            to="/admin/categories"
            className="flex w-full items-center justify-center gap-2 rounded-xl border border-slate-200 bg-white px-5 py-3 text-sm font-semibold text-slate-600 transition-all hover:border-slate-300 hover:bg-slate-50 hover:text-slate-800"
          >
            <span className="material-symbols-outlined text-[18px]">arrow_back</span>
            {mode === 'edit' ? 'Back to List' : 'Go Back'}
          </Link>
        </div>
      </div>
    </form>
  )
}

export default function AdminCategoriesPage() {
  const location = useLocation()
  const mode: CategoryMode = location.pathname.endsWith('/create') ? 'create' : location.pathname.endsWith('/edit') ? 'edit' : 'list'
  const title = mode === 'create' ? 'Add New Category' : mode === 'edit' ? 'Edit Category' : 'Category Management'
  const breadcrumb =
    mode === 'create' ? ['Category Management', 'Add New'] : mode === 'edit' ? ['Category Management', 'Edit'] : ['Dashboard', 'Category Management']

  return (
    <AdminLayout activePage="Categories" breadcrumb={breadcrumb} pageHeader={title}>
      {mode === 'list' ? <CategoryList /> : <CategoryForm mode={mode} />}
    </AdminLayout>
  )
}
