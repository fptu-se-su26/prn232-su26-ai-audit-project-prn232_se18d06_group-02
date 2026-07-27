using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Promotions
{
    public class PromotionLifecycleService : IPromotionLifecycleService
    {
        private readonly IPromotionCampaignRepository _campaigns;
        private readonly IPromotionReservationRepository _reservations;
        private readonly TimeProvider _timeProvider;

        public PromotionLifecycleService(
            IPromotionCampaignRepository campaigns,
            IPromotionReservationRepository reservations,
            TimeProvider timeProvider)
        {
            _campaigns = campaigns;
            _reservations = reservations;
            _timeProvider = timeProvider;
        }

        public async Task ReserveForOrderAsync(
            Order order,
            CancellationToken ct = default)
        {
            var promotedItems = order.SubOrders
                .SelectMany(x => x.Items)
                .Where(x => x.PromotionCampaignId.HasValue)
                .ToList();

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            foreach (var group in promotedItems.GroupBy(x => x.PromotionCampaignId!.Value))
            {
                var quantity = group.Sum(x => x.Quantity);
                if (!await _campaigns.TryReserveQuantityAsync(group.Key, quantity, now, ct))
                {
                    throw new PromotionQuotaExceededException();
                }
            }

            foreach (var item in promotedItems)
            {
                await _reservations.AddAsync(new PromotionReservation
                {
                    Id = Guid.NewGuid(),
                    CampaignId = item.PromotionCampaignId!.Value,
                    OrderId = order.Id,
                    OrderItemId = item.Id,
                    Quantity = item.Quantity,
                    Status = PromotionReservationStatus.Reserved,
                    CreatedAt = now
                }, ct);
            }
        }

        public async Task RedeemForOrderAsync(
            Guid orderId,
            Guid? storeId = null,
            CancellationToken ct = default)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var reservations = await _reservations.GetByOrderAsync(orderId, ct);
            foreach (var reservation in reservations.Where(x =>
                         x.Status == PromotionReservationStatus.Reserved &&
                         (!storeId.HasValue || x.OrderItem.SubOrder.StoreId == storeId.Value)))
            {
                reservation.Status = PromotionReservationStatus.Redeemed;
                reservation.RedeemedAt = now;
                reservation.Campaign.ReservedQuantity =
                    Math.Max(0, reservation.Campaign.ReservedQuantity - reservation.Quantity);
                reservation.Campaign.RedeemedQuantity += reservation.Quantity;
                reservation.Campaign.UpdatedAt = now;
                await _reservations.UpdateAsync(reservation);
                await _campaigns.UpdateAsync(reservation.Campaign);
            }
        }

        public async Task ReleaseForOrderAsync(
            Guid orderId,
            Guid? storeId = null,
            CancellationToken ct = default)
        {
            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var reservations = await _reservations.GetByOrderAsync(orderId, ct);
            foreach (var reservation in reservations.Where(x =>
                         x.Status != PromotionReservationStatus.Released &&
                         (!storeId.HasValue || x.OrderItem.SubOrder.StoreId == storeId.Value)))
            {
                if (reservation.Status == PromotionReservationStatus.Reserved)
                {
                    reservation.Campaign.ReservedQuantity =
                        Math.Max(0, reservation.Campaign.ReservedQuantity - reservation.Quantity);
                }
                else
                {
                    reservation.Campaign.RedeemedQuantity =
                        Math.Max(0, reservation.Campaign.RedeemedQuantity - reservation.Quantity);
                }

                reservation.Status = PromotionReservationStatus.Released;
                reservation.ReleasedAt = now;
                reservation.Campaign.UpdatedAt = now;
                await _reservations.UpdateAsync(reservation);
                await _campaigns.UpdateAsync(reservation.Campaign);
            }
        }
    }
}
