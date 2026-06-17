import { useEffect, useRef, useState } from 'react';
import { chatApi } from '../../api/chat';

interface ConversationSummary {
  conversationId: string;
  buyerName: string;
  buyerAvatar?: string;
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount: number;
  productName?: string;
}

interface Message {
  messageId: string;
  senderId: string;
  senderName: string;
  content: string;
  sentAt: string;
  isRead: boolean;
}

interface ThreadData {
  conversationId: string;
  buyerName: string;
  messages: Message[];
}

export default function SellerMessagesPage() {
  const [conversations, setConversations] = useState<ConversationSummary[]>([]);
  const [activeId, setActiveId] = useState<string | null>(null);
  const [thread, setThread] = useState<ThreadData | null>(null);
  const [input, setInput] = useState('');
  const [sending, setSending] = useState(false);
  const [loadingInbox, setLoadingInbox] = useState(true);
  const [loadingThread, setLoadingThread] = useState(false);
  const bottomRef = useRef<HTMLDivElement>(null);
  const pollRef = useRef<ReturnType<typeof setInterval> | null>(null);

  const loadInbox = () => {
    chatApi.seller.inbox()
      .then(d => {
        const items = (d as { items?: ConversationSummary[] }).items ?? (d as ConversationSummary[]) ?? [];
        setConversations(items);
      })
      .finally(() => setLoadingInbox(false));
  };

  useEffect(() => {
    loadInbox();
  }, []);

  const loadThread = (id: string) => {
    setLoadingThread(true);
    chatApi.seller.thread(id)
      .then(d => {
        setThread(d as ThreadData);
        chatApi.markRead(id).catch(() => {});
        setConversations(prev =>
          prev.map(c => c.conversationId === id ? { ...c, unreadCount: 0 } : c)
        );
      })
      .finally(() => setLoadingThread(false));
  };

  useEffect(() => {
    if (!activeId) return;
    loadThread(activeId);

    if (pollRef.current) clearInterval(pollRef.current);
    pollRef.current = setInterval(() => {
      chatApi.seller.conversationUpdate(activeId)
        .then(d => {
          const update = d as { messages?: Message[] };
          if (update.messages?.length) {
            setThread(prev => prev ? { ...prev, messages: update.messages! } : prev);
          }
        })
        .catch(() => {});
    }, 5000);

    return () => {
      if (pollRef.current) clearInterval(pollRef.current);
    };
  }, [activeId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [thread?.messages]);

  const handleSend = async () => {
    if (!activeId || !input.trim() || sending) return;
    setSending(true);
    try {
      await chatApi.send(activeId, input.trim());
      setInput('');
      loadThread(activeId);
    } catch {
      // ignore
    } finally {
      setSending(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handleSend(); }
  };

  const activeConv = conversations.find(c => c.conversationId === activeId);

  return (
    <div style={{ display: 'flex', height: 'calc(100vh - 64px)', overflow: 'hidden' }}>
      {/* Sidebar */}
      <div style={{ width: 300, borderRight: '1px solid #e2e8f0', display: 'flex', flexDirection: 'column', background: '#fff', flexShrink: 0 }}>
        <div style={{ padding: '1rem 1.25rem', borderBottom: '1px solid #e2e8f0', fontWeight: 700, fontSize: 16 }}>
          Messages
        </div>
        {loadingInbox ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: '#888', fontSize: 13 }}>Loading…</div>
        ) : conversations.length === 0 ? (
          <div style={{ padding: '2rem', textAlign: 'center', color: '#aaa', fontSize: 13 }}>No conversations yet.</div>
        ) : (
          <div style={{ overflowY: 'auto', flex: 1 }}>
            {conversations.map(c => (
              <div
                key={c.conversationId}
                onClick={() => setActiveId(c.conversationId)}
                style={{
                  padding: '0.85rem 1.25rem',
                  cursor: 'pointer',
                  borderBottom: '1px solid #f5f5f5',
                  background: activeId === c.conversationId ? '#ebf8ff' : '#fff',
                  borderLeft: activeId === c.conversationId ? '3px solid #3182ce' : '3px solid transparent',
                }}
              >
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                  <div style={{ fontWeight: 600, fontSize: 14, color: '#222' }}>{c.buyerName}</div>
                  {c.unreadCount > 0 && (
                    <span style={{ background: '#e53e3e', color: '#fff', borderRadius: 10, fontSize: 11, padding: '1px 6px', fontWeight: 700 }}>
                      {c.unreadCount}
                    </span>
                  )}
                </div>
                {c.productName && (
                  <div style={{ fontSize: 11, color: '#888', marginTop: 2 }}>{c.productName}</div>
                )}
                {c.lastMessage && (
                  <div style={{ fontSize: 12, color: '#999', marginTop: 3, whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis', maxWidth: 220 }}>
                    {c.lastMessage}
                  </div>
                )}
                {c.lastMessageAt && (
                  <div style={{ fontSize: 11, color: '#bbb', marginTop: 2 }}>
                    {new Date(c.lastMessageAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Thread panel */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', background: '#f9f9f9' }}>
        {!activeId ? (
          <div style={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#aaa', fontSize: 14 }}>
            Select a conversation to start messaging
          </div>
        ) : (
          <>
            {/* Header */}
            <div style={{ padding: '0.85rem 1.5rem', borderBottom: '1px solid #e2e8f0', background: '#fff', fontWeight: 600, fontSize: 15 }}>
              {activeConv?.buyerName ?? '…'}
              {activeConv?.productName && (
                <span style={{ fontWeight: 400, fontSize: 12, color: '#888', marginLeft: '0.5rem' }}>
                  re: {activeConv.productName}
                </span>
              )}
            </div>

            {/* Messages */}
            <div style={{ flex: 1, overflowY: 'auto', padding: '1.25rem 1.5rem', display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              {loadingThread ? (
                <div style={{ textAlign: 'center', color: '#aaa', fontSize: 13 }}>Loading…</div>
              ) : (thread?.messages ?? []).map(msg => {
                const isMine = msg.senderName !== activeConv?.buyerName;
                return (
                  <div key={msg.messageId} style={{ display: 'flex', justifyContent: isMine ? 'flex-end' : 'flex-start' }}>
                    <div style={{
                      maxWidth: '65%',
                      background: isMine ? '#3182ce' : '#fff',
                      color: isMine ? '#fff' : '#222',
                      borderRadius: isMine ? '12px 12px 2px 12px' : '12px 12px 12px 2px',
                      padding: '0.5rem 0.85rem',
                      fontSize: 14,
                      boxShadow: '0 1px 2px rgba(0,0,0,0.06)',
                      border: isMine ? 'none' : '1px solid #e2e8f0',
                    }}>
                      <div>{msg.content}</div>
                      <div style={{ fontSize: 10, marginTop: 3, opacity: 0.65, textAlign: 'right' }}>
                        {new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </div>
                    </div>
                  </div>
                );
              })}
              <div ref={bottomRef} />
            </div>

            {/* Input */}
            <div style={{ padding: '0.75rem 1.5rem', borderTop: '1px solid #e2e8f0', background: '#fff', display: 'flex', gap: '0.5rem', alignItems: 'flex-end' }}>
              <textarea
                value={input}
                onChange={e => setInput(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder="Type a message… (Enter to send)"
                rows={2}
                style={{ flex: 1, padding: '0.5rem 0.75rem', border: '1px solid #e2e8f0', borderRadius: 8, resize: 'none', fontSize: 14, outline: 'none', boxSizing: 'border-box' }}
              />
              <button
                onClick={handleSend}
                disabled={sending || !input.trim()}
                style={{ padding: '0.5rem 1.25rem', background: '#3182ce', color: '#fff', border: 'none', borderRadius: 8, cursor: 'pointer', fontWeight: 600, fontSize: 14, height: 40 }}
              >
                {sending ? '…' : 'Send'}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
