/* eslint-disable react-hooks/set-state-in-effect, react-hooks/exhaustive-deps */
import { useEffect, useMemo, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import {
  adminApi,
  type AdminUserDto,
  type AdminUserStatsDto,
  type CreateAdminUserRequest,
  type EditAdminUserRequest,
  type PagedResult,
} from '@/api/admin'
import { AdminLayout } from '@/components/admin/AdminLayout'

const PAGE_SIZE = 10

type ModalType = 'add' | 'edit' | 'delete' | 'restore' | 'detail' | null
type DetailTab = 'profile' | 'access' | 'activity'

const emptyStats: AdminUserStatsDto = {
  activeUsers: 0,
  customerCount: 0,
  storeOwnerCount: 0,
  totalUsers: 0,
}

const emptyCreateForm: CreateAdminUserRequest = {
  confirmPassword: '',
  email: '',
  fullName: '',
  isActive: true,
  password: '',
  phoneNumber: '',
  role: '',
}

function emptyEditForm(user?: AdminUserDto | null): EditAdminUserRequest {
  return {
    email: user?.email ?? '',
    fullName: user?.fullName ?? '',
    id: user?.id ?? '',
    isActive: user?.isActive ?? true,
    phoneNumber: user?.phoneNumber ?? '',
    role: user?.role ?? '',
  }
}

function formatDate(value?: string | null) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', { day: '2-digit', month: 'short', year: 'numeric' }).format(new Date(value))
}

function formatDateTime(value?: string | null) {
  if (!value) return 'N/A'
  return new Intl.DateTimeFormat('en-US', {
    day: '2-digit',
    hour: 'numeric',
    minute: '2-digit',
    month: 'short',
    year: 'numeric',
  }).format(new Date(value))
}

function userInitial(user?: AdminUserDto | null) {
  const name = user?.fullName?.trim() || user?.email?.trim() || '?'
  return name.slice(0, 1).toUpperCase()
}

function displayName(user: AdminUserDto) {
  return user.fullName || 'Unnamed User'
}

function totalPages(users: PagedResult<AdminUserDto> | null) {
  if (!users) return 1
  return users.totalPages || Math.max(1, Math.ceil(users.totalCount / users.pageSize))
}

function StatusBadge({ user }: { user: AdminUserDto }) {
  if (user.isDeleted) {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-red-50 px-2.5 py-1 text-xs font-medium text-red-700 ring-1 ring-inset ring-red-600/20">
        <span className="size-1.5 rounded-full bg-red-600" />
        Deleted
      </span>
    )
  }

  if (user.isActive) {
    return (
      <span className="inline-flex items-center gap-1.5 rounded-full bg-green-50 px-2.5 py-1 text-xs font-medium text-green-700 ring-1 ring-inset ring-green-600/20">
        <span className="size-1.5 rounded-full bg-green-600" />
        Active
      </span>
    )
  }

  return (
    <span className="inline-flex items-center gap-1.5 rounded-full bg-slate-50 px-2.5 py-1 text-xs font-medium text-slate-600 ring-1 ring-inset ring-slate-500/20">
      <span className="size-1.5 rounded-full bg-slate-400" />
      Inactive
    </span>
  )
}

function RoleBadge({ role }: { role?: string | null }) {
  if (!role) return <span className="text-xs italic text-slate-400">No role</span>

  return (
    <span className="inline-flex items-center rounded-lg bg-blue-50 px-2.5 py-1 text-xs font-bold uppercase text-blue-700 ring-1 ring-inset ring-blue-700/10">
      {role}
    </span>
  )
}

function Avatar({ user, size = 'size-10' }: { user?: AdminUserDto | null; size?: string }) {
  if (user?.avatarUrl) {
    return <img src={user.avatarUrl} alt={displayName(user)} className={`${size} rounded-full object-cover shadow-sm`} />
  }

  return (
    <div className={`${size} flex items-center justify-center rounded-full bg-primary/10 text-sm font-bold text-primary shadow-sm`}>
      {userInitial(user)}
    </div>
  )
}

function StatCard({ icon, label, tone, value }: { icon: string; label: string; tone: string; value: number }) {
  return (
    <div className="flex items-center gap-4 rounded-xl border border-slate-100 bg-white p-4 shadow-sm">
      <div className={`flex size-12 items-center justify-center rounded-lg ${tone}`}>
        <span className="material-symbols-outlined">{icon}</span>
      </div>
      <div>
        <p className="text-xs font-medium uppercase tracking-wider text-slate-500">{label}</p>
        <h3 className="text-2xl font-bold text-slate-900">{value}</h3>
      </div>
    </div>
  )
}

function LoadingRows() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, index) => (
        <tr key={index} className="animate-pulse">
          <td colSpan={7} className="px-6 py-4">
            <div className="h-12 rounded-lg bg-slate-100" />
          </td>
        </tr>
      ))}
    </>
  )
}

function ModalShell({
  children,
  maxWidth = 'max-w-xl',
  onClose,
}: {
  children: ReactNode
  maxWidth?: string
  onClose: () => void
}) {
  return (
    <div className="fixed inset-0 z-[60]">
      <button type="button" aria-label="Close modal" className="absolute inset-0 bg-slate-900/40 backdrop-blur-sm" onClick={onClose} />
      <div className="absolute inset-0 flex items-center justify-center p-4">
        <div className={`relative flex max-h-[90vh] w-full ${maxWidth} flex-col overflow-hidden rounded-2xl bg-white shadow-2xl`}>{children}</div>
      </div>
    </div>
  )
}

function TextField({
  label,
  onChange,
  required,
  type = 'text',
  value,
}: {
  label: string
  onChange: (value: string) => void
  required?: boolean
  type?: string
  value: string
}) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-xs font-bold uppercase tracking-wider text-slate-500">{label}</span>
      <input
        required={required}
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="h-11 w-full rounded-lg border border-slate-200 bg-slate-50 px-3 text-sm text-slate-900 outline-none transition focus:border-primary focus:bg-white focus:ring-2 focus:ring-primary/20"
      />
    </label>
  )
}

function UserForm({
  form,
  mode,
  onCancel,
  onChange,
  onSubmit,
  roles,
  saving,
}: {
  form: CreateAdminUserRequest | EditAdminUserRequest
  mode: 'create' | 'edit'
  onCancel: () => void
  onChange: (patch: Partial<CreateAdminUserRequest & EditAdminUserRequest>) => void
  onSubmit: (event: FormEvent<HTMLFormElement>) => void
  roles: string[]
  saving: boolean
}) {
  const isCreate = mode === 'create'
  const createForm = form as CreateAdminUserRequest

  return (
    <form onSubmit={onSubmit} className="flex flex-1 flex-col overflow-hidden">
      <div className="flex items-center justify-between border-b border-slate-100 px-6 py-4">
        <h3 className="text-xl font-bold text-slate-900">{isCreate ? 'Add New User' : 'Edit User'}</h3>
        <button type="button" onClick={onCancel} className="rounded-full p-2 text-slate-400 transition hover:bg-slate-100 hover:text-slate-600">
          <span className="material-symbols-outlined">close</span>
        </button>
      </div>

      <div className="flex-1 space-y-8 overflow-y-auto p-6">
        <section className="space-y-4">
          <div className="border-b border-slate-100 pb-1">
            <span className="text-[10px] font-bold uppercase tracking-widest text-slate-400">Basic Information</span>
          </div>
          <div className="grid gap-4 sm:grid-cols-2">
            <TextField label="Full Name" required value={form.fullName} onChange={(value) => onChange({ fullName: value })} />
            <TextField label="Email Address" required={isCreate} type="email" value={form.email ?? ''} onChange={(value) => onChange({ email: value })} />
            <TextField label="Phone Number" value={form.phoneNumber ?? ''} onChange={(value) => onChange({ phoneNumber: value })} />
            <label className="block">
              <span className="mb-1.5 block text-xs font-bold uppercase tracking-wider text-slate-500">Role</span>
              <select
                required
                value={form.role}
                onChange={(event) => onChange({ role: event.target.value })}
                className="h-11 w-full rounded-lg border border-slate-200 bg-slate-50 px-3 text-sm text-slate-900 outline-none transition focus:border-primary focus:bg-white focus:ring-2 focus:ring-primary/20"
              >
                <option value="">Select Role</option>
                {roles.map((role) => (
                  <option key={role} value={role}>
                    {role}
                  </option>
                ))}
              </select>
            </label>
          </div>
        </section>

        {isCreate && (
          <section className="space-y-4">
            <div className="border-b border-slate-100 pb-1">
              <span className="text-[10px] font-bold uppercase tracking-widest text-slate-400">Security</span>
            </div>
            <div className="grid gap-4 sm:grid-cols-2">
              <TextField label="Password" required type="password" value={createForm.password} onChange={(value) => onChange({ password: value })} />
              <TextField
                label="Confirm Password"
                required
                type="password"
                value={createForm.confirmPassword}
                onChange={(value) => onChange({ confirmPassword: value })}
              />
            </div>
          </section>
        )}

        <section className="rounded-xl border border-slate-100 bg-slate-50/50 p-4">
          <label className="flex cursor-pointer items-center justify-between gap-4">
            <span>
              <span className="block text-sm font-bold text-slate-800">Account Status</span>
              <span className="mt-0.5 block text-xs text-slate-500">Allow this user to access their account.</span>
            </span>
            <input
              type="checkbox"
              checked={form.isActive}
              onChange={(event) => onChange({ isActive: event.target.checked })}
              className="size-5 rounded border-slate-300 text-primary focus:ring-primary"
            />
          </label>
        </section>
      </div>

      <div className="flex justify-end gap-3 border-t border-slate-100 bg-white px-6 py-4">
        <button type="button" onClick={onCancel} className="rounded-xl border border-slate-200 px-5 py-2 text-sm font-bold text-slate-600 hover:bg-slate-50">
          Cancel
        </button>
        <button type="submit" disabled={saving} className="rounded-xl bg-primary px-6 py-2 text-sm font-bold text-white shadow-md shadow-primary/20 hover:bg-blue-700 disabled:opacity-60">
          {saving ? 'Saving...' : isCreate ? 'Create User' : 'Save Changes'}
        </button>
      </div>
    </form>
  )
}

export default function AdminUsersPage() {
  const [users, setUsers] = useState<PagedResult<AdminUserDto> | null>(null)
  const [stats, setStats] = useState<AdminUserStatsDto>(emptyStats)
  const [roles, setRoles] = useState<string[]>([])
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [role, setRole] = useState('')
  const [isActive, setIsActive] = useState<boolean | ''>('')
  const [pageNumber, setPageNumber] = useState(1)
  const [modal, setModal] = useState<ModalType>(null)
  const [selectedUser, setSelectedUser] = useState<AdminUserDto | null>(null)
  const [detailTab, setDetailTab] = useState<DetailTab>('profile')
  const [createForm, setCreateForm] = useState<CreateAdminUserRequest>(emptyCreateForm)
  const [editForm, setEditForm] = useState<EditAdminUserRequest>(emptyEditForm())

  const pageCount = totalPages(users)

  const rangeText = useMemo(() => {
    const total = users?.totalCount ?? 0
    const page = users?.pageNumber ?? pageNumber
    const pageSize = users?.pageSize ?? PAGE_SIZE
    const start = total === 0 ? 0 : (page - 1) * pageSize + 1
    const end = Math.min(page * pageSize, total)
    return { end, start, total }
  }, [users, pageNumber])

  const loadUsers = async (
    nextPage = pageNumber,
    overrides?: {
      isActive?: boolean | ''
      role?: string
      searchTerm?: string
    },
  ) => {
    setLoading(true)
    setError('')

    const effectiveSearchTerm = overrides?.searchTerm ?? searchTerm
    const effectiveRole = overrides?.role ?? role
    const effectiveIsActive = overrides?.isActive ?? isActive

    try {
      const data = await adminApi.users.list({
        isActive: effectiveIsActive,
        pageNumber: nextPage,
        pageSize: PAGE_SIZE,
        role: effectiveRole || undefined,
        searchTerm: effectiveSearchTerm.trim() || undefined,
      })

      setUsers(data.users)
      setStats(data.stats)
      setRoles(data.roles)
      setPageNumber(data.users.pageNumber)
    } catch (err) {
      setUsers(null)
      setError(err instanceof Error ? err.message : 'Unable to load users.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void loadUsers(1)
  }, [])

  const closeModal = () => {
    if (saving) return
    setModal(null)
    setSelectedUser(null)
    setDetailTab('profile')
  }

  const openDetail = async (user: AdminUserDto) => {
    setError('')
    setSelectedUser(user)
    setModal('detail')
    setDetailTab('profile')

    try {
      const freshUser = await adminApi.users.get(user.id)
      setSelectedUser(freshUser)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to load user details.')
    }
  }

  const openEdit = async (user: AdminUserDto) => {
    setError('')
    setSelectedUser(user)
    setEditForm(emptyEditForm(user))
    setModal('edit')

    try {
      const freshUser = await adminApi.users.get(user.id)
      setSelectedUser(freshUser)
      setEditForm(emptyEditForm(freshUser))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unable to load user data.')
    }
  }

  const handleSearch = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void loadUsers(1)
  }

  const handleReset = () => {
    setSearchTerm('')
    setRole('')
    setIsActive('')
    void loadUsers(1, { isActive: '', role: '', searchTerm: '' })
  }

  const goToPage = (nextPage: number) => {
    if (nextPage < 1 || nextPage > pageCount || loading) return
    void loadUsers(nextPage)
  }

  const runAction = async (action: () => Promise<string>) => {
    setSaving(true)
    setError('')
    setSuccess('')

    try {
      const message = await action()
      setSuccess(message)
      closeModal()
      await loadUsers(pageNumber)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Action failed.')
    } finally {
      setSaving(false)
    }
  }

  const handleCreate = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (createForm.password !== createForm.confirmPassword) {
      setError('The password and confirmation password do not match.')
      return
    }

    void runAction(() => adminApi.users.create(createForm))
  }

  const handleEdit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    void runAction(() => adminApi.users.update(editForm))
  }

  const confirmDelete = () => {
    if (!selectedUser) return
    void runAction(() => adminApi.users.delete(selectedUser.id))
  }

  const confirmRestore = () => {
    if (!selectedUser) return
    void runAction(() => adminApi.users.restore(selectedUser.id))
  }

  const pageNumbers = Array.from({ length: pageCount }, (_, index) => index + 1)

  return (
    <AdminLayout activePage="Users" breadcrumb={['Dashboard', 'User Management']} pageHeader="User Management">
      <div className="flex flex-col gap-6">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCard icon="group" label="Total Users" tone="bg-blue-50 text-blue-600" value={stats.totalUsers} />
          <StatCard icon="how_to_reg" label="Active Now" tone="bg-green-50 text-green-600" value={stats.activeUsers} />
          <StatCard icon="person" label="Customers" tone="bg-amber-50 text-amber-600" value={stats.customerCount} />
          <StatCard icon="storefront" label="Store Owners" tone="bg-purple-50 text-purple-600" value={stats.storeOwnerCount} />
        </div>

        <div className="flex flex-col items-start justify-between gap-4 rounded-xl border border-slate-100 bg-white p-4 shadow-sm lg:flex-row lg:items-end">
          <form onSubmit={handleSearch} className="flex w-full flex-1 flex-col gap-4 sm:flex-row lg:w-auto">
            <div className="relative w-full sm:max-w-xs">
              <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-slate-400">
                <span className="material-symbols-outlined text-[20px]">search</span>
              </span>
              <input
                value={searchTerm}
                onChange={(event) => setSearchTerm(event.target.value)}
                className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 py-2.5 pl-10 pr-4 text-sm text-slate-900 placeholder:text-slate-400 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20"
                placeholder="Search by name or email..."
                type="text"
              />
            </div>

            <select
              value={role}
              onChange={(event) => {
                const nextRole = event.target.value
                setRole(nextRole)
                void loadUsers(1, { role: nextRole })
              }}
              className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-900 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 sm:w-40"
            >
              <option value="">All Roles</option>
              {roles.map((nextRole) => (
                <option key={nextRole} value={nextRole}>
                  {nextRole}
                </option>
              ))}
            </select>

            <select
              value={isActive === '' ? '' : String(isActive)}
              onChange={(event) => {
                const nextIsActive = event.target.value === '' ? '' : event.target.value === 'true'
                setIsActive(nextIsActive)
                void loadUsers(1, { isActive: nextIsActive })
              }}
              className="h-[42px] w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-900 focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 sm:w-40"
            >
              <option value="">All Status</option>
              <option value="true">Active</option>
              <option value="false">Inactive</option>
            </select>

            <button type="submit" className="hidden">
              Search
            </button>

            <button
              type="button"
              onClick={handleReset}
              className="flex h-[42px] items-center justify-center rounded-lg border border-slate-200 px-4 text-slate-600 transition-colors hover:bg-slate-50 hover:text-slate-900"
              title="Reset filters"
            >
              <span className="material-symbols-outlined text-[20px]">restart_alt</span>
            </button>
          </form>

          <button
            type="button"
            onClick={() => {
              setCreateForm(emptyCreateForm)
              setModal('add')
            }}
            className="flex h-[42px] w-full items-center justify-center gap-2 rounded-lg bg-primary px-5 py-2.5 text-sm font-medium text-white shadow-sm shadow-blue-500/30 transition hover:bg-blue-700 lg:w-auto"
          >
            <span className="material-symbols-outlined text-[20px]">add</span>
            <span>Add New User</span>
          </button>
        </div>

        {error && <div className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm font-medium text-red-700">{error}</div>}
        {success && <div className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">{success}</div>}

        <div className="flex flex-col overflow-hidden rounded-xl border border-slate-100 bg-white shadow-sm">
          <div className="overflow-x-auto">
            <table className="w-full border-collapse text-left">
              <thead>
                <tr className="border-b border-slate-100 bg-slate-50/50">
                  <th className="w-[50px] py-4 pl-6 pr-3 text-xs font-semibold uppercase tracking-wider text-slate-500">
                    <input className="size-4 rounded border-slate-300 text-primary focus:ring-primary" type="checkbox" />
                  </th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">User Profile</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Role</th>
                  <th className="px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500">Status</th>
                  <th className="hidden px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 md:table-cell">Phone</th>
                  <th className="hidden px-3 py-4 text-xs font-semibold uppercase tracking-wider text-slate-500 lg:table-cell">Created At</th>
                  <th className="py-4 pl-3 pr-6 text-right text-xs font-semibold uppercase tracking-wider text-slate-500">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {loading ? (
                  <LoadingRows />
                ) : users?.items.length ? (
                  users.items.map((user) => (
                    <tr key={user.id} onClick={() => void openDetail(user)} className="group cursor-pointer transition-all hover:bg-slate-50">
                      <td className="py-4 pl-6 pr-3">
                        <input
                          onClick={(event) => event.stopPropagation()}
                          className="size-4 rounded border-slate-300 text-primary focus:ring-primary"
                          type="checkbox"
                          value={user.id}
                        />
                      </td>
                      <td className="px-3 py-4">
                        <div className="flex items-center gap-3">
                          <div className="relative shrink-0">
                            <Avatar user={user} />
                            <span
                              className={`absolute bottom-0 right-0 size-2.5 rounded-full border-2 border-white ${
                                user.isActive && !user.isDeleted ? 'bg-green-500' : 'bg-slate-300'
                              }`}
                            />
                          </div>
                          <div className="flex min-w-0 flex-col">
                            <span className="truncate text-sm font-semibold text-slate-900 transition-colors group-hover:text-primary">
                              {displayName(user)}
                            </span>
                            <span className="truncate text-xs text-slate-500">{user.email}</span>
                          </div>
                        </div>
                      </td>
                      <td className="px-3 py-4">
                        <RoleBadge role={user.role} />
                      </td>
                      <td className="px-3 py-4">
                        <StatusBadge user={user} />
                      </td>
                      <td className="hidden px-3 py-4 md:table-cell">
                        <span className="text-sm text-slate-600">{user.phoneNumber || '-'}</span>
                      </td>
                      <td className="hidden px-3 py-4 lg:table-cell">
                        <p className="text-sm text-slate-600">{formatDate(user.createdAt)}</p>
                      </td>
                      <td className="py-4 pl-3 pr-6 text-right">
                        <div
                          onClick={(event) => event.stopPropagation()}
                          className="flex items-center justify-end gap-2 opacity-0 transition-opacity group-hover:opacity-100"
                        >
                          <button type="button" onClick={() => void openDetail(user)} className="p-1 text-slate-400 transition-colors hover:text-primary" title="View Details">
                            <span className="material-symbols-outlined text-[20px]">visibility</span>
                          </button>
                          {user.isDeleted ? (
                            <button
                              type="button"
                              onClick={() => {
                                setSelectedUser(user)
                                setModal('restore')
                              }}
                              className="p-1 text-slate-400 transition-colors hover:text-green-600"
                              title="Restore User"
                            >
                              <span className="material-symbols-outlined text-[20px]">restore</span>
                            </button>
                          ) : (
                            <>
                              <button type="button" onClick={() => void openEdit(user)} className="p-1 text-slate-400 transition-colors hover:text-amber-500" title="Edit User">
                                <span className="material-symbols-outlined text-[20px]">edit</span>
                              </button>
                              <button
                                type="button"
                                onClick={() => {
                                  setSelectedUser(user)
                                  setModal('delete')
                                }}
                                className="p-1 text-slate-400 transition-colors hover:text-red-600"
                                title="Delete User"
                              >
                                <span className="material-symbols-outlined text-[20px]">delete</span>
                              </button>
                            </>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={7} className="py-8 text-center text-slate-500">
                      <div className="flex flex-col items-center justify-center gap-3">
                        <span className="material-symbols-outlined text-4xl text-slate-300">search_off</span>
                        <p className="text-base font-medium">No users found</p>
                        <p className="text-sm">Try adjusting your filters or search term.</p>
                      </div>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          <div className="flex flex-col items-center justify-between gap-4 border-t border-slate-100 bg-slate-50/30 px-6 py-4 sm:flex-row">
            <div className="text-sm text-slate-500">
              Showing <span className="font-medium text-slate-900">{rangeText.start}</span> to{' '}
              <span className="font-medium text-slate-900">{rangeText.end}</span> of{' '}
              <span className="font-medium text-slate-900">{rangeText.total}</span> users
            </div>

            {pageCount > 1 && (
              <nav aria-label="Pagination" className="flex items-center gap-1">
                <button
                  type="button"
                  disabled={pageNumber <= 1 || loading}
                  onClick={() => goToPage(pageNumber - 1)}
                  className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:pointer-events-none disabled:opacity-50"
                  title="Previous"
                >
                  <span className="material-symbols-outlined text-lg leading-none">chevron_left</span>
                </button>

                <div className="flex items-center gap-1 px-1">
                  {pageNumbers.map((page) => (
                    <button
                      key={page}
                      type="button"
                      onClick={() => goToPage(page)}
                      className={`flex h-9 min-w-[36px] items-center justify-center rounded-lg text-sm font-medium transition-all ${
                        page === pageNumber
                          ? 'bg-primary text-white shadow-sm shadow-blue-500/20'
                          : 'border border-transparent text-slate-600 hover:border-slate-200 hover:bg-white hover:text-primary'
                      }`}
                    >
                      {page}
                    </button>
                  ))}
                </div>

                <button
                  type="button"
                  disabled={pageNumber >= pageCount || loading}
                  onClick={() => goToPage(pageNumber + 1)}
                  className="rounded-lg border border-slate-200 p-2 text-slate-400 shadow-sm transition-all hover:border-primary hover:bg-white hover:text-primary disabled:pointer-events-none disabled:opacity-50"
                  title="Next"
                >
                  <span className="material-symbols-outlined text-lg leading-none">chevron_right</span>
                </button>
              </nav>
            )}
          </div>
        </div>
      </div>

      {modal === 'add' && (
        <ModalShell onClose={closeModal}>
          <UserForm
            form={createForm}
            mode="create"
            onCancel={closeModal}
            onChange={(patch) => setCreateForm((current) => ({ ...current, ...patch }))}
            onSubmit={handleCreate}
            roles={roles}
            saving={saving}
          />
        </ModalShell>
      )}

      {modal === 'edit' && (
        <ModalShell onClose={closeModal}>
          <UserForm
            form={editForm}
            mode="edit"
            onCancel={closeModal}
            onChange={(patch) => setEditForm((current) => ({ ...current, ...patch }))}
            onSubmit={handleEdit}
            roles={roles}
            saving={saving}
          />
        </ModalShell>
      )}

      {(modal === 'delete' || modal === 'restore') && selectedUser && (
        <ModalShell maxWidth="max-w-md" onClose={closeModal}>
          <div className="p-6">
            <div className="flex gap-4">
              <div
                className={`mt-1 flex size-10 shrink-0 items-center justify-center rounded-full ${
                  modal === 'delete' ? 'bg-red-100 text-red-600' : 'bg-green-100 text-green-600'
                }`}
              >
                <span className="material-symbols-outlined text-2xl">{modal === 'delete' ? 'warning' : 'restore'}</span>
              </div>
              <div>
                <h3 className="text-xl font-bold text-slate-900">{modal === 'delete' ? 'Delete User' : 'Restore User'}</h3>
                <p className="mt-1 text-sm text-slate-500">
                  {modal === 'delete' ? 'You are about to delete' : 'You are about to restore'}{' '}
                  <span className="font-bold text-slate-900">{displayName(selectedUser)}</span>.
                </p>
              </div>
            </div>
            <div className="mt-6 flex justify-end gap-3 border-t border-slate-100 pt-5">
              <button type="button" onClick={closeModal} className="rounded-xl border border-slate-200 px-5 py-2 text-sm font-bold text-slate-600 hover:bg-slate-50">
                Cancel
              </button>
              <button
                type="button"
                disabled={saving}
                onClick={modal === 'delete' ? confirmDelete : confirmRestore}
                className={`rounded-xl px-6 py-2 text-sm font-bold text-white disabled:opacity-60 ${
                  modal === 'delete' ? 'bg-red-600 hover:bg-red-700' : 'bg-green-600 hover:bg-green-700'
                }`}
              >
                {saving ? 'Processing...' : modal === 'delete' ? 'Confirm Delete' : 'Confirm Restore'}
              </button>
            </div>
          </div>
        </ModalShell>
      )}

      {modal === 'detail' && selectedUser && (
        <ModalShell maxWidth="max-w-4xl" onClose={closeModal}>
          <div className="border-b border-slate-100 px-8 pt-8">
            <div className="flex flex-col gap-6 sm:flex-row sm:items-start">
              <div className="relative shrink-0">
                <Avatar user={selectedUser} size="size-24" />
                <span
                  className={`absolute bottom-1 right-1 size-5 rounded-full border-2 border-white shadow-sm ${
                    selectedUser.isActive && !selectedUser.isDeleted ? 'bg-green-500' : 'bg-slate-300'
                  }`}
                />
              </div>

              <div className="flex-1 pt-1">
                <h2 className="text-2xl font-bold leading-tight text-slate-900">{displayName(selectedUser)}</h2>
                <p className="mt-0.5 font-medium italic text-slate-500">{selectedUser.email}</p>
                <div className="mt-3 flex flex-wrap items-center gap-2.5">
                  <span className="rounded-lg bg-primary/5 px-3 py-1 text-xs font-bold text-primary ring-1 ring-inset ring-primary/10">
                    {selectedUser.role || 'No role'}
                  </span>
                  <span className="rounded-lg bg-slate-100 px-3 py-1 text-xs font-bold text-slate-500">ID: #{selectedUser.id.slice(0, 8)}</span>
                </div>
              </div>
            </div>

            <div className="mt-8 flex items-center gap-8 border-b border-slate-100">
              {(['profile', 'access', 'activity'] as DetailTab[]).map((tab) => (
                <button
                  key={tab}
                  type="button"
                  onClick={() => setDetailTab(tab)}
                  className={`px-1 py-4 text-sm transition-all ${
                    detailTab === tab
                      ? 'border-b-2 border-primary font-bold text-primary'
                      : 'border-b-2 border-transparent font-semibold text-slate-400 hover:text-slate-600'
                  }`}
                >
                  {tab === 'profile' ? 'Profile Info' : tab === 'access' ? 'Access & Roles' : 'Recent Activity'}
                </button>
              ))}
            </div>
          </div>

          <div className="flex-1 overflow-y-auto p-8 pt-6">
            {detailTab === 'profile' && (
              <div className="space-y-8">
                <div className="grid gap-x-12 gap-y-8 sm:grid-cols-2">
                  <div className="space-y-1.5">
                    <span className="block text-[10px] font-bold uppercase tracking-widest text-slate-400">Phone Number</span>
                    <div className="flex items-center gap-3 text-slate-700">
                      <span className="material-symbols-outlined text-xl font-light text-slate-400">call</span>
                      <span className="font-semibold">{selectedUser.phoneNumber || 'Not provided'}</span>
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <span className="block text-[10px] font-bold uppercase tracking-widest text-slate-400">Account Status</span>
                    <StatusBadge user={selectedUser} />
                  </div>
                  <div className="space-y-1.5">
                    <span className="block text-[10px] font-bold uppercase tracking-widest text-slate-400">Joined Date</span>
                    <div className="flex items-center gap-3 text-slate-700">
                      <span className="material-symbols-outlined text-xl font-light text-slate-400">calendar_today</span>
                      <span className="font-semibold">{formatDate(selectedUser.createdAt)}</span>
                    </div>
                  </div>
                  <div className="space-y-1.5">
                    <span className="block text-[10px] font-bold uppercase tracking-widest text-slate-400">Last Login</span>
                    <div className="flex items-center gap-3 text-slate-700">
                      <span className="material-symbols-outlined text-xl font-light text-slate-400">history</span>
                      <span className="font-semibold">Not tracked</span>
                    </div>
                  </div>
                </div>

                <div className="space-y-4 rounded-3xl border border-slate-100/50 bg-slate-50/50 p-6">
                  <h4 className="flex items-center gap-2 text-sm font-bold text-slate-900">
                    <span className="material-symbols-outlined text-[18px]">info</span>
                    About User
                  </h4>
                  <p className="text-sm font-medium leading-relaxed text-slate-600">
                    Account created on {formatDateTime(selectedUser.createdAt)}. Role and account access can be managed from the edit action.
                  </p>
                </div>
              </div>
            )}

            {detailTab === 'access' && (
              <div className="space-y-4 p-12 text-center">
                <div className="mx-auto flex size-16 items-center justify-center rounded-3xl bg-slate-50 text-slate-300">
                  <span className="material-symbols-outlined text-4xl">security</span>
                </div>
                <p className="font-medium text-slate-500">Access details are only available for administrative roles.</p>
              </div>
            )}

            {detailTab === 'activity' && (
              <div className="space-y-4 p-12 text-center">
                <div className="mx-auto flex size-16 items-center justify-center rounded-3xl bg-slate-50 text-slate-300">
                  <span className="material-symbols-outlined text-4xl">history</span>
                </div>
                <p className="font-medium text-slate-500">No recent activity logs found for this period.</p>
              </div>
            )}
          </div>

          <div className="flex items-center justify-between border-t border-slate-100 bg-white px-8 py-6">
            <button type="button" className="flex items-center gap-1.5 text-sm font-bold text-primary hover:underline">
              View detailed logs
            </button>
            <div className="flex gap-3">
              {!selectedUser.isDeleted && (
                <button
                  type="button"
                  onClick={() => void openEdit(selectedUser)}
                  className="rounded-xl border border-slate-200 bg-white px-6 py-2.5 text-sm font-bold text-slate-700 shadow-sm transition-all hover:border-slate-300 hover:bg-slate-50"
                >
                  Edit User
                </button>
              )}
              <button
                type="button"
                onClick={closeModal}
                className="rounded-xl bg-primary px-8 py-2.5 text-sm font-bold text-white shadow-md shadow-primary/20 transition-all hover:bg-blue-700"
              >
                Close
              </button>
            </div>
          </div>
        </ModalShell>
      )}
    </AdminLayout>
  )
}
