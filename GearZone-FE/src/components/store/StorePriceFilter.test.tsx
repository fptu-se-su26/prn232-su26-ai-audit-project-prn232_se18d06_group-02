import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import StorePriceFilter from '@/components/store/StorePriceFilter'

describe('StorePriceFilter', () => {
  it('applies valid positive bounds', () => {
    const onApply = vi.fn()
    render(<StorePriceFilter onApply={onApply} />)

    fireEvent.change(screen.getByLabelText('Minimum price'), { target: { value: '100' } })
    fireEvent.change(screen.getByLabelText('Maximum price'), { target: { value: '500' } })
    fireEvent.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(100, 500)
  })

  it('ignores inverted ranges (min greater than max)', () => {
    const onApply = vi.fn()
    render(<StorePriceFilter onApply={onApply} />)

    fireEvent.change(screen.getByLabelText('Minimum price'), { target: { value: '900' } })
    fireEvent.change(screen.getByLabelText('Maximum price'), { target: { value: '100' } })
    fireEvent.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).not.toHaveBeenCalled()
  })

  it('treats negative or zero values as cleared bounds', () => {
    const onApply = vi.fn()
    render(<StorePriceFilter onApply={onApply} />)

    fireEvent.change(screen.getByLabelText('Minimum price'), { target: { value: '-5' } })
    fireEvent.click(screen.getByRole('button', { name: 'Apply' }))

    expect(onApply).toHaveBeenCalledWith(null, null)
  })
})
