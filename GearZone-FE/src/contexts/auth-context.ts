import { createContext } from 'react'
import type { UserDto } from '@/api/auth'

export interface AuthContextValue {
  user: UserDto | null
  loading: boolean
  login: (username: string, password: string, rememberMe?: boolean) => Promise<void>
  logout: () => Promise<void>
  refresh: () => Promise<void>
}

export const AuthContext = createContext<AuthContextValue | null>(null)
