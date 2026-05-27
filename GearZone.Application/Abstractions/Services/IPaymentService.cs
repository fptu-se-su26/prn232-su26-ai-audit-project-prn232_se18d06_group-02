using GearZone.Application.Features.Payment.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IPaymentService
    {
        Task<PaymentVerificationResult> VerifyAndConfirmPaymentAsync(long orderCode, CancellationToken ct = default);
    }
}
