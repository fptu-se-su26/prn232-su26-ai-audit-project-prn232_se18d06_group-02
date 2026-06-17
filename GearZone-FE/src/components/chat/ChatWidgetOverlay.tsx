interface ChatWidgetOverlayProps {
  onClick: () => void
}

export default function ChatWidgetOverlay({ onClick }: ChatWidgetOverlayProps) {
  return <div onClick={onClick} className="fixed inset-0 z-[69] bg-black/30 lg:hidden" aria-hidden="true" />
}
