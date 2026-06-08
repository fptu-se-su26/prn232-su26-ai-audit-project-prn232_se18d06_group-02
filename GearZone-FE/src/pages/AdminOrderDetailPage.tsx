/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import {
  adminApi,
  orderStatus,
  type AdminOrderDetailDto,
  type AdminOrderPaymentDto,
  type AdminSubOrderDto,
  type OrderStatus,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const currencyFormatter = new Intl.NumberFormat('vi-VN', { maximumFractionDigits: 0 })

function formatCurrency(value: number) {
  return `${currencyFormatter.format(value ?? 0)} ₫`
}

function formatDate(value?: string | null, options?: Intl.DateTimeFormatOptions) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', options ?? { day: '2-digit', month: 'long', year: 'numeric' }).format(new Date(value))
}

function formatTime(value?: string | null) {
  if (!value) return ''
  return new Intl.DateTimeFormat('en-US', { hour: '2-digit', minute: '2-digit' }).format(new Date(value))
}

function statusLabel(status: OrderStatus) {
  if (status === orderStatus.pending) return 'Pending'
  if (status === orderStatus.awaitingPayment) return 'Awaiting Payment'
  if (status === orderStatus.approved) return 'Approved'
  if (status === orderStatus.rejected) return 'Rejected'
  if (status === orderStatus.paid) return 'Paid'
  if (status === orderStatus.processing) return 'Processing'
  if (status === orderStatus.delivered) return 'Delivered'
  if (status === orderStatus.cancelled) return 'Cancelled'
  if (status === orderStatus.completed) return 'Completed'
  return 'Refunded'
}

function statusIcon(status: OrderStatus) {
  if (status === orderStatus.pending) return 'pending'
  if (status === orderStatus.awaitingPayment) return 'payments'
  if (status === orderStatus.approved) return 'verified'
  if (status === orderStatus.rejected) return 'block'
  if (status === orderStatus.paid) return 'paid'
  if (status === orderStatus.processing) return 'package_2'
  if (status === orderStatus.delivered) return 'local_shipping'
  if (status === orderStatus.cancelled) return 'do_not_disturb_on'
  if (status === orderStatus.completed) return 'task_alt'
  return 'keyboard_return'
}

function subOrderBadgeClasses(status: OrderStatus) {
  if (status === orderStatus.delivered || status === orderStatus.completed) return 'bg-green-100 text-green-700'
  if (status === orderStatus.cancelled || status === orderStatus.rejected || status === orderStatus.refunded) return 'bg-red-100 text-red-700'
  return 'bg-blue-100 text-blue-700'
}

function initials(value?: string | null) {
  return (value || '?').slice(0, 1).toUpperCase()
}

function LoadingDetail() {
  return (
    <div className="space-y-8">
      <div className="h-16 animate-pulse rounded-xl bg-slate-200" />
      <div className="grid gap-8 xl:grid-cols-3">
        <div className="space-y-8 xl:col-span-2">
          <div className="h-48 animate-pulse rounded-2xl bg-slate-200" />
          <div className="h-96 animate-pulse rounded-2xl bg-slate-200" />
        </div>
        <div className="space-y-8">
          <div className="h-80 animate-pulse rounded-3xl bg-slate-200" />
          <div className="h-64 animate-pulse rounded-3xl bg-slate-200" />
        </div>
      </div>
    </div>
  )
}

function StatusPill({ children, dotClass, tone }: { children: string; dotClass: string; tone: string }) {
  return (
    <span className={`flex items-center gap-1.5 rounded-full border px-3 py-1 text-xs font-bold shadow-sm ${tone}`}>
      <span className={`size-2 rounded-full ${dotClass}`} />
      {children}
    </span>
  )
}

function SubOrderCard({ subOrder }: { subOrder: AdminSubOrderDto }) {
  return (
    <section className="mb-6 overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm">
      <div className="flex flex-wrap items-center justify-between gap-4 border-b border-slate-50 bg-slate-50/30 p-6">
        <div className="flex items-center gap-3">
          <div className="flex size-10 items-center justify-center rounded-xl border border-slate-100 bg-white text-primary shadow-sm">
            <span className="material-symbols-outlined">storefront</span>
          </div>
          <div>
            <h3 className="font-bold text-slate-800">{subOrder.storeName}</h3>
            <p className="text-[10px] font-black uppercase tracking-widest text-slate-400">Store Shipment</p>
          </div>
        </div>
        <div className="flex items-center gap-4">
          <div className="hidden text-right sm:block">
            <p className="mb-1 text-[10px] font-bold uppercase leading-none tracking-widest text-slate-400">Sub-Order ID</p>
            <p className="font-mono text-xs font-bold text-slate-600">#{subOrder.id.slice(0, 8).toUpperCase()}</p>
          </div>
          <span className={`rounded-xl px-3 py-1.5 text-xs font-black uppercase tracking-wider shadow-sm ${subOrderBadgeClasses(subOrder.status)}`}>
            {statusLabel(subOrder.status)}
          </span>
        </div>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-left">
          <thead>
            <tr className="border-b border-slate-50 bg-slate-50/20">
              <th className="px-6 py-4 text-[11px] font-black uppercase tracking-widest text-slate-400">Product Details</th>
              <th className="px-6 py-4 text-right text-[11px] font-black uppercase tracking-widest text-slate-400">Unit Price</th>
              <th className="px-6 py-4 text-center text-[11px] font-black uppercase tracking-widest text-slate-400">Qty</th>
              <th className="px-6 py-4 text-right text-[11px] font-black uppercase tracking-widest text-slate-400">Amount</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-50">
            {subOrder.items.map((item) => (
              <tr key={item.id} className="transition-colors hover:bg-slate-50/50">
                <td className="px-6 py-5">
                  <div className="flex items-center gap-4">
                    {item.productImage ? (
                      <img alt="Thumbnail" className="size-14 rounded-xl border border-slate-100 bg-slate-100 object-cover shadow-sm" src={item.productImage} />
                    ) : (
                      <div className="flex size-14 items-center justify-center rounded-xl border border-slate-100 bg-slate-50 shadow-sm">
                        <span className="material-symbols-outlined text-slate-200">image</span>
                      </div>
                    )}
                    <div>
                      <p className="mb-1 text-sm font-bold leading-tight text-slate-900">{item.productName}</p>
                      <div className="inline-flex rounded-lg bg-slate-100 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wider text-slate-500">
                        {item.variantName}
                      </div>
                    </div>
                  </div>
                </td>
                <td className="px-6 py-5 text-right text-sm font-medium text-slate-600">{formatCurrency(item.unitPrice)}</td>
                <td className="bg-slate-50/30 px-6 py-5 text-center text-sm font-black text-slate-900">{item.quantity}</td>
                <td className="px-6 py-5 text-right text-sm font-black text-slate-900">{formatCurrency(item.lineTotal)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="border-t border-slate-50 bg-slate-50/20 px-6 py-8">
        <div className="mb-4 flex items-center gap-2">
          <span className="material-symbols-outlined text-xs text-slate-400">payments</span>
          <span className="text-[10px] font-black uppercase tracking-widest text-slate-400">Internal Transaction Analytics</span>
        </div>
        <div className="grid grid-cols-1 gap-6 md:grid-cols-3">
          <div className="flex flex-col gap-1 rounded-2xl border border-slate-100 bg-white p-5 shadow-sm">
            <p className="text-[10px] font-bold uppercase tracking-wider text-slate-400">Subtotal (Gross)</p>
            <p className="text-xl font-black text-slate-900">{formatCurrency(subOrder.subtotal)}</p>
          </div>
          <div className="flex flex-col gap-1 rounded-2xl border border-amber-100/50 bg-amber-50/30 p-5 shadow-sm">
            <p className="text-[10px] font-bold uppercase tracking-wider text-amber-600">Platform Fee ({(subOrder.commissionRate * 100).toFixed(0)}%)</p>
            <p className="text-xl font-black text-amber-700">- {formatCurrency(subOrder.commissionAmount)}</p>
          </div>
          <div className="flex flex-col gap-1 rounded-2xl border border-emerald-100/50 bg-emerald-50/30 p-5 shadow-sm">
            <p className="text-[10px] font-bold uppercase tracking-wider text-emerald-600">Store Net Payout</p>
            <p className="text-xl font-black text-emerald-700">{formatCurrency(subOrder.netAmount)}</p>
          </div>
        </div>
      </div>
    </section>
  )
}

function PaymentCard({ payments }: { payments: AdminOrderPaymentDto[] }) {
  const latestPayment = [...payments].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())[0]

  return (
    <section className="overflow-hidden rounded-3xl border border-slate-100 bg-white shadow-sm">
      <div className="flex items-center gap-3 border-b border-slate-50 p-6">
        <div className="flex size-8 items-center justify-center rounded-lg bg-emerald-50 text-emerald-600">
          <span className="material-symbols-outlined text-lg">account_balance_wallet</span>
        </div>
        <h3 className="font-bold text-slate-800">Financial Clearance</h3>
      </div>
      <div className="p-6">
        {latestPayment ? (
          <div className="space-y-4">
            <InfoRow label="Gateway">
              <span className="flex items-center gap-1.5 text-sm font-black text-slate-900">
                <span className="material-symbols-outlined text-sm text-primary">contactless</span>
                {latestPayment.method}
              </span>
            </InfoRow>
            <InfoRow label="Aggregator">
              <span className="text-sm font-black text-slate-900">{latestPayment.provider}</span>
            </InfoRow>
            <InfoRow label="Status">
              <span
                className={`rounded-lg px-2.5 py-1 text-[10px] font-black uppercase tracking-wider ${
                  latestPayment.status === 'PAID' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'
                }`}
              >
                {latestPayment.status}
              </span>
            </InfoRow>
            <InfoRow label="Voucher Ref">
              <span className="rounded bg-slate-50 px-2 py-1 font-mono text-[11px] font-bold text-slate-500">
                {latestPayment.transactionRef || 'PLATFORM_CREDIT'}
              </span>
            </InfoRow>
            {latestPayment.paidAt && (
              <div className="flex items-center justify-between py-3">
                <span className="text-xs font-bold uppercase tracking-widest text-slate-400">Clearance Date</span>
                <div className="text-right">
                  <p className="text-sm font-black leading-none text-slate-900">{formatDate(latestPayment.paidAt, { day: '2-digit', month: 'short', year: 'numeric' })}</p>
                  <p className="mt-1 text-[10px] font-bold text-slate-400">{formatTime(latestPayment.paidAt)}</p>
                </div>
              </div>
            )}
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center gap-2 py-6 text-slate-400">
            <span className="material-symbols-outlined text-3xl opacity-20">payments</span>
            <p className="text-xs font-bold uppercase tracking-widest opacity-40">Awaiting Settlement</p>
          </div>
        )}
      </div>
    </section>
  )
}

function InfoRow({ children, label }: { children: ReactNode; label: string }) {
  return (
    <div className="flex items-center justify-between border-b border-slate-50 py-3">
      <span className="text-xs font-bold uppercase tracking-widest text-slate-400">{label}</span>
      {children}
    </div>
  )
}

export default function AdminOrderDetailPage() {
  const { id: routeId } = useParams()
  const [searchParams] = useSearchParams()
  const id = routeId ?? searchParams.get('id') ?? ''
  const [order, setOrder] = useState<AdminOrderDetailDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  const loadOrder = async () => {
    if (!id) {
      setError('Order id is missing.')
      setLoading(false)
      return
    }

    setLoading(true)
    setError('')

    try {
      const data = await adminApi.orders.get(id)
      setOrder(data)
    } catch (err) {
      setOrder(null)
      setError(err instanceof Error ? err.message : 'Unable to load order.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadOrder()
  }, [id])

  const orderState = useMemo(() => {
    const subOrders = order?.subOrders ?? []
    const allDelivered = subOrders.length > 0 && subOrders.every((subOrder) => subOrder.status === orderStatus.delivered)
    const anyCancelled = subOrders.some((subOrder) => subOrder.status === orderStatus.cancelled)
    return { allDelivered, anyCancelled }
  }, [order])

  if (loading) {
    return (
      <AdminLayout activePage="Orders" breadcrumb={['Dashboard', 'Orders']} pageHeader="Order Details">
        <LoadingDetail />
      </AdminLayout>
    )
  }

  if (error || !order) {
    return (
      <AdminLayout activePage="Orders" breadcrumb={['Dashboard', 'Orders']} pageHeader="Order Details">
        <div className="rounded-xl border border-red-200 bg-red-50 p-6 text-sm font-medium text-red-700">
          {error || 'Order not found.'}
          <div className="mt-4">
            <Link to="/admin/orders" className="font-bold underline">
              Back to Orders
            </Link>
          </div>
        </div>
      </AdminLayout>
    )
  }

  const subtotalSum = order.subOrders.reduce((sum, subOrder) => sum + subOrder.subtotal, 0)
  const distinctStores = [...new Set(order.subOrders.map((subOrder) => subOrder.storeName))]

  return (
    <AdminLayout activePage="Orders" breadcrumb={['Dashboard', 'Orders', `#${order.orderCode}`]} pageHeader={`Order #${order.orderCode}`}>
      <div className="mb-8 flex flex-col justify-between gap-6 lg:flex-row lg:items-center">
        <div className="flex flex-wrap items-center gap-3">
          {order.paidAt ? (
            <StatusPill dotClass="animate-pulse bg-emerald-500" tone="border-emerald-200/50 bg-emerald-100 text-emerald-700">
              PAID
            </StatusPill>
          ) : (
            <StatusPill dotClass="bg-amber-500" tone="border-amber-200/50 bg-amber-100 text-amber-700">
              UNPAID
            </StatusPill>
          )}

          {orderState.allDelivered ? (
            <StatusPill dotClass="bg-blue-500" tone="border-blue-200/50 bg-blue-100 text-blue-700">
              FULLY DELIVERED
            </StatusPill>
          ) : orderState.anyCancelled ? (
            <StatusPill dotClass="bg-red-500" tone="border-red-200/50 bg-red-100 text-red-700">
              ATTENTION REQUIRED
            </StatusPill>
          ) : (
            <StatusPill dotClass="bg-slate-400" tone="border-slate-200/50 bg-slate-100 text-slate-700">
              PROCESSING
            </StatusPill>
          )}
        </div>

        <div className="flex flex-wrap items-center gap-3">
          <button type="button" className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-bold text-slate-700 shadow-sm transition-all hover:border-slate-300 hover:bg-slate-50">
            <span className="material-symbols-outlined text-lg">print</span>
            <span>Print Invoice</span>
          </button>
          <button type="button" className="flex items-center gap-2 rounded-xl border border-slate-200 bg-white px-4 py-2.5 text-sm font-bold text-slate-700 shadow-sm transition-all hover:border-slate-300 hover:bg-slate-50">
            <span className="material-symbols-outlined text-lg">download</span>
            <span>Export PDF</span>
          </button>
          <button type="button" className="flex items-center gap-2 rounded-xl bg-primary px-5 py-2.5 text-sm font-bold text-white shadow-lg shadow-primary/25 transition-all hover:bg-blue-700">
            <span className="material-symbols-outlined text-lg">edit_square</span>
            <span>Update Status</span>
          </button>
        </div>
      </div>

      <div className="mx-auto max-w-7xl">
        <div className="grid grid-cols-1 gap-8 xl:grid-cols-3">
          <div className="space-y-8 xl:col-span-2">
            <section className="overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm">
              <div className="flex items-center justify-between border-b border-slate-50 p-6">
                <div className="flex items-center gap-2">
                  <span className="material-symbols-outlined text-primary">info</span>
                  <h3 className="font-bold text-slate-800">General Information</h3>
                </div>
                <span className="rounded-lg bg-slate-100 px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider text-slate-500">Internal Reference</span>
              </div>
              <div className="grid grid-cols-1 gap-8 p-6 md:grid-cols-2 lg:grid-cols-3">
                <div className="flex flex-col gap-1">
                  <p className="text-[11px] font-black uppercase tracking-widest text-slate-400">Customer</p>
                  <p className="text-sm font-bold text-slate-900">{order.userName}</p>
                  <p className="mt-1 flex items-center gap-1.5 text-xs text-slate-500">
                    <span className="material-symbols-outlined text-xs">mail</span>
                    {order.userEmail}
                  </p>
                </div>
                <div className="flex flex-col gap-1">
                  <p className="text-[11px] font-black uppercase tracking-widest text-slate-400">Date Placed</p>
                  <p className="text-sm font-bold text-slate-900">{formatDate(order.createdAt)}</p>
                  <p className="mt-1 flex items-center gap-1.5 text-xs text-slate-500">
                    <span className="material-symbols-outlined text-xs">schedule</span>
                    {formatTime(order.createdAt)}
                  </p>
                </div>
                <div className="flex flex-col gap-1">
                  <p className="text-[11px] font-black uppercase tracking-widest text-slate-400">Source Store(s)</p>
                  <p className="text-sm font-bold text-primary">{order.subOrders.length} Store(s)</p>
                  <div className="mt-1.5 flex -space-x-2 overflow-hidden">
                    {distinctStores.map((store) => (
                      <div key={store} className="flex size-6 items-center justify-center rounded-full border-2 border-white bg-slate-200 text-[8px] font-bold text-slate-600" title={store}>
                        {initials(store)}
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            </section>

            {order.subOrders.map((subOrder) => (
              <SubOrderCard key={subOrder.id} subOrder={subOrder} />
            ))}

            <section className="group relative overflow-hidden rounded-3xl bg-slate-900 shadow-2xl">
              <div className="absolute right-0 top-0 -mr-32 h-full w-64 bg-primary/10 blur-3xl transition-all group-hover:bg-primary/20" />
              <div className="relative flex flex-col items-center justify-between gap-8 p-8 md:flex-row">
                <div className="flex items-center gap-4">
                  <div className="flex size-14 items-center justify-center rounded-2xl border border-white/10 bg-white/5 text-white shadow-inner">
                    <span className="material-symbols-outlined text-2xl">receipt_long</span>
                  </div>
                  <div>
                    <p className="mb-1 text-[10px] font-black uppercase tracking-widest text-slate-500">Total Payable Amount</p>
                    <h3 className="text-lg font-black tracking-tight text-white">Consolidated Order Invoice</h3>
                  </div>
                </div>

                <div className="flex min-w-[240px] flex-col items-end gap-3">
                  <div className="flex w-full items-center justify-between border-b border-white/5 pb-2 text-xs font-bold uppercase tracking-widest text-slate-400">
                    <span>Sum of Items</span>
                    <span className="text-slate-300">{formatCurrency(subtotalSum)}</span>
                  </div>
                  <div className="flex w-full items-center justify-between border-b border-white/5 pb-2 text-xs font-bold uppercase tracking-widest text-slate-400">
                    <span>Logistics Fee</span>
                    <span className="text-slate-300">+ {formatCurrency(order.shippingFee)}</span>
                  </div>
                  <div className="mt-2 flex w-full items-center justify-between">
                    <span className="text-base font-black uppercase tracking-tighter text-white">Grand Total</span>
                    <span className="text-3xl font-black tracking-tighter text-white drop-shadow-lg">{formatCurrency(order.grandTotal)}</span>
                  </div>
                </div>
              </div>
            </section>

            <section className="overflow-hidden rounded-3xl border border-slate-100 bg-white shadow-sm">
              <div className="flex items-center justify-between border-b border-slate-50 p-8">
                <div className="flex items-center gap-3">
                  <div className="flex size-10 items-center justify-center rounded-xl bg-slate-900 text-white shadow-lg">
                    <span className="material-symbols-outlined text-lg">history</span>
                  </div>
                  <h3 className="text-lg font-bold text-slate-900">Order Tracking Activity</h3>
                </div>
                <span className="rounded-lg bg-slate-100 px-3 py-1 text-[10px] font-black uppercase tracking-widest text-slate-500">Real-time History</span>
              </div>
              <div className="p-8">
                {order.statusHistories.length ? (
                  <div className="relative space-y-10 before:absolute before:inset-0 before:ml-[19px] before:h-full before:w-1 before:bg-gradient-to-b before:from-primary before:via-blue-300 before:to-transparent">
                    {[...order.statusHistories]
                      .sort((a, b) => new Date(b.changedAt).getTime() - new Date(a.changedAt).getTime())
                      .map((history) => (
                        <div key={`${history.changedAt}-${history.newStatus}`} className="group relative flex items-start gap-8">
                          <div className="z-10 rounded-full border-4 border-slate-50 bg-white p-1 transition-all group-first:border-primary/20">
                            <div className="flex size-7 items-center justify-center rounded-full border-2 border-primary bg-white shadow-sm">
                              <span className="material-symbols-outlined text-[14px] font-black text-primary">{statusIcon(history.newStatus)}</span>
                            </div>
                          </div>
                          <div className="-mt-1 flex-1 rounded-2xl border border-slate-100 bg-slate-50/50 p-5 transition-all hover:border-primary/20 hover:bg-white hover:shadow-md">
                            <div className="mb-2 flex items-center justify-between gap-3">
                              <h4 className="text-sm font-black uppercase tracking-wider text-slate-900">{statusLabel(history.newStatus)}</h4>
                              <span className="flex items-center gap-1.5 text-[10px] font-bold text-slate-400">
                                <span className="material-symbols-outlined text-[10px]">calendar_month</span>
                                {formatDate(history.changedAt, { day: '2-digit', month: 'short', year: 'numeric' })} - {formatTime(history.changedAt)}
                              </span>
                            </div>
                            {history.note && <p className="px-4 text-sm italic text-slate-500">"{history.note}"</p>}
                            <div className="mt-3 flex items-center gap-2 border-t border-slate-100/50 pt-3">
                              <span className="text-[10px] font-bold uppercase tracking-widest text-slate-400">Modified From</span>
                              <span className="rounded bg-slate-100 px-2 py-0.5 text-[10px] font-black text-slate-500">{statusLabel(history.oldStatus)}</span>
                            </div>
                          </div>
                        </div>
                      ))}
                  </div>
                ) : (
                  <div className="flex flex-col items-center justify-center gap-3 rounded-3xl border-2 border-dashed border-slate-100 py-12 text-slate-400">
                    <span className="material-symbols-outlined text-4xl opacity-20">history_toggle_off</span>
                    <p className="text-sm font-bold uppercase tracking-widest opacity-40">No activity recorded yet</p>
                  </div>
                )}
              </div>
            </section>
          </div>

          <aside className="space-y-8">
            <section className="overflow-hidden rounded-3xl border border-slate-100 bg-white shadow-sm">
              <div className="flex items-center gap-3 border-b border-slate-50 p-6">
                <div className="flex size-8 items-center justify-center rounded-lg bg-blue-50 text-primary">
                  <span className="material-symbols-outlined text-lg">local_shipping</span>
                </div>
                <h3 className="font-bold text-slate-800">Logistic Details</h3>
              </div>
              <div className="space-y-8 p-6">
                <div>
                  <p className="mb-3 text-[10px] font-black uppercase tracking-widest text-slate-400">Recipient</p>
                  <div className="flex items-center gap-3">
                    <div className="flex size-10 items-center justify-center rounded-full bg-slate-100 font-bold text-slate-500">{initials(order.receiverName)}</div>
                    <div>
                      <p className="text-sm font-black leading-none text-slate-900">{order.receiverName}</p>
                      <p className="mt-1 text-xs text-slate-500">{order.receiverPhone}</p>
                    </div>
                  </div>
                </div>
                <div>
                  <p className="mb-3 text-[10px] font-black uppercase tracking-widest text-slate-400">Destination Address</p>
                  <div className="rounded-2xl border border-slate-100 bg-slate-50 p-5 text-sm font-medium leading-relaxed text-slate-700 shadow-inner">
                    {order.shippingAddress.split(',').map((part, index) => (
                      <span key={`${part}-${index}`}>
                        {part.trim()}
                        {index < order.shippingAddress.split(',').length - 1 && (
                          <>
                            ,<br />
                          </>
                        )}
                      </span>
                    ))}
                  </div>
                </div>
                <div className="space-y-4 pt-2">
                  <div className="flex items-center justify-between">
                    <p className="text-[10px] font-black uppercase tracking-widest text-slate-400">Shipping Agent</p>
                    <span className="text-xs font-bold text-slate-900">{order.shippingProvider || 'PENDING ASSIGNMENT'}</span>
                  </div>
                  <div>
                    <p className="mb-2 text-right text-[10px] font-black uppercase tracking-widest text-slate-400">Waybill / Tracking</p>
                    {order.trackingNumber ? (
                      <div className="flex items-center justify-end gap-2">
                        <code className="rounded-lg border border-primary/10 bg-primary/5 px-3 py-1.5 font-mono text-[11px] font-bold text-primary">{order.trackingNumber}</code>
                        <button
                          type="button"
                          onClick={() => void navigator.clipboard.writeText(order.trackingNumber ?? '')}
                          className="flex size-8 items-center justify-center rounded-lg border border-slate-200 bg-white text-slate-400 shadow-sm transition-all hover:border-primary hover:text-primary"
                        >
                          <span className="material-symbols-outlined text-[16px]">content_copy</span>
                        </button>
                      </div>
                    ) : (
                      <p className="text-right text-xs italic text-slate-400">No tracking info yet.</p>
                    )}
                  </div>
                </div>
              </div>
            </section>

            <PaymentCard payments={order.payments} />

            <section className="group relative overflow-hidden rounded-3xl bg-gradient-to-br from-slate-900 via-blue-900 to-indigo-950 p-8 text-white shadow-2xl">
              <div className="absolute -right-10 -top-10 size-40 rounded-full bg-blue-500/10 blur-3xl transition-all group-hover:bg-blue-500/20" />
              <div className="absolute -bottom-10 -left-10 size-32 rounded-full bg-indigo-500/10 blur-2xl" />
              <div className="relative">
                <div className="mb-6 flex size-12 items-center justify-center rounded-2xl border border-white/20 bg-white/10 shadow-lg backdrop-blur-xl">
                  <span className="material-symbols-outlined text-white">support_agent</span>
                </div>
                <h4 className="mb-3 text-xl font-black tracking-tight">Need Assistance?</h4>
                <p className="mb-8 text-sm font-medium leading-relaxed text-slate-300">Our senior administrators are ready to resolve any seller-customer disputes 24/7.</p>
                <div className="flex flex-col gap-3">
                  <button type="button" className="flex w-full items-center justify-center gap-2 rounded-2xl bg-white py-3.5 text-sm font-black text-slate-900 shadow-xl shadow-white/5 transition-all hover:scale-[1.02] active:scale-[0.98]">
                    <span className="material-symbols-outlined text-[18px]">emergency</span>
                    Raise Multi-party Ticket
                  </button>
                  <button type="button" className="flex w-full items-center justify-center gap-2 rounded-2xl border border-white/10 bg-white/5 py-3.5 text-sm font-bold text-white transition-all hover:bg-white/10">
                    Learn Dispute Policy
                    <span className="material-symbols-outlined text-[16px]">open_in_new</span>
                  </button>
                </div>
              </div>
            </section>
          </aside>
        </div>
      </div>
    </AdminLayout>
  )
}
