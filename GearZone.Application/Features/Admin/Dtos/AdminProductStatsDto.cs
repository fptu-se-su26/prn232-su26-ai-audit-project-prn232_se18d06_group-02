namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int PendingApproval { get; set; }
        public int OutOfStock { get; set; }
    }
}
