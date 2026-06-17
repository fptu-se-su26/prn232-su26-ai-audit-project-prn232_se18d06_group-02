import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { adminApi, type AdminSettingsMap } from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const defaultSettings: AdminSettingsMap = {
  Payment_CommissionRate: '0.05',
  Payment_MinimumPayout: '50000',
  Payment_OnlinePayments: 'true',
  Payment_CashOnDelivery: 'true',
  Payment_WebhookSecret: '',
  Store_NewStoreApproval: 'true',
  Store_IndividualSellers: 'true',
  Store_DefaultStatus: 'Pending',
  Order_AutoCompleteDays: '7',
  Order_AutoCancelMinutes: '30',
  Order_BuyerCancellation: 'true',
  Finance_ManualWithdraw: 'true',
  Finance_HoldFunds: 'true',
  Finance_PayoutDelayDays: '7',
  Security_KYCRequired: 'false',
  Security_TaxCodeVerification: 'true',
}

function settingValue(settings: AdminSettingsMap, key: string) {
  return settings[key] ?? defaultSettings[key] ?? ''
}

function isEnabled(settings: AdminSettingsMap, key: string) {
  return settingValue(settings, key).toLowerCase() === 'true'
}

function SectionCard({
  children,
  icon,
  iconClassName,
  title,
  wide = false,
}: {
  children: React.ReactNode
  icon: string
  iconClassName: string
  title: string
  wide?: boolean
}) {
  return (
    <section className={`rounded-xl border border-slate-200 bg-white p-6 shadow-sm ${wide ? 'lg:col-span-2' : ''}`}>
      <div className="mb-6 flex items-center gap-3">
        <div className={`rounded-lg p-2 ${iconClassName}`}>
          <span className="material-symbols-outlined">{icon}</span>
        </div>
        <h2 className="text-lg font-bold text-slate-950">{title}</h2>
      </div>
      {children}
    </section>
  )
}

function TextField({
  label,
  name,
  onChange,
  step,
  suffix,
  type = 'text',
  value,
}: {
  label: string
  name: string
  onChange: (key: string, value: string) => void
  step?: string
  suffix?: string
  type?: string
  value: string
}) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-sm font-medium text-slate-500">{label}</span>
      <div className="relative">
        <input
          className={`w-full rounded-lg border border-slate-200 bg-slate-50 py-2.5 pl-4 text-sm text-slate-900 outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20 ${suffix ? 'pr-14' : 'pr-4'}`}
          name={name}
          onChange={(event) => onChange(name, event.target.value)}
          step={step}
          type={type}
          value={value}
        />
        {suffix && <span className="absolute right-3 top-2.5 text-sm font-medium text-slate-400">{suffix}</span>}
      </div>
    </label>
  )
}

function ToggleField({
  checked,
  description,
  label,
  name,
  onChange,
}: {
  checked: boolean
  description: string
  label: string
  name: string
  onChange: (key: string, value: boolean) => void
}) {
  return (
    <div className="flex items-center justify-between gap-4">
      <div>
        <p className="text-sm font-semibold text-slate-900">{label}</p>
        <p className="mt-0.5 text-xs text-slate-500">{description}</p>
      </div>
      <button
        type="button"
        aria-pressed={checked}
        onClick={() => onChange(name, !checked)}
        className={`relative h-6 w-11 shrink-0 rounded-full transition ${checked ? 'bg-primary' : 'bg-slate-200'}`}
      >
        <span
          className={`absolute left-0.5 top-0.5 size-5 rounded-full border border-slate-300 bg-white transition ${checked ? 'translate-x-5 border-white' : ''}`}
        />
      </button>
    </div>
  )
}

function SkeletonSettings() {
  return (
    <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
      {Array.from({ length: 5 }).map((_, index) => (
        <div key={index} className={`h-72 animate-pulse rounded-xl bg-slate-200 ${index === 4 ? 'lg:col-span-2' : ''}`} />
      ))}
    </div>
  )
}

export default function AdminSettingsPage() {
  const [settings, setSettings] = useState<AdminSettingsMap>(defaultSettings)
  const [lastSynced, setLastSynced] = useState('Never')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [message, setMessage] = useState('')
  const [error, setError] = useState('')

  const hasSettings = useMemo(() => Object.keys(settings).length > 0, [settings])

  useEffect(() => {
    const loadSettings = async () => {
      setLoading(true)
      setError('')

      try {
        const data = await adminApi.settings.get()
        setSettings({ ...defaultSettings, ...data.settings })
        setLastSynced(data.lastSynced || 'Never')
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unable to load platform settings.')
      } finally {
        setLoading(false)
      }
    }

    void loadSettings()
  }, [])

  const updateField = (key: string, value: string) => {
    setMessage('')
    setError('')
    setSettings((current) => ({ ...current, [key]: value }))
  }

  const updateToggle = (key: string, value: boolean) => {
    updateField(key, value ? 'true' : 'false')
  }

  const handleReset = () => {
    setSettings(defaultSettings)
    setMessage('Default settings are ready to save.')
    setError('')
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSaving(true)
    setMessage('')
    setError('')

    try {
      const payload = { ...defaultSettings, ...settings }
      const responseMessage = await adminApi.settings.update(payload)
      const refreshed = await adminApi.settings.get()
      setSettings({ ...defaultSettings, ...refreshed.settings })
      setLastSynced(refreshed.lastSynced || 'Never')
      setMessage(responseMessage || 'Platform settings have been successfully updated.')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update settings.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <AdminLayout activePage="Settings" breadcrumb={['Dashboard', 'Platform Settings']} pageHeader="Platform Settings">
      <form onSubmit={handleSubmit} className="flex flex-col gap-8 pb-4">
        <div className="flex flex-col justify-between gap-4 md:flex-row md:items-center">
          <p className="text-sm text-slate-500">Manage global marketplace configurations, payment policies, and security settings.</p>
          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={handleReset}
              disabled={saving || loading}
              className="rounded-lg border border-slate-200 bg-white px-4 py-2.5 text-sm font-semibold text-slate-700 shadow-sm transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
            >
              Reset to Default
            </button>
            <button
              type="submit"
              disabled={saving || loading || !hasSettings}
              className="flex items-center gap-2 rounded-lg bg-primary px-4 py-2.5 text-sm font-semibold text-white shadow-sm transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <span className="material-symbols-outlined text-[18px]">save</span>
              {saving ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </div>

        {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}
        {message && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">{message}</div>}

        {loading ? (
          <SkeletonSettings />
        ) : (
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <SectionCard icon="payments" iconClassName="bg-blue-50 text-primary" title="Payment Configuration">
              <div className="space-y-6">
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <TextField
                    label="Commission Rate (Decimal 0.05 = 5%)"
                    name="Payment_CommissionRate"
                    onChange={updateField}
                    step="0.01"
                    type="number"
                    value={settingValue(settings, 'Payment_CommissionRate')}
                  />
                  <TextField
                    label="Minimum Payout (VND)"
                    name="Payment_MinimumPayout"
                    onChange={updateField}
                    suffix="VND"
                    type="number"
                    value={settingValue(settings, 'Payment_MinimumPayout')}
                  />
                </div>
                <TextField
                  label="Webhook Signature Secret"
                  name="Payment_WebhookSecret"
                  onChange={updateField}
                  value={settingValue(settings, 'Payment_WebhookSecret')}
                />
                <div className="h-px bg-slate-100" />
                <ToggleField
                  checked={isEnabled(settings, 'Payment_OnlinePayments')}
                  description="Allow credit cards & e-wallets"
                  label="Online Payments"
                  name="Payment_OnlinePayments"
                  onChange={updateToggle}
                />
                <ToggleField
                  checked={isEnabled(settings, 'Payment_CashOnDelivery')}
                  description="Allow payment upon receipt"
                  label="Cash on Delivery (COD)"
                  name="Payment_CashOnDelivery"
                  onChange={updateToggle}
                />
              </div>
            </SectionCard>

            <SectionCard icon="storefront" iconClassName="bg-purple-50 text-purple-600" title="Store Configuration">
              <div className="space-y-6">
                <ToggleField
                  checked={isEnabled(settings, 'Store_NewStoreApproval')}
                  description="Manually approve new vendors"
                  label="New Store Approval"
                  name="Store_NewStoreApproval"
                  onChange={updateToggle}
                />
                <ToggleField
                  checked={isEnabled(settings, 'Store_IndividualSellers')}
                  description="Allow non-business entities to sell"
                  label="Individual Sellers"
                  name="Store_IndividualSellers"
                  onChange={updateToggle}
                />
                <label className="block">
                  <span className="mb-1.5 block text-sm font-medium text-slate-500">Default Store Status</span>
                  <select
                    name="Store_DefaultStatus"
                    onChange={(event) => updateField('Store_DefaultStatus', event.target.value)}
                    value={settingValue(settings, 'Store_DefaultStatus')}
                    className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-900 outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/20"
                  >
                    <option value="Active">Active</option>
                    <option value="Pending">Pending</option>
                    <option value="Inactive">Inactive</option>
                  </select>
                </label>
              </div>
            </SectionCard>

            <SectionCard icon="shopping_cart" iconClassName="bg-orange-50 text-orange-600" title="Order Configuration">
              <div className="space-y-6">
                <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                  <TextField
                    label="Auto Complete (Days)"
                    name="Order_AutoCompleteDays"
                    onChange={updateField}
                    suffix="Days"
                    type="number"
                    value={settingValue(settings, 'Order_AutoCompleteDays')}
                  />
                  <TextField
                    label="Auto Cancel (Minutes)"
                    name="Order_AutoCancelMinutes"
                    onChange={updateField}
                    suffix="Min"
                    type="number"
                    value={settingValue(settings, 'Order_AutoCancelMinutes')}
                  />
                </div>
                <ToggleField
                  checked={isEnabled(settings, 'Order_BuyerCancellation')}
                  description="Allow buyers to cancel pending orders"
                  label="Buyer Cancellation"
                  name="Order_BuyerCancellation"
                  onChange={updateToggle}
                />
              </div>
            </SectionCard>

            <SectionCard icon="account_balance_wallet" iconClassName="bg-green-50 text-green-600" title="Payout & Finance">
              <div className="space-y-6">
                <ToggleField
                  checked={isEnabled(settings, 'Finance_ManualWithdraw')}
                  description="Vendors must request payouts manually"
                  label="Manual Withdraw Request"
                  name="Finance_ManualWithdraw"
                  onChange={updateToggle}
                />
                <ToggleField
                  checked={isEnabled(settings, 'Finance_HoldFunds')}
                  description="Release funds only after order completion"
                  label="Hold Funds until Complete"
                  name="Finance_HoldFunds"
                  onChange={updateToggle}
                />
                <TextField
                  label="Payout Delay (Days)"
                  name="Finance_PayoutDelayDays"
                  onChange={updateField}
                  suffix="Days"
                  type="number"
                  value={settingValue(settings, 'Finance_PayoutDelayDays')}
                />
              </div>
            </SectionCard>

            <SectionCard icon="shield" iconClassName="bg-red-50 text-red-600" title="Security & Compliance" wide>
              <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
                <ToggleField
                  checked={isEnabled(settings, 'Security_KYCRequired')}
                  description="Require sellers to upload identification documents before they can start selling."
                  label="KYC Verification Required"
                  name="Security_KYCRequired"
                  onChange={updateToggle}
                />
                <ToggleField
                  checked={isEnabled(settings, 'Security_TaxCodeVerification')}
                  description="Mandatory tax code input for business sellers during registration."
                  label="Tax Code Verification"
                  name="Security_TaxCodeVerification"
                  onChange={updateToggle}
                />
              </div>
            </SectionCard>
          </div>
        )}

        <div className="sticky bottom-0 z-10 -mx-6 border-t border-slate-200 bg-white px-6 py-4 shadow-[0_-4px_6px_-1px_rgba(0,0,0,0.05)]">
          <div className="mx-auto flex max-w-7xl items-center justify-between gap-4">
            <div className="flex items-center gap-2 text-sm text-slate-500">
              <span className="material-symbols-outlined text-[18px]">sync</span>
              <span>
                Last updated: <span className="font-medium text-slate-900">{lastSynced}</span>
              </span>
            </div>
            <button
              type="submit"
              disabled={saving || loading || !hasSettings}
              className="rounded-lg bg-primary px-6 py-2.5 text-sm font-bold text-white shadow-sm transition hover:bg-blue-700 active:scale-95 disabled:cursor-not-allowed disabled:opacity-60"
            >
              {saving ? 'Saving...' : 'Save All Changes'}
            </button>
          </div>
        </div>
      </form>
    </AdminLayout>
  )
}
