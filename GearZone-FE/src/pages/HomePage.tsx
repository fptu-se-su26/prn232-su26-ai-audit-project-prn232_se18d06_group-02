import { Link } from 'react-router-dom'
import { shellConfigMap, type ShellRoleKey } from '@/lib/roleShell'

const landingRoles: ShellRoleKey[] = ['customer', 'store-owner', 'admin', 'staff']

function readinessTone(role: ShellRoleKey) {
  switch (role) {
    case 'admin':
    case 'store-owner':
      return 'border-emerald-500/20 bg-emerald-500/10 text-emerald-200'
    case 'customer':
      return 'border-cyan-500/20 bg-cyan-500/10 text-cyan-200'
    default:
      return 'border-amber-500/20 bg-amber-500/10 text-amber-200'
  }
}

export default function HomePage() {
  return (
    <div className="min-h-screen bg-slate-950 text-white">
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute -top-32 left-0 h-80 w-80 rounded-full bg-cyan-500/20 blur-3xl" />
        <div className="absolute top-24 right-0 h-80 w-80 rounded-full bg-fuchsia-500/20 blur-3xl" />
        <div className="absolute bottom-0 left-1/2 h-96 w-96 -translate-x-1/2 rounded-full bg-amber-500/10 blur-3xl" />
      </div>

      <div className="relative mx-auto flex min-h-screen max-w-7xl flex-col justify-center px-4 py-12 sm:px-6 lg:px-8">
        <div className="max-w-4xl">
          <p className="text-sm font-semibold uppercase tracking-[0.4em] text-slate-400">FE-SHARED</p>
          <h1 className="mt-4 text-5xl font-black tracking-tight text-white sm:text-6xl">
            Role shells for Customer, Store Owner, Admin, and Staff.
          </h1>
          <p className="mt-6 max-w-3xl text-lg leading-8 text-slate-300">
            This landing page is the routing gateway for the shared shell system. It mirrors the backend role model and
            keeps each user in a focused workspace instead of a single generic dashboard.
          </p>
        </div>

        <div className="mt-10 grid gap-5 lg:grid-cols-2">
          {landingRoles.map((role) => {
            const config = shellConfigMap[role]
            return (
              <Link
                key={config.key}
                to={config.routeHint}
                className="group rounded-[2rem] border border-white/10 bg-white/5 p-6 transition hover:-translate-y-1 hover:border-white/20 hover:bg-white/10"
              >
                <div className="flex items-start justify-between gap-4">
                  <div className={`flex h-14 w-14 items-center justify-center rounded-2xl bg-gradient-to-br ${config.accent}`}>
                    <span className="material-symbols-outlined text-[28px] text-white">{config.heroIcon}</span>
                  </div>
                  <span className={`rounded-full border px-3 py-1 text-xs font-semibold uppercase tracking-[0.2em] ${readinessTone(role)}`}>
                    {config.badge}
                  </span>
                </div>

                <h2 className="mt-6 text-2xl font-bold text-white">{config.title}</h2>
                <p className="mt-3 text-sm leading-6 text-slate-300">{config.summary}</p>

                <div className="mt-6 flex items-center gap-3 text-sm font-semibold text-slate-200">
                  <span>Open shell</span>
                  <span className="material-symbols-outlined text-[18px] transition group-hover:translate-x-1">arrow_forward</span>
                </div>
              </Link>
            )
          })}
        </div>
      </div>
    </div>
  )
}
