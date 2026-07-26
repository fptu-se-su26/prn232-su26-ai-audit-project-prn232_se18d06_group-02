import apiClient, { unwrap } from './apiClient'

export const ordersApi = {
  paymentStatus: (orderId: string) =>
    apiClient.get(`/orders/${orderId}/payment-status`).then(res => unwrap(res)),

  track: (subOrderId: string) =>
    apiClient.get(`/orders/track/${subOrderId}`).then(res => unwrap(res)),

  trackLive: (subOrderId: string) =>
    apiClient.get(`/orders/track/${subOrderId}/live`).then(res => unwrap(res)),
}
