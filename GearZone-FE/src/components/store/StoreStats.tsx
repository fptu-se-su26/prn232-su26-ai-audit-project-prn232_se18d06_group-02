import StoreStatItem from '@/components/store/StoreStatItem'
import { formatCount, formatRelativeTime } from '@/lib/format'
import type { StoreProfile } from '@/types/store'

interface StoreStatsProps {
  store: StoreProfile
}

export default function StoreStats({ store }: StoreStatsProps) {
  return (
    <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
      <StoreStatItem icon="inventory_2" value={formatCount(store.productCount)} label="Products" />
      <StoreStatItem icon="local_shipping" value={formatCount(store.totalSold)} label="Total Sold" />
      <StoreStatItem
        icon="star"
        value={`${store.rating.toFixed(1)}/5`}
        label="Rating"
        sublabel={`${formatCount(store.reviewCount)} reviews`}
      />
      <StoreStatItem icon="calendar_month" value={formatRelativeTime(store.createdAt)} label="Joined" />
    </div>
  )
}
