using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminPlatformService
    {
        Task<PagedResult<PlatformTransactionDto>> GetTransactionsAsync(
            PlatformTransactionQuery query,
            CancellationToken ct = default);

        Task<AdminPlatformTransactionSummaryDto> GetTransactionSummaryAsync(
            PlatformTransactionQuery query,
            CancellationToken ct = default);
    }
}
