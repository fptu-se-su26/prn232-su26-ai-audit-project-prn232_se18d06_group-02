import apiClient, { unwrap } from './apiClient'

export const sellerApi = {
  registration: {
    getProgress: () => apiClient.get('/seller-registration/progress').then((res) => unwrap(res)),
    submitStep1: (data: unknown) => apiClient.post('/seller-registration/step1', data).then((res) => unwrap(res)),
    submitStep2: (data: FormData) =>
      apiClient
        .post('/seller-registration/step2', data, { headers: { 'Content-Type': 'multipart/form-data' } })
        .then((res) => unwrap(res)),
    submitStep3: (data: unknown) => apiClient.post('/seller-registration/step3', data).then((res) => unwrap(res)),
    submit: () => apiClient.post('/seller-registration/submit').then((res) => unwrap(res)),
    reapply: () => apiClient.post('/seller-registration/reapply').then((res) => unwrap(res)),
  },
}
