import type { ReactNode } from 'react'

interface EmptyStateProps {
  icon?: string
  title: string
  description?: string
  action?: ReactNode
  className?: string
}

export default function EmptyState({
  icon = 'inbox',
  title,
  description,
  action,
  className = '',
}: EmptyStateProps) {
  return (
    <div className={`flex h-full min-h-[16rem] flex-col items-center justify-center px-6 py-10 text-center ${className}`}>
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-orange-50 text-secondary">
        <span className="material-symbols-outlined text-[24px]">{icon}</span>
      </div>
      <h3 className="mt-4 text-base font-semibold text-gray-800">{title}</h3>
      {description && <p className="mt-2 max-w-xs text-[13px] leading-6 text-gray-500">{description}</p>}
      {action && <div className="mt-4">{action}</div>}
    </div>
  )
}
