import apiClient, { unwrap } from './apiClient';

export const cartApi = {
  get: () =>
    apiClient.get('/cart').then(res => unwrap(res)),

  add: (variantId: string, quantity: number, isBuyNow = false) =>
    apiClient.post('/cart/add', { variantId, quantity, isBuyNow }).then(res => unwrap(res)),

  updateQuantity: (cartItemId: string, quantity: number) =>
    apiClient.put('/cart/update-quantity', { cartItemId, quantity }),

  remove: (cartItemId: string) =>
    apiClient.delete(`/cart/remove/${cartItemId}`),
};
