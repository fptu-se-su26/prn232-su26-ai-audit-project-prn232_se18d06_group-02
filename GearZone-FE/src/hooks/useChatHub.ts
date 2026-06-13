/* eslint-disable react-hooks/set-state-in-effect */
import { useCallback, useEffect, useState } from 'react'
import type { HubConnection } from '@microsoft/signalr'
import {
  bindChatHandlers,
  createChatConnection,
  joinConversation as joinConversationOnHub,
  leaveConversation as leaveConversationOnHub,
  markHubConversationRead,
  sendHubMessage,
  startChatConnection,
  unbindChatHandlers,
  type ChatHubHandlers,
} from '@/lib/chatHub'
import type { SendChatMessageDto } from '@/types/chat'

export type ChatConnectionStatus = 'idle' | 'connecting' | 'connected' | 'disconnected'

export interface ChatHub {
  status: ChatConnectionStatus
  connection: HubConnection | null
  subscribe: (handlers: ChatHubHandlers) => () => void
  joinConversation: (conversationId: string) => Promise<void>
  leaveConversation: (conversationId: string) => Promise<void>
  sendViaHub: (dto: SendChatMessageDto) => Promise<void>
  markRead: (conversationId: string) => Promise<void>
}

/**
 * Owns a single SignalR chat connection. The connection is only published to
 * consumers (via state) once it has successfully started, so `subscribe`/invoke
 * helpers always run against a live connection and re-bind cleanly on reconnect.
 */
export function useChatHub(enabled: boolean): ChatHub {
  const [connection, setConnection] = useState<HubConnection | null>(null)
  const [status, setStatus] = useState<ChatConnectionStatus>('idle')

  useEffect(() => {
    if (!enabled) {
      setConnection(null)
      setStatus('idle')
      return
    }

    const conn = createChatConnection()
    let cancelled = false

    conn.onreconnecting(() => setStatus('connecting'))
    conn.onreconnected(() => setStatus('connected'))
    conn.onclose(() => {
      if (!cancelled) setStatus('disconnected')
    })

    setStatus('connecting')
    startChatConnection(conn)
      .then(() => {
        if (cancelled) return
        setConnection(conn)
        setStatus('connected')
      })
      .catch(() => {
        if (!cancelled) setStatus('disconnected')
      })

    return () => {
      cancelled = true
      setConnection(null)
      conn.stop().catch(() => undefined)
    }
  }, [enabled])

  const subscribe = useCallback(
    (handlers: ChatHubHandlers) => {
      if (!connection) return () => undefined
      bindChatHandlers(connection, handlers)
      return () => unbindChatHandlers(connection, handlers)
    },
    [connection],
  )

  const joinConversation = useCallback(
    async (conversationId: string) => {
      if (connection) await joinConversationOnHub(connection, conversationId)
    },
    [connection],
  )

  const leaveConversation = useCallback(
    async (conversationId: string) => {
      if (connection) await leaveConversationOnHub(connection, conversationId)
    },
    [connection],
  )

  const sendViaHub = useCallback(
    async (dto: SendChatMessageDto) => {
      if (!connection) throw new Error('Chat connection is not ready')
      await sendHubMessage(connection, dto)
    },
    [connection],
  )

  const markRead = useCallback(
    async (conversationId: string) => {
      if (connection) await markHubConversationRead(connection, conversationId)
    },
    [connection],
  )

  return { status, connection, subscribe, joinConversation, leaveConversation, sendViaHub, markRead }
}
