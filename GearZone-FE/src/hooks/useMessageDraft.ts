/* eslint-disable react-hooks/set-state-in-effect */
import { useCallback, useEffect, useState } from 'react'
import { readSession, removeSession, writeSession } from '@/lib/sessionStore'

const DRAFT_PREFIX = 'gearzone_chat_draft:'

/** Per-conversation composer draft persisted in sessionStorage. */
export function useMessageDraft(
  conversationId: string | null,
): [string, (value: string) => void, () => void] {
  const [draft, setDraft] = useState('')

  useEffect(() => {
    if (!conversationId) {
      setDraft('')
      return
    }
    setDraft(readSession<string>(`${DRAFT_PREFIX}${conversationId}`, ''))
  }, [conversationId])

  const updateDraft = useCallback(
    (value: string) => {
      setDraft(value)
      if (!conversationId) return
      if (value) writeSession(`${DRAFT_PREFIX}${conversationId}`, value)
      else removeSession(`${DRAFT_PREFIX}${conversationId}`)
    },
    [conversationId],
  )

  const clearDraft = useCallback(() => {
    setDraft('')
    if (conversationId) removeSession(`${DRAFT_PREFIX}${conversationId}`)
  }, [conversationId])

  return [draft, updateDraft, clearDraft]
}
