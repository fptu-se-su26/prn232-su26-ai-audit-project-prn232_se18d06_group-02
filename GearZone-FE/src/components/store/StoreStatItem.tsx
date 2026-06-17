import type { ReactNode } from 'react'

interface StoreStatItemProps {
  icon: string
  value: ReactNode
  label: string
  sublabel?: string
}

export default function StoreStatItem({ icon, value, label, sublabel }: StoreStatItemProps) {
  return (
    <div className="flex flex-col items-center gap-1 rounded-xl bg-white/5 px-3 py-4 text-center">
      <span className="material-symbols-outlined text-[20px] text-orange-300">{icon}</span>
      <span className="text-lg font-bold leading-tight text-white">{value}</span>
      <span className="text-xs font-medium text-slate-300">{label}</span>
      {sublabel && <span className="text-[11px] text-slate-400">{sublabel}</span>}
    </div>
  )
}
