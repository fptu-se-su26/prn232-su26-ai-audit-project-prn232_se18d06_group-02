import { useRef } from 'react'
import ChatEmptyState from '@/components/chat/ChatEmptyState'
import ConversationListItem from '@/components/chat/ConversationListItem'
import type { ChatConversationListItem as ChatConversation } from '@/types/chat'

interface ConversationListProps {
  items: ChatConversation[]
  activeConversationId: string | null
  loading: boolean
  hasMore: boolean
  onSelect: (conversationId: string) => void
  onLoadMore: () => void
}

const SCROLL_THRESHOLD = 96

export default function ConversationList({
  items,
  activeConversationId,
  loading,
  hasMore,
  onSelect,
  onLoadMore,
}: ConversationListProps) {
  const scrollRef = useRef<HTMLDivElement | null>(null)

  function handleScroll() {
    const element = scrollRef.current
    if (!element || !hasMore) return
    if (element.scrollHeight - element.scrollTop - element.clientHeight <= SCROLL_THRESHOLD) {
      onLoadMore()
    }
  }

  if (!loading && items.length === 0) {
    return (
      <ChatEmptyState
        title="No conversations yet"
        description="Open any shop and press Chat to start a conversation."
      />
    )
  }

  return (
    <div ref={scrollRef} onScroll={handleScroll} className="h-full overflow-y-auto">
      {items.map((conversation) => (
        <ConversationListItem
          key={conversation.conversationId}
          conversation={conversation}
          active={conversation.conversationId === activeConversationId}
          onSelect={onSelect}
        />
      ))}
    </div>
  )
}
