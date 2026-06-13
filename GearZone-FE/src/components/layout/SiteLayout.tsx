/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useMemo, useRef, useState, type FormEvent } from 'react'
import { Link, NavLink, Outlet, useLocation, useNavigate, useParams } from 'react-router-dom'
import { getCatalogCategories, getProductSuggestions } from '@/api/catalog'
import { useAuth } from '@/contexts/useAuth'
import { getShellPath, resolveShellRole } from '@/lib/roleShell'
import type { CatalogCategory, ProductSuggestion } from '@/types/catalog'

const navLabelOverrides: Record<string, string> = {
  'Console & Controllers': 'Console',
  'Setup Accessories': 'Setup',
  'Gaming Furniture': 'Furniture',
}

function getCategoryNavLabel(name: string) {
  return navLabelOverrides[name] ?? name
}

function formatPrice(value: number) {
  return new Intl.NumberFormat('vi-VN').format(value)
}

function findActiveSlug(categories: CatalogCategory[], pathname: string) {
  const match = pathname.match(/^\/products\/([^/?#]+)/i)
  const slug = match?.[1]
  if (!slug) return ''

  for (const category of categories) {
    if (category.slug === slug) return slug
    if (category.subCategories.some((subCategory) => subCategory.slug === slug)) return slug
  }

  return slug
}

function readCartCount(payload: unknown) {
  if (!payload || typeof payload !== 'object') return 0

  const data = 'data' in payload ? (payload as { data?: unknown }).data : payload
  if (!data || typeof data !== 'object') return 0

  const storeGroups = (data as { storeGroups?: unknown }).storeGroups
  if (!Array.isArray(storeGroups)) return 0

  return storeGroups.reduce((total, group) => {
    if (!group || typeof group !== 'object') return total
    const items = (group as { items?: unknown }).items
    if (!Array.isArray(items)) return total

    return (
      total +
      items.reduce((groupTotal, item) => {
        if (!item || typeof item !== 'object') return groupTotal
        const quantity = (item as { quantity?: unknown }).quantity
        return groupTotal + (typeof quantity === 'number' ? quantity : 0)
      }, 0)
    )
  }, 0)
}

export default function SiteLayout() {
  const navigate = useNavigate()
  const location = useLocation()
  const routeParams = useParams()
  const { user, logout } = useAuth()
  const [categories, setCategories] = useState<CatalogCategory[]>([])
  const [searchDraft, setSearchDraft] = useState('')
  const [userMenuOpen, setUserMenuOpen] = useState(false)
  const [hoveredCategorySlug, setHoveredCategorySlug] = useState('')
  const [dropdownPosition, setDropdownPosition] = useState({ left: 0, top: 0 })
  const [suggestions, setSuggestions] = useState<ProductSuggestion[]>([])
  const [searchOpen, setSearchOpen] = useState(false)
  const [searching, setSearching] = useState(false)
  const [cartCount, setCartCount] = useState(0)
  const searchContainerRef = useRef<HTMLDivElement | null>(null)
  const mobileSearchContainerRef = useRef<HTMLDivElement | null>(null)
  const categoryCloseTimeoutRef = useRef<number | null>(null)
  const currentSlug = routeParams.slug ?? findActiveSlug(categories, location.pathname)

  useEffect(() => {
    let cancelled = false

    async function loadCategories() {
      try {
        const result = await getCatalogCategories()
        if (!cancelled) setCategories(result)
      } catch {
        if (!cancelled) setCategories([])
      }
    }

    loadCategories()

    return () => {
      cancelled = true
    }
  }, [])

  useEffect(() => {
    const params = new URLSearchParams(location.search)
    setSearchDraft(params.get('search') ?? '')
  }, [location.search])

  useEffect(() => {
    const handleDocumentClick = () => setUserMenuOpen(false)
    if (!userMenuOpen) return

    document.addEventListener('click', handleDocumentClick)
    return () => document.removeEventListener('click', handleDocumentClick)
  }, [userMenuOpen])

  useEffect(() => {
    const handleDocumentClick = (event: MouseEvent) => {
      const target = event.target as Node
      const clickedDesktopSearch = searchContainerRef.current?.contains(target)
      const clickedMobileSearch = mobileSearchContainerRef.current?.contains(target)

      if (!clickedDesktopSearch && !clickedMobileSearch) {
        setSearchOpen(false)
      }
    }

    document.addEventListener('click', handleDocumentClick)
    return () => document.removeEventListener('click', handleDocumentClick)
  }, [])

  useEffect(() => {
    return () => {
      if (categoryCloseTimeoutRef.current) {
        window.clearTimeout(categoryCloseTimeoutRef.current)
      }
    }
  }, [])

  useEffect(() => {
    const query = searchDraft.trim()
    if (query.length < 2) {
      setSuggestions([])
      setSearching(false)
      setSearchOpen(false)
      return
    }

    const timeoutId = window.setTimeout(async () => {
      try {
        setSearching(true)
        const result = await getProductSuggestions(query)
        setSuggestions(result)
        setSearchOpen(true)
      } catch {
        setSuggestions([])
      } finally {
        setSearching(false)
      }
    }, 250)

    return () => window.clearTimeout(timeoutId)
  }, [searchDraft])

  useEffect(() => {
    let cancelled = false

    async function loadCartCount() {
      if (!user) {
        setCartCount(0)
        return
      }

      try {
        const response = await fetch('/api/cart', {
          credentials: 'include',
        })

        if (!response.ok) {
          if (!cancelled) setCartCount(0)
          return
        }

        const payload = await response.json().catch(() => null)
        if (!cancelled) {
          setCartCount(readCartCount(payload))
        }
      } catch {
        if (!cancelled) setCartCount(0)
      }
    }

    void loadCartCount()

    return () => {
      cancelled = true
    }
  }, [user])

  useEffect(() => {
    const handleCartCountUpdated = (event: Event) => {
      const customEvent = event as CustomEvent<{ count?: number }>
      const nextCount = customEvent.detail?.count
      if (typeof nextCount === 'number') {
        setCartCount(nextCount)
      }
    }

    window.addEventListener('gearzone:cart-count-updated', handleCartCountUpdated)
    return () => window.removeEventListener('gearzone:cart-count-updated', handleCartCountUpdated)
  }, [])

  const activeParentSlug = useMemo(() => {
    for (const category of categories) {
      if (category.slug === currentSlug || category.subCategories.some((subCategory) => subCategory.slug === currentSlug)) {
        return category.slug
      }
    }

    return ''
  }, [categories, currentSlug])

  const activeDropdownCategory = useMemo(
    () => categories.find((category) => category.slug === hoveredCategorySlug && category.subCategories.length > 0) ?? null,
    [categories, hoveredCategorySlug],
  )

  const footerPrimaryCategories = useMemo(() => categories.slice(0, 6), [categories])

  const openCategoryPanel = (categorySlug: string, anchor?: HTMLElement) => {
    if (categoryCloseTimeoutRef.current) {
      window.clearTimeout(categoryCloseTimeoutRef.current)
      categoryCloseTimeoutRef.current = null
    }

    if (anchor) {
      const anchorRect = anchor.getBoundingClientRect()
      const panelWidth = 224
      const preferredLeft = anchorRect.left
      const maxLeft = Math.max(8, window.innerWidth - panelWidth - 8)

      setDropdownPosition({
        left: Math.max(8, Math.min(preferredLeft, maxLeft)),
        top: anchorRect.bottom,
      })
    }

    setHoveredCategorySlug(categorySlug)
  }

  const closeCategoryPanel = () => {
    if (categoryCloseTimeoutRef.current) {
      window.clearTimeout(categoryCloseTimeoutRef.current)
    }

    categoryCloseTimeoutRef.current = window.setTimeout(() => {
      setHoveredCategorySlug('')
    }, 120)
  }

  const handleSuggestionSelect = (query: string) => {
    const params = new URLSearchParams()
    if (query.trim()) params.set('search', query.trim())

    navigate({
      pathname: '/products',
      search: params.toString() ? `?${params.toString()}` : '',
    })
    setSearchOpen(false)
  }

  const handleSearchSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    handleSuggestionSelect(searchDraft)
  }

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  const renderSearchDropdown = () => (
    <div
      className={`absolute left-0 right-0 top-full z-[100] mt-2 overflow-hidden rounded-xl border border-slate-100 bg-white shadow-2xl transition-all duration-200 ${
        searchOpen ? 'scale-100 opacity-100' : 'pointer-events-none scale-95 opacity-0'
      }`}
    >
      {searching ? (
        <div className="p-6 text-center">
          <div className="mx-auto mb-2 inline-block h-6 w-6 animate-spin rounded-full border-2 border-blue-700 border-r-transparent" />
          <p className="text-xs text-slate-500">Searching...</p>
        </div>
      ) : suggestions.length > 0 ? (
        <>
          <div className="max-h-[400px] overflow-y-auto py-2">
            {suggestions.map((suggestion) => (
              <button
                className="flex w-full items-center gap-3 px-4 py-3 text-left transition-colors hover:bg-slate-50"
                key={suggestion.slug}
                onClick={() => handleSuggestionSelect(suggestion.name)}
                type="button"
              >
                <img
                  alt={suggestion.name}
                  className="h-12 w-12 rounded-lg border border-slate-100 bg-slate-50 object-contain p-1"
                  src={suggestion.imageUrl || 'https://placehold.co/48x48/f8fafc/94a3b8?text=GZ'}
                />
                <div className="min-w-0 flex-1">
                  <p className="truncate text-sm font-semibold text-slate-900">{suggestion.name}</p>
                  <p className="mt-1 text-xs text-slate-500">{suggestion.brandName}</p>
                </div>
                <p className="text-sm font-bold text-blue-700">{formatPrice(suggestion.price)} VND</p>
              </button>
            ))}
          </div>
          <button
            className="block w-full border-t border-slate-100 bg-slate-50 py-3 text-center text-sm font-bold text-blue-700 transition-colors hover:bg-slate-100"
            onClick={() => handleSuggestionSelect(searchDraft)}
            type="button"
          >
            View All Results
          </button>
        </>
      ) : searchDraft.trim().length >= 2 ? (
        <div className="p-6 text-center text-sm text-slate-500">No matching products found.</div>
      ) : null}
    </div>
  )

  return (
    <div className="flex min-h-screen flex-col bg-slate-50 text-slate-900">
      <header className="sticky top-0 z-50 border-b border-slate-200 bg-white shadow-sm">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="flex h-20 items-center justify-between gap-8">
            <Link className="group flex flex-shrink-0 items-center gap-2" to="/">
              <div className="h-8 w-8 text-blue-700">
                <span className="material-symbols-outlined text-4xl">memory</span>
              </div>
              <span className="text-2xl font-bold tracking-tight text-slate-900 transition-colors group-hover:text-blue-700">GearZone</span>
            </Link>

            <div className="relative hidden max-w-2xl flex-1 md:block" ref={searchContainerRef}>
              <form className="group relative" onSubmit={handleSearchSubmit}>
                <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                  <span className="material-symbols-outlined text-slate-400">search</span>
                </div>
                <input
                  aria-label="Search"
                  className="block w-full rounded-lg border border-slate-300 bg-slate-50 py-2.5 pl-10 pr-10 text-sm leading-5 placeholder:text-slate-400 focus:border-blue-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-blue-700"
                  onChange={(event) => {
                    setSearchDraft(event.target.value)
                    setSearchOpen(true)
                  }}
                  onFocus={() => {
                    if (searchDraft.trim().length >= 2 || suggestions.length > 0) setSearchOpen(true)
                  }}
                  placeholder="Search for products, brands..."
                  type="text"
                  value={searchDraft}
                />
                <button className="absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400 transition-colors hover:text-blue-700" type="submit">
                  <span className="material-symbols-outlined">arrow_forward</span>
                </button>
              </form>
              {renderSearchDropdown()}
            </div>

            <div className="flex flex-shrink-0 items-center gap-4">
              <a className="relative rounded-full p-2 text-slate-500 transition-colors hover:bg-slate-100 hover:text-blue-700" href="/Cart/Index">
                <span className="material-symbols-outlined">shopping_cart</span>
                {cartCount > 0 ? (
                  <span className="absolute -right-0.5 -top-0.5 flex min-w-5 items-center justify-center rounded-full bg-[#ff6b00] px-1 text-[10px] font-bold leading-5 text-white shadow-sm">
                    {cartCount > 99 ? '99+' : cartCount}
                  </span>
                ) : null}
              </a>

              {user ? (
                <div className="relative flex items-center gap-3 border-l border-slate-200 pl-4">
                  <button
                    className="group flex items-center gap-2 focus:outline-none"
                    onClick={(event) => {
                      event.stopPropagation()
                      setUserMenuOpen((current) => !current)
                    }}
                    type="button"
                  >
                    <div className="flex h-9 w-9 items-center justify-center rounded-full bg-blue-700/10 text-blue-700 transition-all group-hover:bg-blue-700 group-hover:text-white">
                      <span className="material-symbols-outlined">person</span>
                    </div>
                    <div className="hidden text-left sm:block">
                      <p className="text-xs font-medium text-slate-500">Welcome back,</p>
                      <p className="text-sm font-bold leading-tight text-slate-900">{user.fullName}</p>
                    </div>
                    <span className="material-symbols-outlined text-slate-400 transition-colors group-hover:text-blue-700">expand_more</span>
                  </button>

                  <div
                    className={`absolute right-0 top-full z-[100] mt-2 w-56 rounded-xl border border-slate-100 bg-white py-2 shadow-xl transition-all duration-200 ${
                      userMenuOpen ? 'scale-100 opacity-100' : 'pointer-events-none scale-95 opacity-0'
                    }`}
                  >
                    <div className="border-b border-slate-50 px-4 py-3">
                      <p className="text-xs font-semibold uppercase tracking-wider text-slate-400">Account Information</p>
                      <p className="truncate text-sm font-bold text-slate-900">{user.fullName}</p>
                    </div>

                    <div className="py-1">
                      <Link className="flex items-center gap-3 px-4 py-2.5 text-sm text-slate-700 transition-colors hover:bg-slate-50 hover:text-blue-700" to={getShellPath(resolveShellRole(user.role))}>
                        <span className="material-symbols-outlined text-[20px]">dashboard</span>
                        Dashboard
                      </Link>
                      <Link className="flex items-center gap-3 px-4 py-2.5 text-sm text-slate-700 transition-colors hover:bg-slate-50 hover:text-blue-700" to="/products">
                        <span className="material-symbols-outlined text-[20px]">inventory_2</span>
                        Browse Products
                      </Link>
                    </div>

                    <div className="mt-1 border-t border-slate-50 pt-1">
                      <button
                        className="flex w-full items-center gap-3 px-4 py-2.5 text-sm font-medium text-rose-600 transition-colors hover:bg-rose-50"
                        onClick={handleLogout}
                        type="button"
                      >
                        <span className="material-symbols-outlined text-[20px]">logout</span>
                        Sign Out
                      </button>
                    </div>
                  </div>
                </div>
              ) : (
                <Link className="hidden items-center justify-center rounded-lg bg-orange-500 px-5 py-2.5 text-sm font-medium text-white shadow-sm transition-colors hover:bg-orange-600 md:flex" to="/login">
                  Sign Up / Login
                </Link>
              )}
            </div>
          </div>

          <div className="pb-4 md:hidden" ref={mobileSearchContainerRef}>
            <form className="group relative" onSubmit={handleSearchSubmit}>
              <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
                <span className="material-symbols-outlined text-slate-400">search</span>
              </div>
              <input
                aria-label="Search"
                className="block w-full rounded-lg border border-slate-300 bg-slate-50 py-2.5 pl-10 pr-10 text-sm leading-5 placeholder:text-slate-400 focus:border-blue-700 focus:bg-white focus:outline-none focus:ring-1 focus:ring-blue-700"
                onChange={(event) => {
                  setSearchDraft(event.target.value)
                  setSearchOpen(true)
                }}
                onFocus={() => {
                  if (searchDraft.trim().length >= 2 || suggestions.length > 0) setSearchOpen(true)
                }}
                placeholder="Search for products, brands..."
                type="text"
                value={searchDraft}
              />
              <button className="absolute inset-y-0 right-0 flex items-center pr-3 text-slate-400 transition-colors hover:text-blue-700" type="submit">
                <span className="material-symbols-outlined">arrow_forward</span>
              </button>
            </form>
            {renderSearchDropdown()}
          </div>
        </div>
      </header>

      <nav className="relative z-40 hidden border-b border-slate-200 bg-white md:block">
        <div
          className="mx-auto max-w-7xl overflow-visible px-4 sm:px-6 lg:px-8"
          onMouseEnter={() => {
            if (categoryCloseTimeoutRef.current) {
              window.clearTimeout(categoryCloseTimeoutRef.current)
              categoryCloseTimeoutRef.current = null
            }
          }}
          onMouseLeave={closeCategoryPanel}
        >
          <div className="hide-scrollbar overflow-x-auto overflow-y-hidden">
            <div className="relative flex h-12 min-w-max items-center gap-5 text-[13px] font-medium lg:gap-6 lg:text-sm">
              <NavLink
                className={({ isActive }) =>
                  `flex h-full items-center gap-2 whitespace-nowrap border-b-2 px-1 transition-colors ${
                    isActive && location.pathname === '/products'
                      ? 'border-blue-700 text-blue-700'
                      : 'border-transparent text-slate-500 hover:text-blue-700'
                  }`
                }
                to="/products"
              >
                <span className="material-symbols-outlined text-[20px]">grid_view</span>
                All Categories
              </NavLink>

              {categories.map((category) =>
                category.subCategories.length > 0 ? (
                  <div
                    className={`group relative flex h-full items-center border-b-2 ${
                      activeParentSlug === category.slug || hoveredCategorySlug === category.slug
                        ? 'border-blue-700 text-blue-700'
                        : 'border-transparent text-slate-500'
                    }`}
                    key={category.id}
                    onFocus={(event) => openCategoryPanel(category.slug, event.currentTarget)}
                    onMouseEnter={(event) => openCategoryPanel(category.slug, event.currentTarget)}
                  >
                    <Link className="flex h-full items-center whitespace-nowrap pl-1 transition-colors hover:text-blue-700" to={`/products/${category.slug}`}>
                      {getCategoryNavLabel(category.name)}
                    </Link>
                    <button
                      aria-expanded={hoveredCategorySlug === category.slug || activeParentSlug === category.slug}
                      className="inline-flex h-full items-center pr-1 text-current transition-colors hover:text-blue-700"
                      onClick={(event) => {
                        const nextSlug = hoveredCategorySlug === category.slug ? '' : category.slug
                        if (nextSlug) openCategoryPanel(nextSlug, event.currentTarget.parentElement ?? undefined)
                        else setHoveredCategorySlug('')
                      }}
                      type="button"
                    >
                      <span className="material-symbols-outlined text-[16px]">expand_more</span>
                    </button>
                  </div>
                ) : (
                  <Link
                    className={`flex h-full items-center whitespace-nowrap border-b-2 px-1 transition-colors ${
                      category.slug === currentSlug ? 'border-blue-700 text-blue-700' : 'border-transparent text-slate-500 hover:text-blue-700'
                    }`}
                    key={category.id}
                    to={`/products/${category.slug}`}
                  >
                    {getCategoryNavLabel(category.name)}
                  </Link>
                ),
              )}

              <a className="flex h-full items-center whitespace-nowrap px-1 font-semibold text-rose-600 transition-colors hover:text-rose-700" href="/#home-flash-zone">
                <span className="material-symbols-outlined mr-1 text-[18px]">bolt</span>
                Flash
              </a>
            </div>
          </div>
          {activeDropdownCategory ? (
            <div
              className="fixed z-[80]"
              onMouseEnter={() => openCategoryPanel(activeDropdownCategory.slug)}
              onMouseLeave={closeCategoryPanel}
              style={{ left: `${dropdownPosition.left}px`, top: `${dropdownPosition.top}px` }}
            >
              <div className="w-56 overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-2xl ring-1 ring-black/5">
                {activeDropdownCategory.subCategories.map((subCategory) => (
                  <Link
                    className={`block px-4 py-2.5 text-sm transition-colors ${
                      subCategory.slug === currentSlug
                        ? 'bg-slate-50 font-bold text-blue-700'
                        : 'text-slate-700 hover:bg-slate-100 hover:text-blue-700'
                    }`}
                    key={subCategory.id}
                    to={`/products/${subCategory.slug}`}
                  >
                    {subCategory.name}
                  </Link>
                ))}
              </div>
            </div>
          ) : null}
        </div>
      </nav>

      <main className="flex-grow overflow-x-clip">
        <Outlet />
      </main>

      <footer className="border-t border-slate-700 bg-[#1E2937] py-12 text-slate-300">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="mb-8 grid grid-cols-1 gap-8 md:grid-cols-2 lg:grid-cols-4">
            <div>
              <Link className="mb-6 flex items-center gap-2 text-white" to="/">
                <span className="material-symbols-outlined text-3xl text-blue-700">memory</span>
                <span className="text-xl font-bold tracking-tight">GearZone</span>
              </Link>
              <p className="mb-6 text-sm leading-relaxed text-slate-400">
                Vietnam&apos;s leading marketplace for PC components and high-end electronics. We connect enthusiasts
                with authentic hardware vendors.
              </p>
            </div>

            <div>
              <h4 className="mb-4 font-bold text-white">Categories</h4>
              <ul className="space-y-2 text-sm">
                {footerPrimaryCategories.map((category) => (
                  <li key={category.id}>
                    <Link className="transition-colors hover:text-blue-700" to={`/products/${category.slug}`}>
                      {category.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </div>

            <div>
              <h4 className="mb-4 font-bold text-white">Customer Support</h4>
              <ul className="space-y-2 text-sm">
                <li><span className="text-slate-400">Help Center</span></li>
                <li><span className="text-slate-400">Warranty Policy</span></li>
                <li><span className="text-slate-400">Return & Refund</span></li>
                <li><span className="text-slate-400">Shipping Info</span></li>
                <li><Link className="transition-colors hover:text-blue-700" to="/products">Track Order</Link></li>
              </ul>
            </div>

            <div>
              <h4 className="mb-4 font-bold text-white">Contact Us</h4>
              <ul className="space-y-3 text-sm">
                <li className="flex items-start gap-3">
                  <span className="material-symbols-outlined text-[20px] text-slate-500">location_on</span>
                  <span>123 Tech Street, District 1,<br />Ho Chi Minh City, Vietnam</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="material-symbols-outlined text-[20px] text-slate-500">phone</span>
                  <span>1900 123 456</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="material-symbols-outlined text-[20px] text-slate-500">mail</span>
                  <span>support@gearzone.vn</span>
                </li>
              </ul>
            </div>
          </div>

          <div className="flex flex-col items-center justify-between gap-4 border-t border-slate-700 pt-8 text-xs text-slate-500 md:flex-row">
            <p>© 2023 GearZone Vietnam. All rights reserved.</p>
            <div className="flex items-center gap-4">
              <span>Privacy Policy</span>
              <span>Terms of Service</span>
              <Link className="transition-colors hover:text-blue-700" to="/products">
                Sitemap
              </Link>
            </div>
          </div>
        </div>
      </footer>
    </div>
  )
}
