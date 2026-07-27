using GearZone.Application.Features.Promotions.Dtos;
using GearZone.Domain.Enums;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Promotions;

[Authorize(Roles = "Store Owner")]
public class IndexModel : PageModel
{
    private readonly IApiClient _api;

    public IndexModel(IApiClient api)
    {
        _api = api;
    }

    public SellerPromotionListDto Data { get; private set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public PromotionStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken ct)
    {
        PageNumber = Math.Max(1, PageNumber);
        var query = new List<string>
        {
            $"pageNumber={PageNumber}",
            "pageSize=12"
        };
        if (!string.IsNullOrWhiteSpace(Search))
            query.Add($"search={Uri.EscapeDataString(Search)}");
        if (Status.HasValue)
            query.Add($"status={Status.Value}");

        Data = await _api.GetAsync<SellerPromotionListDto>(
            $"/api/seller/promotions?{string.Join("&", query)}",
            ct) ?? new SellerPromotionListDto();
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid id, CancellationToken ct)
    {
        var result = await _api.PatchAsync(
            $"/api/seller/promotions/{id}/toggle-status",
            ct);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success
                ? "Promotion status updated."
                : result.FirstError ?? "Unable to update promotion status.";
        return RedirectToPage(new
        {
            Search,
            Status,
            PageNumber
        });
    }

    public static string StatusClasses(PromotionStatus status) => status switch
    {
        PromotionStatus.Active => "bg-emerald-100 text-emerald-700",
        PromotionStatus.Upcoming => "bg-blue-100 text-blue-700",
        PromotionStatus.Paused => "bg-amber-100 text-amber-700",
        PromotionStatus.Exhausted => "bg-rose-100 text-rose-700",
        PromotionStatus.Expired => "bg-slate-100 text-slate-600",
        _ => "bg-slate-100 text-slate-600"
    };
}
