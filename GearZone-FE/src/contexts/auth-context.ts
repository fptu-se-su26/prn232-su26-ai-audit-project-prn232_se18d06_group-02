import { createContext } from 'react'
import type { UserDto } from '@/api/auth'

export interface AuthContextValue {
  user: UserDto | null
  loading: boolean
  login: (username: string, password: string, rememberMe?: boolean) => Promise<LoginResult>
  logout: () => Promise<void>
  refresh: () => Promise<void>
}

export interface LoginResult {
  userId: string
  role: string
}

export const AuthContext = createContext<AuthContextValue | null>(null)
