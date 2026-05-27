namespace GearZone.Application.Features.Admin.Dtos
{
    public enum WalletStatusLevel
    {
        Healthy,
        Warning,
        Low
    }

    public class WalletSummaryDto
    {
        /// <summary>Live balance from PayOS BaoKim account</summary>
        public string? AvailableBalance { get; set; }

        /// <summary>Raw numeric balance, null if API failed</summary>
        public decimal? AvailableBalanceRaw { get; set; }

        /// <summary>Sum of PayoutBatch amounts in Approved/Processing status</summary>
        public decimal PendingPayoutAmount { get; set; }

        /// <summary>Sum of queued sub-order NetAmounts not yet packed into a batch</summary>
        public decimal NextBatchRequiredAmount { get; set; }

        public WalletStatusLevel StatusLevel { get; set; } = WalletStatusLevel.Healthy;

        /// <summary>Whether the live balance API call succeeded</summary>
        public bool IsBalanceLive { get; set; }
    }
}
