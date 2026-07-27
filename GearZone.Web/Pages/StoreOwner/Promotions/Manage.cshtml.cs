using GearZone.Application.Common.Models;
using GearZone.Application.Features.Promotions.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Promotions;

[Authorize(Roles = "Store Owner")]
public class ManageModel : PageModel
{
    private static readonly TimeZoneInfo SellerTimeZone =
        ResolveSellerTimeZone();

    private readonly IApiClient _api;

    public ManageModel(IApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public PromotionCampaignInputDto Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    public List<PromotionProductDto> Products { get; private set; } = new();
    public PromotionCampaignDto? Campaign { get; private set; }
    public bool IsEdit => Id.HasValue;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadProductsAsync(ct);
        if (!Id.HasValue)
        {
            var sellerNow = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                SellerTimeZone);
            Input.StartAt = sellerNow.AddHours(1);
            Input.EndAt = sellerNow.AddDays(7);
            Input.TotalQuantityLimit = 100;
            Input.DiscountValue = 10;
            Input.IsEnabled = true;
            return Page();
        }

        Campaign = await _api.GetAsync<PromotionCampaignDto>(
            $"/api/seller/promotions/{Id.Value}",
            ct);
        if (Campaign == null)
        {
            TempData["ErrorMessage"] = "Promotion campaign not found.";
            return RedirectToPage("./Index");
        }

        Input = new PromotionCampaignInputDto
        {
            Name = Campaign.Name,
            Description = Campaign.Description,
            DiscountType = Campaign.DiscountType,
            DiscountValue = Campaign.DiscountValue,
            TotalQuantityLimit = Campaign.TotalQuantityLimit,
            StartAt = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(Campaign.StartAt, DateTimeKind.Utc),
                SellerTimeZone),
            EndAt = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.SpecifyKind(Campaign.EndAt, DateTimeKind.Utc),
                SellerTimeZone),
            IsEnabled = Campaign.IsEnabled,
            ProductIds = Campaign.Products.Select(x => x.Id).ToList()
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadProductsAsync(ct);
        Input.ProductIds = Input.ProductIds.Distinct().ToList();
        if (!ModelState.IsValid)
            return Page();

        var result = Id.HasValue
            ? await _api.PutAsync($"/api/seller/promotions/{Id.Value}", Input, ct)
            : await _api.PostAsync("/api/seller/promotions", Input, ct);

        if (result.Success)
        {
            TempData["SuccessMessage"] = Id.HasValue
                ? "Promotion campaign updated."
                : "Promotion campaign created.";
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(
            string.Empty,
            result.FirstError ?? "Unable to save promotion campaign.");
        return Page();
    }

    private async Task LoadProductsAsync(CancellationToken ct)
    {
        Products = new List<PromotionProductDto>();
        var pageNumber = 1;
        while (true)
        {
            var result = await _api.GetAsync<PagedResult<PromotionProductDto>>(
                $"/api/seller/promotions/products?pageNumber={pageNumber}&pageSize=100",
                ct);
            if (result == null)
                break;

            Products.AddRange(result.Items);
            if (pageNumber >= result.TotalPages)
                break;

            pageNumber++;
        }
    }

    private static TimeZoneInfo ResolveSellerTimeZone()
    {
        foreach (var id in new[] { "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            "Vietnam",
            TimeSpan.FromHours(7),
            "Vietnam",
            "Vietnam");
    }
}
