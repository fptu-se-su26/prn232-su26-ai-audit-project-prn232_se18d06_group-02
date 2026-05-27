import type { ReactNode } from 'react'

export function AuthPageShell({ children }: { children: ReactNode }) {
  return (
    <div className="auth-page-backdrop relative flex min-h-screen items-center justify-center p-6">
      <div className="fixed inset-0 z-0 bg-slate-900/60 backdrop-blur-[10px]" />
      {children}
    </div>
  )
}
