interface ClearFiltersLinkProps {
  onClear: () => void
}

export default function ClearFiltersLink({ onClear }: ClearFiltersLinkProps) {
  return (
    <button
      type="button"
      onClick={onClear}
      className="inline-flex items-center gap-1 text-sm font-medium text-secondary transition hover:underline"
    >
      <span className="material-symbols-outlined text-[16px]">close</span>
      Clear all filters
    </button>
  )
}
