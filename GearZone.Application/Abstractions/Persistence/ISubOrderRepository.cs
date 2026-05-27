using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Orders.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ISubOrderRepository : IRepository<SubOrder, Guid>
    {
        Task<List<SubOrder>> GetOrdersNotTransfer();

        Task<List<SubOrder>> GetEligibleForPayoutAsync(
            DateTime periodStart,
            DateTime periodEnd,
            CancellationToken ct = default);

        Task<List<SubOrder>> GetEligibleForPayoutByStoresAsync(
            DateTime periodStart,
            DateTime periodEnd,
            IReadOnlyCollection<Guid> storeIds,
            CancellationToken ct = default);

        Task BulkUpdatePayoutStatusAsync(
            List<Guid> subOrderIds,
            PayoutStatus status,
            CancellationToken ct = default);

        Task<PagedResult<SubOrder>> GetAdminOrdersAsync(AdminOrderQueryDto queryDto);
        Task<AdminOrderStatsDto> GetAdminOrderStatsAsync();
        Task<decimal> GetTotalEligiblePayoutAmountAsync(CancellationToken ct = default);
        
        // Dashboard Methods
        Task<List<ChartDataPoint>> GetRevenueOverviewAsync(DateTime start, DateTime end, string period, CancellationToken ct = default);
        Task<List<CategoryRevenueDto>> GetCategoryBreakdownAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<List<OrderStatusBreakdownDto>> GetOrderStatusBreakdownAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<List<DashboardStoreDto>> GetTopStoresAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<decimal> GetGrossRevenueAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<int> GetTotalOrdersCountAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<List<SubOrder>> GetDeliveredOrdersForAutoCompleteAsync(int days, CancellationToken ct = default);


        Task<PagedResult<UserOrderDto>> GetUserOrdersAsync(string userId, UserOrderQueryDto queryDto, DateTime utcNow, CancellationToken ct = default);
        Task<UserOrderStatusSummaryDto> GetUserOrderStatusSummaryAsync(string userId, DateTime utcNow, CancellationToken ct = default);
        Task<PagedResult<SellerChatOrderListItemDto>> GetSellerChatOrdersAsync(string ownerUserId, SellerChatOrderQueryDto queryDto, CancellationToken ct = default);
        Task<SubOrder?> GetSellerChatSubOrderAsync(string ownerUserId, Guid subOrderId, CancellationToken ct = default);
        Task<SellerChatOrderDetailDto?> GetSellerChatOrderDetailAsync(string ownerUserId, Guid subOrderId, CancellationToken ct = default);
        Task<List<ChatContextOrderDto>> GetConversationOrderContextAsync(string buyerUserId, Guid storeId, int take, CancellationToken ct = default);
    }
}
