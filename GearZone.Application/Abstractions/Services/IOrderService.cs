using GearZone.Application.Common.Models;
using GearZone.Application.Features.Orders.Dtos;
using System;
using GearZone.Domain.Enums;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IOrderService
    {
        Task<PagedResult<UserOrderDto>> GetUserOrdersAsync(string userId, UserOrderQueryDto query);
        Task<UserOrderStatusSummaryDto> GetUserOrderStatusSummaryAsync(string userId);

        Task<Order> CreateOrderAsync(
            string userId,
            CheckoutRequestDto request,
            List<CartItem> cartItems,
            Guid? orderVoucherId = null,
            decimal orderDiscountAmount = 0,
            Guid? shippingVoucherId = null,
            decimal shippingDiscountAmount = 0,
            decimal totalShippingFee = 0,
            List<GearZone.Application.Features.Shipping.Dtos.StoreShippingFeeDto>? storeShippingFees = null,
            CancellationToken ct = default);

        Task<bool> CancelOrderAsync(Guid orderId, string? userId = null, CancellationToken ct = default);
        Task<Order?> GetOrderByIdAsync(Guid orderId, CancellationToken ct = default);
        Task<Order?> GetOrderByOrderCodeAsync(long orderCode, CancellationToken ct = default);
        Task<List<Order>> GetOrdersByStatusAndTimeoutAsync(OrderStatus status, DateTime cutoffTime, CancellationToken ct = default);
        Task<UserOrderTrackingDto?> GetUserOrderTrackingAsync(string userId, Guid subOrderId, CancellationToken ct = default);
    }
}
