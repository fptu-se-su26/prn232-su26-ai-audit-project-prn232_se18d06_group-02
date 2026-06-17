/* eslint-disable react-hooks/set-state-in-effect */
import { useCallback, useEffect, useState } from 'react'
import { readSession, writeSession } from '@/lib/sessionStore'

const WIDGET_STATE_KEY = 'gearzone_buyer_chat_widget_state'

interface PersistedWidgetState {
  open: boolean
}

export interface ChatWidgetState {
  isOpen: boolean
  open: () => void
  close: () => void
  toggle: () => void
  activeConversationId: string | null
  setActiveConversationId: (conversationId: string | null) => void
}

/** Open/closed widget state plus the active conversation, persisted in sessionStorage. */
export function useChatWidget(): ChatWidgetState {
  const [isOpen, setIsOpen] = useState(false)
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null)

  useEffect(() => {
    const persisted = readSession<PersistedWidgetState>(WIDGET_STATE_KEY, { open: false })
    setIsOpen(persisted.open)
  }, [])

  useEffect(() => {
    writeSession<PersistedWidgetState>(WIDGET_STATE_KEY, { open: isOpen })
  }, [isOpen])

  const open = useCallback(() => setIsOpen(true), [])
  const close = useCallback(() => setIsOpen(false), [])
  const toggle = useCallback(() => setIsOpen((value) => !value), [])

  return { isOpen, open, close, toggle, activeConversationId, setActiveConversationId }
}
