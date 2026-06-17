/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useRef, useState } from 'react'
import { Link } from 'react-router-dom'
import { cartApi } from '@/api/cart'

interface CartItem {
  id: string
  variantId: string
  productSlug: string
  productName: string
  imageUrl?: string
  variantName?: string
  price: number
  quantity: number
  stockQuantity: number
}

interface StoreCartGroup {
  storeId: string
  storeName: string
  items: CartItem[]
  storeSubtotal: number
}

interface Cart {
  id: string
  userId: string
  storeGroups: StoreCartGroup[]
  totalPrice: number
}

function formatCurrency(amount: number) {
  return `${amount.toLocaleString('vi-VN')}₫`
}

export default function CartPage() {
  const [cart, setCart] = useState<Cart | null>(null)
  const [loading, setLoading] = useState(true)
  const [updatingItemId, setUpdatingItemId] = useState<string | null>(null)
  const [selectedItemIds, setSelectedItemIds] = useState<string[]>([])
  const [errorMessage, setErrorMessage] = useState('')
  const [pendingRemoveItem, setPendingRemoveItem] = useState<CartItem | null>(null)
  const initializedSelectionRef = useRef(false)

  function updateCartCount(nextCart: Cart | null) {
    const totalQuantity = nextCart?.storeGroups.flatMap((group) => group.items).reduce((sum, item) => sum + item.quantity, 0) ?? 0
    window.dispatchEvent(new CustomEvent('gearzone:cart-count-updated', { detail: { count: totalQuantity } }))
  }

  function syncSelection(nextCart: Cart) {
    const allItemIds = nextCart.storeGroups.flatMap((group) => group.items.map((item) => item.id))

    setSelectedItemIds((current) => {
      if (!initializedSelectionRef.current) {
        initializedSelectionRef.current = true
        return allItemIds
      }

      return current.filter((id) => allItemIds.includes(id))
    })
  }

  function recalculateCart(nextCart: Cart): Cart {
    return {
      ...nextCart,
      storeGroups: nextCart.storeGroups.map((group) => ({
        ...group,
        storeSubtotal: group.items.reduce((sum, item) => sum + item.price * item.quantity, 0),
      })),
      totalPrice: nextCart.storeGroups.reduce(
        (cartSum, group) => cartSum + group.items.reduce((sum, item) => sum + item.price * item.quantity, 0),
        0,
      ),
    }
  }

  function patchCartItemQuantity(currentCart: Cart, itemId: string, quantity: number) {
    return recalculateCart({
      ...currentCart,
      storeGroups: currentCart.storeGroups.map((group) => ({
        ...group,
        items: group.items.map((item) => (item.id === itemId ? { ...item, quantity } : item)),
      })),
    })
  }

  function removeCartItem(currentCart: Cart, itemId: string) {
    return recalculateCart({
      ...currentCart,
      storeGroups: currentCart.storeGroups
        .map((group) => ({
          ...group,
          items: group.items.filter((item) => item.id !== itemId),
        }))
        .filter((group) => group.items.length > 0),
    })
  }

  async function fetchCart(showLoading = true) {
    if (showLoading) setLoading(true)
    setErrorMessage('')

    try {
      const data = recalculateCart(await cartApi.get() as Cart)

      setCart(data)
      syncSelection(data)
      updateCartCount(data)
    } catch (error) {
      setErrorMessage(error instanceof Error ? error.message : 'Failed to load your cart.')
    } finally {
      if (showLoading) setLoading(false)
    }
  }

  useEffect(() => {
    void fetchCart()
  }, [])

  async function handleQtyChange(item: CartItem, nextQuantity: number) {
    const safeQuantity = Math.max(1, Math.min(nextQuantity, item.stockQuantity))
    if (safeQuantity === item.quantity) return

    if (!cart) return

    const previousCart = cart
    const optimisticCart = patchCartItemQuantity(previousCart, item.id, safeQuantity)

    setUpdatingItemId(item.id)
    setErrorMessage('')
    setCart(optimisticCart)
    updateCartCount(optimisticCart)

    try {
      await cartApi.updateQuantity(item.id, safeQuantity)
    } catch (error) {
      setCart(previousCart)
      updateCartCount(previousCart)
      setErrorMessage(error instanceof Error ? error.message : 'Failed to update quantity.')
    } finally {
      setUpdatingItemId(null)
    }
  }

  async function handleRemove(item: CartItem) {
    if (!cart) return

    const previousCart = cart
    const optimisticCart = removeCartItem(previousCart, item.id)

    setUpdatingItemId(item.id)
    setErrorMessage('')
    setCart(optimisticCart)
    setSelectedItemIds((current) => current.filter((id) => id !== item.id))
    updateCartCount(optimisticCart)

    try {
      await cartApi.remove(item.id)
    } catch (error) {
      setCart(previousCart)
      syncSelection(previousCart)
      updateCartCount(previousCart)
      setErrorMessage(error instanceof Error ? error.message : 'Failed to remove item.')
    } finally {
      setUpdatingItemId(null)
      setPendingRemoveItem(null)
    }
  }

  function handleToggleStore(group: StoreCartGroup, checked: boolean) {
    const storeItemIds = group.items.map((item) => item.id)

    setSelectedItemIds((current) => {
      if (checked) {
        return Array.from(new Set([...current, ...storeItemIds]))
      }

      return current.filter((id) => !storeItemIds.includes(id))
    })
  }

  function handleToggleItem(itemId: string, checked: boolean) {
    setSelectedItemIds((current) => {
      if (checked) {
        return current.includes(itemId) ? current : [...current, itemId]
      }

      return current.filter((id) => id !== itemId)
    })
  }

  function handleCheckout() {
    if (!selectedItemIds.length) return

    const searchParams = new URLSearchParams()
    selectedItemIds.forEach((id) => searchParams.append('SelectedCartItemIds', id))
    window.location.href = `/Checkout?${searchParams.toString()}`
  }

  const allItems = cart?.storeGroups.flatMap((group) => group.items) ?? []
  const itemCount = allItems.length
  const selectedItems = allItems.filter((item) => selectedItemIds.includes(item.id))
  const selectedQuantity = selectedItems.reduce((sum, item) => sum + item.quantity, 0)
  const selectedSubtotal = selectedItems.reduce((sum, item) => sum + item.price * item.quantity, 0)

  if (loading) {
    return (
      <div className="flex items-center justify-center py-24">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
      </div>
    )
  }

  if (errorMessage && !cart) {
    return <div className="py-16 text-center text-sm font-medium text-red-500">{errorMessage}</div>
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8 lg:py-12">
      <div className="mb-8 flex items-center justify-between">
        <h1 className="flex items-center gap-3 text-3xl font-bold text-slate-900">
          Your Shopping Cart
          <span className="rounded-full bg-slate-100 px-3 py-1 text-lg font-medium text-slate-500">({itemCount} items)</span>
        </h1>
      </div>

      {errorMessage ? (
        <div className="mb-6 flex items-center gap-2 rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-600">
          <span className="material-symbols-outlined text-[18px]">error</span>
          {errorMessage}
        </div>
      ) : null}

      <div className="grid grid-cols-1 items-start gap-8 lg:grid-cols-12">
        <div className="space-y-6 lg:col-span-8">
          {!cart?.storeGroups?.length ? (
            <div className="rounded-xl border border-slate-200 bg-white p-8 text-center text-slate-500 shadow-sm">
              Your cart is empty.
              <div className="mt-4">
                <Link className="inline-flex items-center gap-2 font-medium text-primary transition-colors hover:text-blue-700" to="/products">
                  <span className="material-symbols-outlined text-[18px]">shopping_bag</span>
                  Continue Shopping
                </Link>
              </div>
            </div>
          ) : (
            <>
              {cart.storeGroups.map((group) => {
                const storeItemIds = group.items.map((item) => item.id)
                const checkedCount = storeItemIds.filter((id) => selectedItemIds.includes(id)).length
                const isStoreChecked = checkedCount > 0 && checkedCount === storeItemIds.length
                const isStorePartial = checkedCount > 0 && checkedCount < storeItemIds.length
                const storeSubtotal = group.items
                  .filter((item) => selectedItemIds.includes(item.id))
                  .reduce((sum, item) => sum + item.price * item.quantity, 0)

                return (
                  <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm" key={group.storeId}>
                    <div className="flex items-center justify-between border-b border-slate-100 bg-white px-6 py-4">
                      <div className="flex items-center gap-4">
                        <input
                          checked={isStoreChecked}
                          className="size-5 cursor-pointer rounded border-slate-300 text-primary focus:ring-primary/20"
                          onChange={(event) => handleToggleStore(group, event.target.checked)}
                          ref={(element) => {
                            if (element) element.indeterminate = isStorePartial
                          }}
                          type="checkbox"
                        />
                        <span className="font-bold text-slate-900 transition-colors hover:text-primary">{group.storeName}</span>
                      </div>
                    </div>

                    {group.items.map((item) => {
                      const isUpdating = updatingItemId === item.id

                      return (
                        <div
                          className={`flex gap-6 border-b border-slate-100 p-6 transition-colors last:border-0 hover:bg-slate-50 ${isUpdating ? 'opacity-60' : ''}`}
                          key={item.id}
                        >
                          <div className="flex items-start pt-2">
                            <input
                              checked={selectedItemIds.includes(item.id)}
                              className="size-5 cursor-pointer rounded border-slate-300 text-primary focus:ring-primary/20"
                              onChange={(event) => handleToggleItem(item.id, event.target.checked)}
                              type="checkbox"
                            />
                          </div>

                          <Link className="h-24 w-24 shrink-0 overflow-hidden rounded-lg border border-slate-200 bg-slate-100" to={`/product/${item.productSlug}`}>
                            {item.imageUrl ? (
                              <img alt={item.productName} className="h-full w-full object-cover" src={item.imageUrl} />
                            ) : (
                              <div className="flex h-full w-full items-center justify-center text-slate-400">
                                <span className="material-symbols-outlined text-4xl">inventory_2</span>
                              </div>
                            )}
                          </Link>

                          <div className="flex min-w-0 flex-1 flex-col justify-between">
                            <div className="flex items-start justify-between gap-4">
                              <div>
                                <Link className="line-clamp-2 text-base font-bold leading-snug text-slate-900 transition-colors hover:text-primary" to={`/product/${item.productSlug}`}>
                                  {item.productName}
                                </Link>
                                {item.variantName ? <div className="mt-1 text-sm text-slate-500">Variant: {item.variantName}</div> : null}
                              </div>

                              <div className="shrink-0 text-right">
                                <div className="text-lg font-bold text-slate-900">{formatCurrency(item.price * item.quantity)}</div>
                                <div className="mt-1 text-xs text-slate-400">{formatCurrency(item.price)} each</div>
                              </div>
                            </div>

                            <div className="mt-4 flex items-end justify-between">
                              <div className="flex h-9 items-center rounded-lg border border-slate-200 bg-white">
                                <button
                                  className="flex h-full w-8 items-center justify-center rounded-l-lg text-slate-500 transition-colors hover:bg-slate-50 hover:text-primary disabled:cursor-not-allowed disabled:opacity-40"
                                  disabled={item.quantity <= 1 || isUpdating}
                                  onClick={() => void handleQtyChange(item, item.quantity - 1)}
                                  type="button"
                                >
                                  <span className="material-symbols-outlined text-[18px]">remove</span>
                                </button>
                                <input
                                  className="h-full w-14 border-none bg-transparent p-0 text-center text-sm font-medium text-slate-900 focus:ring-0"
                                  readOnly
                                  type="number"
                                  value={item.quantity}
                                />
                                <button
                                  className="flex h-full w-8 items-center justify-center rounded-r-lg text-slate-500 transition-colors hover:bg-slate-50 hover:text-primary disabled:cursor-not-allowed disabled:opacity-40"
                                  disabled={item.quantity >= item.stockQuantity || isUpdating}
                                  onClick={() => void handleQtyChange(item, item.quantity + 1)}
                                  type="button"
                                >
                                  <span className="material-symbols-outlined text-[18px]">add</span>
                                </button>
                              </div>

                              <button
                                className="flex items-center gap-1 text-sm font-medium text-slate-400 transition-colors hover:text-red-500 disabled:cursor-not-allowed disabled:opacity-40"
                                disabled={isUpdating}
                                onClick={() => setPendingRemoveItem(item)}
                                type="button"
                              >
                                <span className="material-symbols-outlined text-[18px]">delete</span>
                                <span className="hidden sm:inline">Delete</span>
                              </button>
                            </div>
                          </div>
                        </div>
                      )
                    })}

                    <div className="flex items-center justify-end border-t border-slate-100 bg-slate-50 px-6 py-3">
                      <div className="text-sm font-medium text-slate-700">
                        Store Subtotal: <span className="ml-1 font-bold text-primary">{formatCurrency(storeSubtotal)}</span>
                      </div>
                    </div>
                  </div>
                )
              })}

              <div className="flex justify-start">
                <Link className="inline-flex items-center gap-2 font-medium text-primary transition-colors hover:text-blue-700" to="/products">
                  <span className="material-symbols-outlined text-[18px]">arrow_back</span>
                  Continue Shopping
                </Link>
              </div>
            </>
          )}
        </div>

        <div className="relative h-full lg:col-span-4">
          <div className="sticky top-24 space-y-4">
            <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-lg">
              <h2 className="mb-6 text-xl font-bold text-slate-900">Order Summary</h2>

              <div className="space-y-4 text-sm text-slate-600">
                <div className="flex items-center justify-between">
                  <span>
                    Selected (<span>{selectedQuantity}</span> items)
                  </span>
                  <span className="font-medium text-slate-900">{formatCurrency(selectedSubtotal)}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span>Shipping Fee</span>
                  <span className="text-slate-500">Calculated at checkout</span>
                </div>
              </div>

              <div className="my-6 border-t border-dashed border-slate-200" />

              <div className="mb-6 flex items-center justify-between">
                <span className="text-base font-semibold text-slate-900">Total</span>
                <div className="text-right">
                  <span className="block text-2xl font-bold text-secondary">{formatCurrency(selectedSubtotal)}</span>
                  <span className="text-xs text-slate-400">(VAT Included)</span>
                </div>
              </div>

              <button
                className="flex w-full items-center justify-center gap-2 rounded-lg bg-secondary py-4 text-lg font-bold uppercase tracking-wider text-white shadow-lg shadow-orange-500/20 transition-all hover:scale-[1.01] hover:bg-orange-600 active:scale-[0.99] disabled:cursor-not-allowed disabled:opacity-50 disabled:hover:scale-100"
                disabled={!selectedItemIds.length}
                onClick={handleCheckout}
                type="button"
              >
                Checkout Now
                <span className="material-symbols-outlined text-[20px]">arrow_forward</span>
              </button>

              <div className="mt-6 border-t border-slate-100 pt-4 text-center">
                <p className="flex items-center justify-center gap-1 text-xs text-slate-400">
                  <span className="material-symbols-outlined text-[16px] text-green-600">lock</span>
                  Secure Encrypted Payment
                </p>
              </div>
            </div>

            <div className="cursor-not-allowed rounded-xl border border-slate-200 bg-white p-4 opacity-50 shadow-sm" title="This feature is under development">
              <label className="mb-2 block text-sm font-medium text-slate-700">Have a coupon code?</label>
              <div className="relative flex gap-2">
                <input
                  className="flex-1 rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-sm"
                  disabled
                  placeholder="Enter code"
                  type="text"
                />
                <button
                  className="rounded-lg bg-slate-100 px-4 py-2 text-sm font-medium text-slate-500"
                  disabled
                  type="button"
                >
                  Apply
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {pendingRemoveItem ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <button
            aria-label="Close remove dialog"
            className="absolute inset-0 bg-slate-950/45 backdrop-blur-[2px]"
            onClick={() => setPendingRemoveItem(null)}
            type="button"
          />
          <div className="relative z-10 w-full max-w-md rounded-2xl border border-slate-200 bg-white p-6 shadow-[0_24px_80px_rgba(15,23,42,0.22)]">
            <div className="mb-4 flex h-12 w-12 items-center justify-center rounded-full bg-red-50 text-red-500">
              <span className="material-symbols-outlined text-[24px]">delete</span>
            </div>
            <h3 className="text-xl font-bold text-slate-900">Remove Item</h3>
            <p className="mt-2 text-sm leading-6 text-slate-500">
              Remove <span className="font-semibold text-slate-700">"{pendingRemoveItem.productName}"</span> from your cart?
            </p>
            <div className="mt-6 flex justify-end gap-3">
              <button
                className="rounded-xl border border-slate-200 px-4 py-2.5 text-sm font-semibold text-slate-600 transition hover:bg-slate-50"
                onClick={() => setPendingRemoveItem(null)}
                type="button"
              >
                Cancel
              </button>
              <button
                className="rounded-xl bg-red-500 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-red-600 disabled:cursor-not-allowed disabled:opacity-60"
                disabled={updatingItemId === pendingRemoveItem.id}
                onClick={() => void handleRemove(pendingRemoveItem)}
                type="button"
              >
                {updatingItemId === pendingRemoveItem.id ? 'Removing...' : 'Remove'}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}
