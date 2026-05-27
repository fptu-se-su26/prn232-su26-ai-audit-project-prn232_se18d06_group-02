using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.StoreOwner.Settings
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly ISellerStoreService _storeService;
        private readonly IConfiguration _configuration;

        public Store? Store { get; set; }

        public string? GoongApiKey => _configuration["GOONG_API_KEY"];
        public string? GoongMapKey => _configuration["GOONG_MAP_KEY"];

        public IndexModel(ISellerStoreService storeService, IConfiguration configuration)
        {
            _storeService = storeService;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            Store = await _storeService.GetStoreByOwnerIdAsync(userId);
            if (Store == null)
            {
                return RedirectToPage("/Public/Auth/Login"); // Or handle "no store found" properly
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync([FromForm] GearZone.Application.Features.Seller.Dtos.UpdateStoreProfileDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please check your input and try again.";
                return RedirectToPage();
            }

            var result = await _storeService.UpdateStoreProfileAsync(userId, dto);
            
            if (result)
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
