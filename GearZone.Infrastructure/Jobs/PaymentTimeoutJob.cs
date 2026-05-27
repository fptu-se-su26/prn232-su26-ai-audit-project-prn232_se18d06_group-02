using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Enums;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Jobs
{
    public class PaymentTimeoutJob
    {
        private readonly IOrderService _orderService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentTimeoutJob> _logger;

        private const int TimeoutMinutes = 10;

        public PaymentTimeoutJob(
            IOrderService orderService,
            IUnitOfWork unitOfWork,
            ILogger<PaymentTimeoutJob> logger)
        {
            _orderService = orderService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 120 })]
        [DisplayName("Cancel Unpaid PayOS Orders (Timeout)")]
        public async Task CancelTimedOutOrdersAsync()
        {
            _logger.LogInformation("[Job] PaymentTimeoutJob started at {Time}", DateTime.UtcNow);

            var cutoffTime = DateTime.UtcNow.AddMinutes(-TimeoutMinutes);
            _logger.LogInformation("[Job] Looking for orders created before {CutoffTime}", cutoffTime);

            // We still query here to find which orders to cancel
            // But the cancellation logic is delegated to OrderService
            var timedOutOrders = await _orderService.GetOrdersByStatusAndTimeoutAsync(OrderStatus.AwaitingPayment, cutoffTime);

            if (!timedOutOrders.Any())
            {
                _logger.LogInformation("[Job] No orders found matching the timeout criteria.");
                return;
            }

            _logger.LogInformation("[Job] Found {Count} timed-out orders to cancel", timedOutOrders.Count);

            foreach (var order in timedOutOrders)
            {
                try
                {
                    await _orderService.CancelOrderAsync(order.Id);
                    _logger.LogInformation("[Job] Cancelled order {OrderCode}, stock restored", order.OrderCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[Job] Failed to cancel order {OrderCode}", order.OrderCode);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[Job] PaymentTimeoutJob finished. {Count} orders cancelled.", timedOutOrders.Count);
        }

        [DisplayName("Cancel Order {0} If Unpaid")]
        public async Task CancelOrderIfUnpaid(Guid orderId)
        {
            _logger.LogInformation("[Job] Checking payment status for order {OrderId}", orderId);

            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order == null)
            {
                _logger.LogWarning("[Job] Order {OrderId} not found.", orderId);
                return;
            }

            if (order.SubOrders.All(so => so.Status != OrderStatus.AwaitingPayment))
            {
                _logger.LogInformation("[Job] Order {OrderId} does not have AwaitingPayment sub-orders. Status: {Status}", orderId, order.SubOrders.FirstOrDefault()?.Status);
                return;
            }

            try
            {
                await _orderService.CancelOrderAsync(order.Id);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("[Job] Successfully cancelled unpaid order {OrderCode} via real-time timeout.", order.OrderCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Job] Failed to cancel unpaid order {OrderCode}", order.OrderCode);
                throw;
            }
        }

        [DisplayName("Cancel Order {0} From Buyer Request")]
        public async Task CancelOrderOnRequest(Guid orderId, string? userId)
        {
            _logger.LogInformation("[Job] Processing buyer-requested cancellation for order {OrderId}", orderId);

            try
            {
                await _orderService.CancelOrderAsync(orderId, userId);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("[Job] Successfully processed buyer-requested cancellation for order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Job] Failed to process buyer-requested cancellation for order {OrderId}", orderId);
                throw;
            }
        }
    }
}
