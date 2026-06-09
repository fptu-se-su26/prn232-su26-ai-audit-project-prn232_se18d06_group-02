import { Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { useAuth } from './contexts/AuthContext';
import Layout from './components/layout/Layout';

// Public pages
import HomePage from './pages/HomePage';
import ProductsPage from './pages/ProductsPage';
import ProductDetailPage from './pages/ProductDetailPage';
import StoreProfilePage from './pages/StoreProfilePage';
import LoginPage from './pages/LoginPage';

// Buyer pages
import CartPage from './pages/CartPage';
import CheckoutPage from './pages/CheckoutPage';
import PayOSCheckoutPage from './pages/PayOSCheckoutPage';
import OrderSuccessPage from './pages/OrderSuccessPage';
import OrderTrackPage from './pages/OrderTrackPage';
import ProfilePage from './pages/ProfilePage';
import WriteReviewPage from './pages/WriteReviewPage';
import RegisterSellerPage from './pages/RegisterSellerPage';

// Seller pages
import SellerDashboardPage from './pages/seller/DashboardPage';
import SellerProductsPage from './pages/seller/ProductsPage';
import SellerOrdersPage from './pages/seller/OrdersPage';
import SellerVouchersPage from './pages/seller/VouchersPage';
import SellerRevenuePage from './pages/seller/RevenuePage';
import SellerSettingsPage from './pages/seller/SettingsPage';
import SellerReviewsPage from './pages/seller/ReviewsPage';
import SellerMessagesPage from './pages/seller/MessagesPage';

// Admin pages
import AdminDashboardPage from './pages/admin/DashboardPage';
import AdminProductsPage from './pages/admin/ProductsPage';
import AdminOrderDetailPage from './pages/admin/OrderDetailPage';
import AdminOrdersPage from './pages/admin/OrdersPage';
import AdminUsersPage from './pages/admin/UsersPage';
import AdminStoresPage from './pages/admin/StoresPage';
import AdminStoreDetailPage from './pages/admin/StoreDetailPage';
import AdminStoreApplicationsPage from './pages/admin/StoreApplicationsPage';
import AdminStoreApplicationDetailPage from './pages/admin/StoreApplicationDetailPage';
import AdminProductDetailPage from './pages/admin/ProductDetailPage';
import AdminBrandsPage from './pages/admin/BrandsPage';
import AdminCategoriesPage from './pages/admin/CategoriesPage';
import AdminVouchersPage from './pages/admin/VouchersPage';
import AdminPayoutsPage from './pages/admin/PayoutsPage';
import AdminPayoutBatchesPage from './pages/admin/PayoutBatchesPage';
import AdminPayoutBatchDetailPage from './pages/admin/PayoutBatchDetailPage';
import AdminPayoutsTransactionDetailPage from './pages/admin/PayoutsTransactionDetailPage';
import AdminSellerPayableSummaryPage from './pages/admin/SellerPayableSummaryPage';
import AdminSettingsPage from './pages/admin/SettingsPage';
import AdminWalletPage from './pages/admin/WalletPage';
import AdminTransactionsPage from './pages/admin/TransactionsPage';

function RequireAuth({ children, roles }: { children: JSX.Element; roles?: string[] }) {
  const { user, loading } = useAuth();
  const location = useLocation();
  if (loading) return (
    <div className="flex items-center justify-center min-h-[60vh]">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  );
  if (!user) return <Navigate to="/login" state={{ from: location }} replace />;
  if (roles && !roles.includes(user.role ?? '')) return <Navigate to="/" replace />;
  return children;
}

export default function App() {
  return (
    <Routes>
      {/* Standalone pages (no navbar/footer) */}
      <Route path="/login" element={<LoginPage />} />

      <Route element={<Layout />}>
        {/* Public */}
        <Route path="/" element={<HomePage />} />
        <Route path="/products" element={<ProductsPage />} />
        <Route path="/products/:slug" element={<ProductDetailPage />} />
        <Route path="/store/:slug" element={<StoreProfilePage />} />

        {/* Buyer (authenticated) */}
        <Route path="/cart" element={<RequireAuth><CartPage /></RequireAuth>} />
        <Route path="/checkout" element={<RequireAuth><CheckoutPage /></RequireAuth>} />
        <Route path="/checkout/payos" element={<RequireAuth><PayOSCheckoutPage /></RequireAuth>} />
        <Route path="/checkout/success/:orderId" element={<RequireAuth><OrderSuccessPage /></RequireAuth>} />
        <Route path="/orders/track/:subOrderId" element={<RequireAuth><OrderTrackPage /></RequireAuth>} />
        <Route path="/profile" element={<RequireAuth><ProfilePage /></RequireAuth>} />
        <Route path="/write-review/:orderItemId" element={<RequireAuth><WriteReviewPage /></RequireAuth>} />
        <Route path="/register-seller" element={<RequireAuth><RegisterSellerPage /></RequireAuth>} />

        {/* Seller */}
        <Route path="/seller/dashboard" element={<RequireAuth roles={['Store Owner']}><SellerDashboardPage /></RequireAuth>} />
        <Route path="/seller/products" element={<RequireAuth roles={['Store Owner']}><SellerProductsPage /></RequireAuth>} />
        <Route path="/seller/orders" element={<RequireAuth roles={['Store Owner']}><SellerOrdersPage /></RequireAuth>} />
        <Route path="/seller/vouchers" element={<RequireAuth roles={['Store Owner']}><SellerVouchersPage /></RequireAuth>} />
        <Route path="/seller/revenue" element={<RequireAuth roles={['Store Owner']}><SellerRevenuePage /></RequireAuth>} />
        <Route path="/seller/settings" element={<RequireAuth roles={['Store Owner']}><SellerSettingsPage /></RequireAuth>} />
        <Route path="/seller/reviews" element={<RequireAuth roles={['Store Owner']}><SellerReviewsPage /></RequireAuth>} />
        <Route path="/seller/messages" element={<RequireAuth roles={['Store Owner']}><SellerMessagesPage /></RequireAuth>} />

        {/* Admin */}
        <Route path="/admin/dashboard" element={<RequireAuth roles={['Super Admin']}><AdminDashboardPage /></RequireAuth>} />
        <Route path="/admin/products" element={<RequireAuth roles={['Super Admin']}><AdminProductsPage /></RequireAuth>} />
        <Route path="/admin/products/:id" element={<RequireAuth roles={['Super Admin']}><AdminProductDetailPage /></RequireAuth>} />
        <Route path="/admin/orders" element={<RequireAuth roles={['Super Admin']}><AdminOrdersPage /></RequireAuth>} />
        <Route path="/admin/orders/:id" element={<RequireAuth roles={['Super Admin']}><AdminOrderDetailPage /></RequireAuth>} />
        <Route path="/admin/users" element={<RequireAuth roles={['Super Admin']}><AdminUsersPage /></RequireAuth>} />
        <Route path="/admin/stores" element={<RequireAuth roles={['Super Admin']}><AdminStoresPage /></RequireAuth>} />
        <Route path="/admin/stores/:id" element={<RequireAuth roles={['Super Admin']}><AdminStoreDetailPage /></RequireAuth>} />
        <Route path="/admin/store-applications" element={<RequireAuth roles={['Super Admin']}><AdminStoreApplicationsPage /></RequireAuth>} />
        <Route path="/admin/store-applications/:id" element={<RequireAuth roles={['Super Admin']}><AdminStoreApplicationDetailPage /></RequireAuth>} />
        <Route path="/admin/brands" element={<RequireAuth roles={['Super Admin']}><AdminBrandsPage /></RequireAuth>} />
        <Route path="/admin/categories" element={<RequireAuth roles={['Super Admin']}><AdminCategoriesPage /></RequireAuth>} />
        <Route path="/admin/vouchers" element={<RequireAuth roles={['Super Admin']}><AdminVouchersPage /></RequireAuth>} />
        <Route path="/admin/payouts" element={<RequireAuth roles={['Super Admin']}><AdminPayoutsPage /></RequireAuth>} />
        <Route path="/admin/payouts/batches" element={<RequireAuth roles={['Super Admin']}><AdminPayoutBatchesPage /></RequireAuth>} />
        <Route path="/admin/payouts/batches/:id" element={<RequireAuth roles={['Super Admin']}><AdminPayoutBatchDetailPage /></RequireAuth>} />
        <Route path="/admin/payouts/transactions/:id" element={<RequireAuth roles={['Super Admin']}><AdminPayoutsTransactionDetailPage /></RequireAuth>} />
        <Route path="/admin/payouts/seller-summary" element={<RequireAuth roles={['Super Admin']}><AdminSellerPayableSummaryPage /></RequireAuth>} />
        <Route path="/admin/settings" element={<RequireAuth roles={['Super Admin']}><AdminSettingsPage /></RequireAuth>} />
        <Route path="/admin/wallet" element={<RequireAuth roles={['Super Admin']}><AdminWalletPage /></RequireAuth>} />
        <Route path="/admin/transactions" element={<RequireAuth roles={['Super Admin']}><AdminTransactionsPage /></RequireAuth>} />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
