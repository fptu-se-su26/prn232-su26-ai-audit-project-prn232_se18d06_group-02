import type { PagedResult } from '@/types/catalog'

export type ChatFilter = 'all' | 'unread'

export interface ChatConversationListItem {
  conversationId: string
  storeId: string
  storeName: string
  storeSlug: string
  storeLogoUrl?: string | null
  buyerUserId: string
  buyerDisplayName: string
  buyerAvatarUrl?: string | null
  counterpartName: string
  counterpartAvatarUrl?: string | null
  counterpartSubtitle: string
  lastMessagePreview: string
  lastMessageSenderUserId: string
  lastMessageAt: string
  unreadCount: number
  hasMessages: boolean
}

export interface ChatMessageItem {
  id: string
  conversationId: string
  senderUserId: string
  senderDisplayName: string
  senderAvatarUrl?: string | null
  content: string
  sentAt: string
  isRead: boolean
  readAt?: string | null
}

export interface ChatProductContext {
  productId: string
  storeId: string
  storeName: string
  storeSlug: string
  productName: string
  productSlug: string
  productImageUrl?: string | null
  storeLogoUrl?: string | null
  price: number
  isInStock: boolean
}

export interface ChatContextOrder {
  orderId: string
  orderCode: number
  status: string
  createdAt: string
  totalAmount: number
}

export interface ChatThread {
  conversationId: string
  storeId: string
  storeName: string
  storeSlug: string
  storeLogoUrl?: string | null
  buyerUserId: string
  buyerDisplayName: string
  buyerAvatarUrl?: string | null
  counterpartName: string
  counterpartAvatarUrl?: string | null
  isSellerView: boolean
  loadedPageCount: number
  pageSize: number
  hasOlderMessages: boolean
  messages: ChatMessageItem[]
  recentOrders: ChatContextOrder[]
  activeProductContext?: ChatProductContext | null
}

export interface ChatCounterpartScopeOption {
  key: string
  label: string
  avatarUrl?: string | null
}

export interface ChatWidgetBootstrap {
  activeConversationId?: string | null
  filter: ChatFilter
  searchTerm?: string | null
  counterpartScopeKey?: string | null
  totalUnreadCount: number
  requestedTargetUnavailable: boolean
  counterpartScopeOptions: ChatCounterpartScopeOption[]
  conversations: PagedResult<ChatConversationListItem>
  activeThread?: ChatThread | null
}

export interface ChatConversationUpdate {
  isSellerView: boolean
  totalUnreadCount: number
  conversation: ChatConversationListItem
}

export interface SendChatMessageDto {
  conversationId: string
  content: string
}

export interface ChatSendMessageResult {
  conversationId: string
  buyerUserId: string
  storeOwnerUserId: string
  message: ChatMessageItem
}

export interface ChatInboxQuery {
  filter?: ChatFilter
  searchTerm?: string
  counterpartScopeKey?: string
  pageNumber?: number
  pageSize?: number
}

export interface ChatThreadQuery {
  loadedPageCount?: number
  pageSize?: number
  productSlug?: string
}

export interface ChatBootstrapQuery {
  conversationId?: string
  storeSlug?: string
  productSlug?: string
  filter?: ChatFilter
  searchTerm?: string
  counterpartScopeKey?: string
  loadedPageCount?: number
  inboxPageSize?: number
  messagePageSize?: number
}

export interface UnreadCountsUpdatedPayload {
  isSellerView: boolean
  totalUnreadCount: number
}

export interface ConversationReadPayload {
  conversationId: string
  readByUserId: string
}

export const CHAT_INBOX_PAGE_SIZE = 20
export const CHAT_MESSAGE_PAGE_SIZE = 30
export const CHAT_MESSAGE_MAX_LENGTH = 2000
