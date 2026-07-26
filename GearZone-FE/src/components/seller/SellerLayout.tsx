import { useState, type ReactNode } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'

interface SellerLayoutProps {
  children: ReactNode
  pageHeader: string
  breadcrumb?: string[]
  unreadCount?: number
  contentMode?: 'default' | 'fullCanvas'
}

const NAV_ITEMS = [
  { label: 'Overview', icon: 'dashboard', to: '/store-owner/dashboard', activeKey: 'Dashboard' },
  { label: 'Products', icon: 'inventory_2', to: '/store-owner/products', section: 'Management' },
  { label: 'Orders', icon: 'shopping_bag', to: '/store-owner/orders' },
  { label: 'Vouchers', icon: 'confirmation_number', to: '/store-owner/vouchers' },
  { label: 'Messages', icon: 'chat', to: '/store-owner/messages' },
  { label: 'Reviews', icon: 'reviews', to: '/store-owner/reviews' },
  { label: 'Disputes', icon: 'gavel', to: '/store-owner/disputes', badge: '7', badgeClass: 'bg-red-100 text-red-600' },
  { label: 'Payouts', icon: 'payments', to: '/store-owner/revenue', section: 'System' },
  { label: 'Settings', icon: 'settings', to: '/store-owner/settings' },
]

function initials(name?: string | null) {
  if (!name) return 'SO'
  return name
    .split(/\s+|@/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')
}

export function SellerLayout({
  children,
  pageHeader,
  breadcrumb = ['Dashboard'],
  unreadCount = 0,
  contentMode = 'default',
}: SellerLayoutProps) {
  const { logout, user } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [collapsed, setCollapsed] = useState(
    () => localStorage.getItem('seller-sidebar-collapsed') === 'true',
  )
  const [profileOpen, setProfileOpen] = useState(false)
  const unreadLabel = unreadCount > 99 ? '99+' : String(unreadCount)
  const displayName = user?.email || user?.userName || user?.fullName || 'Store Owner'

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
    <div className="flex h-screen overflow-hidden bg-[#F8FAFC] text-slate-700">
      <aside
        className={`z-30 flex shrink-0 flex-col border-r border-slate-200 bg-white shadow-sm transition-[width] duration-300 ${
          collapsed ? 'w-20' : 'w-64'
        }`}
      >
        <div
          className={`flex h-16 items-center border-b border-slate-200 px-6 ${
            collapsed ? 'justify-center px-0' : ''
          }`}
        >
          <Link to="/store-owner/dashboard" className="flex items-center gap-2 text-primary">
            <span className="material-symbols-outlined filled text-3xl">store</span>
            {!collapsed && (
              <span className="text-xl font-bold tracking-tight text-slate-800">Seller Center</span>
            )}
          </Link>
        </div>

        <nav className="flex flex-1 flex-col gap-1 overflow-y-auto px-3 py-6">
          {NAV_ITEMS.map((item) => {
            const isActive = location.pathname === item.to
            const badge =
              item.label === 'Messages' && unreadCount > 0
                ? unreadLabel
                : 'badge' in item
                  ? item.badge
                  : null

            return (
              <div key={item.label}>
                {'section' in item && item.section && !collapsed && (
                  <div className="mb-4 mt-8 px-3">
                    <span className="text-[10px] font-bold uppercase tracking-[0.2em] text-slate-400">
                      {item.section}
                    </span>
                  </div>
                )}
                <Link
                  to={item.to}
                  className={`group flex items-center gap-3 rounded-lg px-3 py-2.5 transition-colors ${
                    collapsed ? 'justify-center px-0' : ''
                  } ${
                    isActive
                      ? 'bg-blue-50 text-primary'
                      : 'text-slate-500 hover:bg-slate-50 hover:text-primary'
                  }`}
                >
                  <span
                    className={`material-symbols-outlined ${isActive ? 'filled text-primary' : ''}`}
                  >
                    {item.icon}
                  </span>
                  {!collapsed && (
                    <>
                      <span className="text-sm font-medium">{item.label}</span>
                      {badge && (
                        <span
                          className={`ml-auto rounded-full px-2 py-0.5 text-xs font-bold ${
                            'badgeClass' in item && item.badgeClass
                              ? item.badgeClass
                              : 'bg-orange-100 text-orange-600'
                          }`}
                        >
                          {badge}
                        </span>
                      )}
                    </>
                  )}
                </Link>
              </div>
            )
          })}
        </nav>

        <div className="relative mt-auto border-t border-slate-200">
          <button
            type="button"
            onClick={() => setProfileOpen((open) => !open)}
            className={`flex w-full items-center gap-3 p-4 text-left transition-colors hover:bg-slate-50 ${
              collapsed ? 'justify-center' : ''
            }`}
          >
            <div className="flex size-9 shrink-0 items-center justify-center rounded-xl bg-primary text-xs font-bold text-white shadow-sm">
              {initials(displayName)}
            </div>
            {!collapsed && (
              <>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-bold leading-none text-slate-800">
                    {displayName}
                  </p>
                  <p className="mt-1 truncate text-[10px] font-medium text-slate-400">
                    Store Owner
                  </p>
                </div>
                <span
                  className={`material-symbols-outlined ml-auto text-slate-400 transition-transform ${
                    profileOpen ? 'rotate-180' : ''
                  }`}
                >
                  expand_less
                </span>
              </>
            )}
          </button>

          {profileOpen && !collapsed && (
            <div className="absolute bottom-full left-0 mb-1 w-full px-3">
              <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-[0_0_0_1px_rgba(0,0,0,0.03),0_2px_8px_rgba(0,0,0,0.04)]">
                <Link
                  to="/"
                  className="flex items-center gap-3 px-4 py-2.5 text-sm font-medium text-slate-600 transition-colors hover:bg-slate-50 hover:text-primary"
                >
                  <span className="material-symbols-outlined text-[20px] text-slate-400">home</span>
                  Go to Home
                </Link>
                <Link
                  to="/profile"
                  className="flex items-center gap-3 px-4 py-2.5 text-sm font-medium text-slate-600 transition-colors hover:bg-slate-50 hover:text-primary"
                >
                  <span className="material-symbols-outlined text-[20px] text-slate-400">
                    person
                  </span>
                  Account Settings
                </Link>
                <div className="my-1 border-t border-slate-200" />
                <button
                  type="button"
                  onClick={() => void handleLogout()}
                  className="flex w-full items-center gap-3 px-4 py-3 text-left text-sm font-bold text-red-600 transition-colors hover:bg-red-50"
                >
                  <span className="material-symbols-outlined text-[20px]">logout</span>
                  Log Out
                </button>
              </div>
            </div>
          )}
        </div>
      </aside>

      <main className="relative flex h-screen flex-1 flex-col overflow-hidden">
        <header className="z-20 flex h-16 shrink-0 items-center justify-between border-b border-slate-200 bg-white px-6 shadow-sm">
          <div className="flex items-center gap-4">
            <button
              type="button"
              onClick={toggle}
              className="rounded-lg p-2 text-slate-500 transition-colors hover:bg-slate-100"
            >
              <span className="material-symbols-outlined">menu</span>
            </button>
            <div className="flex flex-col">
              <h1 className="text-lg font-semibold leading-tight text-slate-800">{pageHeader}</h1>
              <nav className="mt-0.5 flex items-center gap-1.5 text-xs text-slate-500">
                <Link to="/store-owner/dashboard" className="hover:text-primary">
                  Seller Center
                </Link>
                {breadcrumb.map((crumb) => (
                  <span key={crumb} className="flex items-center gap-1.5">
                    <span className="material-symbols-outlined scale-75 text-[10px]">
                      chevron_right
                    </span>
                    <span className="font-medium text-slate-400">{crumb}</span>
                  </span>
                ))}
              </nav>
            </div>
          </div>

          <div className="flex items-center gap-4">
            <div className="flex items-center gap-1 rounded-xl bg-slate-50 p-1">
              <button className="relative rounded-lg p-2 text-slate-500 transition-all hover:bg-white hover:text-primary hover:shadow-sm">
                <span className="material-symbols-outlined text-[22px]">notifications</span>
                <span className="absolute right-2.5 top-2.5 size-2 rounded-full border-2 border-white bg-red-500" />
              </button>
              <Link
                to="/store-owner/messages"
                className="relative rounded-lg p-2 text-slate-500 transition-all hover:bg-white hover:text-primary hover:shadow-sm"
              >
                <span className="material-symbols-outlined text-[22px]">mail</span>
                {unreadCount > 0 && (
                  <span className="absolute right-2.5 top-2.5 inline-flex min-h-4 min-w-4 items-center justify-center rounded-full bg-orange-500 px-1 text-[10px] font-black text-white">
                    {unreadLabel}
                  </span>
                )}
              </Link>
            </div>
          </div>
        </header>

        <div
          className={`flex-1 scroll-smooth bg-[#F8FAFC] ${
            contentMode === 'fullCanvas'
              ? 'min-h-0 overflow-hidden px-0 pb-0 pt-4'
              : 'overflow-y-auto p-6'
          }`}
        >
          <div
            className={
              contentMode === 'fullCanvas'
                ? 'h-full min-h-0 w-full'
                : 'mx-auto flex max-w-7xl flex-col gap-6'
            }
          >
            {children}
          </div>
        </div>
      </main>
    </div>
  )
}
