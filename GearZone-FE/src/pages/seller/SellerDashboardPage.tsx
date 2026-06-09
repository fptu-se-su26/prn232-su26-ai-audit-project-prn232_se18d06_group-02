import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface DashboardData {
  totalRevenue: number
  totalOrders: number
  pendingOrders: number
  totalProducts: number
  recentOrders?: Array<{
    id: string
    orderCode?: number
    status: string
    totalPrice: number
    createdAt: string
    buyerName?: string
  }>
}

const STATUS_COLOR: Record<string, string> = {
  Pending: 'bg-amber-100 text-amber-700',
  Approved: 'bg-blue-100 text-blue-700',
  Processing: 'bg-violet-100 text-violet-700',
  Shipping: 'bg-yellow-100 text-yellow-700',
  Delivered: 'bg-emerald-100 text-emerald-700',
  Completed: 'bg-emerald-100 text-emerald-700',
  Cancelled: 'bg-red-100 text-red-700',
}

function formatPrice(v: number) {
  return new Intl.NumberFormat('vi-VN').format(v ?? 0)
}

export default function SellerDashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    sellerApi
      .getDashboard()
      .then((d) => setData(d as DashboardData))
      .catch((err: unknown) =>
        setError(err instanceof Error ? err.message : 'Failed to load dashboard.'),
      )
      .finally(() => setLoading(false))
  }, [])

  const stats = [
    {
      label: 'Total Revenue',
      value: `${formatPrice(data?.totalRevenue ?? 0)} ₫`,
      icon: 'payments',
      color: 'text-emerald-600',
      bg: 'bg-emerald-50',
    },
    {
      label: 'Total Orders',
      value: data?.totalOrders ?? 0,
      icon: 'shopping_bag',
      color: 'text-blue-600',
      bg: 'bg-blue-50',
    },
    {
      label: 'Pending Orders',
      value: data?.pendingOrders ?? 0,
      icon: 'pending_actions',
      color: 'text-amber-600',
      bg: 'bg-amber-50',
    },
    {
      label: 'Products Listed',
      value: data?.totalProducts ?? 0,
      icon: 'inventory_2',
      color: 'text-violet-600',
      bg: 'bg-violet-50',
    },
  ]

  return (
    <SellerLayout pageHeader="Dashboard">
      {loading ? (
        <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <div className="h-28 animate-pulse rounded-xl bg-white" key={i} />
          ))}
        </div>
      ) : error ? (
        <div className="rounded-xl border border-red-200 bg-red-50 px-6 py-10 text-center text-red-600">
          {error}
        </div>
      ) : (
        <>
          {/* Stats */}
          <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
            {stats.map((s) => (
              <div
                key={s.label}
                className="flex items-center gap-4 rounded-xl border border-slate-100 bg-white p-5 shadow-sm"
              >
                <div className={`flex h-12 w-12 items-center justify-center rounded-xl ${s.bg}`}>
                  <span className={`material-symbols-outlined text-2xl ${s.color}`}>{s.icon}</span>
                </div>
                <div>
                  <p className="text-xs text-slate-500">{s.label}</p>
                  <p className={`mt-0.5 text-xl font-bold ${s.color}`}>{s.value}</p>
                </div>
              </div>
            ))}
          </div>

          {/* Quick actions */}
          <div className="flex flex-wrap gap-3">
            <Link
              to="/store-owner/products"
              className="inline-flex items-center gap-2 rounded-lg bg-[#ff6b00] px-5 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-[#ea580c]"
            >
              <span className="material-symbols-outlined text-[18px]">inventory_2</span>
              Manage Products
            </Link>
            <Link
              to="/store-owner/orders"
              className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-5 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50"
            >
              <span className="material-symbols-outlined text-[18px]">shopping_bag</span>
              Manage Orders
            </Link>
            <Link
              to="/store-owner/revenue"
              className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-5 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50"
            >
              <span className="material-symbols-outlined text-[18px]">payments</span>
              View Revenue
            </Link>
            <Link
              to="/store-owner/settings"
              className="inline-flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-5 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50"
            >
              <span className="material-symbols-outlined text-[18px]">settings</span>
              Store Settings
            </Link>
          </div>

          {/* Recent orders */}
          {data?.recentOrders && data.recentOrders.length > 0 && (
            <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
              <div className="flex items-center justify-between border-b border-slate-100 px-6 py-4">
                <h2 className="text-base font-semibold text-slate-800">Recent Orders</h2>
                <Link
                  to="/store-owner/orders"
                  className="text-sm font-medium text-[#ff6b00] hover:underline"
                >
                  View all →
                </Link>
              </div>
              <table className="min-w-full divide-y divide-slate-100">
                <thead className="bg-slate-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                      Order
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                      Buyer
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                      Status
                    </th>
                    <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">
                      Amount
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider text-slate-500">
                      Date
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-50">
                  {data.recentOrders.map((o) => (
                    <tr className="hover:bg-slate-50" key={o.id}>
                      <td className="px-6 py-3 font-mono text-sm font-semibold text-slate-800">
                        #{o.orderCode ?? o.id.slice(0, 8).toUpperCase()}
                      </td>
                      <td className="px-6 py-3 text-sm text-slate-600">{o.buyerName ?? '—'}</td>
                      <td className="px-6 py-3">
                        <span
                          className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${STATUS_COLOR[o.status] ?? 'bg-slate-100 text-slate-600'}`}
                        >
                          {o.status}
                        </span>
                      </td>
                      <td className="px-6 py-3 text-right text-sm font-semibold text-slate-800">
                        {formatPrice(o.totalPrice)} ₫
                      </td>
                      <td className="px-6 py-3 text-sm text-slate-500">
                        {new Date(o.createdAt).toLocaleDateString('vi-VN')}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </>
      )}
    </SellerLayout>
  )
}
