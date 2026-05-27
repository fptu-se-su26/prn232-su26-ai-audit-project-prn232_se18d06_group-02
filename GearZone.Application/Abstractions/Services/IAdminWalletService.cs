using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminWalletService
    {
        Task<WalletSummaryDto> GetWalletSummaryAsync(CancellationToken ct = default);

        Task<PagedResult<WalletTransactionDto>> GetTransactionsAsync(
            WalletTransactionQuery query,
            CancellationToken ct = default);

        Task<List<WalletTransactionDto>> GetBalanceHistoryAsync(int days = 30, CancellationToken ct = default);
        Task<List<WalletTransactionDto>> GetCashFlowHistoryAsync(int recentMonths = 6, CancellationToken ct = default);

        Task RecordTopupAsync(
            TopupWalletDto dto,
            string adminId,
            CancellationToken ct = default);
    }
}
