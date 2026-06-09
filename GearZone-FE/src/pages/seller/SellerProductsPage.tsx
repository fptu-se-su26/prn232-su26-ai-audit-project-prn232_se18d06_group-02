import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface Product {
  id: string
  name: string
  basePrice: number
  imageUrl?: string
  status: string
  isActive: boolean
  stockQuantity: number
  categoryName?: string
  brandName?: string
}

const STATUS_BADGE: Record<string, string> = {
  Active: 'bg-emerald-100 text-emerald-700',
  Approved: 'bg-emerald-100 text-emerald-700',
  Pending: 'bg-amber-100 text-amber-700',
  Draft: 'bg-slate-100 text-slate-500',
  Inactive: 'bg-slate-100 text-slate-500',
  Rejected: 'bg-red-100 text-red-700',
}

const STATUS_TABS = ['All', 'Active', 'Draft', 'Pending', 'Inactive', 'Rejected']

interface ProductForm {
  name: string
  basePrice: string
  stockQuantity: string
  description: string
}

const EMPTY_FORM: ProductForm = { name: '', basePrice: '', stockQuantity: '', description: '' }

function formatPrice(v: number) {
  return new Intl.NumberFormat('vi-VN').format(v ?? 0)
}

export default function SellerProductsPage() {
  const [products, setProducts] = useState<Product[]>([])
  const [loading, setLoading] = useState(true)
  const [tab, setTab] = useState('All')
  const [search, setSearch] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [editId, setEditId] = useState<string | null>(null)
  const [form, setForm] = useState<ProductForm>(EMPTY_FORM)
  const [saving, setSaving] = useState(false)
  const [formError, setFormError] = useState('')

  const fetchProducts = () => {
    setLoading(true)
    sellerApi.products
      .list()
      .then((d) =>
        setProducts((d as { items?: Product[] }).items ?? (d as Product[]) ?? []),
      )
      .finally(() => setLoading(false))
  }

  useEffect(() => { fetchProducts() }, [])

  const openCreate = () => {
    setEditId(null)
    setForm(EMPTY_FORM)
    setFormError('')
    setShowForm(true)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const openEdit = (p: Product) => {
    setEditId(p.id)
    setForm({
      name: p.name,
      basePrice: String(p.basePrice),
      stockQuantity: String(p.stockQuantity),
      description: '',
    })
    setFormError('')
    setShowForm(true)
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this product? This cannot be undone.')) return
    await sellerApi.products.delete(id)
    fetchProducts()
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setSaving(true)
    setFormError('')
    try {
      const payload = {
        name: form.name,
        basePrice: Number(form.basePrice),
        stockQuantity: Number(form.stockQuantity),
        description: form.description,
      }
      if (editId) await sellerApi.products.update(editId, payload)
      else await sellerApi.products.create(payload)
      setShowForm(false)
      setEditId(null)
      setForm(EMPTY_FORM)
      fetchProducts()
    } catch (err: unknown) {
      setFormError(err instanceof Error ? err.message : 'Failed to save product.')
    } finally {
      setSaving(false)
    }
  }

  const tabCounts = STATUS_TABS.reduce<Record<string, number>>((acc, t) => {
    acc[t] =
      t === 'All'
        ? products.length
        : products.filter(
            (p) => p.status === t || (t === 'Active' && p.isActive && p.status === 'Approved'),
          ).length
    return acc
  }, {})

  const filtered = products.filter((p) => {
    const matchTab =
      tab === 'All' ||
      p.status === tab ||
      (tab === 'Active' && p.isActive && p.status === 'Approved')
    const matchSearch = !search || p.name.toLowerCase().includes(search.toLowerCase())
    return matchTab && matchSearch
  })

  const stats = [
    { label: 'Total', value: products.length, color: 'text-blue-600', bg: 'bg-blue-50', icon: 'inventory_2' },
    { label: 'Active', value: products.filter((p) => p.isActive).length, color: 'text-emerald-600', bg: 'bg-emerald-50', icon: 'check_circle' },
    { label: 'Out of Stock', value: products.filter((p) => p.stockQuantity === 0).length, color: 'text-red-600', bg: 'bg-red-50', icon: 'remove_shopping_cart' },
    { label: 'Draft / Pending', value: products.filter((p) => p.status === 'Draft' || p.status === 'Pending').length, color: 'text-amber-600', bg: 'bg-amber-50', icon: 'pending' },
  ]

  return (
    <SellerLayout pageHeader="Products" breadcrumb={['Products']}>
      {/* Add/Edit form */}
      {showForm && (
        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          <h3 className="mb-4 text-base font-semibold text-slate-800">
            {editId ? 'Edit Product' : 'Add New Product'}
          </h3>
          <form className="grid max-w-lg grid-cols-1 gap-4" onSubmit={handleSubmit}>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700">Product Name</label>
              <input
                className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
                placeholder="Enter product name"
                required
                value={form.name}
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">Base Price (₫)</label>
                <input
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  min="0"
                  onChange={(e) => setForm((f) => ({ ...f, basePrice: e.target.value }))}
                  placeholder="0"
                  required
                  type="number"
                  value={form.basePrice}
                />
              </div>
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">Stock Quantity</label>
                <input
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  min="0"
                  onChange={(e) => setForm((f) => ({ ...f, stockQuantity: e.target.value }))}
                  placeholder="0"
                  required
                  type="number"
                  value={form.stockQuantity}
                />
              </div>
            </div>
            <div>
              <label className="mb-1.5 block text-sm font-medium text-slate-700">Description</label>
              <textarea
                className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                placeholder="Optional product description"
                rows={3}
                value={form.description}
              />
            </div>
            {formError && <p className="text-sm text-red-600">{formError}</p>}
            <div className="flex gap-3">
              <button
                className="flex-1 rounded-lg bg-[#ff6b00] py-2 text-sm font-semibold text-white transition hover:bg-[#ea580c] disabled:opacity-60"
                disabled={saving}
                type="submit"
              >
                {saving ? 'Saving…' : editId ? 'Update Product' : 'Create Product'}
              </button>
              <button
                className="rounded-lg border border-slate-200 px-6 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-50"
                onClick={() => setShowForm(false)}
                type="button"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {loading ? (
        <div className="space-y-2">
          {Array.from({ length: 6 }).map((_, i) => (
            <div className="h-14 animate-pulse rounded-xl bg-white" key={i} />
          ))}
        </div>
      ) : (
        <>
          {/* Stats */}
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            {stats.map((s) => (
              <div key={s.label} className="flex items-center gap-3 rounded-xl border border-slate-100 bg-white p-4 shadow-sm">
                <div className={`flex h-10 w-10 items-center justify-center rounded-lg ${s.bg}`}>
                  <span className={`material-symbols-outlined text-xl ${s.color}`}>{s.icon}</span>
                </div>
                <div>
                  <p className="text-xs text-slate-500">{s.label}</p>
                  <p className={`text-xl font-bold ${s.color}`}>{s.value}</p>
                </div>
              </div>
            ))}
          </div>

          {/* Toolbar */}
          <div className="flex flex-wrap items-center justify-between gap-3">
            <input
              className="w-full max-w-xs rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search products…"
              value={search}
            />
            <button
              className="inline-flex items-center gap-2 rounded-lg bg-[#ff6b00] px-5 py-2 text-sm font-semibold text-white transition hover:bg-[#ea580c]"
              onClick={openCreate}
              type="button"
            >
              <span className="material-symbols-outlined text-[18px]">add</span>
              Add Product
            </button>
          </div>

          {/* Status tabs */}
          <div className="flex flex-wrap gap-0 border-b border-slate-200 bg-white px-4 pt-2 rounded-t-xl shadow-sm">
            {STATUS_TABS.filter((t) => tabCounts[t] > 0 || t === 'All').map((t) => (
              <button
                className={`flex items-center gap-1.5 border-b-2 px-4 py-2.5 text-sm font-medium transition-colors ${
                  tab === t
                    ? 'border-[#ff6b00] text-[#ff6b00]'
                    : 'border-transparent text-slate-500 hover:text-slate-700'
                }`}
                key={t}
                onClick={() => setTab(t)}
                type="button"
              >
                {t}
                {tabCounts[t] > 0 && t !== 'All' && (
                  <span
                    className={`rounded-full px-1.5 py-0.5 text-[10px] font-bold ${
                      tab === t ? 'bg-[#ff6b00] text-white' : 'bg-slate-100 text-slate-500'
                    }`}
                  >
                    {tabCounts[t]}
                  </span>
                )}
              </button>
            ))}
          </div>

          {/* Table */}
          <div className="overflow-hidden rounded-b-xl border border-t-0 border-slate-200 bg-white shadow-sm">
            <table className="min-w-full divide-y divide-slate-100">
              <thead className="bg-slate-50">
                <tr>
                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Product
                  </th>
                  <th className="px-5 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Category
                  </th>
                  <th className="px-5 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Price
                  </th>
                  <th className="px-5 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Stock
                  </th>
                  <th className="px-5 py-3 text-center text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Status
                  </th>
                  <th className="px-5 py-3 text-center text-xs font-semibold uppercase tracking-wider text-slate-500">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-50">
                {filtered.map((p) => (
                  <tr className="hover:bg-slate-50" key={p.id}>
                    <td className="px-5 py-3">
                      <div className="flex items-center gap-3">
                        {p.imageUrl ? (
                          <img
                            alt={p.name}
                            className="h-10 w-10 flex-shrink-0 rounded-lg object-cover"
                            src={p.imageUrl}
                          />
                        ) : (
                          <div className="h-10 w-10 flex-shrink-0 rounded-lg bg-slate-100" />
                        )}
                        <div>
                          <p className="text-sm font-medium text-slate-800">{p.name}</p>
                          {p.brandName && (
                            <p className="text-xs text-slate-400">{p.brandName}</p>
                          )}
                        </div>
                      </div>
                    </td>
                    <td className="px-5 py-3 text-sm text-slate-500">{p.categoryName ?? '—'}</td>
                    <td className="px-5 py-3 text-right text-sm font-semibold text-slate-800">
                      {formatPrice(p.basePrice)} ₫
                    </td>
                    <td className="px-5 py-3 text-right">
                      <span
                        className={`text-sm font-semibold ${
                          p.stockQuantity === 0
                            ? 'text-red-600'
                            : p.stockQuantity < 5
                              ? 'text-amber-600'
                              : 'text-emerald-600'
                        }`}
                      >
                        {p.stockQuantity}
                      </span>
                    </td>
                    <td className="px-5 py-3 text-center">
                      <span
                        className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${STATUS_BADGE[p.status] ?? 'bg-slate-100 text-slate-600'}`}
                      >
                        {p.status}
                      </span>
                    </td>
                    <td className="px-5 py-3">
                      <div className="flex items-center justify-center gap-2">
                        <button
                          className="rounded-lg border border-slate-200 px-3 py-1 text-xs font-medium text-slate-600 transition hover:bg-slate-50"
                          onClick={() => openEdit(p)}
                          type="button"
                        >
                          Edit
                        </button>
                        <button
                          className="rounded-lg border border-red-200 px-3 py-1 text-xs font-medium text-red-600 transition hover:bg-red-50"
                          onClick={() => handleDelete(p.id)}
                          type="button"
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
                {filtered.length === 0 && (
                  <tr>
                    <td className="px-5 py-10 text-center text-slate-400" colSpan={6}>
                      No products found.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </>
      )}
    </SellerLayout>
  )
}
