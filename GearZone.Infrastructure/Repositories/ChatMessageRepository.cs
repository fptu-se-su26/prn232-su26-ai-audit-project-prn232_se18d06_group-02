using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class ChatMessageRepository : Repository<ChatMessage, Guid>, IChatMessageRepository
    {
        public ChatMessageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<ChatMessageItemDto>> GetRecentMessagesAsync(Guid conversationId, int take, CancellationToken ct = default)
        {
            var items = await _dbSet
                .AsNoTracking()
                .Where(x => x.ConversationId == conversationId)
                .OrderByDescending(x => x.SentAt)
                .Take(take)
                .Select(x => new ChatMessageItemDto
                {
                    Id = x.Id,
                    ConversationId = x.ConversationId,
                    SenderUserId = x.SenderUserId,
                    SenderDisplayName = x.SenderUserId == x.Conversation.Store.OwnerUserId
                        ? x.Conversation.Store.StoreName
                        : (x.SenderUser.FullName ?? x.SenderUser.UserName ?? x.SenderUser.Email ?? "Buyer"),
                    SenderAvatarUrl = x.SenderUserId == x.Conversation.Store.OwnerUserId
                        ? x.Conversation.Store.LogoUrl
                        : x.SenderUser.AvatarUrl,
                    Content = x.Content,
                    SentAt = x.SentAt,
                    IsRead = x.IsRead,
                    ReadAt = x.ReadAt
                })
                .ToListAsync(ct);

            items.Reverse();
            return items;
        }

        public async Task<int> GetMessageCountAsync(Guid conversationId, CancellationToken ct = default)
        {
            return await _dbSet.CountAsync(x => x.ConversationId == conversationId, ct);
        }

        public async Task<int> MarkAsReadAsync(Guid conversationId, string readerUserId, DateTime readAt, CancellationToken ct = default)
        {
            var unreadMessages = await _dbSet
                .Where(x => x.ConversationId == conversationId && x.SenderUserId != readerUserId && !x.IsRead)
                .ToListAsync(ct);

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = readAt;
            }

            return unreadMessages.Count;
        }
    }
}
