import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import StoreSortTabs from '@/components/store/StoreSortTabs'

describe('StoreSortTabs', () => {
  it('marks the active sort option and shows the product count', () => {
    render(<StoreSortTabs sortBy="newest" totalCount={1234} onChange={() => {}} />)

    expect(screen.getByRole('button', { name: 'Newest' })).toHaveAttribute('aria-pressed', 'true')
    expect(screen.getByRole('button', { name: 'Popular' })).toHaveAttribute('aria-pressed', 'false')
    expect(screen.getByText('1,234 products')).toBeInTheDocument()
  })

  it('emits the selected sort value on click', () => {
    const onChange = vi.fn()
    render(<StoreSortTabs sortBy="popular" totalCount={0} onChange={onChange} />)

    fireEvent.click(screen.getByRole('button', { name: 'Price: Low → High' }))
    expect(onChange).toHaveBeenCalledWith('price_asc')
  })
})
