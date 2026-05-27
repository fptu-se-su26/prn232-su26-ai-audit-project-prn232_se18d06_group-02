namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminOrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PaidOrders { get; set; }
        public int UnpaidOrders { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
