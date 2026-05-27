using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Features.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly IUserRepository _userRepository;

        public AdminDashboardService(
            ISubOrderRepository subOrderRepository,
            IStoreRepository storeRepository,
            IUserRepository userRepository)
        {
            _subOrderRepository = subOrderRepository;
            _storeRepository = storeRepository;
            _userRepository = userRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardDataAsync(DashboardQuery query)
        {
            var (start, end) = query.GetDateRange();
            var (prevStart, prevEnd) = query.GetPreviousDateRange();

            var dto = new AdminDashboardDto();

            // 1. KPI Cards
            dto.KpiCards = await GetKpisAsync(start, end, prevStart, prevEnd);

            // 2. Revenue Overview Chart
            dto.RevenueOverview = await _subOrderRepository.GetRevenueOverviewAsync(start, end, query.Period);

            // 3. Category Breakdown
            dto.RevenueByCategory = await _subOrderRepository.GetCategoryBreakdownAsync(start, end);

            // 4. Order Status Breakdown
            dto.OrderStatusBreakdown = await _subOrderRepository.GetOrderStatusBreakdownAsync(start, end);

            // 5. Top Stores
            dto.TopStores = await _subOrderRepository.GetTopStoresAsync(start, end);

            // 6. User Growth
            dto.UserGrowth = await _userRepository.GetUserGrowthAsync(start, end, query.Period);

            return dto;
        }

        private async Task<DashboardKpis> GetKpisAsync(DateTime start, DateTime end, DateTime prevStart, DateTime prevEnd)
        {
            var kpis = new DashboardKpis();

            // Current Period
            var currentGrossRevenue = await _subOrderRepository.GetGrossRevenueAsync(start, end);
            var currentOrders = await _subOrderRepository.GetTotalOrdersCountAsync(start, end);
            var currentActiveStores = await _storeRepository.GetActiveStoresCountAsync();
            var currentNewUsers = await _userRepository.GetNewUsersCountAsync(start, end);

            // Previous Period for Growth
            var prevGrossRevenue = await _subOrderRepository.GetGrossRevenueAsync(prevStart, prevEnd);
            var prevOrders = await _subOrderRepository.GetTotalOrdersCountAsync(prevStart, prevEnd);
            var prevNewUsers = await _userRepository.GetNewUsersCountAsync(prevStart, prevEnd);

            kpis.GrossRevenue = currentGrossRevenue;
            kpis.TotalOrders = currentOrders;
            kpis.ActiveStores = currentActiveStores;
            kpis.NewUsers = currentNewUsers;
            kpis.DisputeRate = 1.8m; // Prototype as requested

            kpis.RevenueGrowth = CalculateGrowth(currentGrossRevenue, prevGrossRevenue);
            kpis.OrderGrowth = CalculateGrowth(currentOrders, prevOrders);
            kpis.UserGrowth = CalculateGrowth(currentNewUsers, prevNewUsers);

            return kpis;
        }

        private decimal CalculateGrowth(decimal current, decimal previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0;
            return (current - previous) / previous * 100;
        }
    }
}
