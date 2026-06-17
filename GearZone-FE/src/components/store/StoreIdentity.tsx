import Avatar from '@/components/ui/Avatar'
import type { StoreProfile } from '@/types/store'

interface StoreIdentityProps {
  store: StoreProfile
}

export default function StoreIdentity({ store }: StoreIdentityProps) {
  return (
    <div className="flex items-start gap-4">
      <Avatar name={store.storeName} src={store.logoUrl} size={72} className="ring-2 ring-white/20" />
      <div className="min-w-0">
        <div className="flex items-center gap-2">
          <h1 className="truncate text-xl font-bold text-white md:text-2xl">{store.storeName}</h1>
          <span className="inline-flex shrink-0 items-center gap-1 rounded-full bg-blue-500/20 px-2 py-0.5 text-[11px] font-semibold text-blue-200">
            <span className="material-symbols-outlined text-[14px]" style={{ fontVariationSettings: "'FILL' 1" }}>
              verified
            </span>
            Verified
          </span>
        </div>
        <div className="mt-1 flex items-center gap-1 text-sm text-slate-300">
          <span className="material-symbols-outlined text-[16px]">location_on</span>
          <span className="truncate">{store.province}</span>
        </div>
        {store.description && (
          <p className="mt-2 line-clamp-2 max-w-prose text-sm text-slate-300">{store.description}</p>
        )}
      </div>
    </div>
  )
}
