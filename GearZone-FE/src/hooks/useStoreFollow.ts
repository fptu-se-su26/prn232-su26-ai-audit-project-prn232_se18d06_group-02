import { useEffect, useState } from 'react'
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
  const [isFollowing, setIsFollowing] = useState(false)
  const [followerCount, setFollowerCount] = useState(0)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    setIsFollowing(store?.isFollowing ?? false)
    setFollowerCount(store?.followerCount ?? 0)
  }, [store])

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
