interface UnreadBadgeProps {
  count: number
  className?: string
}

export default function UnreadBadge({ count, className = '' }: UnreadBadgeProps) {
  if (count <= 0) return null
  return (
    <span
      className={`inline-flex min-w-[1.25rem] items-center justify-center rounded-full bg-red-600 px-1.5 text-[11px] font-bold leading-5 text-white ${className}`}
    >
      {count > 99 ? '99+' : count}
    </span>
  )
}
