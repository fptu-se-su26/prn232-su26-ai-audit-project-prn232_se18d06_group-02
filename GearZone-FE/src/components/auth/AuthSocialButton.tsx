interface AuthSocialButtonProps {
  label: string
  onClick: () => void
}

export function AuthSocialButton({ label, onClick }: AuthSocialButtonProps) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="flex w-full items-center justify-center gap-3 rounded-2xl border-2 border-slate-100 px-6 py-4 text-sm font-bold text-slate-600 transition-all duration-300 hover:border-blue-50 hover:bg-blue-50 hover:text-primary"
    >
      <img src="https://www.svgrepo.com/show/475656/google-color.svg" className="h-5 w-5" alt="Google" />
      {label}
    </button>
  )
}
