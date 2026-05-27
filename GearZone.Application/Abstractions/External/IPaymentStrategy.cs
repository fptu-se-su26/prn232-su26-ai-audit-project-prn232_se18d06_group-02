using GearZone.Application.Features.Payment.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.External
{
    public interface IPaymentStrategy
    {
        PaymentMethod Method { get; }

        Task<PaymentResult> ProcessPaymentAsync(Order order);
    }
}
