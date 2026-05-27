import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { authApi } from '@/api/auth'
import { useAuth } from '@/contexts/useAuth'
import { AuthCard } from '@/components/auth/AuthCard'
import { AuthHeroPanel } from '@/components/auth/AuthHeroPanel'
import { AuthMessage } from '@/components/auth/AuthMessage'
import { AuthPageShell } from '@/components/auth/AuthPageShell'
import { AuthPanelContainer } from '@/components/auth/AuthPanelContainer'
import { AuthSectionDivider } from '@/components/auth/AuthSectionDivider'
import { AuthSocialButton } from '@/components/auth/AuthSocialButton'
import { AuthTextInput } from '@/components/auth/AuthTextInput'

const primaryButtonClassName =
  'auth-button-shadow w-full rounded-2xl bg-primary px-6 py-4 text-sm font-bold text-white transition-all duration-300 hover:bg-blue-700 active:scale-[0.98]'

export default function LoginPage() {
  const { login, refresh } = useAuth()
  const navigate = useNavigate()
  const [isRegister, setIsRegister] = useState(false)
  const [error, setError] = useState('')
  const [success] = useState('')
  const [loading, setLoading] = useState(false)
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)

  const handleLogin = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setError('')
    setLoading(true)

    try {
      await login(username, password, rememberMe)
      await refresh()
      navigate('/')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Login failed.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <AuthPageShell>
      <AuthCard>
        <AuthPanelContainer
          className="absolute top-0 left-0 flex h-full w-1/2 flex-col justify-center px-12 py-10"
          style={{
            zIndex: isRegister ? 1 : 2,
            opacity: isRegister ? 0 : 1,
            transform: isRegister ? 'translateX(100%)' : 'translateX(0)',
            transition: 'all 0.7s ease-in-out',
            pointerEvents: isRegister ? 'none' : 'auto',
          }}
        >
          <div className="mb-10">
            <h2 className="text-3xl font-extrabold tracking-tight text-slate-900">Welcome Back</h2>
            <p className="mt-2 text-sm text-slate-500">Manage your inventory and sales dashboard.</p>
          </div>

          {error && !isRegister ? <AuthMessage variant="error" message={error} /> : null}
          {success ? <AuthMessage variant="success" message={success} /> : null}

          <form onSubmit={handleLogin} className="space-y-5">
            <div>
              <label className="mb-2 block text-[10px] font-bold tracking-widest text-slate-400 uppercase">
                Credential ID
              </label>
              <AuthTextInput
                icon="alternate_email"
                placeholder="Email or Username"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
                required
              />
            </div>

            <div>
              <div className="mb-2 flex items-center justify-between">
                <label className="block text-[10px] font-bold tracking-widest text-slate-400 uppercase">
                  Access Key
                </label>
                <a href="#" className="text-[10px] font-bold text-primary hover:underline">
                  Forgot?
                </a>
              </div>
              <AuthTextInput
                type="password"
                icon="lock"
                placeholder="Enter Password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                required
              />
            </div>

            <label className="flex cursor-pointer items-center gap-3">
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(event) => setRememberMe(event.target.checked)}
                className="h-5 w-5 cursor-pointer rounded-lg border-slate-200 text-primary"
              />
              <span className="text-sm font-medium text-slate-500">Stay logged in</span>
            </label>

            <button type="submit" disabled={loading} className={primaryButtonClassName}>
              {loading ? 'Signing in…' : 'Sign In'}
            </button>
          </form>

          <AuthSectionDivider label="Instant Access" />
          <AuthSocialButton label="Google Login" onClick={() => authApi.startGoogleLogin()} />
        </AuthPanelContainer>

        <div
          className="absolute top-0 left-1/2 z-[100] h-full w-1/2 overflow-hidden"
          style={{
            transform: isRegister ? 'translateX(-100%)' : 'translateX(0)',
            transition: 'all 0.7s ease-in-out',
          }}
        >
          <div
            className="auth-overlay-gradient relative h-full w-[200%] text-white"
            style={{
              left: '-100%',
              transform: isRegister ? 'translateX(50%)' : 'translateX(0)',
              transition: 'all 0.7s ease-in-out',
            }}
          >
            <AuthHeroPanel
              title="Returning?"
              description={
                <>
                  Log in back to your GearZone dashboard
                  <br />
                  and continue your progress.
                </>
              }
              buttonLabel="Sign In Instead"
              onClick={() => setIsRegister(false)}
              className="absolute top-0 left-0 flex h-full w-1/2 flex-col items-center justify-center px-12 text-center"
              style={{
                transform: isRegister ? 'translateX(0)' : 'translateX(-20%)',
                transition: 'all 0.7s ease-in-out',
              }}
            />

            <AuthHeroPanel
              title="New Hero?"
              description={
                <>
                  Register today and start selling gear
                  <br />
                  on our premium platform.
                </>
              }
              buttonLabel="Create Account"
              onClick={() => setIsRegister(true)}
              className="absolute top-0 right-0 flex h-full w-1/2 flex-col items-center justify-center px-12 text-center"
              style={{
                transform: isRegister ? 'translateX(20%)' : 'translateX(0)',
                transition: 'all 0.7s ease-in-out',
              }}
            />
          </div>
        </div>
      </AuthCard>
    </AuthPageShell>
  )
}
