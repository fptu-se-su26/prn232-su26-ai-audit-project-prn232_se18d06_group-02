import type { InputHTMLAttributes } from 'react'

interface AuthTextInputProps extends InputHTMLAttributes<HTMLInputElement> {
  icon?: string
  className?: string
}

const baseClassName =
  'w-full rounded-2xl border-transparent bg-slate-50 py-4 pr-5 text-sm text-slate-700 transition-all duration-300 focus:bg-white focus:ring-4 focus:ring-blue-100 focus:outline-none'

export function AuthTextInput({ icon, className = '', ...props }: AuthTextInputProps) {
  if (!icon) {
    return <input {...props} className={`${baseClassName} pl-5 ${className}`.trim()} />
  }

  return (
    <div className="relative">
      <span className="material-symbols-outlined absolute top-1/2 left-4 -translate-y-1/2 text-[20px] text-slate-300">
        {icon}
      </span>
      <input {...props} className={`${baseClassName} pl-12 ${className}`.trim()} />
    </div>
  )
}
