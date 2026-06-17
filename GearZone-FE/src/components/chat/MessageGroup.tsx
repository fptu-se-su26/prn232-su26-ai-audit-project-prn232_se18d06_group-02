import MessageBubble from '@/components/chat/MessageBubble'
import MessageDateSeparator from '@/components/chat/MessageDateSeparator'
import type { ChatMessageItem } from '@/types/chat'

interface MessageGroupProps {
  label: string
  messages: ChatMessageItem[]
  selfUserId: string
}

export default function MessageGroup({ label, messages, selfUserId }: MessageGroupProps) {
  return (
    <div>
      <MessageDateSeparator label={label} />
      <div className="space-y-2">
        {messages.map((message) => (
          <MessageBubble key={message.id} message={message} isOwn={message.senderUserId === selfUserId} />
        ))}
      </div>
    </div>
  )
}
