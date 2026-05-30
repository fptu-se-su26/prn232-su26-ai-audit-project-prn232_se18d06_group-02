import type { ReactElement } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'
import LoginPage from '@/pages/LoginPage'
import HomePage from '@/pages/HomePage'
import CustomerShellPage from '@/pages/CustomerShellPage'
import StoreOwnerShellPage from '@/pages/StoreOwnerShellPage'
import AdminShellPage from '@/pages/AdminShellPage'
import StaffShellPage from '@/pages/StaffShellPage'
import CartPage from '@/pages/CartPage'
import CheckoutPage from '@/pages/CheckoutPage'
import PayOSCheckoutPage from '@/pages/PayOSCheckoutPage'
import OrderSuccessPage from '@/pages/OrderSuccessPage'

function RequireAuth({ children, roles }: { children: ReactElement; roles?: string[] }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="flex min-h-screen items-center justify-center bg-slate-950 text-white">Loading...</div>
  if (!user) return <Navigate to="/login" replace />
  if (roles && !roles.includes(user.role ?? '')) return <Navigate to="/" replace />
  return children
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route path="/" element={<HomePage />} />

      <Route path="/customer" element={<RequireAuth roles={['Customer']}><CustomerShellPage /></RequireAuth>} />
      <Route path="/staff" element={<RequireAuth roles={['Staff']}><StaffShellPage /></RequireAuth>} />
      <Route path="/store-owner" element={<RequireAuth roles={['Store Owner']}><StoreOwnerShellPage /></RequireAuth>} />
      <Route path="/admin" element={<RequireAuth roles={['Super Admin', 'Admin']}><AdminShellPage /></RequireAuth>} />

      {/* Cart & Checkout (authenticated) */}
      <Route path="/cart" element={<RequireAuth><CartPage /></RequireAuth>} />
      <Route path="/checkout" element={<RequireAuth><CheckoutPage /></RequireAuth>} />
      <Route path="/checkout/payos" element={<RequireAuth><PayOSCheckoutPage /></RequireAuth>} />
      <Route path="/checkout/success/:orderId" element={<RequireAuth><OrderSuccessPage /></RequireAuth>} />

      <Route
        path="/seller/dashboard"
        element={
          <RequireAuth roles={['Store Owner']}>
            <Navigate to="/store-owner" replace />
          </RequireAuth>
        }
      />
      <Route
        path="/admin/dashboard"
        element={
          <RequireAuth roles={['Super Admin', 'Admin']}>
            <Navigate to="/admin" replace />
          </RequireAuth>
        }
      />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
