using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Payment.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IOrderTrackingNotifier _orderTrackingNotifier;
        private readonly IPromotionLifecycleService _promotionLifecycle;
        private readonly IVoucherService _voucherService;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IUnitOfWork unitOfWork,
            IPaymentGateway paymentGateway,
            IOrderTrackingNotifier orderTrackingNotifier,
            IPromotionLifecycleService promotionLifecycle,
            IVoucherService voucherService,
            ILogger<PaymentService> logger)
        {
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _paymentGateway = paymentGateway;
            _orderTrackingNotifier = orderTrackingNotifier;
            _promotionLifecycle = promotionLifecycle;
            _voucherService = voucherService;
            _logger = logger;
        }

        public async Task<PaymentVerificationResult> VerifyAndConfirmPaymentAsync(
            long orderCode, CancellationToken ct = default)
        {
            try
            {
                // 1. Find the order by OrderCode
                var order = await _orderRepository.Query()
                    .Include(o => o.Payments)
                    .Include(o => o.SubOrders)
                        .ThenInclude(so => so.Items)
                            .ThenInclude(oi => oi.Variant)
                                .ThenInclude(v => v.Product)
                    .Include(o => o.StatusHistories)
                    .FirstOrDefaultAsync(o => o.OrderCode == orderCode, ct);

                if (order == null)
                    return PaymentVerificationResult.Fail("Order not found.");

                var payment = order.Payments.FirstOrDefault(p => p.Method == PaymentMethod.PayOS);
                if (payment == null)
                    return PaymentVerificationResult.Fail("Payment record not found.");

                // Already confirmed
                if (payment.Status == PaymentStatus.Paid)
                    return PaymentVerificationResult.Ok(order.Id);

                // 2. Call payment gateway to verify
                var gatewayResult = await _paymentGateway.GetPaymentStatusAsync(orderCode);

                if (gatewayResult.ErrorMessage != null)
                    return PaymentVerificationResult.Fail(gatewayResult.ErrorMessage);

                // 3. If paid, update everything
                if (gatewayResult.IsPaid)
                {
                    await _unitOfWork.ExecuteInTransactionAsync(async transactionCt =>
                    {
                        var paidAt = DateTime.UtcNow;
                        payment.Status = PaymentStatus.Paid;
                        payment.TransactionRef = gatewayResult.TransactionId;
                        payment.PaidAt = paidAt;
                        payment.UpdatedAt = paidAt;
                        await _paymentRepository.UpdateAsync(payment);

                        order.PaidAt = paidAt;
                        order.UpdatedAt = paidAt;

                        foreach (var subOrder in order.SubOrders)
                        {
                            subOrder.Status = OrderStatus.Paid;
                            subOrder.UpdatedAt = paidAt;
                        }

                        ApplyPaidSoldCount(order);

                        order.StatusHistories.Add(new OrderStatusHistory
                        {
                            NewStatus = OrderStatus.Paid,
                            ChangedAt = paidAt,
                            Note = "Payment confirmed via PayOS"
                        });

                        await _promotionLifecycle.RedeemForOrderAsync(
                            order.Id,
                            null,
                            transactionCt);
                        await _voucherService.RedeemOrderVouchersAsync(
                            order.Id,
                            null,
                            transactionCt);
                        await _unitOfWork.SaveChangesAsync(transactionCt);
                        return true;
                    }, ct);

                    foreach (var subOrder in order.SubOrders)
                    {
                        await _orderTrackingNotifier.NotifySubOrderUpdatedAsync(subOrder.Id, ct);
                    }

                    return PaymentVerificationResult.Ok(order.Id);
                }

                return PaymentVerificationResult.Fail(
                    $"Payment not completed. Current status: {gatewayResult.Status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment for order {OrderCode}", orderCode);
                return PaymentVerificationResult.Fail("An error occurred while verifying payment.");
            }
        }

        private static void ApplyPaidSoldCount(Order order)
        {
            var soldByProduct = order.SubOrders
                .SelectMany(so => so.Items)
                .Where(oi => oi.Variant?.Product != null)
                .GroupBy(oi => oi.Variant.ProductId)
                .Select(group => new
                {
                    Product = group.First().Variant.Product,
                    Quantity = group.Sum(item => item.Quantity)
                });

            foreach (var item in soldByProduct)
            {
                if (item.Product.IsDeleted)
                {
                    continue;
                }

                item.Product.SoldCount += item.Quantity;
            }
        }
    }
}
