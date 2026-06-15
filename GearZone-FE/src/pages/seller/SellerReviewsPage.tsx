import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { sellerApi } from '@/api/seller'
import { SellerLayout } from '@/components/seller/SellerLayout'

interface SellerReview {
  id: string
  productId: string
  productName: string
  productSlug: string
  productImageUrl?: string | null
  variantName: string
  buyerDisplayName: string
  rating: number
  comment?: string | null
  createdAt: string
  sellerReplyContent?: string | null
  sellerReplyAt?: string | null
}

interface PagedReviews {
  items?: SellerReview[]
  totalCount?: number
  pageNumber?: number
  pageSize?: number
  totalPages?: number
}

const FILTERS = [
  { label: 'All', value: 'all' },
  { label: 'Awaiting Reply', value: 'unreplied' },
  { label: 'Replied', value: 'replied' },
]

function formatDate(value?: string | null) {
  if (!value) return ''
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) return value

  return new Intl.DateTimeFormat('en-US', {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  }).format(date)
}

function getInitials(name: string) {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('')
}

function pageNumbers(current: number, total: number) {
  if (total <= 7) return Array.from({ length: total }, (_, index) => index + 1)

  const pages = new Set<number>([1, total])
  for (let page = current - 1; page <= current + 1; page += 1) {
    if (page > 1 && page < total) pages.add(page)
  }

  return Array.from(pages).sort((a, b) => a - b)
}

function ReviewStars({ rating }: { rating: number }) {
  return (
    <span
      className="review-stars"
      aria-label={`${rating} out of 5 stars`}
      title={`${rating}/5`}
    >
      {Array.from({ length: 5 }).map((_, index) => {
        const filled = index < rating
        return (
          <span
            key={index}
            className={`material-symbols-outlined filled review-star ${
              filled ? 'review-star--filled' : 'review-star--empty'
            }`}
          >
            star
          </span>
        )
      })}
    </span>
  )
}

export default function SellerReviewsPage() {
  const [reviews, setReviews] = useState<SellerReview[]>([])
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [filter, setFilter] = useState('all')
  const [replyDrafts, setReplyDrafts] = useState<Record<string, string>>({})
  const [savingId, setSavingId] = useState<string | null>(null)
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const visibleReviews = useMemo(() => {
    if (filter === 'unreplied') {
      return reviews.filter((review) => !review.sellerReplyContent?.trim())
    }

    if (filter === 'replied') {
      return reviews.filter((review) => Boolean(review.sellerReplyContent?.trim()))
    }

    return reviews
  }, [filter, reviews])

  const pages = useMemo(() => pageNumbers(page, totalPages), [page, totalPages])

  const loadReviews = () => {
    setLoading(true)
    setError(null)

    sellerApi
      .storeReviews(page)
      .then((result) => {
        const data = result as PagedReviews
        const items = data.items ?? []
        setReviews(items)
        setTotalCount(data.totalCount ?? items.length)
        setTotalPages(Math.max(1, data.totalPages ?? 1))
        setReplyDrafts(
          items.reduce<Record<string, string>>((drafts, review) => {
            drafts[review.id] = review.sellerReplyContent ?? ''
            return drafts
          }, {}),
        )
      })
      .catch((err: unknown) => {
        setError(err instanceof Error ? err.message : 'Failed to load reviews.')
      })
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    loadReviews()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page])

  const submitReply = async (event: FormEvent, review: SellerReview) => {
    event.preventDefault()
    const content = (replyDrafts[review.id] ?? '').trim()
    if (!content) {
      setMessage({ type: 'error', text: 'Reply content cannot be empty.' })
      return
    }

    setSavingId(review.id)
    setMessage(null)
    try {
      await sellerApi.replyToReview(review.id, content)
      setMessage({ type: 'success', text: 'Your reply has been saved successfully.' })
      loadReviews()
    } catch (err: unknown) {
      setMessage({
        type: 'error',
        text: err instanceof Error ? err.message : 'Failed to save your reply.',
      })
    } finally {
      setSavingId(null)
    }
  }

  return (
    <SellerLayout pageHeader="Customer Reviews" breadcrumb={['Dashboard', 'Reviews']}>
      <style>
        {`
          .review-stars {
            display: inline-flex;
            align-items: center;
            gap: 0.18rem;
            line-height: 1;
          }
          .review-star {
            width: 16px;
            height: 16px;
            display: block;
            flex: none;
            font-size: 16px;
          }
          .review-star--filled {
            color: #ee4d2d;
          }
          .review-star--empty {
            color: #d8dde6;
          }
          .review-surface-card {
            background: #fff;
            border: 1px solid #e5e7eb;
            box-shadow: 0 1px 3px 0 rgb(15 23 42 / 0.08), 0 1px 2px -1px rgb(15 23 42 / 0.08);
          }
          .review-chip {
            display: inline-flex;
            min-height: 2.05rem;
            align-items: center;
            justify-content: center;
            gap: 0.4rem;
            border: 1px solid #ededed;
            border-radius: 0.125rem;
            background: #fff;
            color: #555;
            padding: 0.45rem 1rem;
            font-size: 0.875rem;
            font-weight: 500;
            transition: border-color 160ms ease, color 160ms ease, background-color 160ms ease;
          }
          .review-chip:hover {
            border-color: #ee4d2d;
            color: #ee4d2d;
          }
          .review-chip.is-active {
            border-color: #ee4d2d;
            background: #fff6f3;
            color: #ee4d2d;
          }
          .review-summary-shell {
            border: 1px solid #eef2f6;
            background: #f7f9fc;
          }
          .review-reply-card {
            border-left: 0.1875rem solid #f4a58d;
            background: #fafafa;
          }
          .review-textarea {
            border: 1px solid #d9d9d9;
            background: #fff;
            transition: border-color 160ms ease, box-shadow 160ms ease;
          }
          .review-textarea:focus {
            border-color: #ee4d2d;
            box-shadow: 0 0 0 1px rgba(238, 77, 45, 0.1);
            outline: 0;
          }
          .review-primary-button {
            display: inline-flex;
            min-height: 2.8rem;
            align-items: center;
            justify-content: center;
            gap: 0.45rem;
            border: 1px solid #ee4d2d;
            border-radius: 0.25rem;
            background: #ee4d2d;
            color: #fff !important;
            padding: 0.7rem 1.35rem;
            font-size: 0.95rem;
            font-weight: 600;
            transition: border-color 160ms ease, background-color 160ms ease, color 160ms ease;
          }
          .review-primary-button * {
            color: #fff !important;
          }
          .review-primary-button:hover {
            border-color: #d73211;
            background: #d73211;
          }
          .review-page-link {
            display: inline-flex;
            min-width: 2rem;
            min-height: 2rem;
            align-items: center;
            justify-content: center;
            border: 1px solid #e8e8e8;
            border-radius: 0.125rem;
            background: #fff;
            color: #555;
            font-size: 0.875rem;
            font-weight: 500;
          }
          .review-page-link:hover {
            border-color: #ee4d2d;
            color: #ee4d2d;
          }
          .review-page-link.is-active {
            border-color: #ee4d2d;
            background: #ee4d2d;
            color: #fff;
          }
        `}
      </style>

      <div className="space-y-6">
        <section className="review-surface-card rounded-[26px] p-5">
          <div className="flex flex-col justify-between gap-4 md:flex-row md:items-center">
            <div>
              <p className="text-sm font-semibold uppercase tracking-[0.22em] text-primary">
                Shopee-style Reply Desk
              </p>
              <h1 className="mt-2 text-2xl font-black text-slate-900">
                Manage customer reviews
              </h1>
              <p className="mt-2 text-slate-500">
                Reply to buyer feedback and keep your product reputation healthy.
              </p>
            </div>

            <div className="flex flex-wrap items-center gap-2">
              {FILTERS.map((item) => (
                <button
                  key={item.value}
                  type="button"
                  onClick={() => setFilter(item.value)}
                  className={`review-chip ${filter === item.value ? 'is-active' : ''}`}
                >
                  {item.label}
                </button>
              ))}
            </div>
          </div>
        </section>

        {message && (
          <div
            className={`rounded-xl border px-4 py-3 text-sm font-medium ${
              message.type === 'success'
                ? 'border-emerald-200 bg-emerald-50 text-emerald-700'
                : 'border-red-200 bg-red-50 text-red-700'
            }`}
          >
            {message.text}
          </div>
        )}

        {loading ? (
          <div className="space-y-4">
            {Array.from({ length: 3 }).map((_, index) => (
              <div
                key={index}
                className="h-72 animate-pulse rounded-[26px] border border-slate-200 bg-white"
              />
            ))}
          </div>
        ) : error ? (
          <div className="rounded-3xl border border-red-200 bg-red-50 px-6 py-16 text-center text-red-600">
            {error}
          </div>
        ) : visibleReviews.length > 0 ? (
          <>
            <div className="space-y-4">
              {visibleReviews.map((review) => {
                const hasReply = Boolean(review.sellerReplyContent?.trim())

                return (
                  <article key={review.id} className="review-surface-card rounded-[26px] p-5">
                    <div className="flex flex-col gap-5 xl:flex-row">
                      <div className="flex gap-4 xl:w-72">
                        <div className="h-20 w-20 shrink-0 overflow-hidden rounded-3xl border border-slate-100 bg-slate-50">
                          {review.productImageUrl ? (
                            <img
                              src={review.productImageUrl}
                              alt={review.productName}
                              className="h-full w-full object-contain p-2"
                            />
                          ) : (
                            <div className="flex h-full w-full items-center justify-center text-slate-300">
                              <span className="material-symbols-outlined text-4xl">inventory_2</span>
                            </div>
                          )}
                        </div>
                        <div className="min-w-0">
                          <Link
                            to={`/product/${review.productSlug}`}
                            className="line-clamp-2 font-bold text-slate-900 hover:text-primary"
                          >
                            {review.productName}
                          </Link>
                          <p className="mt-1 text-sm text-slate-500">
                            Variant: {review.variantName || 'Default'}
                          </p>
                          <div className="mt-3 flex items-center gap-2">
                            <ReviewStars rating={review.rating} />
                            <span className="ml-1 text-sm font-semibold text-slate-700">
                              {review.rating}/5
                            </span>
                          </div>
                        </div>
                      </div>

                      <div className="flex-1 space-y-4">
                        <div className="review-summary-shell rounded-[22px] p-4">
                          <div className="flex items-center justify-between gap-3">
                            <div className="flex items-center gap-3">
                              <div className="flex size-9 shrink-0 items-center justify-center rounded-full border border-slate-200 bg-white text-xs font-black text-slate-500">
                                {getInitials(review.buyerDisplayName) || '?'}
                              </div>
                              <div>
                                <p className="text-sm font-bold text-slate-900">
                                  {review.buyerDisplayName || 'Buyer'}
                                </p>
                                <p className="text-xs text-slate-400">
                                  {formatDate(review.createdAt)}
                                </p>
                              </div>
                            </div>

                            {hasReply && (
                              <span className="inline-flex items-center rounded-full border border-emerald-200 bg-emerald-50 px-3 py-1 text-xs font-bold text-emerald-600">
                                Replied
                              </span>
                            )}
                          </div>

                          {review.comment?.trim() ? (
                            <p className="mt-3 text-sm leading-6 text-slate-600">
                              {review.comment}
                            </p>
                          ) : (
                            <p className="mt-3 text-sm italic text-slate-400">
                              Buyer gave a star rating without a written comment.
                            </p>
                          )}
                        </div>

                        {hasReply && (
                          <div className="review-reply-card rounded-[22px] p-4">
                            <div className="flex items-center gap-2 text-orange-700">
                              <span className="material-symbols-outlined text-[18px]">campaign</span>
                              <p className="text-sm font-bold">Your reply</p>
                            </div>
                            <p className="mt-2 text-sm leading-6 text-slate-700">
                              {review.sellerReplyContent}
                            </p>
                            {review.sellerReplyAt && (
                              <p className="mt-2 text-xs text-slate-400">
                                Saved {formatDate(review.sellerReplyAt)}
                              </p>
                            )}
                          </div>
                        )}

                        <form className="space-y-3" onSubmit={(event) => void submitReply(event, review)}>
                          <label className="text-sm font-semibold text-slate-700">
                            {hasReply ? 'Edit your reply' : 'Write a reply'}
                          </label>
                          <textarea
                            rows={4}
                            maxLength={2000}
                            value={replyDrafts[review.id] ?? ''}
                            onChange={(event) =>
                              setReplyDrafts((current) => ({
                                ...current,
                                [review.id]: event.target.value,
                              }))
                            }
                            className="review-textarea block w-full rounded-[22px] px-4 py-3 text-sm text-slate-900 placeholder:text-slate-400"
                            placeholder="Thank the buyer, address any concern, and keep the tone professional."
                          />
                          <button
                            type="submit"
                            disabled={savingId === review.id}
                            className="review-primary-button disabled:cursor-not-allowed disabled:opacity-70"
                          >
                            <span className="material-symbols-outlined text-[18px]">reply</span>
                            {savingId === review.id
                              ? 'Saving...'
                              : hasReply
                                ? 'Update Reply'
                                : 'Send Reply'}
                          </button>
                        </form>
                      </div>
                    </div>
                  </article>
                )
              })}
            </div>

            {totalPages > 1 && (
              <div className="flex items-center justify-center gap-2">
                {pages.map((pageNumber, index) => {
                  const previous = pages[index - 1]
                  const showEllipsis = previous && pageNumber - previous > 1

                  return (
                    <span key={pageNumber} className="flex items-center gap-2">
                      {showEllipsis && (
                        <span className="px-1 text-sm font-semibold text-slate-400">...</span>
                      )}
                      <button
                        type="button"
                        onClick={() => setPage(pageNumber)}
                        className={`review-page-link ${pageNumber === page ? 'is-active' : ''}`}
                      >
                        {pageNumber}
                      </button>
                    </span>
                  )
                })}
              </div>
            )}
          </>
        ) : (
          <div className="rounded-3xl border border-slate-200 bg-white px-6 py-20 text-center shadow-[0_1px_3px_0_rgb(15_23_42_/_0.08),0_1px_2px_-1px_rgb(15_23_42_/_0.08)]">
            <div className="mx-auto mb-6 flex h-20 w-20 items-center justify-center rounded-full bg-slate-50 text-slate-300">
              <span className="material-symbols-outlined text-5xl">forum</span>
            </div>
            <h2 className="text-2xl font-black text-slate-900">No reviews in this view</h2>
            <p className="mx-auto mt-3 max-w-lg text-slate-500">
              Once buyers start reviewing your delivered orders, they will appear here so you can
              respond directly from Seller Center.
            </p>
            {filter !== 'all' && totalCount > 0 && (
              <button
                type="button"
                onClick={() => setFilter('all')}
                className="mt-5 text-sm font-bold text-primary hover:underline"
              >
                Show all reviews
              </button>
            )}
          </div>
        )}
      </div>
    </SellerLayout>
  )
}
