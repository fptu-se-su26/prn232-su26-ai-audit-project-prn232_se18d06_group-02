import { useEffect, useRef, useState, useCallback } from 'react';
import { chatApi } from '../../api/chat';
import { useAuth } from '../../contexts/AuthContext';

interface Conversation {
  id: string;
  storeSlug?: string;
  storeName?: string;
  storeLogoUrl?: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount?: number;
}

interface Message {
  id: string;
  content: string;
  senderType: 'buyer' | 'seller' | string;
  createdAt: string;
  senderName?: string;
}

type WidgetStatus = 'closed' | 'open' | 'minimized';

const POLL_INTERVAL = 5000;

function formatTime(iso?: string) {
  if (!iso) return '';
  const d = new Date(iso);
  const now = new Date();
  if (d.toDateString() === now.toDateString()) {
    return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  }
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
}

export default function BuyerChatWidget() {
  const { user } = useAuth();
  const [status, setStatus] = useState<WidgetStatus>('closed');
  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeConvId, setActiveConvId] = useState<string | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [unreadTotal, setUnreadTotal] = useState(0);
  const [messageInput, setMessageInput] = useState('');
  const [sending, setSending] = useState(false);
  const [loadingInbox, setLoadingInbox] = useState(false);
  const [loadingThread, setLoadingThread] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const fetchInbox = useCallback(async (silent = false) => {
    if (!user) return;
    if (!silent) setLoadingInbox(true);
    try {
      const res = await chatApi.buyer.inbox();
      const convs = ((res as { conversations?: Conversation[] }).conversations ?? (res as Conversation[])) ?? [];
      setConversations(convs);
      const total = convs.reduce((sum: number, c: Conversation) => sum + (c.unreadCount ?? 0), 0);
      setUnreadTotal(total);
    } catch { } finally {
      if (!silent) setLoadingInbox(false);
    }
  }, [user]);

  const fetchThread = useCallback(async (convId: string, silent = false) => {
    if (!silent) setLoadingThread(true);
    try {
      const res = await chatApi.buyer.thread(convId);
      const msgs = ((res as { messages?: Message[] }).messages ?? (res as Message[])) ?? [];
      setMessages(msgs);
    } catch { } finally {
      if (!silent) setLoadingThread(false);
    }
  }, []);

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  };

  useEffect(() => {
    if (messages.length > 0) scrollToBottom();
  }, [messages]);

  const openWidget = useCallback(async (storeSlug?: string) => {
    if (!user) return;
    setStatus('open');

    if (storeSlug) {
      try {
        const res = await chatApi.buyer.ensureConversation(storeSlug);
        const convId = (res as { id?: string; conversationId?: string }).id
          ?? (res as { id?: string; conversationId?: string }).conversationId ?? '';
        if (convId) {
          await fetchInbox();
          setActiveConvId(convId);
          await fetchThread(convId);
          await chatApi.markRead(convId).catch(() => {});
          return;
        }
      } catch { }
    }
    await fetchInbox();
  }, [user, fetchInbox, fetchThread]);

  // Listen for open-chat events from product/store pages
  useEffect(() => {
    const handler = (e: Event) => {
      const detail = (e as CustomEvent<{ storeSlug?: string }>).detail;
      openWidget(detail?.storeSlug);
    };
    window.addEventListener('gearzone:open-chat', handler);
    return () => window.removeEventListener('gearzone:open-chat', handler);
  }, [openWidget]);

  // Poll for unread count
  useEffect(() => {
    if (!user) { setUnreadTotal(0); return; }
    chatApi.buyer.unread().then(res => {
      const count = (res as { count?: number; unreadCount?: number }).count
        ?? (res as { count?: number; unreadCount?: number }).unreadCount ?? 0;
      setUnreadTotal(count);
    }).catch(() => {});
  }, [user]);

  // Start/stop polling when widget is open
  useEffect(() => {
    if (status === 'open' && user) {
      pollRef.current = setInterval(async () => {
        await fetchInbox(true);
        if (activeConvId) await fetchThread(activeConvId, true);
      }, POLL_INTERVAL);
    }
    return () => { if (pollRef.current) clearInterval(pollRef.current); };
  }, [status, user, activeConvId, fetchInbox, fetchThread]);

  const handleSelectConversation = async (conv: Conversation) => {
    setActiveConvId(conv.id);
    await fetchThread(conv.id);
    await chatApi.markRead(conv.id).catch(() => {});
    setConversations(cs => cs.map(c => c.id === conv.id ? { ...c, unreadCount: 0 } : c));
    setUnreadTotal(n => Math.max(0, n - (conv.unreadCount ?? 0)));
  };

  const handleSend = async () => {
    if (!activeConvId || !messageInput.trim()) return;
    setSending(true);
    try {
      await chatApi.send(activeConvId, messageInput.trim());
      setMessageInput('');
      await fetchThread(activeConvId, true);
    } catch { } finally { setSending(false); }
  };

  const handleOpen = () => {
    if (status === 'open') { setStatus('minimized'); return; }
    openWidget();
  };

  const activeConv = conversations.find(c => c.id === activeConvId);

  if (!user) return null;

  return (
    <div className="fixed bottom-5 right-5 z-[200] flex flex-col items-end gap-3">
      {/* Drawer */}
      {status === 'open' && (
        <div className="w-[360px] max-w-[calc(100vw-2rem)] bg-white rounded-2xl shadow-[0_20px_60px_-12px_rgba(0,0,0,0.3)] border border-gray-100 flex flex-col overflow-hidden"
          style={{ height: 520 }}>
          {/* Header */}
          <div className="flex items-center justify-between px-4 py-3 border-b border-gray-100 bg-primary text-white">
            <div className="flex items-center gap-2">
              {activeConvId && activeConv ? (
                <button onClick={() => setActiveConvId(null)}
                  className="mr-1 p-0.5 rounded hover:bg-white/20 transition-colors">
                  <span className="material-symbols-outlined text-[18px]">arrow_back</span>
                </button>
              ) : null}
              <span className="material-symbols-outlined text-[18px]">chat</span>
              <span className="text-sm font-bold">
                {activeConv ? activeConv.storeName ?? 'Chat' : 'Messages'}
              </span>
              {unreadTotal > 0 && !activeConvId && (
                <span className="ml-1 bg-white/20 text-white text-[10px] font-bold px-1.5 py-0.5 rounded-full">{unreadTotal}</span>
              )}
            </div>
            <div className="flex items-center gap-1">
              <button onClick={() => setStatus('minimized')}
                className="p-1 rounded hover:bg-white/20 transition-colors" title="Minimize">
                <span className="material-symbols-outlined text-[18px]">remove</span>
              </button>
              <button onClick={() => { setStatus('closed'); setActiveConvId(null); }}
                className="p-1 rounded hover:bg-white/20 transition-colors" title="Close">
                <span className="material-symbols-outlined text-[18px]">close</span>
              </button>
            </div>
          </div>

          {/* Body */}
          <div className="flex-1 overflow-hidden flex flex-col">
            {!activeConvId ? (
              /* Conversation List */
              <div className="flex-1 overflow-y-auto">
                {loadingInbox ? (
                  <div className="flex items-center justify-center py-12">
                    <div className="animate-spin w-6 h-6 border-3 border-primary border-t-transparent rounded-full" />
                  </div>
                ) : conversations.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-16 text-center text-gray-400 px-6">
                    <span className="material-symbols-outlined text-5xl text-gray-200 mb-3">forum</span>
                    <p className="text-sm font-medium">No conversations yet.</p>
                    <p className="text-xs mt-1">Visit a store and tap Chat to start.</p>
                  </div>
                ) : (
                  conversations.map(conv => (
                    <button key={conv.id} onClick={() => handleSelectConversation(conv)}
                      className="w-full flex items-center gap-3 px-4 py-3 hover:bg-gray-50 transition-colors border-b border-gray-50 text-left">
                      <div className="w-10 h-10 rounded-full bg-blue-50 flex items-center justify-center flex-shrink-0 overflow-hidden">
                        {conv.storeLogoUrl
                          ? <img src={conv.storeLogoUrl} alt={conv.storeName} className="w-full h-full object-cover" />
                          : <span className="text-primary font-bold text-sm">{(conv.storeName ?? '?')[0]}</span>
                        }
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center justify-between">
                          <span className={`text-sm ${(conv.unreadCount ?? 0) > 0 ? 'font-bold text-gray-900' : 'font-medium text-gray-700'}`}>
                            {conv.storeName ?? 'Store'}
                          </span>
                          <span className="text-[11px] text-gray-400 flex-shrink-0 ml-1">{formatTime(conv.lastMessageAt)}</span>
                        </div>
                        <p className={`text-xs truncate mt-0.5 ${(conv.unreadCount ?? 0) > 0 ? 'text-gray-700 font-semibold' : 'text-gray-400'}`}>
                          {conv.lastMessage ?? 'No messages yet'}
                        </p>
                      </div>
                      {(conv.unreadCount ?? 0) > 0 && (
                        <span className="bg-red-500 text-white text-[10px] font-bold min-w-[18px] h-[18px] px-1 rounded-full flex items-center justify-center flex-shrink-0">
                          {conv.unreadCount}
                        </span>
                      )}
                    </button>
                  ))
                )}
              </div>
            ) : (
              /* Thread View */
              <>
                <div className="flex-1 overflow-y-auto px-4 py-3 space-y-3">
                  {loadingThread ? (
                    <div className="flex items-center justify-center py-10">
                      <div className="animate-spin w-6 h-6 border-2 border-primary border-t-transparent rounded-full" />
                    </div>
                  ) : messages.length === 0 ? (
                    <div className="flex flex-col items-center justify-center py-10 text-gray-400 text-center">
                      <span className="material-symbols-outlined text-4xl text-gray-200 mb-2">chat_bubble</span>
                      <p className="text-xs">Start a conversation!</p>
                    </div>
                  ) : (
                    messages.map(msg => {
                      const isBuyer = msg.senderType === 'buyer';
                      return (
                        <div key={msg.id} className={`flex ${isBuyer ? 'justify-end' : 'justify-start'}`}>
                          <div className={`max-w-[75%] rounded-2xl px-3.5 py-2.5 text-sm ${isBuyer
                            ? 'bg-primary text-white rounded-tr-sm'
                            : 'bg-gray-100 text-gray-900 rounded-tl-sm'}`}>
                            <p className="leading-relaxed break-words">{msg.content}</p>
                            <p className={`text-[10px] mt-1 ${isBuyer ? 'text-white/70' : 'text-gray-400'}`}>
                              {formatTime(msg.createdAt)}
                            </p>
                          </div>
                        </div>
                      );
                    })
                  )}
                  <div ref={messagesEndRef} />
                </div>

                {/* Input */}
                <div className="px-3 py-2.5 border-t border-gray-100 flex items-center gap-2">
                  <input
                    value={messageInput}
                    onChange={e => setMessageInput(e.target.value)}
                    onKeyDown={e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSend(); } }}
                    placeholder="Type a message…"
                    className="flex-1 text-sm px-3.5 py-2.5 bg-gray-50 rounded-xl border border-gray-200 focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all"
                    disabled={sending}
                  />
                  <button onClick={handleSend} disabled={sending || !messageInput.trim()}
                    className="w-9 h-9 rounded-xl bg-primary hover:bg-blue-700 disabled:bg-gray-200 text-white flex items-center justify-center flex-shrink-0 transition-colors">
                    <span className="material-symbols-outlined text-[18px]">send</span>
                  </button>
                </div>
              </>
            )}
          </div>
        </div>
      )}

      {/* Bubble Button */}
      <button onClick={handleOpen}
        className="w-14 h-14 rounded-full bg-primary hover:bg-blue-700 text-white shadow-[0_8px_24px_-6px_rgba(26,87,219,0.55)] hover:shadow-[0_12px_28px_-6px_rgba(26,87,219,0.65)] flex items-center justify-center transition-all hover:-translate-y-0.5 relative"
        aria-label="Open chat">
        <span className="material-symbols-outlined text-[24px]"
          style={{ fontVariationSettings: "'FILL' 1" }}>chat</span>
        {unreadTotal > 0 && (
          <span className="absolute -top-1 -right-1 bg-red-500 text-white text-[10px] font-bold min-w-[20px] h-5 px-1 rounded-full flex items-center justify-center border-2 border-white">
            {unreadTotal > 99 ? '99+' : unreadTotal}
          </span>
        )}
        <span className="absolute inset-0 rounded-full bg-white opacity-0 hover:opacity-10 transition-opacity" />
      </button>
    </div>
  );
}
