using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;

namespace GearZone.Application.Features.Seller.Dtos
{
    public class SellerOrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int UnpaidOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    /// <summary>Paged seller orders plus the header stat tiles, in one payload.</summary>
    public class SellerOrderListDto
    {
        public PagedResult<SellerChatOrderListItemDto> Orders { get; set; } = new();
        public SellerOrderStatsDto Stats { get; set; } = new();
    }
}
