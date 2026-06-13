/* eslint-disable react-hooks/set-state-in-effect */
import { useCallback, useEffect, useRef, useState } from 'react'
import { getBuyerThread } from '@/api/chat'
import { CHAT_MESSAGE_PAGE_SIZE, type ChatMessageItem, type ChatThread } from '@/types/chat'
import type { ChatHub } from '@/hooks/useChatHub'

export interface UseChatThreadResult {
  thread: ChatThread | null
  messages: ChatMessageItem[]
  loading: boolean
  error: string | null
  hasOlderMessages: boolean
  loadOlder: () => Promise<void>
  appendMessage: (message: ChatMessageItem) => void
}

/**
 * Loads and live-updates a single conversation thread. Fetching the thread also
 * marks it read on the server, which triggers an unread-count broadcast.
 */
export function useChatThread(hub: ChatHub, conversationId: string | null): UseChatThreadResult {
  const [thread, setThread] = useState<ChatThread | null>(null)
  const [messages, setMessages] = useState<ChatMessageItem[]>([])
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const loadedPageCountRef = useRef(1)

  useEffect(() => {
    if (!conversationId) {
      setThread(null)
      setMessages([])
      setError(null)
      return
    }
    let active = true
    loadedPageCountRef.current = 1
    setLoading(true)
    setError(null)
    getBuyerThread(conversationId, { loadedPageCount: 1, pageSize: CHAT_MESSAGE_PAGE_SIZE })
      .then((data) => {
        if (!active) return
        setThread(data)
        setMessages(data.messages)
      })
      .catch((err: unknown) => {
        if (active) setError(err instanceof Error ? err.message : 'Unable to open this conversation right now.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => {
      active = false
    }
  }, [conversationId])

  useEffect(() => {
    if (!conversationId) return
    void hub.joinConversation(conversationId)
    return () => {
      void hub.leaveConversation(conversationId)
    }
  }, [hub, conversationId])

  const appendMessage = useCallback(
    (message: ChatMessageItem) => {
      setMessages((prev) => {
        if (message.conversationId !== conversationId) return prev
        if (prev.some((item) => item.id === message.id)) return prev
        return [...prev, message]
      })
    },
    [conversationId],
  )

  useEffect(() => {
    const unsubscribe = hub.subscribe({
      onMessageReceived: appendMessage,
      onConversationRead: (payload) => {
        if (payload.conversationId !== conversationId) return
        setMessages((prev) => prev.map((item) => (item.isRead ? item : { ...item, isRead: true })))
      },
    })
    return unsubscribe
  }, [hub, conversationId, appendMessage])

  const loadOlder = useCallback(async () => {
    if (!conversationId || !thread?.hasOlderMessages) return
    const nextPage = loadedPageCountRef.current + 1
    const data = await getBuyerThread(conversationId, { loadedPageCount: nextPage, pageSize: CHAT_MESSAGE_PAGE_SIZE })
    loadedPageCountRef.current = nextPage
    setThread(data)
    setMessages(data.messages)
  }, [conversationId, thread?.hasOlderMessages])

  return {
    thread,
    messages,
    loading,
    error,
    hasOlderMessages: thread?.hasOlderMessages ?? false,
    loadOlder,
    appendMessage,
  }
}
