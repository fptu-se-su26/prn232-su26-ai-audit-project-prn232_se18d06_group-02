using GearZone.Application.Abstractions.Persistence;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;

        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _dbContext.SaveChangesAsync(ct);
    }
}
