import apiClient, { unwrap } from '@/api/apiClient'
import type { PagedResult } from '@/types/catalog'
import type {
  ChatBootstrapQuery,
  ChatConversationListItem,
  ChatConversationUpdate,
  ChatCounterpartScopeOption,
  ChatInboxQuery as BuyerChatInboxQuery,
  ChatSendMessageResult,
  ChatThread as BuyerChatThread,
  ChatThreadQuery as BuyerChatThreadQuery,
  ChatWidgetBootstrap,
  SendChatMessageDto,
} from '@/types/chat'

type ParamValue = string | number | undefined | null

function buildParams(entries: Record<string, ParamValue>) {
  const params = new URLSearchParams()
  Object.entries(entries).forEach(([key, value]) => {
    if (value !== undefined && value !== null && value !== '') {
      params.set(key, String(value))
    }
  })
  return params
}

export async function getBuyerBootstrap(query: ChatBootstrapQuery = {}) {
  const response = await apiClient.get('/chat/buyer/bootstrap', {
    params: buildParams({
      conversationId: query.conversationId,
      storeSlug: query.storeSlug,
      productSlug: query.productSlug,
      filter: query.filter,
      searchTerm: query.searchTerm,
      counterpartScopeKey: query.counterpartScopeKey,
      loadedPageCount: query.loadedPageCount,
      inboxPageSize: query.inboxPageSize,
      messagePageSize: query.messagePageSize,
    }),
  })
  return unwrap<ChatWidgetBootstrap>(response)
}

export async function getBuyerInbox(query: BuyerChatInboxQuery = {}) {
  const response = await apiClient.get('/chat/buyer/inbox', {
    params: buildParams({
      filter: query.filter,
      searchTerm: query.searchTerm,
      counterpartScopeKey: query.counterpartScopeKey,
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
    }),
  })
  return unwrap<PagedResult<ChatConversationListItem>>(response)
}

export async function getBuyerThread(conversationId: string, query: BuyerChatThreadQuery = {}) {
  const response = await apiClient.get(`/chat/buyer/conversations/${conversationId}/thread`, {
    params: buildParams({
      loadedPageCount: query.loadedPageCount,
      pageSize: query.pageSize,
      productSlug: query.productSlug,
    }),
  })
  return unwrap<BuyerChatThread>(response)
}

export async function getBuyerConversationUpdate(conversationId: string) {
  const response = await apiClient.get(`/chat/buyer/conversations/${conversationId}/update`)
  return unwrap<ChatConversationUpdate>(response)
}

export async function getBuyerUnread() {
  const response = await apiClient.get('/chat/buyer/unread')
  return unwrap<{ unreadCount: number }>(response).unreadCount
}

export async function getBuyerScopeOptions() {
  const response = await apiClient.get('/chat/buyer/scope-options')
  return unwrap<ChatCounterpartScopeOption[]>(response)
}

export async function ensureBuyerConversation(storeSlug: string) {
  const response = await apiClient.post('/chat/buyer/conversations/ensure', { storeSlug })
  return unwrap<{ conversationId: string }>(response).conversationId
}

export async function sendMessage(dto: SendChatMessageDto) {
  const response = await apiClient.post('/chat/send', dto)
  return unwrap<ChatSendMessageResult>(response)
}

export async function markConversationRead(conversationId: string) {
  const response = await apiClient.patch(`/chat/conversations/${conversationId}/mark-read`)
  return unwrap<{ markedCount: number }>(response).markedCount
}

export interface ChatInboxQuery {
  Filter?: string
  SearchTerm?: string
  CounterpartScopeKey?: string
  PageNumber?: number
  PageSize?: number
}

export interface ChatThreadQuery {
  LoadedPageCount?: number
  PageSize?: number
}

export interface ChatConversation {
  conversationId: string
  storeId: string
  storeName: string
  storeSlug: string
  storeLogoUrl?: string
  buyerUserId: string
  buyerDisplayName: string
  buyerAvatarUrl?: string
  counterpartName: string
  counterpartAvatarUrl?: string
  counterpartSubtitle: string
  lastMessagePreview: string
  lastMessageSenderUserId: string
  lastMessageAt: string
  unreadCount: number
  hasMessages: boolean
}

export interface ChatScopeOption {
  value: string
  label: string
  subtitle?: string
  avatarUrl?: string
}

export interface ChatMessage {
  id: string
  conversationId: string
  senderUserId: string
  senderDisplayName: string
  senderAvatarUrl?: string
  content: string
  sentAt: string
  isRead: boolean
  readAt?: string
}

export interface ChatRecentOrder {
  subOrderId: string
  orderCode: number
  createdAt: string
  deliveredAt?: string
  status: string
  subtotal: number
  itemCount: number
  productPreview: string
}

export interface ChatThread {
  conversationId: string
  storeId: string
  storeName: string
  storeSlug: string
  storeLogoUrl?: string
  buyerUserId: string
  buyerDisplayName: string
  buyerAvatarUrl?: string
  counterpartName: string
  counterpartAvatarUrl?: string
  counterpartSubtitle?: string
  isSellerView: boolean
  loadedPageCount: number
  pageSize: number
  hasOlderMessages: boolean
  messages: ChatMessage[]
  recentOrders: ChatRecentOrder[]
}

export const chatApi = {
  sellerInbox: (params?: ChatInboxQuery) =>
    apiClient.get('/chat/seller/inbox', { params }).then((response) => unwrap<PagedResult<ChatConversation>>(response)),

  sellerThread: (conversationId: string, params?: ChatThreadQuery) =>
    apiClient
      .get(`/chat/seller/conversations/${conversationId}/thread`, { params })
      .then((response) => unwrap<ChatThread>(response)),

  sellerUnread: () =>
    apiClient.get('/chat/seller/unread').then((response) => unwrap<{ unreadCount: number }>(response)),

  sellerScopeOptions: () =>
    apiClient.get('/chat/seller/scope-options').then((response) => unwrap<ChatScopeOption[]>(response)),

  ensureSellerConversationFromOrder: (subOrderId: string) =>
    apiClient
      .post('/chat/seller/conversations/ensure-from-order', { subOrderId })
      .then((response) => unwrap<{ conversationId: string }>(response)),

  send: (conversationId: string, content: string) =>
    apiClient
      .post('/chat/send', { conversationId, content })
      .then((response) => unwrap<{ conversationId: string; message: ChatMessage }>(response)),
}
