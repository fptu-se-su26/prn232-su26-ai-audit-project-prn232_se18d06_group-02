import apiClient, { unwrap } from './apiClient'

export const checkoutApi = {
  getData: () =>
    apiClient.get('/checkout').then(res => unwrap(res)),

  placeOrder: (data: { addressId: string; paymentMethod: string; voucherCode?: string; note?: string }) =>
    apiClient.post('/checkout', data).then(res => unwrap(res)),

  cancelPayment: (orderCode: string) =>
    apiClient.post('/checkout/cancel-payment', { orderCode }),

  paymentReturn: (orderCode: number, status?: string) =>
    apiClient.get('/checkout/payment-return', { params: { orderCode, status } }).then(res => unwrap(res)),

  getSuccess: (orderId: string) =>
    apiClient.get(`/checkout/success/${orderId}`).then(res => unwrap(res)),

  applyVoucher: (code: string) =>
    apiClient.post('/checkout/apply-voucher', { code }).then(res => unwrap(res)),

  availableVouchers: () =>
    apiClient.get('/checkout/vouchers').then(res => unwrap(res)),

  calculateShipping: (addressId: string) =>
    apiClient.post('/checkout/calculate-shipping', { addressId }).then(res => unwrap(res)),
}
