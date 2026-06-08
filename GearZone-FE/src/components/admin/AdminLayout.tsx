import { useState } from 'react'
import type { ReactNode } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'

interface AdminLayoutProps {
  activePage: string
  breadcrumb: string[]
  children: ReactNode
  pageHeader: string
}

interface AdminNavItem {
  label: string
  icon: string
  to?: string
  badge?: string
}

const navGroups: Array<{ title?: string; items: AdminNavItem[] }> = [
  {
    items: [{ label: 'Overview', icon: 'dashboard', to: '/admin/dashboard' }],
  },
  {
    title: 'Management',
    items: [
      { label: 'Store Applications', icon: 'storefront', to: '/admin/store-applications' },
      { label: 'Stores', icon: 'store', to: '/admin/stores' },
      { label: 'Users', icon: 'group', to: '/admin/users' },
      { label: 'Orders', icon: 'shopping_bag', to: '/admin/orders' },
      { label: 'Product', icon: 'inventory_2', to: '/admin/products' },
      { label: 'Categories', icon: 'category', to: '/admin/categories' },
      { label: 'Brands', icon: 'branding_watermark', to: '/admin/brands' },
      { label: 'Vouchers', icon: 'confirmation_number', to: '/admin/vouchers' },
    ],
  },
  {
    title: 'Finance',
    items: [
      { label: 'Transactions', icon: 'receipt_long', to: '/admin/transactions' },
      { label: 'Wallet', icon: 'account_balance_wallet', to: '/admin/wallet' },
      { label: 'Payouts', icon: 'payments', to: '/admin/payouts' },
      { label: 'Seller Payable', icon: 'summarize', to: '/admin/seller-payable' },
      { label: 'Payout Batches', icon: 'account_balance_wallet', to: '/admin/payout-batches' },
      { label: 'Disputes', icon: 'gavel', badge: '7' },
    ],
  },
  {
    title: 'System',
    items: [
      { label: 'Revenue', icon: 'monitoring' },
      { label: 'System Logs', icon: 'receipt_long' },
      { label: 'Settings', icon: 'settings', to: '/admin/settings' },
    ],
  },
]

function initials(name?: string | null) {
  if (!name) return 'AD'
  return name
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')
}

export function AdminLayout({ activePage, breadcrumb, children, pageHeader }: AdminLayoutProps) {
  const { logout, user } = useAuth()
  const navigate = useNavigate()
  const [isCollapsed, setIsCollapsed] = useState(() => localStorage.getItem('admin-sidebar-collapsed') === 'true')

  const toggleSidebar = () => {
    const nextValue = !isCollapsed
    localStorage.setItem('admin-sidebar-collapsed', String(nextValue))
    setIsCollapsed(nextValue)
  }

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  return (
    <div className="flex h-screen overflow-hidden bg-slate-50 text-slate-700">
      <aside
        className={`flex shrink-0 flex-col border-r border-slate-200 bg-white shadow-sm transition-all duration-300 ${
          isCollapsed ? 'w-20' : 'w-64'
        }`}
      >
        <div className={`flex h-16 items-center border-b border-slate-200 px-6 ${isCollapsed ? 'justify-center px-0' : ''}`}>
          <Link to="/admin/dashboard" className="flex items-center gap-2 text-xl font-bold text-primary">
            <span className="material-symbols-outlined filled text-3xl text-primary">hexagon</span>
            {!isCollapsed && <span className="tracking-tight text-slate-800">GearZone Admin</span>}
          </Link>
        </div>

        <nav className="flex flex-1 flex-col gap-1 overflow-y-auto px-3 py-6">
          {navGroups.map((group, groupIndex) => (
            <div key={group.title ?? `group-${groupIndex}`}>
              {group.title && !isCollapsed && (
                <div className="mt-1 px-3 py-2">
                  <span className="text-[10px] font-bold uppercase tracking-[0.2em] text-slate-400">{group.title}</span>
                </div>
              )}

              {group.items.map((item) => {
                const itemActive = item.label === activePage
                const content = (
                  <>
                    <span className={`material-symbols-outlined ${itemActive ? 'filled text-primary' : 'text-slate-500'}`}>{item.icon}</span>
                    {!isCollapsed && <span className="text-sm font-medium">{item.label}</span>}
                    {!isCollapsed && item.badge && (
                      <span className="ml-auto rounded-full bg-red-100 px-2 py-0.5 text-xs font-bold text-red-600">{item.badge}</span>
                    )}
                  </>
                )

                const className = `flex items-center gap-3 rounded-lg px-3 py-2.5 transition-colors ${
                  isCollapsed ? 'justify-center px-0' : ''
                } ${itemActive ? 'bg-primary/10 text-primary' : 'text-slate-500 hover:bg-slate-50 hover:text-primary'}`

                if (!item.to) {
                  return (
                    <button key={item.label} type="button" className={`${className} w-full text-left`}>
                      {content}
                    </button>
                  )
                }

                return (
                  <NavLink key={item.label} to={item.to} className={className}>
                    {content}
                  </NavLink>
                )
              })}
            </div>
          ))}
        </nav>

        <div className="mt-auto border-t border-slate-200">
          <div className={`flex items-center gap-3 p-4 ${isCollapsed ? 'justify-center' : ''}`}>
            <div className="flex size-9 shrink-0 items-center justify-center rounded-xl bg-primary text-xs font-bold text-white shadow-sm">
              {initials(user?.fullName)}
            </div>
            {!isCollapsed && (
              <>
                <div className="min-w-0">
                  <p className="truncate text-sm font-bold leading-none text-slate-800">{user?.fullName ?? 'Admin User'}</p>
                  <p className="mt-1 truncate text-[10px] font-medium text-slate-400">{user?.role ?? 'Administrator'}</p>
                </div>
                <button type="button" onClick={handleLogout} className="ml-auto text-slate-400 transition hover:text-red-600" title="Log out">
                  <span className="material-symbols-outlined text-[20px]">logout</span>
                </button>
              </>
            )}
          </div>
        </div>
      </aside>

      <main className="relative flex h-screen flex-1 flex-col overflow-hidden">
        <header className="z-20 flex h-16 shrink-0 items-center justify-between border-b border-slate-200 bg-white px-6 shadow-sm">
          <div className="flex items-center gap-4">
            <button type="button" onClick={toggleSidebar} className="rounded-lg p-2 text-slate-500 transition hover:bg-slate-100">
              <span className="material-symbols-outlined">menu</span>
            </button>
            <div className="flex flex-col">
              <h1 className="text-lg font-semibold leading-tight text-slate-800">{pageHeader}</h1>
              <nav className="mt-0.5 flex items-center gap-1.5 text-xs text-slate-500">
                <Link to="/admin/dashboard" className="hover:text-primary">
                  Admin
                </Link>
                {breadcrumb.map((crumb) => (
                  <span key={crumb} className="flex items-center gap-1.5">
                    <span className="material-symbols-outlined scale-75 text-[10px]">chevron_right</span>
                    <span className="font-medium text-slate-400">{crumb}</span>
                  </span>
                ))}
              </nav>
            </div>
          </div>

          <div className="flex items-center gap-1 rounded-xl bg-slate-50 p-1">
            <button type="button" className="relative rounded-lg p-2 text-slate-500 transition hover:bg-white hover:text-primary hover:shadow-sm">
              <span className="material-symbols-outlined text-[22px]">notifications</span>
              <span className="absolute right-2.5 top-2.5 size-2 rounded-full border-2 border-white bg-red-500" />
            </button>
            <button type="button" className="rounded-lg p-2 text-slate-500 transition hover:bg-white hover:text-primary hover:shadow-sm">
              <span className="material-symbols-outlined text-[22px]">mail</span>
            </button>
          </div>
        </header>

        <div className="flex-1 overflow-y-auto bg-slate-50 p-6">
          <div className="mx-auto flex max-w-7xl flex-col gap-6">{children}</div>
        </div>
      </main>
    </div>
  )
}
