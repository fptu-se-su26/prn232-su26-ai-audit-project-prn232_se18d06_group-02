using System.Threading;
using System.Threading.Tasks;
using GearZone.Application.Features.Seller.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    /// <summary>Payout/revenue listing for a store owner, scoped to the caller's store.</summary>
    public interface ISellerRevenueService
    {
        Task<SellerRevenueDto> GetRevenueAsync(string ownerUserId, SellerRevenueQueryDto query, CancellationToken ct = default);
    }
}
