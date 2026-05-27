using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.Services
{
    public interface IVoucherService
    {
        Task<VoucherValidationResult> ValidateVoucherAsync(string code, string userId, decimal merchandiseTotal, decimal shippingFee, VoucherType expectedType);
        Task<List<AvailableVoucherDto>> GetAvailableVouchersForCheckoutAsync(string userId, decimal merchandiseTotal, decimal shippingFee, VoucherType type);
        Task RecordVoucherUsageAsync(Guid voucherId, string userId, Guid orderId, decimal discountAmount);
    }
}
