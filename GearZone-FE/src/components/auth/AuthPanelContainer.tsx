import type { CSSProperties, ReactNode } from 'react'

interface AuthPanelContainerProps {
  className: string
  style: CSSProperties
  children: ReactNode
}

export function AuthPanelContainer({ className, style, children }: AuthPanelContainerProps) {
  return (
    <div className={className} style={style}>
      <div className="mx-auto w-full max-w-md">{children}</div>
    </div>
  )
}
