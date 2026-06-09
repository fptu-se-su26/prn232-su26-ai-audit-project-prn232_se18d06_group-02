import { useEffect, useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import {
  adminApi,
  walletTransactionStatus,
  walletTransactionType,
  type AdminWalletDto,
  type PagedResult,
  type TopupWalletRequest,
  type WalletStatusLevel,
  type WalletTransactionDto,
  type WalletTransactionStatus,
  type WalletTransactionType,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const PAGE_SIZE = 10

const emptyWallet: AdminWalletDto = {
  cashFlow: [],
  summary: {
    availableBalance: null,
    availableBalanceRaw: null,
    isBalanceLive: false,
    nextBatchRequiredAmount: 0,
    pendingPayoutAmount: 0,
    statusLevel: 0,
  },
  transactions: {
    items: [],
    pageNumber: 1,
    pageSize: PAGE_SIZE,
    totalCount: 0,
    totalPages: 1,
  },
}

const emptyTopup: TopupWalletRequest = {
  amount: 0,
  note: '',
  providerTransactionId: '',
}

const currencyFormatter = new Intl.NumberFormat('vi-VN', {
  currency: 'VND',
  maximumFractionDigits: 0,
  style: 'currency',
})

const compactCurrencyFormatter = new Intl.NumberFormat('en-US', {
  maximumFractionDigits: 1,
  notation: 'compact',
})

function formatCurrency(value?: number | null) {
  return currencyFormatter.format(value ?? 0)
}

function formatCompactCurrency(value: number) {
  return compactCurrencyFormatter.format(value)
}

function formatDate(value?: string | null) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value))
}

function formatTime(value?: string | null) {
  if (!value) return ''
  return new Intl.DateTimeFormat('en-US', { hour: '2-digit', hour12: false, minute: '2-digit', second: '2-digit' }).format(new Date(value))
}

function normalizeType(type: WalletTransactionDto['type']) {
  if (type === walletTransactionType.topup || String(type).toLowerCase() === 'topup' || String(type).toLowerCase() === '0') return 'Topup'
  if (type === walletTransactionType.payout || String(type).toLowerCase() === 'payout' || String(type).toLowerCase() === '1') return 'Payout'
  if (type === walletTransactionType.refund || String(type).toLowerCase() === 'refund' || String(type).toLowerCase() === '2') return 'Refund'
  return 'Adjustment'
}

function normalizeStatus(status: WalletTransactionDto['status']) {
  if (status === walletTransactionStatus.pending || String(status).toLowerCase() === 'pending' || String(status).toLowerCase() === '0') return 'Pending'
  if (status === walletTransactionStatus.completed || String(status).toLowerCase() === 'completed' || String(status).toLowerCase() === '1') return 'Completed'
  if (status === walletTransactionStatus.failed || String(status).toLowerCase() === 'failed' || String(status).toLowerCase() === '2') return 'Failed'
  return 'Reversed'
}

function normalizeDirection(direction: WalletTransactionDto['direction']) {
  if (direction === 0 || String(direction).toUpperCase() === 'IN') return 'IN'
  return 'OUT'
}

function normalizeStatusLevel(level: WalletStatusLevel) {
  if (level === 2 || String(level).toLowerCase() === 'low') return 'Low'
  if (level === 1 || String(level).toLowerCase() === 'warning') return 'Warning'
  return 'Healthy'
}

function statusCopy(level: WalletStatusLevel) {
  const normalized = normalizeStatusLevel(level)
  if (normalized === 'Low') {
    return {
      border: 'border-red-100',
      desc: 'Top-up required before next payout',
      icon: 'notification_important',
      label: 'Low Balance',
      text: 'text-red-600',
      tone: 'bg-red-50 text-red-600 border-red-100',
    }
  }
  if (normalized === 'Warning') {
    return {
      border: 'border-amber-100',
      desc: 'Balance may be insufficient for pending payouts',
      icon: 'notification_important',
      label: 'Warning',
      text: 'text-amber-600',
      tone: 'bg-amber-50 text-amber-600 border-amber-100',
    }
  }
  return {
    border: 'border-emerald-100',
    desc: 'Balance is sufficient for upcoming payouts',
    icon: 'check_circle',
    label: 'Healthy',
    text: 'text-emerald-600',
    tone: 'bg-emerald-50 text-emerald-600 border-emerald-100',
  }
}

function typeBadgeClass(type: WalletTransactionDto['type']) {
  const normalized = normalizeType(type)
  if (normalized === 'Topup') return 'border-emerald-100 bg-emerald-50 text-emerald-700'
  if (normalized === 'Payout') return 'border-blue-100 bg-blue-50 text-blue-700'
  if (normalized === 'Refund') return 'border-red-100 bg-red-50 text-red-700'
  return 'border-slate-200 bg-slate-100 text-slate-600'
}

function statusBadgeClass(status: WalletTransactionDto['status']) {
  const normalized = normalizeStatus(status)
  if (normalized === 'Completed') return 'border-emerald-100 bg-emerald-50 text-emerald-700'
  if (normalized === 'Failed') return 'border-red-100 bg-red-50 text-red-700'
  if (normalized === 'Reversed') return 'border-slate-200 bg-slate-100 text-slate-500'
  return 'border-amber-100 bg-amber-50 text-amber-700'
}

function statusDotClass(status: WalletTransactionDto['status']) {
  const normalized = normalizeStatus(status)
  if (normalized === 'Completed') return 'bg-emerald-500'
  if (normalized === 'Failed') return 'bg-red-500'
  if (normalized === 'Reversed') return 'bg-slate-400'
  return 'bg-amber-500'
}

function monthKey(date: Date) {
  return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`
}

function buildMonthBuckets(cashFlow: WalletTransactionDto[]) {
  const now = new Date()
  const buckets = Array.from({ length: 6 }).map((_, index) => {
    const date = new Date(now.getFullYear(), now.getMonth() - (5 - index), 1)
    return {
      key: monthKey(date),
      label: `${String(date.getMonth() + 1).padStart(2, '0')}/${date.getFullYear()}`,
      longLabel: new Intl.DateTimeFormat('en-US', { month: 'long', year: 'numeric' }).format(date),
      moneyIn: 0,
      moneyOut: 0,
    }
  })

  const byKey = new Map(buckets.map((bucket, index) => [bucket.key, index]))

  cashFlow.forEach((tx) => {
    const date = new Date(tx.createdAt)
    if (Number.isNaN(date.getTime())) return

    const index = byKey.get(monthKey(date))
    if (index === undefined) return

    const type = normalizeType(tx.type)
    const direction = normalizeDirection(tx.direction)
    if (direction === 'IN' && type === 'Topup') buckets[index].moneyIn += tx.amount
    if (direction === 'OUT' && type === 'Payout') buckets[index].moneyOut += tx.amount
  })

  return buckets
}

function linePoints(values: number[], maxValue: number, height = 180) {
  const step = 1000 / Math.max(1, values.length - 1)
  return values
    .map((value, index) => {
      const x = index * step
      const y = height - (value / maxValue) * (height - 24) + 8
      return `${x.toFixed(2)},${y.toFixed(2)}`
    })
    .join(' ')
}

function StatCard({
  children,
  icon,
  label,
  tone,
}: {
  children: ReactNode
  icon: string
  label: string
  tone: string
}) {
  return (
    <div className="flex min-h-40 flex-col gap-4 rounded-xl border border-slate-100 bg-white p-5 shadow-sm">
      <div className="flex items-center justify-between">
        <div className={`flex size-12 items-center justify-center rounded-lg border ${tone}`}>
          <span className="material-symbols-outlined text-[26px]">{icon}</span>
        </div>
      </div>
      <div>
        <p className="mb-1 text-[11px] font-bold uppercase tracking-widest text-slate-500">{label}</p>
        {children}
      </div>
    </div>
  )
}

function FlowLineChart({ cashFlow }: { cashFlow: WalletTransactionDto[] }) {
  const buckets = useMemo(() => buildMonthBuckets(cashFlow), [cashFlow])
  const maxValue = Math.max(1, ...buckets.flatMap((bucket) => [bucket.moneyIn, bucket.moneyOut]))
  const inPoints = linePoints(buckets.map((bucket) => bucket.moneyIn), maxValue)
  const outPoints = linePoints(buckets.map((bucket) => bucket.moneyOut), maxValue)

  return (
    <section className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
      <div className="border-b border-slate-100 bg-slate-50/30 px-6 py-5">
        <h3 className="text-base font-bold text-slate-900">Wallet Balance History</h3>
        <p className="mt-0.5 text-xs font-medium text-slate-500">Money in & out, last 6 months</p>
      </div>
      <div className="p-6">
        <div className="relative h-[280px] rounded-lg bg-white">
          <svg className="size-full" preserveAspectRatio="none" viewBox="0 0 1000 220">
            <defs>
              <linearGradient id="walletInGradient" x1="0%" x2="0%" y1="0%" y2="100%">
                <stop offset="0%" stopColor="#10b981" stopOpacity="0.18" />
                <stop offset="100%" stopColor="#10b981" stopOpacity="0" />
              </linearGradient>
              <linearGradient id="walletOutGradient" x1="0%" x2="0%" y1="0%" y2="100%">
                <stop offset="0%" stopColor="#ef4444" stopOpacity="0.14" />
                <stop offset="100%" stopColor="#ef4444" stopOpacity="0" />
              </linearGradient>
            </defs>
            {[0, 1, 2, 3].map((line) => (
              <line key={line} x1="0" x2="1000" y1={24 + line * 48} y2={24 + line * 48} stroke="#f1f5f9" strokeWidth="1" />
            ))}
            <polyline points={inPoints} fill="none" stroke="#10b981" strokeWidth="3" />
            <polyline points={outPoints} fill="none" stroke="#ef4444" strokeWidth="3" />
          </svg>
          <div className="absolute left-0 right-0 top-0 flex justify-end gap-4 text-xs font-semibold text-slate-500">
            <span className="flex items-center gap-1.5">
              <span className="size-2.5 rounded-full bg-emerald-500" />
              Money In
            </span>
            <span className="flex items-center gap-1.5">
              <span className="size-2.5 rounded-full bg-red-500" />
              Money Out
            </span>
          </div>
          <div className="absolute inset-x-0 bottom-0 flex justify-between text-[10px] font-medium text-slate-400">
            {buckets.map((bucket) => (
              <span key={bucket.key}>{bucket.label}</span>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}

function MonthlyFlowChart({ cashFlow }: { cashFlow: WalletTransactionDto[] }) {
  const buckets = useMemo(() => buildMonthBuckets(cashFlow), [cashFlow])
  const [selectedKey, setSelectedKey] = useState(() => buckets.at(-1)?.key ?? '')
  const selected = buckets.find((bucket) => bucket.key === selectedKey) ?? buckets.at(-1) ?? buckets[0]
  const maxValue = Math.max(1, selected?.moneyIn ?? 0, selected?.moneyOut ?? 0)

  return (
    <section className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
      <div className="flex items-center justify-between gap-3 border-b border-slate-100 bg-slate-50/30 px-6 py-5">
        <div>
          <h3 className="text-base font-bold text-slate-900">Monthly Cash Flow</h3>
          <p className="mt-0.5 text-xs font-medium text-slate-500">Money in and money out by month</p>
        </div>
        <label className="flex items-center gap-2 text-xs font-semibold text-slate-500">
          Month
          <select
            value={selectedKey}
            onChange={(event) => setSelectedKey(event.target.value)}
            className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-xs font-semibold text-slate-700 outline-none focus:border-primary focus:ring-2 focus:ring-primary/20"
          >
            {buckets
              .slice()
              .reverse()
              .map((bucket) => (
                <option key={bucket.key} value={bucket.key}>
                  {bucket.longLabel}
                </option>
              ))}
          </select>
        </label>
      </div>
      <div className="flex flex-col items-center justify-center gap-6 p-6 md:flex-row md:gap-12">
        <div className="flex h-[280px] w-52 items-end justify-center gap-8 border-b border-slate-100 pb-3">
          {[
            { color: 'bg-emerald-500', label: 'Money In', value: selected?.moneyIn ?? 0 },
            { color: 'bg-red-500', label: 'Money Out', value: selected?.moneyOut ?? 0 },
          ].map((bar) => (
            <div key={bar.label} className="flex h-full w-16 flex-col items-center justify-end gap-2">
              <div className="text-[10px] font-bold text-slate-400">{formatCompactCurrency(bar.value)}</div>
              <div className={`w-12 rounded-t-md ${bar.color}`} style={{ height: `${Math.max(4, (bar.value / maxValue) * 220)}px` }} />
              <div className="text-center text-[10px] font-bold text-slate-500">{bar.label}</div>
            </div>
          ))}
        </div>
        <div className="flex flex-col gap-6 border-slate-50 md:border-l md:pl-10">
          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2.5">
              <span className="inline-block size-3 rounded-full bg-emerald-500" />
              <span className="text-[11px] font-bold uppercase tracking-wider text-slate-400">Money In</span>
            </div>
            <span className="ml-6 text-2xl font-black text-emerald-600">{formatCurrency(selected?.moneyIn ?? 0)}</span>
          </div>
          <div className="flex flex-col gap-1">
            <div className="flex items-center gap-2.5">
              <span className="inline-block size-3 rounded-full bg-red-500" />
              <span className="text-[11px] font-bold uppercase tracking-wider text-slate-400">Money Out</span>
            </div>
            <span className="ml-6 text-2xl font-black text-red-500">{formatCurrency(selected?.moneyOut ?? 0)}</span>
          </div>
        </div>
      </div>
    </section>
  )
}

function ModalShell({ children, onClose }: { children: ReactNode; onClose: () => void }) {
  return (
    <div className="fixed inset-0 z-[70]">
      <button type="button" aria-label="Close modal" className="absolute inset-0 bg-slate-900/60 backdrop-blur-sm" onClick={onClose} />
      <div className="absolute inset-0 flex items-center justify-center p-4">
        <div className="relative w-full max-w-lg rounded-2xl bg-white p-8 shadow-2xl">{children}</div>
      </div>
    </div>
  )
}

function TransactionsTable({
  loading,
  onPageChange,
  page,
  transactions,
}: {
  loading: boolean
  onPageChange: (page: number) => void
  page: number
  transactions: PagedResult<WalletTransactionDto>
}) {
  const pageCount = transactions.totalPages || Math.max(1, Math.ceil(transactions.totalCount / transactions.pageSize))
  const start = transactions.totalCount === 0 ? 0 : (transactions.pageNumber - 1) * transactions.pageSize + 1
  const end = Math.min(transactions.pageNumber * transactions.pageSize, transactions.totalCount)

  return (
    <div className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
      <div className="overflow-x-auto">
        <table className="w-full min-w-[980px] border-collapse text-left">
          <thead>
            <tr className="border-b border-slate-100 bg-slate-50/50 text-slate-500">
              <th className="px-5 py-3.5 text-[10px] font-bold uppercase tracking-widest">Code</th>
              <th className="px-5 py-3.5 text-[10px] font-bold uppercase tracking-widest">Date & Time</th>
              <th className="px-5 py-3.5 text-[10px] font-bold uppercase tracking-widest">Type</th>
              <th className="px-5 py-3.5 text-[10px] font-bold uppercase tracking-widest">Reference</th>
              <th className="px-5 py-3.5 text-[10px] font-bold uppercase tracking-widest">Description / Note</th>
              <th className="px-5 py-3.5 text-right text-[10px] font-bold uppercase tracking-widest">Amount</th>
              <th className="px-5 py-3.5 text-right text-[10px] font-bold uppercase tracking-widest">Balance After</th>
              <th className="px-5 py-3.5 text-center text-[10px] font-bold uppercase tracking-widest">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-50 text-sm">
            {loading ? (
              Array.from({ length: 5 }).map((_, index) => (
                <tr key={index} className="animate-pulse">
                  <td colSpan={8} className="px-5 py-4">
                    <div className="h-12 rounded-lg bg-slate-100" />
                  </td>
                </tr>
              ))
            ) : transactions.items.length ? (
              transactions.items.map((tx) => {
                const direction = normalizeDirection(tx.direction)
                const reference = tx.payoutBatchCode || tx.referenceCode

                return (
                  <tr key={tx.id} className="transition-colors hover:bg-slate-50/50">
                    <td className="px-5 py-4 font-mono text-xs font-bold text-primary">{tx.transactionCode}</td>
                    <td className="whitespace-nowrap px-5 py-4 text-xs font-medium text-slate-500">
                      <div>{formatDate(tx.createdAt)}</div>
                      <div className="text-slate-400">{formatTime(tx.createdAt)}</div>
                    </td>
                    <td className="px-5 py-4">
                      <span className={`rounded-md border px-2.5 py-1 text-[10px] font-black uppercase ${typeBadgeClass(tx.type)}`}>
                        {normalizeType(tx.type)}
                      </span>
                    </td>
                    <td className="px-5 py-4">
                      {reference ? (
                        <span className="rounded-lg bg-blue-50/70 px-2.5 py-1 text-xs font-bold text-primary">{reference}</span>
                      ) : (
                        <span className="text-slate-300">-</span>
                      )}
                    </td>
                    <td className="max-w-[220px] truncate px-5 py-4 text-xs font-medium italic text-slate-600" title={tx.note ?? undefined}>
                      {tx.note || '-'}
                    </td>
                    <td className={`whitespace-nowrap px-5 py-4 text-right font-black ${direction === 'IN' ? 'text-emerald-600' : 'text-red-600'}`}>
                      {direction === 'IN' ? '+' : '-'}
                      {formatCurrency(tx.amount)}
                    </td>
                    <td className="whitespace-nowrap px-5 py-4 text-right font-bold text-slate-700">{formatCurrency(tx.balanceAfter)}</td>
                    <td className="px-5 py-4 text-center">
                      <span className={`inline-flex items-center gap-1.5 rounded-full border px-2.5 py-1 text-[10px] font-black uppercase ${statusBadgeClass(tx.status)}`}>
                        <span className={`size-1.5 rounded-full ${statusDotClass(tx.status)}`} />
                        {normalizeStatus(tx.status)}
                      </span>
                    </td>
                  </tr>
                )
              })
            ) : (
              <tr>
                <td colSpan={8} className="py-20 text-center">
                  <div className="flex flex-col items-center gap-3 text-slate-400">
                    <span className="material-symbols-outlined text-[48px] text-slate-200">account_balance_wallet</span>
                    <p className="text-base font-semibold">No transactions found</p>
                    <p className="text-xs">Try adjusting your search or filter criteria</p>
                  </div>
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="flex flex-col items-center justify-between gap-4 border-t border-slate-100 bg-slate-50/30 px-6 py-4 sm:flex-row">
        <div className="text-sm text-slate-500">
          Showing <span className="font-medium text-slate-900">{start}</span> to <span className="font-medium text-slate-900">{end}</span> of{' '}
          <span className="font-medium text-slate-900">{transactions.totalCount}</span> transactions
        </div>
        <nav className="flex items-center gap-1" aria-label="Wallet pagination">
          <button
            type="button"
            disabled={page <= 1 || loading}
            onClick={() => onPageChange(page - 1)}
            className="rounded-lg border border-slate-200 bg-white p-2 text-slate-400 transition-all hover:text-primary disabled:cursor-not-allowed disabled:opacity-50"
          >
            <span className="material-symbols-outlined text-[20px]">chevron_left</span>
          </button>
          {Array.from({ length: Math.min(5, pageCount) }).map((_, index) => {
            const first = Math.max(1, Math.min(page - 2, pageCount - 4))
            const nextPage = first + index
            if (nextPage > pageCount) return null
            return (
              <button
                key={nextPage}
                type="button"
                onClick={() => onPageChange(nextPage)}
                className={`flex h-9 min-w-9 items-center justify-center rounded-lg text-sm font-bold transition-all ${
                  nextPage === page ? 'bg-primary text-white shadow-sm' : 'border border-transparent text-slate-600 hover:border-slate-200 hover:bg-white hover:text-primary'
                }`}
              >
                {nextPage}
              </button>
            )
          })}
          <button
            type="button"
            disabled={page >= pageCount || loading}
            onClick={() => onPageChange(page + 1)}
            className="rounded-lg border border-slate-200 bg-white p-2 text-slate-400 transition-all hover:text-primary disabled:cursor-not-allowed disabled:opacity-50"
          >
            <span className="material-symbols-outlined text-[20px]">chevron_right</span>
          </button>
        </nav>
      </div>
    </div>
  )
}

export default function AdminWalletPage() {
  const [wallet, setWallet] = useState<AdminWalletDto>(emptyWallet)
  const [search, setSearch] = useState('')
  const [type, setType] = useState<WalletTransactionType | ''>('')
  const [status, setStatus] = useState<WalletTransactionStatus | ''>('')
  const [pageNumber, setPageNumber] = useState(1)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [isTopupOpen, setIsTopupOpen] = useState(false)
  const [topup, setTopup] = useState<TopupWalletRequest>(emptyTopup)
  const [validationError, setValidationError] = useState('')

  const summaryStatus = statusCopy(wallet.summary.statusLevel)

  const loadWallet = async (
    nextPage = pageNumber,
    overrides?: {
      search?: string
      status?: WalletTransactionStatus | ''
      type?: WalletTransactionType | ''
    },
  ) => {
    setLoading(true)
    setError('')

    try {
      const data = await adminApi.wallet.get({
        pageNumber: nextPage,
        pageSize: PAGE_SIZE,
        search: (overrides?.search ?? search).trim() || undefined,
        status: overrides?.status ?? status,
        type: overrides?.type ?? type,
      })
      setWallet(data)
      setPageNumber(data.transactions.pageNumber)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to load wallet data.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    const loadInitialWallet = async () => {
      setLoading(true)
      setError('')

      try {
        const data = await adminApi.wallet.get({ pageNumber: 1, pageSize: PAGE_SIZE })
        setWallet(data)
        setPageNumber(data.transactions.pageNumber)
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unable to load wallet data.')
      } finally {
        setLoading(false)
      }
    }

    void loadInitialWallet()
  }, [])

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadWallet(1)
  }

  const handleReset = () => {
    setSearch('')
    setType('')
    setStatus('')
    void loadWallet(1, { search: '', status: '', type: '' })
  }

  const handleTopupSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setValidationError('')
    setError('')
    setSuccess('')

    if (topup.amount <= 0) {
      setValidationError('Amount must be greater than 0.')
      return
    }

    if (!topup.providerTransactionId.trim()) {
      setValidationError('Provider Transaction ID is required.')
      return
    }

    setSaving(true)
    try {
      const message = await adminApi.wallet.topUp({
        amount: topup.amount,
        note: topup.note?.trim() || undefined,
        providerTransactionId: topup.providerTransactionId.trim(),
      })
      setSuccess(message)
      setIsTopupOpen(false)
      setTopup(emptyTopup)
      await loadWallet(1)
    } catch (err) {
      setValidationError(err instanceof Error ? err.message : 'Failed to record top-up.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <AdminLayout activePage="Wallet" breadcrumb={['Finance', 'Wallet']} pageHeader="Wallet Management">
      <div className="flex flex-col gap-6 pb-10">
        <div className="flex justify-end gap-3">
          <button
            type="button"
            onClick={() => void loadWallet(pageNumber)}
            disabled={loading}
            className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-600 shadow-sm transition-all hover:bg-slate-50 disabled:opacity-60"
          >
            <span className="material-symbols-outlined text-[20px]">refresh</span>
            Refresh Balance
          </button>
          <button
            type="button"
            onClick={() => setIsTopupOpen(true)}
            className="flex items-center gap-2 rounded-lg bg-primary px-5 py-2.5 text-sm font-bold text-white shadow-sm transition-all hover:bg-blue-700"
          >
            <span className="material-symbols-outlined text-[20px]">add_circle</span>
            Topup Wallet
          </button>
        </div>

        {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}
        {success && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">{success}</div>}

        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-4">
          <StatCard icon="account_balance_wallet" label="Available Balance" tone="border-blue-100 bg-blue-50 text-blue-600">
            <div className="mb-2 flex items-center gap-2">
              <h3 className="text-2xl font-black leading-tight text-slate-900">
                {wallet.summary.isBalanceLive && wallet.summary.availableBalanceRaw !== null ? formatCurrency(wallet.summary.availableBalanceRaw) : <span className="text-slate-400">N/A</span>}
              </h3>
              <span
                className={`rounded-full border px-2.5 py-1 text-[10px] font-bold uppercase tracking-wider ${
                  wallet.summary.isBalanceLive ? 'border-emerald-100 bg-emerald-50 text-emerald-600' : 'border-slate-200 bg-slate-50 text-slate-400'
                }`}
              >
                {wallet.summary.isBalanceLive ? 'Live' : 'Offline'}
              </span>
            </div>
            <p className="text-[11px] font-medium text-slate-400">Current BaoKim wallet balance</p>
          </StatCard>

          <StatCard icon="schedule_send" label="Pending Payout" tone="border-amber-100 bg-amber-50 text-amber-600">
            <h3 className="text-2xl font-black leading-tight text-slate-900">{formatCurrency(wallet.summary.pendingPayoutAmount)}</h3>
            <p className="mt-2 text-[11px] font-medium text-slate-400">Approved/Processing batches</p>
          </StatCard>

          <StatCard icon="account_balance" label="Next Batch Required" tone="border-purple-100 bg-purple-50 text-purple-600">
            <h3 className="text-2xl font-black leading-tight text-slate-900">{formatCurrency(wallet.summary.nextBatchRequiredAmount)}</h3>
            <p className="mt-2 text-[11px] font-medium text-slate-400">Pending approval batches</p>
          </StatCard>

          <div className={`relative flex min-h-40 flex-col gap-4 overflow-hidden rounded-xl border-2 bg-white p-5 shadow-sm ${summaryStatus.border}`}>
            <div className="flex items-center justify-between">
              <div className={`flex size-12 items-center justify-center rounded-lg border ${summaryStatus.tone}`}>
                <span className="material-symbols-outlined text-[26px]">{summaryStatus.icon}</span>
              </div>
              {normalizeStatusLevel(wallet.summary.statusLevel) !== 'Healthy' && (
                <span className="relative flex size-2">
                  <span className={`absolute inline-flex size-full animate-ping rounded-full opacity-75 ${normalizeStatusLevel(wallet.summary.statusLevel) === 'Low' ? 'bg-red-400' : 'bg-amber-400'}`} />
                  <span className={`relative inline-flex size-2 rounded-full ${normalizeStatusLevel(wallet.summary.statusLevel) === 'Low' ? 'bg-red-500' : 'bg-amber-500'}`} />
                </span>
              )}
            </div>
            <div>
              <p className="mb-1 text-[11px] font-bold uppercase tracking-widest text-slate-500">Wallet Status</p>
              <h3 className={`text-2xl font-black leading-tight ${summaryStatus.text}`}>{summaryStatus.label}</h3>
              <p className={`mt-2 text-[11px] font-medium ${summaryStatus.text}`}>{summaryStatus.desc}</p>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
          <FlowLineChart cashFlow={wallet.cashFlow} />
          <MonthlyFlowChart cashFlow={wallet.cashFlow} />
        </div>

        <div className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <div className="border-b border-slate-100 bg-slate-50/50 p-5">
            <div className="mb-4 flex items-center justify-between">
              <h3 className="flex items-center gap-2 text-base font-bold text-slate-900">
                <span className="material-symbols-outlined text-[22px] text-primary">history</span>
                Wallet Transactions
                <span className="ml-2 rounded-full bg-slate-100 px-2 py-0.5 text-xs font-bold text-slate-500">{wallet.transactions.totalCount}</span>
              </h3>
            </div>

            <form onSubmit={handleSearch} className="grid grid-cols-1 gap-3 md:grid-cols-2 lg:grid-cols-5">
              <div className="relative lg:col-span-2">
                <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-slate-400">
                  <span className="material-symbols-outlined text-[20px]">search</span>
                </span>
                <input
                  value={search}
                  onChange={(event) => setSearch(event.target.value)}
                  placeholder="Search by code, reference, or note..."
                  className="block w-full rounded-lg border border-slate-200 bg-white py-2 pl-10 pr-4 text-sm font-medium text-slate-900 outline-none transition-all placeholder:text-slate-400 focus:border-primary focus:ring-2 focus:ring-primary/20"
                />
              </div>
              <select
                value={type}
                onChange={(event) => {
                  const nextType = event.target.value === '' ? '' : (Number(event.target.value) as WalletTransactionType)
                  setType(nextType)
                  void loadWallet(1, { type: nextType })
                }}
                className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 outline-none transition-all focus:border-primary focus:ring-2 focus:ring-primary/20"
              >
                <option value="">All Types</option>
                <option value={walletTransactionType.topup}>Topup</option>
                <option value={walletTransactionType.payout}>Payout</option>
                <option value={walletTransactionType.refund}>Refund</option>
                <option value={walletTransactionType.adjustment}>Adjustment</option>
              </select>
              <select
                value={status}
                onChange={(event) => {
                  const nextStatus = event.target.value === '' ? '' : (Number(event.target.value) as WalletTransactionStatus)
                  setStatus(nextStatus)
                  void loadWallet(1, { status: nextStatus })
                }}
                className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm font-medium text-slate-700 outline-none transition-all focus:border-primary focus:ring-2 focus:ring-primary/20"
              >
                <option value="">All Statuses</option>
                <option value={walletTransactionStatus.pending}>Pending</option>
                <option value={walletTransactionStatus.completed}>Completed</option>
                <option value={walletTransactionStatus.failed}>Failed</option>
                <option value={walletTransactionStatus.reversed}>Reversed</option>
              </select>
              <div className="flex gap-2">
                <button type="submit" className="flex flex-1 items-center justify-center gap-2 rounded-lg bg-primary px-4 py-2 text-sm font-bold text-white shadow-sm transition-all hover:bg-blue-700">
                  <span className="material-symbols-outlined text-[18px]">search</span>
                  Apply
                </button>
                <button
                  type="button"
                  onClick={handleReset}
                  className="flex items-center justify-center rounded-lg border border-slate-200 bg-white p-2 text-slate-500 shadow-sm transition-colors hover:bg-slate-50"
                  title="Reset filters"
                >
                  <span className="material-symbols-outlined text-[18px]">restart_alt</span>
                </button>
              </div>
            </form>
          </div>

          <TransactionsTable loading={loading} onPageChange={(nextPage) => void loadWallet(nextPage)} page={pageNumber} transactions={wallet.transactions} />
        </div>
      </div>

      {isTopupOpen && (
        <ModalShell
          onClose={() => {
            if (saving) return
            setIsTopupOpen(false)
            setValidationError('')
          }}
        >
          <form onSubmit={handleTopupSubmit}>
            <div className="mb-6 flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="flex size-10 items-center justify-center rounded-xl border border-blue-100 bg-blue-50 text-blue-600">
                  <span className="material-symbols-outlined text-[22px]">account_balance_wallet</span>
                </div>
                <h3 className="text-xl font-black text-slate-900">Topup Platform Wallet</h3>
              </div>
              <button
                type="button"
                onClick={() => setIsTopupOpen(false)}
                className="rounded-full p-2 text-slate-400 transition-colors hover:bg-slate-100 hover:text-slate-600"
              >
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>

            <div className="space-y-5">
              <label className="block space-y-1.5">
                <span className="block pl-1 text-[11px] font-black uppercase tracking-widest text-slate-500">Amount (VND) *</span>
                <div className="relative">
                  <span className="absolute inset-y-0 left-0 flex items-center pl-3.5 font-bold text-slate-400">VND</span>
                  <input
                    min={1}
                    step={1000}
                    type="number"
                    value={topup.amount || ''}
                    onChange={(event) => setTopup((current) => ({ ...current, amount: Number(event.target.value) }))}
                    className="block w-full rounded-xl border border-slate-200 bg-slate-50 py-3 pl-12 pr-4 text-lg font-black text-slate-900 outline-none transition-all placeholder:text-slate-300 focus:border-primary focus:bg-white focus:ring-2 focus:ring-primary/20"
                    placeholder="0"
                  />
                </div>
              </label>

              <label className="block space-y-1.5">
                <span className="block pl-1 text-[11px] font-black uppercase tracking-widest text-slate-500">Provider Transaction ID *</span>
                <input
                  value={topup.providerTransactionId}
                  onChange={(event) => setTopup((current) => ({ ...current, providerTransactionId: event.target.value }))}
                  className="block w-full rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 font-mono text-sm font-semibold text-slate-900 outline-none transition-all focus:border-primary focus:bg-white focus:ring-2 focus:ring-primary/20"
                  placeholder="e.g. BK12345678"
                />
              </label>

              <label className="block space-y-1.5">
                <span className="block pl-1 text-[11px] font-black uppercase tracking-widest text-slate-500">Note (Optional)</span>
                <textarea
                  rows={3}
                  value={topup.note ?? ''}
                  onChange={(event) => setTopup((current) => ({ ...current, note: event.target.value }))}
                  className="block w-full resize-none rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm font-medium text-slate-900 outline-none transition-all focus:border-primary focus:bg-white focus:ring-2 focus:ring-primary/20"
                  placeholder="Add details about this top-up..."
                />
              </label>

              {validationError && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{validationError}</div>}
            </div>

            <div className="mt-8 flex gap-3">
              <button
                type="button"
                onClick={() => setIsTopupOpen(false)}
                disabled={saving}
                className="flex-1 rounded-xl border border-slate-200 bg-white px-4 py-3 font-bold text-slate-600 transition-all hover:bg-slate-50 disabled:opacity-60"
              >
                Cancel
              </button>
              <button type="submit" disabled={saving} className="flex-1 rounded-xl bg-primary px-4 py-3 font-black text-white shadow-sm transition-all hover:bg-blue-700 disabled:opacity-60">
                {saving ? 'Recording...' : 'Confirm Topup'}
              </button>
            </div>
          </form>
        </ModalShell>
      )}
    </AdminLayout>
  )
}
