namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminPlatformTransactionSummaryDto
    {
        public decimal TotalInflow { get; set; }
        public decimal TotalOutflow { get; set; }
        public int TotalTransactions { get; set; }
        public decimal WalletBalance { get; set; }
    }
}
