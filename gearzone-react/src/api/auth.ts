import apiClient, { unwrap } from './apiClient';

export interface UserDto {
  id: string;
  fullName: string;
  email: string;
  userName: string;
  phoneNumber?: string;
  avatarUrl?: string;
  role?: string;
}

export const authApi = {
  login: (username: string, password: string, rememberMe = false) =>
    apiClient.post('/auth/login', { username, password, rememberMe })
      .then(res => unwrap<{ userId: string; role: string }>(res)),

  register: (fullName: string, email: string, password: string, confirmPassword: string) =>
    apiClient.post('/auth/register', { fullName, email, password, confirmPassword })
      .then(res => res.data),

  logout: () =>
    apiClient.post('/auth/logout'),

  me: () =>
    apiClient.get('/auth/me').then(res => unwrap<UserDto>(res)),

  verifyEmail: (userId: string, token: string) =>
    apiClient.get('/auth/verify-email', { params: { userId, token } }),

  resendVerification: (email: string) =>
    apiClient.post('/auth/resend-verification', { email }),

  // Redirect to Google OAuth — not an API call, redirect the browser
  startGoogleLogin: (returnUrl?: string) => {
    window.location.href = `/api/auth/external-login?provider=Google${returnUrl ? `&returnUrl=${encodeURIComponent(returnUrl)}` : ''}`;
  },
};
