import StoreIdentity from '@/components/store/StoreIdentity'
import StoreFollowButton from '@/components/store/StoreFollowButton'
import StoreChatButton from '@/components/store/StoreChatButton'
import StoreStats from '@/components/store/StoreStats'
import type { StoreProfile } from '@/types/store'

interface StoreHeaderProps {
  store: StoreProfile
  isFollowing: boolean
  followerCount: number
  followPending: boolean
  onToggleFollow: () => void
}

export default function StoreHeader({
  store,
  isFollowing,
  followerCount,
  followPending,
  onToggleFollow,
}: StoreHeaderProps) {
  return (
    <header className="bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900">
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
          <div className="flex flex-1 flex-col gap-4">
            <StoreIdentity store={store} />
            <div className="flex flex-wrap items-center gap-3">
              <StoreFollowButton
                isFollowing={isFollowing}
                followerCount={followerCount}
                pending={followPending}
                onToggle={onToggleFollow}
              />
              <StoreChatButton storeSlug={store.slug} />
            </div>
          </div>
          <div className="w-full lg:max-w-md">
            <StoreStats store={store} />
          </div>
        </div>
      </div>
    </header>
  )
}
