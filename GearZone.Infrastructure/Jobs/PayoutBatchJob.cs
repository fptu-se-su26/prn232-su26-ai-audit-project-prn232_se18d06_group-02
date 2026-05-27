using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Payout;
using Hangfire;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace GearZone.Infrastructure.Jobs
{
    public class PayoutBatchJob
    {
        private readonly IPayoutService _payoutService;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly ILogger<PayoutBatchJob> _logger;

        public PayoutBatchJob(
            IPayoutService payoutService,
            IBackgroundJobClient backgroundJobs,
            ILogger<PayoutBatchJob> logger)
        {
            _payoutService = payoutService;
            _backgroundJobs = backgroundJobs;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 0)] // Không retry — tránh tạo batch 2 lần
        [DisplayName("Generate Weekly Payout Batch")]
        public async Task GenerateWeeklyBatchAsync()
        {
            _logger.LogInformation("[Job] GenerateWeeklyBatch started");

            var batchId = await _payoutService.GenerateWeeklyBatchAsync(DateTime.UtcNow);

            _logger.LogInformation(
                "[Job] Batch {Id} created → PendingApproval. Admin review required.",
                batchId);

            // Batch tạo xong → PendingApproval
            // Chờ Admin vào dashboard duyệt
            // Admin approve → Controller trigger ProcessBatchAsync
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 60, 300 })]
        [DisplayName("Process Approved Payout Batch {0}")]
        public async Task ProcessApprovedBatchAsync(string batchCode)
        {
            _logger.LogInformation(
                "[Job] ProcessApprovedBatch {Code} started", batchCode);

            await _payoutService.ProcessPayoutBatchAsync(batchCode);
        }

        // Mỗi 6 tiếng — Cron: "0 */6 * * *"
        [AutomaticRetry(Attempts = 1)]
        [DisplayName("Retry Failed Payout Transactions")]
        public async Task RetryFailedTransactionsAsync()
        {
            _logger.LogInformation("[Job] RetryFailedTransactions started");

            await _payoutService.RetryAllFailedTransactionsAsync();
        }
    }
}
