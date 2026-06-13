import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import type {
  ChatConversationListItem,
  ChatMessageItem,
  ConversationReadPayload,
  SendChatMessageDto,
  UnreadCountsUpdatedPayload,
} from '@/types/chat'

export interface ChatHubHandlers {
  onMessageReceived?: (message: ChatMessageItem) => void
  onConversationUpdated?: (conversation: ChatConversationListItem) => void
  onUnreadCountsUpdated?: (payload: UnreadCountsUpdatedPayload) => void
  onConversationRead?: (payload: ConversationReadPayload) => void
}

const HUB_URL = '/hubs/chat'

export function createChatConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(HUB_URL)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()
}

export function bindChatHandlers(connection: HubConnection, handlers: ChatHubHandlers) {
  if (handlers.onMessageReceived) connection.on('MessageReceived', handlers.onMessageReceived)
  if (handlers.onConversationUpdated) connection.on('ConversationUpdated', handlers.onConversationUpdated)
  if (handlers.onUnreadCountsUpdated) connection.on('UnreadCountsUpdated', handlers.onUnreadCountsUpdated)
  if (handlers.onConversationRead) connection.on('ConversationRead', handlers.onConversationRead)
}

export function unbindChatHandlers(connection: HubConnection, handlers: ChatHubHandlers) {
  if (handlers.onMessageReceived) connection.off('MessageReceived', handlers.onMessageReceived)
  if (handlers.onConversationUpdated) connection.off('ConversationUpdated', handlers.onConversationUpdated)
  if (handlers.onUnreadCountsUpdated) connection.off('UnreadCountsUpdated', handlers.onUnreadCountsUpdated)
  if (handlers.onConversationRead) connection.off('ConversationRead', handlers.onConversationRead)
}

export async function startChatConnection(connection: HubConnection) {
  if (connection.state === HubConnectionState.Disconnected) {
    await connection.start()
  }
}

export async function joinConversation(connection: HubConnection, conversationId: string) {
  if (connection.state === HubConnectionState.Connected) {
    await connection.invoke('JoinConversation', conversationId)
  }
}

export async function leaveConversation(connection: HubConnection, conversationId: string) {
  if (connection.state === HubConnectionState.Connected) {
    await connection.invoke('LeaveConversation', conversationId)
  }
}

export async function sendHubMessage(connection: HubConnection, dto: SendChatMessageDto) {
  await connection.invoke('SendMessage', dto)
}

export async function markHubConversationRead(connection: HubConnection, conversationId: string) {
  if (connection.state === HubConnectionState.Connected) {
    await connection.invoke('MarkConversationRead', conversationId)
  }
}
