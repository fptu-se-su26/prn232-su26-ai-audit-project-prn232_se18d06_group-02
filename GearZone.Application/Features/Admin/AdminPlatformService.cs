using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Admin
{
    public class AdminPlatformService : IAdminPlatformService
    {
        private readonly IPlatformTransactionRepository _platformTransactionRepository;
        private readonly IPayoutClient _payoutClient;

        public AdminPlatformService(
            IPlatformTransactionRepository platformTransactionRepository,
            IPayoutClient payoutClient)
        {
            _platformTransactionRepository = platformTransactionRepository;
            _payoutClient = payoutClient;
        }

        public async Task<PagedResult<PlatformTransactionDto>> GetTransactionsAsync(
            PlatformTransactionQuery query,
            CancellationToken ct = default)
        {
            return await _platformTransactionRepository.GetPagedTransactionsAsync(query, ct);
        }

        public async Task<AdminPlatformTransactionSummaryDto> GetTransactionSummaryAsync(
            PlatformTransactionQuery query,
            CancellationToken ct = default)
        {
            var summary = await _platformTransactionRepository.GetSummaryAsync(query, ct);
            
            try 
            {
                var accountInfo = await _payoutClient.GetAccountBalance();
                if (decimal.TryParse(accountInfo.Balance, out var balance))
                {
                    summary.WalletBalance = balance;
                }
            }
            catch 
            {
                // Fallback or leave as 0 if external service unavailable
            }

            return summary;
        }
    }
}
