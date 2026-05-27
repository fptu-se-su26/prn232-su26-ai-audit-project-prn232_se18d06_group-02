using GearZone.Domain.Entities;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IRepository<T, TKey> where T : Entity<TKey>
    {
        Task<T> AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> items, CancellationToken ct = default);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task<T?> GetByIdAsync(TKey id, CancellationToken ct = default, params Expression<Func<T, object>>[] includes);
        IQueryable<T> Query();
    }
}
