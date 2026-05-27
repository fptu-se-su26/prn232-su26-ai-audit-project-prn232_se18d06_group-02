using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services;

public interface IPayoutService
{
    /// <summary>
    /// Generates a new Payout Batch aggregating orders up to the specified end date.
    /// Returns the generated PayoutBatch ID or an error result.
    /// </summary>
    Task<Guid> GenerateWeeklyBatchAsync(DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves and processes a specific Payout Transaction within a Batch.
    /// Initiates the transfer via PayOS.
    /// </summary>
    Task ProcessPayoutTransactionAsync(string transactionCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves all eligible transactions in a batch.
    /// </summary>
    Task ProcessPayoutBatchAsync(string batchCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an approved payout batch for selected stores in the provided period.
    /// Returns generated batch code.
    /// </summary>
    Task<string> GenerateApprovedBatchForStoresAsync(
        DateTime periodStart,
        DateTime periodEnd,
        IReadOnlyCollection<Guid> storeIds,
        string adminId,
        CancellationToken cancellationToken = default);

    Task ApproveBatchAsync(Guid batchId, string adminId, CancellationToken cancellationToken = default);

    Task RetryTransactionAsync(Guid transactionId, CancellationToken cancellationToken = default);

    Task RetryAllFailedTransactionsAsync(CancellationToken cancellationToken = default);

    Task HoldBatchAsync(Guid batchId, string reason, CancellationToken cancellationToken = default);

    Task ExcludeTransactionAsync(Guid transactionId, string reason, CancellationToken cancellationToken = default);
}
