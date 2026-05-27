import { Navigate, Route, Routes } from 'react-router-dom'
import LoginPage from '@/pages/LoginPage'

function DashboardPlaceholder() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-950 text-lg font-semibold text-white">
      Dashboard coming soon.
    </div>
  )
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/dashboard" element={<DashboardPlaceholder />} />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  )
}
