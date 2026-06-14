interface ConversationSearchInputProps {
  value: string
  onChange: (value: string) => void
}

export default function ConversationSearchInput({ value, onChange }: ConversationSearchInputProps) {
  return (
    <div className="relative">
      <span className="material-symbols-outlined pointer-events-none absolute left-2 top-1/2 -translate-y-1/2 text-[18px] text-gray-400">
        search
      </span>
      <input
        type="search"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        placeholder="Search conversations"
        className="w-full rounded-lg border border-gray-200 bg-gray-50 py-2 pl-8 pr-3 text-sm outline-none focus:border-secondary"
      />
    </div>
  )
}
