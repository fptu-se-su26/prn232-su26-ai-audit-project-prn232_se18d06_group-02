import type { ReactElement } from 'react'
import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'
import LoginPage from '@/pages/LoginPage'
import HomePage from '@/pages/HomePage'
import CustomerShellPage from '@/pages/CustomerShellPage'
import StoreOwnerShellPage from '@/pages/StoreOwnerShellPage'
import AdminShellPage from '@/pages/AdminShellPage'
import AdminDashboardPage from '@/pages/AdminDashboardPage'
import AdminStoreApplicationDetailPage from '@/pages/AdminStoreApplicationDetailPage'
import AdminStoreApplicationsPage from '@/pages/AdminStoreApplicationsPage'
import StaffShellPage from '@/pages/StaffShellPage'

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

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
