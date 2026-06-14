import { createContext } from 'react'
import type { ChatHub } from '@/hooks/useChatHub'

export interface ChatContextValue {
  enabled: boolean
  hub: ChatHub
  isOpen: boolean
  open: () => void
  close: () => void
  toggle: () => void
  activeConversationId: string | null
  setActiveConversationId: (conversationId: string | null) => void
  totalUnread: number
  openChatWithStore: (storeSlug: string) => Promise<void>
}

export const ChatContext = createContext<ChatContextValue | null>(null)
