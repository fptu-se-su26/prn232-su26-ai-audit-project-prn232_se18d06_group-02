import type { ChatCounterpartScopeOption } from '@/types/chat'

interface ConversationScopeSelectProps {
  value: string
  options: ChatCounterpartScopeOption[]
  onChange: (value: string) => void
}

export default function ConversationScopeSelect({ value, options, onChange }: ConversationScopeSelectProps) {
  if (options.length === 0) return null
  return (
    <select
      value={value}
      onChange={(event) => onChange(event.target.value)}
      className="max-w-[9rem] truncate rounded-lg border border-gray-200 bg-white px-2 py-1.5 text-[13px] text-gray-600 outline-none focus:border-secondary"
      aria-label="Filter by shop"
    >
      <option value="">All shops</option>
      {options.map((option) => (
        <option key={option.key} value={option.key}>
          {option.label}
        </option>
      ))}
    </select>
  )
}
