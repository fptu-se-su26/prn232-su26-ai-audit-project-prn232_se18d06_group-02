using System.Security.Claims;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class DetailsModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the product service in-process.
        // The API scopes the product to the caller's store, so no store lookup is needed here.
        private readonly IApiClient _api;

        public DetailsModel(IApiClient api)
        {
            _api = api;
        }

        public SellerProductDetailDto Product { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Public/Auth/Login");

            var product = await _api.GetAsync<SellerProductDetailDto>($"/api/seller/products/{id}/details", ct);
            if (product == null) return NotFound();

            Product = product;
            return Page();
        }
    }
}
