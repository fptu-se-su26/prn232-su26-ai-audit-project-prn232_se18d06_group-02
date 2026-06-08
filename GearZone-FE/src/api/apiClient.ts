import axios, { AxiosError } from 'axios'

export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}

const apiClient = axios.create({
  baseURL: '/api',
  withCredentials: true,
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.response.use(
  (response) => {
    const body = response.data as ApiResponse<unknown>
    if (body && body.success === false) {
      throw new Error(body.message ?? 'Request failed')
    }
    return response
  },
  (error: AxiosError<ApiResponse<unknown>>) => {
    const message = error.response?.data?.message ?? error.message ?? 'Unexpected error'
    return Promise.reject(new Error(message))
  },
)

export function unwrap<T>(response: { data: ApiResponse<T> }): T {
  return response.data.data as T
}

export default apiClient
