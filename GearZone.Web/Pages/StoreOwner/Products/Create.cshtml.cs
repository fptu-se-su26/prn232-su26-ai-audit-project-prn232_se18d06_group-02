using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace GearZone.Web.Pages.StoreOwner.Products
{
    [Authorize(Roles = "Store Owner")]
    public class CreateModel : PageModel
    {
        private readonly ISellerProductService _productService;
        private readonly ISellerStoreService _storeService;

        public CreateModel(ISellerProductService productService, ISellerStoreService storeService)
        {
            _productService = productService;
            _storeService = storeService;
        }

        [BindProperty]
        public CreateProductDto Input { get; set; } = new();

        public List<SelectListItem> CategoryOptions { get; set; } = new();
        public List<SelectListItem> BrandOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid? copyFromId = null)
        {
            await LoadMetadataAsync();

            if (copyFromId.HasValue)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var store = await _storeService.GetStoreByOwnerIdAsync(userId!);
                if (store != null)
                {
                    var editDto = await _productService.GetProductForEditAsync(copyFromId.Value, store.Id);
                    if (editDto != null)
                    {
                        Input = new CreateProductDto
                        {
                            Name = editDto.Name + " (Copy)",
                            Description = editDto.Description,
                            CategoryId = editDto.CategoryId,
                            BrandId = editDto.BrandId,
                            BasePrice = editDto.BasePrice,
                            IsDraft = true,
                            Specifications = editDto.Specifications,
                            Variants = editDto.Variants.Select(v => new ProductVariantDto
                            {
                                VariantName = v.VariantName,
                                Sku = "", // Clear SKU for cloning
                                Price = v.Price,
                                StockQuantity = v.StockQuantity,
                                Attributes = v.Attributes
                            }).ToList()
                        };
                        return Page();
                    }
                }
            }

            // Initialize with one default variant.
            if (!Input.Variants.Any())
            {
                Input.Variants.Add(new ProductVariantDto { VariantName = string.Empty, StockQuantity = 0 });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadMetadataAsync();
                return Page();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var store = await _storeService.GetStoreByOwnerIdAsync(userId!);

            if (store == null) return RedirectToPage("/StoreOwner/Dashboard");

            try
            {
                await _productService.CreateProductAsync(Input, store.Id, userId!);
                TempData["SuccessMessage"] = "Product created successfully!";
                return RedirectToPage("./Index");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                await LoadMetadataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An unexpected error occurred: {ex.Message}");
                await LoadMetadataAsync();
                return Page();
            }
        }

        public async Task<JsonResult> OnGetAttributesAsync(int categoryId)
        {
            var attributes = await _productService.GetCategoryAttributesAsync(categoryId);
            return new JsonResult(attributes);
        }

        public async Task<JsonResult> OnGetSpecificationsAsync(int categoryId)
        {
            var specs = await _productService.GetCategoryProductSpecsAsync(categoryId);
            return new JsonResult(specs);
        }
        public async Task<JsonResult> OnPostCreateSpecificationAttributeAsync(int categoryId, string name, string? unit, string? valueType)
        {
            if (categoryId <= 0) return new JsonResult(new { success = false, message = "Category is required" });
            if (string.IsNullOrWhiteSpace(name)) return new JsonResult(new { success = false, message = "Specification name is required" });

            try
            {
                var id = await _productService.CreateCategoryProductSpecificationAsync(categoryId, name, unit, valueType);
                return new JsonResult(new { success = true, id = id, name = name.Trim() });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<JsonResult> OnPostCreateBrandAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return new JsonResult(new { success = false, message = "Name is required" });

            try 
            {
                var id = await _productService.CreateBrandByNameAsync(name);
                return new JsonResult(new { success = true, id = id, name = name });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        public async Task<JsonResult> OnPostCreateCategoryAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return new JsonResult(new { success = false, message = "Name is required" });

            try
            {
                var id = await _productService.CreateCategoryByNameAsync(name);
                return new JsonResult(new { success = true, id = id, name = name });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        private async Task LoadMetadataAsync()
        {
            var allCategories = await _productService.GetCategoriesAsync();

            CategoryOptions = allCategories
                .Where(c => c.ParentId != null || !allCategories.Any(child => child.ParentId == c.Id))
                .Select(c => {
                    var parent = c.ParentId.HasValue ? allCategories.FirstOrDefault(pc => pc.Id == c.ParentId.Value) : null;
                    var text = parent != null ? $"{parent.Name} > {c.Name}" : c.Name;
                    return new SelectListItem { Value = c.Id.ToString(), Text = text };
                })
                .OrderBy(s => s.Text)
                .ToList();

            var brands = await _productService.GetBrandsAsync();
            BrandOptions = brands.Select(b => new SelectListItem 
            { 
                Value = b.Id.ToString(), 
                Text = b.Name 
            }).ToList();
        }
    }
}


