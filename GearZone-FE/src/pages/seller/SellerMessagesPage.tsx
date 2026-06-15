import { useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { chatApi, type ChatConversation, type ChatScopeOption, type ChatThread } from '@/api/chat'
import { SellerLayout } from '@/components/seller/SellerLayout'
import { useAuth } from '@/contexts/useAuth'

function initial(value?: string) {
  return value?.trim()?.charAt(0)?.toUpperCase() || '?'
}

function formatTime(value?: string) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  const today = new Date()
  const sameDay = date.toDateString() === today.toDateString()
  return sameDay
    ? new Intl.DateTimeFormat('en-GB', { hour: '2-digit', minute: '2-digit', hour12: false }).format(date)
    : new Intl.DateTimeFormat('en-GB', { day: '2-digit', month: '2-digit' }).format(date)
}

function formatDateLabel(value?: string) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value
  return new Intl.DateTimeFormat('en-GB', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(date)
}

function formatMoney(value?: number) {
  return `${new Intl.NumberFormat('en-US').format(value ?? 0)} VND`
}

function previewText(conversation: ChatConversation, currentUserId?: string) {
  const raw = conversation.lastMessagePreview?.trim()
    ? conversation.lastMessagePreview
    : conversation.hasMessages
      ? 'Open conversation'
      : 'Start chatting'

  return conversation.lastMessageSenderUserId && conversation.lastMessageSenderUserId === currentUserId
    ? `You: ${raw}`
    : raw
}

export default function SellerMessagesPage() {
  const { user } = useAuth()
  const [conversations, setConversations] = useState<ChatConversation[]>([])
  const [scopeOptions, setScopeOptions] = useState<ChatScopeOption[]>([])
  const [activeConversationId, setActiveConversationId] = useState<string | null>(null)
  const [thread, setThread] = useState<ChatThread | null>(null)
  const [filter, setFilter] = useState('all')
  const [scopeKey, setScopeKey] = useState('')
  const [searchInput, setSearchInput] = useState('')
  const [searchTerm, setSearchTerm] = useState('')
  const [totalUnread, setTotalUnread] = useState(0)
  const [loadingList, setLoadingList] = useState(true)
  const [loadingThread, setLoadingThread] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [message, setMessage] = useState('')
  const [sending, setSending] = useState(false)
  const threadScrollRef = useRef<HTMLDivElement | null>(null)

  const activeConversation = useMemo(
    () => conversations.find((item) => item.conversationId === activeConversationId),
    [activeConversationId, conversations],
  )

  const loadInbox = (preferredConversationId?: string | null) => {
    setLoadingList(true)
    setError(null)
    return Promise.all([
      chatApi.sellerInbox({
        Filter: filter,
        SearchTerm: searchTerm || undefined,
        CounterpartScopeKey: scopeKey || undefined,
        PageNumber: 1,
        PageSize: 20,
      }),
      chatApi.sellerUnread(),
      chatApi.sellerScopeOptions(),
    ])
      .then(([inbox, unread, scopes]) => {
        const items = inbox.items ?? []
        setConversations(items)
        setTotalUnread(unread.unreadCount ?? 0)
        setScopeOptions(scopes)

        const nextActive =
          preferredConversationId && items.some((item) => item.conversationId === preferredConversationId)
            ? preferredConversationId
            : activeConversationId && items.some((item) => item.conversationId === activeConversationId)
              ? activeConversationId
              : items[0]?.conversationId ?? null

        setActiveConversationId(nextActive)
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Failed to load conversations.')
        setConversations([])
      })
      .finally(() => setLoadingList(false))
  }

  const loadThread = (conversationId: string) => {
    setLoadingThread(true)
    setError(null)
    return chatApi
      .sellerThread(conversationId, { LoadedPageCount: 1, PageSize: 30 })
      .then((data) => {
        setThread(data)
        setConversations((current) =>
          current.map((item) =>
            item.conversationId === conversationId ? { ...item, unreadCount: 0 } : item,
          ),
        )
        void chatApi.sellerUnread().then((result) => setTotalUnread(result.unreadCount ?? 0))
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Failed to load conversation thread.')
        setThread(null)
      })
      .finally(() => setLoadingThread(false))
  }

  useEffect(() => {
    loadInbox(null)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filter, searchTerm, scopeKey])

  useEffect(() => {
    if (activeConversationId) {
      loadThread(activeConversationId)
    } else {
      setThread(null)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeConversationId])

  useEffect(() => {
    const node = threadScrollRef.current
    if (node) node.scrollTop = node.scrollHeight
  }, [thread?.messages.length, activeConversationId])

  const submitSearch = (event: FormEvent) => {
    event.preventDefault()
    setSearchTerm(searchInput.trim())
  }

  const sendMessage = async (event: FormEvent) => {
    event.preventDefault()
    if (!activeConversationId || !message.trim()) return

    setSending(true)
    try {
      await chatApi.send(activeConversationId, message.trim())
      setMessage('')
      await Promise.all([loadThread(activeConversationId), loadInbox(activeConversationId)])
    } finally {
      setSending(false)
    }
  }

  return (
    <SellerLayout
      pageHeader="Customer Messages"
      breadcrumb={['Messages']}
      unreadCount={totalUnread}
      contentMode="fullCanvas"
    >
      <style>
        {`
          .chat-scroll::-webkit-scrollbar { width: 8px; height: 8px; }
          .chat-scroll::-webkit-scrollbar-track { background: transparent; }
          .chat-scroll::-webkit-scrollbar-thumb { background: rgba(0,0,0,0.14); border-radius: 9999px; }
          .chat-scroll::-webkit-scrollbar-thumb:hover { background: rgba(0,0,0,0.22); }
          .chat-message-bubble { max-width: min(22rem, 100%); border-radius: 0.65rem; padding: 0.5rem 0.68rem; border: 1px solid #ececec; overflow-wrap: anywhere; word-break: break-word; }
          .chat-message-own { border-color: #ffd7cb; background: #fff2ee; }
          .chat-message-other { border-color: #ececec; background: #ffffff; }
        `}
      </style>

      <div className="mx-auto flex h-full min-h-0 w-full max-w-[1600px] flex-col gap-4 px-6 pb-6">
        <div className="rounded-xl border border-slate-200 bg-white px-5 py-4 shadow-sm">
          <h2 className="text-xl font-bold text-slate-900">Customer Messages</h2>
          <p className="mt-1 text-sm text-slate-500">
            Manage all buyer conversations in one place.
          </p>
        </div>

        <div className="min-h-0 flex-1 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
          <div className="grid h-full min-h-0 grid-cols-1 gap-0 xl:grid-cols-[17rem_minmax(0,1fr)]">
            <section className="min-h-0 overflow-hidden border-b border-[#efefef] bg-white xl:border-b-0 xl:border-r">
              <div className="flex h-full min-h-0 flex-col bg-white">
                <div className="border-b border-[#efefef] bg-white px-4 py-3">
                  <div className="flex items-center justify-between gap-3">
                    <div className="min-w-0">
                      <p className="truncate text-[15px] font-semibold text-[#ee4d2d]">
                        Customer Messages
                      </p>
                    </div>
                    <div className="shrink-0 text-[13px] font-medium text-[#999]">
                      {totalUnread}
                    </div>
                  </div>

                  <form className="mt-3 grid gap-2" onSubmit={submitSearch}>
                    <label className="relative min-w-0">
                      <span className="material-symbols-outlined pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-[18px] text-[#b5b5b5]">
                        search
                      </span>
                      <input
                        type="search"
                        value={searchInput}
                        onChange={(event) => setSearchInput(event.target.value)}
                        placeholder="Search by buyer name"
                        className="h-9 w-full rounded-md border border-[#e5e5e5] bg-[#fafafa] pl-10 pr-3 text-[13px] text-[#333] outline-none transition focus:border-[#ee4d2d] focus:bg-white"
                      />
                    </label>

                    <div className="grid grid-cols-2 gap-2">
                      <select
                        value={filter}
                        onChange={(event) => setFilter(event.target.value)}
                        className="h-9 min-w-0 rounded-md border border-[#e5e5e5] bg-white px-3 text-[13px] text-[#333] outline-none transition focus:border-[#ee4d2d]"
                      >
                        <option value="all">All</option>
                        <option value="unread">Unread</option>
                      </select>

                      <select
                        value={scopeKey}
                        onChange={(event) => setScopeKey(event.target.value)}
                        className="h-9 min-w-0 rounded-md border border-[#e5e5e5] bg-white px-3 text-[13px] text-[#333] outline-none transition focus:border-[#ee4d2d]"
                      >
                        <option value="">All buyers</option>
                        {scopeOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </div>
                  </form>
                </div>

                <div className="chat-scroll flex-1 overflow-y-auto bg-white">
                  {loadingList ? (
                    <div className="space-y-2 p-3">
                      {Array.from({ length: 8 }).map((_, index) => (
                        <div key={index} className="h-14 animate-pulse rounded-lg bg-slate-100" />
                      ))}
                    </div>
                  ) : conversations.length === 0 ? (
                    <div className="flex h-full min-h-[18rem] flex-col items-center justify-center px-6 py-10 text-center">
                      <div className="flex h-12 w-12 items-center justify-center rounded-full bg-[#fff1ed] text-[#ee4d2d]">
                        <span className="material-symbols-outlined text-[24px]">chat</span>
                      </div>
                      <h3 className="mt-4 text-base font-semibold text-[#333]">No messages yet</h3>
                      <p className="mt-2 max-w-xs text-[13px] leading-6 text-[#999]">
                        Customer conversations will appear here as soon as a buyer sends a message.
                      </p>
                    </div>
                  ) : (
                    conversations.map((conversation) => {
                      const active = conversation.conversationId === activeConversationId
                      return (
                        <button
                          key={conversation.conversationId}
                          type="button"
                          onClick={() => setActiveConversationId(conversation.conversationId)}
                          className={`flex w-full items-start gap-3 border-b border-[#f5f5f5] px-3 py-2.5 text-left transition ${
                            active ? 'bg-[#f6f6f6]' : 'bg-white hover:bg-[#fafafa]'
                          }`}
                        >
                          <div className="mt-0.5 flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-full bg-[#f5f5f5] text-sm font-semibold text-[#555]">
                            {conversation.counterpartAvatarUrl ? (
                              <img
                                src={conversation.counterpartAvatarUrl}
                                alt={conversation.counterpartName}
                                className="h-full w-full object-cover"
                              />
                            ) : (
                              initial(conversation.counterpartName)
                            )}
                          </div>

                          <div className="min-w-0 flex-1">
                            <div className="flex items-start gap-3">
                              <div className="min-w-0 flex-1">
                                <p className="truncate text-[14px] font-medium text-[#333]">
                                  {conversation.counterpartName}
                                </p>
                                <p
                                  className={`mt-0.5 line-clamp-1 text-[12px] leading-5 ${
                                    conversation.unreadCount > 0
                                      ? 'font-medium text-[#555]'
                                      : 'text-[#888]'
                                  }`}
                                >
                                  {previewText(conversation, user?.id)}
                                </p>
                              </div>
                              <div className="shrink-0 text-right">
                                <p
                                  className={`text-[12px] leading-5 ${
                                    conversation.unreadCount > 0 ? 'text-[#ee4d2d]' : 'text-[#999]'
                                  }`}
                                >
                                  {formatTime(conversation.lastMessageAt)}
                                </p>
                                {conversation.unreadCount > 0 && (
                                  <span className="mt-1 inline-flex min-h-5 min-w-5 items-center justify-center rounded-full bg-[#ee4d2d] px-1 text-[10px] font-semibold text-white">
                                    {conversation.unreadCount > 99 ? '99+' : conversation.unreadCount}
                                  </span>
                                )}
                              </div>
                            </div>
                          </div>
                        </button>
                      )
                    })
                  )}
                </div>
              </div>
            </section>

            <section className="min-h-0">
              <div className="flex h-full min-h-0 min-w-0 flex-col bg-white">
                {!activeConversationId || (!thread && !loadingThread) ? (
                  <div className="flex h-full min-h-[24rem] flex-col items-center justify-center px-8 text-center">
                    <div className="flex h-16 w-16 items-center justify-center rounded-full bg-[#fff1ed] text-[#ee4d2d]">
                      <span className="material-symbols-outlined text-[32px]">chat</span>
                    </div>
                    <h3 className="mt-5 text-xl font-semibold text-[#333]">Choose a buyer</h3>
                    <p className="mt-3 max-w-md text-[13px] leading-7 text-[#999]">
                      Pick a buyer from the left column to open the realtime thread.
                    </p>
                    {error && <p className="mt-3 text-sm text-red-600">{error}</p>}
                  </div>
                ) : (
                  <>
                    <div className="border-b border-[#efefef] bg-white px-4 py-2.5">
                      <div className="flex items-center justify-between gap-3">
                        <div className="flex min-w-0 items-center gap-3">
                          <div className="flex h-10 w-10 shrink-0 items-center justify-center overflow-hidden rounded-full bg-[#f5f5f5] text-sm font-semibold text-[#555]">
                            {thread?.counterpartAvatarUrl || activeConversation?.counterpartAvatarUrl ? (
                              <img
                                src={thread?.counterpartAvatarUrl ?? activeConversation?.counterpartAvatarUrl}
                                alt={thread?.counterpartName ?? activeConversation?.counterpartName}
                                className="h-full w-full object-cover"
                              />
                            ) : (
                              initial(thread?.counterpartName ?? activeConversation?.counterpartName)
                            )}
                          </div>
                          <div className="min-w-0">
                            <p className="truncate text-[14px] font-medium text-[#333]">
                              {thread?.counterpartName ?? activeConversation?.counterpartName}
                            </p>
                            <p className="truncate text-[12px] text-[#999]">Buyer</p>
                          </div>
                        </div>
                      </div>
                    </div>

                    {thread?.recentOrders && thread.recentOrders.length > 0 && (
                      <div className="border-b border-[#f2f2f2] bg-[#fafafa] px-4 py-2.5">
                        <div className="flex flex-wrap items-center gap-2">
                          <span className="text-[11px] font-medium uppercase tracking-[0.08em] text-[#999]">
                            Recent orders
                          </span>
                          {thread.recentOrders.slice(0, 2).map((order) => (
                            <span
                              key={order.subOrderId}
                              className="inline-flex items-center gap-2 rounded-md border border-[#ececec] bg-white px-2.5 py-1.5 text-[12px] text-[#666]"
                            >
                              <span className="font-medium text-[#333]">#{order.orderCode}</span>
                              <span>{order.status}</span>
                              <span className="font-medium text-[#ee4d2d]">
                                {formatMoney(order.subtotal)}
                              </span>
                            </span>
                          ))}
                        </div>
                      </div>
                    )}

                    <div
                      ref={threadScrollRef}
                      className="chat-scroll flex-1 space-y-2.5 overflow-y-auto bg-[#f6f6f6] px-4 py-3.5"
                    >
                      {loadingThread ? (
                        <div className="space-y-3">
                          {Array.from({ length: 8 }).map((_, index) => (
                            <div
                              key={index}
                              className={`h-12 animate-pulse rounded-lg bg-white ${
                                index % 2 === 0 ? 'mr-auto w-2/3' : 'ml-auto w-1/2'
                              }`}
                            />
                          ))}
                        </div>
                      ) : thread?.messages.length === 0 ? (
                        <div className="mx-auto mt-10 max-w-sm rounded-md border border-[#ececec] bg-white px-5 py-6 text-center">
                          <p className="text-[14px] font-medium text-[#333]">Welcome to GearZone Chat</p>
                          <p className="mt-2 text-[13px] leading-6 text-[#999]">
                            Send the first message to start chatting with the buyer.
                          </p>
                        </div>
                      ) : (
                        thread?.messages.map((item, index) => {
                          const previous = thread.messages[index - 1]
                          const showDate =
                            !previous ||
                            formatDateLabel(previous.sentAt) !== formatDateLabel(item.sentAt)
                          const own = item.senderUserId === user?.id

                          return (
                            <div key={item.id}>
                              {showDate && (
                                <div className="flex justify-center py-1">
                                  <span className="rounded-full bg-[#ebebeb] px-3 py-1 text-[11px] font-medium text-[#999]">
                                    {formatDateLabel(item.sentAt)}
                                  </span>
                                </div>
                              )}
                              <div className={`flex min-w-0 ${own ? 'justify-end' : 'justify-start'}`}>
                                <div
                                  className={`chat-message-bubble min-w-0 ${
                                    own ? 'chat-message-own' : 'chat-message-other'
                                  }`}
                                >
                                  {!own && (
                                    <p className="mb-1 text-[11px] font-medium text-[#666]">
                                      {item.senderDisplayName}
                                    </p>
                                  )}
                                  <p className="whitespace-pre-wrap break-words text-[13px] leading-6 text-[#333]">
                                    {item.content}
                                  </p>
                                  <div className="mt-1 flex items-center justify-end gap-2 text-[11px] leading-4 text-[#999]">
                                    <span>{formatTime(item.sentAt)}</span>
                                    {own && item.isRead && (
                                      <span className="font-medium text-[#ee4d2d]">Seen</span>
                                    )}
                                  </div>
                                </div>
                              </div>
                            </div>
                          )
                        })
                      )}
                    </div>

                    <div className="border-t border-[#efefef] bg-white px-4 py-2.5">
                      <form className="flex items-end gap-3" onSubmit={sendMessage}>
                        <div className="flex-1 overflow-hidden rounded-md border border-[#e5e5e5] bg-white">
                          <textarea
                            rows={1}
                            maxLength={2000}
                            placeholder="Type your message"
                            value={message}
                            onChange={(event) => setMessage(event.target.value)}
                            className="min-h-[2.35rem] max-h-[6.5rem] w-full resize-none overflow-y-auto border-0 px-3 py-2.5 text-[13px] text-[#333] outline-none focus:ring-0"
                          />
                          <div className="flex items-center justify-between border-t border-[#f3f3f3] px-2.5 py-1.5">
                            <div className="flex items-center gap-1 text-[#d0d0d0]">
                              <button
                                type="button"
                                className="inline-flex h-7 w-7 items-center justify-center rounded-full"
                                disabled
                              >
                                <span className="material-symbols-outlined text-[17px]">image</span>
                              </button>
                              <button
                                type="button"
                                className="inline-flex h-7 w-7 items-center justify-center rounded-full"
                                disabled
                              >
                                <span className="material-symbols-outlined text-[17px]">
                                  photo_camera
                                </span>
                              </button>
                            </div>

                            <button
                              type="submit"
                              disabled={sending || !message.trim()}
                              className="seller-primary-button inline-flex h-8 min-w-8 items-center justify-center rounded-md bg-[#ee4d2d] px-2.5 text-white transition hover:bg-[#d93c1c] disabled:cursor-not-allowed disabled:opacity-60"
                            >
                              <span className="material-symbols-outlined text-[16px]">send</span>
                            </button>
                          </div>
                        </div>
                      </form>
                    </div>
                  </>
                )}
              </div>
            </section>
          </div>
        </div>
      </div>
    </SellerLayout>
  )
}
