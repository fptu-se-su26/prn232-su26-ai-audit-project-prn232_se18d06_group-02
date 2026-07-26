import { useState } from 'react'
import ChatComposer from '@/components/chat/ChatComposer'
import ChatThreadHeader from '@/components/chat/ChatThreadHeader'
import MessageList from '@/components/chat/MessageList'
import ProductContextCard from '@/components/chat/ProductContextCard'
import EmptyState from '@/components/ui/EmptyState'
import ErrorState from '@/components/ui/ErrorState'
import { sendMessage } from '@/api/chat'
import { useMessageDraft } from '@/hooks/useMessageDraft'
import type { ChatMessageItem, ChatThread } from '@/types/chat'

interface ChatThreadPaneProps {
  conversationId: string | null
  thread: ChatThread | null
  messages: ChatMessageItem[]
  loading: boolean
  error: string | null
  hasOlderMessages: boolean
  onLoadOlder: () => void
  onSent: (message: ChatMessageItem) => void
  onBack?: () => void
}

export default function ChatThreadPane({
  conversationId,
  thread,
  messages,
  loading,
  error,
  hasOlderMessages,
  onLoadOlder,
  onSent,
  onBack,
}: ChatThreadPaneProps) {
  const [draft, setDraft, clearDraft] = useMessageDraft(conversationId)
  const [sending, setSending] = useState(false)
  const [sendError, setSendError] = useState<string | null>(null)

  async function handleSend() {
    if (!conversationId) return
    const content = draft.trim()
    if (!content) return
    setSending(true)
    setSendError(null)
    try {
      const result = await sendMessage({ conversationId, content })
      onSent(result.message)
      clearDraft()
    } catch (err: unknown) {
      setSendError(err instanceof Error ? err.message : 'Message could not be sent.')
    } finally {
      setSending(false)
    }
  }

  if (!conversationId) {
    return (
      <EmptyState
        icon="forum"
        title="Select a conversation"
        description="Choose a chat on the left to start messaging."
      />
    )
  }

  if (error) {
    return <ErrorState title="Unable to open chat" message={error} />
  }

  const selfUserId = thread?.buyerUserId ?? ''

  return (
    <div className="flex h-full flex-col">
      {thread && (
        <ChatThreadHeader name={thread.counterpartName} avatarUrl={thread.counterpartAvatarUrl} onBack={onBack} />
      )}
      {thread?.activeProductContext && <ProductContextCard product={thread.activeProductContext} />}
      {loading && messages.length === 0 ? (
        <div className="flex flex-1 items-center justify-center bg-gray-50">
          <span className="material-symbols-outlined animate-spin text-[24px] text-secondary">progress_activity</span>
        </div>
      ) : (
        <MessageList
          messages={messages}
          selfUserId={selfUserId}
          hasOlderMessages={hasOlderMessages}
          onLoadOlder={onLoadOlder}
        />
      )}
      {sendError && <p className="bg-white px-4 pb-1 text-[11px] text-red-500">{sendError}</p>}
      <ChatComposer value={draft} onChange={setDraft} onSend={handleSend} sending={sending} />
    </div>
  )
}
