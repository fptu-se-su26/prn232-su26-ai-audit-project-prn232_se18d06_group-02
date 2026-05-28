interface AuthMessageProps {
  variant: 'error' | 'success'
  message: string
}

const variantClasses = {
  error: 'border border-red-100 bg-red-50 text-red-500',
  success: 'border border-green-100 bg-green-50 text-green-600',
}

export function AuthMessage({ variant, message }: AuthMessageProps) {
  return <div className={`mb-6 rounded-xl p-4 text-xs font-medium ${variantClasses[variant]}`}>{message}</div>
}
