using GearZone.Application.Features.Payout.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace GearZone.Application.Abstractions.External
{
    public interface IPayoutClient
    {
        Task<PayoutResult> CreatePayoutAsync(PayoutRequestDto payout);
        Task<PayoutResult> CreateBatchPayoutAsync(List<PayoutRequestDto> payouts);
        Task<PayoutAccountInfoDto> GetAccountBalance();
    }
}
