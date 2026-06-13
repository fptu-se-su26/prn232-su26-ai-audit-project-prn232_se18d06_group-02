import Avatar from '@/components/ui/Avatar'
import UnreadBadge from '@/components/ui/UnreadBadge'
import { formatListTime } from '@/lib/chatFormat'
import type { ChatConversationListItem as ChatConversation } from '@/types/chat'

interface ConversationListItemProps {
  conversation: ChatConversation
  active: boolean
  onSelect: (conversationId: string) => void
}

export default function ConversationListItem({ conversation, active, onSelect }: ConversationListItemProps) {
  return (
    <button
      type="button"
      onClick={() => onSelect(conversation.conversationId)}
      className={`flex w-full items-center gap-3 px-4 py-3 text-left transition ${
        active ? 'bg-orange-50' : 'hover:bg-gray-50'
      }`}
    >
      <Avatar name={conversation.counterpartName} src={conversation.counterpartAvatarUrl} size={44} />
      <div className="min-w-0 flex-1">
        <div className="flex items-center justify-between gap-2">
          <span className="truncate text-sm font-semibold text-gray-800">{conversation.counterpartName}</span>
          <span className="shrink-0 text-[11px] text-gray-400">{formatListTime(conversation.lastMessageAt)}</span>
        </div>
        <div className="mt-0.5 flex items-center justify-between gap-2">
          <span className="truncate text-[13px] text-gray-500">
            {conversation.lastMessagePreview || 'No messages yet'}
          </span>
          <UnreadBadge count={conversation.unreadCount} />
        </div>
      </div>
    </button>
  )
}
