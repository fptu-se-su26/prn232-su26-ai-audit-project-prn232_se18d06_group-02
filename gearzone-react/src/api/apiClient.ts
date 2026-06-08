import axios, { AxiosError } from 'axios';

export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

const apiClient = axios.create({
  baseURL: '/api',
  withCredentials: true,   // Required: sends ASP.NET Identity cookies cross-origin
  headers: { 'Content-Type': 'application/json' },
});

// Unwrap ApiResponse<T> — returns T on success, throws on failure
apiClient.interceptors.response.use(
  (res) => {
    const body = res.data as ApiResponse<unknown>;
    if (body && body.success === false) {
      throw new Error(body.message ?? 'Request failed');
    }
    return res;
  },
  (err: AxiosError<ApiResponse<unknown>>) => {
    const message = err.response?.data?.message ?? err.message ?? 'Unexpected error';
    return Promise.reject(new Error(message));
  }
);

export default apiClient;

// Helper to extract data from response
export function unwrap<T>(res: { data: ApiResponse<T> }): T {
  return res.data.data as T;
}
