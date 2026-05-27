using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.External;
using GearZone.Domain.Enums;
using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Jobs
{
    public class OrderAutoCompleteJob
    {
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly ISystemSettingRepository _systemSettingRepository;
        private readonly IOrderTrackingNotifier _orderTrackingNotifier;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderAutoCompleteJob> _logger;

        public OrderAutoCompleteJob(
            ISubOrderRepository subOrderRepository,
            ISystemSettingRepository systemSettingRepository,
            IOrderTrackingNotifier orderTrackingNotifier,
            IUnitOfWork unitOfWork,
            ILogger<OrderAutoCompleteJob> logger)
        {
            _subOrderRepository = subOrderRepository;
            _systemSettingRepository = systemSettingRepository;
            _orderTrackingNotifier = orderTrackingNotifier;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 300, 600 })]
        [DisplayName("Auto Complete Delivered Orders")]
        public async Task AutoCompleteOrdersAsync()
        {
            _logger.LogInformation("[Job] AutoCompleteOrders started");

            // 1. Get AutoCompleteDays from settings
            var setting = await _systemSettingRepository.GetByKeyAsync("Order_AutoCompleteDays");
            int autoCompleteDays = 7; // Default

            if (setting != null && int.TryParse(setting.Value, out int days))
            {
                autoCompleteDays = days;
            }

            _logger.LogInformation("[Job] AutoComplete threshold: {Days} days", autoCompleteDays);

            // 2. Fetch eligible sub-orders
            var eligibleOrders = await _subOrderRepository.GetDeliveredOrdersForAutoCompleteAsync(autoCompleteDays);

            if (eligibleOrders.Count == 0)
            {
                _logger.LogInformation("[Job] No orders eligible for auto-completion.");
                return;
            }

            _logger.LogInformation("[Job] Found {Count} orders to auto-complete", eligibleOrders.Count);

            // 3. Update status
            foreach (var subOrder in eligibleOrders)
            {
                subOrder.Status = OrderStatus.Completed;
                subOrder.UpdatedAt = DateTime.UtcNow;
                _logger.LogInformation("[Job] Auto-completing SubOrder {Id}", subOrder.Id);
            }

            // 4. Save changes
            await _unitOfWork.SaveChangesAsync();

            foreach (var subOrder in eligibleOrders)
            {
                await _orderTrackingNotifier.NotifySubOrderUpdatedAsync(subOrder.Id);
            }

            _logger.LogInformation("[Job] AutoCompleteOrders finished. {Count} orders processed.", eligibleOrders.Count);
        }
    }
}
