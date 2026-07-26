using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories;

public sealed class AiMessageRepository : Repository<AiMessage, Guid>, IAiMessageRepository
{
    public AiMessageRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<AiMessage>> GetPageAsync(
        Guid conversationId,
        DateTime? beforeUtc,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId);

        if (beforeUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc < beforeUtc.Value);
        }

        var page = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(pageSize)
            .ToListAsync(ct);
        page.Reverse();
        return page;
    }

    public async Task<List<AiMessage>> GetRecentCompletedAsync(
        Guid conversationId,
        int take,
        CancellationToken ct = default)
    {
        var messages = await _dbSet
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId &&
                        x.Status == AiMessageStatus.Completed)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync(ct);
        messages.Reverse();
        return messages;
    }

    public Task<AiMessage?> GetByClientIdAsync(
        Guid conversationId,
        Guid clientMessageId,
        string role,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<AiMessageRole>(role, true, out var parsedRole))
        {
            return Task.FromResult<AiMessage?>(null);
        }

        return _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ConversationId == conversationId &&
                     x.ClientMessageId == clientMessageId &&
                     x.Role == parsedRole,
                ct);
    }
}
