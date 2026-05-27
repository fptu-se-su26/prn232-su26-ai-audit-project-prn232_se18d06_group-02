import apiClient, { unwrap } from './apiClient'

export interface UserDto {
  id: string
  fullName: string
  email: string
  userName: string
  phoneNumber?: string
  avatarUrl?: string
  role?: string
}

export const authApi = {
  login: (username: string, password: string, rememberMe = false) =>
    apiClient
      .post('/auth/login', { username, password, rememberMe })
      .then((response) => unwrap<{ userId: string; role: string }>(response)),

  register: (fullName: string, email: string, password: string, confirmPassword: string) =>
    apiClient
      .post('/auth/register', { fullName, email, password, confirmPassword })
      .then((response) => response.data),

  logout: () => apiClient.post('/auth/logout'),

  me: () => apiClient.get('/auth/me').then((response) => unwrap<UserDto>(response)),

  verifyEmail: (userId: string, token: string) => apiClient.get('/auth/verify-email', { params: { userId, token } }),

  resendVerification: (email: string) => apiClient.post('/auth/resend-verification', { email }),

  startGoogleLogin: (returnUrl?: string) => {
    window.location.href = `/api/auth/external-login?provider=Google${returnUrl ? `&returnUrl=${encodeURIComponent(returnUrl)}` : ''}`
  },
}
