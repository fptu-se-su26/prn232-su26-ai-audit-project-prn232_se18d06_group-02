import { describe, expect, it } from 'vitest'
import { render, screen } from '@testing-library/react'
import MessageBubble from '@/components/chat/MessageBubble'
import type { ChatMessageItem } from '@/types/chat'

function makeMessage(overrides: Partial<ChatMessageItem> = {}): ChatMessageItem {
  return {
    id: 'm1',
    conversationId: 'c1',
    senderUserId: 'buyer-1',
    senderDisplayName: 'Shop ABC',
    content: 'Hello there',
    sentAt: '2026-03-07T09:05:00',
    isRead: false,
    ...overrides,
  }
}

describe('MessageBubble', () => {
  it('shows the sender name and content for incoming messages', () => {
    render(<MessageBubble message={makeMessage()} isOwn={false} />)
    expect(screen.getByText('Shop ABC')).toBeInTheDocument()
    expect(screen.getByText('Hello there')).toBeInTheDocument()
  })

  it('hides the sender name and shows Seen for own read messages', () => {
    render(<MessageBubble message={makeMessage({ isRead: true })} isOwn />)
    expect(screen.queryByText('Shop ABC')).not.toBeInTheDocument()
    expect(screen.getByText('Seen')).toBeInTheDocument()
  })

  it('does not show Seen for own unread messages', () => {
    render(<MessageBubble message={makeMessage({ isRead: false })} isOwn />)
    expect(screen.queryByText('Seen')).not.toBeInTheDocument()
  })
})
