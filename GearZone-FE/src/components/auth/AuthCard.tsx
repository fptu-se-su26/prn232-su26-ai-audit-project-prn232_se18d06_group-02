import type { ReactNode } from 'react'

export function AuthCard({ children }: { children: ReactNode }) {
  return (
    <div className="auth-card-shadow relative z-10 w-[1000px] max-w-[95vw] overflow-hidden rounded-[2.5rem] bg-white/95 backdrop-blur-xl min-h-[650px]">
      {children}
    </div>
  )
}
