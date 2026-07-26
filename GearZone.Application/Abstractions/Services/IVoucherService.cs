using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.Services
{
    public interface IVoucherService
    {
        Task<VoucherValidationResult> ValidateVoucherAsync(string code, string userId, decimal merchandiseTotal, decimal shippingFee, VoucherType expectedType);
        Task<List<AvailableVoucherDto>> GetAvailableVouchersForCheckoutAsync(string userId, decimal merchandiseTotal, decimal shippingFee, VoucherType type);
        Task<VoucherValidationResult> ValidateVoucherForContextAsync(
            string code,
            string userId,
            VoucherEvaluationContextDto context,
            VoucherType expectedType,
            CancellationToken ct = default);
        Task<List<AvailableVoucherDto>> GetAvailableVouchersForContextAsync(
            string userId,
            VoucherEvaluationContextDto context,
            VoucherType type,
            CancellationToken ct = default);
        Task<bool> ReserveVoucherUsageAsync(
            Guid voucherId,
            string userId,
            Guid orderId,
            decimal discountAmount,
            CancellationToken ct = default);
        Task RedeemOrderVouchersAsync(
            Guid orderId,
            Guid? storeId = null,
            CancellationToken ct = default,
            bool includePlatform = true);
        Task ReleaseOrderVouchersAsync(
            Guid orderId,
            Guid? storeId = null,
            CancellationToken ct = default,
            bool includePlatform = true);
        Task RecordVoucherUsageAsync(Guid voucherId, string userId, Guid orderId, decimal discountAmount);
    }
}
