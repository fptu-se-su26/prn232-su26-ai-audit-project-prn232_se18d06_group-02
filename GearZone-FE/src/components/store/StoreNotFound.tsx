import { Link } from 'react-router-dom'

export default function StoreNotFound() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center px-6 text-center">
      <div className="flex h-16 w-16 items-center justify-center rounded-full bg-gray-100 text-gray-400">
        <span className="material-symbols-outlined text-[32px]">storefront</span>
      </div>
      <h1 className="mt-4 text-xl font-bold text-gray-800">Store Not Found</h1>
      <p className="mt-2 max-w-sm text-sm text-gray-500">
        We couldn't find the store you're looking for. It may have been removed or the link is incorrect.
      </p>
      <Link
        to="/products"
        className="mt-5 rounded-lg bg-secondary px-4 py-2 text-sm font-semibold text-white transition hover:opacity-90"
      >
        Browse all products
      </Link>
    </div>
  )
}
