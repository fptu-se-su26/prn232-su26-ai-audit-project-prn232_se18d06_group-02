import UnreadBadge from '@/components/ui/UnreadBadge'

interface ChatLauncherProps {
  unreadCount: number
  onClick: () => void
}

export default function ChatLauncher({ unreadCount, onClick }: ChatLauncherProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="relative flex h-14 w-14 items-center justify-center rounded-full bg-secondary text-white shadow-lg transition hover:opacity-90"
      aria-label="Open chat"
    >
      <span className="material-symbols-outlined text-[26px]">chat</span>
      {unreadCount > 0 && (
        <span className="absolute -right-1 -top-1">
          <UnreadBadge count={unreadCount} />
        </span>
      )}
    </button>
  )
}
