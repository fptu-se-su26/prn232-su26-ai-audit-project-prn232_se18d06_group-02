using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories;

public sealed class AiConversationRepository : Repository<AiConversation, Guid>, IAiConversationRepository
{
    public AiConversationRepository(ApplicationDbContext context) : base(context) { }

    public Task<AiConversation?> GetOwnedAsync(
        Guid id,
        AiChatActor actor,
        CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (actor.IsCustomer)
        {
            return query.FirstOrDefaultAsync(
                x => x.Id == id && x.CustomerUserId == actor.CustomerUserId,
                ct);
        }

        if (!string.IsNullOrWhiteSpace(actor.GuestTokenHash))
        {
            return query.FirstOrDefaultAsync(
                x => x.Id == id && x.CustomerUserId == null &&
                     x.GuestTokenHash == actor.GuestTokenHash,
                ct);
        }

        return Task.FromResult<AiConversation?>(null);
    }

    public async Task<PagedResult<AiConversation>> GetForCustomerAsync(
        string customerUserId,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(x => x.CustomerUserId == customerUserId)
            .OrderByDescending(x => x.LastActivityAtUtc);

        var count = await query.CountAsync(ct);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<AiConversation>(items, count, pageNumber, pageSize);
    }

    public Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken ct = default) =>
        _dbSet.Where(x => x.ExpiresAtUtc <= utcNow).ExecuteDeleteAsync(ct);
}
