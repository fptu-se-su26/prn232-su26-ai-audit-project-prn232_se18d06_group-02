import type { ChatProductContext } from '@/types/chat'

interface ProductContextCardProps {
  product: ChatProductContext
}

export default function ProductContextCard({ product }: ProductContextCardProps) {
  return (
    <div className="flex items-center gap-3 border-b border-gray-100 bg-white px-4 py-2">
      <img
        src={product.productImageUrl ?? '/images/placeholder.png'}
        alt={product.productName}
        className="h-10 w-10 rounded object-cover"
        loading="lazy"
      />
      <div className="min-w-0 flex-1">
        <p className="truncate text-[13px] font-medium text-gray-800">{product.productName}</p>
        <p className="text-[12px] text-secondary">{product.price.toLocaleString('vi-VN')} ₫</p>
      </div>
      {!product.isInStock && <span className="text-[11px] font-medium text-red-500">Out of stock</span>}
    </div>
  )
}
