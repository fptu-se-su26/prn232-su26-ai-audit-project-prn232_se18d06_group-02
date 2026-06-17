import type { ChatMessageItem } from '@/types/chat'

function pad2(value: number): string {
  return value < 10 ? `0${value}` : String(value)
}

function isSameDay(a: Date, b: Date): boolean {
  return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate()
}

/** Time of day in 24h local time, e.g. "09:05". */
export function formatMessageTime(iso: string): string {
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  return `${pad2(date.getHours())}:${pad2(date.getMinutes())}`
}

/** Day separator label, e.g. "07/03/2026". */
export function formatDateSeparator(iso: string): string {
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  return `${pad2(date.getDate())}/${pad2(date.getMonth() + 1)}/${date.getFullYear()}`
}

/** Conversation-list timestamp: time if today, otherwise "dd/MM". */
export function formatListTime(iso: string, now: Date = new Date()): string {
  const date = new Date(iso)
  if (Number.isNaN(date.getTime())) return ''
  if (isSameDay(date, now)) return `${pad2(date.getHours())}:${pad2(date.getMinutes())}`
  return `${pad2(date.getDate())}/${pad2(date.getMonth() + 1)}`
}

export interface MessageDateGroup {
  dateKey: string
  label: string
  messages: ChatMessageItem[]
}

/** Groups consecutive messages that fall on the same calendar day. */
export function groupMessagesByDate(messages: ChatMessageItem[]): MessageDateGroup[] {
  const groups: MessageDateGroup[] = []
  for (const message of messages) {
    const label = formatDateSeparator(message.sentAt)
    const last = groups[groups.length - 1]
    if (last && last.dateKey === label) {
      last.messages.push(message)
    } else {
      groups.push({ dateKey: label, label, messages: [message] })
    }
  }
  return groups
}
