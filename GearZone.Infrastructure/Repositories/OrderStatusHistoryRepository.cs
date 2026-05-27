using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class OrderStatusHistoryRepository : Repository<OrderStatusHistory, Guid>, IOrderStatusHistoryRepository
    {
        public OrderStatusHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
