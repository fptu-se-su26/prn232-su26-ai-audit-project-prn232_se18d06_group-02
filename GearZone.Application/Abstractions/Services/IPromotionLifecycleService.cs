using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Services
{
    public interface IPromotionLifecycleService
    {
        Task ReserveForOrderAsync(Order order, CancellationToken ct = default);
        Task RedeemForOrderAsync(
            Guid orderId,
            Guid? storeId = null,
            CancellationToken ct = default);
        Task ReleaseForOrderAsync(
            Guid orderId,
            Guid? storeId = null,
            CancellationToken ct = default);
    }
}
