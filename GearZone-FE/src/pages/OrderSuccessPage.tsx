/* eslint-disable react-hooks/set-state-in-effect */
import { useLocation, Link } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { checkoutApi } from '@/api/checkout'

interface OrderSuccess {
  orderId: string
  totalPrice: number
  items: Array<{ productName: string; quantity: number; price: number; imageUrl?: string }>
  deliveryAddress?: string
}

export default function OrderSuccessPage() {
  const location = useLocation()
  const [order, setOrder] = useState<OrderSuccess | null>(null)
  const [loading, setLoading] = useState(true)

  const pathParts = location.pathname.split('/')
  const orderId = pathParts[pathParts.length - 1]

  useEffect(() => {
    if (!orderId) { setLoading(false); return }
    checkoutApi.getSuccess(orderId)
      .then(d => setOrder(d as OrderSuccess))
      .finally(() => setLoading(false))
  }, [orderId])

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  )

  return (
    <div className="max-w-2xl mx-auto px-4 py-16 text-center">
      <div className="w-24 h-24 rounded-full bg-green-50 flex items-center justify-center mx-auto mb-6">
        <span className="material-symbols-outlined text-5xl text-green-500" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
      </div>
      <h1 className="text-3xl font-extrabold text-gray-900 mb-2">Order Placed!</h1>
      <p className="text-gray-500 mb-8">Thank you for your purchase. We&apos;ll process your order shortly.</p>
      {order && (
        <div className="bg-white rounded-2xl border border-gray-200 text-left overflow-hidden mb-8">
          <div className="px-6 py-4 border-b border-gray-100 bg-gray-50 flex justify-between items-center">
            <span className="text-sm font-semibold text-gray-700">Order #{order.orderId}</span>
            {order.deliveryAddress && <span className="text-xs text-gray-400">{order.deliveryAddress}</span>}
          </div>
          <div className="divide-y divide-gray-50">
            {order.items?.map((item, i) => (
              <div key={i} className="flex items-center justify-between px-6 py-3">
                <div className="flex items-center gap-3">
                  {item.imageUrl && (
                    <div className="w-10 h-10 rounded-lg bg-gray-50 border border-gray-100 overflow-hidden p-0.5 flex-shrink-0">
                      <img src={item.imageUrl} alt={item.productName} className="w-full h-full object-contain mix-blend-multiply" />
                    </div>
                  )}
                  <span className="text-sm text-gray-700">{item.productName} <span className="text-gray-400">× {item.quantity}</span></span>
                </div>
                <span className="text-sm font-semibold text-gray-900">{(item.price * item.quantity).toLocaleString('vi-VN')} ₫</span>
              </div>
            ))}
          </div>
          {order.totalPrice && (
            <div className="flex justify-between items-center px-6 py-4 border-t border-gray-200 bg-gray-50">
              <span className="font-bold text-gray-900">Total</span>
              <span className="font-bold text-primary text-lg">{order.totalPrice.toLocaleString('vi-VN')} ₫</span>
            </div>
          )}
        </div>
      )}
      <div className="flex gap-4 justify-center">
        <Link to="/profile?tab=orders"
          className="flex items-center gap-2 bg-primary text-white font-semibold px-6 py-3 rounded-xl hover:bg-blue-700 transition-colors shadow-[0_8px_20px_-6px_rgba(26,87,219,0.4)]">
          <span className="material-symbols-outlined text-[18px]">shopping_bag</span>
          View My Orders
        </Link>
        <Link to="/products"
          className="flex items-center gap-2 border border-gray-200 bg-white text-gray-700 font-semibold px-6 py-3 rounded-xl hover:bg-gray-50 transition-colors">
          <span className="material-symbols-outlined text-[18px]">storefront</span>
          Continue Shopping
        </Link>
      </div>
    </div>
  )
}
