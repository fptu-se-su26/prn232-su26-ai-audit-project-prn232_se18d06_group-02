using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories;

public sealed class AiKnowledgeRepository : Repository<AiKnowledgeArticle, Guid>, IAiKnowledgeRepository
{
    public AiKnowledgeRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResult<AiKnowledgeArticle>> SearchAsync(
        AiKnowledgeQueryDto query,
        CancellationToken ct = default)
    {
        var source = _dbSet.AsNoTracking().AsQueryable();

        if (query.Status.HasValue)
        {
            source = source.Where(x => x.Status == query.Status.Value);
        }

        if (query.Category.HasValue)
        {
            source = source.Where(x => x.Category == query.Category.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            source = source.Where(x =>
                x.Title.Contains(term) ||
                x.Keywords.Contains(term) ||
                x.Content.Contains(term));
        }

        var count = await source.CountAsync(ct);
        var items = await source
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return new PagedResult<AiKnowledgeArticle>(
            items,
            count,
            query.PageNumber,
            query.PageSize);
    }

    public async Task<List<AiKnowledgeArticle>> SearchPublishedAsync(
        string query,
        string? category,
        int take,
        CancellationToken ct = default)
    {
        var source = _dbSet
            .AsNoTracking()
            .Where(x => x.Status == AiKnowledgeStatus.Published);

        if (Enum.TryParse<AiKnowledgeCategory>(category, true, out var parsedCategory))
        {
            source = source.Where(x => x.Category == parsedCategory);
        }

        var candidates = await source
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(100)
            .ToListAsync(ct);

        var tokens = query
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToArray();

        return candidates
            .Select(article => new
            {
                Article = article,
                Score = Score(article, query, tokens)
            })
            .Where(x => x.Score > 0 || tokens.Length == 0)
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Article.UpdatedAtUtc)
            .Take(Math.Clamp(take, 1, 5))
            .Select(x => x.Article)
            .ToList();
    }

    public Task<AiKnowledgeArticle?> GetBySlugAsync(
        string slug,
        CancellationToken ct = default) =>
        _dbSet.FirstOrDefaultAsync(x => x.Slug == slug, ct);

    public async Task<Dictionary<string, int>> GetStatusCountsAsync(
        CancellationToken ct = default)
    {
        var counts = await _dbSet
            .AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(x => new { Status = x.Key, Count = x.Count() })
            .ToListAsync(ct);

        return counts.ToDictionary(x => x.Status.ToString(), x => x.Count);
    }

    private static int Score(
        AiKnowledgeArticle article,
        string rawQuery,
        IReadOnlyCollection<string> tokens)
    {
        var score = 0;
        if (article.Title.Contains(rawQuery, StringComparison.OrdinalIgnoreCase)) score += 20;
        if (article.Keywords.Contains(rawQuery, StringComparison.OrdinalIgnoreCase)) score += 12;

        foreach (var token in tokens)
        {
            if (article.Title.Contains(token, StringComparison.OrdinalIgnoreCase)) score += 6;
            if (article.Keywords.Contains(token, StringComparison.OrdinalIgnoreCase)) score += 4;
            if (article.Content.Contains(token, StringComparison.OrdinalIgnoreCase)) score += 1;
        }

        return score;
    }
}
