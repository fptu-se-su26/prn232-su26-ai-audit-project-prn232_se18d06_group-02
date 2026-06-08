import apiClient, { unwrap } from './apiClient';

export const adminApi = {
  getDashboard: (params?: Record<string, unknown>) =>
    apiClient.get('/admin/dashboard', { params }).then(res => unwrap(res)),

  getSettings: () => apiClient.get('/admin/settings').then(res => unwrap(res)),
  updateSettings: (data: unknown) => apiClient.put('/admin/settings', data),

  getWallet: (params?: Record<string, unknown>) =>
    apiClient.get('/admin/wallet', { params }).then(res => unwrap(res)),
  topUp: (data: unknown) => apiClient.post('/admin/wallet/top-up', data),

  getTransactions: (params?: Record<string, unknown>) =>
    apiClient.get('/admin/transactions', { params }).then(res => unwrap(res)),

  brands: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/brands', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/admin/brands/${id}`).then(res => unwrap(res)),
    all: () => apiClient.get('/admin/brands/all').then(res => unwrap(res)),
    create: (data: unknown) => apiClient.post('/admin/brands', data).then(res => unwrap(res)),
    update: (id: string, data: unknown) => apiClient.put(`/admin/brands/${id}`, data),
    delete: (id: string) => apiClient.delete(`/admin/brands/${id}`),
    approve: (id: string) => apiClient.post(`/admin/brands/${id}/approve`),
    reject: (id: string) => apiClient.post(`/admin/brands/${id}/reject`),
  },

  categories: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/categories', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/admin/categories/${id}`).then(res => unwrap(res)),
    getAttributes: (id: string) => apiClient.get(`/admin/categories/${id}/attributes`).then(res => unwrap(res)),
    create: (data: unknown) => apiClient.post('/admin/categories', data).then(res => unwrap(res)),
    update: (id: string, data: unknown) => apiClient.put(`/admin/categories/${id}`, data),
    delete: (id: string) => apiClient.delete(`/admin/categories/${id}`),
  },

  orders: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/orders', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/admin/orders/${id}`).then(res => unwrap(res)),
  },

  products: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/products', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/admin/products/${id}`).then(res => unwrap(res)),
    metadata: () => apiClient.get('/admin/products/metadata').then(res => unwrap(res)),
    approve: (id: string) => apiClient.post(`/admin/products/${id}/approve`),
    reject: (id: string, reason?: string) => apiClient.post(`/admin/products/${id}/reject`, { reason }),
    suspend: (id: string, reason?: string) => apiClient.post(`/admin/products/${id}/suspend`, { reason }),
    delete: (id: string, reason?: string) => apiClient.delete(`/admin/products/${id}`, { data: { reason } }),
    activate: (id: string) => apiClient.post(`/admin/products/${id}/activate`),
    deactivate: (id: string) => apiClient.post(`/admin/products/${id}/deactivate`),
    bulkUpdateStatus: (productIds: string[], action: string, reason?: string) =>
      apiClient.post('/admin/products/bulk-update-status', { productIds, action, reason }),
  },

  storeApplications: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/store-applications', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/admin/store-applications/${id}`).then(res => unwrap(res)),
    approve: (id: string) => apiClient.post(`/admin/store-applications/${id}/approve`),
    reject: (id: string, reason: string) =>
      apiClient.post(`/admin/store-applications/${id}/reject`, { reason }),
    requestInfo: (id: string, note: string) =>
      apiClient.post(`/admin/store-applications/${id}/request-info`, { note }),
  },

  stores: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/stores', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/admin/stores/${id}`).then(res => unwrap(res)),
    changeStatus: (id: string, status: string, reason?: string) =>
      apiClient.post(`/admin/stores/${id}/change-status`, { status, reason }),
    activate: (id: string) => apiClient.post(`/admin/stores/${id}/activate`),
    deactivate: (id: string) => apiClient.post(`/admin/stores/${id}/deactivate`),
  },

  users: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/users', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/admin/users/${id}`).then(res => unwrap(res)),
    create: (data: unknown) => apiClient.post('/admin/users', data),
    update: (id: string, data: unknown) => apiClient.put(`/admin/users/${id}`, data),
    delete: (id: string) => apiClient.delete(`/admin/users/${id}`),
    restore: (id: string) => apiClient.post(`/admin/users/${id}/restore`),
    lock: (id: string) => apiClient.post(`/admin/users/${id}/lock`),
    unlock: (id: string) => apiClient.post(`/admin/users/${id}/unlock`),
    changeRole: (id: string, role: string) =>
      apiClient.post(`/admin/users/${id}/change-role`, { role }),
  },

  vouchers: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/vouchers', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/admin/vouchers/${id}`).then(res => unwrap(res)),
    create: (data: unknown) => apiClient.post('/admin/vouchers', data).then(res => unwrap(res)),
    update: (id: string, data: unknown) => apiClient.put(`/admin/vouchers/${id}`, data),
    toggleStatus: (id: string) => apiClient.patch(`/admin/vouchers/${id}/toggle-status`),
    delete: (id: string) => apiClient.delete(`/admin/vouchers/${id}`),
  },

  payouts: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/payouts', { params }).then(res => unwrap(res)),
    listBatches: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/payouts/batches', { params }).then(res => unwrap(res)),
    getBatch: (id: string) => apiClient.get(`/admin/payouts/batches/${id}`).then(res => unwrap(res)),
    approveBatch: (id: string) => apiClient.post(`/admin/payouts/batches/${id}/approve`),
    holdBatch: (id: string, reason: string) =>
      apiClient.post(`/admin/payouts/batches/${id}/hold`, { reason }),
    processBatch: (id: string, batchCode: string) =>
      apiClient.post(`/admin/payouts/batches/${id}/process`, { batchCode }),
    retryTransaction: (batchId: string, txId: string) =>
      apiClient.post(`/admin/payouts/batches/${batchId}/transactions/${txId}/retry`),
    excludeTransaction: (batchId: string, txId: string, reason: string) =>
      apiClient.post(`/admin/payouts/batches/${batchId}/transactions/${txId}/exclude`, { reason }),
    getTransaction: (id: string) =>
      apiClient.get(`/admin/payouts/transactions/${id}`).then(res => unwrap(res)),
    sellerSummary: (params?: Record<string, unknown>) =>
      apiClient.get('/admin/payouts/seller-summary', { params }).then(res => unwrap(res)),
    processBulk: (data: { storeIds: string[]; rangeType?: string; customStart?: string; customEnd?: string }) =>
      apiClient.post('/admin/payouts/process-bulk', data).then(res => unwrap(res)),
    processSingle: (storeId: string, data: { rangeType?: string; customStart?: string; customEnd?: string }) =>
      apiClient.post(`/admin/payouts/process-single/${storeId}`, data).then(res => unwrap(res)),
    process: (id: string) => apiClient.post(`/admin/payouts/${id}/process`),
    reject: (id: string, reason: string) =>
      apiClient.post(`/admin/payouts/${id}/reject`, { reason }),
  },

  banks: () => apiClient.get('/banks').then(res => unwrap(res)),
  mapAutocomplete: (input: string) =>
    apiClient.get('/maps/autocomplete', { params: { input } }).then(res => unwrap(res)),
  mapPlaceDetail: (placeId: string) =>
    apiClient.get('/maps/place-detail', { params: { placeId } }).then(res => unwrap(res)),
};
