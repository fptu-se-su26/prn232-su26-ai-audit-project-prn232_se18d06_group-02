import apiClient, { unwrap } from './apiClient'

export const usersApi = {
  me: () => apiClient.get('/users/me').then(res => unwrap(res)),
  myStore: () => apiClient.get('/users/me/store').then(res => unwrap(res)),

  getOrders: (params?: { status?: string; search?: string; page?: number; pageSize?: number }) =>
    apiClient.get('/users/me/orders', { params }).then(res => unwrap(res)),

  getReviews: (page = 1) =>
    apiClient.get('/users/me/reviews', { params: { page } }).then(res => unwrap(res)),

  getAddresses: () => apiClient.get('/users/me/addresses').then(res => unwrap(res)),
  getAddress: (id: string) => apiClient.get(`/users/me/addresses/${id}`).then(res => unwrap(res)),
  addAddress: (data: unknown) => apiClient.post('/users/me/addresses', data).then(res => unwrap(res)),
  updateAddress: (id: string, data: unknown) => apiClient.put(`/users/me/addresses/${id}`, data),
  deleteAddress: (id: string) => apiClient.delete(`/users/me/addresses/${id}`),
  setDefaultAddress: (id: string) => apiClient.patch(`/users/me/addresses/${id}/set-default`),
}
