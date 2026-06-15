import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { sellerApi } from '@/api/seller'
import { useAuth } from '@/contexts/useAuth'

interface ProgressResponse {
  existingStore?: { id: string; status: string | number; rejectReason?: string | null } | null
  progress?: {
    storeId?: string | null
    currentStep: number
    step1?: Partial<Step1Form>
    step2?: Partial<Step2Form>
    step3?: Partial<Step3Form>
  } | null
}

interface Step1Form {
  storeName: string
  businessType: number
  phone: string
  email: string
  addressLine: string
  province: string
}

interface Step2Form {
  fullName: string
  identityNumber: string
  identityIssuedDate: string
  identityIssuedPlace: string
  taxCode: string
  identityCardFrontImageUrl?: string | null
  identityCardBackImageUrl?: string | null
}

interface Step3Form {
  bankName: string
  bankAccountNumber: string
  bankAccountName: string
  bankBin: string
}

const storeStatuses = ['Draft', 'Pending', 'Approved', 'Rejected', 'Locked']
const banks = [
  ['Vietcombank', '970436'],
  ['BIDV', '970418'],
  ['VietinBank', '970415'],
  ['Agribank', '970405'],
  ['Techcombank', '970407'],
  ['VPBank', '970432'],
  ['MBBank', '970422'],
  ['ACB', '970416'],
  ['TPBank', '970423'],
  ['Sacombank', '970403'],
]

const inputClass = 'block w-full rounded-xl border border-gray-200 px-4 py-3 text-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20'

function normalizeStoreStatus(status: string | number | undefined | null) {
  if (typeof status === 'number') return storeStatuses[status] ?? String(status)
  return status ?? ''
}

export default function RegisterSellerPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { user } = useAuth()
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [info, setInfo] = useState('')
  const [progress, setProgress] = useState<ProgressResponse | null>(null)
  const [frontImage, setFrontImage] = useState<File | null>(null)
  const [backImage, setBackImage] = useState<File | null>(null)
  const [step1, setStep1] = useState<Step1Form>({ storeName: '', businessType: 0, phone: '', email: user?.email ?? '', addressLine: '', province: '' })
  const [step2, setStep2] = useState<Step2Form>({ fullName: user?.fullName ?? '', identityNumber: '', identityIssuedDate: '', identityIssuedPlace: '', taxCode: '' })
  const [step3, setStep3] = useState<Step3Form>({ bankName: '', bankAccountNumber: '', bankAccountName: '', bankBin: '' })

  const loadProgress = async () => {
    const result = (await sellerApi.registration.getProgress()) as ProgressResponse
    setProgress(result)
    if (result.progress?.step1) setStep1((current) => ({ ...current, ...result.progress?.step1 }))
    if (result.progress?.step2) {
      const next = result.progress.step2
      setStep2((current) => ({
        ...current,
        ...next,
        identityIssuedDate: next.identityIssuedDate ? String(next.identityIssuedDate).slice(0, 10) : current.identityIssuedDate,
      }))
    }
    if (result.progress?.step3) setStep3((current) => ({ ...current, ...result.progress?.step3 }))
  }

  useEffect(() => {
    let cancelled = false

    async function run() {
      try {
        if (searchParams.get('reapply') === 'true') {
          await sellerApi.registration.reapply()
          setInfo('Your application has been reopened for editing.')
        }
        await loadProgress()
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : 'Could not load seller registration.')
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void run()
    return () => {
      cancelled = true
    }
  }, [searchParams])

  const existingStatus = normalizeStoreStatus(progress?.existingStore?.status)
  const currentStep = progress?.progress?.currentStep ?? 1

  const submitStep1 = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      await sellerApi.registration.submitStep1(step1)
      await loadProgress()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not save store information.')
    } finally {
      setSaving(false)
    }
  }

  const submitStep2 = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      const formData = new FormData()
      Object.entries(step2).forEach(([key, value]) => {
        if (value !== undefined && value !== null) formData.append(key, String(value))
      })
      if (frontImage) formData.append('identityCardFrontImage', frontImage)
      if (backImage) formData.append('identityCardBackImage', backImage)
      await sellerApi.registration.submitStep2(formData)
      await loadProgress()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not save identity information.')
    } finally {
      setSaving(false)
    }
  }

  const submitStep3 = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setError('')
    try {
      await sellerApi.registration.submitStep3(step3)
      await loadProgress()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not save banking information.')
    } finally {
      setSaving(false)
    }
  }

  const submitApplication = async () => {
    setSaving(true)
    setError('')
    try {
      await sellerApi.registration.submit()
      navigate('/profile')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Could not submit application.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <span className="material-symbols-outlined animate-spin text-[32px] text-primary">progress_activity</span>
      </div>
    )
  }

  if (existingStatus === 'Approved') {
    return (
      <div className="mx-auto max-w-lg px-4 py-20 text-center">
        <span className="material-symbols-outlined mb-4 text-6xl text-emerald-500">check_circle</span>
        <h1 className="text-3xl font-extrabold text-gray-900">Application Approved</h1>
        <p className="mt-2 text-gray-500">Your seller account is ready.</p>
        <Link className="mt-8 inline-flex rounded-xl bg-emerald-600 px-8 py-4 font-bold text-white hover:bg-emerald-700" to="/store-owner">Go to Store</Link>
      </div>
    )
  }

  if (existingStatus === 'Pending' && !progress?.progress) {
    return (
      <div className="mx-auto max-w-lg px-4 py-20 text-center">
        <span className="material-symbols-outlined mb-4 text-6xl text-primary">pending</span>
        <h1 className="text-3xl font-extrabold text-gray-900">Under Review</h1>
        <p className="mt-2 text-gray-500">Your seller application is being reviewed.</p>
        <Link className="mt-8 inline-flex rounded-xl border border-gray-200 px-6 py-3 font-bold text-gray-700 hover:bg-gray-50" to="/profile">Back to Profile</Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6 lg:px-8">
      <div className="mb-10 text-center">
        <span className="material-symbols-outlined mb-3 block text-5xl text-primary">storefront</span>
        <h1 className="text-3xl font-bold text-gray-900">Seller Registration</h1>
        <p className="mt-2 text-gray-500">Complete the steps below to open your store on GearZone</p>
      </div>

      {info ? <div className="mb-6 rounded-2xl border border-blue-200 bg-blue-50 px-5 py-4 text-sm font-medium text-blue-700">{info}</div> : null}
      {error ? <div className="mb-6 rounded-2xl border border-red-200 bg-red-50 px-5 py-4 text-sm font-medium text-red-700">{error}</div> : null}
      {existingStatus === 'Rejected' && progress?.existingStore?.rejectReason ? (
        <div className="mb-6 rounded-2xl border border-orange-200 bg-orange-50 px-5 py-4 text-sm font-medium text-orange-700">
          Previous application was rejected. Reason: {progress.existingStore.rejectReason}
        </div>
      ) : null}

      <div className="mb-8 grid grid-cols-4 gap-2">
        {['Store', 'Identity', 'Banking', 'Submit'].map((label, index) => {
          const number = index + 1
          const active = number === currentStep
          const done = number < currentStep
          return (
            <div className="text-center" key={label}>
              <div className={`mx-auto flex h-10 w-10 items-center justify-center rounded-full border-2 text-sm font-bold ${done ? 'border-primary bg-primary text-white' : active ? 'border-primary text-primary' : 'border-gray-200 text-gray-400'}`}>
                {done ? <span className="material-symbols-outlined text-[18px]">check</span> : number}
              </div>
              <p className={`mt-2 text-xs font-bold ${active ? 'text-primary' : 'text-gray-500'}`}>{label}</p>
            </div>
          )
        })}
      </div>

      <div className="overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm">
        {currentStep === 1 ? (
          <form className="space-y-5 p-6 md:p-8" onSubmit={submitStep1}>
            <h2 className="text-lg font-bold text-gray-900">Store Information</h2>
            <input className={inputClass} placeholder="Store name" required value={step1.storeName} onChange={(event) => setStep1((current) => ({ ...current, storeName: event.target.value }))} />
            <select className={inputClass} value={step1.businessType} onChange={(event) => setStep1((current) => ({ ...current, businessType: Number(event.target.value) }))}>
              <option value={0}>Individual</option>
              <option value={1}>Household Business</option>
              <option value={2}>Company</option>
            </select>
            <div className="grid gap-5 md:grid-cols-2">
              <input className={inputClass} placeholder="Phone number" required value={step1.phone} onChange={(event) => setStep1((current) => ({ ...current, phone: event.target.value }))} />
              <input className={inputClass} placeholder="Contact email" required type="email" value={step1.email} onChange={(event) => setStep1((current) => ({ ...current, email: event.target.value }))} />
            </div>
            <input className={inputClass} placeholder="Full address" required value={step1.addressLine} onChange={(event) => setStep1((current) => ({ ...current, addressLine: event.target.value }))} />
            <input className={inputClass} placeholder="Province / City" required value={step1.province} onChange={(event) => setStep1((current) => ({ ...current, province: event.target.value }))} />
            <div className="flex justify-end">
              <button className="rounded-xl bg-primary px-8 py-3 font-bold text-white hover:bg-blue-700 disabled:bg-gray-300" disabled={saving} type="submit">{saving ? 'Saving...' : 'Continue'}</button>
            </div>
          </form>
        ) : null}

        {currentStep === 2 ? (
          <form className="space-y-5 p-6 md:p-8" onSubmit={submitStep2}>
            <h2 className="text-lg font-bold text-gray-900">Identity Verification</h2>
            <input className={inputClass} placeholder="Full name as per identity card" required value={step2.fullName} onChange={(event) => setStep2((current) => ({ ...current, fullName: event.target.value }))} />
            <div className="grid gap-5 md:grid-cols-2">
              <input className={inputClass} placeholder="Identity card number" required value={step2.identityNumber} onChange={(event) => setStep2((current) => ({ ...current, identityNumber: event.target.value }))} />
              <input className={inputClass} placeholder="Tax code" required value={step2.taxCode} onChange={(event) => setStep2((current) => ({ ...current, taxCode: event.target.value }))} />
            </div>
            <div className="grid gap-5 md:grid-cols-2">
              <input className={inputClass} required type="date" value={step2.identityIssuedDate} onChange={(event) => setStep2((current) => ({ ...current, identityIssuedDate: event.target.value }))} />
              <input className={inputClass} placeholder="Place of issue" required value={step2.identityIssuedPlace} onChange={(event) => setStep2((current) => ({ ...current, identityIssuedPlace: event.target.value }))} />
            </div>
            <div className="grid gap-5 md:grid-cols-2">
              <label className="text-sm font-semibold text-gray-700">
                Identity card front
                <input accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" className={inputClass + ' mt-1.5'} onChange={(event) => setFrontImage(event.target.files?.[0] ?? null)} required={!step2.identityCardFrontImageUrl} type="file" />
              </label>
              <label className="text-sm font-semibold text-gray-700">
                Identity card back
                <input accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp" className={inputClass + ' mt-1.5'} onChange={(event) => setBackImage(event.target.files?.[0] ?? null)} required={!step2.identityCardBackImageUrl} type="file" />
              </label>
            </div>
            <div className="flex justify-end">
              <button className="rounded-xl bg-primary px-8 py-3 font-bold text-white hover:bg-blue-700 disabled:bg-gray-300" disabled={saving} type="submit">{saving ? 'Saving...' : 'Continue'}</button>
            </div>
          </form>
        ) : null}

        {currentStep === 3 ? (
          <form className="space-y-5 p-6 md:p-8" onSubmit={submitStep3}>
            <h2 className="text-lg font-bold text-gray-900">Payment Information</h2>
            <select className={inputClass} required value={step3.bankName} onChange={(event) => {
              const bank = banks.find(([name]) => name === event.target.value)
              setStep3((current) => ({ ...current, bankName: event.target.value, bankBin: bank?.[1] ?? '' }))
            }}>
              <option value="">Select bank</option>
              {banks.map(([name, bin]) => <option key={bin} value={name}>{name}</option>)}
            </select>
            <input className={inputClass} placeholder="Account number" required value={step3.bankAccountNumber} onChange={(event) => setStep3((current) => ({ ...current, bankAccountNumber: event.target.value }))} />
            <input className={inputClass} placeholder="Account holder name" required value={step3.bankAccountName} onChange={(event) => setStep3((current) => ({ ...current, bankAccountName: event.target.value.toUpperCase() }))} />
            <div className="flex justify-end">
              <button className="rounded-xl bg-primary px-8 py-3 font-bold text-white hover:bg-blue-700 disabled:bg-gray-300" disabled={saving} type="submit">{saving ? 'Saving...' : 'Continue'}</button>
            </div>
          </form>
        ) : null}

        {currentStep >= 4 ? (
          <div className="space-y-6 p-6 text-center md:p-8">
            <span className="material-symbols-outlined text-6xl text-emerald-500">task_alt</span>
            <h2 className="text-xl font-bold text-gray-900">Ready to Submit</h2>
            <p className="mx-auto max-w-md text-sm text-gray-500">Submit your seller registration for admin review. You can return to profile after submission.</p>
            <button className="rounded-xl bg-emerald-600 px-8 py-4 font-bold text-white hover:bg-emerald-700 disabled:bg-gray-300" disabled={saving} onClick={() => void submitApplication()} type="button">{saving ? 'Submitting...' : 'Submit Application'}</button>
          </div>
        ) : null}
      </div>
    </div>
  )
}
