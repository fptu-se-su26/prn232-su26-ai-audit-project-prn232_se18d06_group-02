import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface MonthlyRevenuePoint {
  label: string
  revenue: number
}

interface RecentOrderItem {
  subOrderId?: string
  id?: string
  orderCode?: number | string
  buyerName?: string
  status: string
  subtotal?: number
  totalPrice?: number
  createdAt: string
}

interface RecentPayoutItem {
  transactionCode?: string
  TransactionCode?: string
  netAmount?: number
  NetAmount?: number
  status: string
  createdAt?: string
  CreatedAt?: string
}

interface DashboardData {
  hasStore: boolean
  storeName?: string
  conversationCount?: number
  unreadCount?: number
  totalOrders?: number
  pendingOrders?: number
  fulfilledOrders?: number
  grossRevenue?: number
  paidOut?: number
  pendingPayout?: number
  revenueByMonth?: MonthlyRevenuePoint[]
  recentOrders?: RecentOrderItem[]
  recentPayouts?: RecentPayoutItem[]
}

interface ChartPoint {
  x: number
  y: number
  label: string
  revenue: number
}

const orderStatusClasses: Record<string, string> = {
  Pending: 'bg-amber-50 text-amber-700',
  AwaitingPayment: 'bg-orange-50 text-orange-700',
  Approved: 'bg-sky-50 text-sky-700',
  Paid: 'bg-indigo-50 text-indigo-700',
  Processing: 'bg-blue-50 text-blue-700',
  Delivered: 'bg-emerald-50 text-emerald-700',
  Completed: 'bg-emerald-100 text-emerald-800',
  Cancelled: 'bg-rose-50 text-rose-700',
  Refunded: 'bg-orange-50 text-orange-700',
  Rejected: 'bg-red-50 text-red-700',
}

const payoutStatusClasses: Record<string, string> = {
  Queued: 'bg-slate-50 text-slate-700',
  Processing: 'bg-blue-50 text-blue-700',
  Success: 'bg-emerald-50 text-emerald-700',
  Failed: 'bg-red-50 text-red-700',
  ManualRequired: 'bg-orange-50 text-orange-700',
  Excluded: 'bg-slate-100 text-slate-600',
}

const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

function formatNumber(value?: number) {
  return new Intl.NumberFormat('en-US').format(value ?? 0)
}

function pad(value: number) {
  return value.toString().padStart(2, '0')
}

function formatDateTime(value?: string, monthStyle: 'short' | 'numeric' = 'short') {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  const day = pad(date.getDate())
  const month = monthStyle === 'numeric' ? pad(date.getMonth() + 1) : monthNames[date.getMonth()]
  const year = date.getFullYear()
  const time = `${pad(date.getHours())}:${pad(date.getMinutes())}`

  return monthStyle === 'numeric'
    ? `${day}/${month}/${year} ${time}`
    : `${day} ${month} ${year} ${time}`
}

function getPayoutCode(payout: RecentPayoutItem) {
  return payout.transactionCode ?? payout.TransactionCode ?? ''
}

function getPayoutAmount(payout: RecentPayoutItem) {
  return payout.netAmount ?? payout.NetAmount ?? 0
}

function getPayoutDate(payout: RecentPayoutItem) {
  return payout.createdAt ?? payout.CreatedAt
}

export default function SellerDashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    sellerApi
      .getDashboard()
      .then((dashboard) => setData(dashboard as DashboardData))
      .catch((err: unknown) =>
        setError(err instanceof Error ? err.message : 'Failed to load dashboard.'),
      )
      .finally(() => setLoading(false))
  }, [])

  const revenueByMonth = data?.revenueByMonth ?? []
  const recentOrders = data?.recentOrders ?? []
  const recentPayouts = data?.recentPayouts ?? []

  const chart = useMemo(() => {
    const maxRevenue = Math.max(...revenueByMonth.map((point) => point.revenue), 0)
    const chartMax = maxRevenue > 0 ? maxRevenue : 1
    const pointCount = revenueByMonth.length
    const step = pointCount > 1 ? 1000 / (pointCount - 1) : 500

    const points: ChartPoint[] = revenueByMonth.map((point, index) => ({
      x: pointCount > 1 ? index * step : 500,
      y: 220 - (point.revenue / chartMax) * 180,
      label: point.label,
      revenue: point.revenue,
    }))

    const linePoints = points.map((point) => `${point.x},${point.y}`).join(' ')
    const areaPath =
      points.length > 0
        ? `M${points[0].x},220 ${points
            .map((point) => `L${point.x},${point.y}`)
            .join(' ')} L${points[points.length - 1].x},220 L${points[0].x},220 Z`
        : ''

    return { points, linePoints, areaPath }
  }, [revenueByMonth])

  if (loading) {
    return (
      <SellerLayout pageHeader="Store Overview">
        <div className="h-[90px] animate-pulse rounded-xl bg-white" />
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, index) => (
            <div key={index} className="h-[102px] animate-pulse rounded-xl bg-white" />
          ))}
        </div>
        <div className="grid grid-cols-1 gap-6 lg:grid-cols-5">
          <div className="h-[414px] animate-pulse rounded-xl bg-white lg:col-span-3" />
          <div className="h-[414px] animate-pulse rounded-xl bg-white lg:col-span-2" />
        </div>
      </SellerLayout>
    )
  }

  if (error) {
    return (
      <SellerLayout pageHeader="Store Overview" unreadCount={data?.unreadCount}>
        <div className="rounded-xl border border-red-200 bg-red-50 px-6 py-10 text-center text-red-600">
          {error}
        </div>
      </SellerLayout>
    )
  }

  if (!data?.hasStore) {
    return (
      <SellerLayout pageHeader="Store Overview" unreadCount={data?.unreadCount}>
        <div className="rounded-xl border border-slate-200 bg-white p-8 text-center shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-full bg-slate-100 text-slate-500">
            <span className="material-symbols-outlined text-[28px]">store</span>
          </div>
          <h2 className="text-xl font-bold text-slate-900">No store data yet</h2>
          <p className="mt-2 text-sm text-slate-500">
            Your overview will appear after your store is approved and starts receiving orders.
          </p>
        </div>
      </SellerLayout>
    )
  }

  return (
    <SellerLayout pageHeader="Store Overview" unreadCount={data.unreadCount}>
      <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
        <h2 className="text-lg font-bold text-slate-900">{data.storeName ?? 'Your Store'}</h2>
        <p className="text-sm text-slate-500">
          Live performance snapshot of your store operations and payout status.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
          <div className="mb-3 flex items-center justify-between">
            <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">
              Total Orders
            </p>
            <span className="material-symbols-outlined text-primary">receipt_long</span>
          </div>
          <h3 className="text-2xl font-bold text-slate-900">{formatNumber(data.totalOrders)}</h3>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
          <div className="mb-3 flex items-center justify-between">
            <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">
              Awaiting Action
            </p>
            <span className="material-symbols-outlined text-orange-600">pending_actions</span>
          </div>
          <h3 className="text-2xl font-bold text-slate-900">
            {formatNumber(data.pendingOrders)}
          </h3>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
          <div className="mb-3 flex items-center justify-between">
            <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">
              Gross Revenue
            </p>
            <span className="material-symbols-outlined text-indigo-600">payments</span>
          </div>
          <h3 className="text-2xl font-bold text-slate-900">
            {formatNumber(data.grossRevenue)} VND
          </h3>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
          <div className="mb-3 flex items-center justify-between">
            <p className="text-xs font-semibold uppercase tracking-wider text-slate-500">
              Pending Payout
            </p>
            <span className="material-symbols-outlined text-orange-600">
              account_balance_wallet
            </span>
          </div>
          <h3 className="text-2xl font-bold text-slate-900">
            {formatNumber(data.pendingPayout)} VND
          </h3>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-5">
        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)] lg:col-span-3">
          <div className="mb-5 flex items-center justify-between">
            <div>
              <h3 className="text-base font-bold text-slate-900">Revenue (Last 6 Months)</h3>
              <p className="text-sm text-slate-500">Monthly gross subtotal from your orders.</p>
            </div>
          </div>
          <div className="h-64 overflow-hidden rounded-lg border border-slate-100 bg-slate-50/70 p-3">
            {revenueByMonth.length > 0 ? (
              <svg className="h-full w-full" preserveAspectRatio="none" viewBox="0 0 1000 240">
                <line x1="0" y1="220" x2="1000" y2="220" stroke="#e2e8f0" strokeWidth="1" />
                <line x1="0" y1="175" x2="1000" y2="175" stroke="#f1f5f9" strokeWidth="1" />
                <line x1="0" y1="130" x2="1000" y2="130" stroke="#f1f5f9" strokeWidth="1" />
                <line x1="0" y1="85" x2="1000" y2="85" stroke="#f1f5f9" strokeWidth="1" />
                <line x1="0" y1="40" x2="1000" y2="40" stroke="#f1f5f9" strokeWidth="1" />

                {chart.areaPath && <path d={chart.areaPath} fill="url(#sellerRevenueFill)" />}
                {chart.linePoints && (
                  <polyline
                    points={chart.linePoints}
                    fill="none"
                    stroke="#1A56DB"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    strokeWidth="3"
                  />
                )}

                {chart.points.map((point) => (
                  <circle
                    key={`${point.label}-${point.x}`}
                    cx={point.x}
                    cy={point.y}
                    fill="#1A56DB"
                    r="4.5"
                    stroke="#ffffff"
                    strokeWidth="2"
                  >
                    <title>{`${point.label}: ${formatNumber(point.revenue)} VND`}</title>
                  </circle>
                ))}

                <defs>
                  <linearGradient id="sellerRevenueFill" x1="0%" x2="0%" y1="0%" y2="100%">
                    <stop offset="0%" stopColor="#1A56DB" stopOpacity="0.22" />
                    <stop offset="100%" stopColor="#1A56DB" stopOpacity="0.02" />
                  </linearGradient>
                </defs>
              </svg>
            ) : (
              <div className="flex h-full items-center justify-center text-sm text-slate-400">
                No revenue data available.
              </div>
            )}
          </div>
          <div className="mt-3 grid grid-cols-6 gap-2">
            {revenueByMonth.map((point) => (
              <div key={point.label} className="text-center">
                <p className="text-xs font-semibold text-slate-600">{point.label}</p>
                <p className="text-[10px] text-slate-400">{formatNumber(point.revenue)}</p>
              </div>
            ))}
          </div>
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)] lg:col-span-2">
          <h3 className="text-base font-bold text-slate-900">Operations Snapshot</h3>
          <p className="mb-5 text-sm text-slate-500">
            High-level health for orders, chat, and payout.
          </p>
          <div className="space-y-4">
            <div className="flex items-center justify-between rounded-lg bg-slate-50 px-4 py-3">
              <span className="text-sm font-medium text-slate-600">Fulfilled Orders</span>
              <span className="text-base font-bold text-slate-900">
                {formatNumber(data.fulfilledOrders)}
              </span>
            </div>
            <div className="flex items-center justify-between rounded-lg bg-slate-50 px-4 py-3">
              <span className="text-sm font-medium text-slate-600">Paid Out</span>
              <span className="text-base font-bold text-emerald-700">
                {formatNumber(data.paidOut)} VND
              </span>
            </div>
            <div className="flex items-center justify-between rounded-lg bg-slate-50 px-4 py-3">
              <span className="text-sm font-medium text-slate-600">Conversations</span>
              <span className="text-base font-bold text-slate-900">
                {formatNumber(data.conversationCount)}
              </span>
            </div>
            <Link
              to="/store-owner/messages"
              className="flex items-center justify-between rounded-lg border border-orange-200 bg-orange-50 px-4 py-3 text-sm font-semibold text-orange-700 transition-colors hover:bg-orange-100"
            >
              <span>Unread messages</span>
              <span>{formatNumber(data.unreadCount)}</span>
            </Link>
          </div>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
          <div className="flex items-center justify-between border-b border-slate-100 px-5 py-4">
            <h3 className="font-bold text-slate-900">Recent Orders</h3>
            <Link to="/store-owner/orders" className="text-sm font-medium text-primary hover:underline">
              View all
            </Link>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50/70 text-xs uppercase tracking-wider text-slate-500">
                <tr>
                  <th className="px-5 py-3">Order</th>
                  <th className="px-4 py-3">Buyer</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-5 py-3 text-right">Subtotal</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {recentOrders.length > 0 ? (
                  recentOrders.map((order) => {
                    const orderId = order.subOrderId ?? order.id ?? ''
                    const subtotal = order.subtotal ?? order.totalPrice ?? 0

                    return (
                      <tr key={orderId || order.orderCode} className="hover:bg-slate-50">
                        <td className="px-5 py-3">
                          <Link
                            to="/store-owner/orders"
                            className="font-semibold text-slate-900 hover:text-primary"
                          >
                            #{order.orderCode ?? orderId.slice(0, 8).toUpperCase()}
                          </Link>
                          <p className="text-xs text-slate-500">
                            {formatDateTime(order.createdAt)}
                          </p>
                        </td>
                        <td className="px-4 py-3 text-slate-700">{order.buyerName ?? 'Buyer'}</td>
                        <td className="px-4 py-3">
                          <span
                            className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${
                              orderStatusClasses[order.status] ?? 'bg-slate-50 text-slate-700'
                            }`}
                          >
                            {order.status}
                          </span>
                        </td>
                        <td className="px-5 py-3 text-right font-semibold text-slate-900">
                          {formatNumber(subtotal)}
                        </td>
                      </tr>
                    )
                  })
                ) : (
                  <tr>
                    <td colSpan={4} className="px-5 py-10 text-center text-slate-500">
                      No recent orders yet.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>

        <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
          <div className="flex items-center justify-between border-b border-slate-100 px-5 py-4">
            <h3 className="font-bold text-slate-900">Recent Payout Transactions</h3>
            <Link
              to="/store-owner/revenue"
              className="text-sm font-medium text-primary hover:underline"
            >
              Payout management
            </Link>
          </div>
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="bg-slate-50/70 text-xs uppercase tracking-wider text-slate-500">
                <tr>
                  <th className="px-5 py-3">Code</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3 text-right">Amount</th>
                  <th className="px-5 py-3">Date</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {recentPayouts.length > 0 ? (
                  recentPayouts.map((payout, index) => (
                    <tr key={`${getPayoutCode(payout)}-${index}`} className="hover:bg-slate-50">
                      <td className="px-5 py-3 font-mono text-xs font-semibold text-slate-800">
                        {getPayoutCode(payout)}
                      </td>
                      <td className="px-4 py-3">
                        <span
                          className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${
                            payoutStatusClasses[payout.status] ?? 'bg-slate-50 text-slate-700'
                          }`}
                        >
                          {payout.status}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right font-semibold text-slate-900">
                        {formatNumber(getPayoutAmount(payout))}
                      </td>
                      <td className="px-5 py-3 text-slate-600">
                        {formatDateTime(getPayoutDate(payout), 'numeric')}
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={4} className="px-5 py-10 text-center text-slate-500">
                      No payout records found yet.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </SellerLayout>
  )
}
