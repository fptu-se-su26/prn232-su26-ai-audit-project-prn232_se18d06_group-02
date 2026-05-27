using GearZone.Application.Abstractions.External;

namespace GearZone.Infrastructure.External
{
    public class DisabledPaymentGateway : IPaymentGateway
    {
        public Task<PaymentGatewayResult> GetPaymentStatusAsync(long orderCode)
        {
            return Task.FromResult(PaymentGatewayResult.Error(
                "PayOS payment gateway is not configured."));
        }
    }
}
