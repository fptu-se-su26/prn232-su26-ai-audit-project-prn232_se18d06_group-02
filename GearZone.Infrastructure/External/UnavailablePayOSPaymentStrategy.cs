using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Payment.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GearZone.Infrastructure.External
{
    public class UnavailablePayOSPaymentStrategy : IPaymentStrategy
    {
        private readonly ILogger<UnavailablePayOSPaymentStrategy> _logger;

        public UnavailablePayOSPaymentStrategy(ILogger<UnavailablePayOSPaymentStrategy> logger)
        {
            _logger = logger;
        }

        public PaymentMethod Method => PaymentMethod.PayOS;

        public Task<PaymentResult> ProcessPaymentAsync(Order order)
        {
            _logger.LogWarning(
                "PayOS checkout requested for order {OrderCode} while PayOS is not configured.",
                order.OrderCode);

            return Task.FromResult(new PaymentResult(
                success: false,
                checkoutUrl: null,
                errorMessage: "PayOS is not configured. Please use COD or configure PAYOS_* variables."));
        }
    }
}
