using GearZone.Api.Auditing;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/ai-knowledge")]
[Authorize(Roles = "Super Admin,Admin")]
public sealed class AiKnowledgeController : BaseApiController
{
    private readonly IAiKnowledgeService _knowledge;

    public AiKnowledgeController(IAiKnowledgeService knowledge)
    {
        _knowledge = knowledge;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] AiKnowledgeQueryDto query,
        CancellationToken ct)
    {
        return OkResponse(await _knowledge.SearchAsync(query, ct));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var article = await _knowledge.GetAsync(id, ct);
        return article is null
            ? FailResponse("AI knowledge article not found.", 404)
            : OkResponse(article);
    }

    [HttpPost]
    [AdminAuditAction(
        AdminAuditActions.AiKnowledgeCreated,
        AdminAuditModules.AiKnowledge,
        AdminAuditRiskLevel.Medium,
        EntityType = "AiKnowledgeArticle")]
    public async Task<IActionResult> Create(
        [FromBody] SaveAiKnowledgeArticleDto input,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();
        try
        {
            var article = await _knowledge.CreateAsync(input, CurrentUserId!, ct);
            return CreatedResponse(
                article,
                $"/api/admin/ai-knowledge/{article.Id}",
                "AI knowledge article created.");
        }
        catch (InvalidOperationException ex)
        {
            return FailResponse(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [AdminAuditAction(
        AdminAuditActions.AiKnowledgeUpdated,
        AdminAuditModules.AiKnowledge,
        AdminAuditRiskLevel.Medium,
        EntityType = "AiKnowledgeArticle")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] SaveAiKnowledgeArticleDto input,
        CancellationToken ct)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();
        try
        {
            var article = await _knowledge.UpdateAsync(id, input, CurrentUserId!, ct);
            return article is null
                ? FailResponse("AI knowledge article not found.", 404)
                : OkResponse(article, "AI knowledge article updated.");
        }
        catch (InvalidOperationException ex)
        {
            return FailResponse(ex.Message);
        }
    }

    [HttpPost("{id:guid}/publish")]
    [AdminAuditAction(
        AdminAuditActions.AiKnowledgePublished,
        AdminAuditModules.AiKnowledge,
        AdminAuditRiskLevel.Medium,
        EntityType = "AiKnowledgeArticle")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        return await _knowledge.PublishAsync(id, CurrentUserId!, ct)
            ? OkResponse("AI knowledge article published.")
            : FailResponse("AI knowledge article not found or cannot be published.", 404);
    }

    [HttpPost("{id:guid}/archive")]
    [AdminAuditAction(
        AdminAuditActions.AiKnowledgeArchived,
        AdminAuditModules.AiKnowledge,
        AdminAuditRiskLevel.Medium,
        EntityType = "AiKnowledgeArticle")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken ct)
    {
        return await _knowledge.ArchiveAsync(id, CurrentUserId!, ct)
            ? OkResponse("AI knowledge article archived.")
            : FailResponse("AI knowledge article not found.", 404);
    }
}
