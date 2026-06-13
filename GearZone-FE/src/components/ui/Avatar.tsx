import { getInitials } from '@/lib/text'

interface AvatarProps {
  name: string
  src?: string | null
  size?: number
  className?: string
}

export default function Avatar({ name, src, size = 40, className = '' }: AvatarProps) {
  const dimension = { width: size, height: size }

  if (src) {
    return (
      <img
        src={src}
        alt={name}
        style={dimension}
        className={`shrink-0 rounded-full object-cover ${className}`}
        loading="lazy"
      />
    )
  }

  return (
    <div
      style={{ ...dimension, fontSize: Math.max(11, Math.round(size * 0.4)) }}
      className={`flex shrink-0 items-center justify-center rounded-full bg-orange-100 font-semibold text-secondary ${className}`}
      aria-label={name}
    >
      {getInitials(name)}
    </div>
  )
}
