import apiClient, { unwrap } from './apiClient'

export const sellerApi = {
  getDashboard: () => apiClient.get('/seller/dashboard').then((res) => unwrap(res)),

  getRevenue: (params?: Record<string, unknown>) =>
    apiClient.get('/seller/revenue', { params }).then((res) => unwrap(res)),

  getStoreSettings: () => apiClient.get('/seller/store').then((res) => unwrap(res)),

  updateStoreSettings: (data: unknown) => apiClient.put('/seller/store', data),

  storeReviews: (page = 1) =>
    apiClient.get('/seller/store/reviews', { params: { page } }).then((res) => unwrap(res)),

  replyToReview: (reviewId: string, replyContent: string) =>
    apiClient.post(`/seller/store/reviews/${reviewId}/reply`, { replyContent }),

  products: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/products', { params }).then((res) => unwrap(res)),
    get: (id: string) => apiClient.get(`/seller/products/${id}`).then((res) => unwrap(res)),
    create: (data: unknown) => apiClient.post('/seller/products', data).then((res) => unwrap(res)),
    update: (id: string, data: unknown) => apiClient.put(`/seller/products/${id}`, data),
    delete: (id: string) => apiClient.delete(`/seller/products/${id}`),
    toggleStatus: (id: string) => apiClient.patch(`/seller/products/${id}/toggle-status`),
    metadata: () => apiClient.get('/seller/products/metadata').then((res) => unwrap(res)),
  },

  orders: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/orders', { params }).then((res) => unwrap(res)),
    get: (subOrderId: string) =>
      apiClient.get(`/seller/orders/${subOrderId}`).then((res) => unwrap(res)),
    approve: (subOrderId: string) => apiClient.post(`/seller/orders/${subOrderId}/approve`),
    reject: (subOrderId: string, reason?: string) =>
      apiClient.post(`/seller/orders/${subOrderId}/reject`, { reason }),
    markProcessing: (subOrderId: string) =>
      apiClient.post(`/seller/orders/${subOrderId}/mark-processing`),
    markDelivered: (subOrderId: string) =>
      apiClient.post(`/seller/orders/${subOrderId}/mark-delivered`),
  },

  vouchers: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/vouchers', { params }).then((res) => unwrap(res)),
    get: (id: string) => apiClient.get(`/seller/vouchers/${id}`).then((res) => unwrap(res)),
    create: (data: unknown) => apiClient.post('/seller/vouchers', data).then((res) => unwrap(res)),
    update: (id: string, data: unknown) => apiClient.put(`/seller/vouchers/${id}`, data),
    delete: (id: string) => apiClient.delete(`/seller/vouchers/${id}`),
    toggleStatus: (id: string) => apiClient.patch(`/seller/vouchers/${id}/toggle-status`),
  },
}
