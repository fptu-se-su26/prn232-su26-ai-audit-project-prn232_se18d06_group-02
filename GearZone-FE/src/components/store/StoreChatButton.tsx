import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/contexts/useAuth'
import { useChatContext } from '@/contexts/useChatContext'

interface StoreChatButtonProps {
  storeSlug: string
}

export default function StoreChatButton({ storeSlug }: StoreChatButtonProps) {
  const { user } = useAuth()
  const { openChatWithStore } = useChatContext()
  const navigate = useNavigate()
  const [pending, setPending] = useState(false)

  const handleClick = async () => {
    if (!user) {
      navigate('/login')
      return
    }
    setPending(true)
    try {
      await openChatWithStore(storeSlug)
    } finally {
      setPending(false)
    }
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      disabled={pending}
      className="inline-flex items-center gap-1.5 rounded-lg border border-white/20 bg-white/5 px-4 py-2 text-sm font-semibold text-white transition hover:bg-white/15 disabled:cursor-not-allowed disabled:opacity-60"
    >
      <span className="material-symbols-outlined text-[18px]">chat</span>
      Chat
    </button>
  )
}
