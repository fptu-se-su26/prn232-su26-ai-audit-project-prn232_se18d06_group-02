using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IPromotionReservationRepository : IRepository<PromotionReservation, Guid>
    {
        Task<List<PromotionReservation>> GetByOrderAsync(
            Guid orderId,
            CancellationToken ct = default);
    }
}
