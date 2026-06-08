/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { adminApi, type AdminProductDetailDto } from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

type ProductActionType = 'approve' | 'reject' | 'suspend' | 'delete'

interface PendingAction {
  icon: string
  reasonRequired: boolean
  tone: 'green' | 'red' | 'amber'
  type: ProductActionType
}

const currencyFormatter = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 })

function formatCurrency(value: number) {
  return `${currencyFormatter.format(value ?? 0)} VND`
}

function formatDate(value?: string | null, options?: Intl.DateTimeFormatOptions) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', options ?? { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value))
}

function statusConfig(status: string) {
  const normalized = status.toLowerCase()
  if (normalized === 'active' || normalized === 'approved') return { icon: 'check_circle', label: normalized === 'approved' ? 'Approved' : 'Active', tone: 'border-green-200 bg-green-50 text-green-700' }
  if (normalized === 'pending') return { icon: 'pending_actions', label: 'Pending Approval', tone: 'border-amber-200 bg-amber-50 text-amber-700' }
  if (normalized === 'inactive' || normalized === 'suspended') return { icon: 'block', label: 'Inactive', tone: 'border-slate-300 bg-slate-100 text-slate-700' }
  if (normalized === 'outofstock' || normalized === 'out of stock') return { icon: 'production_quantity_limits', label: 'Out of Stock', tone: 'border-red-200 bg-red-50 text-red-700' }
  if (normalized === 'rejected') return { icon: 'cancel', label: 'Rejected', tone: 'border-red-200 bg-red-50 text-red-700' }
  return { icon: 'inventory_2', label: status || 'Draft', tone: 'border-slate-200 bg-slate-50 text-slate-600' }
}

function actionCopy(action: ProductActionType) {
  if (action === 'approve') return { action: 'Approve', title: 'Approve Product?' }
  if (action === 'reject') return { action: 'Reject', title: 'Reject Product?' }
  if (action === 'suspend') return { action: 'Suspend', title: 'Suspend Product?' }
  return { action: 'Delete', title: 'Delete Product?' }
}

function LoadingDetail() {
  return (
    <div className="space-y-8">
      <div className="grid gap-8 lg:grid-cols-12">
        <div className="space-y-6 lg:col-span-8">
          <div className="h-[420px] animate-pulse rounded-xl bg-slate-200" />
          <div className="h-52 animate-pulse rounded-xl bg-slate-200" />
          <div className="h-64 animate-pulse rounded-xl bg-slate-200" />
        </div>
        <div className="space-y-6 lg:col-span-4">
          <div className="h-64 animate-pulse rounded-xl bg-slate-200" />
          <div className="h-72 animate-pulse rounded-xl bg-slate-200" />
          <div className="h-56 animate-pulse rounded-xl bg-slate-200" />
        </div>
      </div>
    </div>
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
  product,
}: {
  busy: boolean
  onClose: () => void
  onConfirm: (reason: string) => void
  pendingAction: PendingAction
  product: AdminProductDetailDto
}) {
  const [reason, setReason] = useState('')
  const copy = actionCopy(pendingAction.type)
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
          <h3 className="text-lg font-bold text-slate-900">{copy.title}</h3>
          <p className="mt-1 text-sm text-slate-500">{product.name}</p>
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

function InfoBadge({ children, icon }: { children: ReactNode; icon: string }) {
  return (
    <span className="flex items-center gap-1.5">
      <span className="material-symbols-outlined text-[16px]">{icon}</span>
      {children}
    </span>
  )
}

export default function AdminProductDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [product, setProduct] = useState<AdminProductDetailDto | null>(null)
  const [selectedImage, setSelectedImage] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null)
  const [actionBusy, setActionBusy] = useState(false)

  const loadProduct = async () => {
    if (!id) {
      setError('Product id is missing.')
      setLoading(false)
      return
    }

    setLoading(true)
    setError('')

    try {
      const data = await adminApi.products.get(id)
      setProduct(data)
      setSelectedImage(data.images?.[0] ?? '')
    } catch (err) {
      setProduct(null)
      setError(err instanceof Error ? err.message : 'Unable to load product.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadProduct()
  }, [id])

  const openAction = (type: ProductActionType) => {
    const reasonRequired = type === 'reject' || type === 'suspend' || type === 'delete'
    const tone = type === 'approve' ? 'green' : type === 'suspend' ? 'amber' : 'red'
    const icon = type === 'approve' ? 'check_circle' : type === 'suspend' ? 'block' : type === 'delete' ? 'delete' : 'cancel'
    setPendingAction({ icon, reasonRequired, tone, type })
  }

  const performAction = async (reason: string) => {
    if (!pendingAction || !product) return
    setActionBusy(true)
    try {
      if (pendingAction.type === 'approve') await adminApi.products.approve(product.id)
      if (pendingAction.type === 'reject') await adminApi.products.reject(product.id, { reason })
      if (pendingAction.type === 'suspend') await adminApi.products.suspend(product.id, { reason })
      if (pendingAction.type === 'delete') {
        await adminApi.products.delete(product.id, { reason })
        navigate('/admin/products')
        return
      }

      setPendingAction(null)
      await loadProduct()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Action failed.')
    } finally {
      setActionBusy(false)
    }
  }

  if (loading) {
    return (
      <AdminLayout activePage="Product" breadcrumb={['Dashboard', 'Product Management']} pageHeader="Product Details">
        <LoadingDetail />
      </AdminLayout>
    )
  }

  if (error || !product) {
    return (
      <AdminLayout activePage="Product" breadcrumb={['Dashboard', 'Product Management']} pageHeader="Product Details">
        <div className="rounded-xl border border-red-200 bg-red-50 p-6 text-sm font-medium text-red-700">
          {error || 'Product not found.'}
          <div className="mt-4">
            <Link to="/admin/products" className="font-bold underline">
              Back to Products
            </Link>
          </div>
        </div>
      </AdminLayout>
    )
  }

  const status = statusConfig(product.status)
  const isPending = product.status === 'Pending'
  const isActive = product.status === 'Active'
  const liveHref = isActive && product.slug ? `/product/${product.slug}` : `/admin/products/${product.id}`

  return (
    <AdminLayout activePage="Product" breadcrumb={['Dashboard', 'Product Management', product.name]} pageHeader="Product Details">
      <div className="relative flex w-full max-w-[1440px] flex-col gap-6 pb-28">
        {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}

        <div className="grid grid-cols-1 gap-8 lg:grid-cols-12">
          <div className="flex flex-col gap-6 lg:col-span-7 xl:col-span-8">
            <section className="flex flex-col rounded-xl border border-slate-100 bg-white p-6 shadow-sm">
              <div className="mb-6">
                {product.images?.length ? (
                  <>
                    <div className="relative mb-4 aspect-[16/9] w-full overflow-hidden rounded-xl border border-slate-100 bg-slate-50 shadow-sm lg:aspect-[21/9] xl:aspect-[16/9]">
                      <div className="h-full w-full bg-contain bg-center bg-no-repeat" style={{ backgroundImage: `url("${selectedImage}")` }} />
                      <div className="absolute inset-0 bg-black/0 transition-colors hover:bg-black/5" />
                    </div>
                    <div className="grid grid-cols-4 gap-3 sm:grid-cols-5 md:grid-cols-6">
                      {product.images.map((image) => (
                        <button
                          key={image}
                          type="button"
                          onClick={() => setSelectedImage(image)}
                          className={`aspect-square overflow-hidden rounded-lg border shadow-sm transition-colors hover:border-primary/50 ${image === selectedImage ? 'border-primary' : 'border-slate-200'}`}
                        >
                          <div className="h-full w-full bg-cover bg-center" style={{ backgroundImage: `url("${image}")` }} />
                        </button>
                      ))}
                      <div className="flex aspect-square items-center justify-center rounded-lg border border-slate-200 bg-slate-50 text-slate-400 shadow-sm">
                        <span className="material-symbols-outlined">add_photo_alternate</span>
                      </div>
                    </div>
                  </>
                ) : (
                  <div className="mb-6 flex aspect-[16/9] w-full flex-col items-center justify-center gap-2 rounded-xl border border-dashed border-slate-300 bg-slate-50 text-slate-400">
                    <span className="material-symbols-outlined text-4xl">image_not_supported</span>
                    <span>No images uploaded</span>
                  </div>
                )}
              </div>

              <div className="flex flex-col gap-2 pt-2">
                <div className="flex flex-wrap items-center gap-2">
                  <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-[10px] font-semibold uppercase tracking-wider text-slate-600">
                    {product.category?.name ?? 'N/A'}
                  </span>
                  <span className={`flex items-center gap-1 rounded-full border px-2.5 py-0.5 text-xs font-bold ${status.tone}`}>
                    <span className="material-symbols-outlined text-[14px]">{status.icon}</span>
                    {status.label}
                  </span>
                </div>

                <h1 className="text-2xl font-bold leading-tight text-slate-900 sm:text-3xl">{product.name}</h1>

                <div className="mt-2 flex flex-wrap items-center gap-x-6 gap-y-2 text-sm text-slate-500">
                  <InfoBadge icon="label">
                    <span className="font-medium text-slate-700">Brand:</span> {product.brand?.name ?? 'N/A'}
                  </InfoBadge>
                  <InfoBadge icon="qr_code_2">
                    <span className="font-medium text-slate-700">SKU:</span> <span className="font-mono">{product.sku}</span>
                  </InfoBadge>
                  <InfoBadge icon="storefront">
                    <span className="font-medium text-slate-700">Store:</span>
                    <Link to={`/admin/stores/${product.store?.id}`} className="flex items-center gap-0.5 font-bold text-primary hover:underline">
                      {product.store?.name ?? 'N/A'}
                      <span className="material-symbols-outlined text-[14px]">open_in_new</span>
                    </Link>
                  </InfoBadge>
                </div>
              </div>
            </section>

            <section className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
              <div className="flex items-center gap-2 border-b border-slate-100 bg-slate-50/50 px-6 py-4">
                <span className="material-symbols-outlined text-slate-400">tune</span>
                <h3 className="text-lg font-semibold text-slate-900">Technical Specifications</h3>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <tbody className="divide-y divide-slate-100">
                    {product.specs?.length ? (
                      product.specs.map((spec) => (
                        <tr key={spec.attributeName} className="hover:bg-slate-50">
                          <td className="w-1/3 bg-slate-50/30 px-6 py-3 align-top font-medium text-slate-600">{spec.attributeName}</td>
                          <td className="px-6 py-3 text-slate-900">
                            {spec.values.length === 1 ? (
                              <span>{spec.values[0]}</span>
                            ) : (
                              <div className="flex flex-wrap gap-1.5">
                                {spec.values.map((value) => (
                                  <span key={value} className="inline-flex items-center rounded-md border border-slate-200 bg-slate-100 px-2 py-0.5 text-xs font-medium text-slate-700">
                                    {value}
                                  </span>
                                ))}
                              </div>
                            )}
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={2} className="px-6 py-8 text-center text-slate-400">
                          <div className="flex flex-col items-center gap-2">
                            <span className="material-symbols-outlined text-3xl text-slate-300">rule</span>
                            <span className="text-sm">No category attribute specifications found for this product.</span>
                          </div>
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </section>

            <section className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
              <div className="flex items-center gap-2 border-b border-slate-100 bg-slate-50/50 px-6 py-4">
                <span className="material-symbols-outlined text-slate-400">style</span>
                <h3 className="text-lg font-semibold text-slate-900">Product Variants</h3>
              </div>
              <div className="overflow-x-auto">
                <table className="w-full text-left text-sm">
                  <thead className="bg-slate-50">
                    <tr>
                      <th className="px-6 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">SKU</th>
                      <th className="px-6 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">Variant Name</th>
                      <th className="px-6 py-3 text-xs font-semibold uppercase tracking-wider text-slate-600">Attributes</th>
                      <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-600">Price</th>
                      <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-600">Stock</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {product.variants?.length ? (
                      product.variants.map((variant) => (
                        <tr key={variant.sku} className="hover:bg-slate-50">
                          <td className="px-6 py-3 font-mono text-xs text-slate-500">{variant.sku}</td>
                          <td className="px-6 py-3 font-medium text-slate-900">{variant.name || (product.variants.length === 1 ? product.name : '-')}</td>
                          <td className="px-6 py-3">
                            <div className="flex flex-wrap gap-1.5">
                              {variant.attributes.map((attribute) => (
                                <span key={`${variant.sku}-${attribute.attributeName}-${attribute.value}`} className="inline-flex items-center gap-1.5 rounded-md border border-slate-200 bg-white px-2 py-1 text-xs font-medium tracking-wide text-slate-600 shadow-sm">
                                  <span className="text-slate-400">{attribute.attributeName}:</span>
                                  <span className="text-slate-900">{attribute.value}</span>
                                </span>
                              ))}
                            </div>
                          </td>
                          <td className="whitespace-nowrap px-6 py-3 text-right font-medium text-primary">{formatCurrency(variant.price)}</td>
                          <td className="px-6 py-3 text-right text-slate-900">
                            <span className={`rounded-md px-2 py-0.5 font-bold ${variant.stock > 0 ? 'bg-slate-100' : 'bg-red-50 text-red-600'}`}>{variant.stock}</span>
                          </td>
                        </tr>
                      ))
                    ) : (
                      <tr>
                        <td colSpan={5} className="px-6 py-4 text-center text-slate-500">
                          No variants. Product uses base configuration.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </section>
          </div>

          <aside className="flex flex-col gap-6 lg:col-span-5 xl:col-span-4">
            <section className="relative overflow-hidden rounded-xl border border-slate-100 bg-white p-6 shadow-sm">
              <div className="pointer-events-none absolute -right-4 -top-4 size-24 rounded-full bg-primary/5 blur-xl" />
              <div className="mb-4 flex items-center gap-2">
                <span className="material-symbols-outlined text-[20px] text-slate-400">payments</span>
                <h3 className="text-lg font-bold text-slate-900">Commercial Insights</h3>
              </div>

              <div className="mb-6 grid grid-cols-2 gap-4">
                <div className="rounded-xl border border-slate-100 bg-slate-50 p-4">
                  <label className="mb-1.5 flex items-center gap-1 text-[10px] font-bold uppercase tracking-widest text-slate-400">
                    <span className="material-symbols-outlined text-[14px]">sell</span>
                    Base Price
                  </label>
                  <div className="text-xl font-black text-slate-900">{formatCurrency(product.basePrice)}</div>
                </div>
                <div className="rounded-xl border border-blue-100 bg-blue-50/40 p-4">
                  <label className="mb-1.5 flex items-center gap-1 text-[10px] font-bold uppercase tracking-widest text-primary/70">
                    <span className="material-symbols-outlined text-[14px]">shopping_cart</span>
                    Sold
                  </label>
                  <div className="text-xl font-black text-primary">{product.soldCount}</div>
                </div>
              </div>

              <div className="mb-6 h-px w-full bg-slate-100" />

              <div className="grid grid-cols-2 gap-4">
                <div className="rounded-xl border border-slate-200 bg-white p-4 text-center shadow-sm">
                  <label className="mb-2 block text-[10px] font-bold uppercase tracking-widest text-slate-400">Total Inventory</label>
                  <div className="flex items-center justify-center gap-2">
                    <span className="text-2xl font-black text-slate-900">{product.stock}</span>
                    <span className={`material-symbols-outlined ${product.stock > 0 ? 'text-green-500' : 'text-red-500'}`}>
                      {product.stock > 0 ? 'check_circle' : 'error'}
                    </span>
                  </div>
                </div>
                <div className="relative overflow-hidden rounded-xl border border-blue-100 bg-blue-50 p-4 text-center shadow-sm">
                  <div className="absolute -bottom-2 -right-2 text-blue-100 opacity-50">
                    <span className="material-symbols-outlined text-6xl">account_balance_wallet</span>
                  </div>
                  <label className="relative z-10 mb-2 block text-[10px] font-bold uppercase tracking-widest text-primary/70">Platform Fee</label>
                  <div className="relative z-10 text-2xl font-black text-primary">{product.commissionRate}%</div>
                </div>
              </div>
            </section>

            <section className="flex flex-col rounded-xl border border-slate-100 bg-white p-6 shadow-sm">
              <div className="mb-4 flex items-center gap-2">
                <span className="material-symbols-outlined text-[20px] text-slate-400">description</span>
                <h3 className="text-lg font-bold text-slate-900">About Product</h3>
              </div>
              <div
                className="prose prose-slate max-h-[300px] max-w-none overflow-y-auto rounded-xl border border-slate-100 bg-slate-50/50 p-4 text-sm text-slate-600"
                dangerouslySetInnerHTML={{ __html: product.description || '<p>No description.</p>' }}
              />

              <div className="mt-4 flex flex-wrap items-center gap-2 border-t border-slate-100 pt-4 text-xs text-slate-400">
                <span className="material-symbols-outlined text-[16px]">history</span>
                <span>
                  Created: <strong>{formatDate(product.createdAt)}</strong>
                </span>
                <span className="mx-1">|</span>
                <span>
                  Updated: <strong>{formatDate(product.updatedAt) === 'N/A' ? 'Never' : formatDate(product.updatedAt)}</strong>
                </span>
              </div>
            </section>

            <section className="flex flex-col gap-4 rounded-xl border border-slate-100 bg-white p-5 shadow-sm">
              <div className="flex items-center gap-4">
                {product.store?.avatarUrl ? (
                  <img src={product.store.avatarUrl} alt={product.store.name} className="size-14 shrink-0 rounded-full border border-slate-200 bg-slate-100 object-cover shadow-sm" />
                ) : (
                  <div className="flex size-14 shrink-0 items-center justify-center rounded-full border border-slate-200 bg-slate-100 text-slate-300 shadow-sm">
                    <span className="material-symbols-outlined text-2xl">store</span>
                  </div>
                )}
                <div className="min-w-0 flex-1">
                  <h4 className="truncate text-base font-bold text-slate-900">{product.store?.name ?? 'N/A'}</h4>
                  <p className="mt-0.5 text-xs font-medium text-slate-500">{product.store?.vendorId ?? 'N/A'}</p>
                </div>
              </div>
              <div className="flex items-center justify-between rounded-lg border border-slate-100 bg-slate-50 p-3 text-xs">
                <span className="text-slate-500">Joined Date</span>
                <span className="font-bold text-slate-700">{formatDate(product.store?.joinedAt)}</span>
              </div>
              <Link to={`/admin/stores/${product.store?.id}`} className="flex w-full items-center justify-center gap-2 rounded-lg border border-slate-200 bg-slate-50 py-2 text-center text-sm font-bold text-slate-700 transition-colors hover:bg-slate-100">
                <span className="material-symbols-outlined text-[18px]">store</span>
                Visit Store Profile
              </Link>
            </section>
          </aside>
        </div>

        <div className="fixed bottom-0 left-0 right-0 z-40 border-t border-slate-200 bg-white/95 p-4 shadow-[0_-10px_15px_-3px_rgba(0,0,0,0.05)] backdrop-blur-sm">
          <div className="mx-auto flex max-w-[1440px] flex-col items-center justify-between gap-4 sm:flex-row">
            <div className="w-full sm:w-auto">
              <button type="button" onClick={() => openAction('delete')} className="flex w-full items-center justify-center gap-1.5 rounded-lg px-3 py-2 text-sm font-bold text-red-500 transition-colors hover:bg-red-50 hover:text-red-700 sm:w-auto">
                <span className="material-symbols-outlined text-[18px]">delete</span>
                Delete Product
              </button>
            </div>

            <div className="flex w-full flex-wrap items-center justify-end gap-3 sm:w-auto">
              <button type="button" onClick={() => openAction('suspend')} className="flex items-center gap-2 rounded-lg border border-slate-300 bg-white px-4 py-2.5 text-sm font-bold text-slate-700 shadow-sm transition-colors hover:bg-slate-50">
                <span className="material-symbols-outlined text-[18px]">visibility_off</span>
                Hide / Suspend
              </button>

              <button type="button" className="flex items-center gap-2 rounded-lg border border-primary/20 bg-white px-4 py-2.5 text-sm font-bold text-primary shadow-sm transition-colors hover:bg-primary/5">
                <span className="material-symbols-outlined text-[18px]">edit</span>
                Edit Spec
              </button>

              <div className="hidden h-8 w-px bg-slate-200 sm:block" />

              <a href={liveHref} target="_blank" rel="noreferrer" className={`flex items-center gap-2 rounded-lg px-4 py-2.5 text-sm font-bold shadow-sm transition-colors ${isActive ? 'bg-emerald-600 text-white hover:bg-emerald-700' : 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50'}`}>
                <span className="material-symbols-outlined text-[18px]">{isActive ? 'open_in_new' : 'visibility'}</span>
                {isActive ? 'View Live' : 'Live Preview'}
              </a>

              <div className="hidden h-8 w-px bg-slate-200 sm:block" />

              {isPending ? (
                <>
                  <button type="button" onClick={() => openAction('reject')} className="flex items-center gap-2 rounded-lg border border-red-200 px-4 py-2.5 text-sm font-bold text-red-600 shadow-sm transition-colors hover:bg-red-50">
                    <span className="material-symbols-outlined text-[18px]">close</span>
                    Reject Request
                  </button>
                  <button type="button" onClick={() => openAction('approve')} className="flex items-center gap-2 rounded-lg bg-green-600 px-6 py-2.5 text-sm font-bold text-white shadow-md transition-colors hover:bg-green-700">
                    <span className="material-symbols-outlined text-[18px]">check_circle</span>
                    Approve Request
                  </button>
                </>
              ) : (
                <button disabled title={`Product is currently ${product.status}`} className="flex cursor-not-allowed items-center gap-2 rounded-lg border border-slate-200 bg-slate-100 px-6 py-2.5 text-sm font-bold text-slate-400 shadow-inner">
                  <span className="material-symbols-outlined text-[18px]">gpp_good</span>
                  Approved
                </button>
              )}
            </div>
          </div>
        </div>

        {pendingAction && <ActionModal busy={actionBusy} onClose={() => setPendingAction(null)} onConfirm={(reason) => void performAction(reason)} pendingAction={pendingAction} product={product} />}
      </div>
    </AdminLayout>
  )
}
