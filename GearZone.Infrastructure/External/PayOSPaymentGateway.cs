using GearZone.Application.Abstractions.External;
using GearZone.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayOS;

namespace GearZone.Infrastructure.External
{
    public class PayOSPaymentGateway : IPaymentGateway
    {
        private readonly PayOSClient? _client;
        private readonly string? _initError;
        private readonly ILogger<PayOSPaymentGateway> _logger;

        public PayOSPaymentGateway(
            IOptions<PayOSSettings> settings,
            ILogger<PayOSPaymentGateway> logger)
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
                _logger.LogError(ex, "Could not initialize PayOS payment gateway client.");
            }
        }

        public async Task<PaymentGatewayResult> GetPaymentStatusAsync(long orderCode)
        {
            if (_client == null)
            {
                return PaymentGatewayResult.Error(_initError ?? "PayOS client is not initialized.");
            }

            try
            {
                var paymentInfo = await _client.PaymentRequests.GetAsync(orderCode);

                if (paymentInfo == null)
                    return PaymentGatewayResult.Error("Could not retrieve payment info from PayOS.");

                var statusStr = paymentInfo.Status.ToString().ToUpperInvariant();

                _logger.LogInformation(
                    "PayOS payment status for order {OrderCode}: {Status}",
                    orderCode, statusStr);

                // PayOS PaymentLinkStatus enum: compare using string representation
                if (statusStr == "PAID")
                {
                    return PaymentGatewayResult.Paid(paymentInfo.Id.ToString());
                }

                return PaymentGatewayResult.NotPaid(statusStr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying PayOS for order {OrderCode}", orderCode);
                return PaymentGatewayResult.Error(ex.Message);
            }
        }
    }
}
