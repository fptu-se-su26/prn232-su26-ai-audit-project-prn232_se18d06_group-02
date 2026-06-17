import { Outlet } from 'react-router-dom';
import { Link } from 'react-router-dom';
import Navbar from './Navbar';
import BuyerChatWidget from '../chat/BuyerChatWidget';

export default function Layout() {
  return (
    <div className="bg-slate-50 text-gray-900 min-h-screen flex flex-col">
      <Navbar />
      <main className="flex-grow overflow-x-clip">
        <Outlet />
      </main>

      <BuyerChatWidget />
      <footer className="bg-[#1E2937] text-gray-300 py-12 border-t border-gray-700">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8 mb-8">
            <div>
              <Link to="/" className="flex items-center gap-2 mb-6 text-white">
                <span className="material-symbols-outlined text-3xl text-primary">memory</span>
                <span className="text-xl font-bold tracking-tight">GearZone</span>
              </Link>
              <p className="text-sm text-gray-400 mb-6 leading-relaxed">
                Vietnam's leading marketplace for PC components and high-end electronics. We connect enthusiasts with the best authentic hardware vendors.
              </p>
              <div className="flex gap-4">
                {['f', 't', 'in'].map(s => (
                  <span key={s} className="w-8 h-8 rounded-full bg-gray-700 flex items-center justify-center text-white/80 text-xs font-bold cursor-default">{s}</span>
                ))}
              </div>
            </div>

            <div>
              <h4 className="text-white font-bold mb-4">Categories</h4>
              <ul className="space-y-2 text-sm">
                {[['Processors (CPU)', '/products?categorySlug=cpu'], ['Graphics Cards (GPU)', '/products?categorySlug=gpu'], ['Motherboards', '/products?categorySlug=motherboards'], ['Memory (RAM)', '/products?categorySlug=ram'], ['Storage & SSDs', '/products?categorySlug=storage'], ['Power Supplies', '/products?categorySlug=psu']].map(([label, href]) => (
                  <li key={href}><Link to={href} className="hover:text-primary transition-colors">{label}</Link></li>
                ))}
              </ul>
            </div>

            <div>
              <h4 className="text-white font-bold mb-4">Customer Support</h4>
              <ul className="space-y-2 text-sm text-gray-400">
                {['Help Center', 'Warranty Policy', 'Return & Refund', 'Shipping Info'].map(s => <li key={s}>{s}</li>)}
                <li><Link to="/profile?tab=orders" className="hover:text-primary transition-colors">Track Order</Link></li>
              </ul>
            </div>

            <div>
              <h4 className="text-white font-bold mb-4">Contact Us</h4>
              <ul className="space-y-3 text-sm">
                <li className="flex items-start gap-3">
                  <span className="material-symbols-outlined text-gray-500 text-[20px]">location_on</span>
                  <span>123 Tech Street, District 1,<br />Ho Chi Minh City, Vietnam</span>
                </li>
                <li className="flex items-center gap-3">
                  <span className="material-symbols-outlined text-gray-500 text-[20px]">phone</span>
                  1900 123 456
                </li>
                <li className="flex items-center gap-3">
                  <span className="material-symbols-outlined text-gray-500 text-[20px]">mail</span>
                  support@gearzone.vn
                </li>
              </ul>
            </div>
          </div>

          <div className="border-t border-gray-700 pt-8 flex flex-col md:flex-row justify-between items-center gap-4 text-xs text-gray-500">
            <p>© 2024 GearZone Vietnam. All rights reserved.</p>
            <div className="flex items-center gap-4">
              <span>Privacy Policy</span>
              <span>Terms of Service</span>
              <Link to="/products" className="hover:text-primary transition-colors">Sitemap</Link>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
