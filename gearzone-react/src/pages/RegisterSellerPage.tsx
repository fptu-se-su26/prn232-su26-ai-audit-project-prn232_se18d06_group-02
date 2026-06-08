import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { sellerApi } from '../api/seller';

interface RegistrationProgress {
  currentStep: number;
  status: string;
  rejectionReason?: string;
  step1Data?: Record<string, unknown>;
  step2Data?: Record<string, unknown>;
  step3Data?: Record<string, unknown>;
}

const STEPS = [
  { label: 'Store Info', icon: 'storefront' },
  { label: 'Business', icon: 'business' },
  { label: 'Banking', icon: 'account_balance' },
];

const inputCls = "w-full border border-gray-300 rounded-xl px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all";

export default function RegisterSellerPage() {
  const navigate = useNavigate();
  const [progress, setProgress] = useState<RegistrationProgress | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const [storeName, setStoreName] = useState('');
  const [storeDesc, setStoreDesc] = useState('');
  const [businessName, setBusinessName] = useState('');
  const [taxId, setTaxId] = useState('');
  const [bankName, setBankName] = useState('');
  const [bankAccount, setBankAccount] = useState('');
  const [bankHolder, setBankHolder] = useState('');

  useEffect(() => {
    sellerApi.registration.getProgress()
      .then(d => setProgress(d as RegistrationProgress))
      .finally(() => setLoading(false));
  }, []);

  const refresh = async () => {
    const p = await sellerApi.registration.getProgress();
    setProgress(p as RegistrationProgress);
  };

  const handleStep1 = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try { await sellerApi.registration.submitStep1({ storeName, description: storeDesc }); await refresh(); }
    catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  const handleStep2 = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try { await sellerApi.registration.submitStep2({ businessName, taxId }); await refresh(); }
    catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  const handleStep3 = async (e: React.FormEvent) => {
    e.preventDefault(); setSaving(true); setError('');
    try { await sellerApi.registration.submitStep3({ bankName, bankAccount, bankHolder }); await refresh(); }
    catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  const handleSubmit = async () => {
    setSaving(true); setError('');
    try { await sellerApi.registration.submit(); await refresh(); }
    catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  const handleReapply = async () => {
    setSaving(true); setError('');
    try { await sellerApi.registration.reapply(); await refresh(); }
    catch (err: unknown) { setError(err instanceof Error ? err.message : 'Failed.'); }
    finally { setSaving(false); }
  };

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  );

  if (progress?.status === 'Approved') return (
    <div className="max-w-lg mx-auto px-4 py-20 text-center">
      <div className="w-24 h-24 rounded-full bg-green-50 flex items-center justify-center mx-auto mb-6">
        <span className="material-symbols-outlined text-5xl text-green-500" style={{ fontVariationSettings: "'FILL' 1" }}>check_circle</span>
      </div>
      <h1 className="text-3xl font-extrabold text-gray-900 mb-2">Application Approved!</h1>
      <p className="text-gray-500 mb-8">Your seller account is ready. Start listing products and growing your business.</p>
      <button onClick={() => navigate('/seller/dashboard')}
        className="inline-flex items-center gap-2 bg-green-600 hover:bg-green-700 text-white font-bold px-8 py-4 rounded-xl transition-colors shadow-[0_8px_20px_-6px_rgba(56,161,105,0.5)]">
        <span className="material-symbols-outlined text-[20px]">storefront</span>
        Go to Seller Dashboard
      </button>
    </div>
  );

  if (progress?.status === 'PendingReview') return (
    <div className="max-w-lg mx-auto px-4 py-20 text-center">
      <div className="w-24 h-24 rounded-full bg-blue-50 flex items-center justify-center mx-auto mb-6">
        <span className="material-symbols-outlined text-5xl text-primary" style={{ fontVariationSettings: "'FILL' 1" }}>pending</span>
      </div>
      <h1 className="text-3xl font-extrabold text-gray-900 mb-2">Under Review</h1>
      <p className="text-gray-500 max-w-sm mx-auto">Your application is being reviewed by our team. We'll notify you via email once a decision is made.</p>
    </div>
  );

  if (progress?.status === 'Rejected') return (
    <div className="max-w-lg mx-auto px-4 py-20 text-center">
      <div className="w-24 h-24 rounded-full bg-red-50 flex items-center justify-center mx-auto mb-6">
        <span className="material-symbols-outlined text-5xl text-red-500" style={{ fontVariationSettings: "'FILL' 1" }}>cancel</span>
      </div>
      <h1 className="text-3xl font-extrabold text-gray-900 mb-2">Application Rejected</h1>
      {progress.rejectionReason && (
        <div className="bg-red-50 border border-red-200 rounded-xl px-5 py-4 text-sm text-red-700 text-left mb-6 max-w-sm mx-auto">
          <p className="font-semibold mb-1">Reason</p>
          <p>{progress.rejectionReason}</p>
        </div>
      )}
      <p className="text-gray-500 mb-8">You may update your information and reapply.</p>
      {error && <p className="text-red-500 text-sm mb-4">{error}</p>}
      <button onClick={handleReapply} disabled={saving}
        className="inline-flex items-center gap-2 bg-primary hover:bg-blue-700 disabled:bg-gray-300 text-white font-bold px-8 py-4 rounded-xl transition-colors">
        <span className="material-symbols-outlined text-[20px]">refresh</span>
        {saving ? 'Processing…' : 'Reapply'}
      </button>
    </div>
  );

  const step = progress?.currentStep ?? 1;

  return (
    <div className="max-w-xl mx-auto px-4 sm:px-6 py-10">
      {/* Page heading */}
      <div className="text-center mb-8">
        <h1 className="text-3xl font-extrabold text-gray-900">Become a Seller</h1>
        <p className="text-gray-500 mt-2">Complete 3 steps to start selling on GearZone</p>
      </div>

      {/* Step progress */}
      <div className="flex items-center mb-8">
        {STEPS.map((s, i) => {
          const n = i + 1;
          const done = n < step;
          const active = n === step;
          return (
            <div key={n} className="flex items-center flex-1 last:flex-none">
              <div className="flex flex-col items-center">
                <div className={`w-10 h-10 rounded-full flex items-center justify-center font-bold text-sm transition-all
                  ${done ? 'bg-green-500 text-white' : active ? 'bg-primary text-white ring-4 ring-primary/20' : 'bg-gray-100 text-gray-400'}`}>
                  {done
                    ? <span className="material-symbols-outlined text-[18px]">check</span>
                    : <span className="material-symbols-outlined text-[18px]">{s.icon}</span>
                  }
                </div>
                <span className={`text-xs font-semibold mt-1.5 ${done ? 'text-green-600' : active ? 'text-primary' : 'text-gray-400'}`}>
                  {s.label}
                </span>
              </div>
              {i < STEPS.length - 1 && (
                <div className={`flex-1 h-0.5 mx-2 mb-5 transition-colors ${done ? 'bg-green-400' : 'bg-gray-200'}`} />
              )}
            </div>
          );
        })}
      </div>

      {/* Form card */}
      <div className="bg-white rounded-2xl border border-gray-200 overflow-hidden">
        {/* Card header */}
        <div className="px-6 py-5 border-b border-gray-100 bg-gray-50">
          <div className="flex items-center gap-3">
            <div className="w-9 h-9 rounded-xl bg-primary/10 flex items-center justify-center">
              <span className="material-symbols-outlined text-primary text-[18px]">{STEPS[(step <= 3 ? step : 3) - 1]?.icon}</span>
            </div>
            <div>
              <h2 className="font-bold text-gray-900">
                {step === 1 ? 'Store Information' : step === 2 ? 'Business Information' : step === 3 ? 'Banking Information' : 'Ready to Submit'}
              </h2>
              <p className="text-xs text-gray-500">Step {Math.min(step, 3)} of 3</p>
            </div>
          </div>
        </div>

        <div className="px-6 py-6">
          {error && (
            <div className="flex items-center gap-2 bg-red-50 text-red-600 px-4 py-3 rounded-xl text-sm mb-5">
              <span className="material-symbols-outlined text-[18px]">error</span>
              {error}
            </div>
          )}

          {step === 1 && (
            <form onSubmit={handleStep1} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1.5">Store Name <span className="text-red-400">*</span></label>
                <input placeholder="e.g. TechGear Store" value={storeName} onChange={e => setStoreName(e.target.value)} required className={inputCls} />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1.5">Store Description</label>
                <textarea placeholder="Describe your store and what you sell…" value={storeDesc} onChange={e => setStoreDesc(e.target.value)}
                  rows={4} className={`${inputCls} resize-none`} />
              </div>
              <button type="submit" disabled={saving}
                className="w-full flex items-center justify-center gap-2 bg-primary hover:bg-blue-700 disabled:bg-gray-300 text-white font-bold py-3.5 rounded-xl transition-all text-sm">
                {saving ? 'Saving…' : <>Continue <span className="material-symbols-outlined text-[18px]">arrow_forward</span></>}
              </button>
            </form>
          )}

          {step === 2 && (
            <form onSubmit={handleStep2} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1.5">Business / Company Name <span className="text-red-400">*</span></label>
                <input placeholder="Legal business name" value={businessName} onChange={e => setBusinessName(e.target.value)} required className={inputCls} />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1.5">Tax ID <span className="text-gray-400 font-normal">(optional)</span></label>
                <input placeholder="Business tax identification number" value={taxId} onChange={e => setTaxId(e.target.value)} className={inputCls} />
              </div>
              <button type="submit" disabled={saving}
                className="w-full flex items-center justify-center gap-2 bg-primary hover:bg-blue-700 disabled:bg-gray-300 text-white font-bold py-3.5 rounded-xl transition-all text-sm">
                {saving ? 'Saving…' : <>Continue <span className="material-symbols-outlined text-[18px]">arrow_forward</span></>}
              </button>
            </form>
          )}

          {step === 3 && (
            <form onSubmit={handleStep3} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1.5">Bank Name <span className="text-red-400">*</span></label>
                <input placeholder="e.g. Vietcombank, Techcombank" value={bankName} onChange={e => setBankName(e.target.value)} required className={inputCls} />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1.5">Account Number <span className="text-red-400">*</span></label>
                <input placeholder="Bank account number" value={bankAccount} onChange={e => setBankAccount(e.target.value)} required className={inputCls} />
              </div>
              <div>
                <label className="block text-xs font-semibold text-gray-500 mb-1.5">Account Holder Name <span className="text-red-400">*</span></label>
                <input placeholder="Name exactly as on the bank account" value={bankHolder} onChange={e => setBankHolder(e.target.value)} required className={inputCls} />
              </div>
              <button type="submit" disabled={saving}
                className="w-full flex items-center justify-center gap-2 bg-primary hover:bg-blue-700 disabled:bg-gray-300 text-white font-bold py-3.5 rounded-xl transition-all text-sm">
                {saving ? 'Saving…' : <>Save Banking Info <span className="material-symbols-outlined text-[18px]">arrow_forward</span></>}
              </button>
            </form>
          )}

          {step === 4 && (
            <div className="text-center py-4">
              <div className="w-16 h-16 rounded-full bg-green-50 flex items-center justify-center mx-auto mb-4">
                <span className="material-symbols-outlined text-4xl text-green-500" style={{ fontVariationSettings: "'FILL' 1" }}>task_alt</span>
              </div>
              <p className="font-bold text-gray-900 mb-1">All Steps Completed!</p>
              <p className="text-sm text-gray-500 mb-6">Review your information and submit your application for approval.</p>
              <button onClick={handleSubmit} disabled={saving}
                className="inline-flex items-center gap-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-300 text-white font-bold px-8 py-4 rounded-xl transition-colors shadow-[0_8px_20px_-6px_rgba(56,161,105,0.5)]">
                <span className="material-symbols-outlined text-[20px]">send</span>
                {saving ? 'Submitting…' : 'Submit Application'}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
