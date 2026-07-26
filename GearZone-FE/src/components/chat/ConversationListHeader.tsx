import ConversationFilterTabs from '@/components/chat/ConversationFilterTabs'
import ConversationScopeSelect from '@/components/chat/ConversationScopeSelect'
import ConversationSearchInput from '@/components/chat/ConversationSearchInput'
import type { ChatCounterpartScopeOption, ChatFilter } from '@/types/chat'

interface ConversationListHeaderProps {
  searchValue: string
  onSearchChange: (value: string) => void
  filter: ChatFilter
  onFilterChange: (value: ChatFilter) => void
  scopeKey: string
  scopeOptions: ChatCounterpartScopeOption[]
  onScopeChange: (value: string) => void
}

export default function ConversationListHeader({
  searchValue,
  onSearchChange,
  filter,
  onFilterChange,
  scopeKey,
  scopeOptions,
  onScopeChange,
}: ConversationListHeaderProps) {
  return (
    <div className="space-y-3 border-b border-gray-100 p-4">
      <ConversationSearchInput value={searchValue} onChange={onSearchChange} />
      <div className="flex items-center justify-between gap-2">
        <ConversationFilterTabs value={filter} onChange={onFilterChange} />
        <ConversationScopeSelect value={scopeKey} options={scopeOptions} onChange={onScopeChange} />
      </div>
    </div>
  )
}
