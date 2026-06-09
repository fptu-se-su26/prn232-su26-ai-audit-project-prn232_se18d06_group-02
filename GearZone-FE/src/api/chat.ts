import apiClient, { unwrap } from '@/api/apiClient'

export const chatApi = {
  buyer: {
    bootstrap: (params?: Record<string, unknown>) =>
      apiClient.get('/chat/buyer/bootstrap', { params }).then((res) => unwrap(res)),

    inbox: (params?: Record<string, unknown>) =>
      apiClient.get('/chat/buyer/inbox', { params }).then((res) => unwrap(res)),

    thread: (id: string, params?: Record<string, unknown>) =>
      apiClient.get(`/chat/buyer/conversations/${id}/thread`, { params }).then((res) => unwrap(res)),

    conversationUpdate: (id: string) =>
      apiClient.get(`/chat/buyer/conversations/${id}/update`).then((res) => unwrap(res)),

    unread: () => apiClient.get('/chat/buyer/unread').then((res) => unwrap(res)),

    scopeOptions: () => apiClient.get('/chat/buyer/scope-options').then((res) => unwrap(res)),

    ensureConversation: (storeSlug: string) =>
      apiClient.post('/chat/buyer/conversations/ensure', { storeSlug }).then((res) => unwrap(res)),
  },

  seller: {
    inbox: (params?: Record<string, unknown>) =>
      apiClient.get('/chat/seller/inbox', { params }).then((res) => unwrap(res)),

    thread: (id: string, params?: Record<string, unknown>) =>
      apiClient.get(`/chat/seller/conversations/${id}/thread`, { params }).then((res) => unwrap(res)),

    conversationUpdate: (id: string) =>
      apiClient.get(`/chat/seller/conversations/${id}/update`).then((res) => unwrap(res)),

    unread: () => apiClient.get('/chat/seller/unread').then((res) => unwrap(res)),

    scopeOptions: () => apiClient.get('/chat/seller/scope-options').then((res) => unwrap(res)),

    ensureFromOrder: (subOrderId: string) =>
      apiClient
        .post('/chat/seller/conversations/ensure-from-order', { subOrderId })
        .then((res) => unwrap(res)),
  },

  send: (conversationId: string, content: string) =>
    apiClient.post('/chat/send', { conversationId, content }).then((res) => unwrap(res)),

  markRead: (id: string) =>
    apiClient.patch(`/chat/conversations/${id}/mark-read`).then((res) => unwrap(res)),
}
