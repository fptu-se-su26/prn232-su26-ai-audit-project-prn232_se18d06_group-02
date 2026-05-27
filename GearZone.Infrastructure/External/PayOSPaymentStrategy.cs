using GearZone.Application.Abstractions.External;
using GearZone.Application.Features.Payment.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using GearZone.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PayOS;
using PayOS.Models.V2.PaymentRequests;

namespace GearZone.Infrastructure.External
{
    public class PayOSPaymentStrategy : IPaymentStrategy
    {
        private readonly PayOSClient? _client;
        private readonly string? _initError;
        private readonly PayOSSettings _settings;
        private readonly ILogger<PayOSPaymentStrategy> _logger;

        public PayOSPaymentStrategy(
            IOptions<PayOSSettings> settings,
            ILogger<PayOSPaymentStrategy> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            try
            {
                _client = PayOSClientFactory.Create(
                    _settings.ClientId,
                    _settings.ApiKey,
                    _settings.ChecksumKey);
            }
            catch (Exception ex)
            {
                _initError = ex.Message;
                _logger.LogError(ex, "Could not initialize PayOS payment client.");
            }
        }

        public PaymentMethod Method => PaymentMethod.PayOS;

        public async Task<PaymentResult> ProcessPaymentAsync(Order order)
        {
            try
            {
                if (_client == null)
                {
                    return new PaymentResult(
                        success: false,
                        checkoutUrl: null,
                        errorMessage: _initError ?? "PayOS client is not initialized.");
                }

                if (order == null)
                    throw new ArgumentNullException(nameof(order));

                var allItems = order.SubOrders.SelectMany(s => s.Items).ToList();
                if (!allItems.Any())
                    throw new Exception("Order must contain items");

                var paymentRequest = new CreatePaymentLinkRequest
                {
                    OrderCode = order.OrderCode,
                    Amount = (long)order.GrandTotal,
                    Description = $"GZ {order.OrderCode}",

                    ReturnUrl = _settings.ReturnUrl,
                    CancelUrl = _settings.CancelUrl,
                    
                    ExpiredAt = (int)DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),

                    BuyerName = order.ReceiverName,
                    BuyerEmail = order.User?.Email,
                    BuyerPhone = order.ReceiverPhone,
                    BuyerAddress = order.ShippingAddress,

                    Items = allItems.Select(i => new PaymentLinkItem
                    {
                        Name = i.ProductNameSnapshot,
                        Quantity = i.Quantity,
                        Price = (int)i.UnitPriceSnapshot
                    }).ToList()
                };

                var response = await _client.PaymentRequests.CreateAsync(paymentRequest);

                // Create Payment record immediately
                var payment = new Domain.Entities.Payment
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PaymentCode = order.OrderCode.ToString(),
                    Method = PaymentMethod.PayOS,
                    Provider = "PayOS",
                    Amount = order.GrandTotal,
                    Status = PaymentStatus.Pending,
                    PaymentLinkId = response.PaymentLinkId,
                    CheckoutUrl = response.CheckoutUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                order.Payments.Add(payment);

                return new PaymentResult(
                    success: true,
                    checkoutUrl: response.CheckoutUrl,
                    paymentLinkId: response.PaymentLinkId
                )
                {
                    Bin = response.Bin,
                    AccountNumber = response.AccountNumber,
                    AccountName = response.AccountName,
                    Amount = response.Amount,
                    Description = response.Description,
                    QrCode = response.QrCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PayOS payment creation failed for order {OrderCode}", order?.OrderCode);

                return new PaymentResult(
                    success: false,
                    checkoutUrl: null,
                    errorMessage: ex.Message
                );
            }
        }
    }
}
