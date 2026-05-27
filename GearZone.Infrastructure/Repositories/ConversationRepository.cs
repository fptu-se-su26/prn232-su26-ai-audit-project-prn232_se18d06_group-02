using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class ConversationRepository : Repository<Conversation, Guid>, IConversationRepository
    {
        public ConversationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Conversation?> GetByBuyerAndStoreAsync(string buyerUserId, Guid storeId, CancellationToken ct = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.BuyerUserId == buyerUserId && c.StoreId == storeId, ct);
        }

        public async Task<Conversation?> GetByIdWithParticipantsAsync(Guid conversationId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(x => x.BuyerUser)
                .Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == conversationId, ct);
        }

        public async Task<PagedResult<ChatConversationListItemDto>> GetBuyerInboxAsync(string buyerUserId, ChatInboxQueryDto query, CancellationToken ct = default)
        {
            var conversationQuery = BuildBuyerInboxQuery(buyerUserId, query);
            var totalCount = await conversationQuery.CountAsync(ct);
            var items = await conversationQuery
                .OrderByDescending(x => x.LastMessageAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(MapBuyerConversation(buyerUserId))
                .ToListAsync(ct);

            return new PagedResult<ChatConversationListItemDto>(items, totalCount, query.PageNumber, query.PageSize);
        }

        public async Task<PagedResult<ChatConversationListItemDto>> GetSellerInboxAsync(string ownerUserId, ChatInboxQueryDto query, CancellationToken ct = default)
        {
            var conversationQuery = BuildSellerInboxQuery(ownerUserId, query);
            var totalCount = await conversationQuery.CountAsync(ct);
            var items = await conversationQuery
                .OrderByDescending(x => x.LastMessageAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(MapSellerConversation(ownerUserId))
                .ToListAsync(ct);

            return new PagedResult<ChatConversationListItemDto>(items, totalCount, query.PageNumber, query.PageSize);
        }

        public async Task<List<ChatCounterpartScopeOptionDto>> GetBuyerCounterpartScopeOptionsAsync(string buyerUserId, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.BuyerUserId == buyerUserId)
                .OrderBy(x => x.Store.StoreName)
                .Select(x => new ChatCounterpartScopeOptionDto
                {
                    Value = x.Store.Slug,
                    Label = x.Store.StoreName,
                    Subtitle = x.Store.Slug,
                    AvatarUrl = x.Store.LogoUrl
                })
                .ToListAsync(ct);
        }

        public async Task<List<ChatCounterpartScopeOptionDto>> GetSellerCounterpartScopeOptionsAsync(string ownerUserId, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.Store.OwnerUserId == ownerUserId)
                .OrderBy(x => x.BuyerUser.FullName ?? x.BuyerUser.UserName ?? x.BuyerUser.Email)
                .Select(x => new ChatCounterpartScopeOptionDto
                {
                    Value = x.BuyerUserId,
                    Label = x.BuyerUser.FullName ?? x.BuyerUser.UserName ?? x.BuyerUser.Email ?? "Buyer",
                    Subtitle = x.BuyerUser.Email,
                    AvatarUrl = x.BuyerUser.AvatarUrl
                })
                .ToListAsync(ct);
        }

        public async Task<ChatConversationListItemDto?> GetBuyerConversationListItemAsync(string buyerUserId, Guid conversationId, CancellationToken ct = default)
        {
            return await BuildBuyerInboxQuery(buyerUserId, new ChatInboxQueryDto())
                .Where(x => x.Id == conversationId)
                .Select(MapBuyerConversation(buyerUserId))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<ChatConversationListItemDto?> GetSellerConversationListItemAsync(string ownerUserId, Guid conversationId, CancellationToken ct = default)
        {
            return await BuildSellerInboxQuery(ownerUserId, new ChatInboxQueryDto())
                .Where(x => x.Id == conversationId)
                .Select(MapSellerConversation(ownerUserId))
                .FirstOrDefaultAsync(ct);
        }

        public async Task<int> GetBuyerUnreadCountAsync(string buyerUserId, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.BuyerUserId == buyerUserId)
                .SelectMany(x => x.Messages)
                .CountAsync(x => x.SenderUserId != buyerUserId && !x.IsRead, ct);
        }

        public async Task<int> GetSellerUnreadCountAsync(string ownerUserId, CancellationToken ct = default)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(x => x.Store.OwnerUserId == ownerUserId)
                .SelectMany(x => x.Messages)
                .CountAsync(x => x.SenderUserId != ownerUserId && !x.IsRead, ct);
        }

        private IQueryable<Conversation> BuildBuyerInboxQuery(string buyerUserId, ChatInboxQueryDto query)
        {
            var conversationQuery = _dbSet
                .AsNoTracking()
                .Where(x => x.BuyerUserId == buyerUserId);

            if (query.Filter == "unread")
            {
                conversationQuery = conversationQuery.Where(x => x.Messages.Any(m => m.SenderUserId != buyerUserId && !m.IsRead));
            }

            if (!string.IsNullOrWhiteSpace(query.CounterpartScopeKey))
            {
                var storeSlug = query.CounterpartScopeKey.Trim().ToLower();
                conversationQuery = conversationQuery.Where(x => x.Store.Slug.ToLower() == storeSlug);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();
                conversationQuery = conversationQuery.Where(x =>
                    x.Store.StoreName.ToLower().Contains(search) ||
                    x.Store.Slug.ToLower().Contains(search));
            }

            return conversationQuery;
        }

        private IQueryable<Conversation> BuildSellerInboxQuery(string ownerUserId, ChatInboxQueryDto query)
        {
            var conversationQuery = _dbSet
                .AsNoTracking()
                .Where(x => x.Store.OwnerUserId == ownerUserId);

            if (query.Filter == "unread")
            {
                conversationQuery = conversationQuery.Where(x => x.Messages.Any(m => m.SenderUserId != ownerUserId && !m.IsRead));
            }

            if (!string.IsNullOrWhiteSpace(query.CounterpartScopeKey))
            {
                var buyerUserId = query.CounterpartScopeKey.Trim();
                conversationQuery = conversationQuery.Where(x => x.BuyerUserId == buyerUserId);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.Trim().ToLower();
                conversationQuery = conversationQuery.Where(x =>
                    (x.BuyerUser.FullName != null && x.BuyerUser.FullName.ToLower().Contains(search)) ||
                    (x.BuyerUser.UserName != null && x.BuyerUser.UserName.ToLower().Contains(search)) ||
                    (x.BuyerUser.Email != null && x.BuyerUser.Email.ToLower().Contains(search)));
            }

            return conversationQuery;
        }

        private static System.Linq.Expressions.Expression<Func<Conversation, ChatConversationListItemDto>> MapBuyerConversation(string buyerUserId)
        {
            return x => new ChatConversationListItemDto
            {
                ConversationId = x.Id,
                StoreId = x.StoreId,
                StoreName = x.Store.StoreName,
                StoreSlug = x.Store.Slug,
                StoreLogoUrl = x.Store.LogoUrl,
                BuyerUserId = x.BuyerUserId,
                BuyerDisplayName = x.BuyerUser.FullName ?? x.BuyerUser.UserName ?? x.BuyerUser.Email ?? "Buyer",
                BuyerAvatarUrl = x.BuyerUser.AvatarUrl,
                CounterpartName = x.Store.StoreName,
                CounterpartAvatarUrl = x.Store.LogoUrl,
                CounterpartSubtitle = "Shop",
                LastMessagePreview = x.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault() ?? "Start chatting with this shop",
                LastMessageSenderUserId = x.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SenderUserId)
                    .FirstOrDefault() ?? string.Empty,
                LastMessageAt = x.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => (DateTime?)m.SentAt)
                    .FirstOrDefault() ?? x.LastMessageAt,
                UnreadCount = x.Messages.Count(m => m.SenderUserId != buyerUserId && !m.IsRead),
                HasMessages = x.Messages.Any()
            };
        }

        private static System.Linq.Expressions.Expression<Func<Conversation, ChatConversationListItemDto>> MapSellerConversation(string ownerUserId)
        {
            return x => new ChatConversationListItemDto
            {
                ConversationId = x.Id,
                StoreId = x.StoreId,
                StoreName = x.Store.StoreName,
                StoreSlug = x.Store.Slug,
                StoreLogoUrl = x.Store.LogoUrl,
                BuyerUserId = x.BuyerUserId,
                BuyerDisplayName = x.BuyerUser.FullName ?? x.BuyerUser.UserName ?? x.BuyerUser.Email ?? "Buyer",
                BuyerAvatarUrl = x.BuyerUser.AvatarUrl,
                CounterpartName = x.BuyerUser.FullName ?? x.BuyerUser.UserName ?? x.BuyerUser.Email ?? "Buyer",
                CounterpartAvatarUrl = x.BuyerUser.AvatarUrl,
                CounterpartSubtitle = x.BuyerUser.Email ?? "Buyer",
                LastMessagePreview = x.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.Content)
                    .FirstOrDefault() ?? "Conversation has not started yet",
                LastMessageSenderUserId = x.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => m.SenderUserId)
                    .FirstOrDefault() ?? string.Empty,
                LastMessageAt = x.Messages
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => (DateTime?)m.SentAt)
                    .FirstOrDefault() ?? x.LastMessageAt,
                UnreadCount = x.Messages.Count(m => m.SenderUserId != ownerUserId && !m.IsRead),
                HasMessages = x.Messages.Any()
            };
        }
    }
}
