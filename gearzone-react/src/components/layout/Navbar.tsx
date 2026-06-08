import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useState, useEffect, useRef } from 'react';
import { cartApi } from '../../api/cart';
import { catalogApi } from '../../api/catalog';

interface Category { name: string; slug: string; }

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [cartCount, setCartCount] = useState(0);
  const [categories, setCategories] = useState<Category[]>([]);
  const [menuOpen, setMenuOpen] = useState(false);
  const [search, setSearch] = useState(searchParams.get('search') ?? '');
  const menuRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    catalogApi.categories()
      .then(d => setCategories((d as Category[]) ?? []))
      .catch(() => {});
  }, []);

  useEffect(() => {
    if (user) {
      cartApi.get().then(cart => {
        setCartCount(((cart as { items?: unknown[] }).items?.length) ?? 0);
      }).catch(() => {});
    } else {
      setCartCount(0);
    }
  }, [user]);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    navigate(`/products?search=${encodeURIComponent(search)}&page=1`);
  };

  const handleLogout = async () => {
    setMenuOpen(false);
    await logout();
    navigate('/login');
  };

  return (
    <>
      <header className="sticky top-0 z-50 bg-white border-b border-gray-200 shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-20 gap-8">
            {/* Logo */}
            <Link to="/" className="flex items-center gap-2 flex-shrink-0 group">
              <span className="material-symbols-outlined text-4xl text-primary">memory</span>
              <span className="text-2xl font-bold tracking-tight text-gray-900 group-hover:text-primary transition-colors">GearZone</span>
            </Link>

            {/* Search */}
            <form onSubmit={handleSearch} className="flex-1 max-w-2xl relative group">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                <span className="material-symbols-outlined text-gray-400 text-[20px]">search</span>
              </div>
              <input
                value={search}
                onChange={e => setSearch(e.target.value)}
                className="block w-full pl-10 pr-10 py-2.5 border border-gray-300 rounded-lg bg-gray-50 placeholder-gray-500 focus:outline-none focus:bg-white focus:ring-1 focus:ring-primary focus:border-primary text-sm transition-all"
                placeholder="Search for products, brands..."
              />
              <button type="submit" className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-primary transition-colors">
                <span className="material-symbols-outlined text-[20px]">arrow_forward</span>
              </button>
            </form>

            {/* Right actions */}
            <div className="flex items-center gap-4 flex-shrink-0">
              {/* Cart */}
              <Link to="/cart" className="relative p-2 text-gray-500 hover:text-primary transition-colors rounded-full hover:bg-gray-100">
                <span className="material-symbols-outlined">shopping_cart</span>
                {cartCount > 0 && (
                  <span className="absolute top-0 right-0 inline-flex items-center justify-center min-w-[18px] h-[18px] px-1 text-[10px] font-bold text-white bg-red-600 rounded-full translate-x-1/4 -translate-y-1/4">
                    {cartCount}
                  </span>
                )}
              </Link>

              {user ? (
                <div className="relative flex items-center gap-3 pl-4 border-l border-gray-200" ref={menuRef}>
                  <button
                    onClick={() => setMenuOpen(o => !o)}
                    className="flex items-center gap-2 focus:outline-none group"
                  >
                    <div className="w-9 h-9 rounded-full bg-blue-50 flex items-center justify-center text-primary group-hover:bg-primary group-hover:text-white transition-all">
                      <span className="material-symbols-outlined text-[20px]">person</span>
                    </div>
                    <div className="hidden sm:block text-left">
                      <p className="text-xs text-gray-500 font-medium">Welcome back,</p>
                      <p className="text-sm font-bold text-gray-900 leading-tight truncate max-w-[120px]">{user.fullName}</p>
                    </div>
                    <span className="material-symbols-outlined text-gray-400 text-[20px]">expand_more</span>
                  </button>

                  {menuOpen && (
                    <div className="absolute right-0 top-full mt-2 w-56 bg-white rounded-xl shadow-xl border border-gray-100 py-2 z-[100]">
                      <div className="px-4 py-3 border-b border-gray-50">
                        <p className="text-xs font-semibold text-gray-400 uppercase tracking-wider">Account</p>
                        <p className="text-sm font-bold text-gray-900 truncate">{user.fullName}</p>
                      </div>
                      <div className="py-1">
                        <Link to="/profile" onClick={() => setMenuOpen(false)} className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50 hover:text-primary transition-colors">
                          <span className="material-symbols-outlined text-[20px]">account_circle</span>
                          My Profile
                        </Link>
                        <Link to="/profile?tab=orders" onClick={() => setMenuOpen(false)} className="flex items-center gap-3 px-4 py-2.5 text-sm text-gray-700 hover:bg-gray-50 hover:text-primary transition-colors">
                          <span className="material-symbols-outlined text-[20px]">shopping_bag</span>
                          My Orders
                        </Link>
                        {(user.role === 'Super Admin' || user.role === 'Admin') && (
                          <Link to="/admin/dashboard" onClick={() => setMenuOpen(false)} className="flex items-center gap-3 px-4 py-2.5 text-sm text-blue-600 hover:bg-blue-50 font-bold border-t border-gray-50 mt-1 transition-colors">
                            <span className="material-symbols-outlined text-[20px]">admin_panel_settings</span>
                            Admin Panel
                          </Link>
                        )}
                        {user.role === 'Store Owner' && (
                          <Link to="/seller/dashboard" onClick={() => setMenuOpen(false)} className="flex items-center gap-3 px-4 py-2.5 text-sm text-blue-600 hover:bg-blue-50 font-bold border-t border-gray-50 mt-1 transition-colors">
                            <span className="material-symbols-outlined text-[20px]">store</span>
                            Seller Dashboard
                          </Link>
                        )}
                      </div>
                      <div className="pt-1 mt-1 border-t border-gray-50">
                        <button onClick={handleLogout} className="flex items-center gap-3 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 font-medium w-full text-left transition-colors">
                          <span className="material-symbols-outlined text-[20px]">logout</span>
                          Sign Out
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              ) : (
                <Link to="/login" className="hidden md:flex items-center justify-center px-5 py-2.5 text-sm font-semibold rounded-lg text-white bg-secondary hover:bg-orange-600 transition-colors shadow-sm">
                  Sign Up / Login
                </Link>
              )}
            </div>
          </div>
        </div>
      </header>

      {/* Category nav */}
      <nav className="bg-white border-b border-gray-200 hidden md:block relative z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center gap-5 h-12 text-[13px] font-medium overflow-x-auto" style={{ scrollbarWidth: 'none' }}>
            <Link to="/products" className="text-primary border-b-2 border-primary flex items-center gap-1.5 h-full px-1 whitespace-nowrap flex-shrink-0">
              <span className="material-symbols-outlined text-[18px]">grid_view</span>
              All Categories
            </Link>
            {categories.slice(0, 10).map(c => (
              <Link key={c.slug} to={`/products?categorySlug=${c.slug}`}
                className="text-gray-500 border-b-2 border-transparent hover:text-primary hover:border-primary h-full flex items-center px-1 whitespace-nowrap transition-colors flex-shrink-0">
                {c.name}
              </Link>
            ))}
            <Link to="/" className="text-red-600 hover:text-red-700 font-semibold h-full flex items-center px-1 whitespace-nowrap flex-shrink-0 ml-auto">
              <span className="material-symbols-outlined text-[18px] mr-1">bolt</span>
              Flash
            </Link>
          </div>
        </div>
      </nav>
    </>
  );
}
