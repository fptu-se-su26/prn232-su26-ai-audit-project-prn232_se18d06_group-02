import type { ChatFilter } from '@/types/chat'

interface ConversationFilterTabsProps {
  value: ChatFilter
  onChange: (value: ChatFilter) => void
}

const TABS: { key: ChatFilter; label: string }[] = [
  { key: 'all', label: 'All' },
  { key: 'unread', label: 'Unread' },
]

export default function ConversationFilterTabs({ value, onChange }: ConversationFilterTabsProps) {
  return (
    <div className="flex gap-1">
      {TABS.map((tab) => (
        <button
          key={tab.key}
          type="button"
          onClick={() => onChange(tab.key)}
          className={`rounded-full px-3 py-1 text-[13px] font-medium transition ${
            value === tab.key ? 'bg-secondary text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'
          }`}
        >
          {tab.label}
        </button>
      ))}
    </div>
  )
}
