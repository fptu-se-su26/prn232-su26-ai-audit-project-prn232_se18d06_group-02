import { formatCount } from '@/lib/format'

interface StoreFollowButtonProps {
  isFollowing: boolean
  followerCount: number
  pending: boolean
  onToggle: () => void
}

export default function StoreFollowButton({
  isFollowing,
  followerCount,
  pending,
  onToggle,
}: StoreFollowButtonProps) {
  return (
    <div className="flex items-center gap-2">
      <button
        type="button"
        onClick={onToggle}
        disabled={pending}
        aria-pressed={isFollowing}
        className={`inline-flex items-center gap-1.5 rounded-lg px-4 py-2 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-60 ${
          isFollowing
            ? 'bg-white/10 text-white hover:bg-white/20'
            : 'bg-secondary text-white hover:opacity-90'
        }`}
      >
        <span className="material-symbols-outlined text-[18px]">
          {isFollowing ? 'check' : 'add'}
        </span>
        {isFollowing ? 'Following' : 'Follow'}
      </button>
      <span className="text-sm text-slate-300">{formatCount(followerCount)} followers</span>
    </div>
  )
}
