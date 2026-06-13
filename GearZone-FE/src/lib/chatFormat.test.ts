import { describe, expect, it } from 'vitest'
import {
  formatDateSeparator,
  formatListTime,
  formatMessageTime,
  groupMessagesByDate,
} from '@/lib/chatFormat'
import type { ChatMessageItem } from '@/types/chat'

function message(id: string, sentAt: string): ChatMessageItem {
  return {
    id,
    conversationId: 'c1',
    senderUserId: 'u1',
    senderDisplayName: 'Buyer',
    content: 'hello',
    sentAt,
    isRead: false,
  }
}

describe('formatMessageTime', () => {
  it('formats a local time as HH:mm', () => {
    expect(formatMessageTime('2026-03-07T09:05:00')).toBe('09:05')
  })

  it('returns empty string for an invalid date', () => {
    expect(formatMessageTime('not-a-date')).toBe('')
  })
})

describe('formatDateSeparator', () => {
  it('formats as dd/MM/yyyy', () => {
    expect(formatDateSeparator('2026-03-07T09:05:00')).toBe('07/03/2026')
  })
})

describe('formatListTime', () => {
  it('shows the time when the message is from today', () => {
    expect(formatListTime('2026-03-07T09:05:00', new Date('2026-03-07T20:00:00'))).toBe('09:05')
  })

  it('shows dd/MM when the message is from another day', () => {
    expect(formatListTime('2026-03-07T09:05:00', new Date('2026-03-10T08:00:00'))).toBe('07/03')
  })
})

describe('groupMessagesByDate', () => {
  it('groups consecutive messages that share a calendar day', () => {
    const groups = groupMessagesByDate([
      message('a', '2026-03-07T09:00:00'),
      message('b', '2026-03-07T10:00:00'),
      message('c', '2026-03-08T08:00:00'),
    ])
    expect(groups).toHaveLength(2)
    expect(groups[0].label).toBe('07/03/2026')
    expect(groups[0].messages).toHaveLength(2)
    expect(groups[1].label).toBe('08/03/2026')
    expect(groups[1].messages).toHaveLength(1)
  })
})
