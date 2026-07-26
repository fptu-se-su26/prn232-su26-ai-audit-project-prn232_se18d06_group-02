using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.AiChat;

public sealed class AiKnowledgeService : IAiKnowledgeService
{
    private readonly IAiKnowledgeRepository _articles;
    private readonly IUnitOfWork _unitOfWork;

    public AiKnowledgeService(
        IAiKnowledgeRepository articles,
        IUnitOfWork unitOfWork)
    {
        _articles = articles;
        _unitOfWork = unitOfWork;
    }

    public async Task<AiKnowledgeListDto> SearchAsync(
        AiKnowledgeQueryDto query,
        CancellationToken ct = default)
    {
        query.PageNumber = Math.Max(1, query.PageNumber);
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        var result = await _articles.SearchAsync(query, ct);
        return new AiKnowledgeListDto
        {
            Articles = new Application.Common.Models.PagedResult<AiKnowledgeArticleDto>(
                result.Items.Select(Map).ToList(),
                result.TotalCount,
                result.PageNumber,
                result.PageSize),
            StatusCounts = await _articles.GetStatusCountsAsync(ct)
        };
    }

    public async Task<AiKnowledgeArticleDto?> GetAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var article = await _articles.GetByIdAsync(id, ct);
        return article is null ? null : Map(article);
    }

    public async Task<AiKnowledgeArticleDto> CreateAsync(
        SaveAiKnowledgeArticleDto input,
        string userId,
        CancellationToken ct = default)
    {
        await EnsureSlugAvailableAsync(input.Slug, null, ct);
        var now = DateTime.UtcNow;
        var article = new AiKnowledgeArticle
        {
            Id = Guid.NewGuid(),
            Title = input.Title.Trim(),
            Slug = NormalizeSlug(input.Slug),
            Category = input.Category,
            Keywords = input.Keywords.Trim(),
            Content = input.Content.Trim(),
            Status = AiKnowledgeStatus.Draft,
            CreatedByUserId = userId,
            UpdatedByUserId = userId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await _articles.AddAsync(article, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return Map(article);
    }

    public async Task<AiKnowledgeArticleDto?> UpdateAsync(
        Guid id,
        SaveAiKnowledgeArticleDto input,
        string userId,
        CancellationToken ct = default)
    {
        var article = await _articles.GetByIdAsync(id, ct);
        if (article is null) return null;

        await EnsureSlugAvailableAsync(input.Slug, id, ct);
        article.Title = input.Title.Trim();
        article.Slug = NormalizeSlug(input.Slug);
        article.Category = input.Category;
        article.Keywords = input.Keywords.Trim();
        article.Content = input.Content.Trim();
        article.UpdatedByUserId = userId;
        article.UpdatedAtUtc = DateTime.UtcNow;

        await _articles.UpdateAsync(article);
        await _unitOfWork.SaveChangesAsync(ct);
        return Map(article);
    }

    public async Task<bool> PublishAsync(
        Guid id,
        string userId,
        CancellationToken ct = default)
    {
        var article = await _articles.GetByIdAsync(id, ct);
        if (article is null || string.IsNullOrWhiteSpace(article.Content)) return false;

        article.Status = AiKnowledgeStatus.Published;
        article.PublishedAtUtc = DateTime.UtcNow;
        article.UpdatedAtUtc = DateTime.UtcNow;
        article.UpdatedByUserId = userId;
        await _articles.UpdateAsync(article);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ArchiveAsync(
        Guid id,
        string userId,
        CancellationToken ct = default)
    {
        var article = await _articles.GetByIdAsync(id, ct);
        if (article is null) return false;

        article.Status = AiKnowledgeStatus.Archived;
        article.UpdatedAtUtc = DateTime.UtcNow;
        article.UpdatedByUserId = userId;
        await _articles.UpdateAsync(article);
        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    private async Task EnsureSlugAvailableAsync(
        string slug,
        Guid? currentId,
        CancellationToken ct)
    {
        var existing = await _articles.GetBySlugAsync(NormalizeSlug(slug), ct);
        if (existing is not null && existing.Id != currentId)
        {
            throw new InvalidOperationException("An AI knowledge article with this slug already exists.");
        }
    }

    private static string NormalizeSlug(string slug) =>
        slug.Trim().ToLowerInvariant();

    private static AiKnowledgeArticleDto Map(AiKnowledgeArticle article) => new()
    {
        Id = article.Id,
        Title = article.Title,
        Slug = article.Slug,
        Category = article.Category,
        Keywords = article.Keywords,
        Content = article.Content,
        Status = article.Status,
        CreatedAtUtc = article.CreatedAtUtc,
        UpdatedAtUtc = article.UpdatedAtUtc,
        PublishedAtUtc = article.PublishedAtUtc
    };
}
