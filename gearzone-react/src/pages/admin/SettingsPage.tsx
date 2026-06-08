import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface Settings {
  siteName: string;
  platformFeePercent: number;
  minPayoutAmount: number;
  maintenanceMode: boolean;
  supportEmail?: string;
}

export default function AdminSettingsPage() {
  const [settings, setSettings] = useState<Settings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [success, setSuccess] = useState('');
  const [error, setError] = useState('');

  useEffect(() => {
    adminApi.getSettings()
      .then(d => setSettings(d as Settings))
      .finally(() => setLoading(false));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!settings) return;
    setSaving(true); setError(''); setSuccess('');
    try {
      await adminApi.updateSettings(settings);
      setSuccess('Settings saved successfully!');
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to save settings.');
    } finally { setSaving(false); }
  };

  if (loading) return <div style={{ padding: '2rem' }}>Loading…</div>;
  if (!settings) return <div style={{ padding: '2rem' }}>Failed to load settings.</div>;

  return (
    <div style={{ padding: '2rem', maxWidth: 600 }}>
      <h1>Platform Settings</h1>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Site Name</label>
          <input value={settings.siteName} onChange={e => setSettings(s => s ? { ...s, siteName: e.target.value } : s)}
            style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Platform Fee (%)</label>
          <input type="number" step="0.1" min="0" max="100" value={settings.platformFeePercent}
            onChange={e => setSettings(s => s ? { ...s, platformFeePercent: Number(e.target.value) } : s)}
            style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Minimum Payout Amount (VND)</label>
          <input type="number" value={settings.minPayoutAmount}
            onChange={e => setSettings(s => s ? { ...s, minPayoutAmount: Number(e.target.value) } : s)}
            style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ display: 'block', fontWeight: 600, marginBottom: '0.3rem' }}>Support Email</label>
          <input type="email" value={settings.supportEmail ?? ''}
            onChange={e => setSettings(s => s ? { ...s, supportEmail: e.target.value } : s)}
            style={{ width: '100%', padding: '0.5rem', boxSizing: 'border-box' }} />
        </div>
        <div>
          <label style={{ cursor: 'pointer' }}>
            <input type="checkbox" checked={settings.maintenanceMode}
              onChange={e => setSettings(s => s ? { ...s, maintenanceMode: e.target.checked } : s)}
              style={{ marginRight: '0.5rem' }} />
            <strong>Maintenance Mode</strong>
          </label>
          <p style={{ margin: '0.25rem 0 0', color: '#888', fontSize: 13 }}>Enabling this will show a maintenance page to all visitors.</p>
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
