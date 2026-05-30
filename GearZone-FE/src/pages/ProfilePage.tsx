import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { usersApi } from '@/api/users'
import { useAuth } from '@/contexts/useAuth'

interface Order {
  id: string
  createdAt: string
  status: string
  totalPrice: number
  subOrders?: Array<{ id: string; status: string }>
}
interface Address {
  id: string
  fullName: string
  phone: string
  addressLine: string
  province: string
  district: string
  ward: string
  isDefault: boolean
}

const STATUS_STYLES: Record<string, string> = {
  Pending:    'bg-orange-50 text-orange-600',
  Processing: 'bg-blue-50 text-blue-600',
  Shipping:   'bg-purple-50 text-purple-600',
  Delivered:  'bg-green-50 text-green-600',
  Completed:  'bg-green-50 text-green-600',
  Cancelled:  'bg-red-50 text-red-500',
}

export default function ProfilePage() {
  const { user } = useAuth()
  const [searchParams] = useSearchParams()
  const [orders, setOrders] = useState<Order[]>([])
  const [addresses, setAddresses] = useState<Address[]>([])
  const [tab, setTab] = useState<'orders' | 'addresses'>(
    (searchParams.get('tab') as 'orders' | 'addresses') ?? 'orders'
  )
  const [loading, setLoading] = useState(true)
  const [showAddAddress, setShowAddAddress] = useState(false)
  const [addrForm, setAddrForm] = useState({ fullName: '', phone: '', addressLine: '', province: '', district: '', ward: '' })
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    Promise.all([usersApi.getOrders(), usersApi.getAddresses()]).then(([o, a]) => {
      setOrders((o as { items?: Order[] }).items ?? (o as Order[]) ?? [])
      setAddresses(a as Address[] ?? [])
    }).finally(() => setLoading(false))
  }, [])

  const handleAddAddress = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      await usersApi.addAddress(addrForm)
      setAddresses(await usersApi.getAddresses() as Address[])
      setShowAddAddress(false)
      setAddrForm({ fullName: '', phone: '', addressLine: '', province: '', district: '', ward: '' })
    } finally { setSaving(false) }
  }

  const handleDeleteAddress = async (id: string) => {
    await usersApi.deleteAddress(id)
    setAddresses(a => a.filter(x => x.id !== id))
  }

  const inputCls = "w-full border border-gray-300 rounded-xl px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  )

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      <div className="bg-white rounded-2xl border border-gray-200 p-6 mb-8 flex items-center gap-5">
        <div className="w-16 h-16 rounded-2xl bg-blue-50 flex items-center justify-center text-primary text-3xl font-bold flex-shrink-0">
          {(user?.fullName || '?')[0].toUpperCase()}
        </div>
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{user?.fullName}</h1>
          <p className="text-gray-500 text-sm mt-0.5">{user?.email}</p>
        </div>
      </div>
      <div className="flex gap-1 border-b border-gray-200 mb-6">
        {([['orders', 'shopping_bag', 'My Orders'], ['addresses', 'location_on', 'Addresses']] as const).map(([key, icon, label]) => (
          <button key={key} onClick={() => setTab(key)}
            className={`flex items-center gap-2 px-5 py-3 text-sm font-semibold border-b-2 -mb-px transition-colors ${tab === key ? 'border-primary text-primary' : 'border-transparent text-gray-500 hover:text-gray-900'}`}>
            <span className="material-symbols-outlined text-[18px]">{icon}</span>
            {label}
          </button>
        ))}
      </div>
      {tab === 'orders' && (
        <div className="space-y-4">
          {orders.length === 0 ? (
            <div className="bg-white rounded-2xl border border-gray-200 p-16 text-center">
              <span className="material-symbols-outlined text-6xl text-gray-200 mb-4 block">shopping_bag</span>
              <p className="text-gray-500 font-medium mb-4">No orders yet.</p>
              <Link to="/products" className="text-primary font-semibold hover:underline">Start shopping →</Link>
            </div>
          ) : orders.map(order => (
            <div key={order.id} className="bg-white rounded-2xl border border-gray-200 p-5">
              <div className="flex items-center justify-between mb-3">
                <div>
                  <p className="font-bold text-gray-900 text-sm">Order #{order.id.slice(0, 8).toUpperCase()}</p>
                  <p className="text-xs text-gray-400 mt-0.5">{new Date(order.createdAt).toLocaleDateString('vi-VN', { year: 'numeric', month: 'long', day: 'numeric' })}</p>
                </div>
                <div className="text-right">
                  <span className={`inline-flex items-center px-3 py-1 rounded-full text-xs font-bold ${STATUS_STYLES[order.status] ?? 'bg-gray-100 text-gray-600'}`}>
                    {order.status}
                  </span>
                  <p className="font-bold text-primary mt-1 text-sm">{order.totalPrice?.toLocaleString('vi-VN')} ₫</p>
                </div>
              </div>
              {order.subOrders && order.subOrders.length > 0 && (
                <div className="flex flex-wrap gap-2 pt-3 border-t border-gray-100">
                  {order.subOrders.map(sub => (
                    <Link key={sub.id} to={`/orders/track/${sub.id}`}
                      className="inline-flex items-center gap-1 text-xs font-semibold bg-gray-50 hover:bg-blue-50 text-gray-600 hover:text-primary border border-gray-200 hover:border-blue-200 px-3 py-1.5 rounded-lg transition-colors">
                      <span className="material-symbols-outlined text-[14px]">local_shipping</span>
                      Track #{sub.id.slice(0, 8).toUpperCase()}
                    </Link>
                  ))}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
      {tab === 'addresses' && (
        <div className="space-y-4">
          {addresses.map(a => (
            <div key={a.id} className="bg-white rounded-2xl border border-gray-200 p-5 flex items-start justify-between gap-4">
              <div className="flex items-start gap-4">
                <div className="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center flex-shrink-0">
                  <span className="material-symbols-outlined text-primary">location_on</span>
                </div>
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className="font-semibold text-gray-900 text-sm">{a.fullName}</span>
                    {a.isDefault && <span className="text-[10px] font-bold bg-blue-100 text-primary px-2 py-0.5 rounded-full">Default</span>}
                  </div>
                  <p className="text-sm text-gray-500">{a.phone}</p>
                  <p className="text-sm text-gray-600 mt-0.5">{a.addressLine}, {a.ward}, {a.district}, {a.province}</p>
                </div>
              </div>
              <button onClick={() => handleDeleteAddress(a.id)}
                className="flex items-center gap-1 text-xs text-red-500 hover:text-red-700 font-medium border border-red-200 hover:border-red-400 px-3 py-1.5 rounded-lg transition-colors flex-shrink-0">
                <span className="material-symbols-outlined text-[14px]">delete</span>
                Delete
              </button>
            </div>
          ))}
          <button onClick={() => setShowAddAddress(s => !s)}
            className="flex items-center gap-2 bg-primary hover:bg-blue-700 text-white font-semibold px-5 py-3 rounded-xl transition-colors text-sm">
            <span className="material-symbols-outlined text-[18px]">add</span>
            Add New Address
          </button>
          {showAddAddress && (
            <form onSubmit={handleAddAddress} className="bg-white rounded-2xl border border-gray-200 p-6 space-y-4">
              <h3 className="font-bold text-gray-900 mb-2">New Address</h3>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold text-gray-500 mb-1.5">Full Name</label>
                  <input placeholder="Full Name" value={addrForm.fullName} onChange={e => setAddrForm(f => ({ ...f, fullName: e.target.value }))} required className={inputCls} />
                </div>
                <div>
                  <label className="block text-xs font-semibold text-gray-500 mb-1.5">Phone</label>
                  <input placeholder="Phone Number" value={addrForm.phone} onChange={e => setAddrForm(f => ({ ...f, phone: e.target.value }))} required className={inputCls} />
                </div>
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1.5">Address</label>
                <input placeholder="Street Address" value={addrForm.addressLine} onChange={e => setAddrForm(f => ({ ...f, addressLine: e.target.value }))} required className={inputCls} />
              </div>
              <div className="grid grid-cols-3 gap-4">
                {(['ward', 'district', 'province'] as const).map(field => (
                  <div key={field}>
                    <label className="block text-xs font-semibold text-gray-500 mb-1.5 capitalize">{field}</label>
                    <input placeholder={field.charAt(0).toUpperCase() + field.slice(1)} value={addrForm[field]}
                      onChange={e => setAddrForm(f => ({ ...f, [field]: e.target.value }))} required className={inputCls} />
                  </div>
                ))}
              </div>
              <div className="flex gap-3 pt-2">
                <button type="submit" disabled={saving} className="bg-primary hover:bg-blue-700 text-white font-semibold px-6 py-3 rounded-xl transition-colors text-sm">
                  {saving ? 'Saving…' : 'Save Address'}
                </button>
                <button type="button" onClick={() => setShowAddAddress(false)} className="bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold px-6 py-3 rounded-xl transition-colors text-sm">
                  Cancel
                </button>
              </div>
            </form>
          )}
        </div>
      )}
    </div>
  )
}
