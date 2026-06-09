import { useEffect, useRef, useState } from 'react'
import { chatApi } from '@/api/chat'

interface ConversationSummary {
  conversationId: string
  buyerName: string
  buyerAvatar?: string
  lastMessage?: string
  lastMessageAt?: string
  unreadCount: number
  productName?: string
}

interface Message {
  messageId: string
  senderId: string
  senderName: string
  content: string
  sentAt: string
  isRead: boolean
}

interface ThreadData {
  conversationId: string
  buyerName: string
  messages: Message[]
}

export default function SellerMessagesPage() {
  const [conversations, setConversations] = useState<ConversationSummary[]>([])
  const [activeId, setActiveId] = useState<string | null>(null)
  const [thread, setThread] = useState<ThreadData | null>(null)
  const [input, setInput] = useState('')
  const [sending, setSending] = useState(false)
  const [loadingInbox, setLoadingInbox] = useState(true)
  const [loadingThread, setLoadingThread] = useState(false)
  const bottomRef = useRef<HTMLDivElement>(null)
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null)

  const loadInbox = () => {
    chatApi.seller
      .inbox()
      .then((d) => {
        const items =
          (d as { items?: ConversationSummary[] }).items ??
          (d as ConversationSummary[]) ??
          []
        setConversations(items)
      })
      .finally(() => setLoadingInbox(false))
  }

  useEffect(() => {
    loadInbox()
  }, [])

  const loadThread = (id: string) => {
    setLoadingThread(true)
    chatApi.seller
      .thread(id)
      .then((d) => {
        setThread(d as ThreadData)
        chatApi.markRead(id).catch(() => {})
        setConversations((prev) =>
          prev.map((c) => (c.conversationId === id ? { ...c, unreadCount: 0 } : c)),
        )
      })
      .finally(() => setLoadingThread(false))
  }

  useEffect(() => {
    if (!activeId) return
    loadThread(activeId)

    if (pollRef.current) clearInterval(pollRef.current)
    pollRef.current = setInterval(() => {
      chatApi.seller
        .conversationUpdate(activeId)
        .then((d) => {
          const update = d as { messages?: Message[] }
          if (update.messages?.length) {
            setThread((prev) =>
              prev ? { ...prev, messages: update.messages! } : prev,
            )
          }
        })
        .catch(() => {})
    }, 5000)

    return () => {
      if (pollRef.current) clearInterval(pollRef.current)
    }
  }, [activeId])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [thread?.messages])

  const handleSend = async () => {
    if (!activeId || !input.trim() || sending) return
    setSending(true)
    try {
      await chatApi.send(activeId, input.trim())
      setInput('')
      loadThread(activeId)
    } catch {
      // ignore
    } finally {
      setSending(false)
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const activeConv = conversations.find((c) => c.conversationId === activeId)

  return (
    <div className="flex h-screen overflow-hidden bg-slate-950 text-slate-100">
      {/* Sidebar */}
      <aside className="flex w-72 shrink-0 flex-col border-r border-white/10 bg-slate-900">
        <div className="flex items-center gap-2 border-b border-white/10 px-5 py-4">
          <span className="material-symbols-outlined text-amber-400 text-[22px]">chat</span>
          <span className="text-base font-bold tracking-wide">Messages</span>
        </div>

        {loadingInbox ? (
          <div className="flex flex-1 items-center justify-center text-sm text-slate-400">
            <span className="material-symbols-outlined animate-spin text-[20px] mr-2">progress_activity</span>
            Loading…
          </div>
        ) : conversations.length === 0 ? (
          <div className="flex flex-1 flex-col items-center justify-center gap-2 px-6 text-center text-sm text-slate-500">
            <span className="material-symbols-outlined text-[36px]">inbox</span>
            No conversations yet.
          </div>
        ) : (
          <div className="flex-1 overflow-y-auto">
            {conversations.map((c) => (
              <button
                key={c.conversationId}
                type="button"
                onClick={() => setActiveId(c.conversationId)}
                className={[
                  'w-full border-b border-white/5 px-5 py-3.5 text-left transition hover:bg-white/5',
                  activeId === c.conversationId
                    ? 'border-l-2 border-l-amber-500 bg-amber-500/10'
                    : 'border-l-2 border-l-transparent',
                ].join(' ')}
              >
                <div className="flex items-start justify-between gap-2">
                  <span className="truncate text-sm font-semibold text-white">{c.buyerName}</span>
                  {c.unreadCount > 0 && (
                    <span className="shrink-0 rounded-full bg-red-500 px-1.5 py-0.5 text-[10px] font-bold text-white">
                      {c.unreadCount}
                    </span>
                  )}
                </div>
                {c.productName && (
                  <p className="mt-0.5 truncate text-xs text-slate-500">{c.productName}</p>
                )}
                {c.lastMessage && (
                  <p className="mt-1 truncate text-xs text-slate-400">{c.lastMessage}</p>
                )}
                {c.lastMessageAt && (
                  <p className="mt-1 text-[10px] text-slate-600">
                    {new Date(c.lastMessageAt).toLocaleTimeString([], {
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </p>
                )}
              </button>
            ))}
          </div>
        )}
      </aside>

      {/* Thread panel */}
      <div className="flex flex-1 flex-col bg-slate-950">
        {!activeId ? (
          <div className="flex flex-1 flex-col items-center justify-center gap-3 text-slate-500">
            <span className="material-symbols-outlined text-[48px]">mark_unread_chat_alt</span>
            <p className="text-sm">Select a conversation to start messaging</p>
          </div>
        ) : (
          <>
            {/* Header */}
            <div className="flex items-center gap-3 border-b border-white/10 bg-slate-900/60 px-6 py-4 backdrop-blur">
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-amber-500 to-orange-600 text-sm font-bold text-white">
                {activeConv?.buyerName?.[0]?.toUpperCase() ?? '?'}
              </div>
              <div className="min-w-0">
                <p className="truncate text-sm font-semibold text-white">
                  {activeConv?.buyerName ?? '…'}
                </p>
                {activeConv?.productName && (
                  <p className="truncate text-xs text-slate-400">re: {activeConv.productName}</p>
                )}
              </div>
            </div>

            {/* Messages */}
            <div className="flex flex-1 flex-col gap-3 overflow-y-auto px-6 py-5">
              {loadingThread ? (
                <div className="flex flex-1 items-center justify-center text-sm text-slate-400">
                  <span className="material-symbols-outlined animate-spin text-[20px] mr-2">progress_activity</span>
                  Loading…
                </div>
              ) : (
                (thread?.messages ?? []).map((msg) => {
                  const isMine = msg.senderName !== activeConv?.buyerName
                  return (
                    <div
                      key={msg.messageId}
                      className={['flex', isMine ? 'justify-end' : 'justify-start'].join(' ')}
                    >
                      <div
                        className={[
                          'max-w-[65%] rounded-2xl px-4 py-2.5 text-sm shadow-sm',
                          isMine
                            ? 'rounded-tr-sm bg-gradient-to-br from-amber-500 to-orange-600 text-white'
                            : 'rounded-tl-sm border border-white/10 bg-white/10 text-slate-100',
                        ].join(' ')}
                      >
                        <p>{msg.content}</p>
                        <p
                          className={[
                            'mt-1 text-right text-[10px]',
                            isMine ? 'text-white/60' : 'text-slate-500',
                          ].join(' ')}
                        >
                          {new Date(msg.sentAt).toLocaleTimeString([], {
                            hour: '2-digit',
                            minute: '2-digit',
                          })}
                        </p>
                      </div>
                    </div>
                  )
                })
              )}
              <div ref={bottomRef} />
            </div>

            {/* Input */}
            <div className="flex items-end gap-3 border-t border-white/10 bg-slate-900/60 px-6 py-4 backdrop-blur">
              <textarea
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder="Type a message… (Enter to send, Shift+Enter for new line)"
                rows={2}
                className="flex-1 resize-none rounded-2xl border border-white/10 bg-white/5 px-4 py-2.5 text-sm text-slate-100 placeholder-slate-500 outline-none transition focus:border-amber-500/50 focus:bg-white/10"
              />
              <button
                type="button"
                onClick={handleSend}
                disabled={sending || !input.trim()}
                className="flex h-10 w-10 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-amber-500 to-orange-600 text-white shadow-lg transition hover:opacity-90 disabled:opacity-40"
              >
                <span className="material-symbols-outlined text-[20px]">
                  {sending ? 'hourglass_empty' : 'send'}
                </span>
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
