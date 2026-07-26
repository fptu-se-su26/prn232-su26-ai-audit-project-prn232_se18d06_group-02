import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import ChatInboxLayout from '@/components/chat/ChatInboxLayout'
import EmptyState from '@/components/ui/EmptyState'
import { usersApi } from '@/api/users'
import apiClient from '@/api/apiClient'
import { useAuth } from '@/contexts/useAuth'
import { useChatContext } from '@/contexts/useChatContext'
import type { PagedResult } from '@/types/catalog'

type ProfileTab = 'account' | 'orders' | 'messages' | 'addresses' | 'reviews' | 'password'
type AddressType = 'Home' | 'Office' | 'Other'

interface StoreSummary {
  id: string
  status: 'Pending' | 'Approved' | 'Rejected' | string | number
  rejectReason?: string | null
}

interface OrderStatusSummary {
  all: number
  processing: number
  delivered: number
  cancelled: number
  toReview: number
}

interface UserOrderItem {
  orderItemId: string
  productName: string
  productSlug: string
  productImageUrl?: string | null
  variantName: string
  quantity: number
  unitPrice: number
  canReview: boolean
  canEditReview: boolean
  reviewDeadline?: string | null
}

interface UserOrder {
  subOrderId: string
  orderId: string
  storeName: string
  storeSlug: string
  orderCode: number
  status: string | number
  createdAt: string
  deliveredAt?: string | null
  subtotal: number
  hasAnyReviewableItem: boolean
  hasAnyEditableReview: boolean
  items: UserOrderItem[]
}

interface OrdersResponse {
  summary: OrderStatusSummary
  orders: PagedResult<UserOrder>
}

interface UserAddress {
  id: string
  fullName: string
  phoneNumber: string
  addressLine: string
  ward?: string | null
  district?: string | null
  province?: string | null
  latitude: number
  longitude: number
  addressType: AddressType | number
  isDefault: boolean
}

interface AddressForm {
  id?: string
  fullName: string
  phoneNumber: string
  addressLine: string
  ward: string
  district: string
  province: string
  latitude: number
  longitude: number
  addressType: AddressType
  isDefault: boolean
}

interface MyReview {
  id: string
  orderItemId: string
  productName: string
  productSlug: string
  productImageUrl?: string | null
  variantName: string
  storeName: string
  rating: number
  comment?: string | null
  createdAt: string
  deliveredAt: string
  reviewDeadline: string
  canEdit: boolean
  sellerReplyContent?: string | null
  sellerReplyAt?: string | null
}

interface MapPrediction {
  place_id: string
  description: string
  structured_formatting?: {
    main_text?: string
    secondary_text?: string
  }
}

const tabs: Array<{ key: ProfileTab; icon: string; label: string }> = [
  { key: 'account', icon: 'person', label: 'My Account' },
  { key: 'orders', icon: 'shopping_bag', label: 'Orders' },
  { key: 'messages', icon: 'chat', label: 'Messages' },
  { key: 'addresses', icon: 'location_on', label: 'Addresses' },
  { key: 'reviews', icon: 'star', label: 'Reviews' },
  { key: 'password', icon: 'lock', label: 'Password' },
]

const statusClasses: Record<string, string> = {
  Pending: 'border-amber-200 bg-amber-50 text-amber-700',
  Approved: 'border-sky-200 bg-sky-50 text-sky-700',
  Paid: 'border-indigo-200 bg-indigo-50 text-indigo-700',
  Processing: 'border-blue-200 bg-blue-50 text-blue-700',
  Delivered: 'border-emerald-200 bg-emerald-50 text-emerald-700',
  Cancelled: 'border-rose-200 bg-rose-50 text-rose-700',
  Refunded: 'border-orange-200 bg-orange-50 text-orange-700',
  Rejected: 'border-red-200 bg-red-50 text-red-700',
}

const orderStatusNames = ['Pending', 'AwaitingPayment', 'Approved', 'Rejected', 'Paid', 'Processing', 'Delivered', 'Cancelled', 'Completed', 'Refunded']
const storeStatusNames = ['Draft', 'Pending', 'Approved', 'Rejected', 'Locked']
const addressTypeNames: AddressType[] = ['Home', 'Office', 'Other']

const emptyAddressForm: AddressForm = {
  fullName: '',
  phoneNumber: '',
  addressLine: '',
  ward: '',
  district: '',
  province: '',
  latitude: 10.762622,
  longitude: 106.660172,
  addressType: 'Home',
  isDefault: false,
}

function formatMoney(value: number) {
  return new Intl.NumberFormat('vi-VN').format(value) + ' VND'
}

function formatDate(value?: string | null) {
  if (!value) return 'N/A'
  return new Date(value).toLocaleDateString('vi-VN', { year: 'numeric', month: 'short', day: '2-digit' })
}

function getPagedItems<T>(result: PagedResult<T> | undefined) {
  return result?.items ?? []
}

function normalizeOrderStatus(status: string | number) {
  return typeof status === 'number' ? orderStatusNames[status] ?? String(status) : status
}

function normalizeStoreStatus(status: string | number | undefined | null) {
  return typeof status === 'number' ? storeStatusNames[status] ?? String(status) : status ?? ''
}

function normalizeAddressType(addressType: AddressType | number): AddressType {
  return typeof addressType === 'number' ? addressTypeNames[addressType] ?? 'Other' : addressType
}

function buildAddressLine(address: UserAddress) {
  return [address.addressLine, address.ward, address.district, address.province].filter(Boolean).join(', ')
}

export default function ProfilePage() {
  const navigate = useNavigate()
  const [searchParams, setSearchParams] = useSearchParams()
  const { user, refresh, logout } = useAuth()
  const { enabled: chatEnabled } = useChatContext()
  const [activeTab, setActiveTab] = useState<ProfileTab>((searchParams.get('tab') as ProfileTab) || 'orders')
  const [store, setStore] = useState<StoreSummary | null>(null)
  const [ordersData, setOrdersData] = useState<OrdersResponse | null>(null)
  const [reviews, setReviews] = useState<PagedResult<MyReview> | null>(null)
  const [addresses, setAddresses] = useState<UserAddress[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [notice, setNotice] = useState<{ type: 'success' | 'error'; text: string } | null>(null)
  const [accountForm, setAccountForm] = useState({ fullName: user?.fullName ?? '', phoneNumber: user?.phoneNumber ?? '', avatarUrl: user?.avatarUrl ?? '' })
  const [passwordForm, setPasswordForm] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' })
  const [addressModalOpen, setAddressModalOpen] = useState(false)
  const [addressForm, setAddressForm] = useState<AddressForm>(emptyAddressForm)
  const [addressSuggestions, setAddressSuggestions] = useState<MapPrediction[]>([])

  const orderStatus = searchParams.get('orderStatus') ?? 'all'
  const orderPage = Number(searchParams.get('orderPage') ?? '1')
  const reviewPage = Number(searchParams.get('reviewPage') ?? '1')
  const searchTerm = searchParams.get('searchTerm') ?? ''

  useEffect(() => {
    setAccountForm({
      fullName: user?.fullName ?? '',
      phoneNumber: user?.phoneNumber ?? '',
      avatarUrl: user?.avatarUrl ?? '',
    })
  }, [user])

  useEffect(() => {
    const tab = (searchParams.get('tab') as ProfileTab) || 'orders'
    setActiveTab(tabs.some((item) => item.key === tab) ? tab : 'orders')
  }, [searchParams])

  useEffect(() => {
    let cancelled = false

    async function load() {
      setLoading(true)
      setNotice(null)
      try {
        const storePromise = usersApi.myStore().catch(() => null)
        if (activeTab === 'orders') {
          const [storeResult, ordersResult] = await Promise.all([
            storePromise,
            usersApi.getOrders({ status: orderStatus, searchTerm, pageNumber: orderPage, pageSize: 5 }),
          ])
          if (!cancelled) {
            setStore(storeResult as StoreSummary | null)
            setOrdersData(ordersResult as OrdersResponse)
          }
        } else if (activeTab === 'reviews') {
          const [storeResult, reviewsResult] = await Promise.all([storePromise, usersApi.getReviews(reviewPage)])
          if (!cancelled) {
            setStore(storeResult as StoreSummary | null)
            setReviews(reviewsResult as PagedResult<MyReview>)
          }
        } else if (activeTab === 'addresses') {
          const [storeResult, addressesResult] = await Promise.all([storePromise, usersApi.getAddresses()])
          if (!cancelled) {
            setStore(storeResult as StoreSummary | null)
            setAddresses(addressesResult as UserAddress[])
          }
        } else {
          const storeResult = await storePromise
          if (!cancelled) setStore(storeResult as StoreSummary | null)
        }
      } catch (error) {
        if (!cancelled) setNotice({ type: 'error', text: error instanceof Error ? error.message : 'Could not load profile data.' })
      } finally {
        if (!cancelled) setLoading(false)
      }
    }

    void load()
    return () => {
      cancelled = true
    }
  }, [activeTab, orderStatus, orderPage, reviewPage, searchTerm])

  useEffect(() => {
    if (!addressModalOpen || addressForm.addressLine.trim().length < 2) {
      setAddressSuggestions([])
      return
    }

    const timeoutId = window.setTimeout(async () => {
      try {
        const response = await apiClient.get('/maps/autocomplete', { params: { input: addressForm.addressLine } })
        const data = response.data as { predictions?: MapPrediction[] }
        setAddressSuggestions(data.predictions ?? [])
      } catch {
        setAddressSuggestions([])
      }
    }, 300)

    return () => window.clearTimeout(timeoutId)
  }, [addressForm.addressLine, addressModalOpen])

  const orderItems = useMemo(() => getPagedItems(ordersData?.orders), [ordersData])
  const reviewItems = useMemo(() => getPagedItems(reviews ?? undefined), [reviews])

  const setTab = (tab: ProfileTab) => {
    const next = new URLSearchParams(searchParams)
    next.set('tab', tab)
    if (tab !== 'orders') {
      next.delete('orderStatus')
      next.delete('orderPage')
      next.delete('searchTerm')
    }
    if (tab !== 'reviews') next.delete('reviewPage')
    setSearchParams(next)
  }

  const setOrderFilter = (status: string) => {
    const next = new URLSearchParams(searchParams)
    next.set('tab', 'orders')
    next.set('orderStatus', status)
    next.set('orderPage', '1')
    setSearchParams(next)
  }

  const submitOrderSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    const formData = new FormData(event.currentTarget)
    const next = new URLSearchParams(searchParams)
    next.set('tab', 'orders')
    next.set('orderPage', '1')
    const value = String(formData.get('searchTerm') ?? '').trim()
    if (value) next.set('searchTerm', value)
    else next.delete('searchTerm')
    setSearchParams(next)
  }

  const handleAccountSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setNotice(null)
    try {
      await usersApi.updateProfile(accountForm)
      await refresh()
      setNotice({ type: 'success', text: 'Profile updated successfully.' })
    } catch (error) {
      setNotice({ type: 'error', text: error instanceof Error ? error.message : 'Could not update profile.' })
    } finally {
      setSaving(false)
    }
  }

  const handlePasswordSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setNotice(null)
    try {
      await usersApi.changePassword(passwordForm)
      setPasswordForm({ currentPassword: '', newPassword: '', confirmPassword: '' })
      setNotice({ type: 'success', text: 'Password updated successfully.' })
    } catch (error) {
      setNotice({ type: 'error', text: error instanceof Error ? error.message : 'Could not update password.' })
    } finally {
      setSaving(false)
    }
  }

  const openAddressModal = (address?: UserAddress) => {
    setAddressForm(
      address
        ? {
            id: address.id,
            fullName: address.fullName,
            phoneNumber: address.phoneNumber,
            addressLine: address.addressLine,
            ward: address.ward ?? '',
            district: address.district ?? '',
            province: address.province ?? '',
            latitude: address.latitude,
            longitude: address.longitude,
            addressType: normalizeAddressType(address.addressType),
            isDefault: address.isDefault,
          }
        : { ...emptyAddressForm, fullName: user?.fullName ?? '', phoneNumber: user?.phoneNumber ?? '' },
    )
    setAddressModalOpen(true)
  }

  const saveAddress = async (event: FormEvent) => {
    event.preventDefault()
    setSaving(true)
    setNotice(null)
    try {
      if (addressForm.id) await usersApi.updateAddress(addressForm.id, addressForm)
      else await usersApi.addAddress(addressForm)

      setAddresses((await usersApi.getAddresses()) as UserAddress[])
      setAddressModalOpen(false)
      setNotice({ type: 'success', text: addressForm.id ? 'Address updated.' : 'Address added.' })
    } catch (error) {
      setNotice({ type: 'error', text: error instanceof Error ? error.message : 'Could not save address.' })
    } finally {
      setSaving(false)
    }
  }

  const deleteAddress = async (id: string) => {
    await usersApi.deleteAddress(id)
    setAddresses((current) => current.filter((address) => address.id !== id))
  }

  const setDefaultAddress = async (id: string) => {
    await usersApi.setDefaultAddress(id)
    setAddresses((await usersApi.getAddresses()) as UserAddress[])
  }

  const selectAddressSuggestion = async (prediction: MapPrediction) => {
    setAddressSuggestions([])
    try {
      const response = await apiClient.get('/maps/place-detail', { params: { placeId: prediction.place_id } })
      const data = response.data as { lat?: number; lng?: number; ward?: string; district?: string; province?: string }
      setAddressForm((current) => ({
        ...current,
        addressLine: prediction.description,
        latitude: data.lat ?? current.latitude,
        longitude: data.lng ?? current.longitude,
        ward: data.ward ?? current.ward,
        district: data.district ?? current.district,
        province: data.province ?? current.province,
      }))
    } catch {
      setAddressForm((current) => ({ ...current, addressLine: prediction.description }))
    }
  }

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  const renderSellerAction = () => {
    const storeStatus = normalizeStoreStatus(store?.status)

    if (!store) {
      return (
        <Link className="mt-4 flex items-center justify-center gap-2 rounded-lg border border-orange-200 bg-orange-50 px-4 py-2 text-sm font-bold text-orange-600 hover:bg-orange-100" to="/seller/register">
          <span className="material-symbols-outlined text-[18px]">storefront</span>
          Register as Seller
        </Link>
      )
    }

    if (storeStatus === 'Approved') {
      return (
        <Link className="mt-4 flex items-center justify-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-2 text-sm font-bold text-emerald-700 hover:bg-emerald-100" to="/store-owner">
          <span className="material-symbols-outlined text-[18px]">store</span>
          Go to Store
        </Link>
      )
    }

    const rejected = storeStatus === 'Rejected'
    return (
      <div className="mt-4 space-y-2">
        <div className={`flex items-center justify-center gap-2 rounded-lg border px-4 py-2 text-sm font-bold ${rejected ? 'border-red-200 bg-red-50 text-red-700' : 'border-blue-200 bg-blue-50 text-blue-700'}`}>
          <span className="material-symbols-outlined text-[18px]">{rejected ? 'cancel' : 'hourglass_empty'}</span>
          {rejected ? 'App Rejected' : 'App Pending'}
        </div>
        <Link className="flex items-center justify-center gap-2 rounded-lg border border-orange-200 bg-orange-50 px-4 py-2 text-sm font-bold text-orange-600 hover:bg-orange-100" to="/seller/register?reapply=true">
          <span className="material-symbols-outlined text-[18px]">refresh</span>
          {rejected ? 'Resubmit Application' : 'Edit & Resubmit'}
        </Link>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <div className="flex flex-col gap-8 lg:flex-row">
        <aside className="w-full flex-shrink-0 lg:w-72">
          <div className="overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm">
            <div className="border-b border-gray-50 bg-gray-50/60 p-6 text-center">
              <div className="mx-auto mb-3 flex h-20 w-20 items-center justify-center overflow-hidden rounded-full border-4 border-white bg-primary/10 text-3xl font-bold text-primary shadow-sm">
                {user?.avatarUrl ? <img alt="Avatar" className="h-full w-full object-cover" src={user.avatarUrl} /> : <span className="material-symbols-outlined text-4xl">person</span>}
              </div>
              <h1 className="text-lg font-bold text-gray-900">{user?.fullName || user?.userName}</h1>
              <p className="mt-1 truncate text-sm text-gray-500">{user?.email}</p>
              <span className="mt-3 inline-flex items-center rounded-full border border-orange-100 bg-orange-50 px-2 py-0.5 text-xs font-semibold text-orange-600">
                <span className="material-symbols-outlined mr-1 text-[14px]">star</span>
                Gold Member
              </span>
              {renderSellerAction()}
            </div>

            <nav className="space-y-1 p-3">
              {tabs.map((item) => (
                <button
                  className={`flex w-full items-center gap-3 rounded-xl px-4 py-3 text-left text-sm transition-all ${
                    activeTab === item.key ? 'border-l-4 border-primary bg-primary/5 font-bold text-primary' : 'font-medium text-gray-600 hover:bg-gray-50 hover:text-primary'
                  }`}
                  key={item.key}
                  onClick={() => setTab(item.key)}
                  type="button"
                >
                  <span className={`material-symbols-outlined text-[20px] ${activeTab === item.key ? 'filled' : ''}`}>{item.icon}</span>
                  {item.label}
                </button>
              ))}
              <div className="mt-3 border-t border-gray-50 pt-3">
                <button className="flex w-full items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium text-red-600 hover:bg-red-50" onClick={handleLogout} type="button">
                  <span className="material-symbols-outlined text-[20px]">logout</span>
                  Logout
                </button>
              </div>
            </nav>
          </div>
        </aside>

        <main className="min-w-0 flex-1 space-y-6">
          {notice ? (
            <div className={`rounded-2xl border px-5 py-4 text-sm font-medium ${notice.type === 'success' ? 'border-emerald-200 bg-emerald-50 text-emerald-700' : 'border-red-200 bg-red-50 text-red-700'}`}>
              {notice.text}
            </div>
          ) : null}

          {store && (normalizeStoreStatus(store.status) === 'Pending' || normalizeStoreStatus(store.status) === 'Rejected') ? (
            <div className="rounded-2xl border border-orange-200 bg-orange-50 px-5 py-4">
              <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <div>
                  <p className="text-sm font-bold text-orange-700">{normalizeStoreStatus(store.status) === 'Pending' ? 'Your store application is pending review.' : 'Your store application was rejected.'}</p>
                  {store.rejectReason ? <p className="mt-1 text-xs text-orange-700/90">Reason: {store.rejectReason}</p> : null}
                </div>
                <Link className="inline-flex items-center justify-center gap-2 rounded-xl border border-orange-300 bg-white px-4 py-2 text-sm font-bold text-orange-700 hover:bg-orange-100" to="/seller/register?reapply=true">
                  <span className="material-symbols-outlined text-[18px]">refresh</span>
                  {normalizeStoreStatus(store.status) === 'Pending' ? 'Edit & Resubmit' : 'Resubmit Application'}
                </Link>
              </div>
            </div>
          ) : null}

          {loading ? (
            <div className="flex items-center justify-center rounded-2xl border border-gray-100 bg-white py-16 shadow-sm">
              <span className="material-symbols-outlined animate-spin text-[28px] text-primary">progress_activity</span>
            </div>
          ) : null}

          {activeTab === 'account' ? (
            <section className="space-y-6">
              <h2 className="text-2xl font-bold text-gray-900">My Account</h2>
              <form className="max-w-2xl space-y-6 rounded-2xl border border-gray-100 bg-white p-8 shadow-sm" onSubmit={handleAccountSubmit}>
                <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
                  <label className="space-y-2 text-sm font-semibold text-gray-700">
                    Full Name
                    <input className="block w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" value={accountForm.fullName} onChange={(event) => setAccountForm((current) => ({ ...current, fullName: event.target.value }))} />
                  </label>
                  <label className="space-y-2 text-sm font-semibold text-gray-700">
                    Email Address
                    <input className="block w-full cursor-not-allowed rounded-xl border border-gray-100 bg-gray-50 px-4 py-3 font-normal text-gray-500" disabled value={user?.email ?? ''} />
                  </label>
                  <label className="space-y-2 text-sm font-semibold text-gray-700">
                    Phone Number
                    <input className="block w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" value={accountForm.phoneNumber} onChange={(event) => setAccountForm((current) => ({ ...current, phoneNumber: event.target.value }))} />
                  </label>
                  <label className="space-y-2 text-sm font-semibold text-gray-700">
                    Username
                    <input className="block w-full cursor-not-allowed rounded-xl border border-gray-100 bg-gray-50 px-4 py-3 font-normal text-gray-500" disabled value={user?.userName ?? ''} />
                  </label>
                  <label className="space-y-2 text-sm font-semibold text-gray-700 md:col-span-2">
                    Avatar URL
                    <input className="block w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" value={accountForm.avatarUrl} onChange={(event) => setAccountForm((current) => ({ ...current, avatarUrl: event.target.value }))} />
                  </label>
                </div>
                <button className="rounded-xl bg-primary px-8 py-3 font-bold text-white shadow-lg shadow-primary/20 hover:bg-blue-700 disabled:bg-gray-300" disabled={saving} type="submit">
                  {saving ? 'Saving...' : 'Save Changes'}
                </button>
              </form>
            </section>
          ) : null}

          {activeTab === 'orders' ? (
            <section className="space-y-5">
              <form className="flex justify-end" onSubmit={submitOrderSearch}>
                <div className="relative w-full sm:w-96">
                  <span className="material-symbols-outlined pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-gray-400">search</span>
                  <input className="block w-full rounded-xl border border-gray-200 bg-white py-3 pl-10 pr-4 text-sm focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" defaultValue={searchTerm} name="searchTerm" placeholder="Search by order code or product name" />
                </div>
              </form>

              <div className="hide-scrollbar flex overflow-x-auto rounded-2xl border border-gray-100 bg-white p-1 shadow-sm">
                {[
                  ['all', 'All Orders', ordersData?.summary.all ?? 0],
                  ['processing', 'Processing', ordersData?.summary.processing ?? 0],
                  ['delivered', 'Delivered', ordersData?.summary.delivered ?? 0],
                  ['to_review', 'To Review', ordersData?.summary.toReview ?? 0],
                  ['cancelled', 'Cancelled', ordersData?.summary.cancelled ?? 0],
                ].map(([key, label, count]) => (
                  <button className={`flex-shrink-0 rounded-xl px-5 py-3 text-sm font-semibold ${orderStatus === key ? 'bg-primary text-white shadow-sm' : 'text-gray-600 hover:bg-gray-50'}`} key={key} onClick={() => setOrderFilter(String(key))} type="button">
                    {label}
                    <span className={`ml-2 ${orderStatus === key ? 'text-white/80' : 'text-gray-400'}`}>{count}</span>
                  </button>
                ))}
              </div>

              {orderItems.length === 0 ? (
                <EmptyState icon="shopping_bag" title="No orders found" description="Orders matching this filter will appear here." />
              ) : (
                <div className="space-y-4">
                  {orderItems.map((order) => (
                    <article className="overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm" key={order.subOrderId}>
                      <div className="flex flex-col gap-3 border-b border-gray-100 bg-gray-50/60 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
                        <div>
                          <p className="text-sm font-bold text-gray-900">Order #{order.orderCode}</p>
                          <p className="mt-0.5 text-xs text-gray-500">{order.storeName} · {formatDate(order.createdAt)}</p>
                        </div>
                        <span className={`inline-flex w-fit items-center rounded-full border px-3 py-1 text-xs font-bold ${statusClasses[normalizeOrderStatus(order.status)] ?? 'border-gray-200 bg-gray-50 text-gray-700'}`}>{normalizeOrderStatus(order.status)}</span>
                      </div>
                      <div className="divide-y divide-gray-100">
                        {order.items.map((item) => (
                          <div className="flex gap-4 px-5 py-4" key={item.orderItemId}>
                            <Link className="h-16 w-16 flex-shrink-0 overflow-hidden rounded-xl border border-gray-100 bg-gray-50 p-1" to={`/product/${item.productSlug}`}>
                              {item.productImageUrl ? <img alt={item.productName} className="h-full w-full object-contain" src={item.productImageUrl} /> : <span className="material-symbols-outlined flex h-full items-center justify-center text-gray-300">inventory_2</span>}
                            </Link>
                            <div className="min-w-0 flex-1">
                              <Link className="line-clamp-2 text-sm font-bold text-gray-900 hover:text-primary" to={`/product/${item.productSlug}`}>{item.productName}</Link>
                              <p className="mt-1 text-xs text-gray-500">{item.variantName} · Qty {item.quantity}</p>
                              <p className="mt-1 text-sm font-semibold text-gray-900">{formatMoney(item.unitPrice)}</p>
                            </div>
                            {(item.canReview || item.canEditReview) ? (
                              <Link className="self-start rounded-lg border border-orange-200 bg-orange-50 px-3 py-2 text-xs font-bold text-orange-700 hover:bg-orange-100" to={`/reviews/write/${item.orderItemId}`}>
                                {item.canEditReview ? 'Edit Review' : 'Write Review'}
                              </Link>
                            ) : null}
                          </div>
                        ))}
                      </div>
                      <div className="flex flex-col gap-3 border-t border-gray-100 px-5 py-4 sm:flex-row sm:items-center sm:justify-between">
                        <Link className="inline-flex items-center gap-2 text-sm font-semibold text-primary hover:underline" to={`/orders/track/${order.subOrderId}`}>
                          <span className="material-symbols-outlined text-[18px]">local_shipping</span>
                          Track Order
                        </Link>
                        <p className="text-right text-sm text-gray-500">Subtotal <span className="ml-2 text-base font-bold text-primary">{formatMoney(order.subtotal)}</span></p>
                      </div>
                    </article>
                  ))}
                </div>
              )}
            </section>
          ) : null}

          {activeTab === 'messages' ? (
            <section className="h-[calc(100vh-10rem)] min-h-[36rem] overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-sm">
              {chatEnabled ? <ChatInboxLayout /> : <EmptyState icon="lock" title="Messages are available for customer accounts" />}
            </section>
          ) : null}

          {activeTab === 'addresses' ? (
            <section className="space-y-4">
              <div className="flex items-center justify-between gap-4">
                <h2 className="text-2xl font-bold text-gray-900">Addresses</h2>
                <button className="inline-flex items-center gap-2 rounded-xl bg-primary px-5 py-3 text-sm font-semibold text-white hover:bg-blue-700" onClick={() => openAddressModal()} type="button">
                  <span className="material-symbols-outlined text-[18px]">add</span>
                  Add Address
                </button>
              </div>
              {addresses.length === 0 ? (
                <EmptyState icon="location_on" title="No addresses yet" description="Add a shipping address to speed up checkout." />
              ) : (
                <div className="grid gap-4">
                  {addresses.map((address) => (
                    <article className="flex flex-col gap-4 rounded-2xl border border-gray-100 bg-white p-5 shadow-sm sm:flex-row sm:items-start sm:justify-between" key={address.id}>
                      <div className="flex gap-4">
                        <div className="flex h-10 w-10 flex-shrink-0 items-center justify-center rounded-xl bg-blue-50 text-primary">
                          <span className="material-symbols-outlined">location_on</span>
                        </div>
                        <div>
                          <div className="flex flex-wrap items-center gap-2">
                            <p className="text-sm font-bold text-gray-900">{address.fullName}</p>
                            <span className="rounded-full bg-gray-100 px-2 py-0.5 text-[10px] font-bold text-gray-600">{normalizeAddressType(address.addressType)}</span>
                            {address.isDefault ? <span className="rounded-full bg-blue-100 px-2 py-0.5 text-[10px] font-bold text-primary">Default</span> : null}
                          </div>
                          <p className="mt-1 text-sm text-gray-500">{address.phoneNumber}</p>
                          <p className="mt-1 text-sm text-gray-700">{buildAddressLine(address)}</p>
                        </div>
                      </div>
                      <div className="flex flex-wrap gap-2">
                        {!address.isDefault ? <button className="rounded-lg border border-blue-200 px-3 py-1.5 text-xs font-bold text-primary hover:bg-blue-50" onClick={() => void setDefaultAddress(address.id)} type="button">Set Default</button> : null}
                        <button className="rounded-lg border border-gray-200 px-3 py-1.5 text-xs font-bold text-gray-700 hover:bg-gray-50" onClick={() => openAddressModal(address)} type="button">Edit</button>
                        <button className="rounded-lg border border-red-200 px-3 py-1.5 text-xs font-bold text-red-600 hover:bg-red-50" onClick={() => void deleteAddress(address.id)} type="button">Delete</button>
                      </div>
                    </article>
                  ))}
                </div>
              )}
            </section>
          ) : null}

          {activeTab === 'reviews' ? (
            <section className="space-y-5">
              <h2 className="text-2xl font-bold text-gray-900">Reviews</h2>
              {reviewItems.length === 0 ? (
                <EmptyState icon="rate_review" title="No reviews yet" description="Your verified reviews will appear here after you rate delivered products." />
              ) : (
                <div className="grid gap-4">
                  {reviewItems.map((review) => (
                    <article className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm" key={review.id}>
                      <div className="flex gap-4">
                        <Link className="h-20 w-20 flex-shrink-0 overflow-hidden rounded-xl border border-gray-100 bg-gray-50 p-1" to={`/product/${review.productSlug}`}>
                          {review.productImageUrl ? <img alt={review.productName} className="h-full w-full object-contain" src={review.productImageUrl} /> : <span className="material-symbols-outlined flex h-full items-center justify-center text-gray-300">inventory_2</span>}
                        </Link>
                        <div className="min-w-0 flex-1">
                          <Link className="line-clamp-2 text-sm font-bold text-gray-900 hover:text-primary" to={`/product/${review.productSlug}`}>{review.productName}</Link>
                          <p className="mt-1 text-xs text-gray-500">{review.storeName} · {review.variantName}</p>
                          <div className="mt-2 flex text-amber-400">{Array.from({ length: 5 }, (_, index) => <span key={index}>{index < review.rating ? '★' : '☆'}</span>)}</div>
                          <p className="mt-3 text-sm leading-6 text-gray-700">{review.comment || 'You left a star rating without a written comment.'}</p>
                          {review.sellerReplyContent ? (
                            <div className="mt-4 rounded-xl bg-orange-50 p-4 text-sm text-gray-700">
                              <p className="font-bold text-orange-700">Seller reply</p>
                              <p className="mt-1">{review.sellerReplyContent}</p>
                            </div>
                          ) : null}
                          <div className="mt-4 flex flex-wrap items-center gap-3 text-xs text-gray-500">
                            <span>Delivered {formatDate(review.deliveredAt)}</span>
                            <span>Edit window {formatDate(review.reviewDeadline)}</span>
                            {review.canEdit ? <Link className="rounded-lg bg-primary px-3 py-2 font-bold text-white hover:bg-blue-700" to={`/reviews/write/${review.orderItemId}`}>Edit Review</Link> : null}
                          </div>
                        </div>
                      </div>
                    </article>
                  ))}
                </div>
              )}
            </section>
          ) : null}

          {activeTab === 'password' ? (
            <section className="space-y-6">
              <h2 className="text-2xl font-bold text-gray-900">Change Password</h2>
              <form className="max-w-md space-y-6 rounded-2xl border border-gray-100 bg-white p-8 shadow-sm" onSubmit={handlePasswordSubmit}>
                {[
                  ['currentPassword', 'Current Password'],
                  ['newPassword', 'New Password'],
                  ['confirmPassword', 'Confirm New Password'],
                ].map(([key, label]) => (
                  <label className="block space-y-2 text-sm font-semibold text-gray-700" key={key}>
                    {label}
                    <input className="block w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" required type="password" value={passwordForm[key as keyof typeof passwordForm]} onChange={(event) => setPasswordForm((current) => ({ ...current, [key]: event.target.value }))} />
                  </label>
                ))}
                <button className="w-full rounded-xl bg-primary py-3 font-bold text-white shadow-lg shadow-primary/20 hover:bg-blue-700 disabled:bg-gray-300" disabled={saving} type="submit">
                  {saving ? 'Updating...' : 'Update Password'}
                </button>
              </form>
            </section>
          ) : null}
        </main>
      </div>

      {addressModalOpen ? (
        <div className="fixed inset-0 z-[120] flex items-center justify-center bg-slate-950/50 p-4">
          <form className="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl" onSubmit={saveAddress}>
            <div className="mb-5 flex items-center justify-between">
              <h3 className="text-xl font-bold text-gray-900">{addressForm.id ? 'Edit Address' : 'Add Address'}</h3>
              <button className="rounded-full p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-700" onClick={() => setAddressModalOpen(false)} type="button">
                <span className="material-symbols-outlined">close</span>
              </button>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="space-y-1.5 text-sm font-semibold text-gray-700">
                Full Name
                <input className="w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" required value={addressForm.fullName} onChange={(event) => setAddressForm((current) => ({ ...current, fullName: event.target.value }))} />
              </label>
              <label className="space-y-1.5 text-sm font-semibold text-gray-700">
                Phone Number
                <input className="w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" required value={addressForm.phoneNumber} onChange={(event) => setAddressForm((current) => ({ ...current, phoneNumber: event.target.value }))} />
              </label>
            </div>
            <label className="relative mt-4 block space-y-1.5 text-sm font-semibold text-gray-700">
              Address
              <input className="w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" required value={addressForm.addressLine} onChange={(event) => setAddressForm((current) => ({ ...current, addressLine: event.target.value }))} />
              {addressSuggestions.length > 0 ? (
                <div className="absolute left-0 right-0 top-full z-10 mt-2 overflow-hidden rounded-xl border border-gray-100 bg-white shadow-xl">
                  {addressSuggestions.map((prediction) => (
                    <button className="flex w-full items-start gap-3 border-b border-gray-50 px-4 py-3 text-left text-sm hover:bg-gray-50" key={prediction.place_id} onClick={() => void selectAddressSuggestion(prediction)} type="button">
                      <span className="material-symbols-outlined text-gray-400">location_on</span>
                      <span>
                        <span className="block font-bold text-gray-900">{prediction.structured_formatting?.main_text ?? prediction.description}</span>
                        <span className="block text-xs text-gray-500">{prediction.structured_formatting?.secondary_text}</span>
                      </span>
                    </button>
                  ))}
                </div>
              ) : null}
            </label>
            <div className="mt-4 grid gap-4 sm:grid-cols-3">
              {(['ward', 'district', 'province'] as const).map((field) => (
                <label className="space-y-1.5 text-sm font-semibold capitalize text-gray-700" key={field}>
                  {field}
                  <input className="w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" value={addressForm[field]} onChange={(event) => setAddressForm((current) => ({ ...current, [field]: event.target.value }))} />
                </label>
              ))}
            </div>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <label className="space-y-1.5 text-sm font-semibold text-gray-700">
                Latitude
                <input className="w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" type="number" value={addressForm.latitude} onChange={(event) => setAddressForm((current) => ({ ...current, latitude: Number(event.target.value) }))} />
              </label>
              <label className="space-y-1.5 text-sm font-semibold text-gray-700">
                Longitude
                <input className="w-full rounded-xl border border-gray-200 px-4 py-3 font-normal focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20" type="number" value={addressForm.longitude} onChange={(event) => setAddressForm((current) => ({ ...current, longitude: Number(event.target.value) }))} />
              </label>
            </div>
            <div className="mt-4 flex flex-wrap gap-2">
              {(['Home', 'Office', 'Other'] as AddressType[]).map((type) => (
                <button className={`rounded-xl border px-4 py-2 text-sm font-bold ${addressForm.addressType === type ? 'border-primary bg-blue-50 text-primary' : 'border-gray-200 text-gray-600 hover:bg-gray-50'}`} key={type} onClick={() => setAddressForm((current) => ({ ...current, addressType: type }))} type="button">
                  {type}
                </button>
              ))}
              <label className="ml-auto flex items-center gap-2 text-sm font-semibold text-gray-700">
                <input checked={addressForm.isDefault} onChange={(event) => setAddressForm((current) => ({ ...current, isDefault: event.target.checked }))} type="checkbox" />
                Set as default
              </label>
            </div>
            <div className="mt-6 flex justify-end gap-3">
              <button className="rounded-xl bg-gray-100 px-5 py-3 text-sm font-bold text-gray-700 hover:bg-gray-200" onClick={() => setAddressModalOpen(false)} type="button">Cancel</button>
              <button className="rounded-xl bg-primary px-5 py-3 text-sm font-bold text-white hover:bg-blue-700 disabled:bg-gray-300" disabled={saving} type="submit">{saving ? 'Saving...' : 'Save Address'}</button>
            </div>
          </form>
        </div>
      ) : null}
    </div>
  )
}
