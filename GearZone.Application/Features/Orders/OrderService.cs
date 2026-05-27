using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Application.Features.Orders.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IOrderTrackingNotifier _orderTrackingNotifier;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            IOrderRepository orderRepository,
            ISubOrderRepository subOrderRepository,
            IPaymentRepository paymentRepository,
            IProductVariantRepository productVariantRepository,
            IOrderTrackingNotifier orderTrackingNotifier,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _subOrderRepository = subOrderRepository;
            _paymentRepository = paymentRepository;
            _productVariantRepository = productVariantRepository;
            _orderTrackingNotifier = orderTrackingNotifier;
            _unitOfWork = unitOfWork;
        }

        public async Task<Order> CreateOrderAsync(
            string userId,
            CheckoutRequestDto request,
            List<CartItem> cartItems,
            Guid? orderVoucherId = null,
            decimal orderDiscountAmount = 0,
            Guid? shippingVoucherId = null,
            decimal shippingDiscountAmount = 0,
            decimal totalShippingFee = 0,
            List<GearZone.Application.Features.Shipping.Dtos.StoreShippingFeeDto>? storeShippingFees = null,
            CancellationToken ct = default)
        {
            var addressParts = new List<string?>
            {
                request.ShippingInfo.Address,
                request.ShippingInfo.Ward,
                request.ShippingInfo.District,
                request.ShippingInfo.Province
            };
            var shippingAddressStr = string.Join(", ", addressParts.Where(s => !string.IsNullOrWhiteSpace(s)));

            var storeGroups = cartItems.GroupBy(ci => ci.Variant.Product.StoreId).ToList();

            long orderCode = long.Parse(
                DateTime.UtcNow.ToString("yyMMddHHmmss") + new Random().Next(10, 99).ToString());

            decimal grandTotal = 0m;

            // Determine initial order status based on payment method
            var initialStatus = request.PaymentMethod == PaymentMethod.PayOS
                ? OrderStatus.AwaitingPayment
                : OrderStatus.Pending;

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderCode = orderCode,
                UserId = userId,
                ShippingFee = totalShippingFee,
                ReceiverName = request.ShippingInfo.FullName,
                ReceiverPhone = request.ShippingInfo.PhoneNumber,
                ShippingAddress = shippingAddressStr,
                CreatedAt = DateTime.UtcNow,
                StatusHistories = new List<OrderStatusHistory>
                {
                    new OrderStatusHistory
                    {
                        NewStatus = initialStatus,
                        ChangedAt = DateTime.UtcNow,
                        ChangedByUserId = userId
                    }
                }
            };

            foreach (var group in storeGroups)
            {
                var storeId = group.Key;
                decimal subtotal = group.Sum(ci => ci.Quantity * ci.Variant.Price);
                decimal commissionRate = 0.05m;
                decimal commissionAmount = subtotal * commissionRate;
                decimal netAmount = subtotal - commissionAmount;

                var subOrder = new SubOrder
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    StoreId = storeId,
                    Status = initialStatus,
                    PayoutStatus = PayoutStatus.Unpaid,
                    Subtotal = subtotal,
                    CommissionRateSnapshot = commissionRate,
                    CommissionAmount = commissionAmount,
                    NetAmount = netAmount,
                    CreatedAt = DateTime.UtcNow,
                    Items = group.Select(ci => new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        VariantId = ci.VariantId,
                        ProductNameSnapshot = ci.Variant.Product.Name,
                        VariantNameSnapshot = ci.Variant.AttributeValues.Any()
                            ? string.Join(", ", ci.Variant.AttributeValues
                                .Select(v => v.CategoryAttributeOption.Value))
                            : string.Empty,
                        SkuSnapshot = ci.Variant.Sku,
                        UnitPriceSnapshot = ci.Variant.Price,
                        Quantity = ci.Quantity,
                        LineTotal = ci.Quantity * ci.Variant.Price
                    }).ToList()
                };

                order.SubOrders.Add(subOrder);

                // Create Shipment for this store
                var storeShipping = storeShippingFees?.FirstOrDefault(sf => sf.StoreId == storeId);
                if (storeShipping != null)
                {
                    order.Shipments.Add(new Shipment
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        StoreId = storeId,
                        ShippingFee = storeShipping.ShippingFee,
                        DistanceKm = storeShipping.DistanceKm,
                        ShippingProvider = "Standard"
                    });
                }

                grandTotal += subtotal;
            }

            order.OrderVoucherId = orderVoucherId;
            order.OrderDiscountAmount = orderDiscountAmount;
            order.ShippingVoucherId = shippingVoucherId;
            order.ShippingDiscountAmount = shippingDiscountAmount;
            order.GrandTotal = grandTotal + totalShippingFee - orderDiscountAmount - shippingDiscountAmount;

            await _orderRepository.AddAsync(order, ct);

            return order;
        }

        public async Task<bool> CancelOrderAsync(Guid orderId, string? userId = null, CancellationToken ct = default)
        {
            var order = await _orderRepository.Query()
                .Include(o => o.SubOrders)
                    .ThenInclude(so => so.Items)
                        .ThenInclude(oi => oi.Variant)
                .Include(o => o.Payments)
                .Include(o => o.StatusHistories)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);

            if (order == null) return false;

            // Security check: if userId is provided, ensure order belongs to them
            if (userId != null && order.UserId != userId) return false;

            // Check if already cancelled or paid
            if (order.StatusHistories.Any(sh => sh.NewStatus == OrderStatus.Cancelled || sh.NewStatus == OrderStatus.Paid))
                return true; // Already processed

            // 1. Restore stock & update SubOrders
            foreach (var subOrder in order.SubOrders)
            {
                foreach (var item in subOrder.Items)
                {
                    if (item.Variant != null)
                    {
                        item.Variant.StockQuantity += item.Quantity;
                    }
                }
                subOrder.Status = OrderStatus.Cancelled;
                subOrder.UpdatedAt = DateTime.UtcNow;
            }

            // 2. Update Pending Payments
            foreach (var payment in order.Payments.Where(p => p.Status == PaymentStatus.Pending))
            {
                payment.Status = PaymentStatus.Cancelled;
                payment.UpdatedAt = DateTime.UtcNow;
            }

            // 3. Add Status History
            order.StatusHistories.Add(new OrderStatusHistory
            {
                NewStatus = OrderStatus.Cancelled,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = userId,
                Note = userId == null
                    ? "Order auto-cancelled by system (payment timeout)"
                    : "Order cancelled by user"
            });

            order.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);

            foreach (var subOrder in order.SubOrders)
            {
                await _orderTrackingNotifier.NotifySubOrderUpdatedAsync(subOrder.Id, ct);
            }

            return true;
        }

        public async Task<Order?> GetOrderByIdAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _orderRepository.Query()
                .Include(o => o.SubOrders)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }

        public async Task<Order?> GetOrderByOrderCodeAsync(long orderCode, CancellationToken ct = default)
        {
            return await _orderRepository.Query()
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode, ct);
        }

        public async Task<List<Order>> GetOrdersByStatusAndTimeoutAsync(OrderStatus status, DateTime cutoffTime, CancellationToken ct = default)
        {
            return await _orderRepository.Query()
                .Include(o => o.SubOrders)
                .Where(o => o.SubOrders.Any(so => so.Status == status) && o.CreatedAt <= cutoffTime)
                .ToListAsync(ct);
        }

        public async Task<PagedResult<UserOrderDto>> GetUserOrdersAsync(string userId, UserOrderQueryDto query)
        {
            query.PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            query.PageSize = query.PageSize < 1 ? 10 : query.PageSize;

            return await _subOrderRepository.GetUserOrdersAsync(userId, query, DateTime.UtcNow);
        }

        public async Task<UserOrderStatusSummaryDto> GetUserOrderStatusSummaryAsync(string userId)
        {
            return await _subOrderRepository.GetUserOrderStatusSummaryAsync(userId, DateTime.UtcNow);
        }

        public async Task<UserOrderTrackingDto?> GetUserOrderTrackingAsync(string userId, Guid subOrderId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId) || subOrderId == Guid.Empty)
            {
                return null;
            }

            var subOrder = await _subOrderRepository.Query()
                .AsNoTracking()
                .Include(x => x.Store)
                .Include(x => x.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p.Images)
                .Include(x => x.Order)
                    .ThenInclude(o => o.StatusHistories)
                        .ThenInclude(h => h.ChangedByUser)
                .FirstOrDefaultAsync(x => x.Id == subOrderId && x.Order.UserId == userId, ct);

            if (subOrder == null)
            {
                return null;
            }

            var history = subOrder.Order.StatusHistories
                .OrderBy(x => x.ChangedAt)
                .Select(x => new UserOrderTrackingStatusHistoryDto
                {
                    ChangedAt = x.ChangedAt,
                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    ChangedByDisplayName = x.ChangedByUser != null
                        ? (x.ChangedByUser.FullName ?? x.ChangedByUser.UserName ?? x.ChangedByUser.Email ?? "System")
                        : "System",
                    Note = x.Note
                })
                .ToList();

            return new UserOrderTrackingDto
            {
                SubOrderId = subOrder.Id,
                OrderId = subOrder.OrderId,
                OrderCode = subOrder.Order.OrderCode,
                StoreId = subOrder.StoreId,
                StoreName = subOrder.Store.StoreName,
                StoreSlug = subOrder.Store.Slug,
                Status = subOrder.Status,
                CreatedAt = subOrder.CreatedAt,
                UpdatedAt = subOrder.UpdatedAt,
                DeliveredAt = subOrder.DeliveredAt,
                Subtotal = subOrder.Subtotal,
                ShippingFee = subOrder.Order.ShippingFee,
                GrandTotal = subOrder.Order.GrandTotal,
                ReceiverName = subOrder.Order.ReceiverName,
                ReceiverPhone = subOrder.Order.ReceiverPhone,
                ShippingAddress = subOrder.Order.ShippingAddress,
                ShippingProvider = subOrder.Order.ShippingProvider,
                TrackingNumber = subOrder.Order.TrackingNumber,
                Items = subOrder.Items
                    .OrderBy(i => i.ProductNameSnapshot)
                    .ThenBy(i => i.SkuSnapshot)
                    .Select(i => new UserOrderTrackingItemDto
                    {
                        OrderItemId = i.Id,
                        ProductId = i.Variant.ProductId,
                        ProductName = i.ProductNameSnapshot,
                        ProductSlug = i.Variant.Product.Slug,
                        ProductImageUrl = i.Variant.Product.Images
                            .Where(img => img.IsPrimary)
                            .Select(img => img.ImageUrl)
                            .FirstOrDefault()
                            ?? i.Variant.Product.Images.Select(img => img.ImageUrl).FirstOrDefault(),
                        VariantName = i.VariantNameSnapshot,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPriceSnapshot,
                        LineTotal = i.LineTotal
                    })
                    .ToList(),
                StatusHistory = history
            };
        }
    }
}
