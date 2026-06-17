import { useAutoScroll } from '@/hooks/useAutoScroll'
import MessageGroup from '@/components/chat/MessageGroup'
import { groupMessagesByDate } from '@/lib/chatFormat'
import type { ChatMessageItem } from '@/types/chat'

interface MessageListProps {
  messages: ChatMessageItem[]
  selfUserId: string
  hasOlderMessages: boolean
  onLoadOlder: () => void
}

const TOP_THRESHOLD = 72

export default function MessageList({ messages, selfUserId, hasOlderMessages, onLoadOlder }: MessageListProps) {
  const lastMessageId = messages.length > 0 ? messages[messages.length - 1].id : ''
  const scrollRef = useAutoScroll<HTMLDivElement>(lastMessageId)
  const groups = groupMessagesByDate(messages)

  function handleScroll() {
    const element = scrollRef.current
    if (!element || !hasOlderMessages) return
    if (element.scrollTop <= TOP_THRESHOLD) onLoadOlder()
  }

  return (
    <div ref={scrollRef} onScroll={handleScroll} className="flex-1 overflow-y-auto bg-gray-50 px-4 py-3">
      {groups.map((group) => (
        <MessageGroup key={group.dateKey} label={group.label} messages={group.messages} selfUserId={selfUserId} />
      ))}
    </div>
  )
}
