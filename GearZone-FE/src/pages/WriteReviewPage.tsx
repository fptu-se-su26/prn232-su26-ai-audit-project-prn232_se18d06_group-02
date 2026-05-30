import { useEffect, useState } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { reviewsApi } from '@/api/reviews'

interface ReviewEditor {
  orderItemId: string
  productName: string
  productImageUrl?: string
  existingRating?: number
  existingComment?: string
}

export default function WriteReviewPage() {
  const { orderItemId } = useParams<{ orderItemId: string }>()
  const navigate = useNavigate()
  const [editor, setEditor] = useState<ReviewEditor | null>(null)
  const [loading, setLoading] = useState(true)
  const [rating, setRating] = useState(5)
  const [hovered, setHovered] = useState(0)
  const [comment, setComment] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    if (!orderItemId) return
    reviewsApi.getEditor(orderItemId)
      .then(d => {
        const data = d as ReviewEditor
        setEditor(data)
        if (data.existingRating) setRating(data.existingRating)
        if (data.existingComment) setComment(data.existingComment)
      })
      .finally(() => setLoading(false))
  }, [orderItemId])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!orderItemId) return
    setSubmitting(true); setError('')
    try {
      await reviewsApi.submit({ orderItemId, rating, comment })
      navigate('/profile?tab=orders')
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to submit review.')
    } finally {
      setSubmitting(false)
    }
  }

  const LABELS = ['', 'Poor', 'Fair', 'Good', 'Very Good', 'Excellent']

  if (loading) return (
    <div className="flex items-center justify-center py-24">
      <div className="animate-spin w-8 h-8 border-4 border-primary border-t-transparent rounded-full" />
    </div>
  )
  if (!editor) return (
    <div className="text-center py-24 text-gray-500">
      <span className="material-symbols-outlined text-6xl text-gray-200 mb-4 block">rate_review</span>
      <p className="font-medium">Order item not found.</p>
    </div>
  )

  return (
    <div className="max-w-xl mx-auto px-4 sm:px-6 py-10">
      {/* Breadcrumb */}
      <div className="flex items-center gap-2 text-sm text-gray-500 mb-6">
        <Link to="/profile?tab=orders" className="hover:text-primary transition-colors">My Orders</Link>
        <span className="material-symbols-outlined text-[16px]">chevron_right</span>
        <span className="text-gray-900 font-medium">Write a Review</span>
      </div>

      <div className="bg-white rounded-2xl border border-gray-200 overflow-hidden">
        {/* Header */}
        <div className="px-6 py-5 border-b border-gray-100">
          <h1 className="text-xl font-bold text-gray-900">Write a Review</h1>
          <p className="text-sm text-gray-500 mt-0.5">Share your experience to help other buyers</p>
        </div>

        {/* Product info */}
        <div className="flex items-center gap-4 px-6 py-4 bg-gray-50 border-b border-gray-100">
          {editor.productImageUrl ? (
            <div className="w-14 h-14 rounded-xl bg-white border border-gray-200 overflow-hidden flex-shrink-0 p-1">
              <img src={editor.productImageUrl} alt={editor.productName} className="w-full h-full object-contain mix-blend-multiply" />
            </div>
          ) : (
            <div className="w-14 h-14 rounded-xl bg-gray-200 flex items-center justify-center flex-shrink-0">
              <span className="material-symbols-outlined text-gray-400">inventory_2</span>
            </div>
          )}
          <div>
            <p className="text-xs text-gray-400 mb-0.5">Reviewing</p>
            <p className="font-semibold text-gray-900 text-sm leading-snug">{editor.productName}</p>
          </div>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit} className="px-6 py-6 space-y-6">
          {/* Star rating */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-3">Your Rating</label>
            <div className="flex items-center gap-1">
              {[1, 2, 3, 4, 5].map(n => (
                <button
                  key={n}
                  type="button"
                  onClick={() => setRating(n)}
                  onMouseEnter={() => setHovered(n)}
                  onMouseLeave={() => setHovered(0)}
                  className="text-4xl leading-none transition-transform hover:scale-110 focus:outline-none"
                >
                  <span className={n <= (hovered || rating) ? 'text-amber-400' : 'text-gray-200'}>★</span>
                </button>
              ))}
              {(hovered || rating) > 0 && (
                <span className="ml-3 text-sm font-semibold text-amber-500">{LABELS[hovered || rating]}</span>
              )}
            </div>
          </div>

          {/* Comment */}
          <div>
            <label className="block text-sm font-semibold text-gray-700 mb-2">
              Your Review
              <span className="ml-1 text-gray-400 font-normal">(optional)</span>
            </label>
            <textarea
              value={comment}
              onChange={e => setComment(e.target.value)}
              placeholder="Share your experience with this product — quality, packaging, delivery…"
              rows={5}
              className="w-full border border-gray-300 rounded-xl px-4 py-3 text-sm text-gray-700 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-primary/20 focus:border-primary transition-all resize-none"
            />
            <p className="text-xs text-gray-400 mt-1 text-right">{comment.length} characters</p>
          </div>

          {error && (
            <div className="flex items-center gap-2 bg-red-50 text-red-600 px-4 py-3 rounded-xl text-sm">
              <span className="material-symbols-outlined text-[18px]">error</span>
              {error}
            </div>
          )}

          <div className="flex gap-3 pt-2">
            <button
              type="submit"
              disabled={submitting}
              className="flex-1 flex items-center justify-center gap-2 bg-primary hover:bg-blue-700 disabled:bg-gray-300 text-white font-bold py-3.5 rounded-xl transition-all shadow-[0_8px_20px_-6px_rgba(26,87,219,0.4)] disabled:shadow-none text-sm"
            >
              <span className="material-symbols-outlined text-[18px]">rate_review</span>
              {submitting ? 'Submitting…' : 'Submit Review'}
            </button>
            <Link
              to="/profile?tab=orders"
              className="flex items-center justify-center px-5 py-3.5 bg-gray-100 hover:bg-gray-200 text-gray-700 font-semibold rounded-xl transition-colors text-sm"
            >
              Cancel
            </Link>
          </div>
        </form>
      </div>
    </div>
  )
}
