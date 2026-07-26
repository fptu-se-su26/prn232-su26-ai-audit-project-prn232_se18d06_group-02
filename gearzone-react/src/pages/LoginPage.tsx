import { useState, FormEvent } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import { authApi } from '../api/auth';

export default function LoginPage() {
  const { login, refresh } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: { pathname?: string } })?.from?.pathname || '/';
  const [isRegister, setIsRegister] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [regPassword, setRegPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const handleLogin = async (e: FormEvent) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      await login(username, password, rememberMe);
      await refresh();
      navigate(from, { replace: true });
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Login failed.');
    } finally { setLoading(false); }
  };

  const handleRegister = async (e: FormEvent) => {
    e.preventDefault();
    setError(''); setLoading(true);
    try {
      await authApi.register(fullName, email, regPassword, confirmPassword);
      setSuccess('Registration successful! Please check your email to verify your account.');
      setIsRegister(false);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Registration failed.');
    } finally { setLoading(false); }
  };

  const inputCls = "w-full pl-12 pr-5 py-4 bg-slate-50 border-transparent rounded-2xl text-slate-700 text-sm focus:outline-none focus:bg-white focus:ring-4 focus:ring-blue-100 focus:border-primary transition-all duration-300";
  const btnPrimary = "w-full py-4 px-6 bg-primary hover:bg-blue-700 text-white font-bold rounded-2xl transition-all duration-300 shadow-[0_12px_24px_-8px_rgba(26,87,219,0.4)] active:scale-[0.98] text-sm";
  const btnOutline = "w-full py-4 px-6 border-2 border-slate-100 hover:border-blue-50 hover:bg-blue-50 text-slate-600 hover:text-primary font-bold rounded-2xl transition-all duration-300 text-sm flex items-center justify-center gap-3";

  return (
    <div
      className="min-h-screen flex items-center justify-center p-6 relative"
      style={{
        backgroundImage: "url('https://images.unsplash.com/photo-1714310289917-3b5c20dbe1af?w=1920&q=80')",
        backgroundSize: 'cover',
        backgroundPosition: 'center',
        backgroundAttachment: 'fixed',
      }}
    >
      {/* Backdrop blur overlay */}
      <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-[10px] z-0" />

      {/* Auth card */}
      <div
        className="relative z-10 overflow-hidden bg-white/95 backdrop-blur-xl"
        style={{
          width: 1000,
          maxWidth: '95vw',
          minHeight: 650,
          borderRadius: '2.5rem',
          boxShadow: '0 32px 64px -16px rgba(0,0,0,0.3)',
        }}
      >
        {/* Sign Up form */}
        <div
          className="absolute top-0 left-0 w-1/2 h-full px-12 py-10 flex flex-col justify-center"
          style={{
            zIndex: isRegister ? 5 : 1,
            opacity: isRegister ? 1 : 0,
            transform: isRegister ? 'translateX(100%)' : 'translateX(0)',
            transition: 'all 0.7s ease-in-out',
            pointerEvents: isRegister ? 'auto' : 'none',
          }}
        >
          <div className="w-full max-w-md mx-auto">
            <div className="mb-8">
              <h2 className="text-3xl font-extrabold tracking-tight text-slate-900">Create Account</h2>
              <p className="text-slate-500 mt-2 text-sm">Join GearZone and start trading gear today.</p>
            </div>
            {error && isRegister && <div className="text-red-500 text-xs bg-red-50 p-4 rounded-xl border border-red-100 mb-6 font-medium">{error}</div>}
            <form onSubmit={handleRegister} className="space-y-4">
              <div className="relative">
                <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 text-[20px]">person</span>
                <input placeholder="Full Name" value={fullName} onChange={e => setFullName(e.target.value)} required className={inputCls} />
              </div>
              <div className="relative">
                <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 text-[20px]">mail</span>
                <input type="email" placeholder="Email Address" value={email} onChange={e => setEmail(e.target.value)} required className={inputCls} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <input type="password" placeholder="Password" value={regPassword} onChange={e => setRegPassword(e.target.value)} required className="w-full pl-5 pr-5 py-4 bg-slate-50 border-transparent rounded-2xl text-slate-700 text-sm focus:outline-none focus:bg-white focus:ring-4 focus:ring-blue-100 transition-all" />
                <input type="password" placeholder="Confirm" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} required className="w-full pl-5 pr-5 py-4 bg-slate-50 border-transparent rounded-2xl text-slate-700 text-sm focus:outline-none focus:bg-white focus:ring-4 focus:ring-blue-100 transition-all" />
              </div>
              <button type="submit" disabled={loading} className={btnPrimary + ' mt-4'}>{loading ? 'Creating account…' : 'Get Started'}</button>
            </form>
            <div className="relative py-4 mt-2">
              <div className="absolute inset-0 flex items-center"><div className="w-full border-t border-slate-100" /></div>
              <div className="relative flex justify-center"><span className="bg-white px-4 text-[10px] font-bold uppercase tracking-widest text-slate-400">Social Access</span></div>
            </div>
            <button onClick={() => authApi.startGoogleLogin()} className={btnOutline}>
              <img src="https://www.svgrepo.com/show/475656/google-color.svg" className="w-5 h-5" alt="Google" />
              Google Account
            </button>
          </div>
        </div>

        {/* Sign In form */}
        <div
          className="absolute top-0 left-0 w-1/2 h-full px-12 py-10 flex flex-col justify-center"
          style={{
            zIndex: isRegister ? 1 : 2,
            opacity: isRegister ? 0 : 1,
            transform: isRegister ? 'translateX(100%)' : 'translateX(0)',
            transition: 'all 0.7s ease-in-out',
            pointerEvents: isRegister ? 'none' : 'auto',
          }}
        >
          <div className="w-full max-w-md mx-auto">
            <div className="mb-10">
              <h2 className="text-3xl font-extrabold tracking-tight text-slate-900">Welcome Back</h2>
              <p className="text-slate-500 mt-2 text-sm">Manage your inventory and sales dashboard.</p>
            </div>
            {error && !isRegister && <div className="text-red-500 text-xs bg-red-50 p-4 rounded-xl border border-red-100 mb-6 font-medium">{error}</div>}
            {success && <div className="text-green-600 text-xs bg-green-50 p-4 rounded-xl border border-green-100 mb-6 font-medium">{success}</div>}
            <form onSubmit={handleLogin} className="space-y-5">
              <div className="group">
                <label className="block text-[10px] font-bold text-slate-400 uppercase tracking-widest mb-2">Credential ID</label>
                <div className="relative">
                  <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 text-[20px]">alternate_email</span>
                  <input placeholder="Email or Username" value={username} onChange={e => setUsername(e.target.value)} required className={inputCls} />
                </div>
              </div>
              <div className="group">
                <div className="flex justify-between items-center mb-2">
                  <label className="block text-[10px] font-bold text-slate-400 uppercase tracking-widest">Access Key</label>
                  <a href="#" className="text-[10px] font-bold text-primary hover:underline">Forgot?</a>
                </div>
                <div className="relative">
                  <span className="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-slate-300 text-[20px]">lock</span>
                  <input type="password" placeholder="Enter Password" value={password} onChange={e => setPassword(e.target.value)} required className={inputCls} />
                </div>
              </div>
              <label className="flex items-center gap-3 cursor-pointer">
                <input type="checkbox" checked={rememberMe} onChange={e => setRememberMe(e.target.checked)} className="w-5 h-5 rounded-lg border-slate-200 text-primary cursor-pointer" />
                <span className="text-sm font-medium text-slate-500">Stay logged in</span>
              </label>
              <button type="submit" disabled={loading} className={btnPrimary}>{loading ? 'Signing in…' : 'Sign In'}</button>
            </form>
            <div className="relative py-4 mt-2">
              <div className="absolute inset-0 flex items-center"><div className="w-full border-t border-slate-100" /></div>
              <div className="relative flex justify-center"><span className="bg-white px-4 text-[10px] font-bold uppercase tracking-widest text-slate-400">Instant Access</span></div>
            </div>
            <button onClick={() => authApi.startGoogleLogin()} className={btnOutline}>
              <img src="https://www.svgrepo.com/show/475656/google-color.svg" className="w-5 h-5" alt="Google" />
              Google Login
            </button>
          </div>
        </div>

        {/* Sliding overlay */}
        <div
          className="absolute top-0 left-1/2 w-1/2 h-full overflow-hidden z-[100]"
          style={{ transform: isRegister ? 'translateX(-100%)' : 'translateX(0)', transition: 'all 0.7s ease-in-out' }}
        >
          <div
            className="relative h-full text-white"
            style={{
              left: '-100%',
              width: '200%',
              background: 'linear-gradient(135deg, #0f172a 0%, #1e40af 100%)',
              transform: isRegister ? 'translateX(50%)' : 'translateX(0)',
              transition: 'all 0.7s ease-in-out',
            }}
          >
            {/* Left panel (go to sign-in) */}
            <div
              className="absolute top-0 left-0 w-1/2 h-full flex flex-col items-center justify-center px-12 text-center"
              style={{ transform: isRegister ? 'translateX(0)' : 'translateX(-20%)', transition: 'all 0.7s ease-in-out' }}
            >
              <h1 className="text-4xl font-extrabold mb-4 tracking-tight">Returning?</h1>
              <p className="text-blue-100 mb-8 font-medium leading-relaxed opacity-90 text-sm">Log in back to your GearZone dashboard<br />and continue your progress.</p>
              <button onClick={() => setIsRegister(false)} className="px-10 py-3 rounded-xl border-2 border-white/30 text-white font-bold uppercase tracking-widest text-[10px] hover:bg-white hover:text-primary transition-all duration-300">
                Sign In Instead
              </button>
              <div className="absolute bottom-12">
                <Link to="/" className="text-2xl font-black tracking-tighter uppercase">GearZone</Link>
              </div>
            </div>
            {/* Right panel (go to sign-up) */}
            <div
              className="absolute top-0 right-0 w-1/2 h-full flex flex-col items-center justify-center px-12 text-center"
              style={{ transform: isRegister ? 'translateX(20%)' : 'translateX(0)', transition: 'all 0.7s ease-in-out' }}
            >
              <h1 className="text-4xl font-extrabold mb-4 tracking-tight">New Hero?</h1>
              <p className="text-blue-100 mb-8 font-medium leading-relaxed opacity-90 text-sm">Register today and start selling gear<br />on our premium platform.</p>
              <button onClick={() => setIsRegister(true)} className="px-10 py-3 rounded-xl border-2 border-white/30 text-white font-bold uppercase tracking-widest text-[10px] hover:bg-white hover:text-primary transition-all duration-300">
                Create Account
              </button>
              <div className="absolute bottom-12">
                <Link to="/" className="text-2xl font-black tracking-tighter uppercase">GearZone</Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
