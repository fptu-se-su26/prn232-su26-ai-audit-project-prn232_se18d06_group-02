import ChatThreadPane from '@/components/chat/ChatThreadPane'
import ConversationList from '@/components/chat/ConversationList'
import ConversationListHeader from '@/components/chat/ConversationListHeader'
import { useChatContext } from '@/contexts/useChatContext'
import { useChatConversations } from '@/hooks/useChatConversations'
import { useChatThread } from '@/hooks/useChatThread'

/** Shared two-column inbox (conversation list + thread) used by both the widget and the full page. */
export default function ChatInboxLayout() {
  const { hub, activeConversationId, setActiveConversationId } = useChatContext()
  const conversations = useChatConversations(hub)
  const thread = useChatThread(hub, activeConversationId)

  const showThreadOnMobile = activeConversationId !== null

  return (
    <div className="grid h-full grid-cols-1 lg:grid-cols-[18.75rem_minmax(0,1fr)]">
      <div
        className={`h-full min-h-0 flex-col border-r border-gray-100 ${
          showThreadOnMobile ? 'hidden lg:flex' : 'flex'
        }`}
      >
        <ConversationListHeader
          searchValue={conversations.searchInput}
          onSearchChange={conversations.setSearchInput}
          filter={conversations.filter}
          onFilterChange={conversations.setFilter}
          scopeKey={conversations.scopeKey}
          scopeOptions={conversations.scopeOptions}
          onScopeChange={conversations.setScopeKey}
        />
        <div className="min-h-0 flex-1">
          <ConversationList
            items={conversations.items}
            activeConversationId={activeConversationId}
            loading={conversations.loading}
            hasMore={conversations.hasMore}
            onSelect={setActiveConversationId}
            onLoadMore={conversations.loadMore}
          />
        </div>
      </div>
      <div className={`h-full min-h-0 flex-col ${showThreadOnMobile ? 'flex' : 'hidden lg:flex'}`}>
        <ChatThreadPane
          conversationId={activeConversationId}
          thread={thread.thread}
          messages={thread.messages}
          loading={thread.loading}
          error={thread.error}
          hasOlderMessages={thread.hasOlderMessages}
          onLoadOlder={thread.loadOlder}
          onSent={thread.appendMessage}
          onBack={() => setActiveConversationId(null)}
        />
      </div>
    </div>
  )
}
