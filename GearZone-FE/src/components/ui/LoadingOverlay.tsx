import type { ReactNode } from 'react'

interface LoadingOverlayProps {
  loading: boolean
  children: ReactNode
  className?: string
}

/** Dims its children and shows a spinner while `loading` is true. */
export default function LoadingOverlay({ loading, children, className = '' }: LoadingOverlayProps) {
  return (
    <div className={`relative ${className}`}>
      <div className={loading ? 'pointer-events-none opacity-70 transition-opacity' : 'transition-opacity'}>
        {children}
      </div>
      {loading && (
        <div className="pointer-events-none absolute inset-0 flex items-start justify-center pt-6">
          <span className="material-symbols-outlined animate-spin text-[22px] text-secondary">progress_activity</span>
        </div>
      )}
    </div>
  )
}
