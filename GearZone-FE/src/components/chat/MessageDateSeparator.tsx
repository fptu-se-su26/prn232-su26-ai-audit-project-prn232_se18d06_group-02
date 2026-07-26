interface MessageDateSeparatorProps {
  label: string
}

export default function MessageDateSeparator({ label }: MessageDateSeparatorProps) {
  return (
    <div className="my-3 flex justify-center">
      <span className="rounded-full bg-gray-100 px-3 py-1 text-[11px] font-medium text-gray-500">{label}</span>
    </div>
  )
}
