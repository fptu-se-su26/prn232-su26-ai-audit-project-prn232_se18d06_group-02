using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace GearZone.Application.Features.Admin
{
    public class AdminWalletService : IAdminWalletService
    {
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly IPayoutBatchRepository _payoutBatchRepository;
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPayoutClient _payoutClient;
        private readonly ILogger<AdminWalletService> _logger;

        public AdminWalletService(
            IWalletTransactionRepository walletTransactionRepository,
            IPayoutBatchRepository payoutBatchRepository,
            ISubOrderRepository subOrderRepository,
            IUnitOfWork unitOfWork,
            IPayoutClient payoutClient,
            ILogger<AdminWalletService> logger)
        {
            _walletTransactionRepository = walletTransactionRepository;
            _payoutBatchRepository = payoutBatchRepository;
            _subOrderRepository = subOrderRepository;
            _unitOfWork = unitOfWork;
            _payoutClient = payoutClient;
            _logger = logger;
        }

        public async Task<WalletSummaryDto> GetWalletSummaryAsync(CancellationToken ct = default)
        {
            var summary = new WalletSummaryDto();

            // 1. Live balance from PayOS
            try
            {
                var accountInfo = await _payoutClient.GetAccountBalance();
                if (accountInfo != null)
                {
                    summary.AvailableBalance = accountInfo.Balance;
                    if (decimal.TryParse(accountInfo.Balance, out var parsed))
                        summary.AvailableBalanceRaw = parsed;
                    summary.IsBalanceLive = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Wallet] Could not fetch live balance from PayOS");
                summary.IsBalanceLive = false;
            }

            // 2. Pending payout = sum of TotalNetAmount for Approved/Processing batches
            summary.PendingPayoutAmount = await _payoutBatchRepository.GetTotalNetAmountByStatusesAsync(
                new[] { PayoutBatchStatus.Approved, PayoutBatchStatus.Processing }, ct);

            // 3. Next batch required = sum of SubOrders that are Completed but Unpaid (Eligible for payout)
            summary.NextBatchRequiredAmount = await _subOrderRepository.GetTotalEligiblePayoutAmountAsync(ct);

            // 4. Determine status level
            var balance = summary.AvailableBalanceRaw ?? 0m;
            var threshold = summary.PendingPayoutAmount;
            
            if (balance == 0 && !summary.IsBalanceLive)
                summary.StatusLevel = WalletStatusLevel.Warning;
            else if (balance < threshold * 0.5m)
                summary.StatusLevel = WalletStatusLevel.Low;
            else if (balance < threshold)
                summary.StatusLevel = WalletStatusLevel.Warning;
            else
                summary.StatusLevel = WalletStatusLevel.Healthy;

            return summary;
        }

        public async Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(
            WalletTransactionQuery query,
            CancellationToken ct = default)
        {
            return await _walletTransactionRepository.GetPagedAsync(query, ct);
        }

        public async Task<List<WalletTransactionDto>> GetBalanceHistoryAsync(int days = 30, CancellationToken ct = default)
        {
            return await _walletTransactionRepository.GetRecentAsync(days, ct);
        }

        public async Task<List<WalletTransactionDto>> GetCashFlowHistoryAsync(int recentMonths = 6, CancellationToken ct = default)
        {
            var normalizedMonths = recentMonths < 1 ? 1 : recentMonths;
            var now = DateTime.UtcNow;
            var fromUtc = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddMonths(-(normalizedMonths - 1));

            return await _walletTransactionRepository.GetCompletedSinceAsync(fromUtc, ct);
        }

        public async Task RecordTopupAsync(
            TopupWalletDto dto,
            string adminId,
            CancellationToken ct = default)
        {
            // Get current balance snapshot from last completed transaction
            var lastTx = await _walletTransactionRepository.GetLastCompletedTransactionAsync(ct);

            var balanceBefore = lastTx?.BalanceAfter ?? 0m;
            var txCode = $"WTX-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var transaction = new WalletTransaction
            {
                Id = Guid.NewGuid(),
                TransactionCode = txCode,
                Type = WalletTransactionType.Topup,
                Direction = TransactionDirection.IN,
                Amount = dto.Amount,
                Currency = "VND",
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceBefore + dto.Amount,
                ReferenceCode = $"TOPUP-{DateTime.UtcNow:yyyyMMddHHmmss}",
                ProviderTransactionId = dto.ProviderTransactionId,
                Note = dto.Note,
                Status = WalletTransactionStatus.Completed,
                CreatedByAdminId = adminId,
                CreatedAt = DateTime.UtcNow
            };

            await _walletTransactionRepository.AddAsync(transaction, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("[Wallet] Topup recorded: {Code} = {Amount} by {Admin}",
                txCode, dto.Amount, adminId);
        }
    }
}
