using System;

namespace GearZone.Application.Abstractions.Services
{
    public interface IBackgroundJobService
    {
        string SchedulePaymentTimeout(Guid orderId, TimeSpan delay);
        string EnqueueOrderCancellation(Guid orderId, string? userId = null);
    }
}
