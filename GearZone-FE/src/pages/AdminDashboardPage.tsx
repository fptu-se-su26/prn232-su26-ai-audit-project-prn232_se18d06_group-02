/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import {
  adminApi,
  type AdminDashboardDto,
  type CategoryRevenueDto,
  type ChartDataPoint,
  type DashboardKpis,
  type DashboardQueryParams,
  type DashboardStoreDto,
  type OrderStatusBreakdownDto,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const periodOptions = [
  { label: 'Today', value: 'today' },
  { label: 'This Week', value: 'week' },
  { label: 'This Month', value: 'month' },
  { label: 'This Year', value: 'year' },
  { label: 'Custom Range', value: 'custom' },
]

const currencyFormatter = new Intl.NumberFormat('vi-VN', {
  currency: 'VND',
  maximumFractionDigits: 0,
  style: 'currency',
})

const numberFormatter = new Intl.NumberFormat('en-US')

function formatCurrency(value: number) {
  return currencyFormatter.format(value ?? 0)
}

function formatNumber(value: number) {
  return numberFormatter.format(value ?? 0)
}

function formatGrowth(value: number) {
  const sign = value >= 0 ? '+' : ''
  return `${sign}${value.toFixed(1)}%`
}

function trendTone(value: number) {
  return value >= 0 ? 'text-emerald-600' : 'text-red-600'
}

function trendIcon(value: number) {
  return value >= 0 ? 'trending_up' : 'trending_down'
}

function chartPoints(data: ChartDataPoint[], key: 'value' | 'secondaryValue', maxValue: number, height = 220) {
  const step = 1000 / Math.max(1, data.length - 1)
  return data
    .map((item, index) => {
      const rawValue = key === 'value' ? item.value : item.secondaryValue ?? 0
      const x = index * step
      const y = height - (rawValue / maxValue) * (height - 24)
      return `${x.toFixed(2)},${y.toFixed(2)}`
    })
    .join(' ')
}

function areaPath(points: string) {
  if (!points) return ''
  const commands = points
    .split(' ')
    .filter(Boolean)
    .map((point) => `L${point}`)
    .join(' ')

  return `M0,256 ${commands} L1000,256 Z`
}

function sampledLabels(data: ChartDataPoint[]) {
  const interval = Math.max(1, Math.floor(data.length / 6))
  return data.filter((_, index) => index % interval === 0)
}

function statusColor(status: string, fallbackClass: string) {
  const normalized = status.toLowerCase()
  if (normalized.includes('complete') || normalized.includes('deliver') || normalized.includes('paid')) return '#10b981'
  if (normalized.includes('cancel') || normalized.includes('reject') || normalized.includes('fail')) return '#ef4444'
  if (normalized.includes('pending') || normalized.includes('process')) return '#f59e0b'
  if (fallbackClass.includes('blue')) return '#1a57db'
  if (fallbackClass.includes('emerald') || fallbackClass.includes('green')) return '#10b981'
  if (fallbackClass.includes('red') || fallbackClass.includes('rose')) return '#ef4444'
  if (fallbackClass.includes('amber') || fallbackClass.includes('yellow')) return '#f59e0b'
  return '#64748b'
}

function KpiCard({
  icon,
  label,
  trend,
  trendLabel,
  value,
}: {
  icon: string
  label: string
  trend?: number
  trendLabel?: string
  value: string
}) {
  return (
    <div className="flex h-32 flex-col justify-between rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition hover:shadow-md">
      <div className="flex items-start justify-between">
        <p className="text-sm font-medium text-slate-500">{label}</p>
        <span className="material-symbols-outlined text-[20px] text-slate-400">{icon}</span>
      </div>
      <div>
        <h3 className="text-2xl font-bold text-slate-950">{value}</h3>
        <p className={`mt-1 flex items-center gap-1 text-sm font-medium ${trend === undefined ? 'text-emerald-600' : trendTone(trend)}`}>
          <span className="material-symbols-outlined text-[16px]">{trend === undefined ? 'trending_up' : trendIcon(trend)}</span>
          {trendLabel ?? formatGrowth(trend ?? 0)}
        </p>
      </div>
    </div>
  )
}

function RevenueOverview({ data }: { data: ChartDataPoint[] }) {
  const maxValue = Math.max(1, ...data.map((point) => Math.max(point.value, point.secondaryValue ?? 0)))
  const grossPoints = chartPoints(data, 'value', maxValue)
  const netPoints = chartPoints(data, 'secondaryValue', maxValue)
  const highest = [...data].sort((a, b) => b.value - a.value)[0]

  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <div className="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <h2 className="text-lg font-bold text-slate-950">Revenue Overview</h2>
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-2">
            <div className="size-3 rounded-full bg-primary" />
            <span className="text-sm text-slate-500">Gross Revenue</span>
          </div>
          <div className="flex items-center gap-2">
            <div className="size-3 rounded-full border border-dashed border-white bg-[#1ABC9C]" />
            <span className="text-sm text-slate-500">Net Revenue</span>
          </div>
        </div>
      </div>

      <div
        className="relative mb-6 h-64 w-full overflow-hidden rounded-lg"
        style={{
          backgroundImage:
            'linear-gradient(to right, #f1f5f9 1px, transparent 1px), linear-gradient(to bottom, #f1f5f9 1px, transparent 1px)',
          backgroundSize: '40px 40px',
        }}
      >
        {data.length > 0 ? (
          <>
            <svg className="size-full" preserveAspectRatio="none" viewBox="0 0 1000 256">
              <defs>
                <linearGradient id="revenueGradient" x1="0%" x2="0%" y1="0%" y2="100%">
                  <stop offset="0%" stopColor="#1A56DB" stopOpacity="0.55" />
                  <stop offset="100%" stopColor="#1A56DB" stopOpacity="0" />
                </linearGradient>
              </defs>
              <path d={areaPath(grossPoints)} fill="url(#revenueGradient)" opacity="0.24" stroke="none" />
              <polyline points={grossPoints} fill="none" stroke="#1A56DB" strokeWidth="3" />
              <polyline points={netPoints} fill="none" stroke="#1ABC9C" strokeDasharray="5,5" strokeWidth="2" />
            </svg>
            <div className="absolute inset-x-0 bottom-2 flex justify-between px-4 text-[10px] text-slate-400">
              {sampledLabels(data).map((point) => (
                <span key={`${point.label}-${point.value}`}>{point.label}</span>
              ))}
            </div>
          </>
        ) : (
          <div className="flex size-full items-center justify-center text-slate-400">No data available for this period</div>
        )}
      </div>

      <div className="grid gap-5 border-t border-slate-100 pt-4 md:grid-cols-3">
        <div>
          <p className="text-sm text-slate-500">Average Daily Revenue</p>
          <p className="mt-1 text-xl font-bold text-slate-950">
            {formatCurrency(data.length ? data.reduce((sum, point) => sum + point.value, 0) / data.length : 0)}/day
          </p>
        </div>
        <div>
          <p className="text-sm text-slate-500">Highest Revenue Day</p>
          <p className="mt-1 text-xl font-bold text-slate-950">
            {highest?.label ?? 'N/A'} <span className="text-sm font-normal text-slate-500">({formatCurrency(highest?.value ?? 0)})</span>
          </p>
        </div>
        <div>
          <p className="text-sm text-slate-500">Net Revenue</p>
          <p className="mt-1 text-xl font-bold text-slate-950">
            {formatCurrency(data.reduce((sum, point) => sum + (point.secondaryValue ?? 0), 0))}
          </p>
        </div>
      </div>
    </section>
  )
}

function RevenueByCategory({ data }: { data: CategoryRevenueDto[] }) {
  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 text-sm shadow-sm lg:col-span-3">
      <h2 className="mb-6 text-lg font-bold text-slate-950">Revenue by Category</h2>
      <div className="space-y-4">
        {data.length > 0 ? (
          data.map((category) => (
            <div key={category.categoryName} className="flex items-center gap-4">
              <span className="w-32 truncate text-sm font-medium text-slate-600" title={category.categoryName}>
                {category.categoryName}
              </span>
              <div className="h-3 flex-1 overflow-hidden rounded-full bg-slate-100">
                <div className="h-full rounded-full bg-primary transition-all duration-700" style={{ width: `${category.percentage}%` }} />
              </div>
              <span className="w-24 text-right text-sm font-bold">{formatCurrency(category.revenue)}</span>
            </div>
          ))
        ) : (
          <div className="py-10 text-center text-slate-400">No category data available</div>
        )}
      </div>
    </section>
  )
}

function OrderStatusBreakdown({ data, totalOrders }: { data: OrderStatusBreakdownDto[]; totalOrders: number }) {
  const segments = data.reduce<{ parts: string[]; current: number }>(
    (result, item) => {
      const start = result.current
      const end = start + item.percentage
      return {
        current: end,
        parts: [...result.parts, `${statusColor(item.status, item.colorClass)} ${start}% ${end}%`],
      }
    },
    { current: 0, parts: [] },
  ).parts

  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 text-sm shadow-sm lg:col-span-2">
      <h2 className="mb-2 text-lg font-bold text-slate-950">Order Status Breakdown</h2>
      <div className="relative flex items-center justify-center py-6">
        <div
          className="flex size-48 items-center justify-center rounded-full"
          style={{ background: segments.length ? `conic-gradient(${segments.join(', ')})` : undefined }}
        >
          <div className="flex size-36 items-center justify-center rounded-full bg-white text-center shadow-[inset_0_0_0_1px_rgba(226,232,240,0.8)]">
            <div>
              <span className="block text-3xl font-bold text-slate-950">{formatNumber(totalOrders)}</span>
              <span className="text-xs text-slate-500">Total Orders</span>
            </div>
          </div>
        </div>
      </div>
      <div className="mt-2 grid grid-cols-2 gap-4 text-xs">
        {data.map((item) => (
          <div key={item.status} className="flex items-center gap-2">
            <div className="size-3 rounded-full" style={{ backgroundColor: statusColor(item.status, item.colorClass) }} />
            <span className="text-slate-600">
              {item.status} ({item.percentage.toFixed(1)}%)
            </span>
          </div>
        ))}
      </div>
    </section>
  )
}

function TopStoresTable({ stores }: { stores: DashboardStoreDto[] }) {
  return (
    <section className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <div className="flex items-center justify-between border-b border-slate-200 px-6 py-5">
        <h2 className="text-lg font-bold text-slate-950">Top 10 Stores by Revenue</h2>
        <Link className="text-sm font-medium text-primary hover:underline" to="/admin/stores">
          View All Stores
        </Link>
      </div>
      <div className="overflow-x-auto text-sm">
        <table className="w-full min-w-[820px] text-left">
          <thead className="border-b border-slate-200 bg-slate-50 font-medium text-slate-600">
            <tr>
              <th className="w-16 px-6 py-3 text-center">#</th>
              <th className="px-6 py-3">Store Name</th>
              <th className="px-6 py-3 text-right">Revenue</th>
              <th className="px-6 py-3 text-right">Orders</th>
              <th className="px-6 py-3 text-center">Rating</th>
              <th className="px-6 py-3 text-right">Commission</th>
              <th className="px-6 py-3 text-center">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {stores.length > 0 ? (
              stores.map((store, index) => (
                <tr key={store.storeId} className="transition hover:bg-slate-50">
                  <td className="px-6 py-4 text-center font-medium text-slate-500">{index + 1}</td>
                  <td className="flex items-center gap-3 px-6 py-4 font-semibold text-slate-950">
                    {store.logoUrl ? (
                      <img src={store.logoUrl} className="size-8 rounded-full border border-slate-100 object-cover" alt={store.storeName} />
                    ) : (
                      <div className="flex size-8 items-center justify-center rounded-full bg-blue-100 text-xs font-bold text-blue-600">
                        {store.storeName.slice(0, 2).toUpperCase()}
                      </div>
                    )}
                    {store.storeName}
                  </td>
                  <td className="px-6 py-4 text-right font-medium">{formatCurrency(store.revenue)}</td>
                  <td className="px-6 py-4 text-right text-slate-600">{formatNumber(store.orders)}</td>
                  <td className="px-6 py-4 text-center">
                    <span className="inline-flex items-center gap-1 rounded-full border border-amber-100 bg-amber-50 px-2 py-1 text-xs font-semibold text-amber-700">
                      <span className="material-symbols-outlined text-[14px]">star</span>
                      {store.rating.toFixed(1)}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right text-slate-600">{formatCurrency(store.commission)}</td>
                  <td className="px-6 py-4 text-center">
                    <span className={`inline-block size-2 rounded-full ${store.status === 'Approved' ? 'bg-emerald-500' : 'bg-slate-400'}`} title={store.status} />
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={7} className="px-6 py-8 text-center text-slate-400">
                  No store data available
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </section>
  )
}

function UserGrowth({ data }: { data: ChartDataPoint[] }) {
  const maxDaily = Math.max(1, ...data.map((point) => point.value))
  const maxTotal = Math.max(1, ...data.map((point) => point.secondaryValue ?? 0))
  const points = chartPoints(data, 'secondaryValue', maxTotal, 200)
  const lastPoint = points.split(' ').filter(Boolean).at(-1)
  const [lastX, lastY] = lastPoint?.split(',') ?? ['1000', '200']

  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <h2 className="mb-4 text-lg font-bold text-slate-950">User Growth</h2>
      <div className="relative flex h-64 w-full items-end justify-between gap-2 px-2">
        {data.length > 0 ? (
          <>
            <div className="absolute inset-0 z-0 flex items-end justify-between gap-2 px-2 opacity-20">
              {data.map((point) => (
                <div
                  key={`${point.label}-${point.value}`}
                  className="flex-1 rounded-t-sm bg-primary"
                  style={{ height: `${(point.value / maxDaily) * 100}%` }}
                />
              ))}
            </div>
            <svg className="absolute inset-0 z-10 size-full overflow-visible" preserveAspectRatio="none" viewBox="0 0 1000 200">
              <polyline points={points} fill="none" stroke="#1ABC9C" strokeWidth="3" />
              <circle cx={lastX} cy={lastY} fill="#1ABC9C" r="4" />
            </svg>
            <div className="absolute right-0 top-0 flex gap-4 text-[10px] text-slate-600">
              <div className="flex items-center gap-1">
                <div className="size-3 bg-primary/20" />
                <span>New Users (Daily)</span>
              </div>
              <div className="flex items-center gap-1">
                <div className="h-1 w-3 bg-[#1ABC9C]" />
                <span>Total Users (Cumulative)</span>
              </div>
            </div>
          </>
        ) : (
          <div className="flex size-full items-center justify-center text-slate-400">No user data available</div>
        )}
      </div>
    </section>
  )
}

function DisputeResolution({ totalOrders }: { totalOrders: number }) {
  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="text-lg font-bold text-slate-950">Dispute &amp; Resolution</h2>
        <span className="text-sm text-slate-500">Avg Resolution: 24h</span>
      </div>
      <div className="space-y-6">
        <div className="mb-4 grid grid-cols-3 gap-4">
          <div className="rounded-lg border border-red-100 bg-red-50 p-3 text-center">
            <p className="text-xs font-semibold uppercase text-red-600">Open</p>
            <p className="text-xl font-bold text-slate-950">{Math.floor(totalOrders / 500)}</p>
          </div>
          <div className="rounded-lg border border-amber-100 bg-amber-50 p-3 text-center">
            <p className="text-xs font-semibold uppercase text-amber-600">Review</p>
            <p className="text-xl font-bold text-slate-950">{Math.floor(totalOrders / 800)}</p>
          </div>
          <div className="rounded-lg border border-emerald-100 bg-emerald-50 p-3 text-center">
            <p className="text-xs font-semibold uppercase text-emerald-600">Resolved</p>
            <p className="text-xl font-bold text-slate-950">{Math.floor(totalOrders / 100)}</p>
          </div>
        </div>

        <div>
          <p className="mb-2 text-sm font-medium text-slate-700">Dispute Reasons (Prototype)</p>
          <div className="space-y-3">
            {[
              ['Item not as described', 42],
              ['Shipping damage', 28],
              ['Missing parts', 15],
            ].map(([reason, percent]) => (
              <div key={reason}>
                <div className="mb-1 flex justify-between text-xs">
                  <span>{reason}</span>
                  <span className="font-medium">{percent}%</span>
                </div>
                <div className="h-2 w-full overflow-hidden rounded-full bg-slate-100">
                  <div className="h-full bg-red-400" style={{ width: `${percent}%` }} />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}

function SkeletonDashboard() {
  return (
    <div className="space-y-6">
      <div className="h-9 w-40 animate-pulse rounded-lg bg-slate-200" />
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-5">
        {Array.from({ length: 5 }).map((_, index) => (
          <div key={index} className="h-32 animate-pulse rounded-xl bg-slate-200" />
        ))}
      </div>
      <div className="h-96 animate-pulse rounded-xl bg-slate-200" />
    </div>
  )
}

function DashboardContent({ data }: { data: AdminDashboardDto }) {
  const kpis: DashboardKpis = data.kpiCards

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-5">
        <KpiCard icon="payments" label="Gross Revenue" trend={kpis.revenueGrowth} value={formatCurrency(kpis.grossRevenue)} />
        <KpiCard icon="shopping_bag" label="Total Orders" trend={kpis.orderGrowth} value={formatNumber(kpis.totalOrders)} />
        <KpiCard icon="store" label="Active Stores" trendLabel="Healthy" value={formatNumber(kpis.activeStores)} />
        <KpiCard icon="person_add" label="New Users" trend={kpis.userGrowth} value={formatNumber(kpis.newUsers)} />
        <KpiCard icon="gavel" label="Dispute Rate" trendLabel="-0.3%" value={`${kpis.disputeRate.toFixed(1)}%`} />
      </div>

      <RevenueOverview data={data.revenueOverview} />

      <div className="grid grid-cols-1 gap-6 lg:grid-cols-5">
        <RevenueByCategory data={data.revenueByCategory} />
        <OrderStatusBreakdown data={data.orderStatusBreakdown} totalOrders={kpis.totalOrders} />
      </div>

      <TopStoresTable stores={data.topStores} />

      <div className="grid grid-cols-1 gap-6 pb-6 text-sm lg:grid-cols-2">
        <UserGrowth data={data.userGrowth} />
        <DisputeResolution totalOrders={kpis.totalOrders} />
      </div>
    </div>
  )
}

export default function AdminDashboardPage() {
  const [dashboard, setDashboard] = useState<AdminDashboardDto | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)
  const [period, setPeriod] = useState('month')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')

  const selectedPeriodLabel = useMemo(() => periodOptions.find((option) => option.value === period)?.label ?? 'This Month', [period])

  const fetchDashboard = async (query: DashboardQueryParams) => {
    setLoading(true)
    setError('')

    try {
      const data = await adminApi.getDashboard(query)
      setDashboard(data)
    } catch (err) {
      setDashboard(null)
      setError(err instanceof Error ? err.message : 'Unable to load dashboard data.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void fetchDashboard({ period: 'month' })
  }, [])

  const handlePeriodChange = (value: string) => {
    setPeriod(value)
    if (value !== 'custom') {
      void fetchDashboard({ period: value })
    }
  }

  const handleCustomApply = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void fetchDashboard({ endDate, period: 'custom', startDate })
  }

  return (
    <AdminLayout activePage="Overview" breadcrumb={['Dashboard']} pageHeader="Platform Overview">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <form onSubmit={handleCustomApply} className="flex flex-wrap items-center gap-3">
          <div className="relative">
            <select
              value={period}
              onChange={(event) => handlePeriodChange(event.target.value)}
              className="w-44 cursor-pointer appearance-none rounded-lg border border-slate-200 bg-white py-2 pl-10 pr-8 text-sm font-medium text-slate-700 shadow-sm transition hover:bg-slate-50 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary"
            >
              {periodOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
            <span className="material-symbols-outlined pointer-events-none absolute left-3 top-2.5 text-[18px] text-primary">calendar_today</span>
            <span className="material-symbols-outlined pointer-events-none absolute right-2 top-2.5 text-[20px] text-slate-400">expand_more</span>
          </div>

          {period === 'custom' && (
            <div className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-3 py-1.5 shadow-sm">
              <span className="text-xs font-medium text-slate-400">From</span>
              <input
                type="date"
                value={startDate}
                onChange={(event) => setStartDate(event.target.value)}
                className="w-32 cursor-pointer border-0 bg-transparent text-sm text-slate-700 outline-none"
              />
              <span className="text-base text-slate-300">-&gt;</span>
              <span className="text-xs font-medium text-slate-400">To</span>
              <input
                type="date"
                value={endDate}
                onChange={(event) => setEndDate(event.target.value)}
                className="w-32 cursor-pointer border-0 bg-transparent text-sm text-slate-700 outline-none"
              />
              <button type="submit" className="ml-1 rounded-md bg-primary px-3 py-1 text-xs font-semibold text-white transition hover:bg-blue-700">
                Apply
              </button>
            </div>
          )}
        </form>

        <div className="flex items-center gap-2">
          <button
            type="button"
            title="Export PDF is a visual placeholder in the migrated dashboard."
            className="flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-600 shadow-sm transition hover:bg-slate-50"
          >
            <span className="material-symbols-outlined text-[18px] text-slate-500">picture_as_pdf</span>
            Export PDF
          </button>
          <button
            type="button"
            title="Export Excel is a visual placeholder in the migrated dashboard."
            className="flex items-center gap-1.5 rounded-lg bg-primary px-3 py-2 text-sm font-medium text-white shadow-sm shadow-blue-200 transition hover:bg-blue-700"
          >
            <span className="material-symbols-outlined text-[18px]">download</span>
            Export Excel
          </button>
        </div>
      </div>

      {error && (
        <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">
          Could not load dashboard for {selectedPeriodLabel}: {error}
        </div>
      )}

      {loading ? <SkeletonDashboard /> : dashboard ? <DashboardContent data={dashboard} /> : null}
    </AdminLayout>
  )
}
