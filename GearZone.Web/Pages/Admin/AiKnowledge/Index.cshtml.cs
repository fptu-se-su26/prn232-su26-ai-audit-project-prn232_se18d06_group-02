using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.AiKnowledge;

[Authorize(Roles = "Super Admin,Admin")]
public sealed class IndexModel : PageModel
{
    private readonly IApiClient _api;

    public IndexModel(IApiClient api)
    {
        _api = api;
    }

    [BindProperty(SupportsGet = true)]
    public AiKnowledgeQueryDto Query { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid? EditId { get; set; }

    [BindProperty]
    public SaveAiKnowledgeArticleDto Input { get; set; } = new();

    public AiKnowledgeListDto Data { get; private set; } = new();
    public AiKnowledgeArticleDto? EditingArticle { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(ct);
    }

    public async Task<IActionResult> OnPostSaveAsync(
        Guid? id,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            EditId = id;
            await LoadAsync(ct, keepBoundInput: true);
            return Page();
        }

        var result = id.HasValue
            ? await _api.PutAsync($"/api/admin/ai-knowledge/{id}", Input, ct)
            : await _api.PostAsync("/api/admin/ai-knowledge", Input, ct);

        SetToast(
            result.Success,
            result.Success
                ? (id.HasValue ? "Knowledge article updated." : "Knowledge article created as draft.")
                : result.FirstError ?? "Unable to save the knowledge article.");

        return RedirectToPage(new { editId = id });
    }

    public async Task<IActionResult> OnPostPublishAsync(
        Guid id,
        CancellationToken ct)
    {
        var result = await _api.PostAsync($"/api/admin/ai-knowledge/{id}/publish", ct);
        SetToast(
            result.Success,
            result.Success
                ? "Knowledge article published to GearZone AI."
                : result.FirstError ?? "Unable to publish the article.");
        return RedirectToPage(new { editId = id });
    }

    public async Task<IActionResult> OnPostArchiveAsync(
        Guid id,
        CancellationToken ct)
    {
        var result = await _api.PostAsync($"/api/admin/ai-knowledge/{id}/archive", ct);
        SetToast(
            result.Success,
            result.Success
                ? "Knowledge article archived."
                : result.FirstError ?? "Unable to archive the article.");
        return RedirectToPage();
    }

    private async Task LoadAsync(
        CancellationToken ct,
        bool keepBoundInput = false)
    {
        Query.PageNumber = Math.Max(1, Query.PageNumber);
        Query.PageSize = Query.PageSize is < 1 or > 100 ? 20 : Query.PageSize;
        Data = await _api.GetAsync<AiKnowledgeListDto>(
            $"/api/admin/ai-knowledge{ApiQueryStringBuilder.Build(Query)}",
            ct) ?? new AiKnowledgeListDto();

        if (!EditId.HasValue) return;
        EditingArticle = await _api.GetAsync<AiKnowledgeArticleDto>(
            $"/api/admin/ai-knowledge/{EditId}",
            ct);
        if (EditingArticle is not null && !keepBoundInput)
        {
            Input = new SaveAiKnowledgeArticleDto
            {
                Title = EditingArticle.Title,
                Slug = EditingArticle.Slug,
                Category = EditingArticle.Category,
                Keywords = EditingArticle.Keywords,
                Content = EditingArticle.Content
            };
        }
    }

    private void SetToast(bool success, string message)
    {
        TempData["ToastType"] = success ? "success" : "error";
        TempData["ToastMessage"] = message;
    }
}
