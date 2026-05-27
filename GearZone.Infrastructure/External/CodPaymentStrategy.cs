using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Payment.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;


namespace GearZone.Infrastructure.External
{
    public class CodPaymentStrategy : IPaymentStrategy
    {
        public PaymentMethod Method => PaymentMethod.COD;

        public Task<PaymentResult> ProcessPaymentAsync(Order order)
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Method = PaymentMethod.COD,
                Provider = "COD",
                Amount = order.GrandTotal,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            order.Payments.Add(payment);

            return Task.FromResult(
                new PaymentResult(
                    success: true,
                    checkoutUrl: null,
                    paymentLinkId: null,
                    errorMessage: null
                )
            );
        }
    }
}
