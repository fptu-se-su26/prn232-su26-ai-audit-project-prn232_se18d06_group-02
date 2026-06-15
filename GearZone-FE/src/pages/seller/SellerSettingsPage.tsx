import { useEffect, useMemo, useState } from 'react'
import type { FormEvent, MouseEvent } from 'react'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface StoreSettings {
  storeName?: string
  name?: string
  description?: string | null
  logoUrl?: string | null
  phone?: string | null
  contactPhone?: string | null
  email?: string | null
  contactEmail?: string | null
  addressLine?: string | null
  province?: string | null
  latitude?: number | null
  longitude?: number | null
  status?: string | null
  commissionRate?: number | null
}

interface SettingsForm {
  storeName: string
  phone: string
  email: string
  description: string
  addressLine: string
  province: string
  latitude: string
  longitude: string
  logoUrl: string
}

const SETTINGS_MENU = [
  { label: 'General Info', icon: 'storefront', active: true },
  { label: 'Business Profile', icon: 'account_circle' },
  { label: 'Security', icon: 'security' },
  { label: 'Notifications', icon: 'notifications_active' },
]

const DEFAULT_CENTER = {
  lat: 21.028,
  lng: 105.83991,
}

function normalizeStore(store: StoreSettings): SettingsForm {
  return {
    storeName: store.storeName ?? store.name ?? '',
    phone: store.phone ?? store.contactPhone ?? '',
    email: store.email ?? store.contactEmail ?? '',
    description: store.description ?? '',
    addressLine: store.addressLine ?? '',
    province: store.province ?? '',
    latitude: store.latitude == null ? '' : String(store.latitude),
    longitude: store.longitude == null ? '' : String(store.longitude),
    logoUrl: store.logoUrl ?? '',
  }
}

function coordinateLabel(latitude: string, longitude: string) {
  const lat = Number(latitude)
  const lng = Number(longitude)
  if (Number.isNaN(lat) || Number.isNaN(lng)) return 'Drag pin to set location'
  return `${lat.toFixed(5)}, ${lng.toFixed(5)}`
}

function mapMarkerPosition(latitude: string, longitude: string) {
  const lat = Number(latitude)
  const lng = Number(longitude)
  if (Number.isNaN(lat) || Number.isNaN(lng)) return { left: '50%', top: '50%' }

  const x = Math.min(88, Math.max(12, 50 + (lng - DEFAULT_CENTER.lng) * 22))
  const y = Math.min(88, Math.max(12, 50 - (lat - DEFAULT_CENTER.lat) * 22))
  return { left: `${x}%`, top: `${y}%` }
}

function buildPayload(form: SettingsForm) {
  const latitude = form.latitude.trim() ? Number(form.latitude) : null
  const longitude = form.longitude.trim() ? Number(form.longitude) : null

  return {
    phone: form.phone.trim(),
    email: form.email.trim(),
    description: form.description.trim() || null,
    addressLine: form.addressLine.trim(),
    province: form.province.trim(),
    latitude: latitude == null || Number.isNaN(latitude) ? null : latitude,
    longitude: longitude == null || Number.isNaN(longitude) ? null : longitude,
  }
}

export default function SellerSettingsPage() {
  const [form, setForm] = useState<SettingsForm | null>(null)
  const [initialForm, setInitialForm] = useState<SettingsForm | null>(null)
  const [searchValue, setSearchValue] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [success, setSuccess] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    sellerApi
      .getStoreSettings()
      .then((store) => {
        const normalized = normalizeStore(store as StoreSettings)
        setForm(normalized)
        setInitialForm(normalized)
        setSearchValue(normalized.addressLine)
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Failed to load store settings.')
      })
      .finally(() => setLoading(false))
  }, [])

  const markerStyle = useMemo(
    () => mapMarkerPosition(form?.latitude ?? '', form?.longitude ?? ''),
    [form?.latitude, form?.longitude],
  )

  const updateField = (field: keyof SettingsForm, value: string) => {
    setForm((current) => (current ? { ...current, [field]: value } : current))
  }

  const handleMapClick = (event: MouseEvent<HTMLDivElement>) => {
    if (!form) return
    const rect = event.currentTarget.getBoundingClientRect()
    const x = (event.clientX - rect.left) / rect.width
    const y = (event.clientY - rect.top) / rect.height
    const latitude = DEFAULT_CENTER.lat + (0.5 - y) * 2.2
    const longitude = DEFAULT_CENTER.lng + (x - 0.5) * 2.2

    setForm({
      ...form,
      latitude: latitude.toFixed(6),
      longitude: longitude.toFixed(6),
    })
  }

  const applySearchToAddress = () => {
    if (!form || !searchValue.trim()) return
    setForm((current) =>
      current
        ? {
            ...current,
            addressLine: searchValue.trim(),
          }
        : current,
    )
  }

  const discardChanges = () => {
    if (!initialForm) return
    setForm(initialForm)
    setSearchValue(initialForm.addressLine)
    setSuccess('')
    setError('')
  }

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    if (!form) return

    setSaving(true)
    setSuccess('')
    setError('')

    try {
      await sellerApi.updateStoreSettings(buildPayload(form))
      setSuccess('Store profile updated successfully!')
      setInitialForm(form)
    } catch (err: unknown) {
      setError(
        err instanceof Error
          ? err.message
          : 'Failed to update store profile. Note: Only approved stores can update profile here.',
      )
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <SellerLayout pageHeader="Store Settings" breadcrumb={['Dashboard', 'Settings']}>
        <div className="mx-auto grid w-full max-w-[1440px] grid-cols-1 gap-8 lg:grid-cols-3">
          <div className="h-56 animate-pulse rounded-xl border border-slate-200 bg-white" />
          <div className="h-[720px] animate-pulse rounded-xl border border-slate-200 bg-white lg:col-span-2" />
        </div>
      </SellerLayout>
    )
  }

  if (!form) {
    return (
      <SellerLayout pageHeader="Store Settings" breadcrumb={['Dashboard', 'Settings']}>
        <div className="rounded-xl border border-red-200 bg-red-50 p-8 text-center text-red-600">
          {error || 'Failed to load store settings.'}
        </div>
      </SellerLayout>
    )
  }

  return (
    <SellerLayout pageHeader="Store Settings" breadcrumb={['Dashboard', 'Settings']}>
      <style>
        {`
          .settings-card-shadow {
            box-shadow: 0 1px 3px 0 rgb(15 23 42 / 0.08), 0 1px 2px -1px rgb(15 23 42 / 0.08);
          }
          .settings-map-grid {
            background:
              linear-gradient(90deg, rgba(148, 163, 184, 0.16) 1px, transparent 1px),
              linear-gradient(0deg, rgba(148, 163, 184, 0.16) 1px, transparent 1px),
              radial-gradient(circle at 25% 25%, rgba(26, 87, 219, 0.12), transparent 25%),
              radial-gradient(circle at 78% 70%, rgba(249, 115, 22, 0.12), transparent 24%),
              #f8fafc;
            background-size: 42px 42px, 42px 42px, 100% 100%, 100% 100%, auto;
          }
        `}
      </style>

      <div className="relative mx-auto flex w-full max-w-[1440px] flex-col gap-6 pb-28">
        {(success || error) && (
          <div
            className={`rounded-xl border px-4 py-3 text-sm font-semibold ${
              success
                ? 'border-emerald-200 bg-emerald-50 text-emerald-700'
                : 'border-red-200 bg-red-50 text-red-700'
            }`}
          >
            {success || error}
          </div>
        )}

        <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
          <aside className="flex flex-col gap-2 lg:col-span-1">
            <div className="settings-card-shadow flex flex-col gap-1 rounded-xl border border-slate-200 bg-white p-3">
              {SETTINGS_MENU.map((item) => (
                <button
                  key={item.label}
                  type="button"
                  className={`flex items-center gap-3 rounded-lg px-4 py-3 text-left font-medium transition-colors ${
                    item.active
                      ? 'bg-primary/10 font-bold text-primary'
                      : 'text-slate-600 hover:bg-slate-50'
                  }`}
                >
                  <span
                    className={`material-symbols-outlined text-[20px] ${
                      item.active ? 'filled' : ''
                    }`}
                  >
                    {item.icon}
                  </span>
                  {item.label}
                </button>
              ))}
            </div>
          </aside>

          <main className="lg:col-span-2">
            <div className="settings-card-shadow overflow-hidden rounded-xl border border-slate-200 bg-white">
              <div className="border-b border-slate-200 bg-slate-50/50 px-6 py-5">
                <h2 className="text-xl font-bold text-slate-900">General Information</h2>
                <p className="mt-1 text-sm text-slate-500">
                  Update your store&apos;s basic details and public-facing information.
                </p>
              </div>

              <div className="p-6">
                <form className="flex flex-col gap-6" onSubmit={handleSubmit}>
                  <section className="flex items-start gap-6 border-b border-slate-100 pb-6">
                    {form.logoUrl ? (
                      <div
                        className="size-24 shrink-0 overflow-hidden rounded-full border border-slate-200 bg-slate-100 bg-cover bg-center shadow-sm"
                        style={{ backgroundImage: `url("${form.logoUrl}")` }}
                      />
                    ) : (
                      <div className="flex size-24 shrink-0 items-center justify-center overflow-hidden rounded-full border border-slate-200 bg-slate-100 shadow-sm">
                        <span className="material-symbols-outlined text-4xl text-slate-300">
                          store
                        </span>
                      </div>
                    )}

                    <div className="mt-2 flex flex-col gap-2">
                      <h3 className="text-sm font-bold text-slate-900">Store Logo</h3>
                      <p className="text-xs text-slate-500">
                        Recommended size: 512x512px. Max 2MB (JPG, PNG).
                      </p>
                      <div className="mt-2 flex gap-3">
                        <button
                          type="button"
                          className="rounded-md border border-slate-300 bg-white px-3 py-1.5 text-sm font-semibold text-slate-700 shadow-sm transition-colors hover:border-slate-400 hover:bg-slate-50"
                        >
                          Change Layout
                        </button>
                        <button
                          type="button"
                          onClick={() => updateField('logoUrl', '')}
                          className="rounded-md px-3 py-1.5 text-sm font-semibold text-red-600 transition-colors hover:bg-red-50 hover:text-red-700"
                        >
                          Remove
                        </button>
                      </div>
                    </div>
                  </section>

                  <section className="grid grid-cols-1 gap-6 md:grid-cols-2">
                    <div className="flex flex-col gap-1.5 md:col-span-2">
                      <label className="text-xs font-bold uppercase tracking-widest text-slate-700">
                        Store Name
                      </label>
                      <input
                        type="text"
                        className="rounded-lg border border-slate-300 bg-slate-100 px-4 py-2 text-sm text-slate-500"
                        value={form.storeName}
                        disabled
                      />
                      <span className="text-[11px] text-slate-400">
                        Contact admin to change store name.
                      </span>
                    </div>

                    <div className="flex flex-col gap-1.5">
                      <label className="text-xs font-bold uppercase tracking-widest text-slate-700">
                        Contact Phone <span className="text-red-500">*</span>
                      </label>
                      <input
                        type="tel"
                        required
                        className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm outline-none transition-shadow focus:border-primary focus:ring-1 focus:ring-primary"
                        placeholder="+84 123 456 789"
                        value={form.phone}
                        onChange={(event) => updateField('phone', event.target.value)}
                      />
                    </div>

                    <div className="flex flex-col gap-1.5">
                      <label className="text-xs font-bold uppercase tracking-widest text-slate-700">
                        Support Email <span className="text-red-500">*</span>
                      </label>
                      <input
                        type="email"
                        required
                        className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm outline-none transition-shadow focus:border-primary focus:ring-1 focus:ring-primary"
                        placeholder="support@mystore.com"
                        value={form.email}
                        onChange={(event) => updateField('email', event.target.value)}
                      />
                    </div>

                    <div className="flex flex-col gap-1.5 md:col-span-2">
                      <label className="text-xs font-bold uppercase tracking-widest text-slate-700">
                        Store Description
                      </label>
                      <textarea
                        rows={4}
                        className="resize-none rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm outline-none transition-shadow focus:border-primary focus:ring-1 focus:ring-primary"
                        placeholder="Tell customers what your store is about..."
                        value={form.description}
                        onChange={(event) => updateField('description', event.target.value)}
                      />
                    </div>

                    <div className="mt-4 flex flex-col gap-1.5 border-t border-slate-100 pt-4 md:col-span-2">
                      <h3 className="text-lg font-bold text-slate-900">Location & Map</h3>
                      <p className="mb-2 text-sm text-slate-500">
                        Pinpoint your exact store location so shipping fees can be calculated
                        correctly.
                      </p>
                    </div>

                    <div className="relative flex flex-col gap-1.5 md:col-span-2">
                      <label className="text-xs font-bold uppercase tracking-widest text-slate-700">
                        Address Search
                      </label>
                      <div className="relative">
                        <input
                          type="text"
                          className="w-full rounded-lg border border-slate-300 bg-white py-2 pl-10 pr-24 text-sm outline-none transition-shadow focus:border-primary focus:ring-1 focus:ring-primary"
                          placeholder="Search for your address or drop a pin on the map..."
                          value={searchValue}
                          onChange={(event) => setSearchValue(event.target.value)}
                          onBlur={applySearchToAddress}
                        />
                        <span className="material-symbols-outlined absolute left-3 top-2 text-slate-400">
                          search
                        </span>
                        <button
                          type="button"
                          onClick={applySearchToAddress}
                          className="absolute right-1.5 top-1.5 rounded-md bg-slate-900 px-3 py-1.5 text-xs font-bold text-white transition-colors hover:bg-slate-800"
                        >
                          Apply
                        </button>
                      </div>
                    </div>

                    <div
                      role="button"
                      tabIndex={0}
                      onClick={handleMapClick}
                      onKeyDown={(event) => {
                        if (event.key === 'Enter' || event.key === ' ') {
                          setForm((current) =>
                            current
                              ? {
                                  ...current,
                                  latitude: String(DEFAULT_CENTER.lat),
                                  longitude: String(DEFAULT_CENTER.lng),
                                }
                              : current,
                          )
                        }
                      }}
                      className="settings-map-grid relative h-[400px] w-full cursor-crosshair overflow-hidden rounded-xl border border-slate-200 md:col-span-2"
                    >
                      <div className="absolute left-[7%] top-[18%] h-12 w-36 rounded-full bg-blue-100/60 blur-sm" />
                      <div className="absolute bottom-[14%] right-[12%] h-16 w-40 rounded-full bg-orange-100/60 blur-sm" />
                      <div className="absolute left-[18%] top-[52%] h-1.5 w-[68%] -rotate-6 rounded-full bg-slate-300/60" />
                      <div className="absolute left-[42%] top-[8%] h-[82%] w-1.5 rotate-12 rounded-full bg-slate-300/50" />
                      <div
                        className="absolute z-10 -translate-x-1/2 -translate-y-full text-orange-500 drop-shadow"
                        style={markerStyle}
                      >
                        <span className="material-symbols-outlined filled text-5xl">location_on</span>
                      </div>
                      <div className="absolute bottom-4 right-4 rounded-lg border border-slate-100 bg-white/90 px-3 py-2 text-xs font-medium text-slate-600 shadow backdrop-blur-sm">
                        {coordinateLabel(form.latitude, form.longitude)}
                      </div>
                    </div>

                    <input type="hidden" value={form.latitude} readOnly />
                    <input type="hidden" value={form.longitude} readOnly />

                    <div className="flex flex-col gap-1.5">
                      <label className="text-xs font-bold uppercase tracking-widest text-slate-700">
                        Detailed Address
                      </label>
                      <input
                        type="text"
                        className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm outline-none transition-shadow focus:border-primary focus:ring-1 focus:ring-primary"
                        placeholder="House number, street name"
                        value={form.addressLine}
                        onChange={(event) => updateField('addressLine', event.target.value)}
                      />
                    </div>

                    <div className="flex flex-col gap-1.5">
                      <label className="text-xs font-bold uppercase tracking-widest text-slate-700">
                        Province/City
                      </label>
                      <input
                        type="text"
                        className="rounded-lg border border-slate-300 bg-white px-4 py-2 text-sm outline-none transition-shadow focus:border-primary focus:ring-1 focus:ring-primary"
                        placeholder="Hanoi"
                        value={form.province}
                        onChange={(event) => updateField('province', event.target.value)}
                      />
                    </div>
                  </section>

                  <div className="mt-4 flex justify-end gap-3 border-t border-slate-100 pt-6">
                    <button
                      type="button"
                      onClick={discardChanges}
                      className="rounded-lg border border-slate-300 bg-white px-5 py-2.5 text-sm font-bold text-slate-600 shadow-sm transition-colors hover:bg-slate-50"
                    >
                      Discard Changes
                    </button>
                    <button
                      type="submit"
                      disabled={saving}
                      className="seller-primary-button flex items-center gap-2 rounded-lg bg-primary px-5 py-2.5 text-sm font-bold text-white shadow-md shadow-primary/20 transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      <span className="material-symbols-outlined text-[18px]">save</span>
                      {saving ? 'Saving...' : 'Save Changes'}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </main>
        </div>
      </div>
    </SellerLayout>
  )
}
