using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common;
using GearZone.Application.Features.Payout.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GearZone.Application.Features.Payout
{
    public class PayoutService : IPayoutService
    {
        private const int MaxRetryCount = 3;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPayoutClient _payoutClient;
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly IPayoutBatchRepository _payoutBatchRepository;
        private readonly IPayoutTransactionRepository _payoutTransactionRepository;
        private readonly IPayoutItemRepository _payoutItemRepository;
        private readonly IWalletTransactionRepository _walletTransactionRepository;
        private readonly ILogger<PayoutService> _logger;

        public PayoutService(
            IUnitOfWork unitOfWork,
            IPayoutClient payoutClient,
            ISubOrderRepository subOrderRepository,
            IPayoutBatchRepository payoutBatchRepository,
            IPayoutTransactionRepository payoutTransactionRepository,
            IPayoutItemRepository payoutItemRepository,
            IWalletTransactionRepository walletTransactionRepository,
            ILogger<PayoutService> logger)
        {
            _unitOfWork = unitOfWork;
            _payoutClient = payoutClient;
            _subOrderRepository = subOrderRepository;
            _payoutBatchRepository = payoutBatchRepository;
            _payoutTransactionRepository = payoutTransactionRepository;
            _payoutItemRepository = payoutItemRepository;
            _walletTransactionRepository = walletTransactionRepository;
            _logger = logger;
        }

        public async Task<Guid> GenerateWeeklyBatchAsync(
        DateTime endDate,
        CancellationToken ct = default)
        {
            // 1. Calculate period
            var periodEnd = endDate.Date;
            var periodStart = periodEnd.AddDays(-7);

            _logger.LogInformation(
                "[Payout] Generating batch {Start:dd/MM} - {End:dd/MM}",
                periodStart, periodEnd);

            // 2. Check duplicates
            var exists = await _payoutBatchRepository.ExistsByPeriodAsync(
                periodStart, periodEnd, ct);

            if (exists)
                throw new InvalidOperationException($"Batch for {periodStart:dd/MM} - {periodEnd:dd/MM} already exists.");

            // 3. Get eligible orders (SubOrders)
            var eligibleSubOrders = await _subOrderRepository
                .GetEligibleForPayoutAsync(periodStart, periodEnd, ct);

            _logger.LogInformation(
                "[Payout] Found {Count} eligible orders", eligibleSubOrders.Count);

            // 4. Group by store
            var storeGroups = eligibleSubOrders
                .GroupBy(o => o.StoreId)
                .ToList();

            // 5. Build batch
            var weekNum = GetWeekNumber(periodStart);
            var batch = new PayoutBatch
            {
                Id = Guid.NewGuid(),
                BatchCode = $"BATCH-{periodStart:yyyy}-W{weekNum:D2}",
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Status = PayoutBatchStatus.PendingApproval,
                TotalStores = storeGroups.Count,
                CreatedAt = DateTime.UtcNow,
            };

            // 6. Build transactions + items
            var transactions = new List<PayoutTransaction>();
            var sequence = 1;

            foreach (var group in storeGroups)
            {
                var store = group.First().Store;
                var orders = group.ToList();

                var items = orders.Select(o => new PayoutItem
                {
                    Id = Guid.NewGuid(),
                    SubOrderId = o.Id,
                    GrandTotal = o.Subtotal,
                    CommissionAmount = o.CommissionAmount,
                    NetAmount = o.Subtotal - o.CommissionAmount,
                    IsExcluded = false,
                }).ToList();

                var transaction = new PayoutTransaction
                {
                    Id = Guid.NewGuid(),
                    PayoutBatchId = batch.Id,
                    StoreId = group.Key,
                    TransactionCode = $"{batch.BatchCode.Replace("BATCH", "PTX")}-{sequence:D3}",
                    BankName = store.BankName,
                    BankAccountNumber = store.BankAccountNumber,
                    BankAccountName = store.BankAccountName,
                    BankBin = store.BankBin,
                    OrderCount = orders.Count,
                    GrossAmount = orders.Sum(o => o.Subtotal),
                    CommissionAmount = orders.Sum(o => o.CommissionAmount),
                    NetAmount = orders.Sum(o => o.Subtotal - o.CommissionAmount),
                    Status = PayoutTransactionStatus.Queued,
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    Items = items,
                };

                transactions.Add(transaction);
                sequence++;
            }

            // 7. Assign totals to batch
            batch.TotalGrossAmount = transactions.Sum(t => t.GrossAmount);
            batch.TotalCommissionAmount = transactions.Sum(t => t.CommissionAmount);
            batch.TotalNetAmount = transactions.Sum(t => t.NetAmount);
            batch.Transactions = transactions;

            // 8. Lock orders
            var subOrderIds = eligibleSubOrders.Select(o => o.Id).ToList();
            await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                subOrderIds, PayoutStatus.Locked, ct);

            // 9. Save batch (cascade save transactions + items)
            await _payoutBatchRepository.AddAsync(batch, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[Payout] Batch {Code} created. Stores: {S}, Net: {N}",
                batch.BatchCode, batch.TotalStores, batch.TotalNetAmount);

            return batch.Id;
        }

        // ────────────────────────────────────────────────────────────
        public async Task<string> GenerateApprovedBatchForStoresAsync(
            DateTime periodStart,
            DateTime periodEnd,
            IReadOnlyCollection<Guid> storeIds,
            string adminId,
            CancellationToken ct = default)
        {
            var uniqueStoreIds = storeIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!uniqueStoreIds.Any())
            {
                throw new InvalidOperationException("Please select at least one seller.");
            }

            var normalizedStart = periodStart.Date;
            var normalizedEnd = periodEnd.Date.AddDays(1).AddTicks(-1);
            var eligibleSubOrders = await _subOrderRepository.GetEligibleForPayoutByStoresAsync(
                normalizedStart, normalizedEnd, uniqueStoreIds, ct);

            if (!eligibleSubOrders.Any())
            {
                throw new InvalidOperationException("No eligible payouts found for selected sellers in this period.");
            }

            var storeGroups = eligibleSubOrders
                .GroupBy(o => o.StoreId)
                .ToList();

            var weekNum = GetWeekNumber(normalizedStart);
            var batch = new PayoutBatch
            {
                Id = Guid.NewGuid(),
                BatchCode = $"BATCH-{normalizedStart:yyyy}-W{weekNum:D2}-SEL-{DateTime.UtcNow:MMddHHmmssfff}",
                PeriodStart = normalizedStart,
                PeriodEnd = normalizedEnd,
                Status = PayoutBatchStatus.Approved,
                TotalStores = storeGroups.Count,
                ApprovedByAdminId = adminId,
                ApprovedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
            };

            var transactions = new List<PayoutTransaction>();
            var sequence = 1;

            foreach (var group in storeGroups)
            {
                var store = group.First().Store;
                var orders = group.ToList();

                var items = orders.Select(o => new PayoutItem
                {
                    Id = Guid.NewGuid(),
                    SubOrderId = o.Id,
                    GrandTotal = o.Subtotal,
                    CommissionAmount = o.CommissionAmount,
                    NetAmount = o.Subtotal - o.CommissionAmount,
                    IsExcluded = false,
                }).ToList();

                var transaction = new PayoutTransaction
                {
                    Id = Guid.NewGuid(),
                    PayoutBatchId = batch.Id,
                    StoreId = group.Key,
                    TransactionCode = $"{batch.BatchCode.Replace("BATCH", "PTX")}-{sequence:D3}",
                    BankName = store.BankName,
                    BankAccountNumber = store.BankAccountNumber,
                    BankAccountName = store.BankAccountName,
                    BankBin = store.BankBin,
                    OrderCount = orders.Count,
                    GrossAmount = orders.Sum(o => o.Subtotal),
                    CommissionAmount = orders.Sum(o => o.CommissionAmount),
                    NetAmount = orders.Sum(o => o.Subtotal - o.CommissionAmount),
                    Status = PayoutTransactionStatus.Queued,
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    Items = items,
                };

                transactions.Add(transaction);
                sequence++;
            }

            batch.TotalGrossAmount = transactions.Sum(t => t.GrossAmount);
            batch.TotalCommissionAmount = transactions.Sum(t => t.CommissionAmount);
            batch.TotalNetAmount = transactions.Sum(t => t.NetAmount);
            batch.Transactions = transactions;

            var subOrderIds = eligibleSubOrders.Select(o => o.Id).ToList();
            await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                subOrderIds, PayoutStatus.Locked, ct);

            await _payoutBatchRepository.AddAsync(batch, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[Payout] Approved seller batch {Code} created with {StoreCount} stores.",
                batch.BatchCode, batch.TotalStores);

            return batch.BatchCode;
        }

        public async Task ApproveBatchAsync(
            Guid batchId,
            string adminId,
            CancellationToken ct = default)
        {
            var batch = await _payoutBatchRepository.GetByIdAsync(batchId, ct)
                ?? throw new KeyNotFoundException($"PayoutBatch with id {batchId} not found");

            if (batch.Status != PayoutBatchStatus.PendingApproval)
                throw new InvalidOperationException(
                    $"Cannot approve batch in status: {batch.Status}");

            batch.Status = PayoutBatchStatus.Approved;
            batch.ApprovedByAdminId = adminId;
            batch.ApprovedAt = DateTime.UtcNow;

            await _payoutBatchRepository.UpdateAsync(batch);
            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[Payout] Batch {Code} approved by {Admin}",
                batch.BatchCode, adminId);
        }

        // ────────────────────────────────────────────────────────────
        public async Task ProcessPayoutBatchAsync(
            string batchCode,
            CancellationToken ct = default)
        {
            // 1. Load batch with transactions
            var batch = await _payoutBatchRepository.Query()
                .Include(x => x.Transactions)
                .FirstOrDefaultAsync(x => x.BatchCode == batchCode, ct)
                ?? throw new KeyNotFoundException($"PayoutBatch with code {batchCode} not found");

            if (batch.Status != PayoutBatchStatus.Approved)
                throw new InvalidOperationException($"Batch {batch.BatchCode} is not Approved. Current: {batch.Status}");

            // 2. Move to Processing
            batch.Status = PayoutBatchStatus.Processing;
            await _payoutBatchRepository.UpdateAsync(batch);
            await _unitOfWork.SaveChangesAsync(ct);

            // 3. Get transactions to process
            var queued = batch.Transactions
                .Where(t => t.Status == PayoutTransactionStatus.Queued)
                .ToList();

            _logger.LogInformation(
                "[Payout] Processing batch {Code}: {Count} transactions",
                batch.BatchCode, queued.Count);

            // 4. Map → PayoutRequestDto
            var lastTx = await _walletTransactionRepository.GetLastCompletedTransactionAsync(ct);
            var runningBalance = lastTx?.BalanceAfter ?? 0m;

            var walletTxs = new List<WalletTransaction>();
            var successfulTxIds = new List<Guid>();

            foreach (var t in queued)
            {
                var request = new PayoutRequestDto
                {
                    Description = BuildPayOSDescription(t.TransactionCode),
                    Amount = (long)t.NetAmount,
                    ToAccountNumber = t.BankAccountNumber,
                    ToBin = t.BankBin,
                };

                var result = await _payoutClient.CreatePayoutAsync(request);

                if (result.IsSuccess)
                {
                    t.Status = PayoutTransactionStatus.Success;
                    t.PayOSTransactionId = result.ReferenceId;
                    t.ProcessedAt = DateTime.UtcNow;
                    successfulTxIds.Add(t.Id);

                    var balanceBefore = runningBalance;
                    var balanceAfter = runningBalance - t.NetAmount;
                    runningBalance = balanceAfter;

                    walletTxs.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        TransactionCode = $"WTX-{DateTime.UtcNow:yyyyMMddHHmmss}-{t.Id.ToString()[..6].ToUpper()}",
                        Type = WalletTransactionType.Payout,
                        Direction = TransactionDirection.OUT,
                        Amount = t.NetAmount,
                        Currency = "VND",
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceAfter,
                        ReferenceCode = batch.BatchCode,
                        PayoutBatchId = batch.Id,
                        PayoutTransactionId = t.Id,
                        Provider = "PayOS",
                        ProviderTransactionId = result.ReferenceId,
                        Status = WalletTransactionStatus.Completed,
                        Note = $"Payout to {t.BankAccountName} - {t.BankAccountNumber}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    t.Status = PayoutTransactionStatus.Failed;
                    t.FailureReason = result.ErrorMessage;

                    walletTxs.Add(new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        TransactionCode = $"WTX-{DateTime.UtcNow:yyyyMMddHHmmss}-{t.Id.ToString()[..6].ToUpper()}",
                        Type = WalletTransactionType.Payout,
                        Direction = TransactionDirection.OUT,
                        Amount = t.NetAmount,
                        Currency = "VND",
                        BalanceBefore = runningBalance,
                        BalanceAfter = runningBalance,
                        ReferenceCode = batch.BatchCode,
                        PayoutBatchId = batch.Id,
                        PayoutTransactionId = t.Id,
                        Provider = "PayOS",
                        Status = WalletTransactionStatus.Failed,
                        Note = $"[FAILED] Payout to {t.BankAccountName}: {result.ErrorMessage ?? "Unknown error"}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (successfulTxIds.Any())
            {
                var successfulSubOrderIds = await _payoutItemRepository
                    .GetSubOrderIdsByTransactionIdsAsync(successfulTxIds, ct);

                await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                    successfulSubOrderIds, PayoutStatus.Paid, ct);
            }

            await _payoutTransactionRepository.UpdateRangeAsync(queued, ct);

            // 9. Persist wallet transactions
            foreach (var wtx in walletTxs)
                await _walletTransactionRepository.AddAsync(wtx, ct);

            // 10. Recalculate batch status
            RecalculateBatchStatus(batch);
            await _payoutBatchRepository.UpdateAsync(batch);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "[Payout] Batch {Code} done → {Status}. S:{S} F:{F}",
                batch.BatchCode, batch.Status,
                batch.SuccessCount, batch.FailedCount);
        }

        // ────────────────────────────────────────────────────────────
        public async Task ProcessPayoutTransactionAsync(
            string transactionCode,
            CancellationToken ct = default)
        {
            throw new NotImplementedException("Not Implemented by batch context only yet.");
        }

        public async Task RetryTransactionAsync(
            Guid transactionId,
            CancellationToken ct = default)
        {
            var transaction = await _payoutTransactionRepository
                .GetByIdWithDetailsAsync(transactionId, ct)
                ?? throw new KeyNotFoundException($"PayoutTransaction with id {transactionId} not found");

            if (transaction.Status != PayoutTransactionStatus.Failed &&
                transaction.Status != PayoutTransactionStatus.ManualRequired)
                throw new InvalidOperationException(
                    $"Transaction {transactionId} is not in a retryable state.");

            if (transaction.RetryCount >= MaxRetryCount)
            {
                transaction.Status = PayoutTransactionStatus.ManualRequired;
                await _payoutTransactionRepository.UpdateAsync(transaction);
                await _unitOfWork.SaveChangesAsync(ct);
                _logger.LogWarning(
                    "[Payout] Tx {Id} exceeded max retries → ManualRequired",
                    transactionId);
                return;
            }

            // Retry
            transaction.Status = PayoutTransactionStatus.Processing;
            transaction.RetryCount += 1;
            await _payoutTransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync(ct);

            try
            {
                var request = new PayoutRequestDto
                {
                    Description = BuildPayOSDescription($"RTY-{transaction.TransactionCode}"),
                    Amount = (long)transaction.NetAmount,
                    ToAccountNumber = transaction.BankAccountNumber,
                    ToBin = transaction.BankBin,
                };

                var result = await _payoutClient.CreatePayoutAsync(request);

                // Get balance snapshot
                var lastTx = await _walletTransactionRepository.GetLastCompletedTransactionAsync(ct);
                var balanceBefore = lastTx?.BalanceAfter ?? 0m;

                WalletTransaction walletTx;

                if (result.IsSuccess)
                {
                    transaction.Status = PayoutTransactionStatus.Success;
                    transaction.PayOSTransactionId = result.ReferenceId;
                    transaction.ProcessedAt = DateTime.UtcNow;
                    transaction.FailureReason = null;

                    var subOrderIds = await _payoutItemRepository
                        .GetSubOrderIdsByTransactionIdAsync(transactionId, ct);
                    await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                        subOrderIds, PayoutStatus.Paid, ct);

                    // Recalculate parent batch
                    await RecalculateParentBatchAsync(
                        transaction.PayoutBatchId, ct);

                    // Create WalletTransaction OUT - Completed
                    walletTx = new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        TransactionCode = $"WTX-{DateTime.UtcNow:yyyyMMddHHmmss}-R{transaction.RetryCount}",
                        Type = WalletTransactionType.Payout,
                        Direction = TransactionDirection.OUT,
                        Amount = transaction.NetAmount,
                        Currency = "VND",
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceBefore - transaction.NetAmount,
                        ReferenceCode = transaction.Batch?.BatchCode,
                        PayoutBatchId = transaction.PayoutBatchId,
                        PayoutTransactionId = transaction.Id,
                        Provider = "PayOS",
                        ProviderTransactionId = result.ReferenceId,
                        Status = WalletTransactionStatus.Completed,
                        Note = $"[RETRY {transaction.RetryCount}] Payout to {transaction.BankAccountName} - {transaction.BankAccountNumber}",
                        CreatedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    transaction.Status = transaction.RetryCount >= MaxRetryCount
                        ? PayoutTransactionStatus.ManualRequired
                        : PayoutTransactionStatus.Failed;
                    transaction.FailureReason =
                        $"[Retry {transaction.RetryCount}] {result.ErrorMessage}";

                    // Create WalletTransaction OUT - Failed (balance unchanged)
                    walletTx = new WalletTransaction
                    {
                        Id = Guid.NewGuid(),
                        TransactionCode = $"WTX-{DateTime.UtcNow:yyyyMMddHHmmss}-R{transaction.RetryCount}",
                        Type = WalletTransactionType.Payout,
                        Direction = TransactionDirection.OUT,
                        Amount = transaction.NetAmount,
                        Currency = "VND",
                        BalanceBefore = balanceBefore,
                        BalanceAfter = balanceBefore,
                        ReferenceCode = transaction.Batch?.BatchCode,
                        PayoutBatchId = transaction.PayoutBatchId,
                        PayoutTransactionId = transaction.Id,
                        Provider = "PayOS",
                        Status = WalletTransactionStatus.Failed,
                        Note = $"[RETRY {transaction.RetryCount} FAILED] {result.ErrorMessage}",
                        CreatedAt = DateTime.UtcNow
                    };
                }

                await _walletTransactionRepository.AddAsync(walletTx, ct);
            }
            catch (Exception ex)
            {
                transaction.Status = PayoutTransactionStatus.Failed;
                transaction.FailureReason = ex.Message;
                _logger.LogError(ex,
                    "[Payout] Exception retrying transaction {Id}", transactionId);
            }

            await _payoutTransactionRepository.UpdateAsync(transaction);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // ────────────────────────────────────────────────────────────
        public async Task RetryAllFailedTransactionsAsync(
            CancellationToken ct = default)
        {
            var failedTransactions = await _payoutTransactionRepository
                .GetFailedWithRetryRemainingAsync(MaxRetryCount, ct);

            _logger.LogInformation(
                "[Payout] Retrying {Count} failed transactions",
                failedTransactions.Count);

            foreach (var transaction in failedTransactions)
            {
                await RetryTransactionAsync(transaction.Id, ct);
            }
        }

        // ────────────────────────────────────────────────────────────
        public async Task HoldBatchAsync(
            Guid batchId,
            string reason,
            CancellationToken ct = default)
        {
            var batch = await _payoutBatchRepository.GetByIdAsync(batchId, ct)
                ?? throw new KeyNotFoundException($"PayoutBatch with id {batchId} not found");

            if (batch.Status != PayoutBatchStatus.PendingApproval)
                throw new InvalidOperationException(
                    $"Can only hold batches in PendingApproval state.");

            batch.Status = PayoutBatchStatus.OnHold;
            batch.HoldReason = reason;

            await _payoutBatchRepository.UpdateAsync(batch);
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // ────────────────────────────────────────────────────────────
        public async Task ExcludeTransactionAsync(
            Guid transactionId,
            string reason,
            CancellationToken ct = default)
        {
            var transaction = await _payoutTransactionRepository
                .GetByIdAsync(transactionId, ct)
                ?? throw new KeyNotFoundException($"PayoutTransaction with id {transactionId} not found");

            if (transaction.Status != PayoutTransactionStatus.Queued)
                throw new InvalidOperationException(
                    "Can only exclude Queued transactions.");

            transaction.Status = PayoutTransactionStatus.Excluded;
            transaction.ExcludeReason = reason;
            await _payoutTransactionRepository.UpdateAsync(transaction);

            // Unlock orders linked to this transaction and return them to Unpaid
            var subOrderIds = await _payoutItemRepository
                .GetSubOrderIdsByTransactionIdAsync(transactionId, ct);
            await _subOrderRepository.BulkUpdatePayoutStatusAsync(
                subOrderIds, PayoutStatus.Unpaid, ct);

            await _unitOfWork.SaveChangesAsync(ct);
        }

        // ── Helpers ──────────────────────────────────────────────────

        private static void RecalculateBatchStatus(PayoutBatch batch)
        {
            var total = batch.Transactions.Count;
            var success = batch.Transactions.Count(t =>
                t.Status == PayoutTransactionStatus.Success);
            var excluded = batch.Transactions.Count(t =>
                t.Status == PayoutTransactionStatus.Excluded);
            var failed = batch.Transactions.Count(t =>
                t.Status == PayoutTransactionStatus.Failed ||
                t.Status == PayoutTransactionStatus.ManualRequired);

            batch.SuccessCount = success;
            batch.FailedCount = failed;
            batch.CompletedAt = DateTime.UtcNow;

            batch.Status = (success + excluded == total)
                ? PayoutBatchStatus.Completed
                : PayoutBatchStatus.PartialFailed;
        }

        private async Task RecalculateParentBatchAsync(
            Guid batchId,
            CancellationToken ct)
        {
            var batch = await _payoutBatchRepository
                .GetByIdWithTransactionsAsync(batchId, ct);
            if (batch is null) return;

            RecalculateBatchStatus(batch);
            await _payoutBatchRepository.UpdateAsync(batch);
        }

        private static string BuildPayOSDescription(string reference)
        {
            const int maxLength = 25;
            var value = $"GZ {reference}".Trim();
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        private static int GetWeekNumber(DateTime date)
        {
            var cal = System.Globalization.CultureInfo
                .InvariantCulture.Calendar;
            return cal.GetWeekOfYear(
                date,
                System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                DayOfWeek.Monday);
        }
    }
}

