import apiClient, { unwrap } from './apiClient'

export const reviewsApi = {
  getEditor: (orderItemId: string) =>
    apiClient.get(`/reviews/editor/${orderItemId}`).then(res => unwrap(res)),

  submit: (data: { orderItemId: string; rating: number; comment: string }) =>
    apiClient.post('/reviews/submit', data).then(res => unwrap(res)),
}
