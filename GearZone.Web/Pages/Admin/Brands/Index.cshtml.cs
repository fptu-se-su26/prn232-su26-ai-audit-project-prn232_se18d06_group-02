using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Common.Models;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Brands
{
    [Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
    {
        private readonly IApiClient _api;

        public IndexModel(IApiClient api)
        {
            _api = api;
        }

        [BindProperty(SupportsGet = true)]
        public AdminBrandQueryDto Query { get; set; } = new AdminBrandQueryDto();

        [BindProperty]
        public CreateBrandDto CreateInput { get; set; } = new CreateBrandDto();

        [BindProperty]
        public EditBrandDto EditInput { get; set; } = new EditBrandDto();

        public PagedResult<AdminBrandDto> Brands { get; set; } = new PagedResult<AdminBrandDto>();
        public AdminBrandStatsDto BrandStats { get; set; } = new AdminBrandStatsDto();

        public async Task OnGetAsync(CancellationToken ct)
        {
            await LoadDataAsync(ct);
        }

        public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation failed. Please check the inputs.";
                return RedirectToPage();
            }

            using var content = BuildMultipartContent(CreateInput.Name, CreateInput.Slug,
                CreateInput.LogoUrl, CreateInput.IsApproved, CreateInput.LogoFile);
            var result = await _api.PostContentAsync("/api/admin/brands", content, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Brand created successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to create brand. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation failed. Please check the inputs.";
                return RedirectToPage();
            }

            using var content = BuildMultipartContent(EditInput.Name, EditInput.Slug,
                EditInput.LogoUrl, EditInput.IsApproved, EditInput.LogoFile, EditInput.Id);
            var result = await _api.PutContentAsync("/api/admin/brands", content, ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Brand updated successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update brand. Please try again.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id, CancellationToken ct)
        {
            var result = await _api.PostAsync($"/api/admin/brands/{id}/approve", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Brand approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve brand. It might not exist or is already approved.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, CancellationToken ct)
        {
            var result = await _api.PostAsync($"/api/admin/brands/{id}/reject", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Brand rejected successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject brand. It might not exist or is already rejected.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id, CancellationToken ct)
        {
            var result = await _api.DeleteAsync($"/api/admin/brands/{id}", ct);
            if (result.Success)
            {
                TempData["SuccessMessage"] = "Brand deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete brand. It might not exist or is already deleted.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetBrandDetailsAsync(int brandId, CancellationToken ct)
        {
            var brand = await _api.GetAsync<AdminBrandDto>($"/api/admin/brands/{brandId}", ct);
            if (brand == null) return NotFound();

            return new JsonResult(brand);
        }

        private async Task LoadDataAsync(CancellationToken ct)
        {
            if (Query.PageNumber < 1) Query.PageNumber = 1;
            if (Query.PageSize < 1) Query.PageSize = 10;

            var data = await _api.GetAsync<AdminBrandListResponseDto>(
                $"/api/admin/brands{ApiQueryStringBuilder.Build(Query)}", ct);
            if (data is not null)
            {
                Brands = data.Brands;
                BrandStats = data.Stats;
            }
        }

        private static MultipartFormDataContent BuildMultipartContent(
            string name,
            string slug,
            string? logoUrl,
            bool isApproved,
            IFormFile? logoFile,
            int? id = null)
        {
            var content = new MultipartFormDataContent();
            if (id.HasValue) content.Add(new StringContent(id.Value.ToString()), "Id");
            content.Add(new StringContent(name ?? string.Empty), "Name");
            content.Add(new StringContent(slug ?? string.Empty), "Slug");
            content.Add(new StringContent(logoUrl ?? string.Empty), "LogoUrl");
            content.Add(new StringContent(isApproved ? "true" : "false"), "IsApproved");

            if (logoFile is not null && logoFile.Length > 0)
            {
                var fileContent = new StreamContent(logoFile.OpenReadStream());
                if (!string.IsNullOrWhiteSpace(logoFile.ContentType))
                {
                    fileContent.Headers.TryAddWithoutValidation("Content-Type", logoFile.ContentType);
                }
                content.Add(fileContent, "LogoFile", logoFile.FileName);
            }

            return content;
        }
    }
}
