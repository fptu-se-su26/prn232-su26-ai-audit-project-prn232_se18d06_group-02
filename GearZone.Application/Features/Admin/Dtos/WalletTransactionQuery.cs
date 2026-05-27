using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class WalletTransactionQuery
    {
        public string? Search { get; set; }
        public WalletTransactionType? Type { get; set; }
        public WalletTransactionStatus? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
