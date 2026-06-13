import { useLocation } from 'react-router-dom'
import ChatLauncher from '@/components/chat/ChatLauncher'
import ChatWidgetDrawer from '@/components/chat/ChatWidgetDrawer'
import ChatWidgetOverlay from '@/components/chat/ChatWidgetOverlay'
import { useChatContext } from '@/contexts/useChatContext'

/** Floating chat entry point mounted in the customer layout. Hidden on the full chat page. */
export default function ChatWidget() {
  const location = useLocation()
  const { enabled, isOpen, open, close, totalUnread } = useChatContext()

  if (!enabled) return null
  if (location.pathname.startsWith('/messages')) return null

  if (!isOpen) {
    return (
      <div className="fixed bottom-4 right-4 z-[70]">
        <ChatLauncher unreadCount={totalUnread} onClick={open} />
      </div>
    )
  }

  return (
    <>
      <ChatWidgetOverlay onClick={close} />
      <ChatWidgetDrawer onClose={close} />
    </>
  )
}
