/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { authApi, type UserDto } from '@/api/auth'
import { AuthContext, type AuthContextValue } from '@/contexts/auth-context'

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<UserDto | null>(null)
  const [loading, setLoading] = useState(true)

  const refresh = async () => {
    try {
      const currentUser = await authApi.me()
      setUser(currentUser)
    } catch {
      setUser(null)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void refresh()
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      loading,
      login: async (username, password, rememberMe = false) => {
        await authApi.login(username, password, rememberMe)
        await refresh()
      },
      logout: async () => {
        await authApi.logout()
        setUser(null)
      },
      refresh,
    }),
    [loading, user],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export default AuthProvider
