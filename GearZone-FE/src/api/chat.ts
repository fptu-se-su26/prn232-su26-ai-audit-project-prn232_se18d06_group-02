import apiClient, { unwrap } from '@/api/apiClient'
import type { PagedResult } from '@/types/catalog'
import type {
  ChatBootstrapQuery,
  ChatConversationListItem,
  ChatConversationUpdate,
  ChatCounterpartScopeOption,
  ChatInboxQuery,
  ChatSendMessageResult,
  ChatThread,
  ChatThreadQuery,
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

export async function getBuyerInbox(query: ChatInboxQuery = {}) {
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

export async function getBuyerThread(conversationId: string, query: ChatThreadQuery = {}) {
  const response = await apiClient.get(`/chat/buyer/conversations/${conversationId}/thread`, {
    params: buildParams({
      loadedPageCount: query.loadedPageCount,
      pageSize: query.pageSize,
      productSlug: query.productSlug,
    }),
  })
  return unwrap<ChatThread>(response)
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
