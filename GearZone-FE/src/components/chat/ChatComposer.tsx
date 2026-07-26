import { type KeyboardEvent as ReactKeyboardEvent } from 'react'
import { CHAT_MESSAGE_MAX_LENGTH } from '@/types/chat'

interface ChatComposerProps {
  value: string
  onChange: (value: string) => void
  onSend: () => void
  disabled?: boolean
  sending?: boolean
}

export default function ChatComposer({ value, onChange, onSend, disabled, sending }: ChatComposerProps) {
  const trimmed = value.trim()
  const tooLong = value.length > CHAT_MESSAGE_MAX_LENGTH
  const canSend = trimmed.length > 0 && !tooLong && !disabled && !sending

  function handleKeyDown(event: ReactKeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault()
      if (canSend) onSend()
    }
  }

  return (
    <div className="border-t border-gray-100 bg-white p-3">
      <div className="flex items-end gap-2">
        <textarea
          value={value}
          onChange={(event) => onChange(event.target.value)}
          onKeyDown={handleKeyDown}
          rows={1}
          placeholder="Type a message"
          disabled={disabled}
          className="max-h-32 min-h-[2.5rem] flex-1 resize-none rounded-lg border border-gray-200 px-3 py-2 text-sm outline-none focus:border-secondary disabled:bg-gray-50"
        />
        <button
          type="button"
          onClick={onSend}
          disabled={!canSend}
          className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full bg-secondary text-white transition hover:opacity-90 disabled:opacity-40"
          aria-label="Send message"
        >
          <span className="material-symbols-outlined text-[20px]">send</span>
        </button>
      </div>
      {tooLong && (
        <p className="mt-1 text-[11px] text-red-500">
          Message is too long (max {CHAT_MESSAGE_MAX_LENGTH} characters).
        </p>
      )}
    </div>
  )
}
