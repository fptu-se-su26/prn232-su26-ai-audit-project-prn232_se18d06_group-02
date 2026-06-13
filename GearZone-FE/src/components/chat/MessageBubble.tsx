import { formatMessageTime } from '@/lib/chatFormat'
import type { ChatMessageItem } from '@/types/chat'

interface MessageBubbleProps {
  message: ChatMessageItem
  isOwn: boolean
}

export default function MessageBubble({ message, isOwn }: MessageBubbleProps) {
  return (
    <div className={`flex ${isOwn ? 'justify-end' : 'justify-start'}`}>
      <div
        className={`max-w-[78%] rounded-2xl px-3 py-2 ${
          isOwn ? 'bg-orange-50 ring-1 ring-orange-200' : 'bg-white ring-1 ring-gray-200'
        }`}
      >
        {!isOwn && <p className="mb-1 text-[11px] font-medium text-gray-500">{message.senderDisplayName}</p>}
        <p className="whitespace-pre-wrap break-words text-[13px] leading-6 text-gray-800">{message.content}</p>
        <div className="mt-1 flex items-center justify-end gap-2 text-[11px] text-gray-400">
          <span>{formatMessageTime(message.sentAt)}</span>
          {isOwn && message.isRead && <span className="font-medium text-secondary">Seen</span>}
        </div>
      </div>
    </div>
  )
}
