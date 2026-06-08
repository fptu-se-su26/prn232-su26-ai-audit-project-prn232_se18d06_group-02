import { useEffect, useState } from 'react';
import { sellerApi } from '../../api/seller';

interface StoreSettings {
  name: string;
  description?: string;
  logoUrl?: string;
  bannerUrl?: string;
  contactEmail?: string;
  contactPhone?: string;
  returnPolicy?: string;
  shippingPolicy?: string;
}

export default function SellerSettingsPage() {
  const [settings, setSettings] = useState<StoreSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    sellerApi.getStoreSettings()
      .then(d => setSettings(d as StoreSettings))
      .finally(() => setLoading(false));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!settings) return;
    setSaving(true); setError(''); setSuccess('');
    try {
      await sellerApi.updateStoreSettings(settings);
      setSuccess('Settings saved successfully!');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save settings.');
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (field: keyof StoreSettings, value: string) => {
    setSettings(s => s ? { ...s, [field]: value } : s);
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!settings) return <div style={{ padding: '2rem' }}>Failed to load settings.</div>;

  return (
    <div style={{ padding: '2rem', maxWidth: 600 }}>
      <h1>Store Settings</h1>

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Store Name</label>
          <input value={settings.name} onChange={e => handleChange('name', e.target.value)} required style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Description</label>
          <textarea value={settings.description ?? ''} onChange={e => handleChange('description', e.target.value)} rows={3} style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Logo URL</label>
          <input value={settings.logoUrl ?? ''} onChange={e => handleChange('logoUrl', e.target.value)} placeholder="https://…" style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
          {settings.logoUrl && <img src={settings.logoUrl} alt="logo preview" style={{ marginTop: '0.5rem', height: 60, objectFit: 'contain', border: '1px solid #e2e8f0', borderRadius: 4 }} />}
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Banner URL</label>
          <input value={settings.bannerUrl ?? ''} onChange={e => handleChange('bannerUrl', e.target.value)} placeholder="https://…" style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
          {settings.bannerUrl && <img src={settings.bannerUrl} alt="banner preview" style={{ marginTop: '0.5rem', width: '100%', maxHeight: 120, objectFit: 'cover', border: '1px solid #e2e8f0', borderRadius: 4 }} />}
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Contact Email</label>
          <input type="email" value={settings.contactEmail ?? ''} onChange={e => handleChange('contactEmail', e.target.value)} style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Contact Phone</label>
          <input value={settings.contactPhone ?? ''} onChange={e => handleChange('contactPhone', e.target.value)} style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Return Policy</label>
          <textarea value={settings.returnPolicy ?? ''} onChange={e => handleChange('returnPolicy', e.target.value)} rows={4} style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Shipping Policy</label>
          <textarea value={settings.shippingPolicy ?? ''} onChange={e => handleChange('shippingPolicy', e.target.value)} rows={4} style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>

        {error && <p style={{ color: 'red', margin: 0 }}>{error}</p>}
        {success && <p style={{ color: '#38a169', margin: 0 }}>{success}</p>}

        <button type="submit" disabled={saving}
          style={{ padding: '0.75rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          {saving ? 'Saving…' : 'Save Settings'}
        </button>
      </form>
    </div>
  );
}
