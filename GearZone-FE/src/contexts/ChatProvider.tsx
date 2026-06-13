import { useCallback, useMemo, type ReactNode } from 'react'
import { ChatContext, type ChatContextValue } from '@/contexts/chat-context'
import { useAuth } from '@/contexts/useAuth'
import { useChatHub } from '@/hooks/useChatHub'
import { useChatUnread } from '@/hooks/useChatUnread'
import { useChatWidget } from '@/hooks/useChatWidget'
import { ensureBuyerConversation } from '@/api/chat'

export default function ChatProvider({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  const enabled = !!user && user.role === 'Customer'
  const hub = useChatHub(enabled)
  const { isOpen, open, close, toggle, activeConversationId, setActiveConversationId } = useChatWidget()
  const totalUnread = useChatUnread(hub, enabled)

  const openChatWithStore = useCallback(
    async (storeSlug: string) => {
      open()
      try {
        const conversationId = await ensureBuyerConversation(storeSlug)
        setActiveConversationId(conversationId)
      } catch {
        /* leave the widget on the conversation list if the shop can't be opened */
      }
    },
    [open, setActiveConversationId],
  )

  const value = useMemo<ChatContextValue>(
    () => ({
      enabled,
      hub,
      isOpen,
      open,
      close,
      toggle,
      activeConversationId,
      setActiveConversationId,
      totalUnread,
      openChatWithStore,
    }),
    [
      enabled,
      hub,
      isOpen,
      open,
      close,
      toggle,
      activeConversationId,
      setActiveConversationId,
      totalUnread,
      openChatWithStore,
    ],
  )

  return <ChatContext.Provider value={value}>{children}</ChatContext.Provider>
}
