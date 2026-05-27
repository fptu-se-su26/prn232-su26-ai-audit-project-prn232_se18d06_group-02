using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class DetailsModel : PageModel
    {
        private readonly ISellerProductService _productService;
        private readonly ISellerStoreService _storeService;

        public DetailsModel(ISellerProductService productService, ISellerStoreService storeService)
        {
            _productService = productService;
            _storeService = storeService;
        }

        public SellerProductDetailDto Product { get; set; } = null!;
        public Store Store { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Public/Auth/Login");

            var store = await _storeService.GetStoreByOwnerIdAsync(userId);
            if (store == null) return RedirectToPage("/StoreOwner/Dashboard");
            Store = store;

            var product = await _productService.GetProductByIdAsync(id, Store.Id);
            if (product == null) return NotFound();

            Product = product;

            return Page();
        }
    }
}
