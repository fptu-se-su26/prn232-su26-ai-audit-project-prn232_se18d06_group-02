import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { toggleStoreFollow } from '@/api/stores'
import { useAuth } from '@/contexts/useAuth'
import type { StoreProfile } from '@/types/store'

interface UseStoreFollowResult {
  isFollowing: boolean
  followerCount: number
  pending: boolean
  error: string | null
  toggle: () => void
}

export function useStoreFollow(store: StoreProfile | null): UseStoreFollowResult {
  const { user } = useAuth()
  const navigate = useNavigate()
  const [isFollowing, setIsFollowing] = useState(store?.isFollowing ?? false)
  const [followerCount, setFollowerCount] = useState(store?.followerCount ?? 0)
  const [syncedId, setSyncedId] = useState(store?.id)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Reconcile local state when a different store loads (adjust state during render).
  if (store && store.id !== syncedId) {
    setSyncedId(store.id)
    setIsFollowing(store.isFollowing)
    setFollowerCount(store.followerCount)
  }

  const toggle = () => {
    if (!store || pending) return
    if (!user) {
      navigate('/login')
      return
    }

    const prevFollowing = isFollowing
    const prevCount = followerCount

    setPending(true)
    setError(null)
    setIsFollowing(!prevFollowing)
    setFollowerCount(prevCount + (prevFollowing ? -1 : 1))

    toggleStoreFollow(store.slug)
      .then((result) => {
        setIsFollowing(result.isFollowing)
        setFollowerCount(result.followerCount)
      })
      .catch((err: unknown) => {
        setIsFollowing(prevFollowing)
        setFollowerCount(prevCount)
        setError(err instanceof Error ? err.message : 'Failed to update follow.')
      })
      .finally(() => setPending(false))
  }

  return { isFollowing, followerCount, pending, error, toggle }
}
