using GearZone.Application.Abstractions.Persistence;
using System;
using Microsoft.EntityFrameworkCore;

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

        public void ClearTrackedEntities()
            => _dbContext.ChangeTracker.Clear();

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> action,
            CancellationToken ct = default)
        {
            if (_dbContext.Database.CurrentTransaction != null)
            {
                return await action(ct);
            }

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
                try
                {
                    var result = await action(ct);
                    await transaction.CommitAsync(ct);
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync(ct);
                    throw;
                }
            });
        }
    }
}
