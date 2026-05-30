import { useEffect, useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { checkoutApi } from '@/api/checkout'

interface Address { id: string; fullName: string; phone: string; addressLine: string; province: string; district: string; ward: string }
interface CheckoutData {
  items: Array<{ productSlug: string; productName: string; price: number; quantity: number; storeName: string; imageUrl?: string }>
  addresses: Address[]
  totalPrice: number
}

export default function CheckoutPage() {
  const navigate = useNavigate()
  const [data, setData] = useState<CheckoutData | null>(null)
  const [loading, setLoading] = useState(true)
  const [selectedAddress, setSelectedAddress] = useState('')
  const [paymentMethod, setPaymentMethod] = useState<'cod' | 'payos'>('cod')
  const [voucherCode, setVoucherCode] = useState('')
  const [voucherMsg, setVoucherMsg] = useState('')
  const [discount, setDiscount] = useState(0)
  const [placing, setPlacing] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    checkoutApi.getData().then(d => {
      const cd = d as CheckoutData
      setData(cd)
      if (cd.addresses?.length > 0) setSelectedAddress(cd.addresses[0].id)
    }).finally(() => setLoading(false))
  }, [])

  const handleApplyVoucher = async () => {
    if (!voucherCode) return
    try {
      const result = await checkoutApi.applyVoucher(voucherCode) as { discount?: number; message?: string }
      setDiscount(result.discount ?? 0)
      setVoucherMsg(result.message ?? 'Voucher applied!')
    } catch (e: unknown) {
      setVoucherMsg(e instanceof Error ? e.message : 'Invalid voucher.')
      setDiscount(0)
    }
  }

  const handlePlaceOrder = async () => {
    if (!selectedAddress) { setError('Please select a delivery address.'); return }
    setPlacing(true); setError('')
    try {
      const result = await checkoutApi.placeOrder({ addressId: selectedAddress, paymentMethod, voucherCode: voucherCode || undefined }) as { orderId?: string; payosData?: unknown }
      if (paymentMethod === 'payos' && result.payosData) {
        navigate('/checkout/payos', { state: { payosData: result.payosData } })
      } else {
        navigate(`/checkout/success/${result.orderId}`)
      }
    } catch (e: unknown) {
      setError(e instanceof Error ? e.message : 'Failed to place order.')
    } finally { setPlacing(false) }
  }

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  )
  if (!data) return <div className="text-center py-16 text-gray-500">Could not load checkout data.</div>

  const subtotal = data.totalPrice ?? 0
  const total = subtotal - discount

  return (
    <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      <h1 className="text-3xl font-bold tracking-tight text-gray-900 mb-8">Checkout</h1>
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-white rounded-2xl border border-gray-200 p-6">
            <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
              <span className="material-symbols-outlined text-primary">location_on</span>
              Delivery Address
            </h2>
            {data.addresses.length === 0 ? (
              <div className="text-center py-6 text-gray-500">
                <p className="mb-3">No address saved.</p>
                <Link to="/profile" className="text-primary font-semibold hover:underline">Add address in profile →</Link>
              </div>
            ) : (
              <div className="space-y-3">
                {data.addresses.map(a => (
                  <label key={a.id} className={`flex items-start gap-3 p-4 rounded-xl border-2 cursor-pointer transition-colors ${selectedAddress === a.id ? 'border-primary bg-blue-50' : 'border-gray-200 hover:border-gray-300'}`}>
                    <input type="radio" name="address" value={a.id} checked={selectedAddress === a.id}
                      onChange={() => setSelectedAddress(a.id)} className="mt-1 text-primary" />
                    <div>
                      <p className="font-semibold text-gray-900 text-sm">{a.fullName} <span className="text-gray-500 font-normal">· {a.phone}</span></p>
                      <p className="text-sm text-gray-600 mt-0.5">{a.addressLine}, {a.ward}, {a.district}, {a.province}</p>
                    </div>
                  </label>
                ))}
              </div>
            )}
          </div>
          <div className="bg-white rounded-2xl border border-gray-200 p-6">
            <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
              <span className="material-symbols-outlined text-primary">payments</span>
              Payment Method
            </h2>
            <div className="grid grid-cols-2 gap-3">
              {([['cod', 'local_shipping', 'Cash on Delivery', 'Pay when you receive'] as const, ['payos', 'credit_card', 'PayOS Online', 'Secure online payment'] as const]).map(([val, icon, label, sub]) => (
                <label key={val} className={`flex items-center gap-3 p-4 rounded-xl border-2 cursor-pointer transition-colors ${paymentMethod === val ? 'border-primary bg-blue-50' : 'border-gray-200 hover:border-gray-300'}`}>
                  <input type="radio" name="payment" value={val} checked={paymentMethod === val}
                    onChange={() => setPaymentMethod(val as 'cod' | 'payos')} className="text-primary" />
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="material-symbols-outlined text-[20px] text-primary">{icon}</span>
                      <span className="font-semibold text-sm text-gray-900">{label}</span>
                    </div>
                    <p className="text-xs text-gray-500 mt-0.5">{sub}</p>
                  </div>
                </label>
              ))}
            </div>
          </div>
          <div className="bg-white rounded-2xl border border-gray-200 p-6">
            <h2 className="text-lg font-bold text-gray-900 mb-4 flex items-center gap-2">
              <span className="material-symbols-outlined text-primary">confirmation_number</span>
              Voucher Code
            </h2>
            <div className="flex gap-3">
              <input placeholder="Enter voucher code" value={voucherCode} onChange={e => setVoucherCode(e.target.value)}
                onKeyDown={e => e.key === 'Enter' && handleApplyVoucher()}
                className="flex-1 border border-gray-300 rounded-xl px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary uppercase tracking-wider" />
              <button onClick={handleApplyVoucher}
                className="px-5 py-3 bg-primary text-white font-semibold rounded-xl hover:bg-blue-700 transition-colors text-sm">Apply</button>
            </div>
            {voucherMsg && (
              <p className={`mt-2 text-sm font-medium ${discount > 0 ? 'text-green-600' : 'text-red-500'}`}>{voucherMsg}</p>
            )}
          </div>
        </div>
        <div className="lg:col-span-1">
          <div className="bg-white rounded-2xl border border-gray-200 p-6 sticky top-24">
            <h2 className="text-lg font-bold text-gray-900 mb-5">Order Summary</h2>
            <div className="space-y-3 mb-5">
              {data.items.map((item, i) => (
                <div key={i} className="flex items-start gap-3">
                  {item.imageUrl && (
                    <div className="w-10 h-10 rounded-lg bg-gray-50 border border-gray-100 overflow-hidden flex-shrink-0 p-0.5">
                      <img src={item.imageUrl} alt={item.productName} className="w-full h-full object-contain mix-blend-multiply" />
                    </div>
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-medium text-gray-700 line-clamp-1">{item.productName}</p>
                    <p className="text-xs text-gray-400">{item.storeName} × {item.quantity}</p>
                  </div>
                  <span className="text-xs font-semibold text-gray-700 flex-shrink-0">{(item.price * item.quantity).toLocaleString('vi-VN')} ₫</span>
                </div>
              ))}
            </div>
            <div className="border-t border-gray-100 pt-4 space-y-2 text-sm mb-5">
              <div className="flex justify-between text-gray-600">
                <span>Subtotal</span><span>{subtotal.toLocaleString('vi-VN')} ₫</span>
              </div>
              {discount > 0 && (
                <div className="flex justify-between text-green-600 font-medium">
                  <span>Voucher Discount</span><span>−{discount.toLocaleString('vi-VN')} ₫</span>
                </div>
              )}
              <div className="flex justify-between font-bold text-gray-900 text-base pt-2 border-t border-gray-100">
                <span>Total</span>
                <span className="text-primary text-lg">{total.toLocaleString('vi-VN')} ₫</span>
              </div>
            </div>
            {error && (
              <div className="flex items-center gap-2 bg-red-50 text-red-600 px-4 py-3 rounded-xl text-sm mb-4">
                <span className="material-symbols-outlined text-[18px]">error</span>
                {error}
              </div>
            )}
            <button onClick={handlePlaceOrder} disabled={placing}
              className="w-full bg-secondary hover:bg-orange-600 disabled:bg-gray-300 text-white font-bold py-4 rounded-xl transition-all shadow-[0_8px_20px_-6px_rgba(255,107,0,0.4)] disabled:shadow-none flex items-center justify-center gap-2 text-sm">
              <span className="material-symbols-outlined text-[20px]">shopping_bag</span>
              {placing ? 'Placing Order…' : 'Place Order'}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
