import { useState } from 'react'
import type { ReactNode } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'

interface SellerLayoutProps {
  children: ReactNode
  pageHeader: string
  breadcrumb?: string[]
}

const NAV_ITEMS = [
  { label: 'Dashboard', icon: 'dashboard', to: '/store-owner/dashboard' },
  { label: 'Products', icon: 'inventory_2', to: '/store-owner/products' },
  { label: 'Orders', icon: 'shopping_bag', to: '/store-owner/orders' },
  { label: 'Reviews', icon: 'star', to: '/store-owner/reviews' },
  { label: 'Revenue', icon: 'payments', to: '/store-owner/revenue' },
  { label: 'Vouchers', icon: 'confirmation_number', to: '/store-owner/vouchers' },
  { label: 'Settings', icon: 'settings', to: '/store-owner/settings' },
]

function initials(name?: string | null) {
  if (!name) return 'SO'
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')
}

export function SellerLayout({ children, pageHeader, breadcrumb }: SellerLayoutProps) {
  const { logout, user } = useAuth()
  const navigate = useNavigate()
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem('seller-sidebar-collapsed') === 'true')

  const toggle = () => {
    const next = !collapsed
    localStorage.setItem('seller-sidebar-collapsed', String(next))
    setCollapsed(next)
  }

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  return (
    <div className="flex h-screen overflow-hidden bg-slate-50 text-slate-700">
      {/* Sidebar */}
      <aside
        className={`flex shrink-0 flex-col border-r border-slate-200 bg-white shadow-sm transition-all duration-300 ${
          collapsed ? 'w-20' : 'w-64'
        }`}
      >
        <div className={`flex h-16 items-center border-b border-slate-200 px-5 ${collapsed ? 'justify-center px-0' : ''}`}>
          <Link to="/store-owner/dashboard" className="flex items-center gap-2">
            <span className="material-symbols-outlined text-3xl text-[#ff6b00]">storefront</span>
            {!collapsed && <span className="text-lg font-bold tracking-tight text-slate-800">Seller Hub</span>}
          </Link>
        </div>

        <nav className="flex flex-1 flex-col gap-0.5 overflow-y-auto px-3 py-5">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              className={({ isActive }) =>
                `flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors ${
                  collapsed ? 'justify-center' : ''
                } ${
                  isActive
                    ? 'bg-orange-50 text-[#ff6b00]'
                    : 'text-slate-500 hover:bg-slate-50 hover:text-[#ff6b00]'
                }`
              }
            >
              {({ isActive }) => (
                <>
                  <span
                    className={`material-symbols-outlined ${
                      isActive ? 'text-[#ff6b00]' : 'text-slate-400'
                    }`}
                    style={{ fontVariationSettings: isActive ? "'FILL' 1" : "'FILL' 0" }}
                  >
                    {item.icon}
                  </span>
                  {!collapsed && item.label}
                </>
              )}
            </NavLink>
          ))}
        </nav>

        <div className="border-t border-slate-200">
          <div className={`flex items-center gap-3 p-4 ${collapsed ? 'justify-center' : ''}`}>
            <div className="flex size-9 shrink-0 items-center justify-center rounded-xl bg-[#ff6b00] text-xs font-bold text-white shadow-sm">
              {initials(user?.fullName)}
            </div>
            {!collapsed && (
              <>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-semibold leading-none text-slate-800">{user?.fullName ?? 'Seller'}</p>
                  <p className="mt-1 truncate text-[10px] font-medium text-slate-400">Store Owner</p>
                </div>
                <button
                  type="button"
                  onClick={handleLogout}
                  className="text-slate-400 transition hover:text-red-500"
                  title="Log out"
                >
                  <span className="material-symbols-outlined text-[20px]">logout</span>
                </button>
              </>
            )}
          </div>
        </div>
      </aside>

      {/* Main content */}
      <main className="relative flex h-screen flex-1 flex-col overflow-hidden">
        <header className="z-20 flex h-16 shrink-0 items-center justify-between border-b border-slate-200 bg-white px-6 shadow-sm">
          <div className="flex items-center gap-4">
            <button
              type="button"
              onClick={toggle}
              className="rounded-lg p-2 text-slate-500 transition hover:bg-slate-100"
            >
              <span className="material-symbols-outlined">menu</span>
            </button>
            <div>
              <h1 className="text-lg font-semibold leading-tight text-slate-800">{pageHeader}</h1>
              {breadcrumb && breadcrumb.length > 0 && (
                <nav className="mt-0.5 flex items-center gap-1 text-xs text-slate-500">
                  <Link to="/store-owner/dashboard" className="transition hover:text-[#ff6b00]">
                    Dashboard
                  </Link>
                  {breadcrumb.map((crumb) => (
                    <span key={crumb} className="flex items-center gap-1">
                      <span className="material-symbols-outlined text-[10px]">chevron_right</span>
                      <span className="text-slate-400">{crumb}</span>
                    </span>
                  ))}
                </nav>
              )}
            </div>
          </div>

          <Link
            to="/"
            className="flex items-center gap-1.5 rounded-lg border border-slate-200 px-3 py-1.5 text-sm text-slate-500 transition hover:bg-slate-50 hover:text-[#ff6b00]"
          >
            <span className="material-symbols-outlined text-[18px]">open_in_new</span>
            View Storefront
          </Link>
        </header>

        <div className="flex-1 overflow-y-auto bg-slate-50 p-6">
          <div className="mx-auto flex max-w-7xl flex-col gap-6">{children}</div>
        </div>
      </main>
    </div>
  )
}
