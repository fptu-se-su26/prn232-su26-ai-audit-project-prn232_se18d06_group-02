/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { adminApi, storeStatus, type StoreApplicationDto, type StoreStatus } from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

type ModalType = 'approve' | 'reject' | 'request-info' | null

const quickReasons = ['Incomplete Information', 'Invalid Documents', 'Duplicate Account', 'Policy Violation']

function formatDate(value?: string | null) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value))
}

function formatDateTime(value?: string | null) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', {
    day: '2-digit',
    hour: 'numeric',
    minute: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(new Date(value))
}

function statusLabel(status: StoreStatus) {
  if (status === storeStatus.pending) return 'Pending'
  if (status === storeStatus.approved) return 'Approved'
  if (status === storeStatus.rejected) return 'Rejected'
  if (status === storeStatus.locked) return 'Locked'
  return 'Draft'
}

function statusBadgeClasses(status: StoreStatus) {
  if (status === storeStatus.pending) return 'bg-amber-50 text-amber-700 ring-amber-600/20'
  if (status === storeStatus.approved) return 'bg-green-50 text-green-700 ring-green-600/20'
  if (status === storeStatus.rejected) return 'bg-red-50 text-red-700 ring-red-600/10'
  return 'bg-slate-50 text-slate-700 ring-slate-600/20'
}

function statusDotClasses(status: StoreStatus) {
  if (status === storeStatus.pending) return 'bg-amber-500'
  if (status === storeStatus.approved) return 'bg-green-500'
  if (status === storeStatus.rejected) return 'bg-red-500'
  return 'bg-slate-500'
}

function fieldValue(value?: string | number | null) {
  if (value === undefined || value === null || value === '') return 'N/A'
  return String(value)
}

function ownerInitials(name?: string | null) {
  if (!name) return 'NA'
  const parts = name.split(' ').filter(Boolean)
  const first = parts[0]?.[0] ?? ''
  const last = (parts.at(-1) ?? '')[0] ?? ''
  return `${first}${last}`.toUpperCase() || 'NA'
}

function InfoCard({ children, icon, title }: { children: ReactNode; icon: string; title: string }) {
  return (
    <section className="overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
      <div className="flex items-center justify-between border-b border-slate-100 bg-slate-50/30 px-6 py-4">
        <h3 className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-900">
          <span className="material-symbols-outlined text-[20px] text-primary">{icon}</span>
          {title}
        </h3>
      </div>
      <div className="p-6">{children}</div>
    </section>
  )
}

function DetailItem({
  children,
  className = '',
  label,
}: {
  children: ReactNode
  className?: string
  label: string
}) {
  return (
    <div className={className}>
      <dt className="text-[10px] font-bold uppercase tracking-widest text-slate-400">{label}</dt>
      <dd className="mt-1 text-sm font-semibold text-slate-900">{children}</dd>
    </div>
  )
}

function DocumentPreview({ label, url }: { label: string; url?: string | null }) {
  if (!url) {
    return (
      <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-sm font-medium text-amber-700">
        {label}: Not uploaded.
      </div>
    )
  }

  return (
    <a
      href={url}
      target="_blank"
      rel="noopener noreferrer"
      className="group relative block aspect-[16/9] overflow-hidden rounded-xl border border-slate-100 bg-slate-200 shadow-sm"
    >
      <img alt={label} className="size-full object-cover transition-transform duration-700 group-hover:scale-110" src={url} />
      <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/0 to-transparent opacity-80 transition-opacity group-hover:opacity-90" />
      <span className="absolute bottom-3 left-4 text-[10px] font-bold uppercase tracking-widest text-white drop-shadow-md">{label}</span>
      <div className="absolute inset-0 flex items-center justify-center bg-black/20 opacity-0 backdrop-blur-[1px] transition-all group-hover:opacity-100">
        <span className="flex translate-y-2 items-center gap-2 rounded-full bg-white px-4 py-2 text-xs font-bold text-slate-900 shadow-xl transition-all group-hover:translate-y-0 hover:bg-primary hover:text-white">
          <span className="material-symbols-outlined text-[18px]">zoom_in</span>
          Full Image
        </span>
      </div>
    </a>
  )
}

function ModalShell({ children, onClose }: { children: ReactNode; onClose: () => void }) {
  return (
    <div className="fixed inset-0 z-[60] flex items-center justify-center overflow-auto bg-slate-900/50 p-4 backdrop-blur-sm">
      <button type="button" aria-label="Close modal" className="absolute inset-0 cursor-default" onClick={onClose} />
      <div className="relative mx-auto w-full max-w-lg rounded-2xl border border-slate-100 bg-white p-6 shadow-xl">{children}</div>
    </div>
  )
}

function LoadingDetail() {
  return (
    <div className="flex flex-col gap-6">
      <div className="h-24 animate-pulse rounded-xl bg-slate-200" />
      <div className="grid gap-6 xl:grid-cols-12">
        <div className="space-y-6 xl:col-span-7">
          <div className="h-72 animate-pulse rounded-xl bg-slate-200" />
          <div className="h-56 animate-pulse rounded-xl bg-slate-200" />
        </div>
        <div className="space-y-6 xl:col-span-5">
          <div className="h-64 animate-pulse rounded-xl bg-slate-200" />
          <div className="h-96 animate-pulse rounded-xl bg-slate-200" />
        </div>
      </div>
    </div>
  )
}

export default function AdminStoreApplicationDetailPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [store, setStore] = useState<StoreApplicationDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [actionLoading, setActionLoading] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [modal, setModal] = useState<ModalType>(null)
  const [rejectReason, setRejectReason] = useState('')
  const [requestNote, setRequestNote] = useState('')
  const [validationError, setValidationError] = useState('')

  const documentCount = useMemo(() => {
    return (store?.identityCardFrontImageUrl ? 1 : 0) + (store?.identityCardBackImageUrl ? 1 : 0)
  }, [store])

  const loadStore = async () => {
    if (!id) return
    setLoading(true)
    setError('')

    try {
      const data = await adminApi.storeApplications.get(id)
      setStore(data)
    } catch (err) {
      setStore(null)
      setError(err instanceof Error ? err.message : 'Unable to load application details.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadStore()
  }, [id])

  const closeModal = () => {
    setModal(null)
    setValidationError('')
  }

  const runAction = async (action: () => Promise<string>) => {
    setActionLoading(true)
    setError('')
    setSuccess('')

    try {
      const message = await action()
      setSuccess(message)
      closeModal()
      await loadStore()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Action failed.')
    } finally {
      setActionLoading(false)
    }
  }

  const handleApprove = () => {
    if (!id) return
    void runAction(() => adminApi.storeApplications.approve(id))
  }

  const handleReject = () => {
    if (!id) return
    const reason = rejectReason.trim()
    if (!reason) {
      setValidationError('Rejection reason is required.')
      return
    }
    if (reason.length > 500) {
      setValidationError('Reason cannot exceed 500 characters.')
      return
    }
    void runAction(() => adminApi.storeApplications.reject(id, { reason }))
  }

  const handleRequestInfo = () => {
    if (!id) return
    const note = requestNote.trim()
    if (!note) {
      setValidationError('Note to applicant is required.')
      return
    }
    void runAction(() => adminApi.storeApplications.requestInfo(id, { note }))
  }

  const addQuickReason = (reason: string) => {
    setRejectReason((current) => {
      if (current.includes(reason)) return current
      return current ? `${current}\n- ${reason}` : `- ${reason}`
    })
    setValidationError('')
  }

  return (
    <AdminLayout activePage="Store Applications" breadcrumb={['Dashboard', 'Store Applications', 'Details']} pageHeader="Application Details">
      {loading ? (
        <LoadingDetail />
      ) : error && !store ? (
        <div className="rounded-xl border border-red-200 bg-red-50 p-6 text-sm font-medium text-red-700">
          {error}
          <div className="mt-4">
            <Link to="/admin/store-applications" className="font-bold text-red-800 underline">
              Back to Store Applications
            </Link>
          </div>
        </div>
      ) : store ? (
        <div className="flex flex-col gap-8 pb-24">
          <div className="flex flex-col justify-between gap-6 md:flex-row md:items-end">
            <div className="flex items-center gap-4">
              {store.logoUrl ? (
                <img src={store.logoUrl} alt={`${store.storeName} Logo`} className="size-20 rounded-xl border border-slate-200 object-cover shadow-sm" />
              ) : (
                <div className="flex size-20 items-center justify-center rounded-xl border border-slate-200 bg-slate-50 shadow-sm">
                  <span className="material-symbols-outlined text-4xl text-slate-300">store</span>
                </div>
              )}

              <div className="flex flex-col gap-2">
                <h1 className="text-3xl font-bold tracking-tight text-slate-900">{store.storeName}</h1>
                <div className="flex flex-wrap items-center gap-4">
                  <span
                    className={`inline-flex items-center gap-1.5 rounded-full px-3 py-1 text-sm font-bold uppercase tracking-tight ring-1 ring-inset ${statusBadgeClasses(
                      store.status,
                    )}`}
                  >
                    <span className={`size-1.5 rounded-full ${statusDotClasses(store.status)}`} />
                    {statusLabel(store.status)}
                  </span>
                  <span className="text-sm font-medium text-slate-500">
                    Submitted on {formatDate(store.createdAt)} - {fieldValue(store.businessType)} Application
                  </span>
                </div>
              </div>
            </div>

            <div className="flex items-center gap-3">
              <button
                type="button"
                className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-700 shadow-sm transition-all hover:bg-slate-50"
              >
                <span className="material-symbols-outlined text-[18px] text-slate-500">print</span>
                Print
              </button>
              <button
                type="button"
                className="flex items-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-2 text-sm font-semibold text-slate-700 shadow-sm transition-all hover:bg-slate-50"
              >
                <span className="material-symbols-outlined text-[18px] text-slate-500">history</span>
                Audit Log
              </button>
            </div>
          </div>

          {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}
          {success && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">{success}</div>}

          {store.status === storeStatus.rejected && store.rejectReason && (
            <div className="flex items-start gap-5 rounded-xl border border-red-200 bg-red-50 p-6 shadow-sm">
              <div className="flex size-10 shrink-0 items-center justify-center rounded-full bg-red-100 text-red-700 shadow-sm">
                <span className="material-symbols-outlined filled text-[20px]">error</span>
              </div>
              <div className="mt-0.5 flex-1">
                <h3 className="text-base font-bold tracking-tight text-red-900">Rejection Reason</h3>
                <div className="mt-2 text-sm font-medium leading-relaxed text-red-800">{store.rejectReason}</div>
                <div className="mt-5 flex items-center gap-3">
                  <label className="relative inline-flex cursor-pointer items-center">
                    <input type="checkbox" className="peer sr-only" checked readOnly />
                    <div className="h-6 w-11 rounded-full bg-slate-200 after:absolute after:left-[2px] after:top-[2px] after:size-5 after:rounded-full after:border after:border-gray-300/50 after:bg-white after:transition-all after:content-[''] peer-checked:bg-primary peer-checked:after:translate-x-full peer-checked:after:border-white" />
                    <span className="ml-3 text-sm font-medium text-slate-600">Allow Resubmission</span>
                  </label>
                </div>
              </div>
            </div>
          )}

          <div className="grid grid-cols-1 gap-8 lg:grid-cols-12">
            <div className="flex flex-col gap-6 lg:col-span-12 xl:col-span-7">
              <InfoCard icon="domain" title="Basic Information">
                <dl className="grid grid-cols-1 gap-x-8 gap-y-8 sm:grid-cols-2">
                  <DetailItem label="Company Name">{store.storeName}</DetailItem>
                  <DetailItem label="Tax Code / EIN">
                    <span className="font-mono tracking-tighter">{store.taxCode}</span>
                  </DetailItem>
                  <DetailItem label="Business Type">{fieldValue(store.businessType)}</DetailItem>
                  <DetailItem label="Store URL Slug">{fieldValue(store.slug)}</DetailItem>
                  <DetailItem label="Store Email">
                    <span className="text-primary">{fieldValue(store.email)}</span>
                  </DetailItem>
                  <DetailItem label="Store Phone">{fieldValue(store.phone)}</DetailItem>
                  <DetailItem className="sm:col-span-2" label="Store Description">
                    {fieldValue(store.description)}
                  </DetailItem>
                  <DetailItem className="sm:col-span-2" label="Headquarters Address">
                    <div className="flex items-start justify-between rounded-lg border border-slate-100 bg-slate-50 p-3">
                      <span className="leading-relaxed">
                        {fieldValue(store.addressLine)}
                        <br />
                        {fieldValue(store.province)}
                      </span>
                      <button type="button" className="rounded p-1.5 text-slate-400 transition-all hover:bg-white hover:text-primary hover:shadow-sm">
                        <span className="material-symbols-outlined text-[20px]">map</span>
                      </button>
                    </div>
                  </DetailItem>
                </dl>
              </InfoCard>

              <InfoCard icon="person" title="Owner & Representative">
                <dl className="grid grid-cols-1 gap-x-8 gap-y-8 sm:grid-cols-2">
                  <DetailItem label="Representative">
                    <span className="flex items-center gap-2">
                      <span className="flex size-6 items-center justify-center rounded-full border border-blue-200 bg-blue-100 text-[10px] font-bold text-blue-700 shadow-sm">
                        {ownerInitials(store.ownerName)}
                      </span>
                      <span>{fieldValue(store.ownerName)}</span>
                    </span>
                  </DetailItem>
                  <DetailItem label="Email Address">
                    <span className="text-primary">{fieldValue(store.ownerEmail)}</span>
                  </DetailItem>
                  <DetailItem label="Phone Number">{fieldValue(store.ownerPhone)}</DetailItem>
                  <DetailItem label="Identity Number (CCCD)">{fieldValue(store.identityNumber)}</DetailItem>
                  <DetailItem label="Identity Issued Date">{store.identityIssuedDate ? formatDate(store.identityIssuedDate) : 'N/A'}</DetailItem>
                  <DetailItem label="Identity Issued Place">{fieldValue(store.identityIssuedPlace)}</DetailItem>
                </dl>
              </InfoCard>

              <InfoCard icon="account_balance" title="Banking & Financial Setup">
                <dl className="grid grid-cols-1 gap-x-8 gap-y-8 sm:grid-cols-2">
                  <DetailItem label="Bank Name">{fieldValue(store.bankName)}</DetailItem>
                  <DetailItem label="Account Name">{fieldValue(store.bankAccountName)}</DetailItem>
                  <DetailItem label="Account Number">
                    <span className="font-mono tracking-tighter">{fieldValue(store.bankAccountNumber)}</span>
                  </DetailItem>
                  <DetailItem label="Commission Rate">
                    <span className="text-emerald-600">{(store.commissionRate * 100).toFixed(2).replace(/\.?0+$/, '')}%</span>
                  </DetailItem>
                </dl>
              </InfoCard>

              <InfoCard icon="info" title="Application Metadata">
                <div className="grid grid-cols-1 gap-6 sm:grid-cols-3">
                  <div>
                    <p className="text-[10px] font-bold uppercase tracking-widest text-slate-400">Application ID</p>
                    <p className="mt-1 font-mono text-sm font-semibold text-slate-900">#{store.id.slice(0, 8).toUpperCase()}</p>
                  </div>
                  <div>
                    <p className="text-[10px] font-bold uppercase tracking-widest text-slate-400">Submission Date</p>
                    <p className="mt-1 text-sm font-semibold text-slate-900">{formatDate(store.createdAt)}</p>
                  </div>
                  <div>
                    <p className="text-[10px] font-bold uppercase tracking-widest text-slate-400">Last Updated</p>
                    <p className="mt-1 text-sm font-semibold text-slate-900">{store.updatedAt ? formatDate(store.updatedAt) : 'N/A'}</p>
                  </div>
                </div>
              </InfoCard>
            </div>

            <div className="flex flex-col gap-6 lg:col-span-12 xl:col-span-5">
              <InfoCard icon="history" title="Application History">
                <div className="relative">
                  <div className="absolute bottom-4 left-[7px] top-6 z-0 w-px bg-slate-200" />

                  <div className="relative z-10 flex gap-4">
                    <div className="mt-0.5 flex size-4 shrink-0 items-center justify-center rounded-full bg-slate-300 ring-4 ring-white" />
                    <div className="flex flex-col gap-0.5 pb-8">
                      <h4 className="text-sm font-bold text-slate-900">Application Submitted</h4>
                      <p className="text-[11px] font-medium text-slate-500">{formatDateTime(store.createdAt)}</p>
                    </div>
                  </div>

                  {store.status === storeStatus.approved && store.approvedAt && (
                    <div className="relative z-10 flex gap-4">
                      <div className="mt-0.5 flex size-4 shrink-0 items-center justify-center rounded-full bg-green-500 shadow-sm shadow-green-500/30 ring-4 ring-white" />
                      <div className="flex flex-col gap-0.5">
                        <h4 className="text-sm font-bold text-green-700">Application Approved</h4>
                        <p className="text-[11px] font-medium text-slate-500">{formatDateTime(store.approvedAt)}</p>
                      </div>
                    </div>
                  )}

                  {store.status === storeStatus.rejected && store.updatedAt && (
                    <div className="relative z-10 flex gap-4">
                      <div className="mt-0.5 flex size-4 shrink-0 items-center justify-center rounded-full bg-red-500 shadow-sm shadow-red-500/30 ring-4 ring-white" />
                      <div className="flex flex-col gap-0.5">
                        <h4 className="text-sm font-bold text-red-700">Application Rejected</h4>
                        <p className="text-[11px] font-medium text-slate-500">{formatDateTime(store.updatedAt)}</p>
                      </div>
                    </div>
                  )}
                </div>
              </InfoCard>

              <section className="flex h-full flex-col overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
                <div className="flex items-center justify-between border-b border-slate-100 bg-slate-50/30 px-6 py-4">
                  <h3 className="flex items-center gap-2 text-xs font-bold uppercase tracking-wider text-slate-900">
                    <span className="material-symbols-outlined text-[20px] text-primary">description</span>
                    Documents Preview
                  </h3>
                  <span className="rounded-full bg-slate-100 px-2.5 py-0.5 text-[11px] font-bold uppercase tracking-wider text-slate-600">
                    {documentCount} Files
                  </span>
                </div>
                <div className="flex flex-col gap-6 p-6">
                  <div className="space-y-4">
                    <div className="mb-3 flex items-center justify-between">
                      <h4 className="text-[11px] font-bold uppercase tracking-widest text-slate-400">Identity Verification</h4>
                      {store.identityCardFrontImageUrl && store.identityCardBackImageUrl ? (
                        <span className="flex items-center gap-1 text-[11px] font-bold uppercase tracking-widest text-emerald-600">
                          <span className="material-symbols-outlined filled text-[16px] text-emerald-500">check_circle</span>
                          Ready for review
                        </span>
                      ) : (
                        <span className="flex items-center gap-1 text-[11px] font-bold uppercase tracking-widest text-amber-600">
                          <span className="material-symbols-outlined text-[16px] text-amber-500">warning</span>
                          Missing files
                        </span>
                      )}
                    </div>

                    <DocumentPreview label="ID Card (Front)" url={store.identityCardFrontImageUrl} />
                    <DocumentPreview label="ID Card (Back)" url={store.identityCardBackImageUrl} />
                  </div>
                </div>
              </section>
            </div>
          </div>

          {store.status === storeStatus.pending && (
            <div className="sticky bottom-0 z-40 -mx-6 border-t border-slate-200 bg-white/90 px-6 py-4 shadow-2xl backdrop-blur-md">
              <div className="mx-auto flex w-full max-w-[1400px] flex-col justify-between gap-4 md:flex-row md:items-center">
                <div className="flex items-center text-sm font-medium text-slate-500">
                  <span className="material-symbols-outlined mr-2 text-[20px]">pending_actions</span>
                  This application is awaiting your review.
                </div>
                <div className="flex w-full flex-col gap-3 md:w-auto md:flex-row md:items-center">
                  <button
                    type="button"
                    onClick={() => setModal('request-info')}
                    className="flex items-center justify-center gap-2 rounded-xl border-2 border-primary/20 px-5 py-2.5 text-sm font-bold text-primary transition-all hover:border-primary hover:bg-primary/5"
                  >
                    <span className="material-symbols-outlined text-[20px]">help_outline</span>
                    REQUEST INFO
                  </button>
                  <div className="flex items-center gap-3">
                    <button
                      type="button"
                      onClick={() => setModal('reject')}
                      className="flex items-center justify-center gap-2 rounded-xl border-2 border-red-100 bg-red-50 px-6 py-2.5 text-sm font-bold text-red-600 shadow-sm transition-all hover:border-red-600 hover:bg-red-600 hover:text-white"
                    >
                      <span className="material-symbols-outlined text-[20px]">block</span>
                      REJECT
                    </button>
                    <button
                      type="button"
                      onClick={() => setModal('approve')}
                      className="flex items-center justify-center gap-2 rounded-xl bg-primary px-8 py-2.5 text-sm font-bold text-white shadow-lg shadow-primary/30 transition-all hover:-translate-y-0.5 hover:bg-blue-700"
                    >
                      <span className="material-symbols-outlined text-[20px]">check_circle</span>
                      APPROVE APPLICATION
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      ) : null}

      {modal === 'approve' && store && (
        <ModalShell onClose={closeModal}>
          <div className="flex flex-col items-center justify-center gap-3 text-center">
            <div className="mx-auto mb-2 flex size-16 items-center justify-center rounded-full bg-green-50 ring-8 ring-green-50/50">
              <span className="material-symbols-outlined text-3xl text-green-600">check_circle</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">Approve Registration</h3>
            <p className="text-sm text-slate-500">
              Are you sure you want to approve the application for <span className="font-bold text-slate-900">{store.storeName}</span>?
            </p>
            <p className="mt-2 text-xs text-slate-400">This will notify the owner and allow them access to the seller dashboard.</p>
          </div>

          <div className="mt-8 flex justify-center gap-3">
            <button
              type="button"
              onClick={closeModal}
              className="flex-1 rounded-xl border border-slate-200 bg-white px-5 py-2.5 text-sm font-bold text-slate-600 shadow-sm transition-colors hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="button"
              disabled={actionLoading}
              onClick={handleApprove}
              className="flex-1 rounded-xl bg-primary px-6 py-2.5 text-sm font-bold text-white shadow-lg shadow-primary/30 transition-colors hover:bg-blue-700 disabled:opacity-60"
            >
              Approve Application
            </button>
          </div>
        </ModalShell>
      )}

      {modal === 'reject' && store && (
        <ModalShell onClose={closeModal}>
          <div className="flex gap-4">
            <div className="mt-1 flex size-10 shrink-0 items-center justify-center rounded-full bg-red-100 text-red-600">
              <span className="material-symbols-outlined text-2xl">warning</span>
            </div>
            <div className="w-full">
              <h3 className="text-xl font-bold text-slate-900">Reject Registration</h3>
              <p className="mt-1 text-sm text-slate-500">
                You are about to reject the application for <span className="font-bold text-slate-900">{store.storeName}</span>.
              </p>

              <div className="mt-6 w-full">
                <label htmlFor="rejectReason" className="mb-2 block text-sm font-bold text-slate-700">
                  Reason for Rejection
                </label>
                <div className="mb-3 flex flex-wrap gap-2">
                  {quickReasons.map((reason) => (
                    <button
                      key={reason}
                      type="button"
                      onClick={() => addQuickReason(reason)}
                      className="rounded-full border border-slate-200 bg-white px-3 py-1.5 text-xs font-semibold text-slate-600 shadow-sm transition-colors hover:border-slate-300 hover:bg-slate-50"
                    >
                      {reason === 'Incomplete Information' ? 'Incomplete Info' : reason}
                    </button>
                  ))}
                </div>
                <textarea
                  id="rejectReason"
                  rows={4}
                  maxLength={500}
                  value={rejectReason}
                  onChange={(event) => {
                    setRejectReason(event.target.value)
                    setValidationError('')
                  }}
                  className="w-full resize-none rounded-xl border border-slate-200 bg-slate-50/50 px-4 py-3 text-sm text-slate-900 shadow-sm transition-all placeholder:text-slate-400 focus:border-red-500 focus:bg-white focus:outline-none focus:ring-4 focus:ring-red-500/10"
                  placeholder="Please provide details about why this application is being rejected..."
                />
                <div className="mt-1 flex justify-between text-xs">
                  <span className="font-medium text-red-600">{validationError}</span>
                  <span className="text-slate-400">{rejectReason.length}/500</span>
                </div>
              </div>
            </div>
          </div>

          <div className="mt-6 flex justify-end gap-3 border-t border-slate-100 pt-5">
            <button
              type="button"
              onClick={closeModal}
              className="rounded-xl border border-slate-200 bg-white px-5 py-2 text-sm font-bold text-slate-600 shadow-sm transition-colors hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="button"
              disabled={actionLoading}
              onClick={handleReject}
              className="rounded-xl border border-transparent bg-red-600 px-6 py-2 text-sm font-bold text-white shadow-sm shadow-red-600/20 transition-colors hover:bg-red-700 disabled:opacity-60"
            >
              Confirm Rejection
            </button>
          </div>
        </ModalShell>
      )}

      {modal === 'request-info' && (
        <ModalShell onClose={closeModal}>
          <div className="flex gap-4">
            <div className="mt-1 flex size-10 shrink-0 items-center justify-center rounded-full bg-blue-50 text-blue-600">
              <span className="material-symbols-outlined text-2xl">help_outline</span>
            </div>
            <div className="w-full">
              <h3 className="text-xl font-bold text-slate-900">Request Information</h3>
              <p className="mt-1 text-sm text-slate-500">Send a message to the applicant to request additional information or clarification.</p>

              <div className="mt-6 w-full">
                <label htmlFor="requestInfo" className="mb-2 block text-sm font-bold text-slate-700">
                  Note to Applicant
                </label>
                <textarea
                  id="requestInfo"
                  rows={4}
                  value={requestNote}
                  onChange={(event) => {
                    setRequestNote(event.target.value)
                    setValidationError('')
                  }}
                  className="w-full resize-none rounded-xl border border-slate-200 bg-slate-50/50 px-4 py-3 text-sm text-slate-900 shadow-sm transition-all placeholder:text-slate-400 focus:border-primary focus:bg-white focus:outline-none focus:ring-4 focus:ring-primary/10"
                  placeholder="E.g., Please provide a clearer copy of your business license..."
                />
                {validationError && <div className="mt-1 text-xs font-medium text-red-600">{validationError}</div>}
              </div>
            </div>
          </div>

          <div className="mt-6 flex justify-end gap-3 border-t border-slate-100 pt-5">
            <button
              type="button"
              onClick={closeModal}
              className="rounded-xl border border-slate-200 bg-white px-5 py-2 text-sm font-bold text-slate-600 shadow-sm transition-colors hover:bg-slate-50"
            >
              Cancel
            </button>
            <button
              type="button"
              disabled={actionLoading}
              onClick={handleRequestInfo}
              className="rounded-xl border border-transparent bg-blue-600 px-6 py-2 text-sm font-bold text-white shadow-sm shadow-blue-600/20 transition-colors hover:bg-blue-700 disabled:opacity-60"
            >
              Send Request
            </button>
          </div>
        </ModalShell>
      )}

      {!loading && store && (
        <button
          type="button"
          onClick={() => navigate('/admin/store-applications')}
          className="fixed bottom-4 left-4 z-30 hidden rounded-full border border-slate-200 bg-white p-3 text-slate-500 shadow-lg transition hover:text-primary lg:flex"
          title="Back to list"
        >
          <span className="material-symbols-outlined">arrow_back</span>
        </button>
      )}
    </AdminLayout>
  )
}
