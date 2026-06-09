import { useEffect, useState } from 'react'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface OrderItem {
  productName: string
  variantName?: string
  quantity: number
  price: number
  imageUrl?: string
}

interface SubOrder {
  id: string
  orderCode?: number
  status: string
  totalPrice: number
  shippingFee?: number
  createdAt: string
  buyerName: string
  recipientName?: string
  recipientPhone?: string
  shippingAddress?: string
  trackingNumber?: string
  items: OrderItem[]
}

const ALL_TABS = ['All', 'Pending', 'Approved', 'Processing', 'Shipping', 'Delivered', 'Completed', 'Cancelled']

const STATUS_BADGE: Record<string, string> = {
  Pending: 'bg-amber-100 text-amber-700',
  Approved: 'bg-blue-100 text-blue-700',
  Processing: 'bg-violet-100 text-violet-700',
  Shipping: 'bg-yellow-100 text-yellow-700',
  Delivered: 'bg-emerald-100 text-emerald-700',
  Completed: 'bg-emerald-100 text-emerald-700',
  Cancelled: 'bg-red-100 text-red-700',
}

const ACTIONS: Record<
  string,
  Array<{ label: string; action: 'approve' | 'reject' | 'markProcessing' | 'markDelivered'; variant: 'primary' | 'danger' | 'default' }>
> = {
  Pending: [
    { label: 'Approve', action: 'approve', variant: 'primary' },
    { label: 'Reject', action: 'reject', variant: 'danger' },
  ],
  Approved: [{ label: 'Mark Processing', action: 'markProcessing', variant: 'default' }],
  Processing: [{ label: 'Mark Delivered', action: 'markDelivered', variant: 'primary' }],
}

function formatPrice(v: number) {
  return new Intl.NumberFormat('vi-VN').format(v ?? 0)
}

export default function SellerOrdersPage() {
  const [orders, setOrders] = useState<SubOrder[]>([])
  const [loading, setLoading] = useState(true)
  const [tab, setTab] = useState('All')
  const [expandedId, setExpandedId] = useState<string | null>(null)
  const [processing, setProcessing] = useState<string | null>(null)

  const fetchOrders = () => {
    setLoading(true)
    sellerApi.orders
      .list()
      .then((d) =>
        setOrders((d as { items?: SubOrder[] }).items ?? (d as SubOrder[]) ?? []),
      )
      .finally(() => setLoading(false))
  }

  useEffect(() => { fetchOrders() }, [])

  const handleAction = async (
    id: string,
    action: 'approve' | 'reject' | 'markProcessing' | 'markDelivered',
  ) => {
    setProcessing(id)
    try {
      const map = {
        approve: () => sellerApi.orders.approve(id),
        reject: () => sellerApi.orders.reject(id, 'Rejected by seller'),
        markProcessing: () => sellerApi.orders.markProcessing(id),
        markDelivered: () => sellerApi.orders.markDelivered(id),
      }
      await map[action]()
      fetchOrders()
    } finally {
      setProcessing(null)
    }
  }

  const tabCounts = ALL_TABS.reduce<Record<string, number>>((acc, t) => {
    acc[t] = t === 'All' ? orders.length : orders.filter((o) => o.status === t).length
    return acc
  }, {})

  const filtered = tab === 'All' ? orders : orders.filter((o) => o.status === tab)

  const stats = [
    { label: 'Total', value: orders.length, color: 'text-blue-600', bg: 'bg-blue-50', icon: 'shopping_bag' },
    { label: 'Pending', value: orders.filter((o) => o.status === 'Pending').length, color: 'text-amber-600', bg: 'bg-amber-50', icon: 'pending_actions' },
    { label: 'Processing', value: orders.filter((o) => ['Processing', 'Approved'].includes(o.status)).length, color: 'text-violet-600', bg: 'bg-violet-50', icon: 'sync' },
    { label: 'Delivered', value: orders.filter((o) => ['Delivered', 'Completed'].includes(o.status)).length, color: 'text-emerald-600', bg: 'bg-emerald-50', icon: 'done_all' },
  ]

  return (
    <SellerLayout pageHeader="Orders" breadcrumb={['Orders']}>
      {loading ? (
        <div className="space-y-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <div className="h-16 animate-pulse rounded-xl bg-white" key={i} />
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

          {/* Tabs */}
          <div className="flex flex-wrap gap-0 border-b border-slate-200 bg-white px-4 pt-2 rounded-t-xl shadow-sm">
            {ALL_TABS.filter((t) => tabCounts[t] > 0 || t === 'All').map((t) => (
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

          {/* Order rows */}
          {filtered.length === 0 ? (
            <div className="rounded-xl border border-dashed border-slate-300 bg-white p-12 text-center text-slate-400">
              <span className="material-symbols-outlined mb-3 block text-5xl">inbox</span>
              <p>No orders in this status.</p>
            </div>
          ) : (
            <div className="flex flex-col gap-2">
              {filtered.map((order) => {
                const expanded = expandedId === order.id
                return (
                  <div
                    className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm"
                    key={order.id}
                  >
                    {/* Header row */}
                    <button
                      className="flex w-full items-center justify-between gap-4 px-5 py-3.5 text-left transition hover:bg-slate-50"
                      onClick={() => setExpandedId(expanded ? null : order.id)}
                      type="button"
                    >
                      <div className="flex flex-wrap items-center gap-4">
                        <span className="font-mono text-sm font-bold text-slate-800">
                          #{order.orderCode ?? order.id.slice(0, 8).toUpperCase()}
                        </span>
                        <span className="text-sm text-slate-500">
                          {new Date(order.createdAt).toLocaleDateString('vi-VN')}
                        </span>
                        <span className="text-sm text-slate-600">{order.buyerName}</span>
                      </div>
                      <div className="flex items-center gap-3">
                        <span
                          className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${STATUS_BADGE[order.status] ?? 'bg-slate-100 text-slate-600'}`}
                        >
                          {order.status}
                        </span>
                        <span className="text-sm font-bold text-slate-800">
                          {formatPrice(order.totalPrice)} ₫
                        </span>
                        <span className="material-symbols-outlined text-slate-400 text-[18px]">
                          {expanded ? 'expand_less' : 'expand_more'}
                        </span>
                      </div>
                    </button>

                    {/* Expanded detail */}
                    {expanded && (
                      <div className="border-t border-slate-100 px-5 py-4">
                        {(order.recipientName || order.shippingAddress) && (
                          <div className="mb-3 flex flex-wrap gap-4 text-sm text-slate-500">
                            {order.recipientName && (
                              <span>
                                <strong className="text-slate-700">Recipient:</strong>{' '}
                                {order.recipientName} {order.recipientPhone}
                              </span>
                            )}
                            {order.shippingAddress && (
                              <span>
                                <strong className="text-slate-700">Address:</strong>{' '}
                                {order.shippingAddress}
                              </span>
                            )}
                          </div>
                        )}

                        <table className="mb-3 w-full text-sm">
                          <thead>
                            <tr className="border-b border-slate-100 text-xs text-slate-400">
                              <th className="pb-1.5 text-left font-medium">Product</th>
                              <th className="pb-1.5 text-right font-medium">Qty</th>
                              <th className="pb-1.5 text-right font-medium">Total</th>
                            </tr>
                          </thead>
                          <tbody className="divide-y divide-slate-50">
                            {order.items?.map((item, i) => (
                              <tr key={i}>
                                <td className="py-2">
                                  <div className="flex items-center gap-2">
                                    {item.imageUrl && (
                                      <img
                                        alt=""
                                        className="h-8 w-8 rounded object-cover"
                                        src={item.imageUrl}
                                      />
                                    )}
                                    <div>
                                      <p className="font-medium text-slate-700">{item.productName}</p>
                                      {item.variantName && (
                                        <p className="text-xs text-slate-400">{item.variantName}</p>
                                      )}
                                    </div>
                                  </div>
                                </td>
                                <td className="py-2 text-right text-slate-500">×{item.quantity}</td>
                                <td className="py-2 text-right font-semibold text-slate-700">
                                  {formatPrice(item.price * item.quantity)} ₫
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>

                        <div className="flex items-center justify-between gap-4">
                          <div className="text-sm text-slate-500">
                            {order.trackingNumber && (
                              <span>
                                Tracking:{' '}
                                <strong className="font-mono text-slate-700">
                                  {order.trackingNumber}
                                </strong>
                              </span>
                            )}
                          </div>
                          <div className="text-sm text-slate-500">
                            {order.shippingFee != null && (
                              <span className="mr-4">
                                Shipping: {formatPrice(order.shippingFee)} ₫
                              </span>
                            )}
                            <strong className="text-slate-800">
                              Total: {formatPrice(order.totalPrice)} ₫
                            </strong>
                          </div>
                        </div>

                        {ACTIONS[order.status] && (
                          <div className="mt-3 flex gap-2 border-t border-slate-100 pt-3">
                            {ACTIONS[order.status].map((btn) => (
                              <button
                                className={`rounded-lg px-4 py-1.5 text-sm font-semibold transition ${
                                  btn.variant === 'primary'
                                    ? 'bg-[#ff6b00] text-white hover:bg-[#ea580c]'
                                    : btn.variant === 'danger'
                                      ? 'bg-red-500 text-white hover:bg-red-600'
                                      : 'border border-slate-200 bg-white text-slate-700 hover:bg-slate-50'
                                }`}
                                disabled={processing === order.id}
                                key={btn.action}
                                onClick={() => handleAction(order.id, btn.action)}
                                type="button"
                              >
                                {processing === order.id ? '…' : btn.label}
                              </button>
                            ))}
                          </div>
                        )}
                      </div>
                    )}
                  </div>
                )
              })}
            </div>
          )}
        </>
      )}
    </SellerLayout>
  )
}
