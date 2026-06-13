import { useEffect, useRef } from 'react'

/**
 * Keeps a scroll container pinned to the bottom whenever `trigger` changes
 * (e.g. the id of the latest message). Loading older messages keeps the same
 * trigger, so the view does not jump.
 */
export function useAutoScroll<T extends HTMLElement>(trigger: string | number) {
  const ref = useRef<T | null>(null)
  useEffect(() => {
    const element = ref.current
    if (element) element.scrollTop = element.scrollHeight
  }, [trigger])
  return ref
}
