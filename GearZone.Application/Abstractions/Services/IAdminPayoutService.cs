using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminPayoutService
    {
        Task<PagedResult<AdminPayoutTransactionDto>> GetPayoutTransactionsAsync(PayoutTransactionQueryDto query);
        Task<AdminPayoutBatchSummaryDto> GetPayoutSummaryAsync(AdminPayoutBatchQueryDto query);
        Task<PagedResult<AdminPayoutBatchDto>> GetPayoutBatchesAsync(AdminPayoutBatchQueryDto query);
        Task<AdminPayoutBatchDto?> GetPayoutBatchDetailAsync(Guid id);

        // Transactions
        Task<AdminPayoutTransactionSummaryDto> GetPayoutTransactionSummaryAsync(PayoutTransactionQueryDto query);
        Task<AdminPayoutTransactionDetailDto?> GetPayoutTransactionDetailAsync(Guid id);
        Task<List<AdminSellerPayableSummaryDto>> GetSellerPayableSummaryAsync(DateTime start, DateTime end);
    }
}
