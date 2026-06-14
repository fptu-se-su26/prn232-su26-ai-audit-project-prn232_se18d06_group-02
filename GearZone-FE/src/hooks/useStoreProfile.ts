import { useCallback, useEffect, useState } from 'react'
import { getStoreProfile } from '@/api/stores'
import type { StoreProfile } from '@/types/store'

interface UseStoreProfileResult {
  store: StoreProfile | null
  loading: boolean
  error: string | null
  notFound: boolean
}

const NOT_FOUND_PATTERN = /not\s*found/i

export function useStoreProfile(slug: string | undefined): UseStoreProfileResult {
  const [store, setStore] = useState<StoreProfile | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [notFound, setNotFound] = useState(false)

  const load = useCallback(async (storeSlug: string) => {
    setLoading(true)
    setError(null)
    setNotFound(false)
    try {
      const profile = await getStoreProfile(storeSlug)
      setStore(profile)
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to load store.'
      setStore(null)
      if (NOT_FOUND_PATTERN.test(message)) {
        setNotFound(true)
      } else {
        setError(message)
      }
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (!slug) {
      setStore(null)
      setLoading(false)
      setNotFound(true)
      return
    }
    void load(slug)
  }, [slug, load])

  return { store, loading, error, notFound }
}
