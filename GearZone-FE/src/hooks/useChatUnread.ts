/* eslint-disable react-hooks/set-state-in-effect */
import { useEffect, useState } from 'react'
import { getBuyerUnread } from '@/api/chat'
import type { ChatHub } from '@/hooks/useChatHub'

/** App-wide total unread count, seeded from the API and kept live via the hub. */
export function useChatUnread(hub: ChatHub, enabled: boolean): number {
  const [totalUnread, setTotalUnread] = useState(0)

  useEffect(() => {
    if (!enabled) {
      setTotalUnread(0)
      return
    }
    let active = true
    async function load() {
      try {
        const count = await getBuyerUnread()
        if (active) setTotalUnread(count)
      } catch {
        /* ignore — badge stays at its last known value */
      }
    }
    void load()
    return () => {
      active = false
    }
  }, [enabled])

  useEffect(() => {
    const unsubscribe = hub.subscribe({
      onUnreadCountsUpdated: (payload) => setTotalUnread(payload.totalUnreadCount),
    })
    return unsubscribe
  }, [hub])

  return totalUnread
}
