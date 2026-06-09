import type { ReactElement } from 'react'
import { Navigate, Route, Routes, useLocation } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'
import SiteLayout from '@/components/layout/SiteLayout'
import LoginPage from '@/pages/LoginPage'
import HomePage from '@/pages/HomePage'
import ProductBrowsePage from '@/pages/ProductBrowsePage'
import ProductDetailPage from '@/pages/ProductDetailPage'
import StoreProfilePage from '@/pages/StoreProfilePage'
import ProfilePage from '@/pages/ProfilePage'
import OrderTrackPage from '@/pages/OrderTrackPage'
import WriteReviewPage from '@/pages/WriteReviewPage'
import CustomerShellPage from '@/pages/CustomerShellPage'
import StaffShellPage from '@/pages/StaffShellPage'
import CartPage from '@/pages/CartPage'
import CheckoutPage from '@/pages/CheckoutPage'
import PayOSCheckoutPage from '@/pages/PayOSCheckoutPage'
import OrderSuccessPage from '@/pages/OrderSuccessPage'

// Admin pages (use AdminLayout internally — must NOT be inside SiteLayout)
import AdminDashboardPage from '@/pages/AdminDashboardPage'
import AdminOrderDetailPage from '@/pages/AdminOrderDetailPage'
import AdminOrdersPage from '@/pages/AdminOrdersPage'
import AdminProductDetailPage from '@/pages/AdminProductDetailPage'
import AdminProductsPage from '@/pages/AdminProductsPage'
import AdminStoreApplicationDetailPage from '@/pages/AdminStoreApplicationDetailPage'
import AdminStoreApplicationsPage from '@/pages/AdminStoreApplicationsPage'
import AdminStoresPage from '@/pages/AdminStoresPage'
import AdminUsersPage from '@/pages/AdminUsersPage'

// Seller pages (use SellerLayout internally — must NOT be inside SiteLayout)
import SellerDashboardPage from '@/pages/seller/SellerDashboardPage'
import SellerProductsPage from '@/pages/seller/SellerProductsPage'
import SellerOrdersPage from '@/pages/seller/SellerOrdersPage'
import SellerSettingsPage from '@/pages/seller/SellerSettingsPage'

function RequireAuth({ children, roles }: { children: ReactElement; roles?: string[] }) {
  const { user, loading } = useAuth()
  const location = useLocation()
  if (loading) return <div className="flex min-h-screen items-center justify-center bg-slate-950 text-white">Loading...</div>
  if (!user) return <Navigate to={`/login?returnUrl=${encodeURIComponent(location.pathname + location.search)}`} replace />
  if (roles && !roles.includes(user.role ?? '')) return <Navigate to="/" replace />
  return children
}

const ADMIN_ROLES = ['Super Admin', 'Admin']
const SELLER_ROLES = ['Store Owner']

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      {/* ── Admin (AdminLayout handles its own layout — no SiteLayout wrapper) ── */}
      <Route
        path="/admin"
        element={<RequireAuth roles={ADMIN_ROLES}><Navigate to="/admin/dashboard" replace /></RequireAuth>}
      />
      <Route
        path="/admin/dashboard"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminDashboardPage /></RequireAuth>}
      />
      <Route
        path="/admin/store-applications"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminStoreApplicationsPage /></RequireAuth>}
      />
      <Route
        path="/admin/store-applications/:id"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminStoreApplicationDetailPage /></RequireAuth>}
      />
      <Route
        path="/admin/stores"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminStoresPage /></RequireAuth>}
      />
      <Route
        path="/admin/stores/:id"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminStoreApplicationDetailPage /></RequireAuth>}
      />
      <Route
        path="/admin/users"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminUsersPage /></RequireAuth>}
      />
      <Route
        path="/admin/orders"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminOrdersPage /></RequireAuth>}
      />
      <Route
        path="/admin/orders/detail"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminOrderDetailPage /></RequireAuth>}
      />
      <Route
        path="/admin/orders/:id"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminOrderDetailPage /></RequireAuth>}
      />
      <Route
        path="/admin/products"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminProductsPage /></RequireAuth>}
      />
      <Route
        path="/admin/products/:id"
        element={<RequireAuth roles={ADMIN_ROLES}><AdminProductDetailPage /></RequireAuth>}
      />

      {/* ── Store Owner / Seller (SellerLayout handles its own layout) ── */}
      <Route
        path="/store-owner"
        element={<RequireAuth roles={SELLER_ROLES}><Navigate to="/store-owner/dashboard" replace /></RequireAuth>}
      />
      <Route
        path="/store-owner/dashboard"
        element={<RequireAuth roles={SELLER_ROLES}><SellerDashboardPage /></RequireAuth>}
      />
      <Route
        path="/store-owner/products"
        element={<RequireAuth roles={SELLER_ROLES}><SellerProductsPage /></RequireAuth>}
      />
      <Route
        path="/store-owner/orders"
        element={<RequireAuth roles={SELLER_ROLES}><SellerOrdersPage /></RequireAuth>}
      />
      <Route
        path="/store-owner/settings"
        element={<RequireAuth roles={SELLER_ROLES}><SellerSettingsPage /></RequireAuth>}
      />

      {/* ── Public + Customer routes (inside SiteLayout) ── */}
      <Route element={<SiteLayout />}>
        {/* Public */}
        <Route path="/" element={<HomePage />} />
        <Route path="/products" element={<ProductBrowsePage />} />
        <Route path="/products/:slug" element={<ProductBrowsePage />} />
        <Route path="/product/:slug" element={<ProductDetailPage />} />
        <Route path="/store/:slug" element={<StoreProfilePage />} />

        {/* Authenticated buyer */}
        <Route path="/cart" element={<RequireAuth><CartPage /></RequireAuth>} />
        <Route path="/checkout" element={<RequireAuth><CheckoutPage /></RequireAuth>} />
        <Route path="/checkout/payos" element={<RequireAuth><PayOSCheckoutPage /></RequireAuth>} />
        <Route path="/checkout/success/:orderId" element={<RequireAuth><OrderSuccessPage /></RequireAuth>} />
        <Route path="/profile" element={<RequireAuth><ProfilePage /></RequireAuth>} />
        <Route path="/orders/track/:subOrderId" element={<RequireAuth><OrderTrackPage /></RequireAuth>} />
        <Route path="/write-review/:orderItemId" element={<RequireAuth><WriteReviewPage /></RequireAuth>} />

        {/* Role shells (customer / staff documentation pages) */}
        <Route path="/customer" element={<RequireAuth roles={['Customer']}><CustomerShellPage /></RequireAuth>} />
        <Route path="/staff" element={<RequireAuth roles={['Staff']}><StaffShellPage /></RequireAuth>} />

        {/* Legacy redirect */}
        <Route
          path="/seller/dashboard"
          element={<RequireAuth roles={SELLER_ROLES}><Navigate to="/store-owner/dashboard" replace /></RequireAuth>}
        />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}
