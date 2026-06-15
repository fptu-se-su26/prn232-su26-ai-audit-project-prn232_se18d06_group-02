import type { ReactElement } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'
import SiteLayout from '@/components/layout/SiteLayout'
import LoginPage from '@/pages/LoginPage'
import HomePage from '@/pages/HomePage'
import ProductBrowsePage from '@/pages/ProductBrowsePage'
import ProductDetailPage from '@/pages/ProductDetailPage'
import CustomerShellPage from '@/pages/CustomerShellPage'
import StoreOwnerShellPage from '@/pages/StoreOwnerShellPage'
import AdminShellPage from '@/pages/AdminShellPage'
import AdminDashboardPage from '@/pages/AdminDashboardPage'
import AdminOrderDetailPage from '@/pages/AdminOrderDetailPage'
import AdminOrdersPage from '@/pages/AdminOrdersPage'
import AdminProductDetailPage from '@/pages/AdminProductDetailPage'
import AdminProductsPage from '@/pages/AdminProductsPage'
import AdminStoreApplicationDetailPage from '@/pages/AdminStoreApplicationDetailPage'
import AdminStoreApplicationsPage from '@/pages/AdminStoreApplicationsPage'
import AdminStoresPage from '@/pages/AdminStoresPage'
import AdminUsersPage from '@/pages/AdminUsersPage'
import StaffShellPage from '@/pages/StaffShellPage'
import CartPage from '@/pages/CartPage'
import CheckoutPage from '@/pages/CheckoutPage'
import PayOSCheckoutPage from '@/pages/PayOSCheckoutPage'
import OrderSuccessPage from '@/pages/OrderSuccessPage'
import ChatPage from '@/pages/ChatPage'
import ProfilePage from '@/pages/ProfilePage'
import OrderTrackPage from '@/pages/OrderTrackPage'
import WriteReviewPage from '@/pages/WriteReviewPage'
import RegisterSellerPage from '@/pages/RegisterSellerPage'

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
      <Route element={<SiteLayout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/products" element={<ProductBrowsePage />} />
        <Route path="/products/:slug" element={<ProductBrowsePage />} />
        <Route path="/product/:slug" element={<ProductDetailPage />} />

        <Route path="/customer" element={<RequireAuth roles={['Customer']}><CustomerShellPage /></RequireAuth>} />
        <Route path="/staff" element={<RequireAuth roles={['Staff']}><StaffShellPage /></RequireAuth>} />
        <Route path="/store-owner" element={<RequireAuth roles={['Store Owner']}><StoreOwnerShellPage /></RequireAuth>} />
        <Route path="/admin" element={<RequireAuth roles={['Super Admin', 'Admin']}><AdminShellPage /></RequireAuth>} />

        <Route path="/cart" element={<RequireAuth><CartPage /></RequireAuth>} />
        <Route path="/checkout" element={<RequireAuth><CheckoutPage /></RequireAuth>} />
        <Route path="/checkout/payos" element={<RequireAuth><PayOSCheckoutPage /></RequireAuth>} />
        <Route path="/checkout/success/:orderId" element={<RequireAuth><OrderSuccessPage /></RequireAuth>} />

        <Route path="/messages" element={<RequireAuth><ChatPage /></RequireAuth>} />
        <Route path="/profile" element={<RequireAuth><ProfilePage /></RequireAuth>} />
        <Route path="/orders/track/:subOrderId" element={<RequireAuth><OrderTrackPage /></RequireAuth>} />
        <Route path="/reviews/write/:orderItemId" element={<RequireAuth><WriteReviewPage /></RequireAuth>} />
        <Route path="/seller/register" element={<RequireAuth><RegisterSellerPage /></RequireAuth>} />

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
              <AdminDashboardPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/store-applications"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminStoreApplicationsPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/store-applications/:id"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminStoreApplicationDetailPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/stores"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminStoresPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/stores/:id"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminStoreApplicationDetailPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/users"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminUsersPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/orders"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminOrdersPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/orders/detail"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminOrderDetailPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/orders/:id"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminOrderDetailPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/products"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminProductsPage />
            </RequireAuth>
          }
        />
        <Route
          path="/admin/products/:id"
          element={
            <RequireAuth roles={['Super Admin', 'Admin']}>
              <AdminProductDetailPage />
            </RequireAuth>
          }
        />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}
