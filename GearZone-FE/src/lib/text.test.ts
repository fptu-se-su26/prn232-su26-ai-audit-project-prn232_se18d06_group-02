import { describe, expect, it } from 'vitest'
import { getInitials } from '@/lib/text'

describe('getInitials', () => {
  it('returns two initials for a full name', () => {
    expect(getInitials('Nguyen Nhat')).toBe('NN')
  })

  it('uses the first two letters for a single-word name', () => {
    expect(getInitials('Gearzone')).toBe('GE')
  })

  it('uses the first and last word for multi-word names', () => {
    expect(getInitials('Tran Van An')).toBe('TA')
  })

  it('falls back to a placeholder for an empty name', () => {
    expect(getInitials('   ')).toBe('?')
  })
})
