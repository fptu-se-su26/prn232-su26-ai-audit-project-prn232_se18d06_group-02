interface ErrorStateProps {
  title?: string
  message?: string
  onRetry?: () => void
  className?: string
}

export default function ErrorState({
  title = 'Something went wrong',
  message,
  onRetry,
  className = '',
}: ErrorStateProps) {
  return (
    <div className={`flex h-full min-h-[16rem] flex-col items-center justify-center px-6 py-10 text-center ${className}`}>
      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-red-50 text-red-500">
        <span className="material-symbols-outlined text-[24px]">error</span>
      </div>
      <h3 className="mt-4 text-base font-semibold text-gray-800">{title}</h3>
      {message && <p className="mt-2 max-w-xs text-[13px] leading-6 text-gray-500">{message}</p>}
      {onRetry && (
        <button
          type="button"
          onClick={onRetry}
          className="mt-4 rounded-lg bg-secondary px-4 py-2 text-sm font-semibold text-white transition hover:opacity-90"
        >
          Try again
        </button>
      )}
    </div>
  )
}
