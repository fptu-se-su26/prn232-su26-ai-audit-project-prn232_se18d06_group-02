export function AuthSectionDivider({ label }: { label: string }) {
  return (
    <div className="relative mt-2 py-4">
      <div className="absolute inset-0 flex items-center">
        <div className="w-full border-t border-slate-100" />
      </div>
      <div className="relative flex justify-center">
        <span className="bg-white px-4 text-[10px] font-bold uppercase tracking-widest text-slate-400">{label}</span>
      </div>
    </div>
  )
}
