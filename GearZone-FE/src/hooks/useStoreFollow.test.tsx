import { act, renderHook, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { useStoreFollow } from '@/hooks/useStoreFollow'
import type { StoreProfile } from '@/types/store'

const toggleStoreFollow = vi.fn()
const navigate = vi.fn()
let mockUser: { id: string } | null = { id: 'u1' }

vi.mock('@/api/stores', () => ({
  toggleStoreFollow: (slug: string) => toggleStoreFollow(slug),
}))
vi.mock('@/contexts/useAuth', () => ({
  useAuth: () => ({ user: mockUser }),
}))
vi.mock('react-router-dom', () => ({
  useNavigate: () => navigate,
}))

const store = {
  id: 's1',
  slug: 'acme',
  isFollowing: false,
  followerCount: 10,
} as StoreProfile

afterEach(() => {
  vi.clearAllMocks()
  mockUser = { id: 'u1' }
})

describe('useStoreFollow', () => {
  it('optimistically toggles then reconciles with the API result', async () => {
    toggleStoreFollow.mockResolvedValue({ isFollowing: true, followerCount: 11 })
    const { result } = renderHook(() => useStoreFollow(store))

    act(() => result.current.toggle())
    // optimistic update applied immediately
    expect(result.current.isFollowing).toBe(true)
    expect(result.current.followerCount).toBe(11)

    await waitFor(() => expect(result.current.pending).toBe(false))
    expect(result.current.followerCount).toBe(11)
    expect(toggleStoreFollow).toHaveBeenCalledWith('acme')
  })

  it('reverts the optimistic update when the API fails', async () => {
    toggleStoreFollow.mockRejectedValue(new Error('boom'))
    const { result } = renderHook(() => useStoreFollow(store))

    act(() => result.current.toggle())
    await waitFor(() => expect(result.current.pending).toBe(false))

    expect(result.current.isFollowing).toBe(false)
    expect(result.current.followerCount).toBe(10)
    expect(result.current.error).toBe('boom')
  })

  it('redirects anonymous users to login instead of toggling', () => {
    mockUser = null
    const { result } = renderHook(() => useStoreFollow(store))

    act(() => result.current.toggle())

    expect(navigate).toHaveBeenCalledWith('/login')
    expect(toggleStoreFollow).not.toHaveBeenCalled()
    expect(result.current.isFollowing).toBe(false)
  })
})
