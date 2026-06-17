export function readSession<T>(key: string, fallback: T): T {
  try {
    const raw = sessionStorage.getItem(key)
    if (raw === null) return fallback
    return JSON.parse(raw) as T
  } catch {
    return fallback
  }
}

export function writeSession<T>(key: string, value: T): void {
  try {
    sessionStorage.setItem(key, JSON.stringify(value))
  } catch {
    /* storage unavailable or quota exceeded — ignore */
  }
}

export function removeSession(key: string): void {
  try {
    sessionStorage.removeItem(key)
  } catch {
    /* storage unavailable — ignore */
  }
}
