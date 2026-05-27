import { AuthSectionDivider } from '@/components/auth/AuthSectionDivider'
import { AuthSocialButton } from '@/components/auth/AuthSocialButton'
import { AuthTextInput } from '@/components/auth/AuthTextInput'
import { AuthMessage } from '@/components/auth/AuthMessage'

interface RegisterFormProps {
  loading: boolean
  error: string
  fullName: string
  email: string
  password: string
  confirmPassword: string
  onFullNameChange: (value: string) => void
  onEmailChange: (value: string) => void
  onPasswordChange: (value: string) => void
  onConfirmPasswordChange: (value: string) => void
  onSubmit: (event: React.FormEvent<HTMLFormElement>) => void
  onGoogleLogin: () => void
}

const primaryButtonClassName =
  'auth-button-shadow mt-4 w-full rounded-2xl bg-primary px-6 py-4 text-sm font-bold text-white transition-all duration-300 hover:bg-blue-700 active:scale-[0.98]'

export function RegisterForm({
  loading,
  error,
  fullName,
  email,
  password,
  confirmPassword,
  onFullNameChange,
  onEmailChange,
  onPasswordChange,
  onConfirmPasswordChange,
  onSubmit,
  onGoogleLogin,
}: RegisterFormProps) {
  return (
    <>
      <div className="mb-8">
        <h2 className="text-3xl font-extrabold tracking-tight text-slate-900">Create Account</h2>
        <p className="mt-2 text-sm text-slate-500">Join GearZone and start trading gear today.</p>
      </div>

      {error ? <AuthMessage variant="error" message={error} /> : null}

      <form onSubmit={onSubmit} className="space-y-4">
        <AuthTextInput
          icon="person"
          placeholder="Full Name"
          value={fullName}
          onChange={(event) => onFullNameChange(event.target.value)}
          required
        />

        <AuthTextInput
          type="email"
          icon="mail"
          placeholder="Email Address"
          value={email}
          onChange={(event) => onEmailChange(event.target.value)}
          required
        />

        <div className="grid grid-cols-2 gap-4">
          <AuthTextInput
            type="password"
            placeholder="Password"
            value={password}
            onChange={(event) => onPasswordChange(event.target.value)}
            required
          />
          <AuthTextInput
            type="password"
            placeholder="Confirm"
            value={confirmPassword}
            onChange={(event) => onConfirmPasswordChange(event.target.value)}
            required
          />
        </div>

        <button type="submit" disabled={loading} className={primaryButtonClassName}>
          {loading ? 'Creating account…' : 'Get Started'}
        </button>
      </form>

      <AuthSectionDivider label="Social Access" />
      <AuthSocialButton label="Google Account" onClick={onGoogleLogin} />
    </>
  )
}
