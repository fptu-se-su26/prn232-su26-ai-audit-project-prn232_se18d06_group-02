using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Payout.Dtos;
using GearZone.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V1.Payouts;
using PayOS.Models.V1.Payouts.Batch;
using System.Linq;

namespace GearZone.Infrastructure.External
{
    public class PayOSPayoutClient : IPayoutClient
    {
        private readonly PayOSClient? _client;
        private readonly string? _initError;
        private readonly ILogger<PayOSPayoutClient> _logger;

        public PayOSPayoutClient(
            IOptions<PayOSPayoutSettings> settings,
            ILogger<PayOSPayoutClient> logger)
        {
            _logger = logger;

            try
            {
                var cfg = settings.Value;
                _client = PayOSClientFactory.Create(cfg.ClientId, cfg.ApiKey, cfg.ChecksumKey);
            }
            catch (Exception ex)
            {
                _initError = ex.Message;
                _logger.LogError(ex, "Could not initialize PayOS payout client.");
            }
        }

        public async Task<PayoutResult> CreatePayoutAsync(PayoutRequestDto payout)
        {
            if (_client == null)
            {
                return new PayoutResult(isSuccess: false, errorMessage: _initError ?? "PayOS payout client is not initialized.");
            }

            var request = new PayoutRequest
            {
                ReferenceId = Guid.NewGuid().ToString(),
                Amount = payout.Amount,
                Description = payout.Description,
                ToBin = payout.ToBin,
                ToAccountNumber = payout.ToAccountNumber
            };

            try
            {
                var response = await _client.Payouts.CreateAsync(request);
                var transaction = response.Transactions?.FirstOrDefault();
                var isSucceeded = transaction?.State == PayoutTransactionState.Succeeded;

                return new PayoutResult(
                    isSuccess: isSucceeded,
                    referenceId: transaction?.Id ?? response.ReferenceId,
                    errorMessage: isSucceeded
                        ? null
                        : transaction?.ErrorMessage
                          ?? $"PayOS payout state: {transaction?.State.ToString() ?? "Unknown"}"
                );
            }
            catch (Exception ex)
            {
                return new PayoutResult(
                    isSuccess: false,
                    errorMessage: ex.Message
                );
            }
        }

        public async Task<PayoutResult> CreateBatchPayoutAsync(List<PayoutRequestDto> payouts)
        {
            if (_client == null)
            {
                return new PayoutResult(isSuccess: false, errorMessage: _initError ?? "PayOS payout client is not initialized.");
            }

            var request = new PayoutBatchRequest
            {
                ReferenceId = Guid.NewGuid().ToString(),
                Payouts = payouts.Select(p => new PayoutBatchItem
                {
                    ReferenceId = Guid.NewGuid().ToString(),
                    Amount = p.Amount,
                    Description = p.Description,
                    ToBin = p.ToBin,
                    ToAccountNumber = p.ToAccountNumber
                }).ToList()
            };

            try
            {
                var response = await _client.Payouts.Batch.CreateAsync(request);
                var transactions = response.Transactions ?? new List<PayoutTransaction>();
                var failedTx = transactions.FirstOrDefault(t => t.State != PayoutTransactionState.Succeeded);
                var allSucceeded = transactions.Count > 0 && failedTx == null;

                return new PayoutResult(
                    isSuccess: allSucceeded,
                    referenceId: response.ReferenceId,
                    errorMessage: allSucceeded
                        ? null
                        : failedTx?.ErrorMessage
                          ?? $"PayOS payout batch has non-success transaction state: {failedTx?.State.ToString() ?? "Unknown"}"
                );
            }
            catch (Exception ex)
            {
                return new PayoutResult(
                    isSuccess: false,
                    errorMessage: ex.Message
                );
            }
        }

        public async Task<PayoutAccountInfoDto> GetAccountBalance()
        {
            if (_client == null)
            {
                _logger.LogWarning("PayOS payout client is not initialized: {Error}", _initError);
                return new PayoutAccountInfoDto
                {
                    AccountName = "Unavailable",
                    AccountNumber = string.Empty,
                    Balance = "0",
                    Currency = string.Empty
                };
            }

            try
            {
                var payoutAccount = await _client.PayoutsAccount.GetBalanceAsync();
                
                if (payoutAccount == null)
                {
                    _logger.LogWarning("PayOS GetBalanceAsync returned null.");
                    return null!;
                }

                return new PayoutAccountInfoDto
                {
                    AccountName = payoutAccount.AccountName ?? string.Empty,
                    AccountNumber = payoutAccount.AccountNumber ?? string.Empty,
                    Balance = payoutAccount.Balance.ToString(),
                    Currency = payoutAccount.Currency ?? string.Empty,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting PayOS account balance");
                return null!;
            }
        }
    }
}
