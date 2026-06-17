/** Formats an integer with thousands separators (e.g. 12345 -> "12,345"). */
export function formatCount(value: number): string {
  return value.toLocaleString('en-US')
}

const RELATIVE_UNITS: Array<{ limit: number; secs: number; label: string }> = [
  { limit: 60, secs: 1, label: 'second' },
  { limit: 3600, secs: 60, label: 'minute' },
  { limit: 86400, secs: 3600, label: 'hour' },
  { limit: 2592000, secs: 86400, label: 'day' },
  { limit: 31536000, secs: 2592000, label: 'month' },
  { limit: Infinity, secs: 31536000, label: 'year' },
]

/** Returns a coarse relative time such as "2 years ago" / "just now". */
export function formatRelativeTime(iso: string, now: Date = new Date()): string {
  const then = new Date(iso).getTime()
  if (Number.isNaN(then)) return ''

  const diffSecs = Math.max(0, Math.floor((now.getTime() - then) / 1000))
  if (diffSecs < 45) return 'just now'

  const unit = RELATIVE_UNITS.find((u) => diffSecs < u.limit) ?? RELATIVE_UNITS[RELATIVE_UNITS.length - 1]
  const amount = Math.floor(diffSecs / unit.secs)
  return `${amount} ${unit.label}${amount === 1 ? '' : 's'} ago`
}
