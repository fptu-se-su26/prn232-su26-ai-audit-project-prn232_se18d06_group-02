using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Payout.Dtos;
using System.Collections.Generic;

namespace GearZone.Infrastructure.External
{
    public class DisabledPayoutClient : IPayoutClient
    {
        public Task<PayoutResult> CreatePayoutAsync(PayoutRequestDto payout)
        {
            return Task.FromResult(new PayoutResult(
                isSuccess: false,
                errorMessage: "PayOS payout is not configured."));
        }

        public Task<PayoutResult> CreateBatchPayoutAsync(List<PayoutRequestDto> payouts)
        {
            return Task.FromResult(new PayoutResult(
                isSuccess: false,
                errorMessage: "PayOS payout is not configured."));
        }

        public Task<PayoutAccountInfoDto> GetAccountBalance()
        {
            return Task.FromResult(new PayoutAccountInfoDto
            {
                AccountName = "Unavailable",
                AccountNumber = string.Empty,
                Balance = "0",
                Currency = string.Empty
            });
        }
    }
}
