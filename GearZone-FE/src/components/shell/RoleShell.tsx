import { useMemo } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'
import type { UserDto } from '@/api/auth'
import { getShellPath, shellConfigMap, type ShellRoleKey } from '@/lib/roleShell'

interface RoleShellProps {
  roleKey: ShellRoleKey
}

function statusStyles(status: 'ready' | 'partial' | 'pending') {
  switch (status) {
    case 'ready':
      return 'border-emerald-500/30 bg-emerald-500/10 text-emerald-200'
    case 'partial':
      return 'border-amber-500/30 bg-amber-500/10 text-amber-200'
    default:
      return 'border-slate-500/30 bg-slate-500/10 text-slate-300'
  }
}

export function RoleShell({ roleKey }: RoleShellProps) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const config = shellConfigMap[roleKey]
  const fullName = user?.fullName

  const initials = useMemo(() => {
    if (!fullName) return 'GZ'
    return fullName
      .split(' ')
      .filter(Boolean)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('')
  }, [fullName])

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <div className="pointer-events-none fixed inset-0 overflow-hidden">
        <div className="absolute -top-32 left-1/2 h-80 w-80 -translate-x-1/2 rounded-full bg-cyan-500/20 blur-3xl" />
        <div className="absolute top-40 right-0 h-72 w-72 rounded-full bg-fuchsia-500/20 blur-3xl" />
        <div className="absolute bottom-0 left-0 h-72 w-72 rounded-full bg-amber-500/10 blur-3xl" />
      </div>

      <div className="relative mx-auto flex min-h-screen max-w-7xl flex-col lg:flex-row">
        <aside className="border-white/10 bg-slate-950/70 px-6 py-8 backdrop-blur xl:w-80 lg:border-r">
          <div className="mb-8 flex items-center gap-3">
            <div className={`flex h-12 w-12 items-center justify-center rounded-2xl bg-gradient-to-br ${config.accent} shadow-lg`}>
              <span className="material-symbols-outlined text-[26px] text-white">{config.heroIcon}</span>
            </div>
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.3em] text-slate-400">FE-SHARED</p>
              <p className="text-xl font-bold text-white">GearZone Shells</p>
            </div>
          </div>

          <div className="rounded-3xl border border-white/10 bg-white/5 p-5 shadow-2xl shadow-black/20">
            <p className="text-xs font-semibold uppercase tracking-[0.3em] text-slate-400">{config.badge}</p>
            <h1 className="mt-3 text-2xl font-black text-white">{config.title}</h1>
            <p className="mt-3 text-sm leading-6 text-slate-300">{config.subtitle}</p>
            <div className="mt-4 flex flex-wrap gap-2">
              <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-semibold text-slate-200">
                Route: {config.routeHint}
              </span>
              <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-semibold text-slate-200">
                Role: {config.label}
              </span>
            </div>
          </div>

          <nav className="mt-8 space-y-2">
            {[
              { label: 'Overview', icon: 'dashboard', href: '#overview' },
              { label: 'Workflow', icon: 'tune', href: '#workflow' },
              { label: 'Backend checks', icon: 'fact_check', href: '#backend' },
              { label: 'Summary', icon: 'insights', href: '#summary' },
            ].map((item) => (
              <a
                key={item.href}
                href={item.href}
                className="flex items-center gap-3 rounded-2xl border border-transparent px-4 py-3 text-sm font-semibold text-slate-300 transition hover:border-white/10 hover:bg-white/5 hover:text-white"
              >
                <span className="material-symbols-outlined text-[20px] text-slate-400">{item.icon}</span>
                {item.label}
              </a>
            ))}
          </nav>

          <div className="mt-8 rounded-3xl border border-white/10 bg-white/5 p-5">
            <p className="text-xs font-semibold uppercase tracking-[0.3em] text-slate-400">Session</p>
            <div className="mt-4 flex items-center gap-3">
              <div className={`flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br ${config.accent} text-sm font-bold text-white`}>
                {initials}
              </div>
              <div className="min-w-0">
                <p className="truncate text-sm font-semibold text-white">{user?.fullName ?? 'Guest'}</p>
                <p className="truncate text-xs text-slate-400">{(user as UserDto | null)?.email ?? 'Not signed in'}</p>
              </div>
            </div>
            <button
              type="button"
              onClick={handleLogout}
              className="mt-4 inline-flex w-full items-center justify-center gap-2 rounded-2xl border border-white/10 bg-white/5 px-4 py-3 text-sm font-semibold text-slate-200 transition hover:bg-white/10"
            >
              <span className="material-symbols-outlined text-[18px]">logout</span>
              Sign out
            </button>
          </div>
        </aside>

        <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8 lg:py-8">
          <section id="overview" className="rounded-[2rem] border border-white/10 bg-white/5 p-6 shadow-2xl shadow-black/20 backdrop-blur">
            <div className="flex flex-col gap-6 xl:flex-row xl:items-end xl:justify-between">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.3em] text-slate-400">{config.badge}</p>
                <h2 className="mt-3 text-4xl font-black tracking-tight text-white">{config.title}</h2>
                <p className="mt-4 max-w-3xl text-base leading-7 text-slate-300">{config.summary}</p>
              </div>
              <div className={`inline-flex items-center gap-3 rounded-2xl bg-gradient-to-r ${config.accent} px-5 py-4 text-white shadow-lg`}>
                <span className="material-symbols-outlined text-[24px]">{config.heroIcon}</span>
                <div>
                  <p className="text-xs uppercase tracking-[0.25em] text-white/80">Active shell</p>
                  <p className="text-sm font-semibold">{config.routeHint}</p>
                </div>
              </div>
            </div>

            <div className="mt-8 grid gap-4 md:grid-cols-3">
              {config.metrics.map((metric) => (
                <div key={metric.label} className="rounded-3xl border border-white/10 bg-slate-900/60 p-5">
                  <p className="text-xs font-semibold uppercase tracking-[0.25em] text-slate-400">{metric.label}</p>
                  <p className="mt-3 text-3xl font-black text-white">{metric.value}</p>
                  <p className="mt-2 text-sm leading-6 text-slate-400">{metric.detail}</p>
                </div>
              ))}
            </div>
          </section>

          <section id="workflow" className="mt-6 grid gap-6 xl:grid-cols-[1.5fr_1fr]">
            <div className="rounded-[2rem] border border-white/10 bg-slate-900/60 p-6">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-[0.3em] text-slate-400">Workflow</p>
                  <h3 className="mt-2 text-2xl font-bold text-white">Role-specific frame</h3>
                </div>
                <Link
                  to={getShellPath(roleKey)}
                  className={`rounded-2xl bg-gradient-to-r ${config.accent} px-4 py-2 text-sm font-semibold text-white`}
                >
                  Open shell
                </Link>
              </div>

              <div className="mt-5 grid gap-4">
                {config.workflow.map((item) => (
                  <div key={item.title} className="flex gap-4 rounded-3xl border border-white/10 bg-white/5 p-5">
                    <div className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br ${config.accent} text-white`}>
                      <span className="material-symbols-outlined text-[22px]">{item.icon}</span>
                    </div>
                    <div>
                      <h4 className="text-lg font-semibold text-white">{item.title}</h4>
                      <p className="mt-2 text-sm leading-6 text-slate-300">{item.description}</p>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <div className="rounded-[2rem] border border-white/10 bg-slate-900/60 p-6">
              <p className="text-xs font-semibold uppercase tracking-[0.3em] text-slate-400">Quick actions</p>
              <div className="mt-4 space-y-3">
                {config.actions.map((action) => (
                  <a
                    key={action.label}
                    href={`#${action.targetId}`}
                    className="flex items-center gap-3 rounded-2xl border border-white/10 bg-white/5 px-4 py-4 transition hover:bg-white/10"
                  >
                    <span className={`flex h-11 w-11 items-center justify-center rounded-2xl bg-gradient-to-br ${config.accent}`}>
                      <span className="material-symbols-outlined text-[20px] text-white">{action.icon}</span>
                    </span>
                    <div className="min-w-0">
                      <p className="text-sm font-semibold text-white">{action.label}</p>
                      <p className="text-xs leading-5 text-slate-400">{action.description}</p>
                    </div>
                  </a>
                ))}
              </div>
            </div>
          </section>

          <section id="backend" className="mt-6 rounded-[2rem] border border-white/10 bg-slate-900/60 p-6">
            <div className="flex items-center justify-between gap-4">
              <div>
                <p className="text-xs font-semibold uppercase tracking-[0.3em] text-slate-400">Backend readiness</p>
                <h3 className="mt-2 text-2xl font-bold text-white">Implementation check</h3>
              </div>
              <span className="rounded-full border border-white/10 bg-white/5 px-3 py-1 text-xs font-semibold text-slate-300">
                Based on backend scan
              </span>
            </div>

            <div className="mt-5 grid gap-3">
              {config.backendChecks.map((check) => (
                <div key={check.label} className="flex items-start justify-between gap-4 rounded-3xl border border-white/10 bg-white/5 px-5 py-4">
                  <div>
                    <p className="text-sm font-semibold text-white">{check.label}</p>
                    <p className="mt-1 text-sm leading-6 text-slate-400">{check.detail}</p>
                  </div>
                  <span className={`shrink-0 rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-[0.2em] ${statusStyles(check.status)}`}>
                    {check.status}
                  </span>
                </div>
              ))}
            </div>
          </section>

          <section id="summary" className="mt-6 rounded-[2rem] border border-white/10 bg-gradient-to-br from-white/10 to-white/5 p-6">
            <p className="text-xs font-semibold uppercase tracking-[0.3em] text-slate-400">Summary</p>
            <p className="mt-3 max-w-4xl text-sm leading-7 text-slate-300">
              The shell is intentionally role-specific but shared in implementation. Customer, Store Owner, and Admin
              are backed by existing API groups. Staff is scaffolded in the frontend and should be connected once a
              dedicated backend contract is added.
            </p>
          </section>
        </main>
      </div>
    </div>
  )
}
