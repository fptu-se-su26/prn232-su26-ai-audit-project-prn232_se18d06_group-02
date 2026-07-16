using System.Security.Claims;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace GearZone.Web.Pages.StoreOwner.Settings
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        // Consumes GearZone.Api over HTTP instead of the store service in-process.
        private readonly IApiClient _api;
        private readonly IConfiguration _configuration;

        public IndexModel(IApiClient api, IConfiguration configuration)
        {
            _api = api;
            _configuration = configuration;
        }

        public StoreProfileResponse? Store { get; set; }

        public string? GoongApiKey => _configuration["GOONG_API_KEY"];
        public string? GoongMapKey => _configuration["GOONG_MAP_KEY"];

        public async Task<IActionResult> OnGetAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            Store = await _api.GetAsync<StoreProfileResponse>("/api/seller/store", ct);
            if (Store == null)
            {
                return RedirectToPage("/Public/Auth/Login"); // Or handle "no store found" properly
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync([FromForm] UpdateStoreProfileDto dto, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check your input and try again.";
                return RedirectToPage();
            }

            var result = await _api.PutAsync("/api/seller/store", dto, ct);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Store profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update store profile. Note: Only approved stores can update profile here.";
            }

            return RedirectToPage();
        }
    }
}
