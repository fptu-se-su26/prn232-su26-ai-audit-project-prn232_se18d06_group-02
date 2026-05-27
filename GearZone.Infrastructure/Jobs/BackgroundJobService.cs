using GearZone.Application.Abstractions.Services;
using Hangfire;
using System;

namespace GearZone.Infrastructure.Jobs
{
    public class BackgroundJobService : IBackgroundJobService
    {
        public string SchedulePaymentTimeout(Guid orderId, TimeSpan delay)
        {
            return BackgroundJob.Schedule<PaymentTimeoutJob>(
                job => job.CancelOrderIfUnpaid(orderId),
                delay
            );
        }

        public string EnqueueOrderCancellation(Guid orderId, string? userId = null)
        {
            return BackgroundJob.Enqueue<PaymentTimeoutJob>(
                job => job.CancelOrderOnRequest(orderId, userId)
            );
        }
    }
}
