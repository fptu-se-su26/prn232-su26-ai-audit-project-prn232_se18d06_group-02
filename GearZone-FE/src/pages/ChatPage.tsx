import ChatInboxLayout from '@/components/chat/ChatInboxLayout'
import EmptyState from '@/components/ui/EmptyState'
import { useChatContext } from '@/contexts/useChatContext'

export default function ChatPage() {
  const { enabled } = useChatContext()

  if (!enabled) {
    return (
      <div className="mx-auto max-w-6xl px-4 py-10">
        <EmptyState icon="lock" title="Messages are available for customer accounts" />
      </div>
    )
  }

  return (
    <div className="mx-auto h-[calc(100vh-8rem)] max-w-6xl px-4 py-6">
      <div className="h-full overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
        <ChatInboxLayout />
      </div>
    </div>
  )
}
