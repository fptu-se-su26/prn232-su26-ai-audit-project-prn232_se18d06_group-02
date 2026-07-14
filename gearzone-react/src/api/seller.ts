import apiClient, { unwrap } from './apiClient';

export const sellerApi = {
  getDashboard: () => apiClient.get('/seller/dashboard').then(res => unwrap(res)),
  getRevenue: (params?: Record<string, unknown>) =>
    apiClient.get('/seller/revenue', { params }).then(res => unwrap(res)),
  getStoreSettings: () => apiClient.get('/seller/store').then(res => unwrap(res)),
  updateStoreSettings: (data: unknown) => apiClient.put('/seller/store', data),
  storeReviews: (page = 1) =>
    apiClient.get('/seller/store/reviews', { params: { page } }).then(res => unwrap(res)),
  replyToReview: (reviewId: string, replyContent: string) =>
    apiClient.post(`/seller/store/reviews/${reviewId}/reply`, { replyContent }),

  products: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/products', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/seller/products/${id}`).then(res => unwrap(res)),
    create: (data: unknown) => apiClient.post('/seller/products', data).then(res => unwrap(res)),
    update: (id: string, data: unknown) => apiClient.put(`/seller/products/${id}`, data),
    delete: (id: string) => apiClient.delete(`/seller/products/${id}`),
    toggleStatus: (id: string) => apiClient.patch(`/seller/products/${id}/toggle-status`),
    metadata: () => apiClient.get('/seller/products/metadata').then(res => unwrap(res)),
    attributes: (categoryId: number) =>
      apiClient.get('/seller/products/attributes', { params: { categoryId } }).then(res => unwrap(res)),
    specifications: (categoryId: number) =>
      apiClient.get('/seller/products/specifications', { params: { categoryId } }).then(res => unwrap(res)),
    createSpecification: (data: { categoryId: number; name: string; unit?: string; valueType?: string }) =>
      apiClient.post('/seller/products/specifications', data).then(res => unwrap(res)),
    createBrand: (name: string) =>
      apiClient.post('/seller/products/brands', { name }).then(res => unwrap(res)),
    createCategory: (name: string) =>
      apiClient.post('/seller/products/categories', { name }).then(res => unwrap(res)),
  },

  orders: {
    list: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/orders', { params }).then(res => unwrap(res)),
    get: (subOrderId: string) =>
      apiClient.get(`/seller/orders/${subOrderId}`).then(res => unwrap(res)),
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
      apiClient.get('/seller/vouchers', { params }).then(res => unwrap(res)),
    get: (id: string) => apiClient.get(`/seller/vouchers/${id}`).then(res => unwrap(res)),
    create: (data: unknown) => apiClient.post('/seller/vouchers', data).then(res => unwrap(res)),
    update: (id: string, data: unknown) => apiClient.put(`/seller/vouchers/${id}`, data),
    delete: (id: string) => apiClient.delete(`/seller/vouchers/${id}`),
    toggleStatus: (id: string) => apiClient.patch(`/seller/vouchers/${id}/toggle-status`),
  },

  reports: {
    sales: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/reports/sales', { params }).then(res => unwrap(res)),
    products: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/reports/products', { params }).then(res => unwrap(res)),
    operations: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/reports/operations', { params }).then(res => unwrap(res)),
    customers: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/reports/customers', { params }).then(res => unwrap(res)),
    marketing: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/reports/marketing', { params }).then(res => unwrap(res)),
    reviews: (params?: Record<string, unknown>) =>
      apiClient.get('/seller/reports/reviews', { params }).then(res => unwrap(res)),
    exportSalesCsv: async (params?: Record<string, unknown>) => {
      const res = await apiClient.get('/seller/reports/sales/export', { params, responseType: 'blob' });
      const url = window.URL.createObjectURL(res.data as Blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `sales-report-${new Date().toISOString().slice(0, 10)}.csv`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    },
  },

  registration: {
    getProgress: () => apiClient.get('/seller-registration/progress').then(res => unwrap(res)),
    submitStep1: (data: unknown) => apiClient.post('/seller-registration/step1', data),
    submitStep2: (data: unknown) => apiClient.post('/seller-registration/step2', data),
    submitStep3: (data: unknown) => apiClient.post('/seller-registration/step3', data),
    submit: () => apiClient.post('/seller-registration/submit'),
    reapply: () => apiClient.post('/seller-registration/reapply'),
  },
};
