import { useState } from 'react'

interface StorePriceFilterProps {
  minPrice?: number
  maxPrice?: number
  onApply: (min: number | null, max: number | null) => void
}

function parseInput(value: string): number | null {
  if (!value.trim()) return null
  const parsed = Number.parseInt(value, 10)
  return Number.isFinite(parsed) && parsed > 0 ? parsed : null
}

export default function StorePriceFilter({ minPrice, maxPrice, onApply }: StorePriceFilterProps) {
  const [min, setMin] = useState(minPrice ? String(minPrice) : '')
  const [max, setMax] = useState(maxPrice ? String(maxPrice) : '')
  const [syncedRange, setSyncedRange] = useState(`${minPrice ?? ''}-${maxPrice ?? ''}`)

  // Sync inputs when the applied range changes externally (e.g. clear filters).
  const range = `${minPrice ?? ''}-${maxPrice ?? ''}`
  if (range !== syncedRange) {
    setSyncedRange(range)
    setMin(minPrice ? String(minPrice) : '')
    setMax(maxPrice ? String(maxPrice) : '')
  }

  const handleApply = () => {
    const parsedMin = parseInput(min)
    const parsedMax = parseInput(max)
    // Ignore inverted ranges; only apply valid positive bounds.
    if (parsedMin !== null && parsedMax !== null && parsedMin > parsedMax) return
    onApply(parsedMin, parsedMax)
  }

  return (
    <section>
      <h2 className="mb-2 text-sm font-bold uppercase tracking-wide text-gray-700">Price Range</h2>
      <div className="flex items-center gap-2">
        <input
          type="number"
          min={0}
          inputMode="numeric"
          value={min}
          onChange={(event) => setMin(event.target.value)}
          placeholder="Min"
          aria-label="Minimum price"
          className="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm focus:border-primary focus:outline-none"
        />
        <span className="text-gray-400">–</span>
        <input
          type="number"
          min={0}
          inputMode="numeric"
          value={max}
          onChange={(event) => setMax(event.target.value)}
          placeholder="Max"
          aria-label="Maximum price"
          className="w-full rounded-md border border-gray-300 px-2 py-1.5 text-sm focus:border-primary focus:outline-none"
        />
      </div>
      <button
        type="button"
        onClick={handleApply}
        className="mt-2 w-full rounded-md bg-secondary px-3 py-1.5 text-sm font-semibold text-white transition hover:opacity-90"
      >
        Apply
      </button>
    </section>
  )
}
