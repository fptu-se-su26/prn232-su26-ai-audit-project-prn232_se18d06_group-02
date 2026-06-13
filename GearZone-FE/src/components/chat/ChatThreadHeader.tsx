import Avatar from '@/components/ui/Avatar'

interface ChatThreadHeaderProps {
  name: string
  avatarUrl?: string | null
  onBack?: () => void
}

export default function ChatThreadHeader({ name, avatarUrl, onBack }: ChatThreadHeaderProps) {
  return (
    <div className="flex items-center gap-3 border-b border-gray-100 bg-white px-4 py-3">
      {onBack && (
        <button type="button" onClick={onBack} className="text-gray-500 lg:hidden" aria-label="Back to conversations">
          <span className="material-symbols-outlined text-[22px]">arrow_back</span>
        </button>
      )}
      <Avatar name={name} src={avatarUrl} size={36} />
      <span className="truncate text-sm font-semibold text-gray-800">{name}</span>
    </div>
  )
}
