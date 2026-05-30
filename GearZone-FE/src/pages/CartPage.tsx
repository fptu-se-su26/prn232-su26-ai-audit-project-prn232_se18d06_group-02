import { useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { cartApi } from '@/api/cart'

interface CartItem {
  productSlug: string
  productName: string
  imageUrl?: string
  price: number
  quantity: number
  storeName: string
}

interface Cart { items: CartItem[]; totalPrice?: number }

export default function CartPage() {
  const navigate = useNavigate()
  const [cart, setCart] = useState<Cart | null>(null)
  const [loading, setLoading] = useState(true)
  const [updating, setUpdating] = useState<string | null>(null)

  const fetchCart = () => {
    setLoading(true)
    cartApi.get().then(d => setCart(d as Cart)).finally(() => setLoading(false))
  }

  useEffect(() => { fetchCart() }, [])

  const handleQtyChange = async (slug: string, qty: number) => {
    if (qty < 1) return
    setUpdating(slug)
    await cartApi.updateQuantity(slug, qty)
    fetchCart()
    setUpdating(null)
  }

  const handleRemove = async (slug: string) => {
    setUpdating(slug)
    await cartApi.remove(slug)
    fetchCart()
    setUpdating(null)
  }

  const total = cart?.items.reduce((s, i) => s + i.price * i.quantity, 0) ?? 0

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  )

  return (
    <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
      <h1 className="text-3xl font-bold tracking-tight text-gray-900 mb-8">Shopping Cart</h1>

      {!cart?.items?.length ? (
        <div className="bg-white rounded-2xl border border-gray-200 p-16 text-center">
          <span className="material-symbols-outlined text-6xl text-gray-200 mb-4 block">shopping_cart</span>
          <p className="text-gray-500 font-medium mb-6">Your cart is empty.</p>
          <Link to="/products" className="inline-flex items-center gap-2 bg-primary text-white font-semibold px-6 py-3 rounded-xl hover:bg-blue-700 transition-colors">
            <span className="material-symbols-outlined text-[18px]">arrow_forward</span>
            Browse Products
          </Link>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
          {/* Cart items */}
          <div className="lg:col-span-2 space-y-4">
            {cart.items.map(item => (
              <div key={item.productSlug}
                className={`bg-white rounded-2xl border border-gray-200 p-5 flex gap-4 items-start transition-opacity ${updating === item.productSlug ? 'opacity-50' : ''}`}>
                {/* Image */}
                <Link to={`/products/${item.productSlug}`} className="flex-shrink-0 w-20 h-20 bg-gray-50 rounded-xl overflow-hidden border border-gray-100 p-1">
                  {item.imageUrl
                    ? <img src={item.imageUrl} alt={item.productName} className="w-full h-full object-contain mix-blend-multiply" />
                    : <span className="material-symbols-outlined text-4xl text-gray-200 flex items-center justify-center h-full">image</span>
                  }
                </Link>

                {/* Info */}
                <div className="flex-1 min-w-0">
                  <Link to={`/products/${item.productSlug}`} className="font-semibold text-gray-900 hover:text-primary transition-colors line-clamp-2 text-sm">
                    {item.productName}
                  </Link>
                  <p className="text-xs text-gray-500 mt-1">{item.storeName}</p>
                  <p className="text-primary font-bold mt-1">{item.price.toLocaleString('vi-VN')} ₫</p>
                </div>

                {/* Qty + subtotal */}
                <div className="flex flex-col items-end gap-3 flex-shrink-0">
                  <p className="font-bold text-gray-900">{(item.price * item.quantity).toLocaleString('vi-VN')} ₫</p>
                  <div className="flex items-center border border-gray-200 rounded-lg overflow-hidden">
                    <button onClick={() => handleQtyChange(item.productSlug, item.quantity - 1)} disabled={item.quantity <= 1 || updating === item.productSlug}
                      className="px-3 py-1.5 hover:bg-gray-50 text-gray-600 text-sm font-bold disabled:opacity-30 transition-colors">−</button>
                    <span className="px-3 py-1.5 text-sm font-semibold min-w-[32px] text-center">{item.quantity}</span>
                    <button onClick={() => handleQtyChange(item.productSlug, item.quantity + 1)} disabled={updating === item.productSlug}
                      className="px-3 py-1.5 hover:bg-gray-50 text-gray-600 text-sm font-bold disabled:opacity-30 transition-colors">+</button>
                  </div>
                  <button onClick={() => handleRemove(item.productSlug)} disabled={updating === item.productSlug}
                    className="flex items-center gap-1 text-xs text-red-500 hover:text-red-700 font-medium transition-colors">
                    <span className="material-symbols-outlined text-[16px]">delete</span>
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>

          {/* Order summary */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-2xl border border-gray-200 p-6 sticky top-24">
              <h2 className="text-lg font-bold text-gray-900 mb-5">Order Summary</h2>
              <div className="space-y-3 mb-5 text-sm">
                <div className="flex justify-between text-gray-600">
                  <span>Subtotal ({cart.items.length} items)</span>
                  <span>{total.toLocaleString('vi-VN')} ₫</span>
                </div>
                <div className="flex justify-between text-gray-600">
                  <span>Shipping</span>
                  <span className="text-green-600 font-medium">Calculated at checkout</span>
                </div>
              </div>
              <div className="border-t border-gray-100 pt-4 mb-6">
                <div className="flex justify-between font-bold text-gray-900 text-base">
                  <span>Total</span>
                  <span className="text-primary text-xl">{total.toLocaleString('vi-VN')} ₫</span>
                </div>
              </div>
              <button onClick={() => navigate('/checkout')}
                className="w-full bg-secondary hover:bg-orange-600 text-white font-bold py-3.5 rounded-xl transition-all shadow-[0_8px_20px_-6px_rgba(255,107,0,0.4)] flex items-center justify-center gap-2">
                <span className="material-symbols-outlined text-[20px]">shopping_bag</span>
                Proceed to Checkout
              </button>
              <Link to="/products" className="block text-center mt-4 text-sm text-gray-500 hover:text-primary transition-colors">
                ← Continue Shopping
              </Link>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
