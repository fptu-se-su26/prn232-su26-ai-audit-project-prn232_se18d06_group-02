/* eslint-disable react-hooks/set-state-in-effect */
import { useCallback, useEffect, useState } from 'react'
import { getBuyerInbox, getBuyerScopeOptions } from '@/api/chat'
import { useDebouncedValue } from '@/hooks/useDebouncedValue'
import {
  CHAT_INBOX_PAGE_SIZE,
  type ChatConversationListItem,
  type ChatCounterpartScopeOption,
  type ChatFilter,
} from '@/types/chat'
import type { ChatHub } from '@/hooks/useChatHub'

export interface UseChatConversationsResult {
  items: ChatConversationListItem[]
  filter: ChatFilter
  setFilter: (filter: ChatFilter) => void
  searchInput: string
  setSearchInput: (value: string) => void
  scopeKey: string
  setScopeKey: (value: string) => void
  scopeOptions: ChatCounterpartScopeOption[]
  loading: boolean
  error: string | null
  hasMore: boolean
  loadMore: () => Promise<void>
  refresh: () => Promise<void>
}

/** Paginated, filterable conversation list that silently refreshes on live updates. */
export function useChatConversations(hub: ChatHub): UseChatConversationsResult {
  const [items, setItems] = useState<ChatConversationListItem[]>([])
  const [filter, setFilter] = useState<ChatFilter>('all')
  const [searchInput, setSearchInput] = useState('')
  const searchTerm = useDebouncedValue(searchInput, 250)
  const [scopeKey, setScopeKey] = useState('')
  const [scopeOptions, setScopeOptions] = useState<ChatCounterpartScopeOption[]>([])
  const [pageNumber, setPageNumber] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    let active = true
    getBuyerScopeOptions()
      .then((options) => {
        if (active) setScopeOptions(options)
      })
      .catch(() => undefined)
    return () => {
      active = false
    }
  }, [])

  useEffect(() => {
    let active = true
    setLoading(true)
    setError(null)
    getBuyerInbox({
      filter,
      searchTerm: searchTerm || undefined,
      counterpartScopeKey: scopeKey || undefined,
      pageNumber: 1,
      pageSize: CHAT_INBOX_PAGE_SIZE,
    })
      .then((page) => {
        if (!active) return
        setItems(page.items)
        setTotalPages(page.totalPages)
        setPageNumber(1)
      })
      .catch((err: unknown) => {
        if (active) setError(err instanceof Error ? err.message : 'Unable to load conversations.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => {
      active = false
    }
  }, [filter, searchTerm, scopeKey])

  const refresh = useCallback(async () => {
    const page = await getBuyerInbox({
      filter,
      searchTerm: searchTerm || undefined,
      counterpartScopeKey: scopeKey || undefined,
      pageNumber: 1,
      pageSize: CHAT_INBOX_PAGE_SIZE,
    })
    setItems(page.items)
    setTotalPages(page.totalPages)
    setPageNumber(1)
  }, [filter, searchTerm, scopeKey])

  const loadMore = useCallback(async () => {
    if (pageNumber >= totalPages || loading) return
    const nextPage = pageNumber + 1
    const page = await getBuyerInbox({
      filter,
      searchTerm: searchTerm || undefined,
      counterpartScopeKey: scopeKey || undefined,
      pageNumber: nextPage,
      pageSize: CHAT_INBOX_PAGE_SIZE,
    })
    setItems((prev) => [...prev, ...page.items])
    setPageNumber(nextPage)
  }, [pageNumber, totalPages, loading, filter, searchTerm, scopeKey])

  useEffect(() => {
    const unsubscribe = hub.subscribe({
      onConversationUpdated: () => {
        void refresh()
      },
    })
    return unsubscribe
  }, [hub, refresh])

  return {
    items,
    filter,
    setFilter,
    searchInput,
    setSearchInput,
    scopeKey,
    setScopeKey,
    scopeOptions,
    loading,
    error,
    hasMore: pageNumber < totalPages,
    loadMore,
    refresh,
  }
}
