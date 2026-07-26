using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Payout;
using GearZone.Application.Features.Admin;
using GearZone.Domain.Enums;
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
        private readonly IAdminAuditRecorder _auditRecorder;

        public PayoutBatchJob(
            IPayoutService payoutService,
            IBackgroundJobClient backgroundJobs,
            ILogger<PayoutBatchJob> logger,
            IAdminAuditRecorder auditRecorder)
        {
            _payoutService = payoutService;
            _backgroundJobs = backgroundJobs;
            _logger = logger;
            _auditRecorder = auditRecorder;
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
        public async Task ProcessApprovedBatchAsync(string batchCode, string? correlationId, string? triggeredByAdminId)
        {
            _logger.LogInformation(
                "[Job] ProcessApprovedBatch {Code} started", batchCode);

            try
            {
                await _payoutService.ProcessPayoutBatchAsync(batchCode);
                await RecordOutcomeAsync(
                    AdminAuditActions.PayoutProcessSucceeded,
                    AdminAuditOutcome.Succeeded,
                    batchCode,
                    correlationId,
                    triggeredByAdminId);
            }
            catch (Exception ex)
            {
                await RecordOutcomeAsync(
                    AdminAuditActions.PayoutProcessFailed,
                    AdminAuditOutcome.Failed,
                    batchCode,
                    correlationId,
                    triggeredByAdminId,
                    ex.GetType().Name);
                throw;
            }
        }

        private Task RecordOutcomeAsync(
            string action,
            AdminAuditOutcome outcome,
            string batchCode,
            string? correlationId,
            string? triggeredByAdminId,
            string? failureType = null) =>
            _auditRecorder.RecordAsync(new AdminAuditEvent
            {
                ActorUserId = triggeredByAdminId,
                ActorDisplayName = "Background payout job",
                Action = action,
                Module = AdminAuditModules.Finance,
                Outcome = outcome,
                RiskLevel = AdminAuditRiskLevel.Critical,
                EntityType = "PayoutBatch",
                EntityId = batchCode,
                EntityDisplayName = batchCode,
                Description = outcome == AdminAuditOutcome.Succeeded
                    ? "Background payout processing completed"
                    : "Background payout processing failed",
                CorrelationId = correlationId,
                StatusCode = outcome == AdminAuditOutcome.Succeeded ? 200 : 500,
                Metadata = new Dictionary<string, string?>
                {
                    ["batchCode"] = batchCode,
                    ["failureType"] = failureType
                }
            });

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
