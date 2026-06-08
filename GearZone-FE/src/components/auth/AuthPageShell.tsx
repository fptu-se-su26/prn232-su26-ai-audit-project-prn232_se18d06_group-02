import type { CSSProperties, ReactNode } from 'react'

const shellStyle: CSSProperties = {
  backgroundImage: "url('https://images.unsplash.com/photo-1714310289917-3b5c20dbe1af?w=1920&q=80')",
  backgroundSize: 'cover',
  backgroundPosition: 'center',
  backgroundAttachment: 'fixed',
}

export function AuthPageShell({ children }: { children: ReactNode }) {
  return (
    <div className="relative flex min-h-screen items-center justify-center p-6" style={shellStyle}>
      <div className="fixed inset-0 z-0 bg-slate-900/60 backdrop-blur-[10px]" />
      {children}
    </div>
  )
}
