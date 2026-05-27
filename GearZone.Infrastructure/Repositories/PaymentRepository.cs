using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class PaymentRepository : Repository<Payment, Guid>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
