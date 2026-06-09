import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface StoreSettings {
  name: string
  description?: string
  logoUrl?: string
  bannerUrl?: string
  contactEmail?: string
  contactPhone?: string
  returnPolicy?: string
  shippingPolicy?: string
}

export default function SellerSettingsPage() {
  const [settings, setSettings] = useState<StoreSettings | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [success, setSuccess] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    sellerApi
      .getStoreSettings()
      .then((d) => setSettings(d as StoreSettings))
      .finally(() => setLoading(false))
  }, [])

  const handleChange = (field: keyof StoreSettings, value: string) => {
    setSettings((s) => (s ? { ...s, [field]: value } : s))
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!settings) return
    setSaving(true)
    setError('')
    setSuccess('')
    try {
      await sellerApi.updateStoreSettings(settings)
      setSuccess('Store settings saved successfully.')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save settings.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) {
    return (
      <SellerLayout pageHeader="Store Settings" breadcrumb={['Settings']}>
        <div className="max-w-2xl space-y-4">
          {Array.from({ length: 5 }).map((_, i) => (
            <div className="h-14 animate-pulse rounded-xl bg-white" key={i} />
          ))}
        </div>
      </SellerLayout>
    )
  }

  if (!settings) {
    return (
      <SellerLayout pageHeader="Store Settings" breadcrumb={['Settings']}>
        <div className="rounded-xl border border-red-200 bg-red-50 p-8 text-center text-red-600">
          Failed to load store settings.
        </div>
      </SellerLayout>
    )
  }

  return (
    <SellerLayout pageHeader="Store Settings" breadcrumb={['Settings']}>
      <form className="max-w-2xl" onSubmit={handleSubmit}>
        <div className="space-y-6">
          {/* Basic info */}
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="mb-5 text-sm font-semibold uppercase tracking-wider text-slate-500">
              Basic Information
            </h2>
            <div className="space-y-4">
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">
                  Store Name <span className="text-red-500">*</span>
                </label>
                <input
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  onChange={(e) => handleChange('name', e.target.value)}
                  required
                  value={settings.name}
                />
              </div>
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">Description</label>
                <textarea
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  onChange={(e) => handleChange('description', e.target.value)}
                  placeholder="Describe your store to customers"
                  rows={3}
                  value={settings.description ?? ''}
                />
              </div>
            </div>
          </div>

          {/* Media */}
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="mb-5 text-sm font-semibold uppercase tracking-wider text-slate-500">
              Media
            </h2>
            <div className="space-y-4">
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">Logo URL</label>
                <input
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  onChange={(e) => handleChange('logoUrl', e.target.value)}
                  placeholder="https://…"
                  type="url"
                  value={settings.logoUrl ?? ''}
                />
                {settings.logoUrl && (
                  <img
                    alt="Logo preview"
                    className="mt-2 h-16 rounded-lg border border-slate-200 object-contain"
                    src={settings.logoUrl}
                  />
                )}
              </div>
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">Banner URL</label>
                <input
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  onChange={(e) => handleChange('bannerUrl', e.target.value)}
                  placeholder="https://…"
                  type="url"
                  value={settings.bannerUrl ?? ''}
                />
                {settings.bannerUrl && (
                  <img
                    alt="Banner preview"
                    className="mt-2 h-24 w-full rounded-lg border border-slate-200 object-cover"
                    src={settings.bannerUrl}
                  />
                )}
              </div>
            </div>
          </div>

          {/* Contact */}
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="mb-5 text-sm font-semibold uppercase tracking-wider text-slate-500">
              Contact
            </h2>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">
                  Contact Email
                </label>
                <input
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  onChange={(e) => handleChange('contactEmail', e.target.value)}
                  placeholder="store@example.com"
                  type="email"
                  value={settings.contactEmail ?? ''}
                />
              </div>
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">
                  Contact Phone
                </label>
                <input
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  onChange={(e) => handleChange('contactPhone', e.target.value)}
                  placeholder="0900 000 000"
                  value={settings.contactPhone ?? ''}
                />
              </div>
            </div>
          </div>

          {/* Policies */}
          <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="mb-5 text-sm font-semibold uppercase tracking-wider text-slate-500">
              Policies
            </h2>
            <div className="space-y-4">
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">
                  Return Policy
                </label>
                <textarea
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  onChange={(e) => handleChange('returnPolicy', e.target.value)}
                  placeholder="Describe your return and refund policy"
                  rows={4}
                  value={settings.returnPolicy ?? ''}
                />
              </div>
              <div>
                <label className="mb-1.5 block text-sm font-medium text-slate-700">
                  Shipping Policy
                </label>
                <textarea
                  className="w-full rounded-lg border border-slate-200 px-3 py-2 text-sm focus:border-[#ff6b00] focus:outline-none focus:ring-1 focus:ring-[#ff6b00]"
                  onChange={(e) => handleChange('shippingPolicy', e.target.value)}
                  placeholder="Describe your shipping methods and timelines"
                  rows={4}
                  value={settings.shippingPolicy ?? ''}
                />
              </div>
            </div>
          </div>
        </div>

        {error && (
          <p className="mt-4 rounded-lg border border-red-200 bg-red-50 px-4 py-2 text-sm text-red-600">
            {error}
          </p>
        )}
        {success && (
          <p className="mt-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-2 text-sm text-emerald-700">
            {success}
          </p>
        )}

        <div className="mt-6">
          <button
            className="rounded-lg bg-[#ff6b00] px-8 py-2.5 text-sm font-semibold text-white transition hover:bg-[#ea580c] disabled:opacity-60"
            disabled={saving}
            type="submit"
          >
            {saving ? 'Saving…' : 'Save Settings'}
          </button>
        </div>
      </form>
    </SellerLayout>
  )
}
