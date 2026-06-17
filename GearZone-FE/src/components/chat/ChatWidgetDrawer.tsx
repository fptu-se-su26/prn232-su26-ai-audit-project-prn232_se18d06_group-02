import ChatInboxLayout from '@/components/chat/ChatInboxLayout'

interface ChatWidgetDrawerProps {
  onClose: () => void
}

export default function ChatWidgetDrawer({ onClose }: ChatWidgetDrawerProps) {
  return (
    <div className="fixed inset-0 z-[71] flex flex-col bg-white shadow-2xl lg:inset-auto lg:bottom-0 lg:right-[18px] lg:h-[min(540px,calc(100vh-132px))] lg:w-[min(800px,calc(100vw-32px))] lg:rounded-t-lg">
      <div className="flex items-center justify-between bg-secondary px-4 py-3 text-white lg:rounded-t-lg">
        <span className="text-sm font-semibold">Messages</span>
        <button type="button" onClick={onClose} aria-label="Close chat" className="text-white/90 transition hover:text-white">
          <span className="material-symbols-outlined text-[22px]">close</span>
        </button>
      </div>
      <div className="min-h-0 flex-1">
        <ChatInboxLayout />
      </div>
    </div>
  )
}
