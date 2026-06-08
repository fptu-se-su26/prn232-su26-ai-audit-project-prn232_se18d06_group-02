import { useEffect, useState } from 'react';
import { adminApi } from '../../api/admin';

interface User {
  id: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  avatarUrl?: string;
  role: string;
  isEmailVerified: boolean;
  isActive?: boolean;
  isLocked?: boolean;
  isDeleted?: boolean;
  createdAt: string;
}

const ROLES = ['Customer', 'Store Owner', 'Super Admin'];

export default function AdminUsersPage() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [roleFilter, setRoleFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [processing, setProcessing] = useState<string | null>(null);
  const [showAddModal, setShowAddModal] = useState(false);
  const [addForm, setAddForm] = useState({ fullName: '', email: '', phoneNumber: '', password: '', role: 'Customer' });
  const [addSaving, setAddSaving] = useState(false);
  const [addError, setAddError] = useState('');
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  const fetchUsers = (q = search, r = roleFilter, s = statusFilter, p = page) => {
    setLoading(true);
    adminApi.users.list({ search: q || undefined, role: r || undefined, isActive: s || undefined, pageNumber: p, pageSize: 20 })
      .then(d => {
        const raw = d as { items?: User[]; totalPages?: number } | User[];
        if (raw && typeof raw === 'object' && 'items' in raw) {
          setUsers((raw as { items?: User[] }).items ?? []);
          setTotalPages((raw as { totalPages?: number }).totalPages ?? 1);
        } else {
          setUsers((raw as User[]) ?? []);
        }
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => { fetchUsers(); }, []);

  const handleLock = async (id: string, lock: boolean) => {
    setProcessing(id);
    try {
      await (lock ? adminApi.users.lock(id) : adminApi.users.unlock(id));
      fetchUsers();
    } finally { setProcessing(null); }
  };

  const handleRoleChange = async (id: string, role: string) => {
    setProcessing(id);
    try {
      await adminApi.users.changeRole(id, role);
      fetchUsers();
    } finally { setProcessing(null); }
  };

  const handleAddUser = async (e: React.FormEvent) => {
    e.preventDefault();
    setAddSaving(true); setAddError('');
    try {
      await adminApi.users.create(addForm);
      setShowAddModal(false);
      setAddForm({ fullName: '', email: '', phoneNumber: '', password: '', role: 'Customer' });
      fetchUsers();
    } catch (err) {
      setAddError(err instanceof Error ? err.message : 'Failed to create user.');
    } finally { setAddSaving(false); }
  };

  const stats = [
    { label: 'Total Users', value: users.length, color: '#3182ce' },
    { label: 'Active', value: users.filter(u => !u.isLocked && !u.isDeleted).length, color: '#38a169' },
    { label: 'Customers', value: users.filter(u => u.role === 'Customer').length, color: '#d69e2e' },
    { label: 'Store Owners', value: users.filter(u => u.role === 'Store Owner').length, color: '#805ad5' },
  ];

  return (
    <div style={{ padding: '2rem' }}>
      {/* Add User Modal */}
      {showAddModal && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000 }}>
          <div style={{ background: '#fff', padding: '2rem', borderRadius: 8, maxWidth: 480, width: '90%' }}>
            <h3 style={{ marginTop: 0 }}>Add New User</h3>
            <form onSubmit={handleAddUser} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              <input placeholder="Full Name" value={addForm.fullName} onChange={e => setAddForm(f => ({ ...f, fullName: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
              <input type="email" placeholder="Email" value={addForm.email} onChange={e => setAddForm(f => ({ ...f, email: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
              <input placeholder="Phone (optional)" value={addForm.phoneNumber} onChange={e => setAddForm(f => ({ ...f, phoneNumber: e.target.value }))} style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
              <input type="password" placeholder="Password" value={addForm.password} onChange={e => setAddForm(f => ({ ...f, password: e.target.value }))} required style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }} />
              <select value={addForm.role} onChange={e => setAddForm(f => ({ ...f, role: e.target.value }))} style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc' }}>
                {ROLES.map(r => <option key={r}>{r}</option>)}
              </select>
              {addError && <div style={{ color: '#e53e3e', fontSize: 13 }}>{addError}</div>}
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button type="submit" disabled={addSaving} style={{ flex: 1, padding: '0.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
                  {addSaving ? 'Creating…' : 'Create User'}
                </button>
                <button type="button" onClick={() => { setShowAddModal(false); setAddError(''); }} style={{ flex: 1, padding: '0.5rem', background: '#eee', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.25rem' }}>
        <h1 style={{ margin: 0 }}>User Management</h1>
        <button onClick={() => setShowAddModal(true)}
          style={{ padding: '0.5rem 1.5rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer', fontWeight: 600 }}>
          + Add New User
        </button>
      </div>

      {/* Stats */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        {stats.map(s => (
          <div key={s.label} style={{ background: '#fff', border: '1px solid #e2e8f0', borderRadius: 8, padding: '1rem' }}>
            <div style={{ fontSize: 12, color: '#888', marginBottom: '0.3rem' }}>{s.label}</div>
            <div style={{ fontWeight: 700, fontSize: 20, color: s.color }}>{s.value}</div>
          </div>
        ))}
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '0.75rem', marginBottom: '1.25rem', flexWrap: 'wrap', alignItems: 'flex-end' }}>
        <input placeholder="Search by name or email…" value={search} onChange={e => setSearch(e.target.value)}
          onKeyDown={e => e.key === 'Enter' && fetchUsers(search)}
          style={{ padding: '0.5rem', flex: '1 1 220px', borderRadius: 4, border: '1px solid #ccc' }} />
        <select value={roleFilter} onChange={e => setRoleFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 140 }}>
          <option value="">All Roles</option>
          {ROLES.map(r => <option key={r}>{r}</option>)}
        </select>
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}
          style={{ padding: '0.5rem', borderRadius: 4, border: '1px solid #ccc', minWidth: 120 }}>
          <option value="">All Status</option>
          <option value="true">Active</option>
          <option value="false">Inactive</option>
        </select>
        <button onClick={() => { setPage(1); fetchUsers(search, roleFilter, statusFilter, 1); }}
          style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
          Filter
        </button>
      </div>

      {loading ? <div style={{ textAlign: 'center', padding: '2rem' }}>Loading…</div> : (
        <>
          <table style={{ width: '100%', borderCollapse: 'collapse' }}>
            <thead>
              <tr style={{ background: '#f0f0f0' }}>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>User</th>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>Role</th>
                <th style={{ padding: '0.75rem', textAlign: 'center' }}>Verified</th>
                <th style={{ padding: '0.75rem', textAlign: 'center' }}>Status</th>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>Phone</th>
                <th style={{ padding: '0.75rem', textAlign: 'left' }}>Joined</th>
                <th style={{ padding: '0.75rem', textAlign: 'center' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map(u => {
                const initials = (u.fullName || '?').charAt(0).toUpperCase();
                const isActive = !u.isLocked && !u.isDeleted;
                return (
                  <tr key={u.id} style={{ borderBottom: '1px solid #eee' }}>
                    <td style={{ padding: '0.75rem' }}>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                        <div style={{ position: 'relative', flexShrink: 0 }}>
                          {u.avatarUrl
                            ? <img src={u.avatarUrl} alt={u.fullName} style={{ width: 36, height: 36, borderRadius: '50%', objectFit: 'cover' }} />
                            : <div style={{ width: 36, height: 36, borderRadius: '50%', background: '#ebf4ff', color: '#3182ce', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700, fontSize: 14 }}>{initials}</div>
                          }
                          <span style={{ position: 'absolute', bottom: 0, right: 0, width: 10, height: 10, borderRadius: '50%', background: isActive ? '#38a169' : '#a0aec0', border: '2px solid #fff' }} />
                        </div>
                        <div>
                          <div style={{ fontWeight: 500 }}>{u.fullName}</div>
                          <div style={{ fontSize: 12, color: '#888' }}>{u.email}</div>
                        </div>
                      </div>
                    </td>
                    <td style={{ padding: '0.75rem' }}>
                      <select value={u.role} onChange={e => handleRoleChange(u.id, e.target.value)} disabled={processing === u.id}
                        style={{ padding: '0.25rem 0.5rem', fontSize: 13, borderRadius: 4, border: '1px solid #ccc' }}>
                        {ROLES.map(r => <option key={r}>{r}</option>)}
                      </select>
                    </td>
                    <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                      <span style={{ color: u.isEmailVerified ? '#38a169' : '#e53e3e' }}>{u.isEmailVerified ? '✓' : '✗'}</span>
                    </td>
                    <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                      {u.isDeleted
                        ? <span style={{ padding: '0.15rem 0.5rem', borderRadius: 12, background: '#fff5f5', color: '#e53e3e', fontWeight: 600, fontSize: 12 }}>Deleted</span>
                        : u.isLocked
                          ? <span style={{ padding: '0.15rem 0.5rem', borderRadius: 12, background: '#fff5f5', color: '#e53e3e', fontWeight: 600, fontSize: 12 }}>Locked</span>
                          : <span style={{ padding: '0.15rem 0.5rem', borderRadius: 12, background: '#f0fff4', color: '#38a169', fontWeight: 600, fontSize: 12 }}>Active</span>
                      }
                    </td>
                    <td style={{ padding: '0.75rem', fontSize: 13, color: '#555' }}>{u.phoneNumber || '—'}</td>
                    <td style={{ padding: '0.75rem', fontSize: 13, color: '#888' }}>{new Date(u.createdAt).toLocaleDateString()}</td>
                    <td style={{ padding: '0.75rem', textAlign: 'center' }}>
                      {!u.isDeleted && (
                        <button onClick={() => handleLock(u.id, !u.isLocked)} disabled={processing === u.id}
                          style={{ padding: '0.2rem 0.6rem', background: u.isLocked ? '#c6f6d5' : '#fed7d7', border: 'none', borderRadius: 4, cursor: 'pointer', color: u.isLocked ? '#276749' : '#c53030', fontSize: 12 }}>
                          {processing === u.id ? '…' : u.isLocked ? 'Unlock' : 'Lock'}
                        </button>
                      )}
                    </td>
                  </tr>
                );
              })}
              {users.length === 0 && (
                <tr><td colSpan={7} style={{ padding: '2rem', textAlign: 'center', color: '#888' }}>No users found.</td></tr>
              )}
            </tbody>
          </table>

          {totalPages > 1 && (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '0.5rem', marginTop: '1.25rem' }}>
              <button onClick={() => { const p = Math.max(1, page - 1); setPage(p); fetchUsers(search, roleFilter, statusFilter, p); }}
                disabled={page <= 1}
                style={{ padding: '0.4rem 0.75rem', borderRadius: 4, border: '1px solid #e2e8f0', background: '#fff', cursor: page <= 1 ? 'default' : 'pointer', opacity: page <= 1 ? 0.5 : 1 }}>
                ‹
              </button>
              <span style={{ padding: '0.4rem 0.75rem', fontSize: 13, color: '#555' }}>Page {page} of {totalPages}</span>
              <button onClick={() => { const p = Math.min(totalPages, page + 1); setPage(p); fetchUsers(search, roleFilter, statusFilter, p); }}
                disabled={page >= totalPages}
                style={{ padding: '0.4rem 0.75rem', borderRadius: 4, border: '1px solid #e2e8f0', background: '#fff', cursor: page >= totalPages ? 'default' : 'pointer', opacity: page >= totalPages ? 0.5 : 1 }}>
                ›
              </button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
